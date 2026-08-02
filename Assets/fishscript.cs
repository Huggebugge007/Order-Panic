using UnityEngine;


public class fishscript : MonoBehaviour
{
    public bool inwater, inpool;
    public stats stats;

    public float speed;
    public Vector2 goal;

    public LayerMask waterlayer, poollayer;
    public GameObject player;
    public bool chasingplayer = false;

    public Rigidbody2D rb;

    public float turnSpeed = 3f;
    public float wanderRadius = 10f;
    public float wanderY = 3f;
    public float timetildry = 30f;
    private float countdown;



    bool wasinwater = false;

    void Awake()
    {
        wasinwater = false;
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        countdown = timetildry;
        speed = stats.speed;
        player = GameObject.FindGameObjectWithTag("Player");
    }

    void Update()
    {
        inwater = rb.IsTouchingLayers(waterlayer);
        inpool = rb.IsTouchingLayers(poollayer);

        if(inwater || inpool)
        {
            countdown = timetildry;
        }

        

        if(!inwater && !inpool)
        {
            countdown -= Time.deltaTime;
        }
        if(countdown <= 0)
        {
            Destroy(gameObject);
        }

        if (!wasinwater && inwater)
        {
            PickNewGoal();
        }

        wasinwater = inwater;

        if (!inwater)
        {
            rb.gravityScale = 0.354f;
            return;
        }

        rb.gravityScale = 0f;

        if (chasingplayer && player != null)
        {
            goal = player.transform.position;
        }
        else if (Vector2.Distance(transform.position, goal) < 0.7f)
        {
            PickNewGoal();
        }
    }

    void FixedUpdate()
    {
        if (!inwater) return;

        SwimTowardsGoal();
    }

    void SwimTowardsGoal()
    {
        Vector2 direction = (goal - (Vector2)transform.position).normalized;

        Vector2 desiredVelocity = direction * speed;

        rb.linearVelocity = Vector2.Lerp(
            rb.linearVelocity,
            desiredVelocity,
            Time.deltaTime * 2f
        );

        if (rb.linearVelocity.sqrMagnitude > 0.01f)
        {
            Vector2 dir = rb.linearVelocity.normalized;

            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * (dir.x < 0 ? -1 : 1);
            transform.localScale = scale;

            float tilt = Mathf.Clamp(dir.y * 45f, -45f, 45f);

            Quaternion targetRotation = Quaternion.Euler(0, 0, tilt);

            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                targetRotation,
                turnSpeed * Time.deltaTime
            );
        }
    }

    void PickNewGoal()
    {
        goal = new Vector2(
            transform.position.x + Random.Range(-wanderRadius, wanderRadius),
            transform.position.y + Random.Range(-wanderY, wanderY)
        );
        goal.y = Mathf.Min(goal.y, -2.5f);
        if (Mathf.Abs(goal.x)< ((Mathf.Abs(goal.y)*2)+ 1))
        {
            PickNewGoal();
        }
        
    }
}