using System;
using System.Text.RegularExpressions;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
public class moneyhandler : MonoBehaviour
{
    public float money = 0;
    public TMP_Text moneytext;
    public Button[] buttons;
    void Start()
    {
        Changemoney(0);
    }

    
    void Update()
    {

    }

    [ContextMenu("add100")]
    public void add100()
    {
        Changemoney(1000);
        Debug.Log("press");
    }

    
    public void Changemoney(float change)
    {

        money = money + change;
        string moneyformatted = NumberFormatter.FormatNumber(money);
        moneytext.text = "Cash: " + moneyformatted + "$";
        foreach (Button btn in buttons)
        {
            string pricetext = btn.transform.parent.Find("Price").GetComponent<TMP_Text>().text;
            string numberText = Regex.Replace(pricetext, @"[^0-9.]", "");

            if (float.TryParse(numberText, out float price))
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
