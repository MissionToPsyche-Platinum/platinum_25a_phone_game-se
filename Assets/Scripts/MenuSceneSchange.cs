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
        int rand = Random.Range(0, 3);
        if (rand == 0)
        {
            SceneManager.LoadScene("MinigameBMetalWeights");
        }
        else if (rand == 1)
        {
            SceneManager.LoadScene("MinigameBPowerBalance");
        }
        else
        {
            SceneManager.LoadScene("MinigameBWireColorLink");
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
