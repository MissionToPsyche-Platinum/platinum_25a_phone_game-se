using UnityEngine;
using UnityEngine.UI;

public class ProgressMeter : MonoBehaviour
{
    public Image fillImage;
    public Transform miniRocket;
    public Transform rocket;
    public float maxHeight = 875f;

    private float startX;
    private float endX;
    private Vector3 initialMiniPos; // initial position of rocket icon

    void Start()
    {
        initialMiniPos = miniRocket.localPosition;

        RectTransform fillRect = fillImage.GetComponent<RectTransform>();
        Vector3[] corners = new Vector3[4];
        fillRect.GetWorldCorners(corners);

        Transform parentTransform = miniRocket.parent;
        Vector3 localStart = parentTransform.InverseTransformPoint(corners[0]);
        Vector3 localEnd = parentTransform.InverseTransformPoint(corners[2]);

        startX = localStart.x + 60; // account for padding
        endX = localEnd.x + 30; // smooth animation 
    }

    void Update()
    {
        float currentHeight = rocket.localPosition.y;
        float clampedHeight = Mathf.Clamp(currentHeight, 0, maxHeight);
        float fillPercent = clampedHeight / maxHeight;

        fillImage.fillAmount = fillPercent;

        float fillRange = endX - startX;

        float targetX = startX + fillRange * fillPercent;

        Vector3 newLocalPos = miniRocket.localPosition;
        newLocalPos.x = targetX;

        miniRocket.localPosition = newLocalPos;
    }
}
