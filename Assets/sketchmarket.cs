using Cainos.LucidEditor;
using System.Collections;
using System.Text.RegularExpressions;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;

public class sketchmarket : MonoBehaviour, IInteractable
{
    [Header("Scripts")]
    public playertavern playertarvenscript;
    public playerscript playerscript;
    public moneyhandler moneyscript;

    public Transform playerfishholder;

    private bool inshop = false;
    private bool ready = true;

    [Header("Ui elements")]
    public Transform faderimage;
    public GameObject marketui;


    public TMP_Text errortext;


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

            StartCoroutine(openshop());
        }
        if (!inshop)
        {
            playerscript.enabled = true;
            playertarvenscript.enabled = true;

            StartCoroutine(openshop());
        }
    }
    public void ShowText()
    {
        StartCoroutine(ShowTextForSeconds());
    }

    private IEnumerator ShowTextForSeconds()
    {
        errortext.gameObject.SetActive(true);

        yield return new WaitForSeconds(2f);

        errortext.gameObject.SetActive(false);
    }
    private IEnumerator openshop()
    {
        yield return faderimage.GetComponent<fadescript>().FadeIn(1);

        marketui.SetActive(!marketui.activeSelf);


        yield return faderimage.GetComponent<fadescript>().FadeOut(1);
        ready = true;
    }


}
