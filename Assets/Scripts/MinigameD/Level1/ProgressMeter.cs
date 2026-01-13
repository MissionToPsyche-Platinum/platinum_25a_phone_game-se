using UnityEngine;

public class ProgressMeter : MonoBehaviour
{
    public RectTransform progressBar; 
    public Transform targetObject; 
    public float maxHeight = 875f; 

    public float minWidth = 0.25f; 
    public float maxWidth = 4.75f; 

    void Update()
    {
        float currentHeight = targetObject.localPosition.y; 
        float clampedHeight = Mathf.Clamp(currentHeight, 0, maxHeight);

        float fillPercent = clampedHeight / maxHeight;
        float newWidth = Mathf.Lerp(minWidth, maxWidth, fillPercent);

        progressBar.sizeDelta = new Vector2(newWidth, progressBar.sizeDelta.y);
    }
}
