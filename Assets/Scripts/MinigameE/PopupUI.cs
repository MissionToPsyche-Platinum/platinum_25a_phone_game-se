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
       HidePopup(); 
    }

    public void ShowSuccessPopup(float totalScore)
    {
       if(popupPanel != null)
        {
            popupPanel.SetActive(true);
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
        if (popupPanel != null)
        {
            popupPanel.SetActive(true);
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
        if(popupPanel != null)
             popupPanel.SetActive(false);
    }
    
}
