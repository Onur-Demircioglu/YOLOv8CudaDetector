using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using OpenCvSharp;

public class YoloDetector : IDisposable
{
    private readonly InferenceSession _session;
    private readonly string[] _labels;
    private const int TargetSize = 640;

    public YoloDetector(string modelPath, string? labelsPath = null)
    {
        // CUDA Hızlandırması için Ayarlar
        var options = new SessionOptions();
        try
        {
            options.AppendExecutionProvider_CUDA(0); // 0. GPU'yu kullan
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Uyarı: CUDA başlatılamadı, CPU kullanılıyor. ({ex.Message})");
        }

        _session = new InferenceSession(modelPath, options);
        
        // Debug: Model Metadata
        foreach (var input in _session.InputMetadata)
        {
            Console.WriteLine($"Model Input: {input.Key} [{string.Join(",", input.Value.Dimensions)}]");
        }
        foreach (var output in _session.OutputMetadata)
        {
            Console.WriteLine($"Model Output: {output.Key} [{string.Join(",", output.Value.Dimensions)}]");
        }

        // LVIS veya Özel Label Yükleme
        if (!string.IsNullOrEmpty(labelsPath) && File.Exists(labelsPath))
        {
            _labels = File.ReadAllLines(labelsPath);
            Console.WriteLine($"Etiketler dosyadan yüklendi: {labelsPath} ({_labels.Length} sınıf)");
        }
        else
        {
            // COCO Labels (YOLOv8 default fallback)
            _labels = new string[] 
            { 
                "person", "bicycle", "car", "motorcycle", "airplane", "bus", "train", "truck", "boat", "traffic light",
                "fire hydrant", "stop sign", "parking meter", "bench", "bird", "cat", "dog", "horse", "sheep", "cow",
                "elephant", "bear", "zebra", "giraffe", "backpack", "umbrella", "handbag", "tie", "suitcase", "frisbee",
                "skis", "snowboard", "sports ball", "kite", "baseball bat", "baseball glove", "skateboard", "surfboard",
                "tennis racket", "bottle", "wine glass", "cup", "fork", "knife", "spoon", "bowl", "banana", "apple",
                "sandwich", "orange", "broccoli", "carrot", "hot dog", "pizza", "donut", "cake", "chair", "couch",
                "potted plant", "bed", "dining table", "toilet", "tv", "laptop", "mouse", "remote", "keyboard",
                "cell phone", "microwave", "oven", "toaster", "sink", "refrigerator", "book", "clock", "vase",
                "scissors", "teddy bear", "hair drier", "toothbrush"
            };
        }
    }

    public List<Prediction> Detect(Mat image, float confThreshold = 0.25f)
    {
        // 1. Preprocess
        var inputTensor = Preprocess(image, out float ratio, out int dw, out int dh);

        // 2. Inference
        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor("images", inputTensor)
        };

        using var results = _session.Run(inputs);
        var output = results.First().AsTensor<float>();

        // Debug output stats
        // Console.WriteLine($"Output Shape: {string.Join("x", output.Dimensions)}");
        
        // 3. Postprocess
        return Postprocess(output, ratio, dw, dh, confThreshold);
    }

    private DenseTensor<float> Preprocess(Mat image, out float ratio, out int dw, out int dh)
    {
        // Letterbox resize
        float ratioW = (float)TargetSize / image.Width;
        float ratioH = (float)TargetSize / image.Height;
        ratio = Math.Min(ratioW, ratioH);

        int newW = (int)(image.Width * ratio);
        int newH = (int)(image.Height * ratio);

        dw = (TargetSize - newW) / 2;
        dh = (TargetSize - newH) / 2;

        using Mat resized = new Mat();
        Cv2.Resize(image, resized, new Size(newW, newH));
        
        using Mat padded = new Mat(new Size(TargetSize, TargetSize), MatType.CV_8UC3, new Scalar(114, 114, 114));
        Cv2.CopyMakeBorder(resized, padded, dh, TargetSize - newH - dh, dw, TargetSize - newW - dw, BorderTypes.Constant, new Scalar(114, 114, 114));

        // Normalize & CHW
        var tensor = new DenseTensor<float>(new[] { 1, 3, TargetSize, TargetSize });
        
        // OpenCvSharp Mat access is a bit tricky for high perf, using simpler loop for clarity here
        // For production, use unsafe pointers or specific bulk copy methods
        
        // Convert to RGB
        Cv2.CvtColor(padded, padded, ColorConversionCodes.BGR2RGB);
        
        // Normalize 0-255 -> 0.0-1.0
        padded.ConvertTo(padded, MatType.CV_32FC3, 1.0 / 255.0);

        var indexer = padded.GetGenericIndexer<Vec3f>();
        for (int y = 0; y < TargetSize; y++)
        {
            for (int x = 0; x < TargetSize; x++)
            {
                var pixel = indexer[y, x];
                tensor[0, 0, y, x] = pixel.Item0; // R
                tensor[0, 1, y, x] = pixel.Item1; // G
                tensor[0, 2, y, x] = pixel.Item2; // B
            }
        }

        return tensor;
    }

    private List<Prediction> Postprocess(Tensor<float> output, float ratio, int dw, int dh, float confThreshold)
    {
        // Output shape: [1, 84, 8400] -> [Batch, Classes+Box, Predictions]
        
        var predictions = new List<Prediction>();
        int dimensions = output.Dimensions[1]; // 84
        int rows = output.Dimensions[2];       // 8400

        float globalMaxScore = 0f;
        float g_cx = 0, g_cy = 0, g_w = 0, g_h = 0; // Debug için

        for (int i = 0; i < rows; i++)
        {
            // Find max confidence
            float maxScore = 0;
            int maxClassId = -1;

            // Classes start at index 4
            for (int c = 4; c < dimensions; c++)
            {
                float score = output[0, c, i];
                if (score > maxScore)
                {
                    maxScore = score;
                    maxClassId = c - 4;
                }
            }

            if (maxScore > globalMaxScore)
            {
                globalMaxScore = maxScore;
                // Debug için en yüksek skorlu kutunun ham verilerini sakla
                g_cx = output[0, 0, i];
                g_cy = output[0, 1, i];
                g_w = output[0, 2, i];
                g_h = output[0, 3, i];
            }

            if (maxScore < confThreshold) continue;

            // Get Box (cx, cy, w, h)
            float cx = output[0, 0, i];
            float cy = output[0, 1, i];
            float w = output[0, 2, i];
            float h = output[0, 3, i];

            // Düzeltme: Eğer model normalize edilmiş çıktı veriyorsa (0-1 arası), piksele çevir
            // Genelde pixel değerleri 1'den çok daha büyüktür (örn. 100, 200).
            // Eğer w ve h 1.0 civarındaysa normalize edilmiş demektir.
            if (w <= 1.0f && h <= 1.0f)
            {
                cx *= TargetSize;
                cy *= TargetSize;
                w *= TargetSize;
                h *= TargetSize;
            }

            // Unpad and Unscale
            float x = (cx - w / 2 - dw) / ratio;
            float y = (cy - h / 2 - dh) / ratio;
            float width = w / ratio;
            float height = h / ratio;

            predictions.Add(new Prediction
            {
                Label = _labels[maxClassId],
                Confidence = maxScore,
                Box = new Rect((int)x, (int)y, (int)width, (int)height)
            });
        }
        
        // Debug
        // Console.WriteLine($"MaxConf: {globalMaxScore:F2} | RawBox: [{g_cx:F1}, {g_cy:F1}, {g_w:F1}, {g_h:F1}] | Count: {predictions.Count}");

        return NonMaxSuppression(predictions, 0.45f);
    }

    private List<Prediction> NonMaxSuppression(List<Prediction> detections, float iouThreshold)
    {
        var result = new List<Prediction>();
        var sorted = detections.OrderByDescending(d => d.Confidence).ToList();

        while (sorted.Count > 0)
        {
            var best = sorted[0];
            result.Add(best);
            sorted.RemoveAt(0);

            for (int i = sorted.Count - 1; i >= 0; i--)
            {
                if (ComputeIoU(best.Box, sorted[i].Box) > iouThreshold)
                {
                    sorted.RemoveAt(i);
                }
            }
        }
        return result;
    }

    private float ComputeIoU(Rect boxA, Rect boxB)
    {
        int xA = Math.Max(boxA.Left, boxB.Left);
        int yA = Math.Max(boxA.Top, boxB.Top);
        int xB = Math.Min(boxA.Right, boxB.Right);
        int yB = Math.Min(boxA.Bottom, boxB.Bottom);

        int interArea = Math.Max(0, xB - xA) * Math.Max(0, yB - yA);
        float boxAArea = boxA.Width * boxA.Height;
        float boxBArea = boxB.Width * boxB.Height;

        return interArea / (boxAArea + boxBArea - interArea);
    }

    public void Dispose()
    {
        _session?.Dispose();
    }
}

public class Prediction
{
    public string Label { get; set; } = "";
    public float Confidence { get; set; }
    public Rect Box { get; set; }
}
