using Cainos.LucidEditor;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;


public class statsChecker : MonoBehaviour, IInteractable
{
    [Header("Scripts")]
    public playertavern playertarvenscript;
    public playerscript playerscript;

    public moneyhandler moneyscript;

    public handler handler;


    public Transform playerfishholder;

    private bool inshop = false;
    private bool ready = true;

    [Header("Ui elements")]
    public Transform faderimage;
    public GameObject marketui;
    public TMP_Text errortext;



    public TMP_Text damage, health, speed, level, upgradeprice;

    public Button upgradefishlevelbutton;
    public int price;

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

            playerfishholder.GetChild(0).GetComponent<fishscript>().countingdown = false;
            changeui(playerfishholder.GetChild(0).GetComponent<stats>());
            
            StartCoroutine(openshop());
        }
        if (!inshop)
        {
            playerfishholder.GetChild(0).GetComponent<fishscript>().countingdown = true;
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

    public void ShowText()
    {
        StartCoroutine(ShowTextForSeconds(2, errortext.gameObject));
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

    private void changeui(stats stats)
    {
        damage.text = "Damage: " + NumberFormatter.FormatNumber(stats.damage);
        health.text = "Health: " + NumberFormatter.FormatNumber(stats.health);
        speed.text = "Speed: " + NumberFormatter.FormatNumber(stats.speed * 100);
        level.text = "Fighting Level: " + stats.skilllevel.ToString();
        upgradeprice.text = "Skill Level Upgrade Price: " + NumberFormatter.FormatNumber(stats.skilllevel * stats.skilllevel * 150) + "$";

        price = stats.skilllevel * stats.skilllevel * 150;

        if (stats.skilllevel >= 100)
        {
            upgradefishlevelbutton.interactable = false;
            upgradefishlevelbutton.GetComponent<Image>().color = new Color(0.55f, 0, 0);
            upgradefishlevelbutton.GetComponentInChildren<TMP_Text>().text = "MAX";
        }
        else
        {
            if (moneyscript.money >= price)
            {
                upgradefishlevelbutton.GetComponent<Image>().color = new Color(0.29f, 0.55f, 0);
            }
            else
            {
                upgradefishlevelbutton.GetComponent<Image>().color = new Color(0.55f, 0, 0);
            }
        }
    }
    public void upgradeskilllevel()
    {
        if(moneyscript.money >= price)
        {
            moneyscript.Changemoney(-price);
            
            stats stats = playerfishholder.GetChild(0).GetComponent<stats>();

            stats.skilllevel += 1;
            stats.damage *= 1.15f;
            stats.health *= 1.15f;
            stats.speed *= 1.15f;
            stats.cost = Mathf.FloorToInt(stats.cost * 1.5f);
            changeui(stats);
            
        }
    }
}
