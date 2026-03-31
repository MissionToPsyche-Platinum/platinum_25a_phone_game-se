using UnityEngine;

public class LevelTransition : MonoBehaviour
{
    public float targetHeight = 875f; 
    public string sceneToLoad;
    public int key = 1;
    private bool hasTransitioned = false;

    public SceneTransitionManager transitionManager;

    void Update()
    {
        if (!hasTransitioned && transform.position.y >= targetHeight)
        {
            hasTransitioned = true;
            transitionManager.LoadScene(sceneToLoad);
            PlayerPrefs.SetInt("MinigameD-TargetPage", key);
            PlayerPrefs.Save();
        }
    }
}
