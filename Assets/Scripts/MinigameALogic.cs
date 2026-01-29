using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class MinigameALogic : MonoBehaviour
{
    [SerializeField] private GameObject[] minigameAgrid;
    private Boolean[] gridChecks = new Boolean[25];
    [SerializeField] private GameObject endScreen;

    [SerializeField] private Sprite checkSprite;
    [SerializeField] private Sprite xSprite;

    private int[] checks;

    [SerializeField] private GameObject tutorial;

    [SerializeField] private TextMeshProUGUI labelA1; //B3
    [SerializeField] private TextMeshProUGUI labelA2; //B2
    [SerializeField] private TextMeshProUGUI labelA3; //B4
    [SerializeField] private TextMeshProUGUI labelA4; //B5
    [SerializeField] private TextMeshProUGUI labelA5; //B1
    [SerializeField] private TextMeshProUGUI labelB1;
    [SerializeField] private TextMeshProUGUI labelB2;
    [SerializeField] private TextMeshProUGUI labelB3;
    [SerializeField] private TextMeshProUGUI labelB4;
    [SerializeField] private TextMeshProUGUI labelB5;

    // Start is called before the first frame update
    void Start()
    {
        Random.InitState(DateTime.Now.Millisecond);
        int max = MinigameAPremises.premises.Count;
        int[] randoms = new int[5];
        for (int i = 0; i < 5; i++)
        {
            int randomIndex = Random.Range(1, max + 1);
            if (randoms.Contains(randomIndex))
            {
                i--;
            }
            else
            {
                randoms[i] = randomIndex;
            }
        }
        for (int i = 0; i < 5; i++)
        {
            randoms[i] = randoms[i] - 1;
        }
        int scenario = Random.Range(0, 2);
        if (Random.Range(0, 2) == 0)
        {
            // set labels for grid
            string[] keys = new string[6];
            MinigameAPremises.premises.Keys.CopyTo(keys, 0);
            int labelScenario = randoms[0];
            labelA1.text = keys[labelScenario];
            labelB3.text = MinigameAPremises.premises[keys[labelScenario]][scenario];
            labelScenario = randoms[1];
            labelA2.text = keys[labelScenario];
            labelB2.text = MinigameAPremises.premises[keys[labelScenario]][scenario];
            labelScenario = randoms[2];
            labelA3.text = keys[labelScenario];
            labelB4.text = MinigameAPremises.premises[keys[labelScenario]][scenario];
            labelScenario = randoms[3];
            labelA4.text = keys[labelScenario];
            labelB5.text = MinigameAPremises.premises[keys[labelScenario]][scenario];
            labelScenario = randoms[4];
            labelA5.text = keys[labelScenario];
            labelB1.text = MinigameAPremises.premises[keys[labelScenario]][scenario];
        }
        else
        {
            string[] keys = new string[6];
            MinigameAPremises.premises.Keys.CopyTo(keys, 0);
            int labelScenario = randoms[0];
            labelA1.text = MinigameAPremises.premises[keys[labelScenario]][scenario];
            labelB3.text = keys[labelScenario];
            labelScenario = randoms[1];
            labelA2.text = MinigameAPremises.premises[keys[labelScenario]][scenario];
            labelB2.text = keys[labelScenario];
            labelScenario = randoms[2];
            labelA3.text = MinigameAPremises.premises[keys[labelScenario]][scenario];
            labelB4.text = keys[labelScenario];
            labelScenario = randoms[3];
            labelA4.text = MinigameAPremises.premises[keys[labelScenario]][scenario];
            labelB5.text = keys[labelScenario];
            labelScenario = randoms[4];
            labelA5.text = MinigameAPremises.premises[keys[labelScenario]][scenario];
            labelB1.text = keys[labelScenario];
        }

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
