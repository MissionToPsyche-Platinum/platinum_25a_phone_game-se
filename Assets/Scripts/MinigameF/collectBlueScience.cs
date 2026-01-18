using System.Runtime.CompilerServices;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class collectBlueScience : MonoBehaviour
{
    private float pointsAdded = 0f;
    float firstCLicktime = 0f;
    float releaseClick = 0f;

    [SerializeField] public PointTracker pt;

    public ParticleSystem clickParticles;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    //starts tracking when object intially held down
    //works with mouse and touch screen
    private void OnMouseDown()
    {
        firstCLicktime = Time.time;
          
       
    }

    //detects when mouse is up to stop click and hold
    //collects blue science and addes points based on how long it was held down
    //works with mouse and touch screen
    private void OnMouseUp()
    {

        if (Time.timeScale == 1)
        {
            releaseClick = Time.time;

            if (clickParticles != null && Time.timeScale == 1)
            {
                clickParticles.transform.position = transform.position;
                clickParticles.Play();

            }




            if ((releaseClick - firstCLicktime) > 5)
            {
                pointsAdded = 10;
            }
            else
            {
                pointsAdded = releaseClick - firstCLicktime;
            }

            pt.addPoints(pointsAdded);

            Destroy(gameObject);

            Debug.Log("Destroyed Blue points added: " + pointsAdded);
        }

    }
}
