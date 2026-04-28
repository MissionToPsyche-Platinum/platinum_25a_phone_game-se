using UnityEngine;
using TMPro;
using UnityEngine.UI;


public class MusicVolumeSliderPrefabController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Slider slider;
    [SerializeField] private TMP_Text percentText;
    [SerializeField] private float defaultVolume = 0.7f;

    private const string PREF_KEY= "MusicVolume";

    private System.Action<float> setVolumeAction;

    private void OnEnable()
    {
        if (slider != null) {slider.onValueChanged.AddListener(OnVolumeChanged);}

        float saved = Mathf.Clamp01(PlayerPrefs.GetFloat(PREF_KEY, defaultVolume)); // Load saved volume or use default

        if (slider != null) {slider.SetValueWithoutNotify(saved);}

        UpdatePercent(saved);
       
    }

    private void OnDisable()
    {
        if (slider != null) {slider.onValueChanged.RemoveListener(OnVolumeChanged);}
    }

    private void OnVolumeChanged(float v)
    {
        v = Mathf.Clamp01(v);
        PlayerPrefs.SetFloat(PREF_KEY, v);
        PlayerPrefs.Save();

        UpdatePercent(v);
    }

    private void OnDestroy()
    {
        if (slider != null)
        {
            slider.onValueChanged.RemoveListener(OnVolumeChanged);
        }
    }

    private void UpdatePercent(float v)
    {
        if (percentText == null) return;
        int pct = Mathf.RoundToInt(v * 100f);
        percentText.text = pct + "%";
    }

}
