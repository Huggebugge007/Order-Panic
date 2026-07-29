using UnityEngine;
using UnityEngine.Rendering.Universal;

public class waterlightscript : MonoBehaviour
{
    public float globallightlevel;
    public float tavernlightlevel = 0.1f;
    bool intavern = false;
    Light2D globallight;
    public Transform player;
    public playertavern playertavernscript;
    void Start()
    {
        globallight = GetComponent<Light2D>();
    }


    void Update()
    {
        intavern = playertavernscript.insideTavern;
        if (intavern && player.transform.position.y > 500)
        {
            globallightlevel = tavernlightlevel;
        }
        else if(!intavern && player.transform.position.y < 500)
        {
            if (player.position.y <= -100)
            {
                globallightlevel = 0f;
            }
            else if (player.position.y >= -1.5f)
            {
                globallightlevel = 0.4f;
            }
            else
            {
                globallightlevel = (1 - Mathf.Abs(player.position.y / 100f)) * 0.4f;
            }
        }
        globallight.intensity = globallightlevel;
    }
}
