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
        Debug.Log("Switching to Minigame F start screne");
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
