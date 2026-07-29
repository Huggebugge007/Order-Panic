using UnityEngine;
using UnityEngine.UI;
public class floatinghealthbar : MonoBehaviour
{
    Slider slider;
    public Transform target;
    public Vector3 offset;

    public void Awake()
    {
        target = transform.parent;
    }
    public void updatehealthbar(float currenthealth, float maxhealth)
    {
        slider = GetComponent<Slider>();
        slider.value = currenthealth / maxhealth;

    }
    private void Update()
    {
        transform.rotation = Camera.main.transform.rotation;
        transform.position = target.position + offset;
    }
}
