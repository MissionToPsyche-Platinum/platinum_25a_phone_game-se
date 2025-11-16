using UnityEngine;
using TMPro;
using UnityEngine.UI;
using NUnit.Framework.Interfaces;

public class PopupUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject popupPanel;
    [SerializeField] private TMP_Text popupTitle;
    [SerializeField] private TMP_Text popupMessage;

    void Awake()
    {
       HidePopup(); 
    }

    public void ShowPopup(string title, string message)
    {
        if(popupPanel != null)
            popupPanel.SetActive(true);
        if(popupTitle != null)
            popupTitle.text = title;
        if(popupMessage != null)
            popupMessage.text = message;
    }

    public void HidePopup()
    {
        if(popupPanel != null)
            popupPanel.SetActive(false);
    }
}
