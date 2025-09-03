using UnityEngine;

public class CameraFollowXZ : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform player;
    [SerializeField] private bool findPlayerByTag = true;
    [SerializeField] private string playerTag = "Player";

    [Header("Offset & smoothing")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 15f, 0f); // x,z offsets; y is camera height
    [SerializeField] private float smoothTime = 0.12f;

    private Vector3 velocity = Vector3.zero;

    void Start()
    {
        if (player == null && findPlayerByTag)
        {
            GameObject go = GameObject.FindWithTag(playerTag);
            if (go != null) player = go.transform;
        }

        if (player == null)
            Debug.LogWarning("CameraFollowXZ: Player not assigned or not found with tag '" + playerTag + "'.");
    }

    void LateUpdate()
    {
        if (player == null) return;

        Vector3 targetPosition = new Vector3(
            player.position.x + offset.x,
            offset.y,
            player.position.z + offset.z
        );

        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }
}
