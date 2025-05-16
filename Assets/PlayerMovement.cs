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
    [SerializeField] bool isGrounded = false;
    [SerializeField] ContactFilter2D isGroundedContactFilter;
    [SerializeField] bool bumpedHead;
    [SerializeField] ContactFilter2D bumpedHeadContactFilter;

    // jump variables
    public float verticalVelocity { get; private set; }
    [SerializeField] bool isJumping;
    [SerializeField] float jumpTime;
    [SerializeField] bool doJumpCut;
    [SerializeField] bool isFalling;
    [SerializeField] float jumpCutTime;
    [SerializeField] float jumpCutReleaseSpeed;
    [SerializeField] int numberOfJumpsUsed;

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
        // if default, initialize ContactFilters (they keep resetting, so here)
        if (isGroundedContactFilter.Equals(new ContactFilter2D()))
        {
            isGroundedContactFilter.SetLayerMask(playerMoveStats.groundLayerMask);
            isGroundedContactFilter.SetNormalAngle(45f, 135f);
        }
        if (bumpedHeadContactFilter.Equals(new ContactFilter2D()))
        {
            bumpedHeadContactFilter.SetLayerMask(playerMoveStats.groundLayerMask);
            bumpedHeadContactFilter.SetNormalAngle(225f, 315f);
        }
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

        // from now on, instead of checking for input, we check for bufferTimer
        bool jumpWasBuffered = jumpBufferTimer > 0f ? true : false;

        if (InputManager.jumpReleased)
        {
            if (jumpWasBuffered)
                jumpReleaseDuringBuffer = true;

            bool isMovingUp = verticalVelocity > 0f;
            if (isJumping && isMovingUp) //playerMoveStats.minJumpCutVelocity
            {
                if (isPastApexThreshhold)
                {
                    isPastApexThreshhold = false;
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
        }

        // grounded jump
        bool theresCoyoteTimeLeft = coyoteTimer > 0f;
        bool hasJumps = numberOfJumpsUsed < playerMoveStats.numberOfJumpsAllowed;
        if (jumpWasBuffered && !isJumping && (isGrounded || theresCoyoteTimeLeft))
        {
            InitiateJump(1);

            if (jumpReleaseDuringBuffer)
            {
                doJumpCut = true; // trigger jump cut
                jumpCutReleaseSpeed = verticalVelocity;
            }
        }
        else // air jump
        if (jumpWasBuffered && isJumping && hasJumps)
        {
            doJumpCut = false;
            InitiateJump(2);

            if (jumpReleaseDuringBuffer) // needed to tap air jump
            {
                doJumpCut = true; // trigger jump cut
                jumpCutReleaseSpeed = verticalVelocity;
            }
        }

        // landing
        bool isMovingDownOrStopped = verticalVelocity <= 0f;
        if ((isJumping || isFalling) && isGrounded && isMovingDownOrStopped)
        {
            isJumping = false;
            isFalling = false;
            doJumpCut = false;
            jumpCutTime = 0f;
            isPastApexThreshhold = false;
            numberOfJumpsUsed = 0;

            verticalVelocity = Physics2D.gravity.y;
        }
    }

    void InitiateJump(int jumps)
    {
        if (!isJumping)
            isJumping = true;
        jumpBufferTimer = 0f;
        jumpTime = 0f;
        jumpReleaseDuringBuffer = false; // to control air jump height
        numberOfJumpsUsed += jumps;
        verticalVelocity = playerMoveStats.initialJumpVelocity;
    }

    void Jump()
    {
        if (isJumping)
        {
            if (bumpedHead)
            {
                doJumpCut = true;
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
            if (!doJumpCut)
            { // TODO: test without gravityonreleasemulti
                verticalVelocity += playerMoveStats.gravity * playerMoveStats.gravityOnReleaseMultiplier * Time.fixedDeltaTime;
            }
            else // falling
            if (verticalVelocity < 0f)
            {
                if (!doJumpCut)
                {
                    doJumpCut = true;
                }
            }
        }

        // jump cut (will wait until the minimum height)
        bool reachedMinimumJumpHeight = jumpTime > playerMoveStats.timeTillJumpApex * playerMoveStats.minJumpCutPercent;
        if (doJumpCut)
        {
            if (jumpCutTime >= playerMoveStats.timeForUpwardsCancel)
            {
                verticalVelocity += playerMoveStats.gravity * playerMoveStats.gravityOnReleaseMultiplier * Time.fixedDeltaTime;
            }
            else
            if (jumpCutTime < playerMoveStats.timeForUpwardsCancel)
            {
                verticalVelocity = Mathf.Lerp(jumpCutReleaseSpeed, 0f, (jumpCutTime / playerMoveStats.timeForUpwardsCancel));
            }

            jumpCutTime += Time.fixedDeltaTime;
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

        if (isJumping)
        {
            jumpTime += Time.fixedDeltaTime;
        }
    }

    #endregion

}
