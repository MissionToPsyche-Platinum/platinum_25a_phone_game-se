
using UnityEngine;
using TMPro;

public class HUDReadouts : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InputDragLaunch input;
    [SerializeField] private Rigidbody2D spacecraftRb;
    [SerializeField] private TMP_Text angleText;
    [SerializeField] private TMP_Text speedText;

    [Header("Display")]
    [SerializeField] private bool showAimWhileDragging = true;
    [SerializeField] private bool showLiveVelocityAfterLaunch = true;

    void Update()
    {
        if(showAimWhileDragging && input != null && input.IsDragging)
        {
            SetAngleSpeed(input.CurrentAimAngleDeg, input.CurrentAimSpeed);
            return;
        }
        if (showLiveVelocityAfterLaunch && spacecraftRb != null)
        {
            Vector2 v = spacecraftRb.velocity;
            float speed = v.magnitude;

            float angle = 0f;
            if(speed > 0.01f)
                angle = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;

            SetAngleSpeed(angle, speed);
        }
    }

    private void SetAngleSpeed(float angleDeg, float speed)
    {
        if (angleText != null)
            angleText.text = $"Angle: {angleDeg:0.0}°";
        if (speedText != null)
            speedText.text = $"Speed: {speed:0.00} u/s";
    }
}