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

    public TMP_Text weight, sellprice;
    public TMP_Text winamount;

    public TMP_Text tournamentEntryFee, fishelinstars;
    public List<GameObject> opponent = new List<GameObject>();
    public bool fightingusingmarket;

    public int tournamentlevel = 1;

    public int maxtournamentlevel = 8;

    public Button tournamentbutton;
    private float tournamententryfeevalue;

    public GameObject wontournamentscreen;


    public void Start()
    {
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
        playerscript.enabled = true;
        playertarvenscript.enabled = true;

        StartCoroutine(openshop());
    }
    public void closeshopwithoutfade()
    {
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
        weight.text = Mathf.RoundToInt(fishstats.weight).ToString() + "g";
        sellpricevalue = Mathf.RoundToInt(fishstats.weight * 0.05f);
        sellprice.text = sellpricevalue.ToString() + "$";
    }
    public void sell()
    {
        if (playerfishholder.childCount == 1)
        {
            playerscript.enabled = true;
            playertarvenscript.enabled = true;

            StartCoroutine(openshop());

            fishscript fishscript = playerfishholder.GetChild(0).GetComponent<fishscript>();
            fishscript.DestroyWithExplosion();
            moneyscript.Changemoney(sellpricevalue);
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
        winamountvalue = Mathf.RoundToInt(fishstats.skilllevel * fishstats.skilllevel * 50);
        winamount.text = "Win amount = " + winamountvalue.ToString() + "$";
    }
    public void joinfight()
    {
        fightingusingmarket = true;
        List<GameObject> possibleopponents = new List<GameObject>();
        foreach (GameObject possibleopponent in handler.avaliblefish)
        {
            if(possibleopponent.GetComponent<stats>().skilllevel == playerfishholder.GetChild(0).GetComponent<stats>().skilllevel)
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

        tournamentEntryFee.text = "Entry fee: " + tournamententryfeevalue + "$";


        string pricetext = tournamentEntryFee.text;
        string numberText = Regex.Replace(pricetext, @"[^0-9.]", "");

        if (float.TryParse(numberText, out float price))
        {
            if (tournamentlevel >= maxtournamentlevel)
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
                    fishscript fishscript = playerfishholder.GetChild(0).GetComponent<fishscript>();
                    closeshopwithoutfade();
                    fishscript.DestroyWithExplosion();
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
            fishscript.DestroyWithExplosion();
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
    private void Update()
    {

    }
}
