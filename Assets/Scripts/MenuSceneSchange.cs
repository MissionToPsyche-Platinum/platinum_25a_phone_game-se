using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuSceneSchange : MonoBehaviour
{
    [SerializeField] private GameObject UnavailableScreen;
    [SerializeField] private float sceneLoadDelay = 0f;

    public void SwitchToSceneMinigameA()
    {
        Debug.Log("Switching to Minigame A");
        StartCoroutine(LoadSceneWithDelay("MinigameA"));
    }

    public void SwitchToSceneMinigameB()
    {
        Debug.Log("Switching to Minigame B");
        Random.InitState(System.DateTime.Now.Millisecond);
        int rand = Random.Range(0, 3);
        if (rand == 0)
        {
            StartCoroutine(LoadSceneWithDelay("MinigameBMetalWeights"));
        }
        else if (rand == 1)
        {
            StartCoroutine(LoadSceneWithDelay("MinigameBPowerBalance"));
        }
        else if (rand == 2)
        {
            StartCoroutine(LoadSceneWithDelay("MinigameBWireColorLink"));
        }
    }

    public void SwitchToSceneMinigameC()
    {
        Debug.Log("Switching to Minigame C");
        StartCoroutine(LoadSceneWithDelay("MinigameC"));
    }

    public void SwitchToSceneMinigameD()
    {
        Debug.Log("Switching to Minigame D");
        StartCoroutine(LoadSceneWithDelay("MinigameD-Start-Menu"));
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
        StartCoroutine(LoadSceneWithDelay("MinigameF"));
    }

    public void SwitchToSceneMinigameF_Start()
    {
        Debug.Log("Switching to Minigame F start page");
        StartCoroutine(LoadSceneWithDelay("MinigameF_Start"));
    }

    public void SwitchToCentralHub()
    {
        Debug.Log("Switching to Central Hub");
        StartCoroutine(LoadSceneWithDelay("CentralHub"));
    }

    public void CloseUnavailableScreen()
    {
        UnavailableScreen.SetActive(false);
    }

    private IEnumerator LoadSceneWithDelay(string sceneName)
    {
        yield return new WaitForSeconds(sceneLoadDelay);
        SceneManager.LoadScene(sceneName);
        Debug.Log("Finished switching");
    }
}
