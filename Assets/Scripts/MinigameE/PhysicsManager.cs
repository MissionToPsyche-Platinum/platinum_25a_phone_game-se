using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsManager : MonoBehaviour
{
    [Header("Gravity Tunables")]
    public float G = 1.0f;
    public float massPlanet = 1000f;
    public Transform planet;
    public Rigidbody2D spacecraft;

    void FixedUpdate()
    {
        if (!planet || !spacecraft) return;
        Vector2 dir = (Vector2)planet.position - spacecraft.position;
        float r2 = Mathf.Max(dir.sqrMagnitude, 0.01f);
        Vector2 F = dir.normalized * (G * massPlanet / r2);
        spacecraft.AddForce(F);
    }
}
