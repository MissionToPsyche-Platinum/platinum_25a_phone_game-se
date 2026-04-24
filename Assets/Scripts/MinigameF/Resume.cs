using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Resume : MonoBehaviour
{
    [SerializeField] orbit space;
        
    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1;
        if(space.enabled == false)
        {
            space.enabled = true;
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        Time.timeScale = 1;
        if (space.enabled == false)
        {
            space.enabled = true;
        }
    }
}
