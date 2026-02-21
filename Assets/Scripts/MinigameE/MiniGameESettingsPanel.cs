using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class MiniGameESettingsPanel : MonoBehaviour
{
    [Header("Settings Panel")]
    [SerializeField] private GameObject settingsOverlay;
    [SerializeField] private GameObject settingsCard;
    [SerializeField] private Button SettingsButton;

    [Header("Volume Controls")]
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private TMP_Text volumePercentText;
    [SerializeField] private AudioManager audioManager;

    [Header("Default Volume")]
    [Range(0f, 1f)]
    [SerializeField] private float defaultVolume = 0.7f;

    private const string PREF_KEY= "MasterVolume";

    private bool isOpen = false;
    private void Awake()
    {
        if (settingsOverlay != null)
            settingsOverlay.SetActive(false); // Ensure overlay is hidden at start

        if (SettingsButton != null)
            SettingsButton.onClick.AddListener(Toggle); // Open settings when button is clicked

        float saved = Mathf.Clamp01(PlayerPrefs.GetFloat(PREF_KEY, defaultVolume)); // Load saved volume or use default
        if (volumeSlider != null)
        {
            volumeSlider.minValue = 0f;
            volumeSlider.maxValue = 1f;
            volumeSlider.value = saved;
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }

        ApplyVolume(saved);
        UpdatePercentText(saved);
    }

    private void OnDestroy()
    {
        if (volumeSlider != null)
            volumeSlider.onValueChanged.RemoveListener(OnVolumeChanged);

        if (SettingsButton != null)            
            SettingsButton.onClick.RemoveListener(Toggle);
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
            
            if (settingsOverlay == null) return;
            
            settingsOverlay.SetActive(true); // Show the settings overlay
            isOpen = true;
    }

    public void Close()
    {
        if (settingsOverlay == null) return;
        if (audioManager != null)
            audioManager.PlayButtonClick(); // Play click sound when closing
            
            settingsOverlay.SetActive(false);
            isOpen = false;

            
    }

    private void Update()
    {
        if (!isOpen) return;

        if (Input.GetMouseButtonDown(0)) // Check for left mouse click
        {
           
           if (EventSystem.current.IsPointerOverGameObject())
            {
                if (settingsCard != null)
                    {
                        var rect = settingsCard.GetComponent<RectTransform>();
                        if (rect != null && !RectTransformUtility.RectangleContainsScreenPoint(rect, Input.mousePosition, null))
                        {
                            Close();
                        }
                    }
                
            }
        }  
    }

    private void OnVolumeChanged(float value)
    {
        value = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(PREF_KEY, value);
        PlayerPrefs.Save();

        ApplyVolume(value);
        UpdatePercentText(value);
    }

    private void ApplyVolume(float volume)
    {
        if (audioManager != null)
        {
            audioManager.SetMasterVolume(volume);
        }
    }

    private void UpdatePercentText(float value)
    {
        if (volumePercentText == null) return;
        
            int percent = Mathf.RoundToInt(value * 100);
            volumePercentText.text = percent + "%";
        
    }

}
