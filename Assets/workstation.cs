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

    public Transform fishholder;

    private Interact interact;

    public GameObject Fishingtab;
    public GameObject Restauranttab;

    public GameObject droppertab;

    public GameObject Shoppingtab;

    private shopscript shopscript;


    


    private void Update()
    {
        money = moneyscript.money;

        if (!interact.CanInteractWith(this))
        {
            if (inshop)
            {
                if (fishholder.childCount > 0)
                {
                    foreach (Transform fish in fishholder)
                    {
                        fish.GetComponent<fishscript>().countingdown = true;
                    }
                }
                playerscript.enabled = true;
                playertarvenscript.enabled = true;
                inshop = false;
                StartCoroutine(openshop());
            }

        }
    }
    private void Awake()
    {
        buttons = moneyscript.buttons;
        interact = playerscript.GetComponent<Interact>();
        shopscript = GetComponent<shopscript>();
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
            if(fishholder.childCount > 0)
            {
                foreach (Transform fish in fishholder)
                {
                    fish.GetComponent<fishscript>().countingdown = false;
                }
            }
        }
        if (!inshop)
        {
            if (fishholder.childCount > 0)
            {
                foreach (Transform fish in fishholder)
                {
                    fish.GetComponent<fishscript>().countingdown = true;
                }
            }
            playerscript.enabled = true;
            playertarvenscript.enabled = true;

            StartCoroutine(openshop());
        }
    }

    public void resetshop()
    {
        foreach (Button btn in buttons)
            {
                
                upgrade upgradescript = btn.GetComponentInParent<upgrade>();
            if (upgradescript != null)
            {
                float price = upgradescript.cost;

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
    private IEnumerator openshop()
    {
        yield return faderimage.GetComponent<fadescript>().FadeIn(1);

        shopui.SetActive(!shopui.activeSelf);
        if(shopui.activeSelf == true)
        {
            resetshop();
            shopscript.resetshop();
        
        }

        yield return faderimage.GetComponent<fadescript>().FadeOut(1);
        ready = true;
    }

    public void togglefishingtab()
    {
        Fishingtab.SetActive(true);
        Restauranttab.SetActive(false);
        Shoppingtab.SetActive(false);
        droppertab.SetActive(false);
        resetshop();
        shopscript.resetshop();
    }
    public void togglerestauranttab()
    {
        Fishingtab.SetActive(false);
        Restauranttab.SetActive(true);
        Shoppingtab.SetActive(false);
        droppertab.SetActive(false);
        resetshop();
        shopscript.resetshop();
    }
    public void toggledroppertab()
    {
        Fishingtab.SetActive(false);
        Restauranttab.SetActive(false);
        Shoppingtab.SetActive(false);
        droppertab.SetActive(true);
        resetshop();
        shopscript.resetshop();
    }
    public void toggleshoptab()
    {
        Fishingtab.SetActive(false);
        Restauranttab.SetActive(false);
        Shoppingtab.SetActive(true);
        droppertab.SetActive(false);
        resetshop();
        shopscript.resetshop();
    }


}
