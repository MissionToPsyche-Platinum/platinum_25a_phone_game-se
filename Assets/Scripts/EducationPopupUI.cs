using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EducationPopupUI : MonoBehaviour
{
    [Header("Root Panel")]
    [SerializeField] private GameObject panel;

    [Header("UI References")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text bodyText;
    [SerializeField] private CanvasGroup canvasGroup;

    private void Awake()
    {
        Hide();
    }

    // Show popup with dynamic fact
    public void Show(string fact)
    {
        if(panel != null)
            panel.SetActive(true);

        if(canvasGroup != null)
            canvasGroup.blocksRaycasts = true;
        
        if(titleText != null)
            titleText.text = "Did You Know?";

        if (bodyText != null)
            bodyText.text = fact;
    }

    public void Hide()
    {
        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = false;
            
        if (panel !=null)
            panel.SetActive(false);
    }


}
