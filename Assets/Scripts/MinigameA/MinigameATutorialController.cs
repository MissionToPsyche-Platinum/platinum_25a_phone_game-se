using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinigameATutorialController : MonoBehaviour
{
    [SerializeField] private GameObject[] tutorialParts;

    private bool active = false;

    void Update()
    {
        if (!active && this.isActiveAndEnabled)
        {
            active = true;
            foreach (GameObject tutorialPart in tutorialParts)
            {
                tutorialPart.SetActive(active);
                Debug.Log("Activated tutorial part: " + tutorialPart.name);
            }
        }
    }

    private void OnDisable()
    {
        active = false;
        foreach (GameObject tutorialPart in tutorialParts)
        {
            tutorialPart.SetActive(active);
        }
    }
}
