using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
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
