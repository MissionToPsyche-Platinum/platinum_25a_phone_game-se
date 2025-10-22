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
                transform.position = new Vector3(target.position.x, targetY, transform.position.z);
                highestY = targetY; 
            }
            else
            {
                Vector3 desiredPosition = new Vector3(transform.position.x, highestY, transform.position.z);
                transform.position = new Vector3(desiredPosition.x, highestY, desiredPosition.z);
            }
        }
    }
}
