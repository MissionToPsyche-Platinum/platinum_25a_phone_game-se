using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public class Spawn_test : MonoBehaviour
{
    [SerializeField] MeshRenderer pointMR;
    [SerializeField] ParticleSystem clickParticles;
   // [SerializeField] AudioSource audio;
    [SerializeField] Material red;
    [SerializeField] Material blue;
    [SerializeField] Material green;
    [SerializeField] Material gold;

    [SerializeField] displayScore score;

    private int Red = 25;
    private int Blue = 50;
    private int Green = 75;
    private int Gold = 100;

    private float time = 2f;
    


    // Start is called before the first frame update
    void Start()
    {
        pointMR.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (pointMR.enabled == false)
        {            
            if(time <= 0)
            {
                colorGenerator();
            }
            time -= Time.deltaTime;
            
        }else if(pointMR.enabled == true)
        {
            time -= Time.deltaTime;
            timeKeeper();
        }

        
    }

    private void OnMouseDown()
    {                
        if(pointMR.enabled == true)
        {
            clickParticles.transform.position = transform.position;
            clickParticles.Play();
            //audio.Play();
            pointMR.enabled = false;
        }

    }

    private void colorGenerator()
    {
        int x = UnityEngine.Random.Range(0, 100);
        

        if (x <= Red)
        {
            pointMR.enabled = true;
            pointMR.material = red;
            time = 10f;
            score.addScore(1);
            
        }
        else if (x > Red && x <= Blue)
        {
            pointMR.enabled = true;
            pointMR.material = blue;
            time = 7.5f;
            score.addScore(2);

        }
        else if (x > Blue && x <= Green)
        {
            pointMR.enabled = true;
            pointMR.material = green;
            time = 5f;
            score.addScore(3);

        }
        else if (x > Green && x <= Gold)
        {
            pointMR.enabled = true;
            pointMR.material = gold;
            time = 0.5f;
            score.addScore(15);
        }
    }

    private void timeKeeper()
    {
        if (time <= 0)
        {
            pointMR.enabled = false;
            time = 3f;
        }
    }


}
