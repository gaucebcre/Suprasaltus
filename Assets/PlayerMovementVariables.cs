using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementVariables : ScriptableObject
{
    [Header("Walk")]
    [Range(1f, 100f)] public float MaxWalkSpeed = 12.5f;
    [Range(1f, 100f)] public float GroundAcceleration = 5f;
    [Range(1f, 100f)] public float GroundDeceleration = 20;
    [Range(1f, 100f)] public float AirAcceleration = 5;
    [Range(1f, 100f)] public float AirDeceleration = 5;

    [Header("Jump")]

    [Header("Grounded/Collision Checks")]
    public LayerMask GroundLayerMask;
    public float GroundDetectionRayLength = 0.02f;
    public float headDetectionRayLength = 0.02f;
    [Range(0f, 1f)] public float HeadWidth = 0.75f;
}
