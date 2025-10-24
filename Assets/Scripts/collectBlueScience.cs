using System.Runtime.CompilerServices;
using UnityEngine;

public class collectBlueScience : MonoBehaviour
{
    private float pointsAdded = 0f;
    float firstCLicktime = 0f;
    float releaseClick = 0f;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnMouseDown()
    {
        firstCLicktime = Time.time;
          
       
    }

    private void OnMouseUp()
    {

        releaseClick = Time.time;
        Destroy(gameObject);

        if ((releaseClick - firstCLicktime) > 5)
        {
            pointsAdded = 10;
        }
        else
        {
            pointsAdded = releaseClick - firstCLicktime;
        }

        Debug.Log("Destroyed Blue points added: " + pointsAdded);

    }
}
