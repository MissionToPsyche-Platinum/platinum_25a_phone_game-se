using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;


public class CentralHubSettingPanel : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject settingOverlay;
    [SerializeField] private GameObject settingsCard;
    [SerializeField] private Button settingsButton;

    [SerializeField] private Slider volumeSlider;
    [SerializeField] private TMP_Text volumePercentText;

    [Header("Audio Manager")]
    [SerializeField] private CentralHubAudioManager audioManager;


    [Header("Default")]
    [SerializeField] private float defaultVolume = 0.2f;

    private const string PREF_KEY= "MasterVolume";
    private bool isOpen = false;
    private void Awake()
    {
        if(settingOverlay != null)
        {
            settingOverlay.SetActive(false);
        }
        if (settingsButton != null)
        {
            settingsButton.onClick.AddListener(Toggle);
        }
        float saved = Mathf.Clamp01(PlayerPrefs.GetFloat(PREF_KEY, defaultVolume));
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

        if (settingsButton != null)
            settingsButton.onClick.RemoveListener(Toggle);
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
        if (settingOverlay == null) return;
            settingOverlay.SetActive(true);
            isOpen = true;
    }

    public void Close()
    {
        if (settingOverlay == null) return;
            settingOverlay.SetActive(false);
            isOpen = false;
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
            audioManager.SetMasterVolume(volume);
        }
    }

    private void UpdatePercentText(float value)
    {
        if(volumePercentText == null) return;
            int percent = Mathf.RoundToInt(value * 100f);
            volumePercentText.text = $"{percent}%";

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


}
