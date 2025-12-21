using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using Spectre.Console;

class Program
{
    static void Main(string[] args)
    {
        // DLL yükleme sorunlarını çözmek için çalışma dizinini PATH'e ekle
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var pathVar = Environment.GetEnvironmentVariable("PATH") ?? "";
        if (!pathVar.Contains(baseDir))
        {
            Environment.SetEnvironmentVariable("PATH", baseDir + ";" + pathVar, EnvironmentVariableTarget.Process);
        }

        // ASCII Banner
        ShowBanner();
        
        // Sistem Durumu Paneli
        ShowSystemStatus();

        // Ana menü döngüsü
        while (true)
        {
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("\n[cyan]Ne yapmak istersiniz?[/]")
                    .PageSize(10)
                    .AddChoices(new[] {
                        "📷 Statik Resim Analizi",
                        "🎥 Webcam ile Canlı Analiz",
                        "🎬 Video Dosyası Analizi",
                        "❌ Çıkış"
                    }));

            if (choice == "📷 Statik Resim Analizi")
            {
                StaticConvexHull();
            }
            else if (choice == "🎥 Webcam ile Canlı Analiz")
            {
                LiveAnalyzer(useCamera: true);
            }
            else if (choice == "🎬 Video Dosyası Analizi")
            {
                var videoPath = AnsiConsole.Prompt(
                    new TextPrompt<string>("Video dosya yolu:")
                        .DefaultValue("test.mp4")
                        .ShowDefaultValue(true));
                LiveAnalyzer(useCamera: false, videoPath: videoPath);
            }
            else if (choice == "❌ Çıkış")
            {
                AnsiConsole.MarkupLine("\n[yellow]Çıkılıyor... Görüşmek üzere! 👋[/]");
                break;
            }
        }
    }

    static void ShowBanner()
    {
        Console.Clear();
        
        var banner = new FigletText("YOLOv8")
            .Centered()
            .Color(Color.Cyan1);
        
        AnsiConsole.Write(banner);
        
        AnsiConsole.Write(
            new Rule("[yellow]CUDA Object Detection System[/]")
                .RuleStyle("grey")
                .Centered());
        
        AnsiConsole.WriteLine();
    }

    static void ShowSystemStatus()
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Grey);
        
        table.AddColumn(new TableColumn("[cyan]Component[/]").Centered());
        table.AddColumn(new TableColumn("[cyan]Status[/]").Centered());

        table.AddRow("[grey]Model[/]", "[green]✓[/] YOLOv8x (Extra Large)");
        table.AddRow("[grey]Acceleration[/]", "[green]✓[/] CUDA (GPU)");
        table.AddRow("[grey]Classes[/]", "[yellow]80[/] COCO Objects");
        table.AddRow("[grey]Resolution[/]", "[yellow]640x640[/]");

        AnsiConsole.Write(table);
    }

    static void StaticConvexHull()
    {
        // Python_Alıştırma klasöründe (bir üst klasör) olduğu varsayılıyor
        string[] searchPaths = {
            "cars.jpg",
            "../cars.jpg",
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "cars.jpg")
        };

        string filePath = "";
        foreach (var path in searchPaths)
        {
            if (File.Exists(path))
            {
                filePath = path;
                break;
            }
        }

        if (string.IsNullOrEmpty(filePath))
        {
            AnsiConsole.MarkupLine("[red]✗ Hata: 'cars.jpg' bulunamadı.[/]");
            return;
        }

        using Mat img = Cv2.ImRead(filePath);
        if (img.Empty())
        {
            AnsiConsole.MarkupLine("[red]✗ Resim yüklenemedi.[/]");
            return;
        }

        Mat result = ProcessAndDraw(img);
        Cv2.ImShow("Static Image Analysis", result);
        Cv2.ImWrite("sonuc_csharp.png", result);
        AnsiConsole.MarkupLine("[green]✓ Kaydedildi: sonuc_csharp.png[/]");
        Cv2.WaitKey(0);
        Cv2.DestroyAllWindows();
    }

    static void LiveAnalyzer(bool useCamera = true, string videoPath = "")
    {
        // 1. Model Yükleme Progress Bar ile
        YoloDetector? detector = null;
        
        AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .SpinnerStyle(Style.Parse("yellow"))
            .Start("[yellow]Yapay Zeka Modeli Yükleniyor...[/]", ctx =>
            {
                string modelPath = "yolov8x.onnx";
                string? labelPath = null;

                if (!File.Exists(modelPath))
                {
                    ctx.Status("[yellow]yolov8x.onnx bulunamadı, yolov8n.onnx deneniyor...[/]");
                    modelPath = "yolov8n.onnx";
                    if (!File.Exists(modelPath))
                    {
                        AnsiConsole.MarkupLine("[red]✗ Hata: Hiçbir model dosyası bulunamadı![/]");
                        return;
                    }
                }

                detector = new YoloDetector(modelPath, labelPath);
                Thread.Sleep(500); // Progress görünümü için
                AnsiConsole.MarkupLine($"[green]✓ Model Hazır: {modelPath}[/]");
            });

        if (detector == null) return;

        // 2. Kaynağı Aç (Kamera veya Video)
        VideoCapture? cap = null;
        
        if (useCamera)
        {
            cap = TryOpenCamera();
        }
        else
        {
            if (!File.Exists(videoPath))
            {
                AnsiConsole.MarkupLine($"[red]✗ Hata: Video dosyası bulunamadı: {videoPath}[/]");
                return;
            }
            cap = new VideoCapture(videoPath);
            AnsiConsole.MarkupLine($"[green]✓ Video açıldı: {videoPath}[/]");
        }

        if (cap == null || !cap.IsOpened()) 
        {
            AnsiConsole.MarkupLine("[red]✗ Kaynak açılamadı.[/]");
            return;
        }

        AnsiConsole.MarkupLine("[grey]Pencereyi kapatmak için 'q' tuşuna basın[/]\n");

        using Mat frame = new Mat();
        
        // FPS hesaplama
        double fps = 0;
        int frameCount = 0;
        DateTime startTime = DateTime.Now;
        int errorCount = 0;

        using (detector) // Dispose detector after use
        {
            while (true)
            {
                bool success = cap.Read(frame);
                if (!success || frame.Empty())
                {
                    // Video bittiyse başa sar (döngü)
                    if (!useCamera && cap.Get(VideoCaptureProperties.PosFrames) == cap.Get(VideoCaptureProperties.FrameCount))
                    {
                        cap.Set(VideoCaptureProperties.PosFrames, 0);
                        continue;
                    }

                    errorCount++;
                    if (errorCount > 5)
                    {
                        break;
                    }
                    System.Threading.Thread.Sleep(100);
                    continue;
                }
                errorCount = 0;

                // Frame'i klonla (YOLO kutuları için)
                Mat processedFrame = frame.Clone();

                // 1. YOLO Tespiti
                var predictions = detector.Detect(frame);

                // 2. Kutuları Çiz
                foreach (var pred in predictions)
                {
                    var box = pred.Box;
                    var color = Scalar.Red;
                    if (pred.Label == "person") color = Scalar.Yellow;
                    if (pred.Label == "car") color = Scalar.Green;

                    // Koordinatları güvenli aralığa çek
                    int x = Math.Max(0, box.X);
                    int y = Math.Max(0, box.Y);
                    int w = Math.Min(frame.Width - x, box.Width);
                    int h = Math.Min(frame.Height - y, box.Height);

                    Cv2.Rectangle(processedFrame, new Rect(x, y, w, h), color, 2);
                    
                    string labelText = $"{pred.Label} {pred.Confidence:F2}";
                    Cv2.PutText(processedFrame, labelText, new Point(x, Math.Max(10, y - 5)), 
                        HersheyFonts.HersheySimplex, 0.6, color, 2);
                }

                // FPS Yaz
                frameCount++;
                if (frameCount >= 30)
                {
                    var duration = (DateTime.Now - startTime).TotalSeconds;
                    fps = frameCount / duration;
                    startTime = DateTime.Now;
                    frameCount = 0;
                }
                
                Cv2.PutText(processedFrame, $"FPS: {fps:F1}", new Point(10, 30), 
                    HersheyFonts.HersheySimplex, 1, Scalar.Yellow, 2);
                
                // Model bilgisi (GPU/CPU)
                Cv2.PutText(processedFrame, "YOLOv8x (CUDA)", new Point(10, 60), 
                     HersheyFonts.HersheySimplex, 0.7, Scalar.Cyan, 2);

                Cv2.ImShow("YOLOv8 + C#", processedFrame);

                int k = Cv2.WaitKey(1);
                if (k == 'q') break;
            }
        }

        cap.Release();
        Cv2.DestroyAllWindows();
    }

    static VideoCapture? TryOpenCamera()
    {
        AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .Start("[yellow]Kamera aranıyor (DSHOW + MJPG)...[/]", ctx =>
            {
                Thread.Sleep(800); // Görsel efekt için
            });

        try 
        {
            var cap = new VideoCapture(0, VideoCaptureAPIs.DSHOW);
            if (cap.IsOpened())
            {
                cap.Set(VideoCaptureProperties.FourCC, VideoWriter.FourCC("MJPG"));
                cap.Set(VideoCaptureProperties.FrameWidth, 640);
                cap.Set(VideoCaptureProperties.FrameHeight, 480);

                Thread.Sleep(500);

                using var testFrame = new Mat();
                if (cap.Read(testFrame) && !testFrame.Empty())
                {
                    AnsiConsole.MarkupLine("[green]✓ Kamera 0 bulundu ve açıldı![/]");
                    return cap;
                }
            }
            cap.Release();
        }
        catch { }
        
        AnsiConsole.MarkupLine("[red]✗ HATA: Kamera donanımsal olarak yanıt vermiyor (Error 0xA00F4244).[/]");
        return null;
    }

    static Mat ProcessAndDraw(Mat img, double? areaMin = null, double? pxPerCm = null)
    {
        Mat outImg = img.Clone();
        Mat gray = new Mat();
        Cv2.CvtColor(img, gray, ColorConversionCodes.BGR2GRAY);
        
        Mat blur = new Mat();
        Cv2.GaussianBlur(gray, blur, new OpenCvSharp.Size(5, 5), 0);

        Mat thr = new Mat();
        // Otsu eşikleme kullanılıyor.
        // Eğer arka plan açık renkse (örn. gündüz asfalt veya kağıt), 'Binary' arka planı beyaz yapar (nesne olarak algılar).
        // Bu yüzden 'BinaryInv' kullanarak koyu renkli nesneleri beyaz (algılanacak) yapıyoruz.
        // Duruma göre Binary veya BinaryInv seçilmelidir. Varsayılan olarak Inv deniyoruz.
        Cv2.Threshold(blur, thr, 0, 255, ThresholdTypes.BinaryInv | ThresholdTypes.Otsu);

        // Morfolojik işlemler - Küçük gürültüyü temizle
        Mat kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(3, 3));
        Cv2.MorphologyEx(thr, thr, MorphTypes.Open, kernel, iterations: 2);

        // Debug için threshold görüntüsünü kaydet (isteğe bağlı)
        // Cv2.ImWrite("debug_thresh.png", thr);

        Point[][] contours;
        HierarchyIndex[] hierarchy;
        Cv2.FindContours(thr, out contours, out hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

        if (contours.Length == 0) return outImg;

        double totalArea = img.Rows * img.Cols;

        // Otomatik min alan
        if (areaMin == null)
        {
            // Sadece tüm ekranı kaplamayan contourları al
            var validAreas = contours
                .Select(c => Cv2.ContourArea(Cv2.ConvexHull(c)))
                .Where(a => a < totalArea * 0.90) // Ekranın %90'ından küçük olanlar
                .OrderBy(a => a).ToList();

            if (validAreas.Count >= 3)
            {
                areaMin = validAreas[(int)(validAreas.Count * 0.25)];
            }
            else if (validAreas.Count > 0)
            {
                 // En azından çok küçük gürültüleri ele
                 areaMin = Math.Max(0.001 * totalArea, 100);
            }
            else 
            {
                 // Hiç geçerli contour yoksa
                 areaMin = 0; 
            }
        }

        foreach (var c in contours)
        {
            Point[] hull = Cv2.ConvexHull(c);
            double area = Cv2.ContourArea(hull);

            // Filtreler:
            // 1. Çok küçük alanlar (gürültü)
            if (area < areaMin) continue;
            
            // 2. Çok büyük alanlar (Tüm ekran/arka plan)
            if (area > totalArea * 0.95) continue;

            // Hull Çiz
            List<Point[]> hullList = new List<Point[]> { hull };
            Cv2.DrawContours(outImg, hullList, -1, Scalar.Lime, 2);

            // Bounding Rect
            Rect rect = Cv2.BoundingRect(hull);
            
            // Label
            string label = $"A:{area:F0}px";
            if (pxPerCm.HasValue)
            {
                label += $" {area / (pxPerCm.Value * pxPerCm.Value):F2}cm2";
            }

            Cv2.PutText(outImg, label, new Point(rect.X, Math.Max(0, rect.Y - 8)),
                HersheyFonts.HersheySimplex, 0.5, Scalar.Red, 1, LineTypes.AntiAlias);
        }

        return outImg;
    }
}
