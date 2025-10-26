using UnityEngine;

public class Enemy : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;
    [SerializeField] private ParticleSystem deathFX;
    [SerializeField] Transform deathFXposition; 

    [Header("Movement Settings")]
    public float moveSpeed = 2f;
    public float moveTime = 2f;
    public float idleTime = 1f;

    private int facingDirection = -1;
    private float moveTimer;
    private float idleTimer;
    private bool isMoving = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        moveTimer = moveTime;
    }

    void Update()
    {
        // timers & state switching
        if (isMoving)
        {
            moveTimer -= Time.deltaTime;
            if (moveTimer <= 0f)
                StartIdle();
        }
        else
        {
            idleTimer -= Time.deltaTime;
            if (idleTimer <= 0f)
                StartMoveAndFlip();
        }

        // update animation
        anim.SetFloat("xVelocity", Mathf.Abs(rb.linearVelocity.x));

        // make sure sprite faces correct direction
        sr.flipX = (facingDirection == 1);
    }

    void FixedUpdate()
    {
        if (isMoving)
            rb.linearVelocity = new Vector2(moveSpeed * facingDirection, rb.linearVelocity.y);
        else
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

    private void StartIdle()
    {
        isMoving = false;
        idleTimer = idleTime;
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

    private void StartMoveAndFlip()
    {
        // flip before moving so visuals are correct when movement starts
        facingDirection *= -1;
        isMoving = true;
        moveTimer = moveTime;
    }

    public void DestroyEnemy()
    {
        AudioManager.instance.PlaySFX(6);
        ParticleSystem newDeathFX = Instantiate(deathFX, deathFXposition.position, transform.rotation);
        Destroy(newDeathFX, 3f);
        
        Destroy(gameObject);
    }
}
