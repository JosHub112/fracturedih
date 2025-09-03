using UnityEngine;

public class GlassBreaker : MonoBehaviour
{
    [SerializeField] private Rigidbody[] GlassPieces;
    public float Speed = 12f;
    private Collider Collider;
    private int score;

    [Header("Audio")]
    [SerializeField] private AudioClip breakClip;
    [SerializeField][Range(0f, 1f)] private float breakVolume = 1f;
    [SerializeField] PlayerControls player;

    private AudioSource audioSource;

    void Start()
    {
        player = GameObject.FindWithTag("Player").GetComponent<PlayerControls>();
        GlassPieces = GetComponentsInChildren<Rigidbody>();
        Collider = GetComponent<Collider>();
        foreach (Rigidbody piece in GlassPieces)
        {
            piece.isKinematic = true;
            piece.useGravity = false;
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {



                if (breakClip != null && audioSource != null)
                    audioSource.PlayOneShot(breakClip, breakVolume);

                foreach (Rigidbody piece in GlassPieces)
                {
                    piece.isKinematic = false;
                    piece.useGravity = true;
                    if (Collider != null) Collider.enabled = false;
                }
                Object.Destroy(gameObject, 4f);
        }
    }
}
