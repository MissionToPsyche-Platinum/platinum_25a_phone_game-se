using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class orbitPschye : MonoBehaviour
{
    [SerializeField] private Transform obj;
    public float speed = -0.25f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {        
        //sets Pschye to orginal coordinates at the start of the game
        obj.Rotate(0, 0, 0);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        orbit();
    }

    void orbit()
    {
        //has Pschye rotate counterclockwise to simulate orbiting
        obj.Rotate(-speed, speed, 0);
    }

    public void setSpeed(float speed)
    {
        this.speed = speed;
    }
}
