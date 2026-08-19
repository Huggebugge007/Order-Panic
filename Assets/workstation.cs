using UnityEngine;
using System.Collections;
using Microsoft.Unity.VisualStudio.Editor;
using Cainos.LucidEditor;
using TMPro;
using UnityEngine.UI;
using UnityEngine.PlayerLoop;
using Unity.VisualScripting;

public class workstation : MonoBehaviour, IInteractable
{
    [Header("Scripts")]
    public playertavern playertarvenscript;
    public playerscript playerscript;
    public moneyhandler moneyscript;

    private bool inshop = false;
    private bool ready = true;

    [Header("Ui elements")]
    public Transform faderimage;
    public GameObject shopui;




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

        yield return faderimage.GetComponent<fadescript>().FadeOut(1);
        ready = true;
    }

    void Update()
    {

        
    }

}
