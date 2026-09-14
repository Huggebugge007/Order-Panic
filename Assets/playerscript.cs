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
    public GameObject cannonhookpos,cannonlinepos;
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


        if (!fishing && Input.GetKeyDown(KeyCode.E) && Vector2.Distance(transform.position,cannon.transform.position) < cannonatatchdistance)
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
            transform.position = new Vector2(cannonhookpos.transform.position.x, cannonhookpos.transform.position.y);


            rb.MoveRotation(cannonbarrel.transform.rotation.eulerAngles.z - 90);

            if (Input.GetButtonDown("Jump"))
            {
                waiting = false;
                rb.angularVelocity = 0f;
                rb.AddForce(-transform.up * cannonlaunchforce, ForceMode2D.Impulse);
            }
            currentAngle += -x * cannonrotationspeed * Time.deltaTime;
            currentAngle = Mathf.Clamp(currentAngle, -54, 20);
            cannonbarrel.transform.rotation = Quaternion.Euler(0, 0, currentAngle);
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


        Collider2D [] fishCols = Physics2D.OverlapCapsuleAll(fishpickuppoint.position, new Vector2(pickupradius*0.25f,pickupradius*0.65f), CapsuleDirection2D.Vertical,0f,fishlayer);

        Collider2D fishCol = null;
        float closestDist = float.MaxValue;

        foreach (Collider2D hit in fishCols)
        {
            float dist = (hit.transform.position - transform.position).sqrMagnitude;

            if (dist < closestDist)
            {
                closestDist = dist;
                fishCol = hit;
            }
        }

        if (fishCol != null && Input.GetKeyDown(KeyCode.F) && fishpos.transform.childCount <= 1)
        {
            if (!fishCol.GetComponent<stats>().defeated)
            {
                fishCol.GetComponent<fishscript>().chasingplayer = true;

                if (fishpos.childCount == 0)
                {
                    rb.constraints = RigidbodyConstraints2D.FreezePosition;
                    catchingfish = fishCol.GetComponent<stats>().gameObject;
                    fightmanager.changescene(1);
                    this.enabled = false;
                }
                else
                {
                    rb.constraints = RigidbodyConstraints2D.FreezePosition;
                    catchingfish = fishCol.GetComponent<stats>().gameObject;
                    mybait = fishpos.GetChild(0).gameObject;
                    fightmanager.changescene(1);
                    this.enabled = false;
                }
            }
            else if (fishpos.childCount < 2 && fishCol.GetComponent<stats>().defeated)
            {
                GameObject fish = fishCol.GetComponent<stats>().gameObject;
                BoxCollider2D col = fish.GetComponent<BoxCollider2D>();
                fishscript fishscript = fish.GetComponent<fishscript>();

                if (col == null || fish == null) return;

                fish.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
                fish.transform.parent = fishpos;
                fish.transform.position = fishpos.position;
            }
        }

        if (fightmanager.fightover && catchingfish != null)
        {
            if (!fightmanager.won && mybait != null)
            {
                Destroy(mybait);
                stopfishing();
            }
            else if (fightmanager.won && fightmanager.fishamount == 1)
            {
                resurfacewithfish(catchingfish);
                catchingfish.GetComponent<stats>().defeated = true;
                catchingfish.GetComponent<fishscript>().chasingplayer = false;
            }
            else if (!fightmanager.won && fightmanager.fishamount == 1)
            {
                stopfishing();
                catchingfish.GetComponent<fishscript>().chasingplayer = false;
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
            rb.MoveRotation(rb.rotation - x * rotationspeed * Time.fixedDeltaTime);

            if (fishing)
            {
                
                if (Input.GetButton("Jump") && usableboost > 0)
                {
                    usableboost -= 1 * Time.fixedDeltaTime;
                    rb.AddForce(transform.up * boostpower, ForceMode2D.Force);
                }
                if (Input.GetKey(KeyCode.W))
                {
                    rb.AddForce(transform.up * reelspeed * 0.5f, ForceMode2D.Force);
                }


                if (Input.GetKey(KeyCode.S))
                {
                    rb.AddForce(-transform.up * reelspeed * 0.5f, ForceMode2D.Force);
                }
            }



            if (joint.distance >= ropelenght)
                joint.enabled = true;
            else
                joint.enabled = false;

            joint.distance = Mathf.Clamp(joint.distance, 1f, ropelenght);
        }

        if (isgrappled)
        {
            Vector2 offset = (Vector2)transform.position - grapplepoint;

            if (offset.magnitude > ropelenght)
            {
                offset = offset.normalized * ropelenght;

                transform.position = grapplepoint + offset;

                Vector2 outward = Vector2.Dot(rb.linearVelocity, offset.normalized) * offset.normalized;

                if (Vector2.Dot(outward, offset.normalized) > 0)
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
        if (isfacingright && x < 0f || !isfacingright && x > 0f)
        {
            isfacingright = !isfacingright;
            Vector3 localscale = transform.localScale;
            localscale.x *= -1;
            transform.localScale = localscale;
        }
    }

    void rope()
    {
        grapplepoint = cannonlinepos.transform.position;
        isgrappled = true;
        joint.connectedAnchor = grapplepoint;
        joint.autoConfigureDistance = false;
        joint.frequency = springfreq;
        joint.dampingRatio = springdamp;
        joint.distance = ropelenght;
        joint.enabled = false;
        lr.enabled = true;
        lr.startColor = Color.white;
        lr.endColor = Color.white;

        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        transform.position = new Vector2(cannonhookpos.transform.position.x,cannonhookpos.transform.position.y);
        transform.rotation = Quaternion.Euler(0, 0, cannonbarrel.transform.rotation.eulerAngles.z - 90);
        currentAngle = cannonbarrel.transform.eulerAngles.z;


        if (currentAngle > 180f)
            currentAngle -= 360f;


    }

    public void stopfishing()
    {
        fishing = false;
        transform.position = new Vector2(0,0);
        joint.enabled = false;
        isgrappled = false;
        lr.enabled = false;
        transform.rotation = Quaternion.identity;
        rb.linearVelocity = new Vector2(0, 0);
        rb.constraints = RigidbodyConstraints2D.None;
        rb.bodyType = RigidbodyType2D.Dynamic;
    }

    void drop()
    {
        fishpos.GetChild(0).gameObject.GetComponent<BoxCollider2D>().enabled = true;
        fishpos.GetChild(0).GetComponent<fishscript>().enabled = true;
        fishpos.GetChild(0).GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;

        fishpos.GetChild(0).parent = null;
    }

    public void respawn()
    {
        transform.position = new Vector3(0, 0, 0);
        transform.rotation = Quaternion.identity;
    }

    public void resurfacewithfish(GameObject fish)
    {
        BoxCollider2D col = fish.GetComponent<BoxCollider2D>();
        fishscript fishscript = fish.GetComponent<fishscript>();
        stats fishstats = fish.GetComponent<stats>();

        if (col == null || fish == null) return;

        stopfishing();
        fish.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
        fishstats.defeated = true;
        fish.transform.parent = fishpos;
        fish.transform.localPosition = new Vector3(0, 0, 0);
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