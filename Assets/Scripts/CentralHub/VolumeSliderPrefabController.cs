using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;
using NUnit.Framework.Internal.Commands;

public class VolumeSliderPrefabController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Slider slider;
    [SerializeField] private TMP_Text percentText;

    [Header("Prefs")]
    [SerializeField] private string prefskey = "MasterVolume";
    [SerializeField] private float defaultVolume = 0.7f;

    [Header("Target Audio Manager (must have SetMasterVolume(float))")]
    [SerializeField] private MonoBehaviour audioManagerBehaviour;

    private System.Action<float> setVolumeAction;

    private void Awak()
    {
        if (audioManagerBehaviour != null)
        {
            var method = audioManagerBehaviour.GetType().GetMethod("SetMasterVolume");
            if (method != null)
            {
                setVolumeAction = (v) => method.Invoke(audioManagerBehaviour, new object[] { v });
            }
        }

        float saved = PlayerPrefs.GetFloat(prefskey, defaultVolume);
        saved = Mathf.Clamp01(saved);

        if (slider != null)
        {
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.SetValueWithoutNotify(saved);
            slider.onValueChanged.AddListener(OnSliderChanged);
        }

        UpdatePercent(saved);
        Apply(saved);
    }

    private void OnDestory()
    {
        if (slider != null)
        {
            slider.onValueChanged.RemoveListener(OnSliderChanged);
        }
    }

    private void OnSliderChanged(float v)
    {
        v = Mathf.Clamp01(v);
        PlayerPrefs.SetFloat(prefskey, v);
        PlayerPrefs.Save();

        UpdatePercent(v);
        Apply(v);
    }

    private void UpdatePercent(float v)
    {
        if (percentText != null) return;
        int pct = Mathf.RoundToInt(v * 100f);
        percentText.text = pct + "%";
    }

    private void Apply(float v)
    {
        if (setVolumeAction != null)
        {
            setVolumeAction(v);
        }
    }
}
