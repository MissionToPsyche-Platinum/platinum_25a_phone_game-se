using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MinigameDescriptionModalController : MonoBehaviour
{
    [Header("Modal UI References")]
    [SerializeField] private GameObject minigameDescriptionOverlay;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text relationText;

    private string selectedSceneName;

    private void Awake()
    {
        if (minigameDescriptionOverlay != null)
        {
            minigameDescriptionOverlay.SetActive(false);
        }
    }

    private void ShowModal(string gameTitle, string description, string relation, string sceneName)
    {
        if (minigameDescriptionOverlay != null)
        {
            minigameDescriptionOverlay.SetActive(true);
        }
        if (titleText != null)
        {
            titleText.text = gameTitle;
        }
        if (descriptionText != null)
        {
            descriptionText.text = description;
        }
        if (relationText != null)
        {
            relationText.text = relation;
        }
        selectedSceneName = sceneName;
    }

    public void OpenPlanModal()
    {
        Debug.Log("Opening Plan Modal");
        ShowModal(
            "Plan",
            "Desciption coming soon.",
            "Pysche mission connection coming soon.",
            "MinigameA"
        );
    }

    public void OpenDesignModal()
    {
        Debug.Log("Opening Design Modal");
        ShowModal(
            "Design",
            "Desciption coming soon.",
            "Pysche mission connection coming soon.",
            "MinigameBMetalWeights"
        );
    }

    public void OpenBuildModal()
    {
        Debug.Log("Opening Build Modal");
        ShowModal(
            "Build",
            "Desciption coming soon.",
            "Pysche mission connection coming soon.",
            "MinigameC"
        );
    }

    public void OpenLaunchModal()
    {
        Debug.Log("Opening Launch Modal");
        ShowModal(
            "Launch",
            "Guide the Pscyhe spacecraft through a three-level launch sequence.Navigate through target rings while avoiding obstcales to maintain speed and successfully reach the final ejection point.",
            "This minigame represents the Launch phase of the Psyche mission, where a SpaceX Falcon heavy rocket carried the spacecraft beyond Earth's gravity and placed it on its journey towar the Psyche asteroid.",
            "MinigameD-Tutorial"
        );
    }

    public void OpenGravityModal()
    {
        Debug.Log("Opening Gravity Assist Modal");
        ShowModal(
            "Gravity Assist",
            "This minigame gives the player a short introduction to the selected phase and explains what the challenge represents",
            "This stage reflects part of the Psyche mission journey and helps the player understand how that phase contributes to reaching and studying the asteroid.",
            "GravityAssist"
        );
    }

    public void OpenOrbitModal()
    {
        Debug.Log("Opening Orbit Modal");
        ShowModal(
            "Orbit",
            "Desciption coming soon.",
            "Pysche mission connection coming soon.",
            "MinigameF"
        );
    }

    public string GetSelectedSceneName()
    {
        return selectedSceneName;
    }
}
