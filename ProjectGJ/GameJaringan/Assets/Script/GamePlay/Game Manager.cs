using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using Cinemachine;

public class GameManager : MonoBehaviourPunCallbacks
{
    [Header("Character Settings")]
    public GameObject redShirtPrefab; // Prefab untuk karakter baju merah

    private GameObject playerCharacter;
    private GameObject currentPlayerWithStatusU; // Pemain dengan status "U"
    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 movement;
    private bool isFacingRight = true;

    [Header("Camera Settings")]
    public CinemachineVirtualCamera virtualCamera;

    [Header("UI Settings")]
    public GameObject panelSetting;
    public Slider musicVolumeSlider;
    public AudioSource backgroundMusic;
    public Text timerText; // UI Text untuk menampilkan waktu mundur
    public GameObject losePanel; // Panel untuk pemain yang kalah
    public GameObject winnerPanel; // Panel untuk pemain yang menang

    private bool isPanelSettingActive = false;

    [Header("Gameplay Settings")]
    public float gameDuration = 60f; // Durasi permainan dalam detik (120 detik)
    private float remainingTime; // Waktu tersisa
    private bool isGameRunning = true; // Apakah permainan masih berjalan
    public float moveSpeed = 5f; // Kecepatan pergerakan pemain
    public float sprintMultiplier = 1.5f; // Faktor pengali kecepatan saat sprint

    private PhotonView photonView;
    private bool gameEnded = false;

    void Start()
    {
        photonView = GetComponent<PhotonView>();

        // Spawn karakter berdasarkan pemain
        Vector3 spawnPosition = PhotonNetwork.IsMasterClient ? new Vector3(-2, 0, 0) : new Vector3(2, 0, 0);
        playerCharacter = PhotonNetwork.Instantiate(redShirtPrefab.name, spawnPosition, Quaternion.identity);

        if (playerCharacter != null)
        {
            rb = playerCharacter.GetComponent<Rigidbody2D>();
            animator = playerCharacter.GetComponent<Animator>();

            // Atur kamera untuk mengikuti karakter pemain
            virtualCamera.Follow = playerCharacter.transform;

            // MasterClient memulai timer
            if (PhotonNetwork.IsMasterClient)
            {
                remainingTime = gameDuration;
                photonView.RPC("SyncGameStartTime", RpcTarget.Others, gameDuration);
            }
        }
        else
        {
            Debug.LogError("Player Character is not assigned!");
        }

        // Set volume awal
        if (backgroundMusic != null)
        {
            musicVolumeSlider.value = backgroundMusic.volume;
            musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        }

        // Status "U" langsung aktif pada MasterClient
        if (PhotonNetwork.IsMasterClient)
        {
            currentPlayerWithStatusU = playerCharacter;
            SetStatusU(playerCharacter, true);
        }
        else
        {
            SetStatusU(playerCharacter, false);
        }

        // Pastikan panel setting tidak aktif di awal
        panelSetting.SetActive(false);
        losePanel.SetActive(false);
        winnerPanel.SetActive(false);
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

        // Pastikan Status U mengikuti posisi karakter yang berjaga
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
        if (photonView.IsMine && playerCharacter != null)
        {
            // Mengambil input W, A, S, D
            movement.x = Input.GetAxisRaw("Horizontal");
            movement.y = Input.GetAxisRaw("Vertical");

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
                animator.SetFloat("Speed", movement.magnitude);
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
            GameObject statusUObject = statusUTransform.gameObject;

            // Aktifkan atau nonaktifkan StatusU
            statusUObject.SetActive(isActive);

            // Sinkronisasi komponen PhotonView
            PhotonView photonView = statusUObject.GetComponent<PhotonView>();
            if (photonView == null)
            {
                photonView = statusUObject.AddComponent<PhotonView>();
            }

            // Tambahkan PhotonTransformView jika belum ada
            PhotonTransformView transformView = statusUObject.GetComponent<PhotonTransformView>();
            if (transformView == null)
            {
                transformView = statusUObject.AddComponent<PhotonTransformView>();
                photonView.ObservedComponents.Add(transformView);

                // Konfigurasi transform view untuk sinkronisasi
                transformView.m_SynchronizePosition = true;
                transformView.m_SynchronizeRotation = true;
                transformView.m_SynchronizeScale = false;
            }

            // Pastikan PhotonView mengatur Ownership ke pemain yang relevan
            if (isActive && photonView.IsMine == false)
            {
                photonView.TransferOwnership(PhotonNetwork.LocalPlayer);
            }
        }
        else
        {
            Debug.LogError($"Player {player.name} does not have a 'StatusU' child object!");
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

    public void EndGame()
    {
        if (gameEnded) return;

        gameEnded = true;

        // Tentukan hasil akhir permainan berdasarkan status "U"
        if (currentPlayerWithStatusU == playerCharacter)
        {
            winnerPanel.SetActive(true);
        }
        else
        {
            losePanel.SetActive(true);
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
}
