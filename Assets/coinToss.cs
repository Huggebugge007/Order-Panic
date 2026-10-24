using System.Collections;
using UnityEngine;

public class coinToss : MonoBehaviour
{
    private Rigidbody2D rb;
    public float tossforce;
    public float spinforce;
    public bool ready = false;
    public bool reseting = false;
    public Vector2 resetpos;
    public float resetDuration = 0.5f;
    public bool landed = false;

    public bool gold = false;




    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

    }


    void Update()
    {

    }

    [ContextMenu("Toss Coin")]
    public IEnumerator TossAndWait(bool wantGold, System.Action<bool> result)
    {
        ready = false;
        landed = false;

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        rb.AddForce(
            new Vector2(0, tossforce * Random.Range(0.8f, 1.2f)),
            ForceMode2D.Impulse
        );

        float direction = Random.value < 0.5f ? -1f : 1f;

        rb.AddTorque(
            direction * spinforce * Random.Range(0.8f, 1.2f),
            ForceMode2D.Impulse
        );


        yield return new WaitForSeconds(0.5f);

        yield return new WaitForSeconds(0.5f);

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

        determinewinner();
        landed = true;


        bool won = gold == wantGold;
        result(won);

        yield return new WaitForSeconds(1f);

        yield return StartCoroutine(ResetCoin());
    }






    private IEnumerator ResetCoin()
    {
        reseting = true;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        float startRotation = rb.rotation;
        float endRotation = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;
        float elapsedTime = 0f;
        Vector2 startpos = transform.position;
        while (elapsedTime < resetDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / resetDuration);
            t = Mathf.SmoothStep(0f, 1f, t);

            rb.position = Vector2.Lerp(startpos, resetpos, t);
            rb.rotation = Mathf.LerpAngle(startRotation, endRotation, t);
            yield return null;
        }
        rb.position = resetpos;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        ready = true;
        reseting = false;

    }


    public void determinewinner()
    {
        float angle = Mathf.Repeat(rb.rotation, 360f);


        gold = angle < 90f || angle >= 270f;
    }
}
