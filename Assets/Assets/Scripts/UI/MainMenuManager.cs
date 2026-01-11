using UnityEngine;
using UnityEngine.SceneManagement; // Sahne yönetimi için gerekli

public class MainMenuManager : MonoBehaviour
{
    // Play butonuna basıldığında çağrılacak metot
    public void PlayGame()
    {
        // "SampleScene" kısmını oyunun ana sahnesinin adıyla değiştir
        // Görselde Scenes altında "SampleScene" görünüyor.
        SceneManager.LoadScene("SampleScene"); 
    }

    // Exit butonuna basıldığında çağrılacak metot
    public void QuitGame()
    {
        Debug.Log("Oyundan çıkıldı."); // Editörde çalıştığını anlamak için

#if UNITY_EDITOR
        // Eğer Unity Editör içerisindeysek, Play modunu durdurur
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // Eğer derlenmiş (Build) bir oyundaysak, uygulamayı tamamen kapatır
        Application.Quit();
#endif
    }
}