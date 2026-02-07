using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class startCountDown : MonoBehaviour
{
    float time = 5f;
    [SerializeField] TMP_Text text;
    [SerializeField] Image img;
    bool countdown = true;

    // Start is called before the first frame update
    void Start()
    {
        text.SetText(time + "");
    }

    // Update is called once per frame
    void Update()
    {
        if (countdown == true)
        {
            countDown();
        }
    }

    void countDown()
    {
        time -= Time.deltaTime;
        
        int roundtime = Mathf.RoundToInt(time);
        text.SetText(roundtime + "");

        if (roundtime <= 0)
        {
            Destroy(img);
            Destroy(text);
            countdown = false;
        }
    }
}
