using Cainos.LucidEditor;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;


public class sketchmarket : MonoBehaviour, IInteractable
{
    [Header("Scripts")]
    public playertavern playertarvenscript;
    public playerscript playerscript;
    public moneyhandler moneyscript;
    public fightmanager fightmanager;


    public Transform playerfishholder;

    private bool inshop = false;
    private bool ready = true;

    [Header("Ui elements")]
    public Transform faderimage;
    public GameObject marketui;

    public handler handler;

    int sellpricevalue;
    int winamountvalue;

    public TMP_Text errortext;
    public GameObject FightTab;
    public GameObject sellTab;
    public GameObject tournamentTab;

    public TMP_Text weight, sellprice, sellpricenormal, basevalue;
    public TMP_Text winamount;

    public TMP_Text tournamentEntryFee, fishelinstars;
    public List<GameObject> opponent = new List<GameObject>();
    public bool fightingusingmarket;

    public int tournamentlevel = 1;


    public Button tournamentbutton;
    private float tournamententryfeevalue;

    public GameObject wontournamentscreen;

    public int maxlevel;

    private Interact interact;

    public void Update()
    {
        if (!interact.CanInteractWith(this))
        {
            if (inshop)
            {
                inshop = false;
                playerfishholder.GetChild(0).GetComponent<fishscript>().countingdown = true;
                closeshop();
            }

        }
    }
    private void Start()
    {
        interact = playerscript.GetComponent<Interact>();
        fightmanager = GameObject.FindGameObjectWithTag("fightmanager").GetComponent<fightmanager>();
    }



    public void Interact()
    {
        if (!ready)
            return;
        if (playerfishholder.childCount != 1)
        {
            ShowText();
            return;
        }

        ready = false;
        inshop = !inshop;
        if (inshop)
        {

            maxlevel = Mathf.FloorToInt(handler.avaliblefish.Count/3);
            playerfishholder.GetChild(0).GetComponent<fishscript>().countingdown = false;
            playerscript.enabled = false;
            playertarvenscript.enabled = false;
            selltab();
            fighttab();
            tournamenttab();
            StartCoroutine(openshop());
        }
        if (!inshop)
        {
            closeshop();
        }
    }
    public void closeshop()
    {
        playerfishholder.GetChild(0).GetComponent<fishscript>().countingdown = true;
        playerscript.enabled = true;
        playertarvenscript.enabled = true;

        StartCoroutine(openshop());
    }
    public void closeshopwithoutfade()
    {
        playerfishholder.GetChild(0).GetComponent<fishscript>().countingdown = true;
        inshop = !inshop;
        playerscript.enabled = true;
        playertarvenscript.enabled = true;
        marketui.SetActive(false);
        ready = true;
    }
    public void ShowText()
    {
        StartCoroutine(ShowTextForSeconds(2,errortext.gameObject));
    }

    private IEnumerator ShowTextForSeconds(float numberofseconds, GameObject text)
    {
       text.gameObject.SetActive(true);

        yield return new WaitForSeconds(numberofseconds);

        errortext.gameObject.SetActive(false);
    }
    private IEnumerator openshop()
    {
        yield return faderimage.GetComponent<fadescript>().FadeIn(1);

        marketui.SetActive(!marketui.activeSelf);


        yield return faderimage.GetComponent<fadescript>().FadeOut(1);
        ready = true;
    }

    public void OpenFightTab()
    {
        CloseAllTabs();
        FightTab.SetActive(true);
    }

    public void OpenSellTab()
    {
        CloseAllTabs();
        sellTab.SetActive(true);
    }

    public void OpenTournamentTab()
    {
        CloseAllTabs();
        tournamentTab.SetActive(true);
    }

    void CloseAllTabs()
    {
        FightTab.SetActive(false);
        sellTab.SetActive(false);
        tournamentTab.SetActive(false);
    }
    public void selltab()
    {
        if (playerfishholder.childCount != 1)
        {
            weight.text = "0g";
            sellprice.text = "0$";
            return;
        }
        stats fishstats = playerfishholder.GetChild(0).GetComponent<stats>();
        weight.text = NumberFormatter.FormatNumber(Mathf.RoundToInt(fishstats.weight)) + "g";
        sellpricevalue = Mathf.RoundToInt(fishstats.weight * 0.05f);
        sellprice.text = NumberFormatter.FormatNumber(sellpricevalue) + "$";

        basevalue.text = "Base value: " + NumberFormatter.FormatNumber(fishstats.cost) + "$";
        sellpricenormal.text = "Sellprice: " + NumberFormatter.FormatNumber(fishstats.cost * 0.3f) + "$";
    }
    public void sell()
    {
        if (playerfishholder.childCount == 1)
        {
            playerscript.enabled = true;
            playertarvenscript.enabled = true;

            Destroy(playerfishholder.GetChild(0).gameObject);

            closeshop();

            moneyscript.Changemoney(sellpricevalue);
        }

    }
    public void sellnormal()
    {
        if (playerfishholder.childCount == 1)
        {
            playerscript.enabled = true;
            playertarvenscript.enabled = true;

            moneyscript.Changemoney(playerfishholder.GetChild(0).GetComponent<stats>().cost * 0.3f);

            Destroy(playerfishholder.GetChild(0).gameObject);

            closeshop();

           
        }
    }

    public void fighttab()
    {
        if (playerfishholder.childCount != 1)
        {
            winamount.text = "Win amount = 0$";
            return;
        }
        stats fishstats = playerfishholder.GetChild(0).GetComponent<stats>();

        int usableskillllevel = Mathf.Min(fishstats.skilllevel, maxlevel);
        winamountvalue = Mathf.RoundToInt(usableskillllevel * usableskillllevel * 50);
        winamount.text = "Win amount = " + NumberFormatter.FormatNumber(winamountvalue) + "$";
    }
    public void joinfight()
    {
        fightingusingmarket = true;
        List<GameObject> possibleopponents = new List<GameObject>();
        foreach (GameObject possibleopponent in handler.avaliblefish)
        {
            if(possibleopponent.GetComponent<stats>().skilllevel == Mathf.Min(playerfishholder.GetChild(0).GetComponent<stats>().skilllevel, maxlevel))
            {
                possibleopponents.Add(possibleopponent);
            }
        }

        opponent.Add(possibleopponents[Random.Range(0, possibleopponents.Count)]);
        possibleopponents.Clear();
        StartCoroutine(Singlefight());
    }

    public void tournamenttab()
    {
        tournamentlevel = handler.fishelinstars + 1;

        fishelinstars.text = "Fishelin stars: " + handler.fishelinstars + " > " + (handler.fishelinstars + 1);

        tournamententryfeevalue = tournamentlevel * tournamentlevel * tournamentlevel * 500f;

        tournamentEntryFee.text = "Entry fee: " + NumberFormatter.FormatNumber(tournamententryfeevalue) + "$";


        string pricetext = tournamentEntryFee.text;
        string numberText = Regex.Replace(pricetext, @"[^0-9.]", "");

        if (float.TryParse(numberText, out float price))
        {

            if (tournamentlevel > maxlevel)
            {
                tournamentbutton.interactable = false;
                tournamentbutton.GetComponent<Image>().color = new Color(0.55f, 0, 0);
                tournamentbutton.GetComponentInChildren<TMP_Text>().text = "MAX";
            }
            else
            {
                if (moneyscript.money >= price)
                {
                    tournamentbutton.GetComponent<Image>().color = new Color(0.29f, 0.55f, 0);
                }
                else
                {
                    tournamentbutton.GetComponent<Image>().color = new Color(0.55f, 0, 0);
                }
            }
        }
    }

    public void startAtournamentfight(int numberoffish)
    {
        fightingusingmarket = true;

        List<GameObject> possibleopponents = new List<GameObject>();
        foreach (GameObject possibleopponent in handler.avaliblefish)
        {
            if (possibleopponent.GetComponent<stats>().skilllevel == tournamentlevel)
            {
                possibleopponents.Add(possibleopponent);
            }
        }
        for(int i = 0;i < numberoffish; i++)
        {
            opponent.Add(possibleopponents[Random.Range(0, possibleopponents.Count)]);
        }
        possibleopponents.Clear();
        fightmanager.changescene(numberoffish);
    }

    public void entertournament()
    {
        if(moneyscript.money >= tournamententryfeevalue)
        {
            moneyscript.Changemoney(-tournamententryfeevalue);
            StartCoroutine(Tournament());
        }
    }
    IEnumerator WaitForObjectAndGiveMoney()
    {
        yield return new WaitUntil(() => moneyscript.gameObject.activeInHierarchy);
        moneyscript.Changemoney(winamountvalue);
        
    }
    private IEnumerator Tournament()
    {
        for (int j = 2; j < 5; j++)
        {
            startAtournamentfight(j);


            yield return new WaitUntil(() => fightmanager.fightover);


            if (!fightmanager.won)
            {
                fightmanager.fightover = false;
                if (!fightmanager.won && playerfishholder.childCount == 1)
                {
                    
                    closeshopwithoutfade();
                    Destroy(playerfishholder.GetChild(0));
                    opponent.Clear();
                }
                yield break;
            }


            fightmanager.fightover = false;
            opponent.Clear();
            yield return null;
        }
        closeshopwithoutfade();
        handler.fishelinstars += 1;
        StartCoroutine(ShowTextForSeconds(15, wontournamentscreen));
    }
    private IEnumerator Singlefight()
    {

        startAtournamentfight(1);


        yield return new WaitUntil(() => fightmanager.fightover);


        if (!fightmanager.won && playerfishholder.childCount == 1)
        {
            fishscript fishscript = playerfishholder.GetChild(0).GetComponent<fishscript>();
            closeshopwithoutfade();
            Destroy(playerfishholder.GetChild(0));
            opponent.Clear();
        }
        else if (fightmanager.won)
        {
            fightmanager.fightover = false;
            opponent.Clear();
            yield return null;
            closeshopwithoutfade();
            StartCoroutine(WaitForObjectAndGiveMoney());
        }

        



    }
}
