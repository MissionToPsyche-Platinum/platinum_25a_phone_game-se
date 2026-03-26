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
    [SerializeField] private Toggle tutorialToggle;
    [SerializeField] private Toggle fontToggle;
    [SerializeField] private Toggle popupsToggle;

    [Header("Audio Manager")]
    [SerializeField] private CentralHubAudioManager audioManager;


    [Header("Default")]
    [SerializeField] private float defaultVolume = 0.2f;
    [SerializeField] private bool defaultTutorial = true; 
    [SerializeField] private bool defaultFont = false;
    [SerializeField] private bool defaultPopups = true;

    private const string PREF_KEY= "MasterVolume";
    private const string PREF_TUT_KEY= "TutorialsOn";
    private const string PREF_FONT_KEY= "AccessibleFont";
    private const string PREF_POP_UP_KEY = "Pop-ups";
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
        defaultTutorial = PlayerPrefs.GetInt(PREF_TUT_KEY, defaultTutorial ? 1 : 0) == 1;
        if (tutorialToggle != null)
        {
            tutorialToggle.isOn = defaultTutorial;
        }
        defaultFont = PlayerPrefs.GetInt(PREF_FONT_KEY, defaultFont ? 1 : 0) == 1;
        if (fontToggle != null)
        {
            fontToggle.isOn = defaultFont;
        }
        defaultPopups = PlayerPrefs.GetInt(PREF_POP_UP_KEY, defaultPopups ? 1 : 0) == 1;
        if (popupsToggle != null)
        {
            popupsToggle.isOn = defaultPopups;
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

    public void toggleTutorials()
    {
        defaultTutorial = !defaultTutorial;
        PlayerPrefs.SetInt(PREF_TUT_KEY, defaultTutorial ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void toggleFont()
    {
        defaultFont = !defaultFont;
        PlayerPrefs.SetInt(PREF_FONT_KEY, defaultFont ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void togglePopups()
    {
        defaultPopups = !defaultPopups;
        PlayerPrefs.SetInt(PREF_POP_UP_KEY, defaultPopups ? 1 : 0);
        PlayerPrefs.Save();
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
