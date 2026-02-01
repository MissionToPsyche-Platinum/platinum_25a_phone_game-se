using JetBrains.Annotations;
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

    [SerializeField] private TextMeshProUGUI[] labelA; //A1 - B3 //A2 - B2 //A3 - B4 //A4 - B5 //A5 - B1
    [SerializeField] private TextMeshProUGUI[] labelB;

    private int[] generateRandomChecks(int total, int max)
    {
        int[] randoms = new int[total];
        for (int i = 0; i < total; i++)
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
        for (int i = 0; i < total; i++)
        {
            randoms[i] = randoms[i] - 1;
        }
        return randoms;
    }
    private int[] generateRandomChecks(int[] current)
    {
        int total = current.Length;
        for (int i = 0; i < total; i++)
        {
            current[i] = current[i] + 1;
        }
        int[] randoms = new int[total];
        for (int i = 0; i < total; i++)
        {
            int randomIndex = Random.Range(0, total);
            if (i == 0)
            {
                randoms[i] = current[randomIndex];
            }
            else
            {
                if (randoms.Contains(current[randomIndex]))
                {
                    i--;
                }
                else
                {
                    randoms[i] = current[randomIndex];
                }

            }
        }
        for (int i = 0; i < total; i++)
        {
            randoms[i] = randoms[i] - 1;
        }
        return randoms;
    }

    // Start is called before the first frame update
    void Start()
    {
        checks = new int[5];
        Random.InitState(DateTime.Now.Millisecond);
        int max = MinigameAPremises.premises.Count;
        int[] randoms = generateRandomChecks(5, max);
        int scenario = Random.Range(0, 2);
        if (Random.Range(0, 2) == 0)
        {
            // set labels for grid
            string[] keys = new string[6];
            MinigameAPremises.premises.Keys.CopyTo(keys, 0);
            Dictionary<int, string> map = new Dictionary<int, string>();
            for (int i = 0; i < 5; i++)
            { 
                labelA[i].text = keys[randoms[i]];
                map[i] = keys[randoms[i]];
            }
            randoms = generateRandomChecks(randoms);
            for (int i = 0; i < 5; i++)
            {
                labelB[i].text = MinigameAPremises.premises[keys[randoms[i]]][scenario];
                for (int j = 0; j < 5; j++)
                {
                    if (map[j] == keys[randoms[i]])
                    {
                        checks[i] = (j * 5) + i;
                    }
                }
            }
        }
        else
        {
            // set labels for grid
            string[] keys = new string[6];
            MinigameAPremises.premises.Keys.CopyTo(keys, 0);
            Dictionary<int, string> map = new Dictionary<int, string>();
            for (int i = 0; i < 5; i++)
            {
                labelB[i].text = keys[randoms[i]];
                map[i] = keys[randoms[i]];
            }
            randoms = generateRandomChecks(randoms);
            for (int i = 0; i < 5; i++)
            {
                labelA[i].text = MinigameAPremises.premises[keys[randoms[i]]][scenario];
                for (int j = 0; j < 5; j++)
                {
                    if (map[j] == keys[randoms[i]])
                    {
                        checks[i] = (i * 5) + j;
                    }
                }
            }
        }
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
                    minigameAgrid[i].GetComponentInParent<MinigameAMarkGrid>().setCheck(true);
                    gridChecks[i] = true;
                    break;
                }
            }
        }
        // assign clues
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
