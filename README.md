# YOLOv8x Object Detection with CUDA (C#)

[🇹🇷 Türkçe](#türkçe) | [🇬🇧 English](#english)

---

## Türkçe

Modern, interaktif CLI arayüzüne sahip gerçek zamanlı nesne algılama sistemi. CUDA GPU hızlandırması ile YOLOv8x modelini kullanarak 80 farklı nesne sınıfını tanır.

### ✨ Özellikler

- 🎯 **YOLOv8x Nesne Algılama** - 80 COCO sınıfı ile Extra Large model
- 🚀 **CUDA GPU Hızlandırması** - NVIDIA GPU ile yüksek hızlı işleme
- 🎨 **Modern CLI Arayüzü** - Spectre.Console ile renkli, interaktif deneyim
- 📊 **Gerçek Zamanlı Metriks** - FPS göstergesi ve sistem durumu
- 📹 **Çoklu Giriş Modu**:
  - Statik resim analizi
  - Canlı webcam algılama
  - Video dosyası işleme

### 🎭 UI Özellikleri

- **ASCII Banner** - Göz alıcı YOLOv8 logosu
- **İnteraktif Menü** - Ok tuşları ile gezinme (↑↓) + Enter
- **Progress Spinners** - Model yükleme animasyonu
- **Renkli Mesajlar** - ✓ Başarı (Yeşil), ✗ Hata (Kırmızı)
- **Sistem Tablosu** - Model, GPU, Çözünürlük bilgileri

### 🛠️ Gereksinimler

- .NET 8.0 SDK
- CUDA desteği olan NVIDIA GPU
- CUDA Toolkit 11.8+
- CuDNN 8.9.7

### 📦 Kurulum

1. **Projeyi klonla**
   ```bash
   git clone <repo-url>
   cd YOLOv8-CUDA-Detector
   ```

2. **YOLOv8x modelini indir**
   
   [yolov8x.onnx](https://github.com/ultralytics/assets/releases/download/v8.1.0/yolov8x.onnx) dosyasını indirip proje klasörüne koy.

3. **CUDA bağımlılıklarını kur**
   
   - [CUDA Toolkit 11.8](https://developer.nvidia.com/cuda-11-8-0-download-archive) kur
   - [CuDNN 8.9.7](https://developer.nvidia.com/rdp/cudnn-archive) indir (CUDA 11.x için)
   - CuDNN zip'inden `bin/` klasöründeki **TÜM .dll dosyalarını** `bin/Debug/net8.0/` klasörüne kopyala

4. **Bağımlılıkları yükle**
   ```bash
   dotnet restore
   ```

### 🚀 Kullanım

```bash
dotnet run
```

**İnteraktif Menü:**
- Ok tuşları (↑↓) ile seçenekler arasında gezin
- Enter ile seçimi onayla
- Video penceresini kapatmak için `Q` tuşuna bas

### 📊 Performans

- **Model**: YOLOv8x (273MB)
- **Hedef Çözünürlük**: 640x640
- **FPS**: 5-8 FPS (GPU ve içeriğe bağlı)
- **Tespit Sınıfları**: 80 COCO nesnesi (insan, araba, köpek, laptop, vs.)

### 🎨 Tespit Edilen Nesneler

Sistem şu nesneleri tanır ve renkli kutularla gösterir:
- 👤 **İnsan** (Sarı)
- 🚗 **Araba** (Yeşil)
- 🐶 **Köpek, Kedi** (Kırmızı)
- 💻 **Laptop, Telefon** (Kırmızı)
- Ve 70+ nesne daha!

### 🐛 Sorun Giderme

#### CUDA Hatası 126
- **Sebep**: CuDNN DLL'leri eksik
- **Çözüm**: CuDNN zip dosyasından `bin/` klasöründeki **TÜM .dll dosyalarını** `bin/Debug/net8.0/` klasörüne kopyalayın

#### Webcam Hatası 0xA00F4244
- **Sebep**: Windows kamera sürücü çakışması
- **Çözüm**: 
  - Bilgisayarı yeniden başlat
  - Windows Ayarları > Gizlilik > Kamera'yı kontrol et
  - Video Modu'nu kullan (Seçenek 3)

---

## English

Modern, interactive CLI-based real-time object detection system using YOLOv8x with GPU acceleration (CUDA) in C#. Detects 80 different object classes.

### ✨ Features

- 🎯 **YOLOv8x Object Detection** - Extra Large model with 80 COCO classes
- 🚀 **CUDA GPU Acceleration** - Powered by NVIDIA GPU for high-speed inference
- 🎨 **Modern CLI Interface** - Beautiful, interactive experience with Spectre.Console
- 📊 **Real-time Metrics** - FPS display and system status
- 📹 **Multiple Input Modes**:
  - Static image analysis
  - Live webcam detection
  - Video file processing

### 🎭 UI Features

- **ASCII Banner** - Eye-catching YOLOv8 logo
- **Interactive Menu** - Arrow key navigation (↑↓) + Enter
- **Progress Spinners** - Model loading animation
- **Color-Coded Messages** - ✓ Success (Green), ✗ Error (Red)
- **System Table** - Model, GPU, Resolution info

### 🛠️ Requirements

- .NET 8.0 SDK
- NVIDIA GPU with CUDA support
- CUDA Toolkit 11.8+
- CuDNN 8.9.7

### 📦 Installation

1. **Clone the repository**
   ```bash
   git clone <repo-url>
   cd YOLOv8-CUDA-Detector
   ```

2. **Download YOLOv8x model**
   
   Download [yolov8x.onnx](https://github.com/ultralytics/assets/releases/download/v8.1.0/yolov8x.onnx) and place it in the project root.

3. **Install CUDA dependencies**
   
   - Install [CUDA Toolkit 11.8](https://developer.nvidia.com/cuda-11-8-0-download-archive)
   - Download [CuDNN 8.9.7](https://developer.nvidia.com/rdp/cudnn-archive) (for CUDA 11.x)
   - Copy **ALL .dll files** from CuDNN `bin/` folder to `bin/Debug/net8.0/`

4. **Restore dependencies**
   ```bash
   dotnet restore
   ```

### 🚀 Usage

```bash
dotnet run
```

**Interactive Menu:**
- Navigate options with arrow keys (↑↓)
- Confirm selection with Enter
- Press `Q` to close video window

### 📊 Performance

- **Model**: YOLOv8x (273MB)
- **Target Resolution**: 640x640
- **FPS**: 5-8 FPS (depends on GPU and video content)
- **Detection Classes**: 80 COCO objects

### 🎨 Detected Objects

System recognizes and displays with color-coded boxes:
- 👤 **Person** (Yellow)
- 🚗 **Car** (Green)
- 🐶 **Dog, Cat** (Red)
- 💻 **Laptop, Phone** (Red)
- And 70+ more objects!

### 🐛 Troubleshooting

#### CUDA Error 126
- **Cause**: Missing CuDNN DLLs
- **Fix**: Copy **ALL .dll files** from CuDNN download's `bin/` folder to `bin/Debug/net8.0/`

#### Webcam Error 0xA00F4244
- **Cause**: Windows camera driver conflict
- **Fix**: 
  - Restart computer
  - Check Windows Settings > Privacy > Camera
  - Use Video Mode instead (Option 3)

---

## 🛠️ Tech Stack

- **Language**: C# (.NET 8.0)
- **ML Framework**: ONNX Runtime (GPU)
- **Computer Vision**: OpenCvSharp
- **CLI Framework**: Spectre.Console
- **AI Model**: YOLOv8x (Ultralytics)

## 📝 License

MIT License - Feel free to use and modify!

## 🙏 Credits

- [Ultralytics YOLOv8](https://github.com/ultralytics/ultralytics)
- [OpenCvSharp](https://github.com/shimat/opencvsharp)
- [ONNX Runtime](https://github.com/microsoft/onnxruntime)
- [Spectre.Console](https://github.com/spectreconsole/spectre.console)

---

**⭐ Star this repo if you found it useful!**
