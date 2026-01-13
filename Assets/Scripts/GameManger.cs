
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManger : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private PopupUI popupUI;
    [SerializeField] private TMP_Text messageText;

    [Header("Controls")]
    [SerializeField] private InputDragLaunch dragLaunch;

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
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void Start()
    {
        // Start bbackground space ambience
        AudioManager.Instance.PlayBackground();
      
    }


}
