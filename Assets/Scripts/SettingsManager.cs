using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;


public class SettingsManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject settingOverlay;
    [SerializeField] private GameObject settingsCard;
    [SerializeField] private Button settingsButton;

    [SerializeField] private Slider volumeSlider;
    [SerializeField] private TMP_Text volumePercentText;
    [SerializeField] private Toggle tutorialToggle;
    [SerializeField] private Toggle fontToggle;

    [Header("Managers")]
    [SerializeField] private AudioClipManager audioClipManager;
    [SerializeField] private MusicManager musicManager;

    [Header("Default")]
    [SerializeField] private float defaultVolume = 0.2f;
    [SerializeField] private bool defaultTutorial = true;
    [SerializeField] private bool defaultFont = false;

    private const string PREF_KEY = "MasterVolume";
    private const string PREF_TUT_KEY = "TutorialsOn";
    private const string PREF_FONT_KEY = "AccessibleFont";
    private bool isOpen = false;
    private void Awake()
    {
        if (settingOverlay != null)
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
        bool savedTutorial = PlayerPrefs.GetInt(PREF_TUT_KEY, defaultTutorial ? 1 : 0) == 1;
        if (tutorialToggle != null)
        {
            tutorialToggle.isOn = savedTutorial;
        }
        bool savedFont = PlayerPrefs.GetInt(PREF_FONT_KEY, defaultFont ? 1 : 0) == 1;
        if (fontToggle != null)
        {
            fontToggle.isOn = savedFont;
        }

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

        UpdatePercentText(value);
        if (audioClipManager != null)
        {
            audioClipManager.UpdateVolume();
        }
        if (musicManager != null)
        {
            musicManager.UpdateVolume();
        }
    }

    private void UpdatePercentText(float value)
    {
        if (volumePercentText == null) return;
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
