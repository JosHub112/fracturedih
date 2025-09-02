using UnityEngine;

public class Glassbreak: MonoBehaviour
{
    private Rigidbody rb;
    [SerializeField] private float Speed = 10f; 

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
 

        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;  
        rb.useGravity = false;  
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Check if we collided with the Player
        if (collision.gameObject.CompareTag("Player"))
        {     

            // Adjust player's speed
            if (Speed >= 10f)
            {
                Speed -= 2f;
                rb.isKinematic = false;
                rb.useGravity = true;
            }
            else
            {
                Speed = 0f;
            }
        }
    }
}
