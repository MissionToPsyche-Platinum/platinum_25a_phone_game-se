using UnityEngine;
using UnityEngine.SceneManagement;

public class SwitchScene : MonoBehaviour
{
    public string sceneName;
    private const string PREF_TUT_KEY = "TutorialsOn";
    private bool tutorial = true; // assume tutorial enabled

    public void ChangeScene()
    {
        MinigameD_AudioManager.Instance.buttonClick();

        tutorial = PlayerPrefs.GetInt(PREF_TUT_KEY, tutorial ? 1 : 0) == 1;
        if (sceneName == "MinigameD-Tutorial" && !tutorial) // tutorial is disabled
        {
            SceneManager.LoadScene("MinigameD-Writing");
        } else
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}
