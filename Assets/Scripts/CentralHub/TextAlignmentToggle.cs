using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class TextAlignmentToggle : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public TextMeshProUGUI text;

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

            if (Mathf.Approximately(currentY, 40f))
            {
                rectTransform.localPosition = new Vector3(rectTransform.localPosition.x, 35f, rectTransform.localPosition.z);
            }
            else
            {
                rectTransform.localPosition = new Vector3(rectTransform.localPosition.x, 40f, rectTransform.localPosition.z);
            }
        }
    }
}
