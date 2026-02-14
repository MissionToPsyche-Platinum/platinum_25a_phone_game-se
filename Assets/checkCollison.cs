using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class checkCollison : MonoBehaviour
{
    private float check = 0.000005f;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnCollisionEnter2D(Collision2D collision) {

             

        if(collision.gameObject.name == "RedScience(Clone)" || collision.gameObject.name == "GreenScience(Clone)" || collision.gameObject.name == "BlueScience(Clone)" )
        {

            if (gameObject.name == "RedScience(Clone)" || gameObject.name == "GreenScience(Clone)" || gameObject.name == "BlueScience(Clone)")
            {

                if (gameObject.name == "RedScience(Clone)" && collision.gameObject.name == "GreenScience(Clone)")
                {
                    if (collision.gameObject.transform.position.z - gameObject.transform.position.z < check)
                    {
                        Destroy(gameObject, 0.5f);
                    }

                } else if (gameObject.name == "GreenScience(Clone)" && collision.gameObject.name == "BlueScience(Clone)")
                {
                    if (collision.gameObject.transform.position.z - gameObject.transform.position.z < check)
                    {
                        Destroy(gameObject, 0.5f);
                    }

                } else if (gameObject.name == "BlueScience(Clone)" && collision.gameObject.name == "RedScience(Clone)")
                {
                    if (collision.gameObject.transform.position.z - gameObject.transform.position.z < check)
                    {
                        Destroy(gameObject, 0.5f);
                    }
                } else if (gameObject.name == collision.gameObject.name)
                {
                    if (collision.gameObject.transform.position.z - gameObject.transform.position.z < check)
                    {
                        Destroy(gameObject, 0.5f);
                    }
                }

                   
            }
        }
    }

    public void setCheck(float check)
    {
        this.check = check;
    }
    
}
