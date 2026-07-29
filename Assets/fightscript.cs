using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;


public class fightscript : MonoBehaviour
{
    public GameObject player, fish,bait;
    public int fishamount;
    List<Transform> spawns = new List<Transform>();
    bool usingbait;
    public Transform center;
    List<GameObject> fighters = new List<GameObject>();
    public float playerhealth = 100;
    public int fightersleft;
    public bool alive = true;
    public Image image;
    public TMP_Text countdowntext;
    bool fightover;
    public GameObject myplayerprefab;
    public GameObject healthbar;
    PlayerCombat playercombat;
    public bool started = false;
    bool initialized = false;
    public float countdowntimer;
    float timertime;


    private void Awake()
    {
        countdowntext.enabled = false;
        timertime = countdowntimer;
        started = false;
        initialized = false;
        player = GameObject.FindGameObjectWithTag("Player");
        fish = player.GetComponent<playerscript>().catchingfish;
        fightover = false;
        alive = true;
        fighters.Clear();
        spawns.Clear();
        fishamount = fightmanager.instance.fishamount;
        
        if (player.transform.Find("FIshpos").childCount > 0)
        {
            usingbait = true;
            bait = player.GetComponent<playerscript>().mybait;
        }
        else
        {
            usingbait = false;
        }

        GameObject[] objects = GameObject.FindGameObjectsWithTag("spawn");

        spawns.Clear();

        foreach (GameObject obj in objects)
        {
            spawns.Add(obj.transform);
        }

        if(fishamount > 1)
        {
            for(int i = 0; i < spawns.Count; i++)
            {
                int spawn = Random.Range(0, spawns.Count);
                GameObject enemy = Instantiate(fish, spawns[spawn].transform);
                enemy.transform.localPosition = Vector3.zero;
                enemy.transform.parent = null;
                enemy.transform.rotation = spawns[spawn].transform.rotation;
                enemy.transform.localScale = new Vector3(Mathf.Abs(enemy.transform.localScale.x), enemy.transform.localScale.y, enemy.transform.localScale.z);
                spawns.RemoveAt(spawn);
                //random stats for fish, based on the fishstats it gets feeded. Du ska alltså skapa en average fishstats och sen göra lite random bättre lite random sämmre.

                healthbar = Instantiate(healthbar, enemy.transform.position, Quaternion.identity);
                healthbar.transform.parent = enemy.transform;

                fighters.Add(enemy);
                fightersleft += 1;
            }
        }
        else
        {
            int spawn = Random.Range(0, spawns.Count);
            GameObject enemy = Instantiate(fish, spawns[spawn].transform);
            enemy.transform.localPosition = Vector3.zero;
            enemy.transform.parent = null;
            enemy.transform.rotation = spawns[spawn].transform.rotation;
            enemy.transform.localScale = new Vector3(Mathf.Abs(enemy.transform.localScale.x),enemy.transform.localScale.y,enemy.transform.localScale.z);
            spawns.RemoveAt(spawn);
            enemy.GetComponent<fishscript>().enabled = false;
            healthbar = Instantiate(healthbar,enemy.transform.position, Quaternion.identity);
            healthbar.transform.parent = enemy.transform;
            fighters.Add(enemy);
            fightersleft += 1;
        }
        if (usingbait)
        {
            GameObject myplayer = Instantiate(bait, spawns[0].transform);
            myplayer.transform.localPosition = Vector3.zero;
            myplayer.transform.parent = null;
            myplayer.transform.rotation = spawns[0].transform.rotation;
            myplayer.GetComponent<fishscript>().enabled = false;
            myplayer.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
            myplayer.GetComponent<BoxCollider2D>().enabled = true;
            healthbar = Instantiate(healthbar, myplayer.transform.position, Quaternion.identity);
            healthbar.transform.parent = myplayer.transform;
            fightersleft += 1;
            playercombat = myplayer.GetComponent<PlayerCombat>();
            myplayer.transform.localScale = new Vector3(Mathf.Abs(myplayer.transform.localScale.x), myplayer.transform.localScale.y, myplayer.transform.localScale.z);
        }
        else
        {
            GameObject myplayer = Instantiate(myplayerprefab, spawns[0].transform);
            myplayer.transform.parent = null;
            myplayer.transform.eulerAngles = spawns[0].transform.eulerAngles + new Vector3(0, 0, -90);
            healthbar = Instantiate(healthbar, myplayer.transform.position, Quaternion.identity);
            healthbar.transform.parent = myplayer.transform;
            fightersleft += 1;
            playercombat = myplayer.GetComponent<PlayerCombat>();
        }
        
    }

    private void Update()
    {
        if (started && !initialized)
        {
            playercombat.enabled = true;
            foreach (GameObject fighter in fighters)
            {
                fighter.GetComponent<FishAI>().enabled = true;
            }
            initialized = true;
        }

        if (!started)
        {
            countdowntext.enabled = true;

            if (timertime > 0)
            {
                countdowntext.text = Mathf.CeilToInt(timertime).ToString();
                timertime -= Time.deltaTime;
            }
            else if (timertime > -1)
            {
                countdowntext.text = "GO";
                timertime -= Time.deltaTime;
            }
            else
            {
                countdowntext.enabled = false;
                started = true;
            }
        }

        playerhealth = playercombat.fighthealth;
        if (playerhealth <= 0 && alive)
        {
            fightersleft -= 1;
            alive = false;
        }
        for (int i = fighters.Count - 1; i >= 0; i--)
        {
            var enemy = fighters[i];
            if (enemy.GetComponent<FishAI>().health <= 0)
            {
                fightersleft--;
                enemy.GetComponent<FishAI>().enabled = false;
                fighters.RemoveAt(i);
            }
        }
        if (fightersleft <= 1 && alive && !fightover)
        {
            fightmanager.instance.changeback(true, fishamount);
            fightover = true;
        }
        else if(fightersleft <= 1 && !alive && !fightover)
        {
            fightmanager.instance.changeback(false, fishamount);
            fightover = true;
        }
    }

}
