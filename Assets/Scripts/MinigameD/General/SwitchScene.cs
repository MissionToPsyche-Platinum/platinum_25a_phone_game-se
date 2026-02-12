using UnityEngine;
using UnityEngine.SceneManagement;

public class SwitchScene : MonoBehaviour
{
    public string sceneName;

    public void ChangeScene()
    {
        MinigameD_AudioManager.Instance.playButton();
        SceneManager.LoadScene(sceneName);
    }
}
