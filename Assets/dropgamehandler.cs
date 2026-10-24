using UnityEngine;
using UnityEngine.UIElements;

public class dropgamehandler : MonoBehaviour
{
    private float camWidth, camHeight;
    private float leftEdge;
    private float rightEdge;
    private float topEdge;
    private Rigidbody2D rb;

    private int direction = 1;
    public float speed = 2f;

    public Camera cam;

    private dropscript dropscript;
    void Awake()
    {
        cam.orthographicSize = 2f;
        dropscript = GetComponent<dropscript>();
        rb = GetComponent<Rigidbody2D>();
        reposition();
    }


    public void reposition()
    {
        camWidth = cam.orthographicSize * 2f * cam.aspect;
        camHeight = cam.orthographicSize * 2f;



        cam.transform.position = new Vector3(cam.transform.position.x, cam.orthographicSize - 1 + 5000, cam.transform.position.z);

        leftEdge = (cam.transform.position.x - camWidth / 2f) * 0.8f;
        rightEdge = (cam.transform.position.x + camWidth / 2f) * 0.8f;
        topEdge = cam.transform.position.y + cam.orthographicSize;

        transform.position = new Vector3(transform.position.x, topEdge + 0.1f, transform.position.z);
        speed = cam.orthographicSize * 0.6f;


        dropscript.segmentLength = cam.orthographicSize / 30;
        dropscript.CreateRope();
    }

    public void setcamsize(float camsize)
    {
        cam.orthographicSize = camsize;
        reposition();
    }
    void FixedUpdate()
    {
        move();
    }

    public void move()
    {
        float nextX = rb.position.x + direction * speed * Time.fixedDeltaTime;

        if (nextX >= rightEdge)
        {
            nextX = rightEdge;
            direction = -1;
        }
        else if (nextX <= leftEdge)
        {
            nextX = leftEdge;
            direction = 1;
        }

        rb.MovePosition(new Vector2(nextX, rb.position.y));

    }
}
