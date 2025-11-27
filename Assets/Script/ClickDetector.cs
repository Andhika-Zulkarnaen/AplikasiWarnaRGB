using UnityEngine;
using UnityEngine.EventSystems;

public class ClickDetector : MonoBehaviour, IPointerClickHandler
{
    // Variabel untuk menghubungkan kembali ke ImageProcessor
    public ImageProcessor processor; 

    public void OnPointerClick(PointerEventData eventData)
    {
        // Debugging paling dasar: pastikan klik tercapai
        Debug.Log("KLIK BERHASIL DITANGKAP OLEH GAMBAR ASLI!"); 
        
        // Panggil fungsi pemrosesan utama di ImageProcessor
        if (processor != null)
        {
            processor.ProsesPixel(eventData); // Kita akan membuat fungsi ini
        }
    }
}