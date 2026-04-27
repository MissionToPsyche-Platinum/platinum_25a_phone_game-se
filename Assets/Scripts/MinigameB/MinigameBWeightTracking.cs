using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MinigameBWeightTracking : MonoBehaviour
{
    [SerializeField] private AudioClipManager audioClipManager;
    [SerializeField] private MusicManager musicManager;

    [SerializeField] private TextMeshProUGUI goalWeight;
    [SerializeField] private MinigameBScaleWeightTracker scaleArea;
    [SerializeField] private TextMeshProUGUI currentWeight;
    private int goalWeightInt = 34;
    private int currentWeightInt = 0;

    [SerializeField] GameObject lightCoin;
    [SerializeField] GameObject darkCoin;
    [SerializeField] GameObject orangeCoin;

    private bool won = false;
    private bool congratsPlayed = false;

    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject backButton;
    [SerializeField] private GameObject settingsButton;

    private const string PREF_TUT_KEY = "TutorialsOn";

    // Start is called before the first frame update
    void Start()
    {
        bool showTutorial = PlayerPrefs.GetInt(PREF_TUT_KEY, 1) == 1;
        if (showTutorial)
        {
            tutorialPanel.SetActive(true);
        }
        else
        {
            tutorialPanel.SetActive(false);
        }

        Random.InitState(System.DateTime.Now.Millisecond);
        int lightWeight = Random.Range(2, 5); // 2-4
        int orangeWeight = Random.Range(4, 9); // 4-8
        int darkWeight = Random.Range(9, 13); // 9-12
        lightCoin.GetComponent<MinigameBCoinScript>().CoinWeight = lightWeight;
        orangeCoin.GetComponent<MinigameBCoinScript>().CoinWeight = orangeWeight;
        darkCoin.GetComponent<MinigameBCoinScript>().CoinWeight = darkWeight;
        goalWeightInt = Random.Range(0, 6) * lightWeight + Random.Range(0, 6) * orangeWeight + Random.Range(0, 6) * darkWeight;
        goalWeight.text = goalWeightInt.ToString();
        // this is where we'll generate randomness later.
    }

    // Update is called once per frame
    void Update()
    {
        currentWeightInt = scaleArea.CalculateWeight();
        currentWeight.text = currentWeightInt.ToString();
        if (!won && goalWeightInt == currentWeightInt)
        {
            winPanel.SetActive(true);
            winPanel.transform.SetAsLastSibling();
            backButton.transform.SetAsLastSibling();
            settingsButton.transform.SetAsLastSibling();
            if (!congratsPlayed)
            {
                musicManager.StopMusic();
                audioClipManager.PlayCongrats();
                congratsPlayed = true;
            }
            won = true;
        }
    }

    public void closeTutorial()
    {
        tutorialPanel.SetActive(false);
    }
}
