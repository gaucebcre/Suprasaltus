using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(menuName = "Player Movement Variables")]
public class PlayerMovementStats : ScriptableObject
{
    // This should be only referenced in PlayerMovement.cs, and only once

    [Header("Walk")]
    [Range(1f, 100f)] public float maxWalkSpeed = 12.5f;
    [Range(1f, 100f)] public float groundAcceleration = 5f;
    [Range(1f, 100f)] public float groundDeceleration = 20;
    [Range(1f, 100f)] public float airAcceleration = 5;
    [Range(1f, 100f)] public float airDeceleration = 5;

    [Header("Grounded/Collision Checks")]
    public LayerMask groundLayerMask;

    [Header("Jump")]
    public float jumpHeight = 6.5f;
    public float minimumJumpHeight = 2f;
    [Range(1f, 1.1f)] public float jumpHeightCompensationFactor = 1.05f;
    public float timeTillJumpApex = 0.4f;
    [Range(0.01f, 5f)] public float gravityOnReleaseMultiplier = 2f;
    public float maxFallSpeed = 26f;
    [Range(1f, 5f)] public int numberOfJumpsAllowed = 2;

    [Header("Jump Cut")]
    [Range(0.02f, 0.3f)] public float timeForUpwardsCancel = 0.027f;

    [Header("Jump Apex")]
    [Range(0.5f, 1f)] public float apexThreshhold = 0.97f;
    [Range(0.01f, 0.1f)] public float apexHangTime = 0.075f;

    [Header("Jump Buffer")]
    [Range(0f, 1f)] public float jumpBufferTime = 0.125f;

    [Header("Jump Coyote Time")]
    [Range(0f, 1f)] public float jumpCoyoteTime = 0.125f;

    [Header("JumpVizualization Tool")]
    public bool showWalkJumpArc = false;
    public bool stopOnCollision = false;
    public bool drawRight = false;
    [Range(5f, 100f)] public float arcResolution = 10f;
    [Range(0f, 500)] public float visualizationSteps = 90;

    public float gravity { get; private set; }
    public float initialJumpVelocity { get; private set; }
    public float adjustedJumpHeight { get; private set; }

    void OnValidate()
    {
        CalculateValues();
    }

    void OnEnable()
    {
        CalculateValues();
    }

    void CalculateValues()
    {
        adjustedJumpHeight = jumpHeight * jumpHeightCompensationFactor;

        // formulae of https://www.youtube.com/watch?v=hG9SzQxaCm8
        gravity = -(2f * adjustedJumpHeight) / Mathf.Pow(timeTillJumpApex, 2f);
        initialJumpVelocity = Mathf.Abs(gravity) * timeTillJumpApex;
    }
}
