using System.Collections;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class camscript : MonoBehaviour
{

    public handler handler;
    public Transform player;
    public playerscript playerscript;
    public float smoothTime = 0.2f;
    public Vector3 offset;
    private Vector3 velocity = Vector3.zero;

    public Gradient watergradient;
    Camera cam;
    public float depht = 1000f;
    private void Awake()
    {

        cam = gameObject.GetComponent<Camera>();
    }

    void Update()
    {
        //cam.backgroundColor = watergradient.Evaluate((Mathf.Abs(transform.position.y) / depht));

        if (!playerscript.isgrappled && playerscript.iswater())
        {
            if(player.transform.position.y <= -3)
            {
                playerscript.respawn();
            }
        }
        else
        {
            Vector3 targetPosition = player.position + offset;


            transform.position = Vector3.SmoothDamp(
                transform.position,
                targetPosition,
                ref velocity,
                smoothTime
            );
        }
    }


}