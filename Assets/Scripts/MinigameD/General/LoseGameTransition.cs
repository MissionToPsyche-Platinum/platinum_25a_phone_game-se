using UnityEngine;
using UnityEngine.SceneManagement;

public class LoseGameTransition : MonoBehaviour
{
    public Camera mainCamera;
    public GameObject player;
    public float thresholdFraction = 0.05f; 

    void Update()
    {
        Vector3 viewportPos = mainCamera.WorldToViewportPoint(player.transform.position);

        if (viewportPos.y < -thresholdFraction)
        {
            StartCoroutine(TransitionScene());
        }
    }

    private System.Collections.IEnumerator TransitionScene()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("MinigameD-Game-Lost");

        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }
}
