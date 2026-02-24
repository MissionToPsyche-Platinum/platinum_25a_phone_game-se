using System.Collections;
using System.Collections.Generic;
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
        SceneManager.LoadScene("MinigameD-Game-Lost");
        yield return null;
    }
}
