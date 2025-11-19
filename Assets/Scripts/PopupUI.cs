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

        if(popupTitle != null)
        {
            popupTitle.text = "Orbit Achieved!";
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

        if (popupTitle != null)
        {
            popupTitle.text = "Orbit Missed!";
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
