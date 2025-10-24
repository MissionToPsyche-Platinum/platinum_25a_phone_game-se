using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketVelocity : MonoBehaviour
{
    public float initialVelocity = 15f; 
    public float gravity = -5f;

    private float currentVelocity;
    private bool ascending = true;

    void Start()
    {
        currentVelocity = initialVelocity;
    }

    void FixedUpdate()
    {
        if (ascending)
        {
            currentVelocity += gravity * Time.deltaTime;
            transform.position += new Vector3(0, currentVelocity * Time.deltaTime, 0);

            // check if velocity has reached zero (peak)
            if (currentVelocity <= 0)
            {
                currentVelocity = 0;
                ascending = false;
            }
        }
        else
        {
            // falling down
            currentVelocity += gravity * Time.deltaTime;
            transform.position += new Vector3(0, currentVelocity * Time.deltaTime, 0);

            // check if velocity is positive again (ring interaction)
            if (currentVelocity > 0)
            {
                ascending = true;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("BoostRing"))
        {
            boostVelocity();

            other.gameObject.SetActive(false);
        }
        else if (other.CompareTag("PenaltyRing"))
        {
            decreaseVelocity();

            other.gameObject.SetActive(false);
        }
    }

    // triggers when interacting with boost ring
    public void boostVelocity()
    {
        currentVelocity = initialVelocity;
    }

    // triggers when interacting with penalty ring
    public void decreaseVelocity()
    {
        // decrease by 25%
        currentVelocity = currentVelocity * (float)0.75;
    }
}
