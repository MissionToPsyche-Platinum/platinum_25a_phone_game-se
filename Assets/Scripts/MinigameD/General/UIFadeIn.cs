using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIFadeIn : MonoBehaviour
{
    public CanvasGroup component;
    public float delay = 0f; // for sequential timing
    public float duration = 1f;

    void Start()
    {
        // initially transparent
        component.alpha = 0;
        component.interactable = false;
        component.blocksRaycasts = false;

        StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        if (delay > 0)
            yield return new WaitForSeconds(delay);

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            component.alpha = Mathf.Lerp(0, 1, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        component.alpha = 1;
        component.interactable = true;
        component.blocksRaycasts = true;
    }
}
