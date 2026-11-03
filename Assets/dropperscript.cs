using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class dropperscript : MonoBehaviour
{
    public float time, maxtime;
    public TMP_Text timetext, Dropperbuttontext;

    public Button dropbutton,dropperbutton;

    public bestfish bestfishscript;

    public Transform dropPos;

    private bool on = false;

    public Image faderimage;
    public GameObject normalcam, dropcam;

    public GameObject shop;

    void Awake()
    {
        newtime(maxtime);
        
    }

    void Update()
    {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);

        timetext.text = minutes + "m " + seconds + "s";
        if(time > 0)
        {
            time -= Time.deltaTime;
            dropbutton.interactable = false;
        }
        else
        {
            time = 0f;
            dropbutton.interactable = true;
        }
        if (bestfishscript.unlocked)
        {
            dropperbutton.interactable = true;
            Dropperbuttontext.text = "Dropper";
        }
        else
        {
            dropperbutton.interactable = false;
            Dropperbuttontext.text = "???";
        }
    }

    public void newtime(float value)
    {
        maxtime = value;
        time = maxtime;
    }

    public void dropafish()
    {
        StartCoroutine(drop(bestfishscript.possiblefish[Random.Range(0,bestfishscript.possiblefish.Count)]));
    }

    public IEnumerator drop(GameObject fishprefab)
    {
        newtime(maxtime);

        yield return faderimage.GetComponent<fadescript>().FadeIn(1);

        shop.SetActive(false);
        dropcam.SetActive(true);
        normalcam.SetActive(false);


        yield return faderimage.GetComponent<fadescript>().FadeOut(1);
        
        GameObject fish = fishprefab;
        GameObject fishclone = Instantiate(fish,dropPos.position,Quaternion.Euler(0,0,270f));
        fishclone.GetComponent<fishscript>().wasdefeated = true;
        fishclone.GetComponent<stats>().defeated = true;


        yield return new WaitForSeconds(4f);

        yield return faderimage.GetComponent<fadescript>().FadeIn(1);

        shop.SetActive(true);
        normalcam.SetActive(true);
        dropcam.SetActive(false);
        
        yield return faderimage.GetComponent<fadescript>().FadeOut(1);



    }
}
