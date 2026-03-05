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
    [SerializeField] private AudioClipManager audioClipManager;

    [SerializeField] private GameObject[] minigameAgrid;
    private Boolean[] gridChecks = new Boolean[25];
    [SerializeField] private GameObject endScreen;

    [SerializeField] private Sprite checkSprite;
    [SerializeField] private Sprite xSprite;

    private int[] checks;

    [SerializeField] private GameObject tutorial;

    [SerializeField] private TextMeshProUGUI[] labelA; //A1 - B3 //A2 - B2 //A3 - B4 //A4 - B5 //A5 - B1
    [SerializeField] private TextMeshProUGUI[] labelB;

    [SerializeField] private TextMeshProUGUI clues1;
    [SerializeField] private TextMeshProUGUI clues2;
    [SerializeField] private TextMeshProUGUI clues3;
    [SerializeField] private TextMeshProUGUI clues4;

    private const string PREF_TUT_KEY = "TutorialsOn";

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
        bool tutorialOn = PlayerPrefs.GetInt(PREF_TUT_KEY, 1) == 1;
        if (tutorialOn)
        {
            tutorial.SetActive(true);
        }
        else
        {
            tutorial.SetActive(false);
        }

        checks = new int[5];
        Random.InitState(DateTime.Now.Millisecond);
        int max = MinigameAPremises.premises.Count;
        string[] keys = new string[6];
        MinigameAPremises.premises.Keys.CopyTo(keys, 0);
        int[] randoms = generateRandomChecks(5, max);
        int scenario = Random.Range(0, 2);
        
        //set clues
        string texttmp = MinigameAPremises.clues[scenario][Random.Range(0,6)];
        clues1.text = texttmp.Replace("1", keys[randoms[0]]).Replace("2", MinigameAPremises.premises[keys[randoms[0]]][scenario]);
        texttmp = MinigameAPremises.clues[scenario][Random.Range(0, 6)];
        clues2.text = texttmp.Replace("1", keys[randoms[1]]).Replace("2", MinigameAPremises.premises[keys[randoms[1]]][scenario]);
        texttmp = MinigameAPremises.clues[scenario][Random.Range(0, 6)];
        clues3.text = texttmp.Replace("1", keys[randoms[2]]).Replace("2", MinigameAPremises.premises[keys[randoms[2]]][scenario]);
        texttmp = MinigameAPremises.clues[scenario][Random.Range(6, 10)];
        clues4.text = texttmp.Replace("3", keys[randoms[3]]).Replace("4", MinigameAPremises.premises[keys[randoms[4]]][scenario]);

        // set labels for grid
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
            audioClipManager.PlayCongrats();
            endScreen.SetActive(true);
        }
    }

    public void closeTutorial()
    {
        tutorial.SetActive(false);
    }
}
