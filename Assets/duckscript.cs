using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;
using static UnityEngine.Rendering.DebugUI;
public class duckscript : MonoBehaviour
{
    public LayerMask waterLayer, groundLayer;
    public Transform target;

    private SpriteRenderer spriteRenderer;
    public moneyhandler moneyhandler;
    public Transform floatPoint, tavernentrance, tavernexit;
    public float flatAngleThreshold = 5f;
    public float maxTiltAngle = 40f;
    public float levelingStrength = 5f;
    public float dampingStrength = 1f;
    private Rigidbody2D rb;
    public float movementSpeed = 5f;
    public float floatForce = 15f;
    public float waterDrag = 3f;
    public GameObject explosion1, explosion2;

    float time;
    public bool finsihed = false;
    public float airdrag;
    public List<Transform> targets;

    public int targetindex = 0;

    public bool arewethereyet = false;

    public GameObject seat;

    public handler handler;

    private int iterationCount = 0;

    public GameObject wantedfish;
    private bool exploding = false;

    private GameObject thoughtbubble;

    public LayerMask fishlayer;
    void Awake()
    {
        thoughtbubble = transform.Find("thoughtbubble").gameObject;
        thoughtbubble.SetActive(false);
        time = 0;
        arewethereyet = false;
        targetindex = 0;
        rb = GetComponent<Rigidbody2D>();
        StartCoroutine(FindSeatsNextFrame());
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private IEnumerator FindSeatsNextFrame()
    {
        yield return null;
        target = targets[targetindex];
        wantedfish = handler.avaliblefish[Random.Range(0, handler.resturantquality - 1)];
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

            float depth = waterSurface - floatPoint.position.y;
            rb.AddForce(Vector2.up * depth * floatForce);

            rb.linearDamping = waterDrag;
        }
        else
        {
            rb.linearDamping = airdrag;
        }

        if (!arewethereyet)
        {
            if (!exploding)
            {
                movement();
            }

        }

        float angle = rb.rotation;

        // Figure out the angle we should be leveling to: 0 if flat/no ground
        // detected (e.g. on water), or the slope angle if standing on ground.
        float targetAngle = 0f;

        RaycastHit2D groundHit = Physics2D.Raycast(
            transform.position,
            Vector2.down,
            0.3f, // tweak this to your character's height/leg length
            groundLayer
        );

        if (groundHit.collider != null)
        {
            Vector2 normal = groundHit.normal;
            targetAngle = Mathf.Atan2(-normal.x, normal.y) * Mathf.Rad2Deg;

            // Snap to flat if the slope is close enough to level
            if (Mathf.Abs(targetAngle) < flatAngleThreshold)
            {
                targetAngle = 0f;
            }
        }

        // Signed shortest difference between current and target angle
        float angleDiff = Mathf.DeltaAngle(angle, targetAngle);

        // If we're basically at rest on the target angle, snap fully and skip
        // applying torque so it doesn't sit there jittering forever.
        if (Mathf.Abs(angleDiff) < 0.5f && Mathf.Abs(rb.angularVelocity) < 0.5f)
        {
            rb.rotation = targetAngle;
            rb.angularVelocity = 0f;
        }
        else
        {
            // Spring torque: proportional to how far we are from the target angle
            float springTorque = angleDiff * levelingStrength;

            // Damping: opposes current angular velocity so it settles instead of oscillating
            float dampingTorque = -rb.angularVelocity * dampingStrength;

            rb.AddTorque(springTorque + dampingTorque);
        }

        // Hard clamp relative to the target angle, not absolute zero
        float minAngle = targetAngle - maxTiltAngle;
        float maxAngle = targetAngle + maxTiltAngle;

        if (angle > maxAngle)
        {
            rb.rotation = maxAngle;
            rb.angularVelocity = 0f;
        }
        else if (angle < minAngle)
        {
            rb.rotation = minAngle;
            rb.angularVelocity = 0f;
        }
    }

    void Update()
    {
        time += Time.deltaTime;

        if (target != null)
        {
            if (Vector2.Distance(transform.position, target.position) < 0.035f && !arewethereyet)
            {
                if (targetindex == targets.Count - 1)
                {
                    arewethereyet = true;
                    if(target.name.Contains("3") || target.name.Contains("1"))
                    {
                        transform.localScale = new Vector3(-1, 1, 1);
                    }
                    thoughtbubble.SetActive(true);
                    int sortingnumber = 0;

                    if (target.name.Contains("(") && target.name.Contains(")"))
                    {
                        int.TryParse(target.name.Split('(', ')')[1], out sortingnumber);
                    }
                    thoughtbubble.GetComponent<SpriteRenderer>().sortingOrder = -sortingnumber;
                    SpriteRenderer thoughtbubblechildrenderer = thoughtbubble.transform.GetChild(0).GetComponent<SpriteRenderer>();
                    thoughtbubblechildrenderer.sortingOrder = -sortingnumber;
                    thoughtbubblechildrenderer.sprite = wantedfish.transform.Find("sprite").GetComponent<SpriteRenderer>().sprite;
                }
                else
                {
                    if (target == tavernentrance)
                    {
                        transform.position = tavernexit.position;
                    }

                    targetindex++;
                    target = targets[targetindex];
                }
            }
        }
        if(iterationCount != 0)
        {
            if (finsihed || target == null)
            {
                if (seat != null)
                {
                    if (!handler.avalibleseats.Contains(seat))
                    {
                        handler.avalibleseats.Add(seat);
                        DestroyWithExplosion();
                    }
                }
                else
                {
                    DestroyWithExplosion();
                }
            }
        }
        if(time >= 100 && !arewethereyet)
        {
            if (seat != null)
            {
                if (!handler.avalibleseats.Contains(seat))
                {
                    handler.avalibleseats.Add(seat);
                    DestroyWithExplosion();
                }
            }
            else
            {
                DestroyWithExplosion();
            }
        }
        iterationCount++;
        if(arewethereyet && !exploding && !finsihed)
        {
            bool touchingfish = Physics2D.OverlapCircle(transform.position, 0.2f, fishlayer);
            if (touchingfish)
            {
                Collider2D hit = Physics2D.OverlapCircle(transform.position, 0.2f);
                if (hit.name.Substring(0, hit.name.Length - 7) == wantedfish.name)
                {

                    int cost = hit.GetComponent<stats>().cost;
                    moneyhandler.Changemoney(cost);

                    hit.GetComponent<fishscript>().DestroyWithExplosion();
                    finsihed = true;
                    DestroyWithExplosion();
                }
            }
        }
    }

    private void movement()
    {
        if (target == null)
        {
            return;
        }

        float direction = Mathf.Sign(target.position.x - transform.position.x);
        if(target.position.x > transform.position.x)
        {
            spriteRenderer.flipX = true;
        }
        else
        {
            spriteRenderer.flipX = false;
        }

        if (Mathf.Abs(transform.position.x - target.position.x) > 0.03f)
        {
            RaycastHit2D hit = Physics2D.Raycast(
                transform.position,
                Vector2.down,
                0.15f,
                groundLayer
            );

            if (hit.collider != null)
            {
                Vector2 normal = hit.normal;

                // Direction along the slope
                Vector2 slopeDirection = new Vector2(normal.y, -normal.x);

                // Make sure we're moving toward the target
                if (slopeDirection.x * direction < 0)
                {
                    slopeDirection = -slopeDirection;
                }

                rb.linearVelocity = slopeDirection * movementSpeed;
            }
            else
            {
                // Normal movement when there's no slope
                rb.linearVelocity = new Vector2(
                    direction * movementSpeed,
                    rb.linearVelocity.y
                );
            }
        }
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