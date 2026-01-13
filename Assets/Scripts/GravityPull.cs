
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class GravityPull : MonoBehaviour
{
    [Header("Gravity Settings")]
    [SerializeField] private float gravityStrength = 12f;

    private Rigidbody2D targetRb;

    public void SetTarget(Rigidbody2D rb)
    {
        targetRb = rb;
    }

    private void FixedUpdate()
    {
        if (targetRb == null) return;

        Vector2 direction = (Vector2)transform.position - targetRb.position;
        float distance = direction.magnitude;

        if (distance <= 0.1f) return; // fail-safe
        float forceMag = gravityStrength / (distance * distance); // inverse square law
        Vector2 force = direction.normalized * forceMag;

        targetRb.AddForce(force);
    }

}
