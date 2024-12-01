using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using System.Collections;

public class GameCountdown : MonoBehaviourPunCallbacks
{
    public Text countdownText; // UI untuk waktu mundur
    public GameObject losePanel; // Panel untuk pemain yang kalah
    public GameObject winnerPanel; // Panel untuk pemain yang menang
    public GameObject statusU; // Tanda "U" di atas pemain

    public InputField timerInputField; // Input field untuk pengaturan waktu mundur
    private float countdownTime = 30f; // Default waktu mundur
    private bool gameEnded = false;

    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("StartCountdown", RpcTarget.AllBuffered, countdownTime);
        }
    }

    public void SetCountdownTime()
    {
        if (float.TryParse(timerInputField.text, out float inputTime) && inputTime > 0)
        {
            countdownTime = inputTime;
            Debug.Log("Waktu mundur diatur ke: " + countdownTime + " detik.");
        }
        else
        {
            Debug.LogWarning("Masukkan waktu yang valid! (angka positif)");
        }
    }

    [PunRPC]
    void StartCountdown(float startTime)
    {
        countdownTime = startTime;
        gameEnded = false;
        losePanel.SetActive(false);
        winnerPanel.SetActive(false);
        StartCoroutine(Countdown());
    }

    IEnumerator Countdown()
    {
        while (countdownTime > 0 && !gameEnded)
        {
            countdownTime -= Time.deltaTime;
            countdownText.text = Mathf.CeilToInt(countdownTime).ToString(); // Tampilkan waktu
            yield return null;
        }

        if (!gameEnded)
        {
            photonView.RPC("CheckGameOver", RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    void CheckGameOver()
    {
        if (statusU.activeSelf)
        {
            // Jika pemain masih "jaga", mereka kalah
            if (photonView.IsMine)
            {
                losePanel.SetActive(true);
            }
        }
        else
        {
            // Jika pemain tidak "jaga", mereka menang
            if (photonView.IsMine)
            {
                winnerPanel.SetActive(true);
            }
        }

        gameEnded = true;
    }

    public void ResetGame()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("StartCountdown", RpcTarget.AllBuffered, countdownTime);
        }
    }
}
