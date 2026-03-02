using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CentralHubInfoPanel : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button infoButton;
    [SerializeField] private GameObject infoOverlay;
    [SerializeField] private GameObject infoCard;


    private bool isOpen = false;

    private void Awake()
    {
        if (infoOverlay != null)
        {
            infoOverlay.SetActive(false);
        }
        if (infoButton != null)
        {
            infoButton.onClick.AddListener(Toggle);
        }
    }

    public void Toggle()
    {
        if (isOpen) Close();
        else Open();
    }


    public void Open()
    {
        if (infoOverlay == null) return;
        infoOverlay.SetActive(true);
        isOpen = true;
    }

    public void Close()
    {
        if (infoOverlay == null) return;
        infoOverlay.SetActive(false);
        isOpen = false;
        
    }

        private void Update()
    {
        if (!isOpen) return;

        if (Input.GetMouseButtonDown(0)) // Check for left mouse click
        {
           
           if (EventSystem.current.IsPointerOverGameObject())
            {
                if (infoCard != null)
                    {
                        var rect = infoCard.GetComponent<RectTransform>();
                        if (rect != null && !RectTransformUtility.RectangleContainsScreenPoint(rect, Input.mousePosition, null))
                        {
                            Close();
                        }
                    }
                
            }
        }  
    }
}