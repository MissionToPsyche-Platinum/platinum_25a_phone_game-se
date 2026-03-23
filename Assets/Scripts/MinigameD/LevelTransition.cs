using UnityEngine;

public class LevelTransition : MonoBehaviour
{
    public float targetHeight = 875f; 
    public string sceneToLoad; 
    private bool hasTransitioned = false;

    public SceneTransitionManager transitionManager;

    void Update()
    {
        if (!hasTransitioned && transform.position.y >= targetHeight)
        {
            hasTransitioned = true;
            transitionManager.LoadScene(sceneToLoad);
        }
    }
}
