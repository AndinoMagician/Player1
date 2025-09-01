using UnityEngine;

public class Player : MonoBehaviour
{
    public Rigidbody2D rb;

    [Header("Particles")]
   //[SerializeField] private ParticleSystem dustFX;
   //[SerializeField] private GameObject landing;
   //[SerializeField] private GameObject jumpHitFX;
    //private float dustFxTimer;

    [Header("Move info")]
    public float moveSpeed;
    public float jumpForce;
    public float doubleJumpForce;
    public Vector2 wallJumpDirection;
    public float maxfallSpeed;
    [SerializeField] private float wallSlideSpeed = 2f;
    [SerializeField] private float cayoteJumpTime;
    [SerializeField] private float bufferJumpTime;
    private float bufferJumpCounter;
    private float cayoteJumpCounter;
    private bool canHaveCayoteJump;
    private float flipDelayTimer;

    [Header("Collision info")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask whatIsWall;
    [SerializeField] private float groundCheckDistance;
    [SerializeField] private float wallCheckDistance;
    [SerializeField] private Transform enemyCheck;
    [SerializeField] private float enemyCheckRadius;
    [SerializeField] private Transform enemyCheckJump;
    [SerializeField] private float enemyCheckJumpWidth = 2f;
    [SerializeField] private float enemyCheckJumpHeight = 1f;

    [Header("Knockback info")]
    [SerializeField] private Vector2 knockbackDirection;
    [SerializeField] private float knockbackTime;
    [SerializeField] private float knockbackProtectionTime;


    private float hInput;
    private bool isKnocked;
    private bool canBeKnocked = true;
    private bool canDoubleJump = true;
    private bool canMove;
    private bool readyToLand;
    public bool isGrounded;
    private bool isWallDetected;
    private bool canWallSlide;
    private bool isWallSliding;
    private bool justWallJumped;
    private bool isLandingOnEnemy;
    private bool canJumpOnEnemy = true;
    private bool facingRight = true;
    private int facingDirection =1;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        CollisionChecks();
        FlipController();
            
            
        // Move input
        hInput = Input.GetAxisRaw("Horizontal");

        // Jump input
        if (Input.GetButtonDown("Jump"))
        {
            Jump();
        }

        // Check grounded state
        //isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (isGrounded)
        {
            canDoubleJump = true;
        }
    }
      private void FixedUpdate()
    {
        // Move left/right
        rb.linearVelocity = new Vector2(hInput * moveSpeed, rb.linearVelocity.y);
    }
   
    private void Move()
    {
        if (canMove)
        {
            rb.linearVelocity = new Vector2(moveSpeed * hInput, rb.linearVelocity.y);
        }
    }
    private void WallJump()
    {
        //canMove = false;
        justWallJumped = true;
        rb.linearVelocity = new Vector2(wallJumpDirection.x * -facingDirection, wallJumpDirection.y);
        //dustFX.Play();
    }
     private void Jump()
    {   
        canHaveCayoteJump = false;
        cayoteJumpCounter =-1;

       if (isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
        else if (canDoubleJump)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, doubleJumpForce);
            canDoubleJump = false;
        }

        //dustFX.Play();
    }
     private void FlipController()
    {
        if (justWallJumped)
            return;

        //dustFxTimer -= Time.deltaTime;

        if (facingRight && rb.linearVelocity.x < 0)
        {
            Flip();
        }
        else if (!facingRight && rb.linearVelocity.x > 0)
        {
            Flip();
        }
    }
      private void Flip()
    {  

        // if(dustFxTimer < 0)
        // {
        //     dustFX.Play();
        //     dustFxTimer = .7f;
        // }
        facingDirection = facingDirection * -1; 
        facingRight = !facingRight;
        transform.Rotate(0, 180, 0);
    }
    private void CollisionChecks()
    {
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, groundLayer);
        isWallDetected = Physics2D.Raycast(transform.position, Vector2.right * facingDirection, wallCheckDistance, whatIsWall);
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position, new Vector3(transform.position.x, transform.position.y - groundCheckDistance));
        Gizmos.DrawLine(transform.position, new Vector3(transform.position.x * wallCheckDistance * facingDirection, transform.position.y));

        Gizmos.DrawWireSphere(enemyCheck.position, enemyCheckRadius);
        Vector2 boxSize = new Vector2(enemyCheckJumpWidth, enemyCheckJumpHeight);
        Gizmos.DrawWireCube(enemyCheckJump.position, boxSize);
    }
}
