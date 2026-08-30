using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class duckscript : MonoBehaviour
{
    public LayerMask waterLayer, groundLayer;
    public Transform target;

    private SpriteRenderer spriteRenderer;
    public Transform floatPoint, tavernentrance, tavernexit;
    public float flatAngleThreshold = 5f;
    public float maxTiltAngle = 40f;
    public float levelingStrength = 5f;
    public float dampingStrength = 1f;
    private Rigidbody2D rb;
    public float movementSpeed = 5f;
    public float floatForce = 15f;
    public float waterDrag = 3f;

    float time;
    public bool finsihed = false;
    public float airdrag;
    public List<Transform> targets;

    public int targetindex = 0;

    public bool arewethereyet = false;

    public GameObject seat;

    public handler handler;

    private int iterationCount = 0;

    void Awake()
    {
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
            movement();
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
            if (Vector2.Distance(transform.position, target.position) < 0.05f && !arewethereyet)
            {
                if (targetindex == targets.Count - 1)
                {
                    arewethereyet = true;
                }
                else
                {
                    if (Vector2.Distance(transform.position, tavernentrance.position) < 0.1f)
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
                        Destroy(gameObject);
                    }
                }
                else
                {
                    Destroy(gameObject);
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
                    Destroy(gameObject);
                }
            }
            else
            {
                Destroy(gameObject);
            }
        }
        iterationCount++;

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
            // Changed from 0.3f to 0.15f
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
}