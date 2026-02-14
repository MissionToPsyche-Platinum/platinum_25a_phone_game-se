using UnityEngine;
using UnityEngine.SceneManagement;

public class SwitchScene : MonoBehaviour
{
    public string sceneName;

    public void ChangeScene()
    {
        MinigameD_AudioManager.Instance.buttonClick();
        SceneManager.LoadScene(sceneName);
    }
}
