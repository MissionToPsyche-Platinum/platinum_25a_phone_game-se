using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class checkCollison : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnCollisionEnter2D(Collision2D collision) {

        

        if(collision.gameObject.name == "RedScience(Clone)" || collision.gameObject.name == "GreenScience(Clone)" || collision.gameObject.name == "BlueScience(Clone")
        {
            if(collision.gameObject.GetInstanceID() < gameObject.GetInstanceID())
            {
                Destroy(collision.gameObject);
            }
        }
    }
}
