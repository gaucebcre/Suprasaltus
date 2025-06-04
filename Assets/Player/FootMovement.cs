using UnityEngine;

public class FootMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GroundRaycast backRaycast;
    [SerializeField] private GroundRaycast frontRaycast;
    [SerializeField] private Transform playerCenter;

    [Header("Configuration")]
    [SerializeField] private float stepDuration = 0.3f;
    [SerializeField] private float stepHeight = 0.2f;
    [SerializeField] private AnimationCurve stepCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    // Private variables 
    private Vector2 targetPosition;
    private bool isMoving = false;
    private float moveProgress = 0f;
    private Vector2 startPosition;

    void Update()
    {
        transform.localScale = playerCenter.localScale; // flip feet too

        if (isMoving)
        {
            PerformStep();
        }
        else // not moving
        {
            CheckForStep();
        }
    }

    void CheckForStep()
    {
        if (!backRaycast.seesFloor || !frontRaycast.seesFloor) return;

        // Check if the foot is between both raycasts on the X axis
        float footX = transform.position.x;
        float backRayX = backRaycast.groundHitPosition.x;
        float frontRayX = frontRaycast.groundHitPosition.x;

        float bound1 = Mathf.Min(backRayX, frontRayX);
        float bound2 = Mathf.Max(backRayX, frontRayX);

        if (footX < bound1 || footX > bound2)
        {
            // Move to the furthest raycast
            float distanceToBack = Mathf.Abs(footX - backRayX);
            float distanceToFront = Mathf.Abs(footX - frontRayX);

            if (distanceToBack > distanceToFront)
            {
                StartStep(backRaycast.groundHitPosition);
            }
            else
            {
                StartStep(frontRaycast.groundHitPosition);
            }
        }
    }

    void StartStep(Vector3 newTargetPosition)
    {
        startPosition = transform.position;
        targetPosition = newTargetPosition;
        isMoving = true;
        moveProgress = 0f;
    }

    void PerformStep()
    {
        moveProgress += Time.deltaTime / stepDuration;

        if (moveProgress >= 1f)
        {
            transform.position = targetPosition;
            isMoving = false;
            moveProgress = 0f;
        }
        else
        {
            // Calculate interpolation
            float curveValue = stepCurve.Evaluate(moveProgress);
            Vector3 horizontalPosition = Vector3.Lerp(startPosition, targetPosition, curveValue);

            // Calculate arc
            float heightMultiplier = Mathf.Sin(moveProgress * Mathf.PI);
            Vector3 finalPosition = horizontalPosition + Vector3.up * (stepHeight * heightMultiplier);

            transform.position = finalPosition;
        }
    }
}
