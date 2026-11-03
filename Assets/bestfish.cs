using System.Collections.Generic;
using UnityEngine;

public class bestfish : MonoBehaviour
{

    public bool unlocked;

    public handler handler;


    public List<GameObject> possiblefish = new List<GameObject>();

    private int previousquailty = 0;

    public GameObject boss;
    

    void Update()
    {
        if (!unlocked)
        {
            int index = handler.resturantquality-1;
            if(index > 0)
            {
                unlocked = true;
            }
        }

        if (unlocked && handler.resturantquality != previousquailty)
        {
            possiblefish = getpossiblefish();
            previousquailty = handler.resturantquality;
        }
        

    }
    public List<GameObject> getpossiblefish()
    {
        List<GameObject>fishlist = new List<GameObject>();

        fishlist.Clear();

        int max = Mathf.Min(3, handler.resturantquality-1);
        for(int i = 0; i < max;i++)
        {
            fishlist.Add(handler.avaliblefish[handler.resturantquality-2-i]);
        }
        if(boss != null)
        {
            fishlist.Add(boss);
        }

        return fishlist;
    }
}
