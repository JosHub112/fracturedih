using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class RagdollDrag : MonoBehaviour
{
    [Header("Slingshot Settings")]
    public Transform slingshotAnchor;   // fallback: this.transform
    public float maxStretch = 5f;
    public float launchForceMultiplier = 50f;

    private Rigidbody rb;
    private bool isDragging = false;
    private Vector3 dragStartPos;
    private float planeHeight;
    private Camera mainCam;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("RagdollDrag: geen Rigidbody gevonden!");
            enabled = false;
            return;
        }

        if (GetComponent<Collider>() == null)
        {
            Debug.LogError("RagdollDrag: geen Collider gevonden! OnMouse events werken alleen met een Collider.");
            enabled = false;
            return;
        }

        if (slingshotAnchor == null) slingshotAnchor = transform;

        rb.isKinematic = true;
        mainCam = Camera.main;
        if (mainCam == null)
        {
            Debug.LogWarning("RagdollDrag: Camera.main is null. Zorg dat je camera de tag 'MainCamera' heeft.");
        }
    }

    void Update()
    {
        // Deze helper-methodes bestaan en worden aangeroepen -> voorkomt 'does not exist' errors
        HandleMouseInput();
        HandleTouchInput();
    }

    // --- Input helpers (zorg dat deze namen bestaan!) ---
    void HandleMouseInput()
    {
        if (mainCam == null) mainCam = Camera.main;

        if (Input.GetMouseButtonDown(0))
        {
            // optioneel: controleer of we op dit object klikken (raycast)
            Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider != null && hit.collider.gameObject == gameObject)
                {
                    BeginDrag();
                }
            }
        }
        else if (Input.GetMouseButton(0) && isDragging)
        {
            Vector3 world = GetMouseWorldPositionOnPlane(Input.mousePosition, planeHeight);
            ContinueDrag(world);
        }
        else if (Input.GetMouseButtonUp(0) && isDragging)
        {
            EndDragAndLaunch();
        }
    }

    void HandleTouchInput()
    {
        if (Input.touchCount == 0 || mainCam == null) return;

        Touch t = Input.GetTouch(0);
        if (t.phase == TouchPhase.Began)
        {
            Ray ray = mainCam.ScreenPointToRay(t.position);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider != null && hit.collider.gameObject == gameObject)
                {
                    BeginDrag();
                }
            }
        }
        else if ((t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary) && isDragging)
        {
            Vector3 world = GetMouseWorldPositionOnPlane(t.position, planeHeight);
            ContinueDrag(world);
        }
        else if ((t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled) && isDragging)
        {
            EndDragAndLaunch();
        }
    }

    // --- Drag lifecycle ---
    void BeginDrag()
    {
        isDragging = true;
        dragStartPos = slingshotAnchor.position;
        planeHeight = dragStartPos.y;
        rb.isKinematic = true;
        Debug.Log("RagdollDrag: begin drag");
    }

    void ContinueDrag(Vector3 worldPoint)
    {
        Vector3 pullVector = worldPoint - dragStartPos;
        pullVector.y = 0f; // houd op anchor-level (optioneel)
        if (pullVector.magnitude > maxStretch)
            pullVector = pullVector.normalized * maxStretch;

        Vector3 target = dragStartPos + pullVector;
        transform.position = target;
    }

    void EndDragAndLaunch()
    {
        isDragging = false;
        rb.isKinematic = false;

        Vector3 launchDirection = (dragStartPos - transform.position);
        if (launchDirection.sqrMagnitude < 0.0001f)
        {
            Debug.Log("RagdollDrag: weinig stretch, geen launch.");
            return;
        }

        Vector3 impulse = launchDirection * launchForceMultiplier;
        // reset velocity voor consistente resultaten
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.AddForce(impulse, ForceMode.Impulse);
        Debug.Log($"RagdollDrag: Gelanceerd, impulse: {impulse}");
    }

    // --- Hulpfunctie ---
    Vector3 GetMouseWorldPositionOnPlane(Vector3 screenPos, float y)
    {
        if (mainCam == null) mainCam = Camera.main;
        if (mainCam == null) return Vector3.zero;

        Ray ray = mainCam.ScreenPointToRay(screenPos);
        Plane plane = new Plane(Vector3.up, new Vector3(0f, y, 0f));
        if (plane.Raycast(ray, out float enter))
        {
            return ray.GetPoint(enter);
        }
        return Vector3.zero;
    }
}
