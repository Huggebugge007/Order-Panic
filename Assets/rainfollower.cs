using UnityEngine;

public class rainfollower : MonoBehaviour
{
    public Transform player;
    public float smoothTime = 0.2f;
    public Vector3 offset;
    private Vector3 velocity = Vector3.zero;
    void Start()
    {
        
    }


    void Update()
    {
        Vector3 targetPosition = new Vector3(player.position.x,transform.position.y,transform.position.z) + offset;


        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref velocity,
            smoothTime
        );
    }
}
