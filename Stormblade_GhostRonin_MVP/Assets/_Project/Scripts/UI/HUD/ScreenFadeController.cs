using UnityEngine;
using System.Collections;

public class ScreenFadeController : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;

    public IEnumerator FadeTo(float targetAlpha, float duration)
    {
        if(canvasGroup == null)
            yield break;

        float startAlpha = canvasGroup.alpha;
        float elapsedTime = 0f;

        while(elapsedTime < duration)
        {
            elapsedTime += Time.unscaledDeltaTime;

            float progress = elapsedTime / duration;

            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, progress);

            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
    }
}
