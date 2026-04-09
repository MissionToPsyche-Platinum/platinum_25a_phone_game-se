using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitBtn : MonoBehaviour
{
    public string sceneName; 

    public void SwitchScene()
    {
        SceneManager.LoadScene(sceneName);
    }
}
