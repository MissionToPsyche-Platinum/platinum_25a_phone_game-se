using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MinigameBWeightTracking : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI goalWeight;
    [SerializeField] private MinigameBScaleWeightTracker scaleArea;
    [SerializeField] private TextMeshProUGUI currentWeight;
    private int goalWeightInt = 34;
    private int currentWeightInt = 0;

    [SerializeField] GameObject lightCoin;
    [SerializeField] GameObject darkCoin;
    [SerializeField] GameObject orangeCoin;

    private bool won = false;

    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private GameObject winPanel;

    // Start is called before the first frame update
    void Start()
    {
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
            won = true;
        }
    }

    public void closeTutorial()
    {
        tutorialPanel.SetActive(false);
    }
}
