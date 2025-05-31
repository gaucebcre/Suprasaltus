using UnityEngine;

public class GroundRaycast : MonoBehaviour
{   // TODO: estoy inviertiendo los hinges, a ver si funciona
    [Header("Raycast Settings")]
    [SerializeField] private float rayLength = 1f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Color rayColor = Color.red;

    [Header("Output")]
    [SerializeField] private Vector3 groundHitPosition;
    [SerializeField] private bool isGrounded;

    void Update()
    {
        // Create a ray pointing downward from this object's position
        Ray ray = new Ray(transform.position, Vector3.down);

        if (Physics.Raycast(ray, out RaycastHit hit, rayLength, groundLayer))
        {
            isGrounded = true;
            groundHitPosition = hit.point;
        }
        else
        {
            isGrounded = false;
            groundHitPosition = Vector3.zero;
        }
    }

    // Visualize the ray in the editor
    void OnDrawGizmos()
    {
        Gizmos.color = rayColor;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * rayLength);
    }
}