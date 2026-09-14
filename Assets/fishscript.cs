using UnityEngine;
using System.Collections;

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
    public float wanderRadius = 2f;
    public float wanderY = 3f;
    public float timetildry = 30f;
    public float countdown;

    public bool exploding = false;

    public Vector2 startpos;

    bool wasinwater = false;
    bool wasinpool = false;
    public GameObject market;

    public GameObject explosion1, explosion2;

    private float timetonewgoal;
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
        Transform grandparent = transform.parent?.parent;

        if (grandparent == player.transform)
        {
            transform.position = transform.parent.position;
        }

        if (startpos == Vector2.zero)
        {
            startpos = transform.position;
        }
        inwater = rb.IsTouchingLayers(waterlayer);
        inpool = rb.IsTouchingLayers(poollayer);

        if(inwater || inpool)
        {
            countdown = timetildry;
        }
        timetonewgoal -= Time.deltaTime;

        if(timetonewgoal <= 0)
        {
            PickNewGoal();
            timetonewgoal = 8;
        }
        

        if(!inwater && !inpool)
        {
            if (market.activeSelf == false)
            {
                countdown -= Time.deltaTime;
            }
            
        }
        if(countdown <= 0)
        {
            DestroyWithExplosion();
        }

        if (!wasinwater && inwater)
        {
            startpos = transform.position;
            PickNewGoal();
            timetonewgoal = 8;
        }
        if(inpool && !wasinpool)
        {
            startpos = transform.position;
            PickNewGoal();
            timetonewgoal = 8;
        }

        wasinwater = inwater;
        wasinpool = inpool;

        if (!inwater && !inpool)
        {
            rb.gravityScale = 0.354f;

        }
        else
        {
            rb.gravityScale = 0f;
        }
        

        if (chasingplayer && player != null)
        {
            goal = player.transform.position;
        }
        else if (Vector2.Distance(transform.position, goal) < 0.3f)
        {
            PickNewGoal();
            timetonewgoal = 8;
        }

        if(Vector2.Distance(transform.position, goal) > 10)
        {
            PickNewGoal();
            timetonewgoal = 8;
        }
        if (inwater)
        {
            goal.y = Mathf.Min(goal.y, -2.5f);
        }
    }

    void FixedUpdate()
    {
        if (inwater || inpool)
        {
            if (!exploding)
            {
                SwimTowardsGoal();
            }

        }

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
        do
        {
            goal = new Vector2(
                startpos.x + Random.Range(-wanderRadius, wanderRadius),
                startpos.y + Random.Range(-wanderY, wanderY)
            );

            // X position of the slope at this Y
            float slopeX = -3f + ((goal.y + 1f) * 2f);

            // Keep trying while the goal is NOT on the left side
            if (goal.x < slopeX)
            {
                break;
            }

        } while (true);
    }
    public void DestroyWithExplosion()
    {
        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;
        exploding = true;
        Instantiate(explosion1, transform.position, Quaternion.identity);
        Instantiate(explosion2, transform.position, Quaternion.identity);
        StartCoroutine(DestroyAfterExplosion());
    }

    IEnumerator DestroyAfterExplosion()
    {
        yield return new WaitForSeconds(0.3f);
        Destroy(gameObject);
    }
}