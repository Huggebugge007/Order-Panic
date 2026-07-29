using System.Collections;
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
    void Awake()
    {
        attackreloadtime = 1f;
        Weapon = transform.Find("weapon").gameObject;
        weaponhitbox = Weapon.GetComponent<BoxCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        rb.linearDamping = 0.7f;
        rb.angularDamping = 50f;
        if (gameObject.GetComponent<stats>() != null)
        {
            usingfish = true;
            maxSpeed = gameObject.GetComponent<stats>().speed;
            moveforce = 20f;
            rotationspeed = 200f;
            stabdir = Vector3.right;
            fighthealth = gameObject.GetComponent<stats>().health;
        }
        else
        {
            usingfish = false;
            stabdir = Vector3.up;
            fighthealth = 100f;
        }
        nexattacktime = Time.time + attackreloadtime;
        healthbar = gameObject.GetComponentInChildren<floatinghealthbar>();
        maxhealth = fighthealth;
    }
    void FixedUpdate()
    {
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
            nexattacktime = Time.time + attackreloadtime;
            StartCoroutine(attack());
        }
        if(fighthealth <= 0)
        {
            this.enabled = false;
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
}