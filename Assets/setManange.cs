using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class setManange : MonoBehaviour
{
    [SerializeField] Canvas canvas;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnMouseDown()
    {
        if (canvas.enabled == true)
        {
            canvas.enabled = false;
        }
        else if (canvas.enabled == false)
        {
            canvas.enabled = true;
        }
    }
}
