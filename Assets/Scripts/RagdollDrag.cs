using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RagdollDrag : MonoBehaviour
{
    [Header("Slingshot Settings")]
    public Transform slingshotAnchor;   // Where the projectile is held before release
    public float maxStretch = 5f;       // Max distance you can pull
    public float launchForceMultiplier = 50f;

    private Rigidbody rb;
    private bool isDragging = false;
    private Vector3 dragStartPos;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true; // Hold still until launched
    }

    void OnMouseDown()
    {
        isDragging = true;
        dragStartPos = slingshotAnchor.position;
    }

    void OnMouseDrag()
    {
        if (!isDragging) return;

        // Convert mouse position to world point
        Vector3 mouseWorldPoint = GetMouseWorldPosition();

        // Calculate pull direction & clamp stretch
        Vector3 pullVector = mouseWorldPoint - dragStartPos;
        if (pullVector.magnitude > maxStretch)
        {
            pullVector = pullVector.normalized * maxStretch;
        }

        // Move projectile to dragged position
        transform.position = dragStartPos + pullVector;
    }

    void OnMouseUp()
    {
        if (!isDragging) return;

        isDragging = false;
        rb.isKinematic = false;

        // Launch projectile (opposite of pull direction)
        Vector3 launchDirection = (dragStartPos - transform.position);
        rb.AddForce(launchDirection * launchForceMultiplier, ForceMode.Impulse);
    }

    Vector3 GetMouseWorldPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.up, Vector3.zero); // ground plane (Y = 0)
        float distance;
        if (plane.Raycast(ray, out distance))
        {
            return ray.GetPoint(distance);
        }
        return Vector3.zero;
    }
}
