using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour
{
    /// <summary>
    /// Berpindah ke scene dengan nama tertentu.
    /// </summary>
    /// <param name="sceneName">Nama scene yang ingin dibuka.</param>
    public void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene("LoginLobby");
    }
}
