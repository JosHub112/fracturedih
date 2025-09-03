using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(AudioSource))]
public class PlayerControls: MonoBehaviour
{
    [Header("Slingshot Settings")]
    [SerializeField] private float maxStretch = 5f;
    [SerializeField] private float launchForceMultiplier = 10f;
    [SerializeField] private float maxLaunchForce = 50f;
    [SerializeField] private float stopThreshold = 0.05f;
    [SerializeField] private float stopAngularThreshold = 0.1f;
    [SerializeField] private float restDuration = 0.15f;
    [SerializeField] private int lives = 3;
    [SerializeField] private float speed = 1f;
    [SerializeField] private float airDrag = 0.5f;
    [SerializeField] private float currentSpeed;

    [Header("Throw Settings")]
    [SerializeField] private float height = 2f;

    [Header("Death Settings")]
    [SerializeField] private float deathLaunchForce = 20f;
    [SerializeField] private float destroyDelay = 2f;

    [Header("Audio")]
    [SerializeField] private AudioClip launchClip;
    [SerializeField] private AudioClip deathClip;
    [SerializeField][Range(0f, 1f)] private float launchVolume = 1f;
    [SerializeField][Range(0f, 1f)] private float deathVolume = 1f;

    private Camera mainCam;
    private Rigidbody rb;
    private Collider col;
    private Vector3 slingshotAnchor;
    private Vector3 dragStartPos;
    private bool isDragging;
    private bool canSlingshot = true;
    private float restTimer;
    private float originalDrag;
    private AudioSource audioSource;
    private bool isDead = false;

    public float Speed
    {
        get => speed;
        set => speed = Mathf.Max(0f, value);
    }

    public float CurrentSpeed => currentSpeed;

    public int Lives
    {
        get => lives;
        set
        {
            lives = Mathf.Max(0, value);

            // Update UI via GameManager if available
            GameManager.Instance?.UpdateLivesUI(lives);

            if (lives == 0 && !isDead)
                Die();
        }
    }

    void Start()
    {
        mainCam = Camera.main;
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        slingshotAnchor = transform.position;

        // keep original damping if used in your project (you used linearDamping previously)
        originalDrag = rb.linearDamping;
        rb.isKinematic = true;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        if (mainCam == null)
            Debug.LogWarning("WormSlingshot3D: Camera.main is null. Make sure the camera has tag 'MainCamera'.");

        // Ensure UI shows current lives at start
        GameManager.Instance?.UpdateLivesUI(lives);
    }

    void Update()
    {
        if (isDead) return;

        if (lives <= 0)
            return;

        if (canSlingshot)
            HandleInput();
        else
            CheckRestAndReset();
    }

    void FixedUpdate()
    {
        if (rb != null)
            currentSpeed = rb.linearVelocity.magnitude;
    }

    private void HandleInput()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            dragStartPos = slingshotAnchor;
            isDragging = true;
            rb.isKinematic = true;
            rb.linearDamping = originalDrag;
        }

        if (Mouse.current.leftButton.isPressed && isDragging)
        {
            Ray ray = mainCam.ScreenPointToRay(Mouse.current.position.ReadValue());
            Plane plane = new Plane(Vector3.up, Vector3.zero);
            if (plane.Raycast(ray, out float enter))
            {
                Vector3 currentPos = ray.GetPoint(enter);
                ContinueDrag(currentPos);
            }
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame && isDragging)
        {
            Ray ray = mainCam.ScreenPointToRay(Mouse.current.position.ReadValue());
            Plane plane = new Plane(Vector3.up, Vector3.zero);
            if (plane.Raycast(ray, out float enter))
            {
                Vector3 releasePoint = ray.GetPoint(enter);
                EndDragAndLaunch(releasePoint);
            }
        }
    }

    private void ContinueDrag(Vector3 worldPoint)
    {
        Vector3 pullVector = worldPoint - dragStartPos;
        pullVector.y = 0f;
        if (pullVector.magnitude > maxStretch)
            pullVector = pullVector.normalized * maxStretch;

        transform.position = dragStartPos + pullVector;
    }

    private void EndDragAndLaunch(Vector3 releasePoint)
    {
        isDragging = false;
        Vector3 pull = transform.position - slingshotAnchor;
        float pullMagnitude = pull.magnitude;

        if (pullMagnitude <= 0.0001f)
        {
            transform.position = slingshotAnchor;
            rb.isKinematic = true;
            rb.linearDamping = originalDrag;
            return;
        }

        Vector3 launchDirection = (slingshotAnchor - transform.position).normalized;
        float calculatedForce = pullMagnitude * launchForceMultiplier * speed;
        float finalForce = Mathf.Min(calculatedForce, maxLaunchForce);

        rb.isKinematic = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.linearDamping = airDrag;
        rb.AddForce(launchDirection * finalForce, ForceMode.VelocityChange);

        transform.position += Vector3.up * height;

        if (launchClip != null && audioSource != null)
            audioSource.PlayOneShot(launchClip, launchVolume);

        canSlingshot = false;
        Lives--; // use property so death triggers and UI updates
        restTimer = 0f;
    }

    private void CheckRestAndReset()
    {
        float linSpeed = rb.linearVelocity.magnitude;
        float angSpeed = rb.angularVelocity.magnitude;

        if (linSpeed <= stopThreshold && angSpeed <= stopAngularThreshold)
        {
            restTimer += Time.deltaTime;
            if (restTimer >= restDuration)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.Sleep();
                rb.linearDamping = originalDrag;
                slingshotAnchor = transform.position;
                transform.position = slingshotAnchor;
                rb.isKinematic = true;
                canSlingshot = true;
                restTimer = 0f;
            }
        }
        else
        {
            restTimer = 0f;
        }
    }

    private void Die()
    {
        isDead = true;

        if (deathClip != null && audioSource != null)
            audioSource.PlayOneShot(deathClip, deathVolume);

        // notify manager (optional but useful)
        GameManager.Instance?.OnPlayerDied();

        // Disable input/collider so the body can't be re-shot or collide oddly
        canSlingshot = false;
        if (col != null) col.enabled = false;

        rb.isKinematic = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.AddForce(Vector3.up * deathLaunchForce, ForceMode.VelocityChange);

        Destroy(gameObject, destroyDelay);
    }

    public void KillByTimer()
    {
        if (!isDead)
            Lives = 0;
    }
}
