using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class collectPoints : MonoBehaviour
{
    public float pointsAdded = 1f;
    public ParticleSystem clickParticles;
    [SerializeField] public PointTracker pt;
    [SerializeField] AudioSource audioSource;

    [SerializeField] GameObject asteroid;
    GameObject test;
    
    

    private UnityAction addPoints;
    

    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        

    }

    // Update is called once per frame
    void Update()  
    {
        
     
    }


    //detects click of a mouse or touchscreen input and collects the science of the redbubble and adds 1 point
    //works with mouse and touch screen
    public void OnMouseDown()
    {
       

        
            pointsAdded = 1;
            Debug.Log("Points added: " + pointsAdded);

            if (Time.timeScale == 1)
            {
                if (clickParticles != null)
                {
                    clickParticles.transform.position = transform.position;
                    clickParticles.Play();
                    audioSource.Play();
                }


                pt.addPoints(pointsAdded);


                Destroy(gameObject);
            }
        

    }    

    
}
