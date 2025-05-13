using System.Collections;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    public PlayerMovementStats playerMoveStats;
    [SerializeField] private Collider2D playerCollider;

    Rigidbody2D rb2d;

    // movement variables
    Vector2 moveVelocity;
    bool isFacingRight;

    // collision variables
    bool isGrounded;
    public ContactFilter2D isGroundedContactFilter;
    bool bumpedHead;
    public ContactFilter2D bumpedHeadContactFilter;

    // jump variables
    public float verticalVelocity { get; private set; }
    bool isJumping;
    bool isFastFalling;
    bool isFalling;
    float fastFallTime;
    float fastFallReleaseSpeed;
    int numberOfJumpsUsed;

    // apex variables
    float apexPoint;
    float timePastApexThreshold;
    bool isPastApexThreshhold;

    // jump buffer variables
    float jumpBufferTimer;
    bool jumpReleaseDuringBuffer;

    // coyote time variables
    float coyoteTimer;

    #region Unity Methods
    void Awake()
    {
        isFacingRight = true;

        rb2d = GetComponent<Rigidbody2D>();

    }

    void Start()
    {

    }

    void Update()
    {
        DoTimers();
        JumpChecks();
    }

    void FixedUpdate()
    {
        CollisionChecks();
        Jump();

        if (isGrounded)
        {
            Move(playerMoveStats.groundAcceleration, playerMoveStats.groundDeceleration, InputManager.movementDirection);
        }
        else // airborne
        {
            Move(playerMoveStats.airAcceleration, playerMoveStats.airDeceleration, InputManager.movementDirection);
        }
    }
    #endregion

    #region Movement Methods
    void Move(float acceleration, float deceleration, Vector2 moveInput)
    {
        if (moveInput != Vector2.zero)
        {
            if (TurnCheck(moveInput.x))
            {
                Turn();
            }

            Vector2 targetVel = Vector2.zero;
            targetVel = new Vector2(moveInput.x, 0f) * playerMoveStats.maxWalkSpeed;

            moveVelocity = Vector2.Lerp(moveVelocity, targetVel, acceleration * Time.fixedDeltaTime);
            rb2d.velocity = new Vector2(moveVelocity.x, rb2d.velocity.y);


        }
        else if (moveInput == Vector2.zero)
        {
            moveVelocity = Vector2.Lerp(moveVelocity, Vector2.zero, deceleration * Time.fixedDeltaTime);
            rb2d.velocity = new Vector2(moveVelocity.x, rb2d.velocity.y);
        }
    }

    bool TurnCheck(float moveInputX)
    {
        if ((isFacingRight && moveInputX < 0) || (!isFacingRight && moveInputX > 0))
        {
            return true;
        }
        return false;
    }

    void Turn() // player must always start facing right
    {
        // https://stackoverflow.com/questions/26568542/flipping-a-2d-sprite-animation-in-unity-2d#26577124 
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }


    #endregion

    #region Jump
    void JumpChecks()
    {
        if (InputManager.jumpPressed)
        {
            jumpBufferTimer = playerMoveStats.jumpBufferTime;
            jumpReleaseDuringBuffer = false;
        }

        if (InputManager.jumpReleased)
        {
            if (jumpBufferTimer > 0f)
            {
                jumpReleaseDuringBuffer = true;
            }
            if (isJumping && verticalVelocity > 0f)
            {
                if (isPastApexThreshhold)
                {
                    isPastApexThreshhold = true;
                    isFastFalling = true;
                    fastFallTime = playerMoveStats.timeForUpwardsCancel;
                    verticalVelocity = 0f;
                }
                else
                {
                    isFastFalling = true;
                    fastFallReleaseSpeed = verticalVelocity;
                }
            }
        }

        // jump with buffering and coyote time
        if (jumpBufferTimer > 0f && !isJumping && (isGrounded || coyoteTimer > 0f))
        {
            InitiateJump(1);

            // grounded jump
            if (jumpReleaseDuringBuffer)
            {
                isFastFalling = true;
                fastFallReleaseSpeed = verticalVelocity;
            }
        }
        else // double jump
        if (jumpBufferTimer > 0f && isJumping && numberOfJumpsUsed < playerMoveStats.numberOfJumpsAllowed)
        {
            isFastFalling = false;
            InitiateJump(1);
        }
        else // air jump after coyote
        if (jumpBufferTimer <= 0f && isFalling && numberOfJumpsUsed < playerMoveStats.numberOfJumpsAllowed)
        {
            isFastFalling = false;
            InitiateJump(2);
        }

        // landing
        if ((isJumping || isFacingRight) && isGrounded && verticalVelocity <= 0f)
        {
            isJumping = false;
            isFalling = false;
            isFastFalling = false;
            fastFallTime = 0f;
            isPastApexThreshhold = false;
            numberOfJumpsUsed = 0;

            verticalVelocity = Physics2D.gravity.y;
        }
    }

    void InitiateJump(int jumps)
    {
        if (!isJumping)
        {
            isJumping = true;
        }
        jumpBufferTimer = 0f;
        numberOfJumpsUsed += jumps;
        verticalVelocity = playerMoveStats.initialJumpVelocity;
    }

    void Jump()
    {
        if (isJumping)
        {
            if (bumpedHead)
            {
                isFastFalling = true;
            }

            // gravity ascending
            if (verticalVelocity >= 0f)
            {
                // apex
                apexPoint = Mathf.InverseLerp(playerMoveStats.initialJumpVelocity, 0f, verticalVelocity);

                if (apexPoint > playerMoveStats.apexThreshhold)
                {
                    if (!isPastApexThreshhold)
                    {
                        isPastApexThreshhold = true;
                        timePastApexThreshold = 0f;
                    }

                    if (isPastApexThreshhold)
                    {
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
                }
                else // gravity ascending before apex
                {
                    verticalVelocity += playerMoveStats.gravity * Time.fixedDeltaTime;
                    if (isPastApexThreshhold)
                    {
                        isPastApexThreshhold = false;
                    }
                }
            }
            else // gravity descending
            if (!isFastFalling)
            { // TODO: test without gravityonreleasemulti
                verticalVelocity += playerMoveStats.gravity * playerMoveStats.gravityOnReleaseMultiplier * Time.fixedDeltaTime;
            }
            else // falling
            if (verticalVelocity < 0f)
            {
                if (!isFastFalling)
                {
                    isFastFalling = true;
                }
            }
        }

        // jump cut
        if (isFastFalling)
        {
            if (fastFallTime <= playerMoveStats.timeForUpwardsCancel)
            {
                verticalVelocity += playerMoveStats.gravity * playerMoveStats.gravityOnReleaseMultiplier * Time.fixedDeltaTime;
            }
            else
            if (fastFallTime < playerMoveStats.timeForUpwardsCancel)
            {
                verticalVelocity = Mathf.Lerp(fastFallReleaseSpeed, 0f, (fastFallTime / playerMoveStats.timeForUpwardsCancel));
            }

            fastFallTime += Time.fixedDeltaTime;
        }

        // normal falling gravity
        if (!isGrounded && !isJumping)
        {
            if (!isFalling)
            {
                isFalling = true;
            }

            verticalVelocity += playerMoveStats.gravity * Time.fixedDeltaTime;
        }

        // clamp fall speed
        verticalVelocity = Mathf.Clamp(verticalVelocity, -playerMoveStats.maxFallSpeed, 50f);

        rb2d.velocity = new Vector2(rb2d.velocity.x, verticalVelocity);
    }

    #endregion

    #region Collision Methods


    void CollisionChecks()
    {
        IsGrounded();
        BumpedHead();
    }

    void BumpedHead()
    {
        bumpedHead = rb2d.IsTouching(bumpedHeadContactFilter);
    }

    void IsGrounded()
    {
        isGrounded = rb2d.IsTouching(isGroundedContactFilter);
    }


    #endregion

    #region Timers

    void DoTimers()
    {
        jumpBufferTimer -= Time.deltaTime;

        if (isGrounded)
        {
            coyoteTimer -= Time.deltaTime;
        }
        else
        {
            coyoteTimer = playerMoveStats.jumpCoyoteTime;
        }
    }

    #endregion

}
