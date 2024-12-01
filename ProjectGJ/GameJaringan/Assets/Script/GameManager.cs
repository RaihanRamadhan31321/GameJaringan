using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class GameManager : MonoBehaviourPunCallbacks
{
    [Header("Character Settings")]
    public float walkSpeed = 5f;
    public float runSpeed = 10f;
    public Animator animator;

    private Rigidbody2D rb;
    private Vector2 movement;
    private bool isRunning;
    private bool isFacingRight = true; // Menyimpan arah pandangan karakter

    [Header("UI Settings")]
    public GameObject panelSetting; // Panel pengaturan
    public Slider musicVolumeSlider; // Slider untuk volume musik
    public AudioSource backgroundMusic; // Sumber audio untuk musik latar

    private bool isPanelSettingActive = false;

    void Start()
    {
        if (!photonView.IsMine)
        {
            Destroy(rb);
            Destroy(animator);
            return;
        }

        rb = GetComponent<Rigidbody2D>();

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
        if (!isPanelSettingActive)
        {
            // Input untuk gerakan karakter
            movement.x = Input.GetAxisRaw("Horizontal");
            movement.y = Input.GetAxisRaw("Vertical");
            movement = movement.normalized;

            // Cek jika lari
            isRunning = Input.GetKey(KeyCode.LeftShift);

            // Atur animasi berdasarkan gerakan
            float animationSpeed = movement.magnitude * (isRunning ? runSpeed : walkSpeed);
            animator.SetFloat("Speed", animationSpeed);

            // Flip karakter jika bergerak ke kiri atau kanan
            if (movement.x > 0 && !isFacingRight)
            {
                Flip();
            }
            else if (movement.x < 0 && isFacingRight)
            {
                Flip();
            }
        }

        // Tampilkan atau sembunyikan panel setting saat tombol Esc ditekan
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleSettingPanel();
        }
    }

    void FixedUpdate()
    {
        if (!isPanelSettingActive)
        {
            float currentSpeed = isRunning ? runSpeed : walkSpeed;

            if (rb != null)
            {
                rb.velocity = movement * currentSpeed;
            }
            else
            {
                Debug.LogError("Rigidbody2D tidak ditemukan!");
            }
        }
        else
        {
            // Hentikan gerakan jika panel setting aktif
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (rb != null)
        {
            rb.angularVelocity = 0;
            rb.rotation = 0;
        }
    }

    private void Flip()
    {
        // Membalik arah pandangan karakter
        isFacingRight = !isFacingRight;

        Vector3 localScale = transform.localScale;
        localScale.x *= -1; // Balik sumbu X
        transform.localScale = localScale;
    }

    private void ToggleSettingPanel()
    {
        isPanelSettingActive = !isPanelSettingActive;
        panelSetting.SetActive(isPanelSettingActive);

        // Hentikan waktu jika panel setting aktif
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
