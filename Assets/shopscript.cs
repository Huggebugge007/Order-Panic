using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class shopscript : MonoBehaviour
{
    public List<GameObject> slots = new List<GameObject>();

    private bestfish bestfishscript;
    public float costmultiplier = 0.3f;

    private moneyhandler moneyscript;
    private dropperscript dropperscript;

    public Button shopbutton;
    public TMP_Text shoptext;

    private bool previousunlocked = false;
    void Awake()
    {
        bestfishscript = GetComponent<dropperscript>().bestfishscript;
        moneyscript = GameObject.FindFirstObjectByType<moneyhandler>();
        dropperscript = GetComponent<dropperscript>();
        previousunlocked = false;
        shopbutton.interactable = false;
        shoptext.text = "???";
    }
    void Update()
    {
        if(!previousunlocked && bestfishscript.unlocked)
        {
            shopbutton.interactable = true;
            shoptext.text = "Shop";
            previousunlocked = bestfishscript.unlocked;
        }
    }



    public void resetshop()
    {
        Debug.Log("restshop");
        List<GameObject> allfish = bestfishscript.possiblefish;
        Debug.Log(allfish);

        for(int j = 0; j < slots.Count; j++)
        {
            if(allfish[j] == null)
            {
                slots[j].SetActive(false);
            }
            else
            {
                slots[j].SetActive(true);
            }
        }

        for(int i = 0; i < allfish.Count; i++)
        {

            if(slots[i] != null)
            {

                TMP_Text costtext = null;
                Button buybutton = null;
                Image image = null;

                foreach (Transform child in slots[i].transform)
                {

                    if (costtext == null)
                        costtext = child.GetComponent<TMP_Text>();

                    if (buybutton == null)
                        buybutton = child.GetComponent<Button>();

                    if (image == null && child.GetComponent<Button>() == null)
                        image = child.GetComponent<Image>();
                }

                Debug.Log($"Slot {i}: Text={costtext}, Button={buybutton}, Image={image}");

                SpriteRenderer sr = null;
                foreach (Transform child in allfish[i].transform)
                {
                    sr = child.GetComponent<SpriteRenderer>();

                    if (sr != null)
                        break;
                }
                if (sr != null)
                {
                    image.sprite = sr.sprite;
                }

                Debug.Log(sr);
                float cost = allfish[i].GetComponent<stats>().cost * costmultiplier;
                costtext.text = "$" + NumberFormatter.FormatNumber(cost);

                if(moneyscript.money >= cost)
                {
                    buybutton.interactable = true;
                }
                else
                {
                    buybutton.interactable = false;
                }
                int index = i;

                buybutton.onClick.RemoveAllListeners();
                buybutton.onClick.AddListener(() => buy(index));

            }
        }
    }

    public void buy(int index)
    {
        List<GameObject> allfish = bestfishscript.getpossiblefish();

        if (index < 0 || index >= allfish.Count)
            return;

        GameObject fish = allfish[index];

        float cost = fish.GetComponent<stats>().cost * costmultiplier;

        if (moneyscript.money < cost)
            return;


        moneyscript.Changemoney(-cost);

        StartCoroutine(dropperscript.drop(fish));
        resetshop();

    }
}
