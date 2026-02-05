
using UnityEngine;
using TMPro;

public class HUDReadouts : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InputDragLaunch input;
    [SerializeField] private TextMeshProUGUI angleText;
    [SerializeField] private TextMeshProUGUI speedText;

    [Header("Foratting Settings")]
    [SerializeField] private string anglePrefix = "Angle: ";
    [SerializeField] private string speedPrefix = "Speed: ";

    private void Awake()
    {
        if (!input)
        {
            input = FindFirstObjectByType<InputDragLaunch>();
        }
        if (!angleText)
        {
            var t = transform.Find("AngleText");
            if (t)
            {
                angleText = t.GetComponent<TextMeshProUGUI>();
            }
        }
        if (!speedText)
        {
            var t = transform.Find("SpeedText");
            if (t)
            {
                speedText = t.GetComponent<TextMeshProUGUI>();
            }
        }
    }

    private void Update()
    {
        if (!input || !angleText || !speedText)
        {
            return;
        }
        if (!input.IsDragging && input.HasLastShot)
        {
            angleText.text = anglePrefix + input.CurrentAimAngleDeg.ToString("F0") + "°";
            speedText.text = speedPrefix + input.CurrentAimSpeed.ToString("F1") + " m/s";
            return;

        }
            angleText.text = anglePrefix + input.CurrentAimAngleDeg.ToString("F0") + "°";
            speedText.text = speedPrefix + input.CurrentAimSpeed.ToString("F1") + " m/s";
    }
}