
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManger : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private PopupUI popupUI;
    [SerializeField] private TMP_Text messageText;
   

    [Header("Controls")]
    [SerializeField] private InputDragLaunch dragLaunch;

    [Header("Intro Overlay")]
    [SerializeField] private GameObject introOverlayPanel;
    [SerializeField] private float introDuration = 3f;

    [Header("Eduational Popup")]
    [SerializeField] private EducationalPopupController educationalPopupController;

    [Header ("Confirm Exit Modal")]
    [SerializeField] private GameObject confirmExitPanel;
    [SerializeField] private UnityEngine.UI.Button confirmExitYesButton;
    [SerializeField] private UnityEngine.UI.Button confirmExitNoButton;

    [Header ("Info Panel")]
    [SerializeField] private GameObject developerInfoPanel;

    //Static flag: lets us skip intro after reset reloads the scene
    private static bool skipIntroNextLoad = false;

    private bool gameEnded = false;


    // --------------------------------
    // SCORE SYSTEM VARIABLES
    // --------------------------------
    private float accuracyScore = 0f;
    private float fuelScore = 0f;
    private float totalScore = 0f;

    [Header("Scoring Settings")]
    [SerializeField] private float accuracyWeight = 0.7f;
    [SerializeField] private float fuelWeight = 0.3f;
    [SerializeField] private float maxOrbitError = 5f; // Maximum error distance for scoring
    [SerializeField] private float maxLaunchPower = 10f; // max drag power used for normalization  

    // --------------------------------
    // Game Event Handlers
    // --------------------------------

    // --------------------------------
    // OLD EVENt (no score)
    // --------------------------------
    public void OrbitSuccess()
    {
        if (gameEnded) return;
       
        gameEnded = true;

        // Force stop any ongoing drag/thrust sounds
        if(dragLaunch != null)
        {
            AudioManager.Instance.StopAllGameplaySounds();
        }
        // stop background sound if needed
        AudioManager.Instance.StopBackground();
        // audio
        AudioManager.Instance.PlaySuccess();

        ShowMessage("Orbit Achieved!");
        DisableControl();
    }

    // --------------------------------
    // Main Success Event with Scoring
    // --------------------------------

    public void OrbitSuccess(float orbitError, float launchPower)
    {
        if (gameEnded) return;
         Debug.Log("game manager received orbit success event");
        gameEnded = true;

        // Force stop any ongoing drag/thrust sounds
        if(dragLaunch != null)
        {
            AudioManager.Instance.StopAllGameplaySounds();
        }
        // stop background sound if needed
        AudioManager.Instance.StopBackground();

        // Calculate score based on accuracy and fuel efficiency
        CalculateScores(orbitError, launchPower);

        // audio
        AudioManager.Instance.PlaySuccess();

        // prefer the popup UI if assigned
       if(popupUI != null)
        {
            popupUI.ShowSuccessPopup(totalScore);
        }
        else
        {
            // Fallback: old HUD message
            ShowMessage("Orbit Achieved!\n Score: " + totalScore.ToString("F0"));
        }
        DisableControl();
    }

    // --------------------------------
    // Failure Event
    // --------------------------------
    public void MissedOrbit()
    {
        if (gameEnded) return;
            Debug.Log("game manager received missed orbit event");
        gameEnded = true;

        // Force stop any ongoing drag/thrust sounds
        if(dragLaunch != null)
        {
            AudioManager.Instance.StopAllGameplaySounds();
        }
        // stop background sound if needed
        AudioManager.Instance.StopBackground();

        // audio
        AudioManager.Instance.PlayFail();

        // prefer the popup UI if assigned
        if (popupUI != null)
        {
            popupUI.ShowFailurePopup();
        }
        else
        {
            // Fallback: old HUD message
            ShowMessage("Orbit Missed! Try Again.");
        }
        DisableControl();
    }

    // --------------------------------
    // Score Calculation
    // --------------------------------
    private void CalculateScores(float orbitError, float launchPower)
    {
        // Accuracy score decreases with orbit error
        accuracyScore = Mathf.Clamp01(1f - (orbitError / maxOrbitError)) * 100f;

        // Fuel score decreases with launch power used
        fuelScore = Mathf.Clamp01(1f - (launchPower / maxLaunchPower)) * 100f;

        // Weighted total score
        totalScore = (accuracyScore * accuracyWeight) + (fuelScore * fuelWeight);

        Debug.Log($"[Score System] Accuracy: {accuracyScore:F1}, Fuel: {fuelScore:F1}, Total: {totalScore:F1}");
    }

    // --------------------------------
    // UI and Control Helpers  
    // --------------------------------

    private void ShowMessage(string msg)
    {
        if (messageText != null)
        {
            messageText.text = msg;
            messageText.gameObject.SetActive(true);
        }
    }
    
    private void DisableControl()
    {
        if (dragLaunch != null)
        {
            dragLaunch.enabled = false;
        }
    }

    // --------------------------------
    // Restart Game
    // --------------------------------
    public void ResetGame()
    {
        // button click sound should still play fully
        AudioManager.Instance.PlayButtonClick();

        // stop all lingering sounds
        AudioManager.Instance.StopBackground();
        if(dragLaunch != null)
        {
            AudioManager.Instance.StopAllGameplaySounds();
        }

        skipIntroNextLoad = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // --------------------------------
    // Back to Central Hub
    // --------------------------------
    public void GoBackToCentralHub()
    {
        StartCoroutine(GoBackRountine());
    } 
    private IEnumerator GoBackRountine()
    {
        // play click
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }

        // Stop background/gameplay sounds
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopBackground();
            AudioManager.Instance.StopAllGameplaySounds();
        }

        // let the click sound start
        yield return new WaitForSeconds(0.2f);

        SceneManager.LoadScene("CentralHub");
    }  

    // --------------------------------
    // Back Button Confirmation Screen
    // --------------------------------
    public void ShowConfirmExit()
    {
        AudioManager.Instance.PlayButtonClick();
        if (confirmExitPanel != null)
        {
            confirmExitPanel.SetActive(true);
        }
    }

    public void HideConfirmExit()
    {
        if (confirmExitPanel != null)
        {
            confirmExitPanel.SetActive(false);
        }
    }

    public void ConfirmExitYes()
    {
        HideConfirmExit();
        GoBackToCentralHub();
    }

    public void ConfirmExitNo()
    {
        AudioManager.Instance.PlayButtonClick();
        HideConfirmExit();
    }

    // --------------------------------
    // Developer Info Panel
    // --------------------------------
    public void ShowDeveloperInfo()
    {
        AudioManager.Instance.PlayButtonClick();
        if (developerInfoPanel != null)
        {
            developerInfoPanel.SetActive(true);
        }
    }

    public void HideDeveloperInfo()
    {
        AudioManager.Instance.PlayButtonClick();
        if (developerInfoPanel != null)
        {
            developerInfoPanel.SetActive(false);
        }
    }

    // --------------------------------
    // Educational Popup Handling
    // --------------------------------
    private void ShowEduationalPopup()
    {
        if (educationalPopupController != null)
        {
            educationalPopupController.ShowEducationalPopup();
           
        }
    }

    private void Start()
    {
        // Start background space ambience
        AudioManager.Instance.PlayBackground();

        // if we just reset, skip intro one time
        if (skipIntroNextLoad)
        {
            skipIntroNextLoad = false;
            if(introOverlayPanel != null)
                introOverlayPanel.SetActive(false);
            
            ShowEduationalPopup(); // show educational popup 
            return;
        }
        // Normal entry from hub -> show intro
        StartCoroutine(IntroRountine());
      
    }

    // --------------------------------
    // Intro Overlay Coroutine 
    // --------------------------------
    private System.Collections.IEnumerator IntroRountine()
    {
        ShowIntro();
        
        float t = 0f;
        while (t < introDuration)
        {
            // dkip only on tap/click start (not dragging)
            if (Input.GetMouseButtonDown(0))
            {
                break;
            }
            if(Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                break;
            }
           
            t += Time.deltaTime;
            yield return null;
        }
        HideIntro();
    }


    private void ShowIntro() 
    {
        if (introOverlayPanel != null)
        {
            introOverlayPanel.SetActive(true);
        }
        DisableControl();
        
    }

    private void HideIntro() 
    {
        if (introOverlayPanel != null)
        {
            introOverlayPanel.SetActive(false);
        }
        ShowEduationalPopup(); // show educational popup after intro
      
    }

}
