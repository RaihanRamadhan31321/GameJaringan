// using UnityEngine;
// using UnityEngine.UI;
// using Photon.Pun;

// public class GameCountdown : MonoBehaviourPunCallbacks
// {
//     public Text countdownText; // UI untuk waktu mundur
//     public GameObject losePanel; // Panel untuk pemain yang kalah
//     public GameObject winnerPanel; // Panel untuk pemain yang menang
//     public GameObject statusU; // Tanda "U" di atas pemain

//     private float countdownTime = 30f;
//     private bool gameEnded = false;

//     void Start()
//     {
//         if (PhotonNetwork.IsMasterClient)
//         {
//             photonView.RPC("StartCountdown", RpcTarget.AllBuffered, countdownTime);
//         }
//     }

//     [PunRPC]
//     void StartCountdown(float startTime)
//     {
//         countdownTime = startTime;
//         gameEnded = false;
//         losePanel.SetActive(false);
//         winnerPanel.SetActive(false);
//         StartCoroutine(Countdown());
//     }

//     IEnumerator Countdown()
//     {
//         while (countdownTime > 0 && !gameEnded)
//         {
//             countdownTime -= Time.deltaTime;
//             countdownText.text = Mathf.CeilToInt(countdownTime).ToString(); // Tampilkan waktu
//             yield return null;
//         }

//         if (!gameEnded)
//         {
//             photonView.RPC("CheckGameOver", RpcTarget.AllBuffered);
//         }
//     }

//     [PunRPC]
//     void CheckGameOver()
//     {
//         if (statusU.activeSelf)
//         {
//             // Jika pemain masih "jaga", mereka kalah
//             if (photonView.IsMine)
//             {
//                 losePanel.SetActive(true);
//             }
//         }
//         else
//         {
//             // Jika pemain tidak "jaga", mereka menang
//             if (photonView.IsMine)
//             {
//                 winnerPanel.SetActive(true);
//             }
//         }

//         gameEnded = true;
//     }

//     public void ResetGame()
//     {
//         if (PhotonNetwork.IsMasterClient)
//         {
//             photonView.RPC("StartCountdown", RpcTarget.AllBuffered, 30f);
//         }
//     }
// }
