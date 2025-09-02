using UnityEngine;

public class GlassBreaker : MonoBehaviour
{
    [SerializeField]private Rigidbody[] GlassPieces;
    public float Speed = 12f; // example starting speed

    void Start()
    {

        GlassPieces = GetComponentsInChildren<Rigidbody>();

        foreach (Rigidbody piece in GlassPieces)
        {
            piece.isKinematic = true;
            piece.useGravity = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (Speed >= 10f)
            {
                Speed -= 2f;
                foreach (Rigidbody piece in GlassPieces)
                {
                    piece.isKinematic = false;
                    piece.useGravity = true;
                }
             Object.Destroy(gameObject, 4f);
            }
            else
            {
                Speed = 0f;
            }
        }
    }
}