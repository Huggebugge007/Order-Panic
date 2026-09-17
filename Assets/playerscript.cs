using JetBrains.Annotations;
using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class playerscript : MonoBehaviour
{
    public bool fishing = false;
    public float walkspeed;
    public float maxspeed = 6f;
    public Rigidbody2D rb;
    public LayerMask ground;
    bool grounded;
    public float jumppower;
    bool isfacingright = false;
    float x;
    public Transform groundcheck;
    [SerializeField] bool waiting;
    bool previous = false;
    public handler handler;
    Vector2 grapplepoint;
    public bool isgrappled;
    public LineRenderer lr;
    public Transform ringcheck;
    public float ropelenght;
    public SpringJoint2D joint;
    public float springfreq, springdamp;
    public float reelspeed;
    public LayerMask water, poolwater;
    public Transform fishpos;
    public float pickupradius;
    public LayerMask fishlayer;
    public float swingforce;
    public fightmanager fightmanager;
    public GameObject catchingfish;
    public GameObject mybait;
    public float rotationspeed = 200f;
    public float rolltorque = 15f;
    public GameObject cannonhookpos, cannonlinepos;
    public GameObject cannon, cannonbarrel;
    public float cannonatatchdistance = 1f;
    public float cannonlaunchforce;
    public float cannonrotationspeed;
    private float currentAngle;
    public float totalboost;
    public float boostpower;
    public float usableboost;
    private bool previouswater = false;
    private bool previouspool = false;
    public Transform fishpickuppoint;

    // Interaction animations
    public GameObject eanimation;
    public GameObject fanimation;
    public GameObject ranimation;
    public GameObject ganimation;

    public Vector3 cannonAnimationOffset = new Vector3(0, 1f, 0);
    public Vector3 fishAnimationOffset = new Vector3(0, 1f, 0);
    public Vector3 cannonRAnimationOffset = new Vector3(0, 1f, 0);
    public Vector3 waterRAnimationOffset = new Vector3(0, 1f, 0);
    public Vector3 gAnimationOffset = new Vector3(0, 1.5f, 0);

    public Vector3 fishAnimationScale = Vector3.one;
    public Vector3 rAnimationScale = Vector3.one;
    public Vector3 gAnimationScale = Vector3.one;

    private GameObject currentCannonAnimation;
    private GameObject currentFishAnimation;
    private GameObject currentCannonRAnimation;
    private GameObject currentWaterRAnimation;
    private GameObject currentGAnimation;

    private Transform currentFishAnimationTarget;

    Bounds waterBounds;
    public Collider2D waterCollider;


    void Awake()
    {
        fightmanager = GameObject.FindGameObjectWithTag("fightmanager").GetComponent<fightmanager>();
    }


    void Update()
    {
        bool inWater = waterCollider.bounds.Contains(transform.position);
        bool inPool = ispool();


        // R animation above player while in water
        if (inWater || inPool)
        {
            if (currentWaterRAnimation == null)
            {
                currentWaterRAnimation = Instantiate(
                    ranimation,
                    transform.position + waterRAnimationOffset,
                    Quaternion.identity
                );

                currentWaterRAnimation.transform.localScale = rAnimationScale;
            }

            currentWaterRAnimation.transform.position =
                transform.position + waterRAnimationOffset;

            currentWaterRAnimation.transform.rotation = Quaternion.identity;
            currentWaterRAnimation.transform.localScale = rAnimationScale;
        }
        else
        {
            if (currentWaterRAnimation != null)
            {
                Destroy(currentWaterRAnimation);
                currentWaterRAnimation = null;
            }
        }


        // G animation when holding a fish
        if (fishpos.childCount > 0 && !waiting)
        {
            if (currentGAnimation == null)
            {
                currentGAnimation = Instantiate(
                    ganimation,
                    transform.position + gAnimationOffset,
                    Quaternion.identity
                );

                currentGAnimation.transform.localScale = gAnimationScale;
            }

            currentGAnimation.transform.position =
                transform.position + gAnimationOffset;

            currentGAnimation.transform.rotation = Quaternion.identity;
            currentGAnimation.transform.localScale = gAnimationScale;
        }
        else
        {
            if (currentGAnimation != null)
            {
                Destroy(currentGAnimation);
                currentGAnimation = null;
            }
        }


        // När spelaren precis går ner i vatten
        if (!previouswater && inWater && isgrappled)
        {
            usableboost = totalboost;
        }


        if (!previouspool && inPool)
        {
            usableboost = totalboost;
        }


        if (!previouswater && inWater && isgrappled || !previouspool && inPool)
        {
            rb.angularVelocity = 0f;
        }


        fishing = inPool || (inWater && isgrappled);

        previouswater = inWater;
        previouspool = inPool;


        if (isgrappled && inWater)
        {
            joint.distance = Vector2.Distance(transform.position, grapplepoint);
            joint.enabled = true;
        }


        isgrounded();


        // Cannon E animation
        if (!fishing && !waiting &&
            Vector2.Distance(transform.position, cannon.transform.position) < cannonatatchdistance)
        {
            if (currentCannonAnimation == null)
            {
                currentCannonAnimation = Instantiate(
                    eanimation,
                    cannon.transform.position + cannonAnimationOffset,
                    Quaternion.identity,
                    cannon.transform
                );
            }
        }
        else
        {
            if (currentCannonAnimation != null)
            {
                Destroy(currentCannonAnimation);
                currentCannonAnimation = null;
            }
        }


        // Cannon R animation
        if (waiting)
        {
            if (currentCannonRAnimation == null)
            {
                currentCannonRAnimation = Instantiate(
                    ranimation,
                    cannon.transform.position + cannonRAnimationOffset,
                    Quaternion.identity
                );

                currentCannonRAnimation.transform.localScale = rAnimationScale;
            }

            currentCannonRAnimation.transform.position =
                cannon.transform.position + cannonRAnimationOffset;

            currentCannonRAnimation.transform.rotation = Quaternion.identity;
            currentCannonRAnimation.transform.localScale = rAnimationScale;
        }
        else
        {
            if (currentCannonRAnimation != null)
            {
                Destroy(currentCannonRAnimation);
                currentCannonRAnimation = null;
            }
        }


        if (!fishing &&
            Input.GetKeyDown(KeyCode.E) &&
            Vector2.Distance(transform.position, cannon.transform.position) < cannonatatchdistance)
        {
            waiting = true;
            rope();
        }


        x = Input.GetAxis("Horizontal");


        if (!fishing && !waiting)
        {
            if (Input.GetButtonDown("Jump") && isgrounded() && !waiting)
            {
                rb.AddForce(transform.up * jumppower, ForceMode2D.Impulse);
            }
        }


        if (waiting)
        {
            transform.position = new Vector2(
                cannonhookpos.transform.position.x,
                cannonhookpos.transform.position.y
            );


            rb.MoveRotation(cannonbarrel.transform.rotation.eulerAngles.z - 90);


            if (Input.GetButtonDown("Jump"))
            {
                waiting = false;
                rb.angularVelocity = 0f;
                rb.AddForce(-transform.up * cannonlaunchforce, ForceMode2D.Impulse);
            }


            currentAngle += -x * cannonrotationspeed * Time.deltaTime;
            currentAngle = Mathf.Clamp(currentAngle, -54, 20);

            cannonbarrel.transform.rotation =
                Quaternion.Euler(0, 0, currentAngle);

            grapplepoint = cannonlinepos.transform.position;
        }


        if (isgrappled)
        {
            lr.SetPosition(0, ringcheck.position);
            lr.SetPosition(1, grapplepoint);
        }


        if (Input.GetKeyDown(KeyCode.R) && isgrappled && !fishing)
        {
            lr.enabled = false;
            joint.enabled = false;
            isgrappled = false;
            waiting = false;
        }


        if (isgrappled)
        {
            float dist = Vector2.Distance(transform.position, grapplepoint);

            if (dist >= joint.distance)
            {
                joint.enabled = true;
            }
            else
            {
                joint.enabled = false;
            }
        }


        if (Input.GetKeyDown(KeyCode.G) && fishpos.childCount > 0 && !waiting)
        {
            drop();
        }


        if (Input.GetKeyDown(KeyCode.R) && fishing && transform.position.y < 500)
        {
            stopfishing();
        }


        Collider2D[] fishCols = Physics2D.OverlapCapsuleAll(
            fishpickuppoint.position,
            new Vector2(
                pickupradius * 0.25f,
                pickupradius * 0.65f
            ),
            CapsuleDirection2D.Vertical,
            0f,
            fishlayer
        );


        Collider2D fishCol = null;
        float closestDist = float.MaxValue;


        foreach (Collider2D hit in fishCols)
        {
            // Ignore fish that the player is already carrying
            if (hit.transform.IsChildOf(fishpos))
                continue;

            float dist =
                (hit.transform.position - transform.position).sqrMagnitude;

            if (dist < closestDist)
            {
                closestDist = dist;
                fishCol = hit;
            }
        }


        // Fish F animation
        if (fishCol != null)
        {
            if (currentFishAnimationTarget != fishCol.transform)
            {
                if (currentFishAnimation != null)
                    Destroy(currentFishAnimation);

                currentFishAnimationTarget = fishCol.transform;

                currentFishAnimation = Instantiate(
                    fanimation,
                    fishCol.transform.position + fishAnimationOffset,
                    Quaternion.identity
                );

                currentFishAnimation.transform.localScale =
                    fishAnimationScale;
            }


            if (currentFishAnimation != null)
            {
                currentFishAnimation.transform.position =
                    fishCol.transform.position + fishAnimationOffset;

                currentFishAnimation.transform.rotation =
                    Quaternion.identity;

                currentFishAnimation.transform.localScale =
                    fishAnimationScale;
            }
        }
        else
        {
            if (currentFishAnimation != null)
            {
                Destroy(currentFishAnimation);

                currentFishAnimation = null;
                currentFishAnimationTarget = null;
            }
        }


        if (fishCol != null &&
            Input.GetKeyDown(KeyCode.F) &&
            fishpos.transform.childCount <= 1)
        {
            if (!fishCol.GetComponent<stats>().defeated)
            {
                fishCol.GetComponent<fishscript>().chasingplayer = true;


                if (fishpos.childCount == 0)
                {
                    rb.constraints =
                        RigidbodyConstraints2D.FreezePosition;

                    catchingfish =
                        fishCol.GetComponent<stats>().gameObject;

                    fightmanager.changescene(1);


                    if (currentFishAnimation != null)
                    {
                        Destroy(currentFishAnimation);

                        currentFishAnimation = null;
                        currentFishAnimationTarget = null;
                    }


                    this.enabled = false;
                }
                else
                {
                    rb.constraints =
                        RigidbodyConstraints2D.FreezePosition;

                    catchingfish =
                        fishCol.GetComponent<stats>().gameObject;

                    mybait =
                        fishpos.GetChild(0).gameObject;

                    fightmanager.changescene(1);


                    if (currentFishAnimation != null)
                    {
                        Destroy(currentFishAnimation);

                        currentFishAnimation = null;
                        currentFishAnimationTarget = null;
                    }


                    this.enabled = false;
                }
            }
            else if (fishpos.childCount < 2 &&
                     fishCol.GetComponent<stats>().defeated)
            {
                GameObject fish =
                    fishCol.GetComponent<stats>().gameObject;

                Collider2D col =
                    fish.GetComponent<Collider2D>();

                fishscript fishscript =
                    fish.GetComponent<fishscript>();


                if (col == null || fish == null)
                    return;


                fish.GetComponent<Rigidbody2D>().bodyType =
                    RigidbodyType2D.Kinematic;

                fish.transform.parent = fishpos;
                fish.transform.position = fishpos.position;


                if (currentFishAnimation != null)
                {
                    Destroy(currentFishAnimation);

                    currentFishAnimation = null;
                    currentFishAnimationTarget = null;
                }
            }
        }


        if (fightmanager.fightover && catchingfish != null)
        {
            if (!fightmanager.won && mybait != null)
            {
                Destroy(mybait);
                stopfishing();
            }
            else if (fightmanager.won &&
                     fightmanager.fishamount == 1)
            {
                resurfacewithfish(catchingfish);

                catchingfish.GetComponent<stats>().defeated = true;

                catchingfish
                    .GetComponent<fishscript>()
                    .chasingplayer = false;
            }
            else if (!fightmanager.won &&
                     fightmanager.fishamount == 1)
            {
                stopfishing();

                catchingfish
                    .GetComponent<fishscript>()
                    .chasingplayer = false;
            }


            catchingfish = null;
            fightmanager.fightover = false;
        }


        if (fishing)
        {
            rb.gravityScale = 0.05f;
        }
        else if (waiting)
        {
            rb.gravityScale = 0f;
        }
        else
        {
            rb.gravityScale = 0.35f;
        }
    }


    private void FixedUpdate()
    {
        if (!fishing && !waiting)
        {
            rb.AddTorque(-x * rolltorque);


            Vector2 v = rb.linearVelocity;


            if (Mathf.Abs(v.x) > maxspeed && !isgrappled)
            {
                v.x = Mathf.Sign(v.x) * maxspeed;
                rb.linearVelocity = v;
            }
        }


        if (fishing)
        {
            rb.MoveRotation(
                rb.rotation -
                x * rotationspeed * Time.fixedDeltaTime
            );


            if (fishing)
            {
                if (Input.GetButton("Jump") &&
                    usableboost > 0)
                {
                    usableboost -=
                        1 * Time.fixedDeltaTime;

                    rb.AddForce(
                        transform.up * boostpower,
                        ForceMode2D.Force
                    );
                }


                if (Input.GetKey(KeyCode.W))
                {
                    rb.AddForce(
                        transform.up * reelspeed * 0.5f,
                        ForceMode2D.Force
                    );
                }


                if (Input.GetKey(KeyCode.S))
                {
                    rb.AddForce(
                        -transform.up * reelspeed * 0.5f,
                        ForceMode2D.Force
                    );
                }
            }


            if (joint.distance >= ropelenght)
                joint.enabled = true;
            else
                joint.enabled = false;


            joint.distance =
                Mathf.Clamp(
                    joint.distance,
                    1f,
                    ropelenght
                );
        }


        if (isgrappled)
        {
            Vector2 offset =
                (Vector2)transform.position -
                grapplepoint;


            if (offset.magnitude > ropelenght)
            {
                offset =
                    offset.normalized *
                    ropelenght;

                transform.position =
                    grapplepoint + offset;


                Vector2 outward =
                    Vector2.Dot(
                        rb.linearVelocity,
                        offset.normalized
                    ) *
                    offset.normalized;


                if (Vector2.Dot(
                    outward,
                    offset.normalized) > 0)
                {
                    rb.linearVelocity -= outward;
                }
            }
        }
    }


    private bool isgrounded()
    {
        return rb.IsTouchingLayers(ground);
    }


    public bool iswater()
    {
        return waterCollider.bounds.Contains(transform.position);
    }


    public bool ispool()
    {
        return rb.IsTouchingLayers(poolwater);
    }


    private void flip()
    {
        if (isfacingright && x < 0f ||
            !isfacingright && x > 0f)
        {
            isfacingright = !isfacingright;

            Vector3 localscale =
                transform.localScale;

            localscale.x *= -1;

            transform.localScale =
                localscale;
        }
    }


    void rope()
    {
        grapplepoint =
            cannonlinepos.transform.position;

        isgrappled = true;


        joint.connectedAnchor =
            grapplepoint;

        joint.autoConfigureDistance =
            false;

        joint.frequency =
            springfreq;

        joint.dampingRatio =
            springdamp;

        joint.distance =
            ropelenght;

        joint.enabled =
            false;


        lr.enabled = true;

        lr.startColor =
            Color.white;

        lr.endColor =
            Color.white;


        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;


        transform.position = new Vector2(
            cannonhookpos.transform.position.x,
            cannonhookpos.transform.position.y
        );


        transform.rotation =
            Quaternion.Euler(
                0,
                0,
                cannonbarrel.transform.rotation.eulerAngles.z - 90
            );


        currentAngle =
            cannonbarrel.transform.eulerAngles.z;


        if (currentAngle > 180f)
            currentAngle -= 360f;
    }


    public void stopfishing()
    {
        fishing = false;

        transform.position =
            new Vector2(0, 0);

        joint.enabled = false;

        isgrappled = false;

        lr.enabled = false;

        transform.rotation =
            Quaternion.identity;

        rb.linearVelocity =
            new Vector2(0, 0);

        rb.constraints =
            RigidbodyConstraints2D.None;

        rb.bodyType =
            RigidbodyType2D.Dynamic;
    }


    void drop()
    {
        fishpos.GetChild(0)
            .gameObject
            .GetComponent<CapsuleCollider2D>()
            .enabled = true;


        fishpos.GetChild(0)
            .GetComponent<fishscript>()
            .enabled = true;


        fishpos.GetChild(0)
            .GetComponent<Rigidbody2D>()
            .bodyType =
            RigidbodyType2D.Dynamic;


        fishpos.GetChild(0).parent = null;
    }


    public void respawn()
    {
        transform.position =
            new Vector3(0, 0, 0);

        transform.rotation =
            Quaternion.identity;
    }


    public void resurfacewithfish(GameObject fish)
    {
        CapsuleCollider2D col =
            fish.GetComponent<CapsuleCollider2D>();

        fishscript fishscript =
            fish.GetComponent<fishscript>();

        stats fishstats =
            fish.GetComponent<stats>();


        if (col == null || fish == null)
            return;


        stopfishing();


        fish.GetComponent<Rigidbody2D>().bodyType =
            RigidbodyType2D.Kinematic;

        fishstats.defeated = true;

        fish.transform.parent =
            fishpos;

        fish.transform.localPosition =
            new Vector3(0, 0, 0);
    }


    public void upgraderopelenght(float amount)
    {
        ropelenght = amount;
    }


    public void upgradecannonpower(float amount)
    {
        cannonlaunchforce = amount;
    }


    public void upgradeboostamount(float amount)
    {
        totalboost = amount;
    }


    public void upgradereelspeed(float amount)
    {
        reelspeed = amount;
    }


    public void upgradecatchradius(float amount)
    {
        pickupradius = amount;
    }
}