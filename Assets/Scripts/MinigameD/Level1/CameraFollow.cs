using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float offset = 0f;
    private float highestY;

    void Start()
    {
        if (target != null)
        {
            highestY = transform.position.y; 
        }
    }

    void LateUpdate()
    {
        if (target != null)
        {
            float targetY = target.position.y + offset;

            if (targetY > highestY)
            {
                // keep camera's x position fixed 
                float fixedX = transform.position.x;

                transform.position = new Vector3(fixedX, targetY, transform.position.z);
                highestY = targetY; 
            }
            else
            {
                // keep camera's x position fixed 
                float fixedX = transform.position.x;

                transform.position = new Vector3(fixedX, highestY, transform.position.z);
            }
        }
    }
}
