using UnityEngine;

public class GroundRaycast : MonoBehaviour
{
    [Header("Raycast Settings")]
    [SerializeField] private float rayLength = 1f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Color rayColor = Color.red;

    [Header("Output")]
    [SerializeField] public Vector2 groundHitPosition;
    [SerializeField] public bool seesFloor;

    void Update()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, rayLength, groundLayer);

        if (hit.collider != null)
        {
            seesFloor = true;
            groundHitPosition = hit.point;
        }
        else
        {
            seesFloor = false;
        }
    }

    // debug ray
    void OnDrawGizmos()
    {
        Gizmos.color = rayColor;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * rayLength);
    }
}