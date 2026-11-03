using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class rocketmeterscript : MonoBehaviour
{
    float maxboost, currentboost;
    private Slider slider;

    private Image [] images;

    public bool fading;
    public playerscript playerscript;



    void Awake()
    {
        images = GetComponentsInChildren<Image>();
        slider = GetComponent<Slider>();
        StartCoroutine(Fadein());
        playerscript.usableboost = playerscript.totalboost;
    }


    void Update()
    {   
        slider.value = playerscript.usableboost / playerscript.totalboost;
        if(!playerscript.isgrappled && !playerscript.inPool && !fading)
            {
                fading = false;
                StartCoroutine(Fadeout());
                
            }
        if(playerscript.isgrappled || playerscript.inPool)
        {
            if(!fading && images[0].color.a < 1)
            {
                StartCoroutine(Fadein());
            }
            
        }
    }

    IEnumerator Fadein()
    {
        fading = true;
        while (images[0].color.a < 1f)
        {
            if(!playerscript.isgrappled && !playerscript.inPool)
            {

                StartCoroutine(Fadeout());
                yield break;
            }
            foreach (Image image in images)
            {
                Color color = image.color;
                color.a = Mathf.MoveTowards(color.a, 1f, Time.deltaTime);
                image.color = color;
            }

            yield return null;
        }
        fading = false;
    }

    IEnumerator Fadeout()
    {
        fading = true;
        while (images[0].color.a > 0f)
        {
            if(playerscript.isgrappled || playerscript.inPool)
            {
                fading = false;
                StartCoroutine(Fadein());
                yield break;
            }

            foreach (Image image in images)
            {
                Color color = image.color;
                color.a = Mathf.MoveTowards(color.a, 0f, Time.deltaTime);
                image.color = color;
            }

            yield return null;
        }
        fading = false;
        transform.parent.gameObject.SetActive(false);
    }

}
