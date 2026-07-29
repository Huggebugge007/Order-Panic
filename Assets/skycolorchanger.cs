using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class skycolorchanger : MonoBehaviour
{
    private SpriteRenderer spr;
    public Gradient color;
    private float height;
    public Transform player;
    void Start()
    {
        spr = GetComponent<SpriteRenderer>();
    }


    void Update()
    {
        height = spr.bounds.size.y;
        spr.color = color.Evaluate(player.position.y / height);
    }
}
