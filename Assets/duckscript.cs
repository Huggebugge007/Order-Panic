using UnityEngine;
public class duckscript : MonoBehaviour
{
    public LayerMask waterLayer;
    public Transform target;

    public Transform floatPoint;

    private Rigidbody2D rb;
    public float movementSpeed = 5f;
    public float floatForce = 15f;
    public float waterDrag = 3f;

    public float depth;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

void FixedUpdate()
{
    Collider2D water = Physics2D.OverlapPoint(
        floatPoint.position,
        waterLayer
    );

    if (water != null)
    {
        float waterSurface = water.bounds.max.y;

        depth = waterSurface - floatPoint.position.y;
        rb.AddForce(Vector2.up  * depth * floatForce);


        rb.linearDamping = waterDrag;
    }
    else
    {
        Debug.Log("not in water");
        rb.linearDamping = 0f;
    }
}

    void Update()
    {

        movement();
    }
    
    private void movement()
    {
        float direction = Mathf.Sign(target.position.x - transform.position.x);
        rb.linearVelocity = new Vector2(direction * movementSpeed, rb.linearVelocity.y);
    }

}
