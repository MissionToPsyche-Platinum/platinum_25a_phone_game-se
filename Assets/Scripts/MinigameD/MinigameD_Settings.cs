using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MinigameD_Settings : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject settingOverlay;
    [SerializeField] private GameObject settingsCard;
    [SerializeField] private Button settingsButton;

    [SerializeField] private Slider volumeSlider;
    [SerializeField] private TMP_Text volumePercentText;
    [SerializeField] private Toggle tutorialToggle;
    [SerializeField] private Toggle fontToggle;
    [SerializeField] private Toggle storyToggle;

    [Header("Default")]
    [SerializeField] private float defaultVolume = 0.2f;
    [SerializeField] private bool defaultTutorial = true;
    [SerializeField] private bool defaultFont = false;
    [SerializeField] private bool defaultStory = true;

    private const string PREF_KEY = "MasterVolume";
    private const string PREF_TUT_KEY = "TutorialsOn";
    private const string PREF_FONT_KEY = "AccessibleFont";
    private const string PREF_STORY_KEY = "MinigameD-Story";
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
        bool savedStory = PlayerPrefs.GetInt(PREF_STORY_KEY, defaultStory ? 1 : 0) == 1;
        if (storyToggle != null)
        {
            storyToggle.isOn = savedStory;
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

    public void toggleStory()
    {
        defaultStory = !defaultStory;
        PlayerPrefs.SetInt(PREF_STORY_KEY, defaultStory ? 1 : 0);
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

        if (Input.GetMouseButtonDown(0))
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
