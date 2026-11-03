using Cainos.Common;
using Unity.VisualScripting;
using UnityEngine;

public class projectilescript : MonoBehaviour
{
    Rigidbody2D rb;
    public float shotspeed;
    public float damage;
    public GameObject parent;

    public LayerMask wallLayer = 1 << 9;

    private int maxBounces = 2;
    private int bounceCount = 0;
    private Vector2 lastVelocity;

    private FishAI fishai;
    private PlayerCombat playercombat;

    private bool lifesteal;
    private float lifestealamount;

    private bool freeze;
    private float freezeduration;

    void Awake()
    {
        transform.GetChild(0).AddComponent<PolygonCollider2D>();
        transform.GetChild(0).gameObject.layer = 22;
        rb = gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.linearDamping = 0f;
        rb.angularDamping = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.freezeRotation = true;
        GetComponentInChildren<SpriteRenderer>().color = Color.white;
    }

public float lifetime = 20f;

void Start()
{
    rb.linearVelocity = transform.right * shotspeed;
    lastVelocity = rb.linearVelocity;

    Destroy(gameObject, lifetime);
}

public void Initialize(GameObject projectileParent)
{
    parent = projectileParent;
    stats fishstats = parent.GetComponent<stats>();

    lifesteal = fishstats.lifesteal;
    lifestealamount = fishstats.lifestealamount;
    freeze = fishstats.freezer;
    freezeduration = fishstats.freezeduration;
    playercombat = parent.GetComponent<PlayerCombat>();
    fishai = parent.GetComponent<FishAI>();
}

void FixedUpdate()
{
    Vector2 v = rb.linearVelocity;

    if (v.sqrMagnitude > 0.0001f)
    {
        // Lock speed no matter what the physics engine did to us
        v = v.normalized * shotspeed;
        rb.linearVelocity = v;

        float angle = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
        rb.MoveRotation(angle);
    }
    else
    {
        // Got stopped dead (e.g. head-on collision), keep flying the way we were
        v = lastVelocity;
        rb.linearVelocity = v;
    }

    lastVelocity = v;
}

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject == parent) return;

        PlayerCombat pc = collision.GetComponent<PlayerCombat>();
        if (pc == null) return;

        if (playercombat.enabled && lifesteal)
        {
            StartCoroutine(playercombat.heal(damage * lifestealamount));
        }
        else if(fishai.enabled && lifesteal)
        {

            StartCoroutine(fishai.heal(damage * lifestealamount));
        }

        if (pc.enabled)
        {
            if (freeze)
            {
                pc.Freeze(freezeduration);
            }
            pc.fighthealth -= damage;
            Destroy(gameObject);
        }
        else
        {
            FishAI fish = collision.GetComponent<FishAI>();
            if (fish != null) fish.health -= damage;
            if (freeze)
            {
                fish.Freeze(freezeduration);
            }
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Use the layer of the collider that was actually hit
        int hitLayer = collision.collider.gameObject.layer;


        if (((1 << hitLayer) & wallLayer.value) == 0)
            return;

        if (bounceCount >= maxBounces)
        {
            Destroy(gameObject);
            return;
        }

        Vector2 normal = collision.GetContact(0).normal;

        if (Vector2.Dot(lastVelocity, normal) >= 0f)
            return;

        Vector2 newVelocity = Vector2.Reflect(lastVelocity, normal).normalized * shotspeed;
        rb.linearVelocity = newVelocity;
        lastVelocity = newVelocity; // so a second contact in the same step (corners) uses the new direction

        bounceCount++;
    }
}