using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class displayScore : MonoBehaviour
{
    [SerializeField] TMP_Text text;

    private int score = 0;

    // Start is called before the first frame update
    void Start()
    {
        text.text = "Points: " + score;
    }

    // Update is called once per frame
    void Update()
    {
        text.text = "Points: " + score;
    }

    public void addScore(int score)
    {
      this.score += score;
    }

    public int getScore()
    {
        return score;
    }
}
