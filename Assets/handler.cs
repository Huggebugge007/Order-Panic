using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class handler : MonoBehaviour
{
    public int fishCount = 1000;
    public Transform player;
    public float width = 500f;
    playerscript playerscript;
    public List<GameObject> fishList = new List<GameObject>();
    public List<GameObject> avaliblefish = new List<GameObject>();
    public TMP_Text dephttext;
    GameObject parent;
    public GameObject ground;
    public Transform duckspawn;
    public GameObject duckprefab;
    public List<Transform> fallpoints;
    public List<GameObject> avalibleseats = new List<GameObject>();
    public List<GameObject> seats = new List<GameObject>();

    public Transform tavernentrance, tavernexit;

    public GameObject market;
    public int resturantquality = 1;

    public float spawntime = 60f;
    public float usablespawntime;

    public int fishelinstars = 0;


    void Start()
    {
        usablespawntime = spawntime;
        playerscript = player.GetComponent<playerscript>();
        parent = new GameObject();
        parent.name = "fishparent";
        for (int i = 0; i < fishCount; i++)
        {
            SpawnRandom();
        }

        StartCoroutine(CheckFish());
    }


    public void findseats()
    {
        GameObject[] seatObjects = GameObject.FindGameObjectsWithTag("seat");
        seats = new List<GameObject>(seatObjects);
        seats.RemoveAll(s => s == null);

        foreach (GameObject seat in seats)
        {
            if (!avalibleseats.Contains(seat))
            {
                avalibleseats.Add(seat);
            }
        }
    }

    void SpawnRandom()
    {
        GameObject selected = GetRandomPrefab();
        float spawndepht = selected.GetComponent<stats>().spawndepht;
        Vector2 pos = new Vector2(Random.Range(-width / 2f, width * 1.5f), spawndepht + (Random.Range(-10,10)));


        bool found = false;
        for(int i = 0; i < 3; i++)
        {
            pos = new Vector2(Random.Range(-width / 2f, width * 1.5f), spawndepht + (Random.Range(-10, 10)));

            if(!(Mathf.Abs(pos.x) < ((Mathf.Abs(pos.y) * 2)+1)))
            {
                found = true;
            }
            if (found)
            {
                break;
            }
        }

        if (found)
        {
            GameObject fish = Instantiate(selected, pos, Quaternion.identity);
            fishList.Add(fish);
            fish.GetComponent<fishscript>().market = market;
            fish.transform.parent = parent.transform;
        }

        
    }

    IEnumerator CheckFish()
    {
        while (player != null && player.gameObject.activeInHierarchy)
        {
            for (int i = 0; i < fishList.Count; i++)
            {
                if (fishList[i] == null) continue;

                float sqrDist = (player.position - fishList[i].transform.position).sqrMagnitude;

                fishList[i].SetActive(sqrDist < 7 * 7);
            }

            yield return new WaitForSeconds(0.2f);
        }
    }

    GameObject GetRandomPrefab()
    {
        float totalWeight = 0f;

        foreach (GameObject p in avaliblefish)
            totalWeight += p.GetComponent<stats>().spawnweight;

        float random = Random.Range(0f, totalWeight);

        foreach (var p in avaliblefish)
        {
            float w = p.GetComponent<stats>().spawnweight;

            if (random < w)
                return p;

            random -= w;
        }

        return avaliblefish[0];
    }
    private void Update()
    {
        if(avalibleseats.Count > 0)
        {
            usablespawntime -= Time.deltaTime;
            if(usablespawntime <= 0)
            {
                spawnduck();
                usablespawntime = spawntime;
            }
        }
        if (playerscript.fishing && player.transform.position.y < 5)
        {
            dephttext.text = "Depth:" + Mathf.FloorToInt((ground.transform.position.y + (ground.transform.localScale.y)) - player.transform.position.y).ToString();
        }
        else
        {
            dephttext.text = "Depth:???";
        }

    }
    [ContextMenu("Spawn Duck")]
    private void spawnduck()
    {
        avalibleseats.RemoveAll(s => s == null); // clean stale/destroyed refs first

        if (avalibleseats.Count == 0)
        {
            Debug.LogWarning("No available seats to spawn duck.");
            return;
        }

        GameObject duckclone = Instantiate(duckprefab, duckspawn);
        int seatindex = Random.Range(0, avalibleseats.Count);
        duckscript duckclonescript = duckclone.GetComponent<duckscript>();
        duckclonescript.handler = this;
        duckclonescript.moneyhandler = gameObject.GetComponent<moneyhandler>();
        duckclonescript.seat = avalibleseats[seatindex];

        duckclonescript.tavernentrance = tavernentrance;
        duckclonescript.tavernexit = tavernexit;

        duckclonescript.targets.Add(tavernentrance);
        duckclonescript.targets.Add(avalibleseats[seatindex].transform.parent);
        duckclonescript.targets.Add(avalibleseats[seatindex].transform);

        avalibleseats.RemoveAt(seatindex);
    }

    public void upgradequality(int amount)
    {
        resturantquality = amount;
    }
    public void upgradespawnrate(int amount)
    {
        spawntime = amount;
    }
}