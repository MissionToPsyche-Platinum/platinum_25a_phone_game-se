using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MiniGameESettingsPanel : MonoBehaviour
{
    [Header("Settings Panel")]
    [SerializeField] private GameObject settingsOverlay;
    [SerializeField] private GameObject settingsCard;
    [SerializeField] private Button SettingsButton;
    [SerializeField] private AudioManager audioManager;

    private bool isOpen = false;
    private void Awake()
    {
        if (settingsOverlay != null)
            settingsOverlay.SetActive(false); // Ensure overlay is hidden at start

        if (SettingsButton != null)
            SettingsButton.onClick.AddListener(Toggle); // Open settings when button is clicked
    }

    public void Toggle()
    {
       if (isOpen)
       Close();
         else
         Open();
    }

    public void Open()
    {
            settingsOverlay.SetActive(true);
            isOpen = true;
    }

    public void Close()
    {
        if (audioManager != null)
            audioManager.PlayButtonClick(); // Play click sound when closing
            
            settingsOverlay.SetActive(false);
            isOpen = false;

            
    }

    private void Update()
    {
        if (!isOpen)
            return;

        if (Input.GetMouseButtonDown(0)) // Check for left mouse click
        {
           
           if (EventSystem.current.IsPointerOverGameObject())
            {
                if (!RectTransformUtility.RectangleContainsScreenPoint(
                    settingsCard.GetComponent<RectTransform>(), 
                    Input.mousePosition, 
                    null))
                {
                    Close();
                }
            }
        }
    }
}
