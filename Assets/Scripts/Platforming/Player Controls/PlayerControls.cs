using Prime31;
using UnityEngine;
using System.Collections;

public class PlayerControls : MonoBehaviour
{
    enum PlayerDirection { Right, Left }
    private PlayerDirection direction = PlayerDirection.Right;

    [Header("Gameplay")]
    public float yDeathHeight = -50f;

    [Header("Jump")]
    public float coyoteTime = 0.15f;
    private float groundedCoyoteCounter;
    private float wallCoyoteCounter;
    private bool jumpPressed;

    [Header("Wall Jump")]
    public float wallSlideSpeed = 1.0f;
    public float wallJumpForce = 10f;
    public Vector2 wallJumpDirection = new Vector2(1.5f, 2f);
    public float wallJumpBufferTime = 0.1f;
    private float wallJumpTimeCounter;
    private bool isTouchingWall = false;
    private bool lastCollisionRight = false;

    [Header("Colliders")]
    public BoxCollider2D physicsCollider;
    public Vector2 defaultColliderSize;
    public Vector2 defaultColliderOffset;

    [Header("Audio")]
    public AudioClip jumpSound;
    private AudioSource audioSource;

    [Header("Inputs")]
    // private bool jumpPressed = false;
    private bool jumpReleased = false;
    private bool lockInput = false;

    [Header("Camera Stuff")]
    [SerializeField] private CameraController camera;
    [SerializeField] private float fallShakeMinTime = 2.0f;
    private float fallStartTime = -1;
    [SerializeField] private float fallShakeDuration = 0.2f;
    [SerializeField] private float fallShakeStrength = 0.5f;


    private CharacterController2D controller;
    private SpriteRenderer sprite;
    private Rigidbody2D rb;

    private Vector3 velocity;
    private Vector3 lastVelocity;
    private float normalizedHorizontalSpeed = 0;
    private bool wasGroundedLastFrame = false;
    
    private Animator animator;
    private Vector3 originalScale;
    private PlayerStats stats;



    void Awake()
    {
        controller = GetComponent<CharacterController2D>();
        animator = GetComponentInChildren<Animator>();

        sprite = GetComponent<SpriteRenderer>();


        controller.onControllerCollidedEvent += OnControllerCollider;
        controller.onTriggerEnterEvent += OnTriggerEnterEvent;
        controller.onTriggerExitEvent += OnTriggerExitEvent;

        sprite = GetComponentInChildren<SpriteRenderer>();
        originalScale = sprite.transform.localScale; 

        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponentInChildren<AudioSource>();
    }

    void Start()
    {
        stats = GetComponentInChildren<PlayerStats>();
        if (stats == null)
        {
            Debug.LogWarning("PlayerStats not found on player! Make sure a PlayerStats component is attached.");
        }
    }

    #region Event Listeners

    void OnControllerCollider(RaycastHit2D hit)
    {
        if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Walls"))
        {
            if (Mathf.Abs(hit.normal.x) > 0)
            {
                velocity.x = 0;
            }
        }
    }

    void OnTriggerEnterEvent(Collider2D collision)
    {
        Debug.Log("Trigger enter: " + collision.gameObject.name);
    }

    void OnTriggerExitEvent(Collider2D col)
    {
        Debug.Log("Trigger exit: " + col.gameObject.name);
    }

    #endregion


    void Update()
    {
        if (transform.position.y < yDeathHeight && GameManager.Instance != null) {
            GameManager.Instance.PlayerDeath();
        }

        if (Input.GetButtonDown("Jump"))
        {
            wallJumpTimeCounter = wallJumpBufferTime;
            jumpPressed = true;
        }
        else if (Input.GetButtonUp("Jump"))
        {
            jumpReleased = true;
        }
    }

    void UpdateAnimatorStates()
    {
        if (animator == null) return;

        animator.SetFloat("xVelocity", Mathf.Abs(velocity.x));
        animator.SetFloat("yVelocity", velocity.y);
        animator.SetBool("isJumping", !controller.isGrounded);
        animator.SetBool("isSliding", isTouchingWall && !controller.isGrounded);
    }

    void FixedUpdate()
    {
        if (stats == null) return;

        if (controller.isGrounded)
        {
            velocity.y = 0;
            groundedCoyoteCounter = coyoteTime;
        }
        else
        {
            groundedCoyoteCounter -= Time.fixedDeltaTime;
        }
        wallJumpTimeCounter -= Time.fixedDeltaTime;

        if (velocity.y < 0) {
            velocity.y *= stats.fallSpeedModifier;
            if (fallStartTime == -1) {
                fallStartTime = Time.time;
            }
        }
        else {
            if (fallStartTime > 0 && Time.time - fallStartTime > fallShakeMinTime && camera) {
                camera.StartShake(fallShakeStrength, fallShakeDuration);
            }
            fallStartTime = -1;
        }

        CheckIsTouchingWall();
        HandleHorizontal();
        HandleJump();
        ClampFallSpeed();

        controller.move(velocity * Time.deltaTime);
        velocity = controller.velocity;
        jumpPressed = false;
        jumpReleased = false;
        wasGroundedLastFrame = controller.isGrounded;
        lastVelocity = velocity;
    }

    void LateUpdate()
    {
        UpdateAnimatorStates();
        Vector3 scale = sprite.transform.localScale;
        float desiredSign = (direction == PlayerDirection.Right) ? 1f : -1f;
        float currentAmplitude = Mathf.Abs(scale.x);
        scale.x = currentAmplitude * desiredSign;
        sprite.transform.localScale = scale;
    }




    void HandleHorizontal()
    {
        if (lockInput) return;

        float input = Input.GetAxisRaw("Horizontal");
        bool isMoving = Mathf.Abs(input) > 0.1f;

        if (isMoving)
        {
            direction = input > 0 ? PlayerDirection.Right : PlayerDirection.Left;
            float targetSpeed = input * stats.runSpeed;

            if (controller.isGrounded)
            {
                float acceleration = 5f;
                velocity.x = Mathf.Lerp(velocity.x, targetSpeed, Time.deltaTime * acceleration);
            }
            else
            {
                float airControl = stats.inAirDamping;
                velocity.x = Mathf.Lerp(velocity.x, targetSpeed, Time.deltaTime * airControl);
            }

            // Flip Sprite correctly
            FlipSprite(direction);
        }
        else
        {
            if (controller.isGrounded)
            {
                velocity.x = 0;
            }
            else
            {
                float deceleration = 0.96f;
                velocity.x *= deceleration;
            }
        }
    }


    void FlipSprite(PlayerDirection targetDirection)
    {
        if (targetDirection == PlayerDirection.Right)
            sprite.flipX = false;
        else if (targetDirection == PlayerDirection.Left)
            sprite.flipX = true;
    }


    void HandleJump()
    {
       
        bool isReallyGrounded = controller.isGrounded || Mathf.Abs(velocity.y) < 0.1f;


        if (isReallyGrounded)
        {
            if (jumpPressed && groundedCoyoteCounter > 0)
            {
                Jump();
            }


            if (velocity.y < 0)
                velocity.y += stats.gravity * stats.gravityFallModifier * Time.deltaTime;
            else
                velocity.y += stats.gravity * Time.deltaTime;
            ClampFallSpeed();

            if (jumpReleased && velocity.y > 0)
                velocity.y *= 0.5f;
        
            return; 
        }


        if (jumpPressed && groundedCoyoteCounter > 0)
        {
            Jump();
        }

        if (velocity.y < 0)
            velocity.y += stats.gravity * stats.gravityFallModifier * Time.deltaTime;
        else
            velocity.y += stats.gravity * Time.deltaTime;
        ClampFallSpeed();

        if (jumpReleased && velocity.y > 0)
            velocity.y *= 0.5f;
    }


    void CheckIsTouchingWall()
    {
        if (controller.isGrounded) 
        {
            isTouchingWall = false;
            wallCoyoteCounter = 0;
            return; 
        }

        isTouchingWall = controller.collisionState.left || controller.collisionState.right;

        if (isTouchingWall)
        {
            lastCollisionRight = controller.collisionState.right;
            wallCoyoteCounter = coyoteTime;
        }
        else {
            wallCoyoteCounter -= Time.fixedDeltaTime;
        }
    }


    void WallJump()
    {
        int wallDir = lastCollisionRight ? -1 : 1;

        float jumpHorizontalForce = Mathf.Lerp(velocity.x, wallJumpDirection.x * wallDir * wallJumpForce, 0.5f);

        velocity = new Vector2(jumpHorizontalForce, wallJumpDirection.y * wallJumpForce);

        wallCoyoteCounter = 0;
    }


    public void Jump(bool force = false, AudioClip soundOverride = null)
    {
        if (audioSource != null)
        {
            AudioClip sfx = soundOverride ?? jumpSound;
            if (sfx != null)
            {
                audioSource.PlayOneShot(sfx);
            }
        }

        if (force)
        {
            wallJumpTimeCounter = wallJumpBufferTime;
        }

        velocity.y = Mathf.Sqrt(2f * stats.jumpHeight * -stats.gravity);
        groundedCoyoteCounter = 0;
    }


    private void ClampFallSpeed()
    {
        if (velocity.y < stats.maxFallSpeed)
        {
            velocity.y = stats.maxFallSpeed; //caps out fall speed
        }
    }

    /*
     * Get/Set
     */
    public void SetVelocity(Vector2 newVelocity) {
        velocity = newVelocity;
    }

    public Vector2 GetVelocity() {
        return velocity;
    }
}
