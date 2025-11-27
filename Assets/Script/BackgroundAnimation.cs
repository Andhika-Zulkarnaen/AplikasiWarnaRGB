using UnityEngine;
using UnityEngine.UI;

public class BackgroundAnimator : MonoBehaviour
{
    private Image bgImage;
    public Color colorA = new Color(0.1f, 0.2f, 0.3f, 1f); // Biru Gelap
    public Color colorB = new Color(0.3f, 0.1f, 0.3f, 1f); // Ungu Gelap
    public float speed = 0.5f; // Kecepatan Transisi

    void Start()
    {
        // Mendapatkan komponen Image
        bgImage = GetComponent<Image>();
    }

    void Update()
    {
        // Menggunakan fungsi sin untuk mendapatkan nilai antara 0 dan 1 yang berulang
        float t = (Mathf.Sin(Time.time * speed) + 1f) / 2f; 
        
        // Melakukan interpolasi (peralihan) halus antara ColorA dan ColorB
        bgImage.color = Color.Lerp(colorA, colorB, t);
    }
}