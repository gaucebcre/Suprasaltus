using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    public PlayerMovementStats playerMoveStats;
    [SerializeField] private Collider2D playerCollider;

    private Rigidbody2D rb2d;

    // Movement variables
    private Vector2 moveVelocity;
    private bool isFacingRight = true;

    // Collision variables
    [SerializeField] private bool isGrounded = false;
    [SerializeField] private ContactFilter2D isGroundedContactFilter2D;
    [SerializeField] private bool bumpedHead;
    [SerializeField] private ContactFilter2D bumpedHeadContactFilter2D;

    // Jump variables
    public float verticalVelocity { get; private set; }
    [SerializeField] private bool isJumping;
    [SerializeField] private bool doJumpCut; // jumpCut means: start going down when player release
    [SerializeField] private bool isFalling;
    [SerializeField] private float jumpCutTime; // delay to start going down
    [SerializeField] private float jumpCutReleaseSpeed;
    [SerializeField] private int numberOfJumpsUsed;

    // Apex variables
    private float apexPoint; // even if player didn't release, this will make it go down
    private float timePastApexThreshold;
    private bool isPastApexThreshold;

    // Jump buffer variables
    private float jumpBufferTimer;
    private bool jumpReleaseDuringBuffer;

    // Coyote time variables
    private float coyoteTimer;

    #region Unity Methods
    void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        InitializeContactFilters();
    }

    void Update()
    {
        UpdateTimers();
        HandleJumpInput();
    }

    void FixedUpdate()
    {
        CheckCollisions();
        ProcessJump();
        ProcessMovement();
    }
    #endregion

    #region Initialization
    private void InitializeContactFilters()
    {
        // if default, initialize ContactFilters (they keep resetting, so here)

        if (isGroundedContactFilter2D.Equals(new ContactFilter2D()))
        {
            isGroundedContactFilter2D.SetLayerMask(playerMoveStats.groundLayerMask);
            isGroundedContactFilter2D.SetNormalAngle(45f, 135f);
        }

        if (bumpedHeadContactFilter2D.Equals(new ContactFilter2D()))
        {
            bumpedHeadContactFilter2D.SetLayerMask(playerMoveStats.groundLayerMask);
            bumpedHeadContactFilter2D.SetNormalAngle(225f, 315f);
        }
    }
    #endregion

    #region Movement Methods
    private void ProcessMovement()
    {
        if (isGrounded)
        {
            Move(playerMoveStats.groundAcceleration, playerMoveStats.groundDeceleration, InputManager.movementDirection);
        }
        else // isAirborne
        {
            Move(playerMoveStats.airAcceleration, playerMoveStats.airDeceleration, InputManager.movementDirection);
        }
    }

    private void Move(float acceleration, float deceleration, Vector2 moveInput)
    {
        if (moveInput != Vector2.zero)
        {
            HandleMovementInput(moveInput, acceleration);
        }
        else
        {
            HandleDeceleration(deceleration);
        }
    }

    private void HandleMovementInput(Vector2 moveInput, float acceleration)
    {
        if (ShouldTurn(moveInput.x))
        {
            Turn();
        }

        Vector2 targetVelocity = new Vector2(moveInput.x, 0f) * playerMoveStats.maxWalkSpeed;
        moveVelocity = Vector2.Lerp(moveVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
        rb2d.velocity = new Vector2(moveVelocity.x, rb2d.velocity.y);
    }

    private void HandleDeceleration(float deceleration)
    {
        moveVelocity = Vector2.Lerp(moveVelocity, Vector2.zero, deceleration * Time.fixedDeltaTime);
        rb2d.velocity = new Vector2(moveVelocity.x, rb2d.velocity.y);
    }

    private bool ShouldTurn(float moveInputX)
    {
        return (isFacingRight && moveInputX < 0) || (!isFacingRight && moveInputX > 0);
    }

    private void Turn() // player must always START facing right
    {
        // https://stackoverflow.com/questions/26568542/flipping-a-2d-sprite-animation-in-unity-2d#26577124 
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
    #endregion

    #region Jump methods
    private void HandleJumpInput()
    {
        ProcessJumpPress();
        ProcessJumpRelease();
        ProcessJumpExecution();
        ProcessLanding();
    }

    private void ProcessJumpPress()
    {
        if (InputManager.jumpPressed)
        {
            // from now on, instead of checking for input, we check for bufferTimer
            jumpBufferTimer = playerMoveStats.jumpBufferTime;
            jumpReleaseDuringBuffer = false;
        }
    }

    private void ProcessJumpRelease()
    {
        if (!InputManager.jumpReleased) return;

        bool jumpWasBuffered = jumpBufferTimer > 0f;
        if (jumpWasBuffered)
            jumpReleaseDuringBuffer = true;

        if (CanCutJump())
        {
            ExecuteJumpCut();
        }
    }

    private bool CanCutJump()
    {
        return isJumping && verticalVelocity > 0f;
    }

    private void ExecuteJumpCut()
    {
        if (isPastApexThreshold)
        {
            isPastApexThreshold = false;
            doJumpCut = true;
            jumpCutTime = playerMoveStats.timeForUpwardsCancel;
            verticalVelocity = 0f;
        }
        else
        {
            doJumpCut = true;
            jumpCutReleaseSpeed = verticalVelocity;
        }
    }

    private void ProcessJumpExecution()
    {
        bool jumpWasBuffered = jumpBufferTimer > 0f;

        if (CanGroundJump(jumpWasBuffered))
        {
            InitiateJump(1);
            if (jumpReleaseDuringBuffer)
            {
                doJumpCut = true;
                jumpCutReleaseSpeed = verticalVelocity;
            }
        }
        else if (CanAirJump(jumpWasBuffered))
        {
            doJumpCut = false;
            InitiateJump(2);
            if (jumpReleaseDuringBuffer)
            {
                doJumpCut = true;
                jumpCutReleaseSpeed = verticalVelocity;
            }
        }
    }

    private bool CanGroundJump(bool jumpWasBuffered)
    {
        bool theresCoyoteTimeLeft = coyoteTimer > 0f;
        return jumpWasBuffered && !isJumping && (isGrounded || theresCoyoteTimeLeft);
    }

    private bool CanAirJump(bool jumpWasBuffered)
    {
        bool hasJumps = numberOfJumpsUsed < playerMoveStats.numberOfJumpsAllowed;
        return jumpWasBuffered && isJumping && hasJumps;
    }

    private void ProcessLanding()
    {
        bool isMovingDownOrStopped = verticalVelocity <= 0f;
        if ((isJumping || isFalling) && isGrounded && isMovingDownOrStopped)
        {
            ResetJumpState();
        }
    }

    private void InitiateJump(int jumps)
    {
        if (!isJumping)
            isJumping = true;

        jumpBufferTimer = 0f;
        jumpReleaseDuringBuffer = false;
        numberOfJumpsUsed += jumps;
        verticalVelocity = playerMoveStats.initialJumpVelocity;
    }

    private void ResetJumpState()
    {
        isJumping = false;
        isFalling = false;
        doJumpCut = false;
        jumpCutTime = 0f;
        isPastApexThreshold = false;
        numberOfJumpsUsed = 0;
        verticalVelocity = Physics2D.gravity.y;
    }

    private void ProcessJump()
    {
        if (isJumping)
        {
            ProcessJumpPhysics();
        }

        ProcessJumpCut();
        ProcessFalling();
        ClampFallSpeed();
        ApplyVerticalVelocity();
    }

    private void ProcessJumpPhysics()
    {
        if (bumpedHead)
        {
            doJumpCut = true;
        }

        if (verticalVelocity >= 0f)
        {
            ProcessAscendingJump();
        }
        else if (!doJumpCut)
        {
            ProcessDescendingJump();
        }
        else if (verticalVelocity < 0f && !doJumpCut)
        {
            doJumpCut = true;
        }
    }

    private void ProcessAscendingJump()
    {
        apexPoint = Mathf.InverseLerp(playerMoveStats.initialJumpVelocity, 0f, verticalVelocity);

        if (apexPoint > playerMoveStats.apexThreshhold)
        {
            ProcessApexHang();
        }
        else
        {
            verticalVelocity += playerMoveStats.gravity * Time.fixedDeltaTime;
            if (isPastApexThreshold)
            {
                isPastApexThreshold = false;
            }
        }
    }

    private void ProcessApexHang()
    {
        if (!isPastApexThreshold)
        {
            isPastApexThreshold = true;
            timePastApexThreshold = 0f;
        }

        timePastApexThreshold += Time.fixedDeltaTime;

        if (timePastApexThreshold < playerMoveStats.apexHangTime)
        {
            verticalVelocity = 0f;
        }
        else
        {
            verticalVelocity = -0.01f;
        }
    }

    private void ProcessDescendingJump()
    {
        verticalVelocity += playerMoveStats.gravity * playerMoveStats.gravityOnReleaseMultiplier * Time.fixedDeltaTime;
    }

    private void ProcessJumpCut()
    {
        if (!doJumpCut) return;

        if (jumpCutTime >= playerMoveStats.timeForUpwardsCancel)
        {
            verticalVelocity += playerMoveStats.gravity * playerMoveStats.gravityOnReleaseMultiplier * Time.fixedDeltaTime;
        }
        else
        {
            verticalVelocity = Mathf.Lerp(jumpCutReleaseSpeed, 0f, jumpCutTime / playerMoveStats.timeForUpwardsCancel);
        }

        jumpCutTime += Time.fixedDeltaTime;
    }

    private void ProcessFalling()
    {
        if (!isGrounded && !isJumping)
        {
            if (!isFalling)
            {
                isFalling = true;
            }

            verticalVelocity += playerMoveStats.gravity * playerMoveStats.gravityOnLedgeFall * Time.fixedDeltaTime;
        }
    }

    private void ClampFallSpeed()
    {
        verticalVelocity = Mathf.Clamp(verticalVelocity, -playerMoveStats.maxFallSpeed, 50f);
    }

    private void ApplyVerticalVelocity()
    {
        rb2d.velocity = new Vector2(rb2d.velocity.x, verticalVelocity);
    }
    #endregion

    #region Collision Methods
    private void CheckCollisions()
    {
        CheckGrounded();
        CheckHeadBump();
    }

    private void CheckHeadBump()
    {
        bumpedHead = rb2d.IsTouching(bumpedHeadContactFilter2D);
    }

    private void CheckGrounded()
    {
        isGrounded = rb2d.IsTouching(isGroundedContactFilter2D);
    }
    #endregion

    #region Timers
    private void UpdateTimers()
    {
        UpdateJumpBufferTimer();
        UpdateCoyoteTimer();
    }

    private void UpdateJumpBufferTimer()
    {
        jumpBufferTimer -= Time.deltaTime;
    }

    private void UpdateCoyoteTimer()
    {
        if (isGrounded)
        {
            coyoteTimer = playerMoveStats.jumpCoyoteTime;
        }
        else
        {
            coyoteTimer -= Time.deltaTime;
        }
    }
    #endregion
}