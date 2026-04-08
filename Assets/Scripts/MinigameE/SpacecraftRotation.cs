using UnityEngine;

public class SpacecraftRotation : MonoBehaviour
{
    [SerializeField] private Rigidbody2D spacecraftRb;
    [SerializeField] private InputDragLaunch inputDragLaunch;

    [Header("Rotation Settings")]
    [SerializeField] private float minSpeedToRotate = 0.05f;

    private void Awake()
    {
        if (spacecraftRb == null)
        {
            spacecraftRb = GetComponent<Rigidbody2D>();
        }
        if (inputDragLaunch == null)
        {
            inputDragLaunch = FindAnyObjectByType<InputDragLaunch>();
        }
    }

    private void Update()
    {
        if (inputDragLaunch != null && inputDragLaunch.IsDragging)
        {
            float aimAngle = inputDragLaunch.CurrentAimAngleDeg;
            transform.rotation = Quaternion.Euler(0f, 0f, aimAngle - 90f);
            return; 
        }

      if(spacecraftRb == null) return;
      Vector2 velocity = spacecraftRb.velocity;

      // Don't rotate if barely moving
      if (velocity.magnitude < minSpeedToRotate){ return;}

        float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);

    }
}
