using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuSceneSchange : MonoBehaviour
{
    [SerializeField] private GameObject UnavailableScreen;

    public void SwitchToSceneMinigameA()
    {
        Debug.Log("Switching to Minigame A");
        SceneManager.LoadScene("MinigameA");
    }

    public void SwitchToSceneMinigameB()
    {
        Debug.Log("Switching to Minigame B");
        Random.InitState(System.DateTime.Now.Millisecond);
        int rand = Random.Range(0, 2);
        if (rand == 0)
        {
            SceneManager.LoadScene("MinigameBMetalWeights");
        }
        else
        {
            SceneManager.LoadScene("MinigameBPowerBalance");
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
        SceneManager.LoadScene("MinigameD-Tutorial");
    }

    public void SwitchToSceneMinigameE()
    {
        // Scene Loading for Minigame E
        Debug.Log("Switching to Minigame E");
        SceneManager.LoadScene("GravityAssist");
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
}
