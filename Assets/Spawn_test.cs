using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public class Spawn_test : MonoBehaviour
{
    [SerializeField] MeshRenderer pointMR;
    [SerializeField] ParticleSystem clickParticles;
   // [SerializeField] AudioSource succaudio;
   // [SerializeField] AudioSource failaudio;
    [SerializeField] Material red;    

    [SerializeField] SpriteRenderer circle;

    [SerializeField] Sprite purpleScience;
    [SerializeField] Sprite blueScience;
    [SerializeField] Sprite greenScience;   
    [SerializeField] Sprite goldScience;

    [SerializeField] displayScore score;

    private int Purple = 50;
    private int Blue = 85;
    private int Green = 100;
    private int Gold = 110;
    private int Red = 135;
    private int points = 0;

    private float time = 2f; 




    // Start is called before the first frame update
    void Start()
    {
        pointMR.enabled = false;
        circle.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (pointMR.enabled == false && circle.enabled == false)
        {            
            if(time <= 0)
            {
                colorGenerator();
            }
            time -= Time.deltaTime;
            
        }else if(pointMR.enabled == true || circle.enabled == true)
        {
            time -= Time.deltaTime;
            timeKeeper();
        }

        
    }

    private void OnMouseDown()
    {                
        if(pointMR.enabled == true || circle.enabled == true)
        {
            clickParticles.transform.position = transform.position;
            clickParticles.Play();
            //audio.Play();
            score.addScore(points);
            circle.enabled = false;
            pointMR.enabled = false;
        }

    }

    private void colorGenerator()
    {
        int x = UnityEngine.Random.Range(0, 250);
        

        if (x <= Purple)
        {
            circle.enabled = true;
            circle.sprite = purpleScience;
            circle.color = Color.magenta; 
            time = 5f;
            points = 1;
            
        }
        else if (x > Purple && x <= Blue)
        {
            circle.enabled = true;
            circle.sprite = blueScience;
            circle.color = Color.blue;
            time = 3.5f;
            points = 2;

        }
        else if (x > Blue && x <= Green)
        {
            circle.enabled = true;
            circle.sprite = greenScience;
            circle.color = Color.green;
            time = 2.5f;
            points = 3;
        }
        else if (x > Green && x <= Gold)
        {
            circle.enabled = true;
            circle.sprite = goldScience;
            circle.color = Color.yellow; 
            time = 1f;
            points = 15;
        }
        else if(x > Gold && x <= Red)
        {
            pointMR.enabled = true;
            pointMR.material = red;
            pointMR.material.color = Color.red;
            time = 2f;
            points = -3;
        }
        else
        {
            time = 3f;
        }
    }

    private void timeKeeper()
    {
        if (time <= 0)
        {
            circle.enabled = false;
            pointMR.enabled = false;
            time = 3f;
            points = 0;
        }
    }


}
