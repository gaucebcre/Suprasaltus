using UnityEngine;

public class FootMovement1 : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GroundRaycast backRaycast;
    [SerializeField] private GroundRaycast frontRaycast;

    [Header("Configuración")]
    [SerializeField] private float distanceThreshold = 0.5f;
    [SerializeField] private float stepDuration = 0.3f;
    [SerializeField] private float stepHeight = 0.2f;
    [SerializeField] private AnimationCurve stepCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    // Variables privadas
    [Header("Debug")]

    [SerializeField] private Vector2 targetPosition;
    [SerializeField] private bool isMoving = false;
    [SerializeField] private float moveProgress = 0f;
    [SerializeField] private Vector2 startPosition;

    void Update()
    {
        if (isMoving)
        {
            PerformStep();
        }
        else // notMoving
        {
            CheckForStep();
        }
    }

    void CheckForStep()
    {
        // both must touch ground (TODO need to improve this but no time)
        if (!backRaycast.seesFloor || !frontRaycast.seesFloor) return;

        float distanceToBackRay = Vector3.Distance(transform.position, backRaycast.groundHitPosition);
        if (distanceToBackRay < distanceThreshold)
        {
            StartStep(frontRaycast.groundHitPosition);
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
            // Complete step
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