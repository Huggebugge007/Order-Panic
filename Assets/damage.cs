using UnityEngine;

public class damage : MonoBehaviour
{
    public float weapondamage;
    BoxCollider2D col;
    void Awake()
    {
        if (transform.parent.GetComponent<stats>() != null)
            weapondamage = transform.parent.GetComponent<stats>().damage;
        else weapondamage = 10;
        col = GetComponent<BoxCollider2D>();
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<PlayerCombat>() != null)
        {
            if (collision.gameObject.GetComponent<PlayerCombat>().enabled)
            {
                collision.gameObject.GetComponent<PlayerCombat>().fighthealth -= weapondamage;
                col.enabled = false;
            }
            else if (!collision.gameObject.GetComponent<PlayerCombat>().enabled)
            {
                collision.gameObject.GetComponent<FishAI>().health -= weapondamage;
                col.enabled = false;
            }
        }
    }
}
