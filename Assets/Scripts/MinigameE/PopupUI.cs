using UnityEngine;
using TMPro;
using UnityEngine.UI;
using NUnit.Framework.Interfaces;

public class PopupUI : MonoBehaviour
{
    [Header("Root Panel")]
    [SerializeField] private GameObject popupPanel;

    [Header("UI References")]
    [SerializeField] private TMP_Text popupTitle;
    [SerializeField] private TMP_Text popupMessage;

    void Awake()
    {
        Debug.Log("PopupUI Awake called");
       HidePopup(); 
    }

    public void ShowSuccessPopup(float totalScore)
    {
        Debug.Log("Showing success popup");
       if(popupPanel != null)
        {
            popupPanel.SetActive(true);
            Debug.Log("Popup panel activated");
        }
        else    
        {
            Debug.LogError("Popup panel reference is missing!");
        }
        AudioManager.Instance.StopAllGameplaySounds();
        AudioManager.Instance.PlaySuccess();

        if(popupTitle != null)
        {
            popupTitle.text = "Gravity Assist Achieved!";
        }

        if(popupMessage != null)
        {
            popupMessage.text = $"Score: {totalScore:F0}";

        }
    }

    public void ShowFailurePopup()
    {
        Debug.Log("Showing failure popup");
        if (popupPanel != null)
        {
            popupPanel.SetActive(true);
            Debug.Log("Popup panel activated for failure");
        }
        else   
        {
            Debug.LogError("Popup panel reference is missing for failure popup!");
        }

        AudioManager.Instance.StopAllGameplaySounds();
        AudioManager.Instance.PlayFail();

        if (popupTitle != null)
        {
            popupTitle.text = "Orbit Missed - No Gravity Assist!";
        }

        if (popupMessage != null)
        {
            popupMessage.text ="Try another launch to achieve orbit.";
        }
    }
    public void HidePopup()
    {
       // AudioManager.Instance.PlayBackground();
        if(popupPanel != null)
            { popupPanel.SetActive(false);
            Debug.Log("Popup panel deactivated"); }
            else {
        Debug.LogError("Popup panel reference is missing when trying to hide!");
    }
    }
    
}
