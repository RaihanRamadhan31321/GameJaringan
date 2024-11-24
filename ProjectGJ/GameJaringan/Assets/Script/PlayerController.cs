using UnityEngine;
using Photon.Pun;

public class PlayerController : MonoBehaviourPun
{
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private Transform ballHoldPoint;
    [SerializeField] private GameObject hitIndicator;

    private Animator animator;
    private Rigidbody2D rb;
    private GameObject heldBall;

    private float currentSpeed;

    private void Start()
    {
        if (!photonView.IsMine)
        {
            Destroy(GetComponent<Rigidbody2D>());
        }

        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        hitIndicator.SetActive(false);
        currentSpeed = walkSpeed;
    }

    private void Update()
    {
        if (!photonView.IsMine) return;

        HandleMovement();
        HandleBallInteraction();
    }

    private void HandleMovement()
    {
        // Movement input
        float moveX = 0f;
        float moveY = 0f;

        if (Input.GetKey(KeyCode.W)) moveY = 1f; // Up
        if (Input.GetKey(KeyCode.S)) moveY = -1f; // Down
        if (Input.GetKey(KeyCode.A)) moveX = -1f; // Left
        if (Input.GetKey(KeyCode.D)) moveX = 1f; // Right

        Vector2 movement = new Vector2(moveX, moveY).normalized;

        // Run if Shift is held
        currentSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;

        // Move the character
        rb.velocity = movement * currentSpeed;

        // Handle Animations
        if (movement.magnitude > 0)
        {
            animator.SetBool("isWalking", true);
            animator.SetBool("isRunning", Input.GetKey(KeyCode.LeftShift));
        }
        else
        {
            animator.SetBool("isWalking", false);
            animator.SetBool("isRunning", false);
        }

        // Flip sprite based on movement direction
        if (moveX != 0)
        {
            transform.localScale = new Vector3(Mathf.Sign(moveX), 1f, 1f);
        }
    }

    private void HandleBallInteraction()
    {
        if (Input.GetKeyDown(KeyCode.E) && heldBall == null)
        {
            Collider2D ballCollider = Physics2D.OverlapCircle(transform.position, 1f, LayerMask.GetMask("Ball"));
            if (ballCollider != null)
            {
                heldBall = ballCollider.gameObject;
                heldBall.transform.SetParent(ballHoldPoint);
                heldBall.transform.localPosition = Vector3.zero;
                heldBall.GetComponent<Rigidbody2D>().isKinematic = true;
            }
        }

        if (Input.GetMouseButtonDown(0) && heldBall != null)
        {
            heldBall.transform.SetParent(null);
            Rigidbody2D rb = heldBall.GetComponent<Rigidbody2D>();
            rb.isKinematic = false;
            rb.AddForce(transform.up * 500f);
            heldBall = null;
        }
    }

    [PunRPC]
    public void ShowHitIndicator()
    {
        hitIndicator.SetActive(true);
        Invoke(nameof(HideHitIndicator), 2f);
    }

    private void HideHitIndicator()
    {
        hitIndicator.SetActive(false);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ball"))
        {
            PhotonView ballView = collision.gameObject.GetComponent<PhotonView>();
            if (ballView != null && !photonView.IsMine)
            {
                photonView.RPC(nameof(ShowHitIndicator), RpcTarget.All);
            }
        }
    }
}
