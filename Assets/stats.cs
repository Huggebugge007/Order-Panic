using JetBrains.Annotations;
using System.Drawing;
using UnityEngine;

public class stats : MonoBehaviour
{
    [Header("Species spawn values")]
    public float spawndepht;
    public float spawnweight;
    public float middlespeed;
    public float middlehealth;
    public float variation = 0.3f;
    public float middledamage;
    public float weightmiddlegrams = 8;
    public float middlesize;


    [Header("Species ai values")]
    public float reactionMaxaverage;
    public float evadetimeaverage;
    public float aggressionaverage;
    public float idlemiddletimeaverage;
    public float aivariation;

    [Header("Random script values")]
    public float speed;
    public float health;
    public float weight;
    public float depht;
    public float dephtmultiplier = 5;
    public bool defeated = false;
    public bool instanisated = false;
    public float damage;
    public float size;

    [Header("Species ai random values")]
    public float reactionMax;
    public float evadetime;
    public float aggression;
    public float idlemiddletime;

    private void Awake()
    {
        if (!instanisated)
        {
            reactionMax = Random.Range(reactionMaxaverage * (1-aivariation), reactionMaxaverage * (1 + aivariation));
            evadetime = Random.Range(evadetimeaverage * (1 - aivariation), evadetimeaverage * (1 + aivariation));
            aggression = Random.Range(aggressionaverage * (1 - aivariation), aggressionaverage * (1 + aivariation));
            idlemiddletime = Random.Range(idlemiddletimeaverage * (1 - aivariation), idlemiddletimeaverage * (1 + aivariation));
            speed = Random.Range(middlespeed - (middlespeed * variation), middlespeed + ((middlespeed * variation)));
            health = Random.Range(middlehealth - (middlehealth * variation), middlehealth + ((middlehealth * variation)));
            damage = Random.Range(middledamage - (middledamage * variation), middledamage + (middledamage * variation));
            size = Random.Range(middlesize - (middlesize * variation), middlesize + (middlesize * variation));
            reactionMax = Mathf.Max(reactionMax, 0);
            evadetime = Mathf.Max(evadetime, 0);
            aggression = Mathf.Max(aggression, 0);
            idlemiddletime = Mathf.Max(idlemiddletime, 0);
            speed = Mathf.Max(speed, 0);
            health = Mathf.Max(health, 0);
            damage = Mathf.Max(damage, 0);
            size = Mathf.Max(size, 0);


            depht =  spawndepht + Random.Range(-10,10);


            weight = Random.Range(weightmiddlegrams - (weightmiddlegrams * variation), weightmiddlegrams + (weightmiddlegrams * variation));

            transform.localScale = new Vector2(size, size);




            instanisated = true;
        }

    }
}
