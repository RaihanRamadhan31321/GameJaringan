using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class GameMechanics : MonoBehaviourPunCallbacks
{
    public GameObject losePanel; // Panel untuk pemain yang kalah
    public GameObject winnerPanel; // Panel untuk pemain yang menang
    public GameObject redShirtCharacter; // Karakter baju merah
    public GameObject blueShirtCharacter; // Karakter baju biru

    private GameObject currentPlayerWithStatusU; // Pemain yang saat ini memiliki status "U"
    private bool gameEnded = false;

    void Start()
    {
        // Atur ulang status awal permainan
        gameEnded = false;
        losePanel.SetActive(false);
        winnerPanel.SetActive(false);

        // Status "U" langsung aktif pada karakter baju merah
        if (redShirtCharacter != null)
        {
            currentPlayerWithStatusU = redShirtCharacter;
            SetStatusU(redShirtCharacter, true); // Aktifkan status "U" pada karakter baju merah
        }
        else
        {
            Debug.LogError("Red shirt character is not assigned!");
        }

        if (blueShirtCharacter != null)
        {
            SetStatusU(blueShirtCharacter, false); // Nonaktifkan status "U" pada karakter baju biru
        }
        else
        {
            Debug.LogError("Blue shirt character is not assigned!");
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (gameEnded) return;

        // Jika pemain dengan status "U" bertabrakan dengan pemain lain
        if (collision.gameObject == currentPlayerWithStatusU)
        {
            GameObject otherPlayer = collision.otherCollider.gameObject;

            // Pastikan pemain lain valid
            if (otherPlayer != redShirtCharacter && otherPlayer != blueShirtCharacter)
                return;

            // Pindahkan status "U" ke pemain lain
            TransferStatusU(otherPlayer);
        }
    }

    void TransferStatusU(GameObject newPlayer)
    {
        if (newPlayer == null || currentPlayerWithStatusU == null)
            return;

        // Nonaktifkan status "U" dari pemain saat ini
        SetStatusU(currentPlayerWithStatusU, false);

        // Aktifkan status "U" untuk pemain baru
        currentPlayerWithStatusU = newPlayer;
        SetStatusU(currentPlayerWithStatusU, true);
    }

    void SetStatusU(GameObject player, bool isActive)
    {
        Transform statusUTransform = player.transform.Find("StatusU");
        if (statusUTransform != null)
        {
            statusUTransform.gameObject.SetActive(isActive);
        }
        else
        {
            Debug.LogError($"Player {player.name} does not have a 'StatusU' child object!");
        }
    }

    public void EndGame()
    {
        if (gameEnded) return;

        gameEnded = true;

        // Tentukan hasil akhir permainan berdasarkan status "U"
        if (currentPlayerWithStatusU == redShirtCharacter)
        {
            losePanel.SetActive(true); // Karakter baju merah kalah
        }
        else if (currentPlayerWithStatusU == blueShirtCharacter)
        {
            losePanel.SetActive(true); // Karakter baju biru kalah
        }

        // Pemain tanpa status "U" menang
        if (currentPlayerWithStatusU == redShirtCharacter)
        {
            winnerPanel.SetActive(true); // Karakter baju biru menang
        }
        else if (currentPlayerWithStatusU == blueShirtCharacter)
        {
            winnerPanel.SetActive(true); // Karakter baju merah menang
        }
    }
}
