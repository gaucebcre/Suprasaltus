using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [Header("Follow Target")]
    public Transform target;

    [Header("Horizontal Movement")]
    [SerializeField] private bool followX = true;
    [SerializeField][Range(0.1f, 1f)] private float xSmoothTime = 0.3f;
    [SerializeField] private float xOffset = 0f;
    private float xVelocity = 0f; // need 2 velocity variables, they actually changue at runtime

    [Header("Vertical Movement")]
    [SerializeField] private bool followY = true;
    [SerializeField][Range(0.1f, 1f)] private float ySmoothTime = 0.3f;
    [SerializeField] private float yOffset = 0f;
    private float yVelocity = 0f;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 newPosition = transform.position;

        if (followX)
        {
            newPosition.x = Mathf.SmoothDamp(
                transform.position.x,
                target.position.x + xOffset,
                ref xVelocity,
                xSmoothTime
            );
        }

        if (followY)
        {
            newPosition.y = Mathf.SmoothDamp(
                transform.position.y,
                target.position.y + yOffset,
                ref yVelocity,
                ySmoothTime
            );
        }

        transform.position = newPosition;
    }
}