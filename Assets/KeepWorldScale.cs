using UnityEngine;

public class KeepWorldScale : MonoBehaviour
{
    public float targetScale = 1f;

    void LateUpdate()
    {
        if (transform.parent == null)
            return;

        float parentScale = transform.parent.lossyScale.x;

        if (Mathf.Abs(parentScale) < 0.0001f)
            return;

        float scale = targetScale / Mathf.Abs(parentScale);

        transform.localScale = new Vector3(scale, scale, scale);
    }
}