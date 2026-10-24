
using TMPro;
using UnityEngine;

public class gamblingmanager : MonoBehaviour
{
    public GameObject fish;
    public Transform playerfishhodler;

    private callgambling callgamblingscript;

    public TMP_Text errortext;

    public Transform fishholder;
    private bool initiated;


    [ContextMenu("OpenGambling")]
    public void OpenGame()
    {
        fish = playerfishhodler.GetChild(0).gameObject;


        if (SceneLoader.instance == null)
        {

            return;
        }

        SceneLoader.instance.LoadScene("Gambling");
    }

    public void CloseGame()
    {
        SceneLoader.instance.UnloadScene("Gambling");
        callgamblingscript.insidegambling = false;
    }
    private void Update()
    {
        if(callgamblingscript == null)
        {
            callgamblingscript = FindFirstObjectByType<callgambling>();
            
        }
        if (!initiated && callgamblingscript != null)
        {
            callgamblingscript.gamblingmanager = this;
            callgamblingscript.errortext = errortext;
            callgamblingscript.playerfishholder = fishholder;
            initiated = true;
        }
    }

}
