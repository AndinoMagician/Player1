using System.Collections;
using System;
using UnityEditor.Tilemaps;
using UnityEngine;

public class Player : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator anim;


    [Header("Particles")]
    [SerializeField] private ParticleSystem dustFX;
    [SerializeField] private GameObject landing;
    [SerializeField] private Transform landingFX;

    //[SerializeField] private GameObject jumpHitFX;
    private float dustFxTimer;

    [Header("Move info")]
    public float moveSpeed;
    public float jumpForce;
    //public float doubleJumpForce;
    
    public Vector2 wallJumpDirection;
    public float maxfallSpeed;
    [SerializeField] private float bufferJumpTime;
    private float bufferJumpCounter;
    [SerializeField] private float cayoteJumpTime;
    private float cayoteJumpCounter;
    private bool canHaveCayoteJump;
    private float flipDelayTimer;

    [Header("Glide info")]
    [SerializeField] private float glidingSpeed;
    public float initalGravityScale;
    private float glideExitBuffer = 0.1f;
    private float glideExitTimer;
    private bool canStartGlide;

    [Header("Attack info")]
    [SerializeField] private float groundPoundForce;
    private GameObject attackArea = default;
    private bool isAttacking;
    private bool isGroundPounding;
    private bool attacking = false;
    private bool attackStarted = false;
    private float timeToAttack = 0.25f;
    private float timer = 0f;

    [Header("Dash info")]
    [SerializeField] private float dashSpeed = 20f;       
    [SerializeField] private float dashDuration = 0.2f;   
    [SerializeField] private float dashCooldown = 0.5f;   
    private bool isDashing;
    private float dashTimer;
    private float dashCooldownTimer;
    [Header("Jump control")]
    [SerializeField] private float jumpCutMultiplier = 0.5f;

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
    [SerializeField] private float knockbackForceX = 8f;
    [SerializeField] private float knockbackForceY = 5f;
    [SerializeField] private float knockbackTime = 0.2f;
    [SerializeField] private float knockbackProtectionTime = 1f;

    private bool isKnocked;
    private bool canBeKnocked = true;


    //private float hInput;
    
    private float movingInput;
    //private bool canDoubleJump = true;
    private bool canMove;
    private bool readyToLand;
    public bool isGrounded;
    private bool isWallDetected;
    private bool canWallSlide;
    private bool isWallSliding; 
    public bool isGliding;
    public bool isGlideButtonHeld;
    private bool justWallJumped;
    private bool isLandingOnEnemy;
    private bool canJumpOnEnemy = true;
    private bool facingRight = true;
    private int facingDirection = 1;
    private ScreenShake screenShake;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        attackArea = transform.GetChild(0).gameObject;
        screenShake = Camera.main.GetComponent<ScreenShake>();
        
    }

    void Update()
    {
        AnimationsConrollers();
        CollisionChecks();
        FlipController();
        InputChecks();

        CheckForEnemyJump();
        CheckForEnemy();

        // Variable jump height control
        // if (Input.GetButtonUp("Jump") && rb.linearVelocity.y > 0 && !isGliding)
        // {
        //     rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
        // }

        canMove = true;

        //Counters 
        bufferJumpCounter -= Time.deltaTime;
        cayoteJumpCounter -= Time.deltaTime;

        if (dashCooldownTimer > 0) dashCooldownTimer -= Time.deltaTime;

        //Dash
        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0f)
            {
                isDashing = false;
                canMove = true;
            }
        }

        if (!isKnocked && canMove && !isDashing)
        {
            Move();
        }

        if (!isDashing && rb.gravityScale == 0) rb.gravityScale = initalGravityScale;

        //Ground Movement
        if (isGrounded)
        {
            //canDoubleJump = true;
            canMove = true;
            isGliding = false;
            rb.gravityScale = initalGravityScale;

            if (bufferJumpCounter > 0)
            {
                bufferJumpCounter = -1;
                Jump();
            }

            canHaveCayoteJump = true;
             
             //landing effect
            if(readyToLand)
            {
                Landing();
                readyToLand = false;
            }
        }
        else
        {
            if (canHaveCayoteJump)
            {
                canHaveCayoteJump = false;
                cayoteJumpCounter = cayoteJumpTime;
            }
             
            if(!readyToLand)
            {
                readyToLand = true;
            }
        }
        //WallSlide
        if (canWallSlide)
        {
            isWallSliding = true;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.1f);
        }
        else
        {
            isWallSliding = false;
        }
        
        //Attacking
        if (attacking)
        {
            timer += Time.deltaTime;

            if (timer >= timeToAttack)
            {
                timer = 0;
                attacking = false;
                // isCrunch = false;
                attackStarted = false;
                attackArea.SetActive(false);
            }


        }

        //DownAttack
        if (isGrounded && isGroundPounding)
        {
            isGroundPounding = false;
            isAttacking = false;
            anim.ResetTrigger("DownAttack");
        }

        // Reset Attacks
        if (isAttacking && anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
        {
            isAttacking = false;
        }
        HandleAttack();
        HandleGlide();
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
        anim.SetBool("isGliding", isGliding);
        anim.SetBool("isKnocked", isKnocked);
    }
    private void InputChecks()
    {
        //Move
        movingInput = Input.GetAxisRaw("Horizontal");

        if (Input.GetAxis("Vertical") < 0)
            canWallSlide = false;

        // Jump pressed
        if (Input.GetButtonDown("Jump"))
        {
            // If grounded or within coyote time — normal jump
            if (isGrounded || isWallSliding || cayoteJumpCounter > 0)
            {
                JumpButton();
                canStartGlide = false; // reset glide ability until jump is released
            }
            else if (!isGrounded && canStartGlide && rb.linearVelocity.y < 0 && !isWallSliding)
            {
                // Pressing jump again midair (while falling) starts gliding
                isGlideButtonHeld = true;
            }
        }

        // Jump released
        if (Input.GetButtonUp("Jump"))
        {
            // Allow glide on next press once released after a jump
            canStartGlide = true;

            // Stop gliding if currently gliding
            isGlideButtonHeld = false;
            if (isGliding)
            {
                isGliding = false;
                rb.gravityScale = initalGravityScale;
            }

            // Variable jump height control
            if (rb.linearVelocity.y > 0 && !isGliding)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
            }
        }


        // //Jump
        // if (Input.GetButtonDown("Jump"))
        // {
        //     JumpButton();
        // }


        //  // When jump is held midair, start gliding
        // if (Input.GetButton("Jump") && !isGrounded && rb.linearVelocity.y < -0.1f && !isWallSliding)
        // {
        //     isGlideButtonHeld = true;
        // }
        // else
        // {
        //     isGlideButtonHeld = false;
        // }
  
        if (Input.GetKeyDown(KeyCode.L) && dashCooldownTimer <= 0f && !isDashing)
        {
            StartDash();
        }

    }
    private void HandleGlide()
    {
    
        // if (!isGrounded && !isWallSliding && !isWallDetected && isGlideButtonHeld && rb.linearVelocity.y < -0.1f)
        // {
        //     if (!isGliding) 
        //     {
        //         isGliding = true;
        //         rb.gravityScale = 0;
        //     }
        // rb.linearVelocity = new Vector2(rb.linearVelocity.x, -glidingSpeed);
        // }
        // else
        // {
        //     if (isGliding) 
        //     {
        //         isGliding = false;
        //         rb.gravityScale = initalGravityScale;
        //     }
        // }
        if (isGlideButtonHeld)
        {
            if (!isGliding)
            {
                isGliding = true;
                rb.gravityScale = 0;
            }
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, -glidingSpeed);
            }
            else if (isGliding)
            {
                isGliding = false;
                rb.gravityScale = initalGravityScale;
            }
    }

    private void HandleAttack()
    {
    if (Input.GetKeyDown(KeyCode.K))
    {
            //Up Attack
            if (Input.GetKey(KeyCode.W))
            {
                anim.SetTrigger("UpAttack");
                OnAttack();
            }
            else if (Input.GetKey(KeyCode.S) && !isGrounded) // Down attack only in air
            {
                anim.SetTrigger("DownAttack");
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, groundPoundForce);
                OnAttack();
            }
            else // Straight attack
            {
                anim.SetTrigger("Attack");
                OnAttack();
            }
        }
    }

    private void StartDash()
    {
        anim.SetTrigger("isDashing");
        isDashing = true;
        dashTimer = dashDuration;
        dashCooldownTimer = dashCooldown;

        canMove = false;
        rb.gravityScale = 0;
        rb.linearVelocity = new Vector2(facingDirection * dashSpeed, 0f);

        //ScreenShake
        if (screenShake != null)
        StartCoroutine(screenShake.Shake(0.05f, 0.05f));

    }

    private void Move()
    {
        if (!isKnocked && canMove && !isDashing)
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
                
            }
            else if (isGrounded || cayoteJumpCounter > 0)
            {
                Jump();
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
    private void Landing()
    {
        GameObject newLandingFX = Instantiate(landing, landingFX.position, transform.rotation);
        Destroy(newLandingFX, .7f);
    }

    private void FlipController()
    {
        //Timer for dust particles
        dustFxTimer -= Time.deltaTime;

        // if (justWallJumped)
        //     return;

        if (isKnocked) return; 

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

        //Play dust particles
        if(isGrounded && dustFxTimer < 0)
        {
            dustFX.Play();
            dustFxTimer = .3f;
        }
    }

    public void KnockBack(Transform trap)
    {
        if (!canBeKnocked) return;

        canBeKnocked = false;
        isKnocked = true;
        canMove = false;
        if (screenShake != null)
        StartCoroutine(screenShake.Shake(0.1f, 0.15f));

        // Determine horizontal direction: +1 if player is left of trap, -1 if right
        float direction = (transform.position.x < trap.position.x) ? -1f : 1f;

        // Apply velocity
        rb.linearVelocity = new Vector2(direction * knockbackForceX, knockbackForceY);

        // Start the recovery coroutine
        StartCoroutine(KnockbackRoutine());
    }

    private IEnumerator KnockbackRoutine()
    {
        yield return new WaitForSeconds(knockbackTime);
        isKnocked = false;
        canMove = true;
        

        // Extra protection time before next knockback
        yield return new WaitForSeconds(knockbackProtectionTime);
        canBeKnocked = true;

    }
    public void CheckForEnemyJump()
        {
            if (rb.linearVelocity.y < 0 || isGliding)
            {
            //size of the box
            Vector2 boxSize = new Vector2(enemyCheckJumpWidth, enemyCheckJumpHeight);

            //box check
            Collider2D[] hitColliders = Physics2D.OverlapBoxAll(enemyCheckJump.position, boxSize, 0f);

            foreach (var collider in hitColliders)
            {
            //Check Enemy 
            if (collider.GetComponent<Enemy>() != null)
                {
                    Enemy newEnemy = collider.GetComponent<Enemy>();

                    if (isGliding)
                    {
                        isGliding = false;
                    }
                    
                    GameObject newLandingFX = Instantiate(landing, landingFX.position, transform.rotation);
                    Destroy(newLandingFX, .7f);

                    if (screenShake != null)
                        StartCoroutine(screenShake.Shake(0.05f, 0.05f));
                    
                    Jump();

                    canBeKnocked = false;

                    //return Knockback
                    StartCoroutine(KnockbackRoutine());

                }
            }
        }
    }

    private void CheckForEnemy()
 {
    // Perform circle overlap check
    Collider2D[] hitColliders = Physics2D.OverlapCircleAll(enemyCheck.position, enemyCheckRadius);

    foreach (var hit in hitColliders)
    {
        if (hit.CompareTag("Enemy") && attacking)
        {
            Enemy enemyScript = hit.GetComponent<Enemy>();
                if (enemyScript != null)
                {
                     if (screenShake != null)
                    StartCoroutine(screenShake.Shake(0.1f, 0.15f));
                    enemyScript.DestroyEnemy();
                }
        }
    }
}

    public void OnAttack()
    {
        if(!attackStarted && !isWallDetected)
        {
            attackArea.SetActive(true);

            if (isGrounded)
            {
              attacking = true;
                    attackStarted = true;
                    timer = 0f;
                    CheckForEnemy();
            }     

            if (isWallDetected)
            {   
                attacking = false;
                attackArea.SetActive(false);          
            }
        
        }
    }

    private void CollisionChecks()
    {
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, groundLayer);
        isWallDetected = Physics2D.Raycast(transform.position, Vector2.right * facingDirection, wallCheckDistance, whatIsWall);

        canWallSlide = isWallDetected && !isGrounded && rb.linearVelocity.y < 0;

        if (!isWallDetected)
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