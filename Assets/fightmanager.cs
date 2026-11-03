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

    

    public sketchmarket sketchmarket;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        sketchmarket = GameObject.FindGameObjectWithTag("sketchmarket").GetComponent<sketchmarket>();
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
        if (sketchmarket.fightingusingmarket)
        {
            playerscript.gameObject.SetActive(false);
        }

        yield return image.GetComponent<fadescript>().FadeIn(2);

        if (!sketchmarket.fightingusingmarket)
        {
            playerscript.gameObject.SetActive(false);
        }
        sceneparent.SetActive(false);
        playerscript.enabled = false;
        yield return SceneManager.LoadSceneAsync(scenename, LoadSceneMode.Additive);


        playerscript.gameObject.GetComponent<LineRenderer>().enabled = false;
        if (image != null)
        {
            yield return image.GetComponent<fadescript>().FadeOut(2);

        }
    }

    IEnumerator unloadFight()
    {

        yield return image.GetComponent<fadescript>().FadeIn(2);

        
        if (sketchmarket.fightingusingmarket == false)
        {
            
            playerscript.enabled = true;
            playerscript.gameObject.GetComponent<LineRenderer>().enabled = true;
            Debug.Log("Linerenderer");
        }
        sketchmarket.fightingusingmarket = false;


        yield return SceneManager.UnloadSceneAsync("Fightscene");
        sceneparent.SetActive(true);
        playerscript.gameObject.SetActive(true);

        yield return null;


        if (image != null)
        {
            yield return image.GetComponent<fadescript>().FadeOut(2);
        }
    }
}