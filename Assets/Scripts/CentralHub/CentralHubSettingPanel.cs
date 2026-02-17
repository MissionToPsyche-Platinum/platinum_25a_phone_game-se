using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;


public class CentralHubSettingPanel : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject settingOverlay;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private TMP_Text volumePercentText;

    [Header("Audio Manager")]
    [SerializeField] private CentralHubAudioManager audioManager;

    [Header("Default")]
    [SerializeField] private float defaultVolume = 0.2f;

    private const string PREF_KEY= "CentralHubVolume";

    private void Awake()
    {
        if(settingOverlay != null)
        {
            settingOverlay.SetActive(false);
        }

        float saved = PlayerPrefs.GetFloat(PREF_KEY, defaultVolume);
        saved = Mathf.Clamp01(saved);

        if(volumeSlider != null)
        {
            volumeSlider.minValue = 0f;
            volumeSlider.maxValue = 1f;
            volumeSlider.value = saved;
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }

        ApplyVolume(saved);
        UpdatePercentText(saved);

        if(settingsButton != null)
        {
            settingsButton.onClick.AddListener(ToggleSettings);
        }
    }

    public void ToggleSettings()
    {
        if(settingOverlay == null) return;
        settingOverlay.SetActive(!settingOverlay.activeSelf);
    }

    public void OnOverlayClicked()
    {
        if(settingOverlay != null && settingOverlay.activeSelf)
        {
            settingOverlay.SetActive(false);
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
        if(audioManager != null)
        {
            audioManager.SetHubMasterVolume(volume);
        }
    }

    private void UpdatePercentText(float value)
    {
        if(volumePercentText == null) return;
            int percent = Mathf.RoundToInt(value * 100f);
            volumePercentText.text = $"{percent}%";

    }


}
