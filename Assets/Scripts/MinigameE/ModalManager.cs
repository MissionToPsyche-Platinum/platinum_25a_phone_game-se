using System;
using TMPro;
using TMPro.EditorUtilities;
using UnityEngine;

public class ModalManager : MonoBehaviour
{
    [Header("Modal Panels")]
    [SerializeField] private GameObject introOverlayPanel;
    [SerializeField] private GameObject educationPopupPanel;
    [SerializeField] private GameObject confirmExitPanel;
    [SerializeField] private GameObject developerInfoPanel;
    [SerializeField] private GameObject settingsOverlay;

    [Header("Eduation Popup UI")]
    [SerializeField] private TMP_Text eduationTitleText;
    [SerializeField] private TMP_Text educationBodyText;
    [SerializeField] private CanvasGroup educationCanvasGroup;

    private void Awake()
    {
        HideAllManageModals();
    }   

    public void HideAllManageModals()
    {
        HideIntro();
        HideEducation();
        HideConfirmExit();
        HideDeveloperInfo();
        HideSettings();
    }

    // ----------------------
    // Intro Overlay
    // ----------------------
    public void ShowIntro()
    {
        if(introOverlayPanel != null)
        {
            introOverlayPanel.SetActive(true);
        }
    }

    public void HideIntro()
    {
        if(introOverlayPanel != null)
        {
            introOverlayPanel.SetActive(false);
        }
    }

    // ----------------------
    // Education Popup
    // ----------------------
    public void ShowEducation(String fact)
    {
        if (educationPopupPanel != null)
        {
          educationPopupPanel.SetActive(true); 
        }
        if (educationCanvasGroup != null)
        {
            educationCanvasGroup.blocksRaycasts = true; 
        }
        if (eduationTitleText != null)
        {
            eduationTitleText.text = "Did you know?";
        }
        if (educationBodyText != null)
        {
            educationBodyText.text = fact;
        }
        
    }

    public void HideEducation()
    {
        if (educationCanvasGroup != null)
        {
            educationCanvasGroup.blocksRaycasts = false; 
        }
         if (educationPopupPanel != null)
        {
            educationPopupPanel.SetActive(false);
        }
    }

    // ----------------------
    // Confirm Exit
    // ----------------------
    public void ShowConfirmExit()
    {
        if (confirmExitPanel != null)
        {
            confirmExitPanel.SetActive(true);
        }
    }

    public void HideConfirmExit()
    {
        if (confirmExitPanel != null)
        {
            confirmExitPanel.SetActive(false);
        }
    }

    // ----------------------
    // Developer Info
    // ----------------------

    public void ShowDeveloperInfo()
    {
        if (developerInfoPanel != null)
        {
            developerInfoPanel.SetActive(true);
        }
    }

    public void HideDeveloperInfo()
    {
        if (developerInfoPanel != null)
        {
            developerInfoPanel.SetActive(false);
        }
    }

    // ----------------------
    // Settings Overlay
    // ----------------------

    public void ShowSettings()
    {
        if (settingsOverlay != null)
        {
            settingsOverlay.SetActive(true);
        }
    }

    public void HideSettings()
    {
        if (settingsOverlay != null)
        {
            settingsOverlay.SetActive(false);
        }
    }


}