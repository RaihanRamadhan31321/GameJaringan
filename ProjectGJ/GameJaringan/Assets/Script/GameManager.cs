using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using Cinemachine;

public class GameManager : MonoBehaviourPunCallbacks
{
    [Header("Character Settings")]
    public GameObject redShirtCharacter;
    public GameObject blueShirtCharacter;

    private GameObject playerCharacter;
    private GameObject currentPlayerWithStatusU; // Pemain dengan status "U"
    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 movement;
    private bool isFacingRight = true;

    [Header("Camera Settings")]
    public CinemachineVirtualCamera virtualCameraRed;
    public CinemachineVirtualCamera virtualCameraBlue;

    [Header("UI Settings")]
    public GameObject panelSetting;
    public Slider musicVolumeSlider;
    public AudioSource backgroundMusic;
    public Text timerText; // UI Text untuk menampilkan waktu mundur
    public GameObject losePanel; // Panel untuk pemain yang kalah
    public GameObject winnerPanel; // Panel untuk pemain yang menang

    private bool isPanelSettingActive = false;

    [Header("Gameplay Settings")]
    public float gameDuration = 60f; // Durasi permainan dalam detik (60 detik)
    private float remainingTime; // Waktu tersisa
    private bool isGameRunning = true; // Apakah permainan masih berjalan
    public float moveSpeed = 5f; // Kecepatan pergerakan pemain
    public float sprintMultiplier = 1.5f; // Faktor pengali kecepatan saat sprint

    private PhotonView photonView;
    private bool gameEnded = false;

    void Start()
    {
        photonView = GetComponent<PhotonView>();

        // Pastikan kedua karakter aktif
        redShirtCharacter.SetActive(true);
        blueShirtCharacter.SetActive(true);

        // Tentukan karakter yang dikontrol berdasarkan pemain
        if (PhotonNetwork.IsMasterClient)
        {
            playerCharacter = redShirtCharacter;
            virtualCameraRed.Follow = playerCharacter.transform;
            virtualCameraRed.enabled = true;
            virtualCameraBlue.enabled = false;
        }
        else
        {
            playerCharacter = blueShirtCharacter;
            virtualCameraBlue.Follow = playerCharacter.transform;
            virtualCameraBlue.enabled = true;
            virtualCameraRed.enabled = false;
        }

        // Setup komponen pemain
        if (playerCharacter != null)
        {
            rb = playerCharacter.GetComponent<Rigidbody2D>();
            animator = playerCharacter.GetComponent<Animator>();
        }

        // Status "U" langsung aktif pada karakter baju merah
        if (redShirtCharacter != null && blueShirtCharacter != null)
        {
            currentPlayerWithStatusU = redShirtCharacter;
            SetStatusU(redShirtCharacter, true);  // Aktifkan status "U" pada karakter baju merah
            SetStatusU(blueShirtCharacter, false); // Nonaktifkan status "U" pada karakter baju biru
        }

        // Pastikan panel setting tidak aktif di awal
        panelSetting.SetActive(false);
        losePanel.SetActive(false);
        winnerPanel.SetActive(false);

        // Set waktu mulai
        remainingTime = gameDuration;
    }

    void Update()
    {
        if (!isPanelSettingActive && isGameRunning)
        {
            // Update waktu mundur
            if (PhotonNetwork.IsMasterClient)
            {
                remainingTime -= Time.deltaTime;

                if (remainingTime <= 0)
                {
                    remainingTime = 0;
                    EndGame();
                }

                // Sinkronisasi waktu ke semua pemain
                photonView.RPC("SyncRemainingTime", RpcTarget.Others, remainingTime);
            }

            // Perbarui UI waktu mundur
            UpdateTimerUI();

            // Pergerakan pemain
            HandleMovement();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleSettingPanel();
        }

        // Status U mengikuti posisi karakter yang berjaga
        if (currentPlayerWithStatusU != null)
        {
            Transform statusUTransform = currentPlayerWithStatusU.transform.Find("StatusU");
            if (statusUTransform != null)
            {
                statusUTransform.position = currentPlayerWithStatusU.transform.position + new Vector3(0, 1.5f, 0); // Letakkan di atas karakter
            }
        }
    }

    private void HandleMovement()
    {
        if (playerCharacter != null)
        {
            // Mengambil input untuk pergerakan
            if (playerCharacter == redShirtCharacter) // Pemain pertama (Red Shirt)
            {
                movement.x = Input.GetAxisRaw("Horizontal");
                movement.y = Input.GetAxisRaw("Vertical");
            }
            else if (playerCharacter == blueShirtCharacter) // Pemain kedua (Blue Shirt)
            {
                movement.x = Input.GetAxisRaw("Horizontal2"); // Gunakan axis custom (Horizontal2) untuk pemain kedua
                movement.y = Input.GetAxisRaw("Vertical2");   // Gunakan axis custom (Vertical2) untuk pemain kedua
            }

            // Normalisasi gerakan agar tidak lebih dari 1 saat diagonal
            movement = movement.normalized;

            // Tambahkan kecepatan saat tombol Shift kiri ditekan
            float currentSpeed = moveSpeed;
            if (Input.GetKey(KeyCode.LeftShift))
            {
                currentSpeed *= sprintMultiplier;
            }

            // Animasi pergerakan
            if (animator != null)
            {
                animator.SetFloat("Speed", movement.magnitude); // Mengatur animasi kecepatan
            }

            // Flip karakter jika berubah arah horizontal (hanya pada sumbu X)
            if (movement.x > 0 && !isFacingRight)
            {
                Flip();
            }
            else if (movement.x < 0 && isFacingRight)
            {
                Flip();
            }

            // Terapkan gerakan ke Rigidbody2D
            rb.velocity = movement * currentSpeed;
        }
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;

        Vector3 localScale = playerCharacter.transform.localScale;
        localScale.x *= -1; // Membalikkan sumbu X
        playerCharacter.transform.localScale = localScale;
    }

    private void SetStatusU(GameObject player, bool isActive)
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

    void OnTriggerEnter2D(Collider2D other)
    {
        if (gameEnded) return;

        // Jika pemain dengan status "U" bertabrakan dengan pemain lain
        if (other.gameObject == currentPlayerWithStatusU)
        {
            GameObject otherPlayer = other.gameObject == redShirtCharacter ? blueShirtCharacter : redShirtCharacter;

            // Pastikan pemain lain valid
            if (otherPlayer != null)
            {
                // Pindahkan status "U" ke pemain lain
                TransferStatusU(otherPlayer);
            }
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

    public void EndGame()
    {
        if (gameEnded) return;

        gameEnded = true;

        // Tentukan hasil akhir permainan berdasarkan status "U"
        if (currentPlayerWithStatusU == redShirtCharacter)
        {
            losePanel.SetActive(true); // Karakter baju merah kalah
            winnerPanel.SetActive(false);
        }
        else if (currentPlayerWithStatusU == blueShirtCharacter)
        {
            losePanel.SetActive(true); // Karakter baju biru kalah
            winnerPanel.SetActive(false);
        }

        // Pemain tanpa status "U" menang
        if (currentPlayerWithStatusU != redShirtCharacter)
        {
            winnerPanel.SetActive(true); // Karakter baju biru menang
        }
        else if (currentPlayerWithStatusU != blueShirtCharacter)
        {
            winnerPanel.SetActive(true); // Karakter baju merah menang
        }
    }

    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(remainingTime / 60);
            int seconds = Mathf.FloorToInt(remainingTime % 60);
            timerText.text = $"{minutes:00}:{seconds:00}";
        }
    }

    private void ToggleSettingPanel()
    {
        isPanelSettingActive = !isPanelSettingActive;
        panelSetting.SetActive(isPanelSettingActive);

        Time.timeScale = isPanelSettingActive ? 0 : 1;
    }

    private void SetMusicVolume(float volume)
    {
        if (backgroundMusic != null)
        {
            backgroundMusic.volume = volume;
        }
    }

    [PunRPC]
    private void SyncGameStartTime(float duration)
    {
        remainingTime = duration;
    }

    [PunRPC]
    private void SyncRemainingTime(float time)
    {
        remainingTime = time;
    }
}