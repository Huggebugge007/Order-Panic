using UnityEngine;

public class PARRALAX : MonoBehaviour
{
    private float lenght, startpos;
    public GameObject cam;
    public float parralaxeffect;

    void Start()
    {
        startpos = transform.position.x;
        lenght = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    void LateUpdate()
    {
        float temp = (cam.transform.position.x * (1 - parralaxeffect));
        float distance = (cam.transform.position.x * parralaxeffect);

        transform.position = new Vector3 (startpos + distance, transform.position.y, transform.position.z);

        if (temp > startpos + lenght) startpos += lenght;
        else if(temp < startpos - lenght) startpos -= lenght;
    }
}
