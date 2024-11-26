using UnityEngine;
using Photon.Pun;

public class GameManager : MonoBehaviourPunCallbacks
{
    [Header("Character Settings")]
    public float walkSpeed = 5f;
    public float runSpeed = 10f;
    public Animator animator;

    private Rigidbody2D rb;
    private Vector2 movement;
    private bool isRunning;

    [Header("Ball Settings")]
    public GameObject ballPrefab;  // Prefab bola yang akan dipakai
    public Transform holdPosition; // Posisi karakter untuk memegang bola
    public float throwForce = 10f;
    private GameObject ball;      // Instansiasi bola
    private bool isHoldingBall = false;

    void Start()
    {
        if (!photonView.IsMine)
        {
            Destroy(rb);
            Destroy(animator);
            return;
        }

        rb = GetComponent<Rigidbody2D>();

        // Jika bola belum ada, spawn bola
        if (ball == null && ballPrefab != null)
        {
            SpawnBall();
        }
    }

    void Update()
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

        // Otomatis ambil bola jika mendekat
        AutoPickupBall();

        // Lempar bola
        if (Input.GetMouseButtonDown(0) && isHoldingBall)
        {
            ThrowBall();
        }
    }

    void FixedUpdate()
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

    private void AutoPickupBall()
    {
        if (ball != null && !isHoldingBall)
        {
            float distanceToBall = Vector2.Distance(transform.position, ball.transform.position);
            if (distanceToBall <= 1.5f)
            {
                ball.GetComponent<Rigidbody2D>().isKinematic = true;
                ball.transform.position = holdPosition.position;
                ball.transform.SetParent(holdPosition);
                isHoldingBall = true;

                // Ubah warna bola untuk indikasi
                ball.GetComponent<SpriteRenderer>().color = Color.green;
            }
        }
    }

    private void ThrowBall()
    {
        if (ball != null && isHoldingBall)
        {
            ball.GetComponent<Rigidbody2D>().isKinematic = false;
            ball.transform.SetParent(null);

            // Hitung arah lemparan berdasarkan posisi mouse
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 throwDirection = (mousePosition - (Vector2)transform.position).normalized;

            ball.GetComponent<Rigidbody2D>().velocity = throwDirection * throwForce;

            isHoldingBall = false;

            // Kembalikan warna bola
            ball.GetComponent<SpriteRenderer>().color = Color.white;
        }
    }

    private void SpawnBall()
    {
        if (ballPrefab != null)
        {
            // Spawn bola pada posisi tertentu
            ball = Instantiate(ballPrefab, holdPosition.position, Quaternion.identity);
        }
        else
        {
            Debug.LogError("Prefab bola belum diset!");
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
}
