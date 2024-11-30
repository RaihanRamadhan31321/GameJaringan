using UnityEngine;

public class ExitGame : MonoBehaviour
{
    public void OnExitButtonClicked()
    {
        // Log pesan untuk debugging (hanya muncul di editor)
        Debug.Log("Keluar dari game...");

        // Jika dijalankan dalam editor, hentikan play mode
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // Jika dijalankan di build, keluar dari aplikasi
        Application.Quit();
#endif
    }
}
