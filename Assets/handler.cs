using System.Collections;
using System.Collections.Generic;
using TMPro;
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


    void Start()
    {
        playerscript = player.GetComponent<playerscript>();
        parent = new GameObject();
        parent.name = "fishparent";
        for (int i = 0; i < fishCount; i++)
        {
            SpawnRandom();
        }

        StartCoroutine(CheckFish());
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
        if (playerscript.fishing && player.transform.position.y < 5)
        {
            dephttext.text = "Depth:" + Mathf.FloorToInt((ground.transform.position.y + (ground.transform.localScale.y)) - player.transform.position.y).ToString();
        }
        else
        {
            dephttext.text = "Depth: ???";
        }

    }
}