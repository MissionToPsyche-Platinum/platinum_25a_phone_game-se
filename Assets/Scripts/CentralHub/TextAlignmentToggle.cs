using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class TextAlignmentToggle : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public TextMeshProUGUI text;
    public float currentPos = 40f;
    public float targetPos = 35f;

    public void OnPointerDown(PointerEventData eventData)
    {
        ToggleAlignment();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        ToggleAlignment();
    }

    private void ToggleAlignment()
    {
        if (text != null)
        {
            RectTransform rectTransform = text.GetComponent<RectTransform>();
            float currentY = rectTransform.localPosition.y;

            if (Mathf.Approximately(currentY, currentPos))
            {
                rectTransform.localPosition = new Vector3(rectTransform.localPosition.x, targetPos, rectTransform.localPosition.z);
            }
            else
            {
                rectTransform.localPosition = new Vector3(rectTransform.localPosition.x, currentPos, rectTransform.localPosition.z);
            }
        }
    }
}
