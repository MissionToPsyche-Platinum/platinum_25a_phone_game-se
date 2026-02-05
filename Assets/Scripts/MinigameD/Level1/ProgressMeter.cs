using UnityEngine;
using UnityEngine.UI;

public class ProgressMeter : MonoBehaviour
{
    public Image fillImage;
    public Transform miniRocket;
    public Transform targetObject;
    public float maxHeight = 875f;
    public float minX = -2.326f;
    public float maxX = 2.888f;

    void Update()
    {
        float currentHeight = targetObject.localPosition.y;
        float clampedHeight = Mathf.Clamp(currentHeight, 0, maxHeight);
        float fillPercent = clampedHeight / maxHeight;

        fillImage.fillAmount = fillPercent;

        float markerX = Mathf.Lerp(minX, maxX, fillPercent);
        miniRocket.localPosition = new Vector3(
            markerX,
            miniRocket.localPosition.y,
            miniRocket.localPosition.z
        );
    }
}
