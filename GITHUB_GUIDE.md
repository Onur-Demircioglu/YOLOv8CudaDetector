# GitHub Push Rehberi

## Adım 1: Git Başlat

```bash
cd C:\Users\onur_\Desktop\Python_Alıştırma\YOLOv8-CUDA-Detector
git init
```

## Adım 2: Dosyaları Ekle

```bash
git add .
```

## Adım 3: İlk Commit

```bash
git commit -m "Initial commit: YOLOv8x CUDA Object Detection with Spectre.Console UI"
```

## Adım 4: GitHub'da Repo Oluştur

1. GitHub.com'a git
2. "New repository" tıkla
3. **Repository name**: `YOLOv8-CUDA-Detector`
4. **Description**: "Real-time object detection with YOLOv8x, CUDA acceleration, and modern CLI (C# + Spectre.Console)"
5. **Public** seç (portfolio için)
6. "Create repository" tıkla

## Adım 5: Remote Ekle ve Push

GitHub'da gösterilen komutları kullan (kendi kullanıcı adınla değiştir):

```bash
git remote add origin https://github.com/<kullanıcı-adın>/YOLOv8-CUDA-Detector.git
git branch -M main
git push -u origin main
```

## Adım 6: Model Dosyası İçin Not Ekle

README.md zaten model indirme linkini içeriyor. Alternatif olarak GitHub Releases kullanabilirsin:

1. GitHub repo sayfasında "Releases" > "Create a new release"
2. Tag: `v1.0`
3. Title: "Initial Release - YOLOv8x CUDA Detector"
4. Description:
   ```
   ## Download Required Model
   
   Download `yolov8x.onnx` from:
   https://github.com/ultralytics/assets/releases/download/v8.1.0/yolov8x.onnx
   
   Place it in the project root directory before running.
   ```
5. "Publish release"

## ✅ Push Sonrası Kontrol

Repo sayfanda şunlar olmalı:
- ✅ README.md güzel görünüyor
- ✅ .gitignore çalışıyor (bin/, obj/, *.onnx yok)
- ✅ Kod dosyaları düzgün
- ✅ LINKEDIN_POST.md klavuz olarak var

---

# CV'ye Ekleme Önerileri

## Proje Başlığı

**YOLOv8x Real-Time Object Detection System**

## Açıklama (Türkçe)

C# ve CUDA kullanarak gerçek zamanlı nesne tespiti yapan, modern interaktif CLI arayüzüne sahip masaüstü uygulaması. YOLOv8x modeli ile 80 farklı nesne sınıfını GPU hızlandırması sayesinde 5-8 FPS hızında tespit eder.

**Teknolojiler**: C#, .NET 8, CUDA, ONNX Runtime, OpenCvSharp, Spectre.Console

**Kazanımlar**:
- GPU programlama ve CUDA optimizasyonu
- Gerçek zamanlı görüntü işleme
- AI model entegrasyonu (ONNX)
- Modern CLI/UX tasarımı

## İngilizce Versiyon

Desktop application for real-time object detection using C# and CUDA, featuring a modern interactive CLI. Detects 80 object classes using YOLOv8x model at 5-8 FPS with GPU acceleration.

**Technologies**: C#, .NET 8, CUDA, ONNX Runtime, OpenCvSharp, Spectre.Console

**Key Achievements**:
- GPU programming and CUDA optimization
- Real-time image processing
- AI model integration (ONNX)
- Modern CLI/UX design

---

# LinkedIn Paylaşım Takvimi

1. **İlk Paylaşım**: Proje duyurusu (`LINKEDIN_POST.md` şablonunu kullan)
2. **Ekran Görüntüleri**: 
   - Banner + Tablo
   - İnteraktif menü
   - Canlı tespit videosu
3. **Hashtagler**: #CSharp #MachineLearning #CUDA #ComputerVision #AI
4. **Etiketle**: İlgili gruplar (C# Developers, AI/ML vb.)

## Paylaşım Zamanlaması

- En iyi zaman: Hafta içi 09:00-11:00 veya 17:00-19:00
- Görsellerle engagement %300 artar
- Koddan kısa snippet ekle

---

**🎉 TEBRİKLER! Projen portfolio-ready!**
