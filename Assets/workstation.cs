using Cainos.LucidEditor;
using System.Collections;
using System.Text.RegularExpressions;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

public class workstation : MonoBehaviour, IInteractable
{
    [Header("Scripts")]
    public playertavern playertarvenscript;
    public playerscript playerscript;
    public moneyhandler moneyscript;

    public Button[] buttons;
    private bool inshop = false;
    private bool ready = true;

    [Header("Ui elements")]
    public Transform faderimage;
    public GameObject shopui;

    public float money;


    private void Awake()
    {
        buttons = moneyscript.buttons;
    }


    public void Interact()
    {
        if (!ready)
            return;

        ready = false;
        inshop = !inshop;
        if (inshop)
        {
            playerscript.enabled = false;
            playertarvenscript.enabled = false;

            StartCoroutine(openshop());
        }
        if (!inshop)
        {
            playerscript.enabled = true;
            playertarvenscript.enabled = true;

            StartCoroutine(openshop());
        }
    }

    private IEnumerator openshop()
    {
        yield return faderimage.GetComponent<fadescript>().FadeIn(1);

        shopui.SetActive(!shopui.activeSelf);
        if(shopui.activeSelf == true)
        {
            foreach (Button btn in buttons)
            {
                string pricetext = btn.transform.parent.Find("Price").GetComponent<TMP_Text>().text;
                string numberText = Regex.Replace(pricetext, @"[^0-9.]", "");

                if (float.TryParse(numberText, out float price))
                {
                    upgrade upgradescript = btn.GetComponentInParent<upgrade>();
                    if (upgradescript.currentrupgradenumber >= upgradescript.maxupgrade)
                    {
                        btn.interactable = false;
                        btn.GetComponent<Image>().color = new Color(0.55f, 0, 0);
                        btn.GetComponentInChildren<TMP_Text>().text = "MAX";
                    }
                    else
                    {
                        if (money >= price)
                        {
                            btn.GetComponent<Image>().color = new Color(0.29f, 0.55f, 0);
                        }
                        else
                        {
                            btn.GetComponent<Image>().color = new Color(0.55f, 0, 0);
                        }
                    }



                }
            }
        
        }

        yield return faderimage.GetComponent<fadescript>().FadeOut(1);
        ready = true;
    }

    void Update()
    {
        money = moneyscript.money;

    }

}
