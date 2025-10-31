using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class collectGreenScience : MonoBehaviour
{   
    

    private float lastClickTime = 0f;
    private float threshold = 0.5f;
    private float pointsAdded = 2f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    //checks for a double click action to collect red science and addes two points
    //works with mouse and touch screen
    private void OnMouseDown()
    {
        if(Time.time - lastClickTime <= threshold)
        {
            Destroy(gameObject);
            
            Debug.Log("Destroyed Green Points added: " + pointsAdded);

        }

        lastClickTime = Time.time;
        
    }
}
