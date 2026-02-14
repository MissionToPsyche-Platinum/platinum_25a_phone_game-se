using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class checkCollison : MonoBehaviour
{

    float time = 1.5f;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnCollisionEnter2D(Collision2D collision) {

        float check = 0.000001f;
        

        if(collision.gameObject.name == "RedScience(Clone)" || collision.gameObject.name == "GreenScience(Clone)" || collision.gameObject.name == "BlueScience(Clone)" )
        {
            if (gameObject.name == "RedScience(Clone)" || gameObject.name == "GreenScience(Clone)" || gameObject.name == "BlueScience(Clone)")
            {
                if (collision.gameObject.transform.position.z - gameObject.transform.position.z < check)
                {
                    

                    countDown(gameObject);
                }
            }
        }
    }

    void countDown(GameObject gameObject)
    {
        time -= Time.deltaTime;      
        

        if (time <= 0)
        {
            Destroy(gameObject);
        }
    }
}
