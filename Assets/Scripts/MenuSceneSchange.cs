using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuSceneSchange : MonoBehaviour
{
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

    }

    public void SwitchToSceneMinigameD()
    {

    }

    public void SwitchToSceneMinigameE()
    {

    }

    public void SwitchToSceneMinigameFstart()
    {
        Debug.Log("Switching to Minigame F start screen");
        SceneManager.LoadScene("MinigameF_Start");

    }

    public void SwitchToSceneMinigameF()
    {
        Debug.Log("Switching to Minigame F");
        SceneManager.LoadScene("MinigameF");

    }

    public void SwitchToCentralHub()
    {
        Debug.Log("Switching to CentralHub");
        SceneManager.LoadScene("CentralHub");
    }
}
