using System.Collections;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public bool isAttacking;
    public Rigidbody2D rb;
    public float moveforce;
    public float maxSpeed = 8f;
    public float rotationspeed;
    Vector3 movedir;
    Vector3 stabdir;
    bool usingfish;
    public float attackreloadtime;
    float nexattacktime;
    GameObject Weapon;
    BoxCollider2D weaponhitbox;
    public float fighthealth;
    floatinghealthbar healthbar;
    float maxhealth;

    private SpriteRenderer []sprites;

    private float previoushealth;

    bool shooter;

    float shotspeed;
    stats fishstats;
    
    public bool posioned = false;

    private float regentimer;
    Coroutine regenCoroutine;

    private bool frozen;
    private Coroutine freezeRoutine;
    public bool IsFrozen => frozen;

    void Awake()
    {

        sprites = GetComponentsInChildren<SpriteRenderer>();
        attackreloadtime = 1f;
        Weapon = transform.Find("weapon").gameObject;
        weaponhitbox = Weapon.GetComponent<BoxCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        rb.linearDamping = 0.2f;
        rb.angularDamping = 10f;
        if (gameObject.GetComponent<stats>() != null)
        {
            fishstats = gameObject.GetComponent<stats>();
            usingfish = true;
            maxSpeed = fishstats.speed * 0.3f;
            moveforce = 20f;
            rotationspeed = 180f;
            stabdir = Vector3.right;
            fighthealth = fishstats.health;
            attackreloadtime = fishstats.reloadtime;
            shooter = fishstats.shooter;
            shotspeed = fishstats.shotspeed;
            if(fishstats.reloadtime <= 0)
            {
                attackreloadtime = 2f;
            }
             if (fishstats.shooter)
            {
                Destroy(Weapon.GetComponent<damage>());
            }
        }
        else
        {
            usingfish = false;
            stabdir = Vector3.up;
            fighthealth = 100f;
            shooter = false;
            attackreloadtime = 1f;
        }

        nexattacktime = Time.time + attackreloadtime;
        healthbar = gameObject.GetComponentInChildren<floatinghealthbar>();
        maxhealth = fighthealth;
        previoushealth = fighthealth;
       
    }
   void FixedUpdate()
{
    if (frozen)
    {
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        return;   // no movement force, no rotation
    }

    if (usingfish)
    {
        movedir = transform.right;
    }
    else
    {
        movedir = transform.up;
    }

    float vertical = Input.GetAxis("Vertical");
    rb.AddForce(movedir * vertical * moveforce);

    if (rb.linearVelocity.magnitude > maxSpeed)
    {
        rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
    }

    float horizontal = Input.GetAxis("Horizontal");
    transform.Rotate(0, 0, -horizontal * rotationspeed * Time.deltaTime);
}
    private void Update()
    {
        if (healthbar != null)
        {
            healthbar.updatehealthbar(fighthealth, maxhealth);
        }
        if (healthbar == null)
        {
            healthbar = gameObject.GetComponentInChildren<floatinghealthbar>();
        }

        
        if (Time.time > nexattacktime && Input.GetKeyDown(KeyCode.Space))
        {
            if (!shooter)
            {
                nexattacktime = Time.time + attackreloadtime;
                StartCoroutine(attack());
            }
            else if (shooter)
            {
                nexattacktime = Time.time + attackreloadtime;
                shoot(fishstats.shotsatatime);
            }

        }


        if(fighthealth <= 0)
        {
            this.enabled = false;
            foreach(SpriteRenderer sprite in sprites)
            {
                sprite.color = Color.red;
            }
        }

        if (fighthealth <= 0)
{
    this.enabled = false;

    foreach (SpriteRenderer sprite in sprites)
    {
        sprite.color = Color.red;
    }

    if (regenCoroutine != null)
    {
        StopCoroutine(regenCoroutine);
        regenCoroutine = null;
    }

    return;
}

// Detect health changes
    if (fighthealth < previoushealth && !posioned)
    {
            if (!frozen)
            {
                StartCoroutine(flashred(Color.white));
            }
            else
            {
                StartCoroutine(flashred(new Color(0/ 255f, 210/255f, 1f)));
            }
        

        regentimer = 0f;

        if (regenCoroutine != null)
        {
            StopCoroutine(regenCoroutine);
            regenCoroutine = null;
        }
    }

    // Wait 5 seconds after damage before regenerating
    if (!posioned && fighthealth < maxhealth && regentimer >= 5f)
    {
        if (regenCoroutine == null)
        {
            regenCoroutine = StartCoroutine(regen());
        }
    }

    regentimer += Time.deltaTime;
    previoushealth = fighthealth;
        
    }

    IEnumerator regen()
{
    while (fighthealth < maxhealth && !posioned)
    {
        foreach (SpriteRenderer sprite in sprites)
        {
            sprite.color =new Color (34/255, 255/255, 0);
        }

        if (fighthealth < maxhealth && !posioned)
        {
            fighthealth = Mathf.Min(
                fighthealth + maxhealth * 0.02f,
                maxhealth
            );
        }

        yield return new WaitForSeconds(0.3f);



        foreach (SpriteRenderer sprite in sprites)
        {
            sprite.color = Color.white;
        }
        yield return new WaitForSeconds(0.7f);
    }

    regenCoroutine = null;
}

    IEnumerator flashred(Color originalcolor)
    {
        foreach(SpriteRenderer sprite in sprites)
        {
            sprite.color = Color.red;
        }


        yield return new WaitForSeconds(0.3f);

        foreach(SpriteRenderer sprite in sprites)
        {
            sprite.color = originalcolor;
        }
    
    }

    IEnumerator attack()
    {
        if (Weapon != null)
        {
            weaponhitbox.enabled = true;

            Vector3 startPos = Weapon.transform.localPosition;
            Vector3 forwardPos = startPos + stabdir * 0.18f;

            float duration = 1f;
            float t = 0;

            while (t < duration)
            {
                float half = duration / 2f;

                if (t < half)
                {
                    Weapon.transform.localPosition = Vector3.Lerp(startPos, forwardPos, t / half);
                }
                else
                {
                    Weapon.transform.localPosition = Vector3.Lerp(forwardPos, startPos, (t - half) / half);
                }

                t += Time.deltaTime;
                yield return null;
            }


            Weapon.transform.localPosition = startPos;

            weaponhitbox.enabled = false;
        }
    }

    void shoot(int shotsatatime)
    {
        if (Weapon != null)
        {
            if (!fishstats.burst)
            {
                float step = 360f / shotsatatime;

                for(int i = 0; i < shotsatatime; i++)
                {
                    float angle = transform.eulerAngles.z + step * i;

                    GameObject projectile = Instantiate(Weapon,transform.position,Quaternion.Euler(0f, 0f, angle));

                    weaponhitbox = projectile.GetComponent<BoxCollider2D>();
                    projectilescript projectilescript = projectile.AddComponent<projectilescript>();
                    projectilescript.damage = fishstats.damage;
                    projectilescript.shotspeed = shotspeed;
                    projectilescript.Initialize(gameObject);
                    weaponhitbox.enabled = true;
                }
            }
            else
            {
                StartCoroutine(burstshots(fishstats.shotsatatime, fishstats.bursttime));
            }
            

        }
    }
    IEnumerator burstshots(int shots, float delay)
    {
        for(int i = 0; i < shots; i++)
        {
                    GameObject projectile = Instantiate(Weapon,transform.position,transform.rotation);

                    weaponhitbox = projectile.GetComponent<BoxCollider2D>();
                    projectilescript projectilescript = projectile.AddComponent<projectilescript>();
                    projectilescript.damage = fishstats.damage;
                    projectilescript.shotspeed = shotspeed;
                    projectilescript.Initialize(gameObject);
                    weaponhitbox.enabled = true;
                    yield return new WaitForSeconds(delay);
        }
        yield return null;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(!isActiveAndEnabled)
            return;


        if(collision.gameObject.tag == "fish")
        {
            if (collision.gameObject.GetComponent<stats>().poisnous)
            {
                if (!posioned)
                {
                    StartCoroutine(poision(collision.gameObject.GetComponent<stats>().damage));
                }
            }
        }
    }
    public IEnumerator poision(float poisondamage)
    {
        posioned = true;
  
        for(int i = 0;i < 5;i++)
        {
            foreach(SpriteRenderer sprite in sprites)
            {
                sprite.color = new Color(6/255,96/255,31/255);
            }
            yield return new WaitForSeconds(1f);
            fighthealth -= poisondamage / 5;
            
            foreach(SpriteRenderer sprite in sprites)
            {
                sprite.color = Color.red;
            }
            yield return new WaitForSeconds(0.3f);
        }

        foreach(SpriteRenderer sprite in sprites)
        {
            sprite.color = Color.white;
        }
        posioned = false;
    }

    public IEnumerator heal(float amount)
    {

            foreach(SpriteRenderer sprite in sprites)
            {
                sprite.color =new Color (84/255, 255/255, 0);
            }
            fighthealth += amount;

            yield return new WaitForSeconds(0.3f);

            foreach(SpriteRenderer sprite in sprites)
            {
                sprite.color = Color.white;
            }

    }

    public void Freeze(float duration)
    {
        if (fighthealth <= 0) return;

        if (freezeRoutine != null)
            StopCoroutine(freezeRoutine);

        freezeRoutine = StartCoroutine(FreezeRoutine(duration));
    }

    IEnumerator FreezeRoutine(float duration)
    {
        frozen = true;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        foreach (SpriteRenderer sprite in sprites)
            sprite.color = new Color(0/ 255f, 210/255f, 1f);

        yield return new WaitForSeconds(duration);

        frozen = false;

        foreach (SpriteRenderer sprite in sprites)
            sprite.color = Color.white;

        freezeRoutine = null;
    }
}