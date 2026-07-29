using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class fadescript : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private Gradient fadeGradient;

    private void Awake()
    {
        StartCoroutine(FadeOut(2f));
    }

    public IEnumerator FadeOut(float duration)
    {
        if (duration <= 0f)
        {
            image.color = fadeGradient.Evaluate(0f);
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);

            image.color = fadeGradient.Evaluate(1f - progress);
            yield return null;
        }

        // Guarantee the exact final color.
        image.color = fadeGradient.Evaluate(0f);
    }

    public IEnumerator FadeIn(float duration)
    {
        if (duration <= 0f)
        {
            image.color = fadeGradient.Evaluate(1f);
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);

            image.color = fadeGradient.Evaluate(progress);
            yield return null;
        }

        image.color = fadeGradient.Evaluate(1f);
    }

    public IEnumerator BothFades(float duration, float middleTime)
    {
        yield return FadeIn(duration);

        yield return new WaitForSeconds(middleTime);

        yield return FadeOut(duration);
    }
}