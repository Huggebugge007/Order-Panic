using UnityEngine;
using System.Collections;
using TMPro;

public class callgambling : MonoBehaviour, IInteractable
{
    public Transform playerfishholder;
    public TMP_Text errortext;

    public gamblingmanager gamblingmanager;

    public bool insidegambling;
    public void Interact()
    {
        if (playerfishholder.childCount != 1)
        {
            ShowText();
            return;
        }
        if(insidegambling == false)
        {
            gamblingmanager.OpenGame();
            insidegambling = true;
        }

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
}
