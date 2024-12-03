using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using Cinemachine;

public class GameManager : MonoBehaviourPunCallbacks
{
    [Header("Character Settings")]
    public GameObject redShirtCharacter; // Karakter baju merah
    public GameObject blueShirtCharacter; // Karakter baju biru

    private GameObject playerCharacter; // Referensi karakter pemain
    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 movement;
    private bool isRunning;
    private bool isFacingRight = true;

    [Header("Camera Settings")]
    public CinemachineVirtualCamera virtualCameraRed;  // Kamera untuk karakter baju merah
    public CinemachineVirtualCamera virtualCameraBlue; // Kamera untuk karakter baju biru

    [Header("UI Settings")]
    public GameObject panelSetting;
    public Slider musicVolumeSlider;
    public AudioSource backgroundMusic;

    private bool isPanelSettingActive = false;

    [Header("Gameplay Settings")]
    public float gameStartTime;
    private float elapsedTime;

    private PhotonView photonView;
    private bool isPlayerTwoJoined = false; // Status apakah pemain kedua sudah masuk

    void Start()
    {
        photonView = GetComponent<PhotonView>();

        // Pastikan kedua karakter aktif
        redShirtCharacter.SetActive(true);
        blueShirtCharacter.SetActive(true);

        // Pemain pertama mengontrol karakter baju merah, pemain kedua mengontrol baju biru
        if (PhotonNetwork.IsMasterClient)
        {
            playerCharacter = redShirtCharacter;
        }
        else
        {
            playerCharacter = blueShirtCharacter;
        }

        // Setup komponen pemain
        if (playerCharacter != null)
        {
            rb = playerCharacter.GetComponent<Rigidbody2D>();
            animator = playerCharacter.GetComponent<Animator>();

            // Atur kamera untuk mengikuti karakter pemain
            if (PhotonNetwork.IsMasterClient)
            {
                virtualCameraRed.Follow = playerCharacter.transform;
                virtualCameraRed.enabled = true;
                virtualCameraBlue.enabled = false;
            }
            else
            {
                virtualCameraBlue.Follow = playerCharacter.transform;
                virtualCameraBlue.enabled = true;
                virtualCameraRed.enabled = false;
            }

            // Pastikan karakter memiliki PhotonView dan TransformView
            if (!playerCharacter.GetComponent<PhotonView>())
            {
                PhotonView charPhotonView = playerCharacter.AddComponent<PhotonView>();
                charPhotonView.ObservedComponents = new System.Collections.Generic.List<Component>
                {
                    playerCharacter.AddComponent<PhotonTransformView>(),
                    playerCharacter.GetComponent<Animator>()
                };
                charPhotonView.Synchronization = ViewSynchronization.UnreliableOnChange;
            }
        }
        else
        {
            Debug.LogError("Player Character is not assigned!");
        }

        // Sinkronisasi waktu permainan
        if (PhotonNetwork.IsMasterClient)
        {
            gameStartTime = (float)PhotonNetwork.Time; // Waktu awal diambil dari server
            photonView.RPC("SyncGameStartTime", RpcTarget.Others, gameStartTime);
        }

        // Pastikan hanya pemain lokal yang mengontrol karakter mereka
        if (!photonView.IsMine)
        {
            Destroy(rb);
            Destroy(animator);
            return;
        }

        // Set volume awal dari audio source
        if (backgroundMusic != null)
        {
            musicVolumeSlider.value = backgroundMusic.volume;
            musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        }

        // Pastikan panel setting tidak aktif di awal
        panelSetting.SetActive(false);
    }

    void Update()
    {
        if (!isPanelSettingActive && photonView.IsMine)
        {
            if (playerCharacter == blueShirtCharacter && !isPlayerTwoJoined)
            {
                // Jika pemain kedua belum masuk, karakter biru tidak bisa bergerak
                return;
            }

            movement.x = Input.GetAxisRaw("Horizontal");
            movement.y = Input.GetAxisRaw("Vertical");
            movement = movement.normalized;

            isRunning = Input.GetKey(KeyCode.LeftShift);

            // Sinkronisasi animasi ke semua pemain
            float animationSpeed = movement.magnitude * (isRunning ? 10f : 5f);
            if (animator != null)
            {
                animator.SetFloat("Speed", animationSpeed);
                photonView.RPC("SyncAnimation", RpcTarget.Others, animationSpeed);
            }

            if (movement.x > 0 && !isFacingRight)
            {
                Flip();
            }
            else if (movement.x < 0 && isFacingRight)
            {
                Flip();
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleSettingPanel();
        }

        elapsedTime = (float)PhotonNetwork.Time - gameStartTime;
    }

    void FixedUpdate()
    {
        if (!isPanelSettingActive && rb != null && photonView.IsMine)
        {
            if (playerCharacter == blueShirtCharacter && !isPlayerTwoJoined)
            {
                // Jika pemain kedua belum masuk, karakter biru tidak bisa bergerak
                rb.velocity = Vector2.zero;
                return;
            }

            float currentSpeed = isRunning ? 10f : 5f;
            rb.velocity = movement * currentSpeed;
        }
        else if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;

        Vector3 localScale = playerCharacter.transform.localScale;
        localScale.x *= -1;
        playerCharacter.transform.localScale = localScale;

        // Sinkronisasi flipping ke semua pemain
        photonView.RPC("SyncFlip", RpcTarget.All, isFacingRight);
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
    private void SyncGameStartTime(float startTime)
    {
        gameStartTime = startTime;
    }

    [PunRPC]
    private void SyncAnimation(float speed)
    {
        if (animator != null)
        {
            animator.SetFloat("Speed", speed);
        }
    }

    [PunRPC]
    private void SyncFlip(bool facingRight)
    {
        isFacingRight = facingRight;

        Vector3 localScale = playerCharacter.transform.localScale;
        localScale.x = facingRight ? Mathf.Abs(localScale.x) : -Mathf.Abs(localScale.x);
        playerCharacter.transform.localScale = localScale;
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);

        // Aktifkan kontrol karakter biru jika pemain kedua masuk
        if (PhotonNetwork.IsMasterClient)
        {
            isPlayerTwoJoined = true;
        }
    }
}
