using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class moneyhandler : MonoBehaviour
{
    public float money = 0;
    public TMP_Text moneytext;
    public Button[] buttons;

    private Coroutine moneyAnimation;

    void Start()
    {
        UpdateMoneyText(money);
    }

    [ContextMenu("add100")]
    public void add100()
    {
        Changemoney(1000);
        Debug.Log("press");
    }

    public void Changemoney(float change)
    {
        money += change;

        // Stop an existing animation if another money change happens
        if (moneyAnimation != null)
            StopCoroutine(moneyAnimation);

        moneyAnimation = StartCoroutine(AnimateMoney(money - change, money));
    }

    private System.Collections.IEnumerator AnimateMoney(float startMoney, float targetMoney)
    {
        float duration = 2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float currentMoney = Mathf.Lerp(startMoney, targetMoney, elapsed / duration);

            UpdateMoneyText(currentMoney);

            yield return null;
        }

        // Make sure we end exactly on the correct amount
        UpdateMoneyText(targetMoney);

        moneyAnimation = null;
    }

    private void UpdateMoneyText(float amount)
    {
        string moneyformatted = NumberFormatter.FormatNumber(amount);
        moneytext.text = "Cash: " + moneyformatted + "$";
    }
}

