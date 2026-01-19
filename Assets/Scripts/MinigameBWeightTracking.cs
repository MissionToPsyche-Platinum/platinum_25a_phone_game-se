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
