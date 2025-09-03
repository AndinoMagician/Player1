using System;
using UnityEngine;

public class Player : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator anim;


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
    [SerializeField] private float bufferJumpTime;
    private float bufferJumpCounter;
    [SerializeField] private float cayoteJumpTime;
    private float cayoteJumpCounter;
    private bool canHaveCayoteJump;
    private float flipDelayTimer;

    [Header("Collision info")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask whatIsWall;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckDistance;
    [SerializeField] private Transform wallCheck;
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


    //private float hInput;
    private float movingInput;
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
    private int facingDirection = 1;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        
    }

    void Update()
    {
        AnimationsConrollers();
        CollisionChecks();
        FlipController();
        InputChecks();

        canMove = true;

        bufferJumpCounter -= Time.deltaTime;
        cayoteJumpCounter -= Time.deltaTime;

        if (isGrounded)
        {
            canDoubleJump = true;
            canMove = true;

            if (bufferJumpCounter > 0)
            {
                bufferJumpCounter = -1;
                Jump();
            }

            canHaveCayoteJump = true;
        }
        else
        {   if (canHaveCayoteJump)
            {
                canHaveCayoteJump = false;
                cayoteJumpCounter = cayoteJumpTime;
            }
        }

        if (canWallSlide)
        {
            isWallSliding = true;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.1f);
        }
        else
        {
            isWallSliding = false;
        }

        Move();
    }

    private void AnimationsConrollers()
    {
        bool isMoving = rb.linearVelocity.x != 0;
        anim.SetBool("isGrounded", isGrounded);
        anim.SetBool("isMoving", isMoving);
        anim.SetFloat("yVelocity", rb.linearVelocity.y);
        anim.SetBool("isWallSliding", isWallSliding);
        anim.SetBool("isWallDetected", isWallDetected);
    }
    private void InputChecks()
    {
    
        movingInput = Input.GetAxisRaw("Horizontal");

        if (Input.GetAxis("Vertical") < 0)
            canWallSlide = false;

        if (Input.GetButtonDown("Jump"))
            {
                JumpButton();
            }
    }

    private void Move()
    {
        if(canMove)
        {
            rb.linearVelocity = new Vector2(moveSpeed * movingInput, rb.linearVelocity.y);
        }
    }
    private void JumpButton()
    {
        if (!isGrounded)
            bufferJumpCounter = bufferJumpTime;

        if (isWallSliding)
            {
                WallJump();
                canDoubleJump = true;
            }
            else if (isGrounded || cayoteJumpCounter > 0)
            {
                Jump();
            }
            else if (canDoubleJump)
            {
                canMove = true;
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, doubleJumpForce);
                canDoubleJump = false;
            }

        canWallSlide = false;
    }
    private void WallJump()
    {
        canMove = false; 
        //justWallJumped = true;
        rb.linearVelocity = new Vector2(wallJumpDirection.x * -facingDirection, wallJumpDirection.y);
    }

    private void Jump()
    {
        canHaveCayoteJump = false;
        cayoteJumpCounter = -1;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
      
    }

    private void FlipController()
    {
        // if (justWallJumped)
        //     return;

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
        facingDirection = facingDirection * -1;
        facingRight = !facingRight;
        transform.Rotate(0, 180, 0);
    }

    private void KnockBack()
    {
        if (!canBeKnocked)
            return;

        isKnocked = true;
        canBeKnocked = false;
        rb.linearVelocity = new Vector2(knockbackDirection.x, knockbackDirection.y);
        canMove = false;
    }

    private void CollisionChecks()
    {
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, groundLayer);
        isWallDetected = Physics2D.Raycast(transform.position, Vector2.right * facingDirection, wallCheckDistance, whatIsWall);

        canWallSlide = isWallDetected && !isGrounded && rb.linearVelocity.y < 0;

        if(!isWallDetected)
        {
            isWallSliding = false;
            canWallSlide = false;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(groundCheck.position, new Vector3(groundCheck.position.x, groundCheck.position.y - groundCheckDistance));
        Gizmos.DrawLine(wallCheck.position, new Vector3(wallCheck.position.x + wallCheckDistance * facingDirection, wallCheck.position.y));

        Gizmos.DrawWireSphere(enemyCheck.position, enemyCheckRadius);
        Vector2 boxSize = new Vector2(enemyCheckJumpWidth, enemyCheckJumpHeight);
        Gizmos.DrawWireCube(enemyCheckJump.position, boxSize);
    }
}