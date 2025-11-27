using UnityEngine;
using UnityEngine.SceneManagement; // Wajib diimpor untuk SceneManager

public class SceneSwitcher : MonoBehaviour
{
    /**
     * Fungsi untuk memuat scene berdasarkan nama scene.
     * Dapat dipanggil dari tombol (Button) di Unity Editor.
     * @param sceneName Nama scene tujuan (misal: "MainMenu" atau "Level1").
     */
    public void GoToScene(string sceneName)
    {
        // Memuat scene yang memiliki nama yang sama dengan yang diberikan.
        SceneManager.LoadScene(sceneName);
    }
    
    /**
     * Fungsi opsional untuk memuat scene berikutnya berdasarkan Build Index.
     */
    public void GoToNextScene()
    {
        // Memuat scene dengan index urutan (SceneManager.GetActiveScene().buildIndex + 1)
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        
        // Pastikan index scene tidak melebihi jumlah scene yang terdaftar di Build Settings
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.LogWarning("Tidak ada scene berikutnya yang terdaftar di Build Settings!");
        }
    }
}