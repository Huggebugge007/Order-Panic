using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class breeding : MonoBehaviour
{
    float waittime = 2f;
    float timer = 0f;
    bool countingdown = false;
    stats[] parents = new stats[2];
    public Transform fishpos;
    private bool wascountingdown = false;

    public float breedcooldown = 60f;

    public GameObject censorblock;

    public bool isBreeding = false;

    public float variation = 0.2f;
    void Start()
    {
        timer = waittime;
    }

    void Update()
    {
        if(fishonbed.Count != 2)
        {
            countingdown = false;
            timer = waittime;
        }
        if(countingdown && !wascountingdown)
        {
            timer = waittime;
        }
        wascountingdown = countingdown;

        if (fishonbed.Count == 2)
        {
            for (int i = 0; i < 2; i++)
            {
                 parents[i] = fishonbed[i].GetComponent<fishscript>().stats;
            }
            if (fishonbed[0].GetComponent<fishscript>().breedtimer <= 0f && fishonbed[1].GetComponent<fishscript>().breedtimer <= 0f)
            {
                if(fishonbed[0].transform.parent != fishpos && fishonbed[1].transform.parent != fishpos)
                {
                    countingdown = true;
                }
            }
        }
        if(countingdown)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                StartCoroutine(breed(parents));
                countingdown = false;
                timer = waittime;
            }
        }
    }
    IEnumerator breed(stats[] parents)
    {
        isBreeding = true;
        GameObject block = Instantiate(censorblock, new Vector2(1, 784), Quaternion.identity);

        Rigidbody2D rb = block.GetComponent<Rigidbody2D>();

        foreach (stats parent in parents)
        {
            fishscript fish = parent.GetComponent<fishscript>();
            fish.breedtimer = breedcooldown;
        }

        yield return new WaitForSeconds(4f);


        GameObject main = parents[Random.Range(0,2)].gameObject;
        GameObject child = Instantiate(main,new Vector2(1, 779.3f), Quaternion.identity);
        stats stats = child.GetComponent<stats>();

        stats.size = stats.size * Random.Range(0.9f, 1.1f);
        child.transform.localScale = new Vector2(stats.size, stats.size);
        stats.weight = stats.weight * Random.Range(1 - variation, 1 + variation);

        stats.speed = parents[Random.Range(0, 2)].speed * Random.Range(1 - variation, 1 + variation);
        stats.damage = parents[Random.Range(0, 2)].damage * Random.Range(1 - variation, 1 + variation);
        stats.health = parents[Random.Range(0, 2)].health * Random.Range(1 - variation, 1 + variation);

        stats.reactionMax = parents[Random.Range(0, 2)].reactionMax * Random.Range(1 - variation, 1 + variation);
        stats.evadetime = parents[Random.Range(0, 2)].evadetime * Random.Range(1 - variation, 1 + variation);
        stats.aggression = parents[Random.Range(0, 2)].aggression * Random.Range(1 - variation, 1 + variation);
        stats.idlemiddletime = parents[Random.Range(0, 2)].idlemiddletime * Random.Range(1 - variation, 1 + variation);

        stats.defeated = true;

        yield return new WaitForSeconds(8f);
        Collider2D col = block.GetComponent<Collider2D>();
        col.isTrigger = true;
        isBreeding = false;
        yield return new WaitUntil(() => block.transform.position.y < 700);
        Destroy(block);


    }


    public List<GameObject> fishonbed = new List<GameObject>();

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("fish"))
        {
            fishonbed.Add(other.gameObject);
            fishscript fishscript = other.GetComponent<fishscript>();
            fishscript.countingdown = false;

        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("fish"))
        {
            fishonbed.Remove(other.gameObject);
            fishscript fishscript = other.GetComponent<fishscript>();
            fishscript.countingdown = true;
            fishscript.countdown = fishscript.timetildry;
        }
    }

}
