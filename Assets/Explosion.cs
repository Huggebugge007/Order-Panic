using UnityEngine;

public class Explosion : MonoBehaviour
{
    private void Start()
    {
        Destroy(gameObject, GetComponent<Animator>().runtimeAnimatorController.animationClips[0].length * 3);
    }
}