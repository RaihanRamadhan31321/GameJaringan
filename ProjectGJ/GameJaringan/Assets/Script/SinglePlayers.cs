using UnityEngine;
using UnityEngine.UI;

public class SinglePlayers : MonoBehaviour
{
    [Header("Character Settings")]
    public GameObject redShirtCharacter;
    public GameObject blueShirtCharacter;

    private GameObject currentPlayerWithStatusU; // Pemain dengan status "U"
    private Rigidbody2D rbRedShirt;
    private Animator animatorRedShirt;
    private Rigidbody2D rbBlueShirt;

    [Header("AI Settings")]
    public float chaseSpeed = 5f;
    public float fleeSpeed = 10f;

    [Header("UI Settings")]
    public Text timerText;
    public GameObject losePanel;
    public GameObject winnerPanel;

    [Header("Gameplay Settings")]
    public float gameDuration = 60f;
    private float remainingTime;
    private bool isGameRunning = true;
    private bool gameEnded = false;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float sprintMultiplier = 1.5f;

    void Start()
    {
        // Pastikan kedua karakter aktif
        redShirtCharacter.SetActive(true);
        blueShirtCharacter.SetActive(true);

        // Setup komponen pemain
        rbRedShirt = redShirtCharacter.GetComponent<Rigidbody2D>();
        animatorRedShirt = redShirtCharacter.GetComponent<Animator>();
        rbBlueShirt = blueShirtCharacter.GetComponent<Rigidbody2D>();

        // Atur waktu mulai
        remainingTime = gameDuration;

        // Status "U" dimulai pada karakter baju biru
        currentPlayerWithStatusU = blueShirtCharacter;
        SetStatusU(blueShirtCharacter, true);
        SetStatusU(redShirtCharacter, false);

        // Pastikan UI panel tidak aktif
        losePanel.SetActive(false);
        winnerPanel.SetActive(false);
    }

    void Update()
    {
        if (isGameRunning)
        {
            // Update waktu mundur
            remainingTime -= Time.deltaTime;
            UpdateTimerUI();

            if (remainingTime <= 0 && !gameEnded)
            {
                EndGame();
            }

            // Logika AI untuk karakter baju biru
            if (!gameEnded)
            {
                if (currentPlayerWithStatusU == blueShirtCharacter)
                {
                    // Mengejar karakter baju merah
                    Vector2 direction = (redShirtCharacter.transform.position - blueShirtCharacter.transform.position).normalized;
                    rbBlueShirt.velocity = direction * chaseSpeed;
                }
                else
                {
                    // Kabur menjauh dari karakter baju merah
                    Vector2 direction = (blueShirtCharacter.transform.position - redShirtCharacter.transform.position).normalized;
                    rbBlueShirt.velocity = direction * fleeSpeed;
                }

                // Flip karakter baju biru berdasarkan posisi karakter baju merah
                FlipBlueShirt(redShirtCharacter.transform.position.x - blueShirtCharacter.transform.position.x);
            }

            // Kontrol pemain karakter baju merah
            HandleRedShirtMovement();
        }
    }

    void HandleRedShirtMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector2 movement = new Vector2(horizontal, vertical).normalized;

        float speed = moveSpeed;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            speed *= sprintMultiplier;
            SetRedShirtAnimation(2); // Run
        }
        else if (movement.magnitude > 0)
        {
            SetRedShirtAnimation(1); // Walk
        }
        else
        {
            SetRedShirtAnimation(0); // Idle
        }

        rbRedShirt.velocity = movement * speed;

        // Flip karakter berdasarkan arah horizontal
        if (horizontal != 0)
        {
            FlipCharacter(horizontal);
        }
    }

    void FlipCharacter(float horizontal)
    {
        Vector3 scale = redShirtCharacter.transform.localScale;
        if (horizontal > 0) // Gerak ke kanan
        {
            scale.x = Mathf.Abs(scale.x); // Pastikan orientasi positif
        }
        else if (horizontal < 0) // Gerak ke kiri
        {
            scale.x = -Mathf.Abs(scale.x); // Balik ke arah negatif
        }
        redShirtCharacter.transform.localScale = scale;
    }

    void FlipBlueShirt(float horizontalDifference)
    {
        Vector3 scale = blueShirtCharacter.transform.localScale;
        if (horizontalDifference > 0) // Karakter merah berada di kanan
        {
            scale.x = Mathf.Abs(scale.x); // Pastikan orientasi positif
        }
        else if (horizontalDifference < 0) // Karakter merah berada di kiri
        {
            scale.x = -Mathf.Abs(scale.x); // Balik ke arah negatif
        }
        blueShirtCharacter.transform.localScale = scale;
    }

    void SetRedShirtAnimation(int state)
    {
        if (animatorRedShirt != null)
        {
            animatorRedShirt.SetInteger("State", state);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (gameEnded) return;

        if (collision.gameObject == redShirtCharacter && currentPlayerWithStatusU == blueShirtCharacter)
        {
            TransferStatusU(redShirtCharacter);
        }
        else if (collision.gameObject == blueShirtCharacter && currentPlayerWithStatusU == redShirtCharacter)
        {
            TransferStatusU(blueShirtCharacter);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!gameEnded && collision.gameObject.CompareTag("Player"))
        {
            GameObject otherPlayer = collision.gameObject;
            TransferStatusU(otherPlayer);
        }
    }

    public void TransferStatusU(GameObject newPlayer)
    {
        if (newPlayer == null || currentPlayerWithStatusU == null)
            return;

        // Nonaktifkan status "U" dari pemain saat ini
        SetStatusU(currentPlayerWithStatusU, false);

        // Aktifkan status "U" untuk pemain baru
        currentPlayerWithStatusU = newPlayer;
        SetStatusU(currentPlayerWithStatusU, true);
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

    public void EndGame()
    {
        gameEnded = true;
        isGameRunning = false;

        if (currentPlayerWithStatusU == redShirtCharacter)
        {
            losePanel.SetActive(true); // Karakter baju merah kalah
            winnerPanel.SetActive(false);
        }
        else
        {
            losePanel.SetActive(false);
            winnerPanel.SetActive(true); // Karakter baju merah menang
        }

        // Hentikan pergerakan karakter baju biru
        rbBlueShirt.velocity = Vector2.zero;
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
}
