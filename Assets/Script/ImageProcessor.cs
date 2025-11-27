using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // Tetap diperlukan untuk PointerEventData
using System.IO;
using TMPro; 

#if UNITY_EDITOR
using UnityEditor; 
#endif

public class ImageProcessor : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button btnPilihGambar;
    [SerializeField] private RawImage imgAsli; 
    [SerializeField] private RawImage imgGrayscale;
    [SerializeField] private RawImage imgHistogram;
    [SerializeField] private TextMeshProUGUI txtInfo;
    [SerializeField] private TextMeshProUGUI txtMaxPixelCount; // Untuk label Y-max histogram

    private Texture2D textureAsli; // Menyimpan data gambar di memori

    void Start()
    {
        btnPilihGambar.onClick.AddListener(OnBtnPilihGambarClick);
        txtInfo.text = "Klik Gambar"; // Pesan awal di UI
    }

    // --- 1. BAGIAN MEMILIH GAMBAR ---
    private void OnBtnPilihGambarClick()
    {
        #if UNITY_EDITOR
            // Jika dijalankan di Unity Editor (PC)
            string path = EditorUtility.OpenFilePanel("Pilih Gambar", "", "png,jpg,jpeg");
            if (!string.IsNullOrEmpty(path))
            {
                MuatGambarDariPath(path);
            }
        #elif UNITY_ANDROID || UNITY_IOS // ⭐ KODE INI YANG DIMODIFIKASI ⭐
            // Jika dijalankan di Android atau iOS: Gunakan NativeGallery
            NativeGallery.GetImageFromGallery((path) =>
            {
                if (path != null)
                {
                    Debug.Log("Gambar berhasil dipilih dari galeri: " + path);
                    MuatGambarDariPath(path);
                }
                else
                {
                    Debug.LogWarning("Pemilihan gambar dibatalkan atau gagal.");
                    txtInfo.text = "Pemilihan gambar dibatalkan."; // Perbarui UI
                }
            }, "Pilih Gambar", "image/*"); // Judul dialog dan tipe media
        #else
            // Jika platform selain Editor, Android, atau iOS
            Debug.Log("Platform tidak didukung untuk pemilihan gambar.");
            txtInfo.text = "Platform tidak didukung.";
        #endif
    }

    private void MuatGambarDariPath(string path)
    {
        if (File.Exists(path))
        {
            byte[] fileData = File.ReadAllBytes(path);
            textureAsli = new Texture2D(2, 2); // Ukuran awal tidak masalah, LoadImage akan mengubahnya
            
            if (ImageConversion.LoadImage(textureAsli, fileData))
            {
                // Tampilkan gambar asli
                imgAsli.texture = textureAsli;
                FitImage(imgAsli); // Jaga rasio aspek

                // Jalankan proses pengolahan
                ProsesGrayscale();
                ProsesHistogram();
            }
            else
            {
                Debug.LogError("Gagal memuat gambar dari path: " + path);
                txtInfo.text = "Gagal memuat gambar!";
            }
        }
        else
        {
            Debug.LogError("File tidak ditemukan di path: " + path);
            txtInfo.text = "File tidak ditemukan!";
        }
    }

    // Fungsi helper agar gambar proporsional di UI
    private void FitImage(RawImage targetImg)
    {
        Texture tex = targetImg.texture;
        if(tex == null) return;
        
        float ratio = (float)tex.width / tex.height;
        AspectRatioFitter fitter = targetImg.GetComponent<AspectRatioFitter>();
        if (fitter == null) fitter = targetImg.gameObject.AddComponent<AspectRatioFitter>();
        
        fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
        fitter.aspectRatio = ratio;
    }

    // --- 2. BAGIAN GRAYSCALE (rgb2gray) ---
    private void ProsesGrayscale()
    {
        if (textureAsli == null) return;
        
        Texture2D texGray = new Texture2D(textureAsli.width, textureAsli.height);
        Color[] pixels = textureAsli.GetPixels();
        Color[] grayPixels = new Color[pixels.Length];

        for (int i = 0; i < pixels.Length; i++)
        {
            // Rumus standar Luminositas
            float grayVal = (pixels[i].r * 0.299f) + (pixels[i].g * 0.587f) + (pixels[i].b * 0.114f);
            grayPixels[i] = new Color(grayVal, grayVal, grayVal, 1f);
        }

        texGray.SetPixels(grayPixels);
        texGray.Apply();

        imgGrayscale.texture = texGray;
        FitImage(imgGrayscale);
    }

    // --- 3. BAGIAN HISTOGRAM ---
    private void ProsesHistogram()
    {
        // Asumsi: [SerializeField] private TextMeshProUGUI txtMaxPixelCount; sudah dideklarasikan di class ImageProcessor.

        if (textureAsli == null) return;

        int hWidth = 256; 
        int hHeight = 100; // Tinggi tetap 100 piksel untuk mempermudah scaling
        Texture2D texHist = new Texture2D(hWidth, hHeight);

        int[] rCount = new int[256];
        int[] gCount = new int[256];
        int[] bCount = new int[256];

        Color[] pixels = textureAsli.GetPixels();
        int maxVal = 0;

        // 1. Hitung frekuensi dan cari nilai tertinggi (maxVal)
        foreach (Color p in pixels)
        {
            int r = (int)(p.r * 255); int g = (int)(p.g * 255); int b = (int)(p.b * 255);
            rCount[r]++; gCount[g]++; bCount[b]++;
            if (rCount[r] > maxVal) maxVal = rCount[r];
            if (gCount[g] > maxVal) maxVal = gCount[g];
            if (bCount[b] > maxVal) maxVal = gCount[b]; // ⭐ Perbaikan: Ini seharusnya bCount[b]
        }
        // Perbaikan kecil: Pastikan baris if ini memeriksa gCount[g] dan bCount[b] bukan gCount[g] dan gCount[b]
        // if (gCount[g] > maxVal) maxVal = gCount[g];
        // if (bCount[b] > maxVal) maxVal = bCount[b];

        // ⭐ MODIFIKASI DIMULAI DI SINI ⭐
        
        // Perbarui label sumbu Y (Y maksimum) dengan nilai maxVal yang ditemukan
        if (txtMaxPixelCount != null)
        {
            // Format "N0" akan menampilkan pemisah ribuan (misalnya 10,000)
            txtMaxPixelCount.text = maxVal.ToString("N0"); 
        }
        
        // ⭐ MODIFIKASI SELESAI ⭐

        Color[] histPixels = new Color[hWidth * hHeight];
        // Inisialisasi latar abu-abu/transparan
        for (int k = 0; k < histPixels.Length; k++) histPixels[k] = new Color(0,0,0,0.1f); 

        // 2. Gambar area grafik (menggunakan maxVal untuk normalisasi)
        for (int x = 0; x < hWidth; x++)
        {
            // Normalisasi tinggi bar
            int yR = (int)((float)rCount[x] / maxVal * hHeight);
            int yG = (int)((float)gCount[x] / maxVal * hHeight);
            int yB = (int)((float)bCount[x] / maxVal * hHeight);

            for (int y = 0; y < hHeight; y++)
            {
                int idx = y * hWidth + x;
                Color pixelColor = histPixels[idx];
                
                // Tambahkan warna dengan alpha (untuk efek tumpang tindih)
                if (y < yR) pixelColor += new Color(1, 0, 0, 0.3f);
                if (y < yG) pixelColor += new Color(0, 1, 0, 0.3f);
                if (y < yB) pixelColor += new Color(0, 0, 1, 0.3f);
                
                if (y < yR || y < yG || y < yB) pixelColor.a = 1f; // Pastikan area grafik terlihat solid
                histPixels[idx] = pixelColor;
            }
        }

        texHist.SetPixels(histPixels);
        texHist.Apply();
        imgHistogram.texture = texHist;
    }

    // --- FUNGSI KLIK GAMBAR (DIPANGGIL OLEH CLICKDETECTOR) ---
    // Fungsi ini dipanggil dari ClickDetector.cs
    public void ProsesPixel(PointerEventData eventData)
    {
        if (textureAsli == null) 
        {
            Debug.LogError("Error: Klik terdeteksi, tetapi textureAsli NULL. Muat gambar dulu!");
            return;
        }

        RectTransform rt = imgAsli.GetComponent<RectTransform>();
        Vector2 localPoint;
        
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, eventData.position, eventData.pressEventCamera, out localPoint))
        {
            float u = (localPoint.x + rt.rect.width * 0.5f) / rt.rect.width;
            float v = (localPoint.y + rt.rect.height * 0.5f) / rt.rect.height;

            int x = (int)(u * textureAsli.width);
            int y = (int)((1f - v) * textureAsli.height); // Inversi Y
            
            if (x >= 0 && x < textureAsli.width && y >= 0 && y < textureAsli.height)
            {
                Color c = textureAsli.GetPixel(x, y);
                
                txtInfo.text = string.Format(
                    "<b>Posisi:</b> X={0}, Y={1}\n<color=red>R={2}</color>  <color=green>G={3}</color>  <color=blue>B={4}</color>",
                    x, y,
                    (int)(c.r * 255),
                    (int)(c.g * 255),
                    (int)(c.b * 255)
                );
            }
        }
    }
}