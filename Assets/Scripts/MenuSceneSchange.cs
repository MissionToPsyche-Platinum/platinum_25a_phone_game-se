using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuSceneSchange : MonoBehaviour
{
    [SerializeField] private GameObject UnavailableScreen;
    [SerializeField] private float sceneLoadDelay = 0.15f;

    public void SwitchToSceneMinigameA()
    {
        Debug.Log("Switching to Minigame A");
        StartCoroutine(LoadSceneWithDelay("MinigameA"));
    }

    public void SwitchToSceneMinigameB()
    {
        Debug.Log("Switching to Minigame B");
        Random.InitState(System.DateTime.Now.Millisecond);
        int rand = Random.Range(0, 2);
        if (rand == 0)
        {
            StartCoroutine(LoadSceneWithDelay("MinigameBMetalWeights"));
        }
        else
        {
            StartCoroutine(LoadSceneWithDelay("MinigameBPowerBalance"));
        }
    }

    public void SwitchToSceneMinigameC()
    {
        Debug.Log("Switching to Minigame C");
        UnavailableScreen.SetActive(true);
    }

    public void SwitchToSceneMinigameD()
    {
        Debug.Log("Switching to Minigame D");
        StartCoroutine(LoadSceneWithDelay("MinigameD-Tutorial"));
    }

    public void SwitchToSceneMinigameE()
    {
        // Scene Loading for Minigame E
        Debug.Log("Switching to Minigame E");
        StartCoroutine(LoadSceneWithDelay("GravityAssist"));
    }

    public void SwitchToSceneMinigameF()
    {
        Debug.Log("Switching to Minigame F");
        UnavailableScreen.SetActive(true);
    }

    public void CloseUnavailableScreen()
    {
        UnavailableScreen.SetActive(false);
    }

    private IEnumerator LoadSceneWithDelay(string sceneName)
    {
        yield return new WaitForSeconds(sceneLoadDelay);
        SceneManager.LoadScene(sceneName);
    }
}
