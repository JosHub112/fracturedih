using UnityEngine;

public class WallBreaker : MonoBehaviour
{
    [SerializeField] private Rigidbody[] wallPieces;
    [SerializeField] private float breakSpeedThreshold = 10f;
    [SerializeField] private float destroyDelay = 4f;
    [SerializeField] private float explosionForce = 5f;
    [SerializeField] private float explosionRadius = 3f;
    [SerializeField] private bool applyExplosionFromImpactPoint = true;

    [Header("Audio")]
    [SerializeField] private AudioClip breakClip;
    [SerializeField][Range(0f, 1f)] private float breakVolume = 1f;

    private AudioSource audioSource;
    private Collider mainCollider;
    private bool broken = false;

    void Start()
    {
        if (wallPieces == null || wallPieces.Length == 0)
            wallPieces = GetComponentsInChildren<Rigidbody>();

        mainCollider = GetComponent<Collider>();

        foreach (Rigidbody piece in wallPieces)
        {
            if (piece == null) continue;
            piece.isKinematic = true;
            piece.useGravity = false;
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (broken) return;

        float impactSpeed = collision.relativeVelocity.magnitude;
        if (collision.rigidbody != null)
            impactSpeed = Mathf.Max(impactSpeed, collision.rigidbody.linearVelocity.magnitude);

        if (impactSpeed >= breakSpeedThreshold)
        {
            Vector3 impactPoint = collision.contacts.Length > 0 ? collision.contacts[0].point : transform.position;
            Break(impactPoint);
        }
    }

    public void ForceBreak()
    {
        if (broken) return;
        Break(transform.position);
    }

    private void Break(Vector3 origin)
    {
        broken = true;

        if (breakClip != null && audioSource != null)
            audioSource.PlayOneShot(breakClip, breakVolume);

        foreach (Rigidbody piece in wallPieces)
        {
            if (piece == null) continue;
            piece.isKinematic = false;
            piece.useGravity = true;
            if (applyExplosionFromImpactPoint)
                piece.AddExplosionForce(explosionForce, origin, explosionRadius, 0.5f, ForceMode.Impulse);
        }

        if (mainCollider != null) mainCollider.enabled = false;

        Destroy(gameObject, destroyDelay);
    }
}
