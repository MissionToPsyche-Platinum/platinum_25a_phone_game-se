using UnityEngine;
using UnityEngine.SceneManagement;

public class SwitchScene : MonoBehaviour
{
    public string sceneName;

    private const string PREF_TUT_KEY = "TutorialsOn";
    private const string PREF_STORY_KEY = "MinigameD-Story";

    private bool tutorial = true; // assume tutorial enabled
    private bool story = true; // assume story is enabled

    public void ChangeScene()
    {
        MinigameD_AudioManager.Instance.buttonClick();

        tutorial = PlayerPrefs.GetInt(PREF_TUT_KEY, tutorial ? 1 : 0) == 1;
        story = PlayerPrefs.GetInt(PREF_STORY_KEY, story ? 1 : 0) == 1;

        if (sceneName == "MinigameD-Tutorial" && !tutorial)
        {
            if (story)
            {
                SceneManager.LoadScene("MinigameD-Writing");
            } else
            {
                SceneManager.LoadScene("MinigameD-Level1");
            }
        } 
        else if (sceneName == "MinigameD-Level1")
        {
            if (story)
            {
                SceneManager.LoadScene("MinigameD-Writing");
            }
            else
            {
                SceneManager.LoadScene("MinigameD-Level1");
            }
        }
        else
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}
