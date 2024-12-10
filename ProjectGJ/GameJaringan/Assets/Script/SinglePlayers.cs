using UnityEngine;
using UnityEngine.AI;
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
    public NavMeshAgent blueShirtAgent;
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

        // Atur NavMesh Agent untuk karakter baju biru
        if (blueShirtAgent != null)
        {
            blueShirtAgent.speed = chaseSpeed;
            blueShirtAgent.updateRotation = false;
            blueShirtAgent.updateUpAxis = false;
        }
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
            if (blueShirtAgent != null && !gameEnded)
            {
                if (currentPlayerWithStatusU == blueShirtCharacter)
                {
                    // Mengejar karakter baju merah
                    blueShirtAgent.speed = chaseSpeed;
                    blueShirtAgent.SetDestination(redShirtCharacter.transform.position);
                }
                else
                {
                    // Kabur menjauh dari karakter baju merah
                    Vector3 fleeDirection = (blueShirtCharacter.transform.position - redShirtCharacter.transform.position).normalized;
                    Vector3 fleePosition = blueShirtCharacter.transform.position + fleeDirection * 10f; // Kabur sejauh 10 unit
                    blueShirtAgent.speed = fleeSpeed;
                    blueShirtAgent.SetDestination(fleePosition);
                }
            }
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
            losePanel.SetActive(false);
            winnerPanel.SetActive(true); // Karakter baju merah menang
        }
        else
        {
            losePanel.SetActive(true); // Karakter baju merah kalah
            winnerPanel.SetActive(false);
        }

        // Hentikan AI karakter baju biru
        if (blueShirtAgent != null)
        {
            blueShirtAgent.isStopped = true;
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
}
