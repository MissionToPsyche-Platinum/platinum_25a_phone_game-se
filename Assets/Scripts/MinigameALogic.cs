using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinigameALogic : MonoBehaviour
{
    [SerializeField] private GameObject[] minigameAgrid;
    private Boolean[] gridChecks = new Boolean[25];
    [SerializeField] private GameObject endScreen;

    [SerializeField] private Sprite checkSprite;
    [SerializeField] private Sprite xSprite;

    private int[] checks;

    [SerializeField] private GameObject tutorial;

    // Start is called before the first frame update
    void Start()
    {
        // assign clues
        // assign labels to grid
        // find checks
        checks = new int[] { 2, 6, 13, 19, 20 };
        // assign checks and x's
        for (int i = 0; i < 25; i++)
        {
            minigameAgrid[i].GetComponent<Image>().sprite = xSprite;
            gridChecks[i] = false;
            for (int j = 0; j < 5; j++)
            {
                int element = checks[j];
                if (i == element)
                {
                    minigameAgrid[i].GetComponent<Image>().sprite = checkSprite;
                    gridChecks[i] = true;
                    break;
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        int totalChecks = 0;
        for (int i = 0; i < 5; i++)
        {
            int element = checks[i];
            if (minigameAgrid[element].activeSelf)
            {
                totalChecks++;
            }
        }
        if (totalChecks >= 5) 
        {
            endScreen.SetActive(true);
        }
    }

    public void closeTutorial()
    {
        tutorial.SetActive(false);
    }
}
