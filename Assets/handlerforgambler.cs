using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using UnityEngine.Rendering.Universal;

public class handlerforgambler : MonoBehaviour
{
    public GameObject fish;

    public GameObject cointossgame, fishdropgame;
    public GameObject cointosscanvas, fishdropcanvas;

    public Button fishdropgamebutton, cointossgamebutton, exitbutton;

    public bool midgame;

    public coinToss cointoss;

    public dropgamehandler dropgamehandler;

    public TMP_Text cointosscurrenttext, cointosspotentialtext;
    public TMP_Text fishdropcurrent, fishdroppotential;

    public TMP_Text moneytext;
    private GameObject fishcopy;
    Fishropeattachment fishropescript;

    public GameObject claw;

    public float fishgravity;

    private moneyhandler moneyscript;

    public gamblingmanager gamblingmanager;

    float worth, cointosspotential, dropgamepotential, dropgamecurrent;
    public int streak;
    bool havelost = false;
    void Awake()
    {
        havelost = false;
        moneyscript = FindFirstObjectByType<moneyhandler>();
        gamblingmanager = FindFirstObjectByType<gamblingmanager>();
        fish = gamblingmanager.fish;
        midgame = false;
        recheckstats();
        fishcopy = Instantiate(fish, new Vector2(0,5000), transform.rotation);
        fishcopy.AddComponent<Light2D>();
        fishropescript = fishcopy.AddComponent<Fishropeattachment>();
        fishropescript.rope = claw.GetComponent<dropscript>();
        fishropescript.handlerforgambler = this;
        fishcopy.GetComponent<Rigidbody2D>().gravityScale = fishgravity;
        Destroy(fishcopy.GetComponent<fishscript>());
        fishcopy.SetActive(false);

        activatecointoss();
        
    }

    void recheckstats()
    {
        stats stats = fish.GetComponent<stats>();
        worth = stats.cost;
        cointosspotential = stats.cost * 1.5f;


        dropgamecurrent = worth * (streak * 0.1f) * Mathf.Pow(1.5f, streak - 1);
        dropgamepotential = worth * ((streak + 1)* 0.1f) * Mathf.Pow(1.5f, streak);

        fishdropcurrent.text = "Current winnings: " + dropgamecurrent + "$";
        fishdroppotential.text = "Potential winnings: " + dropgamepotential + "$";

        cointosspotentialtext.text = "Potential worth: " + cointosspotential + "$";
        cointosscurrenttext.text = "Current worth: "+ worth + "$";

        moneytext.text = "Cash: " + NumberFormatter.FormatNumber(moneyscript.money) + "$";
    }


    void Update()
    {
        if (midgame)
        {
            fishdropgamebutton.interactable = false;
            cointossgamebutton.interactable = false;
            exitbutton.interactable = false;
        }
        else
        {
            fishdropgamebutton.interactable = true;
            cointossgamebutton.interactable = true;
            exitbutton.interactable = true;
        }

        if(!havelost && fish == null )
        {
            havelost = true;
            lost();
        }

    }

    public void activatecointoss()
    {
        fishcopy.SetActive(false);
        cointossgame.SetActive(true);
        fishdropgame.SetActive(false);
        cointosscanvas.SetActive(true);
        fishdropcanvas.SetActive(false);
    }
    public void activatefishdrop()
    {
        cointossgame.SetActive(false);
        fishdropgame.SetActive(true);
        cointosscanvas.SetActive(false);
        fishdropcanvas.SetActive(true);
        fishcopy.SetActive(true);
        fishropescript.AttachFish();
    }

    public void cashout()
    {
        if (midgame)
        {
            streak = 0;
            midgame = false;
            recheckstats();
            moneyscript.Changemoney(dropgamecurrent);
        }
    }

    public void tosssilver()
    {
        if (!midgame)
        {
            StartCoroutine(PlayCoinToss(false));
        }

    }
    public void tossgold()
    {
        if (!midgame)
        {
            StartCoroutine(PlayCoinToss(true));
        }
    }

    private IEnumerator PlayCoinToss(bool wantGold)
    {
        bool won = false;
        midgame = true;
        yield return StartCoroutine(
            cointoss.TossAndWait(wantGold, result =>
            {
                won = result;
            })
        );

        if (!won)
        {
            lost();
        }
        if (won)
        {
            fish.GetComponent<stats>().cost = Mathf.FloorToInt(fish.GetComponent<stats>().cost * 1.5f);
        }
        midgame = false;
        recheckstats();
    }

    public void drop()
    {
        if (!midgame)
        {
            midgame = true;
        }
        fishropescript.DropFish();
    }

    public void nextlevel()
    {
        dropgamehandler.setcamsize(dropgamehandler.cam.orthographicSize + 1);
        fishropescript.AttachFish();
        streak++;
        recheckstats();
    }

    public void lost()
    {
        havelost = true;
        Destroy(fish);
        gamblingmanager.CloseGame();
    }
}
