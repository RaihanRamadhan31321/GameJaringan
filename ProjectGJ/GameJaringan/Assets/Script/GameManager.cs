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

    void Start()
    {
        if (!photonView.IsMine)
        {
            Destroy(rb);
            Destroy(animator);
            return;
        }

        rb = GetComponent<Rigidbody2D>();
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (rb != null)
        {
            rb.angularVelocity = 0;
            rb.rotation = 0;
        }
    }
}
