using UnityEngine;

public class AscendOnClick : MonoBehaviour
{
    public float ascendSpeed = 600f;

    private RectTransform rectTransform;
    private bool isAscending = false;
    private Canvas parentCanvas;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        parentCanvas = GetComponentInParent<Canvas>();
    }

    public void StartAscending()
    {
        isAscending = true;
    }

    void Update()
    {
        if (isAscending)
        {
            rectTransform.anchoredPosition += Vector2.up * ascendSpeed * Time.deltaTime;

            Vector2 viewPos = rectTransform.anchoredPosition;
            RectTransform canvasRect = rectTransform.parent.GetComponent<RectTransform>();
            if (viewPos.y > canvasRect.rect.height + rectTransform.rect.height + 50)
            {
                isAscending = false;
            }
        }
    }
}
