using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EducationalPopupController : MonoBehaviour
{
    [Header("Modal Manager")]
    [SerializeField] private ModalManager modalManager;

    [Header("Gameplay Control")]
    [SerializeField] private InputDragLaunch dragLaunch;

    private string[] facts;

    private const string PREF_DISABLE_EDU = "MinigameE_DisableEducationalPopups";
    private void Awake()
    {
        // initialize facts
        facts = new string[]
        {
            "Psyche is a metal-rich asteroid that scientists believe may be the exposed core of an early planet.",
            "The Psyche spacecraft uses solar-electric propulsion to travel more than 2.2 billion miles through space.",
            "Psyche’s surface may contain nickel-iron metal similar to the material inside Earth’s core.",
            "The mission will help scientists learn how rocky planets like Earth and Mars formed billions of years ago.",
            "Psyche orbits the Sun between Mars and Jupiter in the main asteroid belt.",
            "The spacecraft uses Hall-effect thrusters, which accelerate ions to produce extremely efficient thrust.",
            "Psyche’s metal surface may reflect sunlight differently than rocky asteroids, making it shine more brightly.",
            "Scientists think Psyche may contain ancient magnetic fields frozen into its metal from early solar system history.",
            "The mission includes a technology demonstration that tests NASA’s new high-bandwidth laser communication system.",
            "Studying Psyche could reveal how violent collisions shaped the early solar system’s planets and materials."
        };
    }

    // ---------------------------------------------------------
    // Show a random educational message
    // ---------------------------------------------------------
    public void ShowEducationalPopup()
    {

        bool isDisabled = PlayerPrefs.GetInt(PREF_DISABLE_EDU, 0) == 1;

        if (isDisabled)
        {
            if(dragLaunch != null)
                dragLaunch.enabled = true; 
            
            return;
        }

        if (dragLaunch != null)
            dragLaunch.enabled = false; 

        string fact = facts[Random.Range(0, facts.Length)];
        if (modalManager != null)
        {
            modalManager.ShowEducation(fact);
        }
    }

    // ---------------------------------------------------------
    // Called by the Continue button
    // ---------------------------------------------------------
    public void CloseEducationalPopup()
    {
        if (modalManager != null)
        {
            modalManager.HideEducation();
        }

        if (dragLaunch != null)
            dragLaunch.enabled = true;
    }

}
