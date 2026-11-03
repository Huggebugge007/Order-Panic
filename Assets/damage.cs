
using System.Collections;
using UnityEngine;

public class damage : MonoBehaviour
{
    public float weapondamage;
    BoxCollider2D col;

    private bool venemous = false;
    private bool lifesteal = false;
    private float lifestealamount;

    private bool freeze;
    private float freezeduration;

    private FishAI fishai;
    private PlayerCombat playercombat;
    private stats fishstats;

    void Awake()
    {
        col = GetComponent<BoxCollider2D>();

        fishstats = GetComponentInParent<stats>();

        if (fishstats != null)
        {
            weapondamage = fishstats.damage;
            venemous = fishstats.venemousweapon;
            lifesteal = fishstats.lifesteal;
            lifestealamount = fishstats.lifestealamount;
            freeze = fishstats.freezer;
            freezeduration = fishstats.freezeduration;

            fishai = fishstats.GetComponent<FishAI>();
            playercombat = fishstats.GetComponent<PlayerCombat>();
        }
        else
        {
            weapondamage = 20;
        }

        if (venemous)
        {
            SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();

            if (sr != null)
                sr.color = Color.green;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerCombat pc = collision.GetComponent<PlayerCombat>();

        if (pc != null && pc.enabled)
        {
            pc.fighthealth -= weapondamage;
            if (freeze)
            {
                pc.Freeze(freezeduration);
            }

            if (lifesteal)
            {
                if (playercombat != null && playercombat.enabled)
                {
                    playercombat.fighthealth += weapondamage;
                }
                else if (fishai != null && fishai.enabled)
                {
                    fishai.health += weapondamage;
                }
            }

            if (venemous)
            {
                StartCoroutine(pc.poision(weapondamage));
            }

            col.enabled = false;
            return;
        }

        FishAI fa = collision.GetComponent<FishAI>();

        if (fa != null && fa.enabled)
        {
            fa.health -= weapondamage;

            if (freeze)
            {
                fa.Freeze(freezeduration);
            }

            if (lifesteal)
            {
                if (playercombat != null && playercombat.enabled)
                {
                    StartCoroutine(playercombat.heal(weapondamage*lifestealamount));
                }
                else if (fishai != null && fishai.enabled)
                {
                    StartCoroutine(fishai.heal(weapondamage*lifestealamount));
                }
            }

            if (venemous)
            {
                StartCoroutine(fa.poision(weapondamage));
            }

            col.enabled = false;
        }
    }
}
