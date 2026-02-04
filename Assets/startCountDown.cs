using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class startCountDown : MonoBehaviour
{
    float time = 10f;
    [SerializeField] pause pause;

    // Start is called before the first frame update
    void Start()
    {
        pause.setPause();
        Debug.Log("Start");
    }

    // Update is called once per frame
    void Update()
    {
        countDown();
    }

    void countDown()
    {
        time -= Time.deltaTime;
        Debug.Log(time);


        if (time <= 0)
        {
            Time.timeScale = 1;
        }
    }
}
