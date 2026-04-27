using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class MinigameDescriptionModalController : MonoBehaviour
{
    [Header("Modal UI References")]
    [SerializeField] private GameObject minigameDescriptionOverlay;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text relationText;

    [Header("Scene Switching")]
    [SerializeField] private MenuSceneSchange menuSceneSchange;

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
            "Solve a zebra-style logic puzzle based on space-related clues to determine correct associations and relationships.",
            "This minigame introduces players to the Psyche mission by building foundational knowledge about the asteroid and its characteristics, helping them understand why the mission is important.",
            "MinigameA"
        );
    }

    public void OpenDesignModal()
    {
        Debug.Log("Opening Design Modal");
        ShowModal(
            "Design",
            "Complete a series of randomized math and logic-based puzzles that simulate problem-solving and decision-making challenges.",
            "This phase reflects the design process of a space mission, where engineers analyze constraints and make critical decisions to ensure a successful spacecraft design.",
            "MinigameB"
        );
    }

    public void OpenBuildModal()
    {
        Debug.Log("Opening Build Modal");
        ShowModal(
            "Build",
            "Collect and deliver components to the correct teams to assemble the Psyche spacecraft piece by piece, including communication systems and structural elements.",
            "This minigame represents the build phase of the Psyche mission, where all spacecraft components are constructed and integrated before launch.",
            "MinigameC"
        );
    }

    public void OpenLaunchModal()
    {
        Debug.Log("Opening Launch Modal");
        ShowModal(
            "Launch",
            "Guide the Psyche spacecraft through a three-level launch sequence. Navigate through target rings while avoiding obstacles to maintain speed and successfully reach the final ejection point.",
            "This minigame represents the Launch phase of the Psyche mission, where a SpaceX Falcon Heavy rocket carried the spacecraft beyond Earth's gravity and placed it on its journey toward the Psyche asteroid.",
            "MinigameD-Start-Menu"
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
            "Navigate through orbital paths around the Psyche asteroid, collecting scientific data while avoiding obstacles and hazards.",
            "This phase represents the final stage of the Psyche mission, where the spacecraft orbits the asteroid to gather data about its surface, composition, gravity, and structure.",
            "MinigameF"
        );
    }

    public void PlaySelectedMinigame()
    {
       if (string.IsNullOrEmpty(selectedSceneName))
        {
            Debug.LogWarning("No minigame selected. Please select a minigame before trying to play.");
            return;
        }

        

        // Special case for MinigameB
        if(selectedSceneName == "MinigameB")
        {
            if(menuSceneSchange != null)
            {
            menuSceneSchange.SwitchToSceneMinigameB();
            }
            else
            {
                Debug.LogError("MenuSceneSchange reference is missing. Cannot switch to Minigame B.");
            }
            return;
        }

        // Open minigameF start screen
        if(selectedSceneName == "MinigameF")
        {
            if (menuSceneSchange != null)
            {
                menuSceneSchange.SwitchToSceneMinigameF_Start();
            }
            else
            {
                Debug.LogError("MenuSceneSchange reference is missing. Cannot switch to Minigame F.");
            }
            return;
        }
            // For other minigames, load the selected scene directly
            SceneManager.LoadScene(selectedSceneName);
        
    }

    public void CloseModal()
    {
        if (minigameDescriptionOverlay != null)
        {
            minigameDescriptionOverlay.SetActive(false);
        }
        selectedSceneName = null;
    }

    public string GetSelectedSceneName()
    {
        return selectedSceneName;
    }
}
