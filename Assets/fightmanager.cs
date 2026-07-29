using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class fightmanager : MonoBehaviour
{
    public static fightmanager instance;
    public Image image;
    public float fadeduration;
    public int fishamount;
    public bool won;
    public bool fightover;
    public Canvas canvas;
    public GameObject sceneparent;
    public playerscript playerscript;
    public Gradient fadegradient;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        
    }

    public void changescene(int numberoffish)
    {
        fightover = false;
        fishamount = numberoffish;
        StartCoroutine(fader("Fightscene"));

    }

    public void changeback(bool didwin, int numberoffish)
    {
        fightover = true;
        won = didwin;
        fishamount = numberoffish;


        StartCoroutine(unloadFight());

    }


    IEnumerator fader(string scenename)
    {
        yield return image.GetComponent<fadescript>().FadeIn(2);



        yield return SceneManager.LoadSceneAsync(scenename, LoadSceneMode.Additive);
        sceneparent.SetActive(false);
        playerscript.enabled = false;
        playerscript.gameObject.GetComponent<LineRenderer>().enabled = false;
        if (image != null)
        {
            yield return image.GetComponent<fadescript>().FadeOut(2);

        }
    }

    IEnumerator unloadFight()
    {

        yield return image.GetComponent<fadescript>().FadeIn(2);

        sceneparent.SetActive(true);
        playerscript.enabled = true;
        playerscript.gameObject.GetComponent<LineRenderer>().enabled = true;

        yield return SceneManager.UnloadSceneAsync("Fightscene");

        yield return null;


        if (image != null)
        {
            yield return image.GetComponent<fadescript>().FadeOut(2);
        }
    }
}