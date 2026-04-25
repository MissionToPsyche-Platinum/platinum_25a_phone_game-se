using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleManager : MonoBehaviour
{
    [SerializeField] private CanvasGroup cover;

    void Start()
    {
        StartCoroutine(FadeOut(2f));
    }

    public IEnumerator FadeIn(float fadeTime)
    {
        float elapsedTime = 0f;
        while (elapsedTime < fadeTime) {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsedTime / fadeTime);
            cover.alpha = alpha;
            yield return null;
        }
        SceneManager.LoadScene("CentralHub");
    }

    public IEnumerator WaitForSeconds(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        StartCoroutine(FadeIn(2f));
    }

    public IEnumerator FadeOut(float fadeTime)
    {
        float elapsedTime = 0f;
        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01((2 - elapsedTime) / fadeTime);
            cover.alpha = alpha;
            yield return null;
        }
        StartCoroutine(WaitForSeconds(3));
    }
}
