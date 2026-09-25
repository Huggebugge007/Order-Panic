using TMPro;
using UnityEngine;

public class upgrade : MonoBehaviour
{
    public float cost, upgradeamount, costmultiplier, currentamount;
    public TMP_Text pricetext, currentupgradetext, upgradenexttext;
    public moneyhandler moneyhandler;
    public playerscript playerscript;
    public playertavern playertavernscript;

    public int maxupgrade;
    public handler handler;
    public int upgradenumber;
    public int currentrupgradenumber;

    public int addormult;

    private void Start()
    {
        string formattedcost = NumberFormatter.FormatNumber(cost);
        pricetext.text = "Cost: " + formattedcost + "$";
        if(addormult == 1)
        {
            string formattedupgrade = NumberFormatter.FormatNumber((currentamount + upgradeamount));
            upgradenexttext.text = formattedupgrade;
        }
        else
        {
            string formattedupgrade = NumberFormatter.FormatNumber((currentamount * upgradeamount));
            upgradenexttext.text = formattedupgrade;
        }
        
        string formattedcurrentupgrade = NumberFormatter.FormatNumber(currentamount);
        currentupgradetext.text = formattedcurrentupgrade;
    }


    public void doupgrade()
    {
        if(moneyhandler.money >= cost)
        {
            currentrupgradenumber +=1;
            moneyhandler.Changemoney(-cost);
            cost *= costmultiplier;

            
            
            string formattedcost = NumberFormatter.FormatNumber(cost);
            pricetext.text = "Cost: " + formattedcost + "$";
            if (addormult == 1)
            {
                currentamount += upgradeamount;
                string formattedupgrade = NumberFormatter.FormatNumber((currentamount + upgradeamount));
                upgradenexttext.text = formattedupgrade;
            }
            else
            {
                currentamount *= upgradeamount;
                string formattedupgrade = NumberFormatter.FormatNumber((currentamount * upgradeamount));
                upgradenexttext.text = formattedupgrade;
            }
            
            
            string formattedcurrentupgrade = NumberFormatter.FormatNumber(currentamount);
            currentupgradetext.text = formattedcurrentupgrade;

            if (upgradenumber == 1)
            {
                playerscript.upgraderopelenght(currentamount);
            }
            else if(upgradenumber == 2)
            {
                playerscript.upgradecannonpower(currentamount);
            }
            else if (upgradenumber == 3)
            {
                playerscript.upgradeboostamount(currentamount);
            }
            else if (upgradenumber == 4)
            {
                playerscript.upgradereelspeed(currentamount);
            }
            else if (upgradenumber == 5)
            {
                playerscript.upgradecatchradius(currentamount);
            }
            else if (upgradenumber == 6)
            {
                playertavernscript.addroom(((int)currentamount));
            }
            else if (upgradenumber == 7)
            {
                handler.upgradespawnrate(((int)currentamount));
            }
            else if (upgradenumber == 8)
            {
                handler.upgradequality(((int)currentamount));
            }
        }
    }

}
