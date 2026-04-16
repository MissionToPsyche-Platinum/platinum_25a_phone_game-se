using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScriptController : MonoBehaviour
{
    public float duration = 0f; // fade out duration

    public GameObject[] pages;
    private int currentPageIndex = 0;

    private GameObject[] currentObjects;
    private int currentObjectIndex = 0;

    private Coroutine currentFadeCoroutine;

    private const string PREF_TUT_KEY = "TutorialsOn";
    private const string PREF_TUT_PART_KEY = "MinigameD-Tutorial-Part";
    private bool tutorial = true; // assume tutorial is enabled

    void Start()
    {
        foreach (var p in pages)
            p.SetActive(false);

        int targetPage = PlayerPrefs.GetInt("MinigameD-TargetPage", 0);

        PlayerPrefs.DeleteKey("MinigameD-TargetPage");

        if (targetPage >= 0 && targetPage < pages.Length)
        {
            ActivatePage(targetPage);
        }
        else
        {
            ActivatePage(0);
        }
    }

    public void ActivatePage(int pageID)
    {
        foreach (var p in pages)
            p.SetActive(false);

        if (pageID >= 0 && pageID < pages.Length)
        {
            pages[pageID].SetActive(true);
            currentPageIndex = pageID;
            SetupCurrentObjects();
        }
    }

    void SetupCurrentObjects()
    {
        currentObjects = new GameObject[pages[currentPageIndex].transform.childCount];
        int i = 0;
        foreach (Transform child in pages[currentPageIndex].transform)
        {
            currentObjects[i] = child.gameObject;
            i++;
        }
        currentObjectIndex = 0;
        ShowCurrentObject();
    }

    void ShowCurrentObject()
    {
        for (int i = 0; i < currentObjects.Length; i++)
        {
            currentObjects[i].SetActive(i == currentObjectIndex);
            if (i == currentObjectIndex)
            {
                CanvasGroup cg = currentObjects[i].GetComponent<CanvasGroup>();
                if (cg != null)
                {
                    cg.alpha = 1f;
                }
            }
        }
    }

    public void CycleObjectsInPage()
    {
        if (currentFadeCoroutine != null)
            StopCoroutine(currentFadeCoroutine);
        currentFadeCoroutine = StartCoroutine(CycleSequence());
    }

    private IEnumerator CycleSequence()
    {
        if (currentObjects[currentObjectIndex].activeSelf)
        {
            yield return currentFadeCoroutine = StartCoroutine(FadeOut(currentObjects[currentObjectIndex], duration));
        }

        tutorial = PlayerPrefs.GetInt(PREF_TUT_KEY, tutorial ? 1 : 0) == 1;

        currentObjectIndex++;
        if (currentObjectIndex >= currentObjects.Length)
        {
            PlayerPrefs.DeleteKey("MinigameD-TargetPage");

            if (currentPageIndex == 0)
            {
                SceneManager.LoadScene("MinigameD-Level1");
            }
            else if (currentPageIndex == 1)
            {
                if (tutorial)
                {
                    PlayerPrefs.SetInt(PREF_TUT_PART_KEY, 2);
                    PlayerPrefs.Save();
                    SceneManager.LoadScene("MinigameD-Tutorial");
                }
                else
                {
                    SceneManager.LoadScene("MinigameD-Level2");
                }
            }
            else if (currentPageIndex == 2)
            {
                SceneManager.LoadScene("MinigameD-Level3");
            }
            else if (currentPageIndex == 3)
            {
                SceneManager.LoadScene("MinigameD-Game-Won");
            }
        }
        else
        {
            ShowCurrentObject();
        }
    }

    private IEnumerator FadeOut(GameObject obj, float duration)
    {
        CanvasGroup cg = obj.GetComponent<CanvasGroup>();
        if (cg == null)
            yield break;

        float startAlpha = cg.alpha;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            cg.alpha = Mathf.Lerp(startAlpha, 0, elapsedTime / duration);
            yield return null;
        }

        cg.alpha = 0;
        obj.SetActive(false);
    }
}
