using System.Collections;
using System.Collections.Generic;
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
    public ContactFilter2D contactFilter;
    bool isGrounded;
    bool bumpedHead;

    // jump variables
    public float verticalVelocity { get; private set; }
    bool isJumping;
    bool isFastFalling;
    bool isFalling;
    float fastFallTime;
    float fastFallReleaseSpeed;
    bool numberOfJumpsUsed;

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

        if (jumpBufferTimer > 0f && !isJumping && (isGrounded || coyoteTimer > 0f))
        {
            // CONTINUE 11:35
        }
    }

    void Jump()
    {

    }

    #endregion

    #region Collision Methods
    void CollisionChecks()
    {
        IsGrounded();
    }

    void IsGrounded()
    {
        isGrounded = rb2d.IsTouching(contactFilter);
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
