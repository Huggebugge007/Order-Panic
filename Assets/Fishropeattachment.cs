
using System.Collections;
using UnityEngine;

public class Fishropeattachment : MonoBehaviour
{
    public dropscript rope;

    private Rigidbody2D rb;
    private HingeJoint2D ropeJoint;

    public bool ontable;

    public handlerforgambler handlerforgambler;
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {

    }

    public void AttachFish()
    {
        if (rope == null || rope.LastBody == null)
        {
            return;
        }

        ropeJoint = gameObject.AddComponent<HingeJoint2D>();

        ropeJoint.connectedBody = rope.LastBody;


        ropeJoint.anchor = new Vector2(0f, 0f);


        ropeJoint.autoConfigureConnectedAnchor = false;
        ropeJoint.connectedAnchor = Vector2.zero;

        ropeJoint.enableCollision = false;
    }

    public void DropFish()
    {
        if (ropeJoint == null)
            return;

        Destroy(ropeJoint);
        ropeJoint = null;
        StartCoroutine(checkifdone());
    }


    public IEnumerator checkifdone()
    {
        float stillTime = 0f;
        float requiredStillTime = 0.5f;

        while (stillTime < requiredStillTime)
        {
            bool isStill =
                rb.linearVelocity.magnitude < 0.03f &&
                Mathf.Abs(rb.angularVelocity) < 2f;

            if (isStill)
            {
                stillTime += Time.deltaTime;
            }
            else
            {
                stillTime = 0f;
            }

            yield return null;
        }
        yield return new WaitForSeconds(0.6f);
        if (ontable)
        {
            handlerforgambler.nextlevel();
        }
        else
        {
            handlerforgambler.lost();
        }
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Table"))
        {
            ontable = true;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Table"))
        {
            ontable = false;
        }
    }
}
