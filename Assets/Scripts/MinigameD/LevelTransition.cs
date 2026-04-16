using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelTransition : MonoBehaviour
{
    public float targetHeight = 875f; 
    public string sceneToLoad;
    public int key = 1;
    private bool hasTransitioned = false;

    private const string PREF_STORY_KEY = "MinigameD-Story";
    private const string PREF_TUT_KEY = "TutorialsOn";
    private const string PREF_TUT_PART_KEY = "MinigameD-Tutorial-Part";

    private bool story = true; // assume story is enabled
    private bool tutorial = true; // assume tutorial is enabled

    public SceneTransitionManager transitionManager;

    void Update()
    {
        if (!hasTransitioned && transform.position.y >= targetHeight)
        {
            hasTransitioned = true;

            story = PlayerPrefs.GetInt(PREF_STORY_KEY, story ? 1 : 0) == 1;
            tutorial = PlayerPrefs.GetInt(PREF_TUT_KEY, tutorial ? 1 : 0) == 1;

            if (!story) // skip Writing scene if story is disabled
            {
                if (key == 1)
                {
                    if (tutorial)
                    {
                        PlayerPrefs.SetInt(PREF_TUT_PART_KEY, 2);
                        PlayerPrefs.Save();
                        transitionManager.LoadScene("MinigameD-Tutorial");
                    }
                    else
                    {
                        transitionManager.LoadScene("MinigameD-Level2");
                    }
                } 
                else if (key == 2)
                {
                    transitionManager.LoadScene("MinigameD-Level3");
                } 
                else if (key == 3)
                {
                    transitionManager.LoadScene("MinigameD-Game-Won");
                }
            } else
            {
                PlayerPrefs.SetInt("MinigameD-TargetPage", key);
                PlayerPrefs.Save();
                transitionManager.LoadScene(sceneToLoad);
            }
        }
    }
}
