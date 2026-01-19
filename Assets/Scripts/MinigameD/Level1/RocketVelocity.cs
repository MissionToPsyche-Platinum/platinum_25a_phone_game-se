using System.Collections;
using System.Collections.Generic;
using Unity.Android.Gradle.Manifest;
using UnityEngine;

public class RocketVelocity : MonoBehaviour
{
    public float initialVelocity = 15f;
    public float initialGravity = -2.5f;

    private float currentVelocity;
    private float currentGravity;
    private bool ascending = true;
    private bool collisionEnabled = true;

    private bool shielded = false; // is the shield ring enabled
    private float shieldStartTime = 0f;
    private float shieldDuration = 5f; // effect lasts 5 seconds

    void Start()
    {
        currentVelocity = initialVelocity;
        currentGravity = initialGravity;
    }

    private void Update()
    {
        // tracks duration for shield ring
        if (shielded)
        {
            if (Time.time - shieldStartTime >= shieldDuration)
            {
                shielded = false;
                shieldStartTime = 0f;
            }
        }
    }

    void FixedUpdate()
    {
        // checks if jump ring is enabled
        if (Mathf.Abs(currentVelocity) > initialVelocity)
        {
            // disable collision
            collisionEnabled = false;

            // set gravity to one-third of initialVelocity, so that invulnerability ends after 3 seconds
            currentGravity = -(initialVelocity / 3f);
        } else
        {
            // re-enable collision once velocity is back to normal
            if (!collisionEnabled)
            {
                collisionEnabled = true;
                currentGravity = initialGravity; // reset to original gravity
            }
        }

        if (ascending)
        {
            currentVelocity += currentGravity * Time.deltaTime;
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
            currentVelocity += currentGravity * Time.deltaTime;
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
        if (!collisionEnabled)
            return; // ignore collisions when disabled

        if (other.CompareTag("BoostRing"))
        {
            boostVelocity();

            other.gameObject.SetActive(false);
        }
        else if (other.CompareTag("PenaltyRing"))
        {
            if (shielded)
                return; // ignore collisions if shield ring is enabled

            decreaseVelocity();

            other.gameObject.SetActive(false);
        }
        else if (other.CompareTag("JumpRing"))
        {
            doubleVelocity();

            other.gameObject.SetActive(false);
        } else if (other.CompareTag("ShieldRing"))
        {
            shielded = true;
            shieldStartTime = Time.time;

            other.gameObject.SetActive(false);
        }
    }

    // boost ring: stabilize velocity
    public void boostVelocity()
    {
        currentVelocity = initialVelocity;
    }

    // penalty ring: decrease velocity by 25%
    public void decreaseVelocity()
    {
        currentVelocity = currentVelocity * (float)0.75;
    }

    // jump ring: double velocity + start invulnerability
    public void doubleVelocity()
    {
        currentVelocity = initialVelocity * 2;
    }
}
