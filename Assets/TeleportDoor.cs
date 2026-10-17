using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Unity.Collections.AllocatorManager;

public class TeleportDoor : MonoBehaviour, IInteractable
{
    [Header("Teleport")]
    [SerializeField] private Transform destination;

    

    [Header("Optional state change")]
    [SerializeField] private bool changesTavernState;
    [SerializeField] private bool destinationIsInsideTavern;

    public bool doorunlocked = false;

    public int unlockcost;

    public Transform player;
    playertavern playertavernscript;


    public GameObject buyscreen;
    public TMP_Text costtext;
    public GameObject unlockbutton;
    moneyhandler moneyscript;

    private void Awake()
    {
        if(unlockcost <= 0)
        {
            doorunlocked = true;
        }
        moneyscript = Object.FindFirstObjectByType<moneyhandler>();
        if (playertavernscript == null)
            playertavernscript = player.GetComponent<playertavern>();
    }

    public void Interact()
    {
        if (!doorunlocked)
        {
            ShowText();
            return;
        }
        else
        {
            playertavernscript.UseDoor(
            destination.position,
            changesTavernState,
            destinationIsInsideTavern
            );
        }


    }


    public void ShowText()
    {
        StartCoroutine(ShowTextForSeconds(2, buyscreen.gameObject));
    }

    private IEnumerator ShowTextForSeconds(float numberofseconds, GameObject text)
    {
        costtext.text = "This door is locked \n Pay " + unlockcost + "$ to unlock";
        Button button = unlockbutton.GetComponent<Button>();

        button.onClick.RemoveAllListeners();

        if (!doorunlocked && moneyscript.money >= unlockcost)
        {
            unlockbutton.SetActive(true);
            button.onClick.AddListener(buydoor);
        }
        else
        {
            unlockbutton.SetActive(false);
        }
        text.gameObject.SetActive(true);

        yield return new WaitForSeconds(numberofseconds);

        buyscreen.gameObject.SetActive(false);
    }
    public void buydoor()
    {
        if (!doorunlocked)
        {
            doorunlocked = true;
            moneyscript.Changemoney(-unlockcost);
        }

    }

}