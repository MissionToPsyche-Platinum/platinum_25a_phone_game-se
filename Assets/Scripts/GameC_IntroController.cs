using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameC_IntroController : MonoBehaviour
{
    [Header("UI Elements")]
    // No buttons needed - automatic transition to game
    
    [Header("Visual Elements")]
    public GameObject psycheLogo;
    public GameObject floatingPapers;
    public GameObject psycheAsteroidSilhouette;
    public GameObject backgroundOverlay; // For mobile contrast
    
    [Header("Audio")]
    public AudioSource backgroundMusic;
    public AudioSource voiceoverAudio;
    
    [Header("Text Elements")]
    public TextMeshProUGUI missionText;
    public TextMeshProUGUI phaseCText;
    
    private void Start()
    {
        SetupMobileLayout();
        SetupVisuals();
        HidePhaseText();
        PlayIntroSequence();
    }
    
    private void SetupMobileLayout()
    {
        // Setup mobile-optimized layout
        SetupMobileTextSizes();
        SetupBackgroundContrast();
    }
    
    private void SetupMobileTextSizes()
    {
        // Mobile-optimized text sizes
        if (missionText != null)
        {
            missionText.fontSize = 18f; // Mobile-friendly size
            missionText.lineSpacing = 1.2f;
        }
        
        if (phaseCText != null)
        {
            phaseCText.fontSize = 26f; // Mobile-friendly size
        }
    }
    
    private void SetupBackgroundContrast()
    {
        // Ensure background overlay exists for mobile readability
        if (backgroundOverlay != null)
        {
            Image overlayImage = backgroundOverlay.GetComponent<Image>();
            if (overlayImage != null)
            {
                overlayImage.color = new Color(0, 0, 0, 0.4f); // Semi-transparent black for contrast
            }
        }
    }
    
    // Removed SetupButtons method - no buttons needed for automatic transition
    
    private void SetupVisuals()
    {
        if (psycheLogo != null)
        {
            StartCoroutine(AnimateLogo());
        }

        if (floatingPapers != null)
        {
            StartCoroutine(AnimateFloatingPapers());
        }

        if (psycheAsteroidSilhouette != null)
        {
            StartCoroutine(AnimateAsteroid());
        }
    }

    private void HidePhaseText()
    {
        if (phaseCText != null)
        {
            phaseCText.gameObject.SetActive(false);
        }
    }
    
    private void PlayIntroSequence()
    {
        if (backgroundMusic != null)
        {
            backgroundMusic.Play();
        }
        
        if (voiceoverAudio != null)
        {
            StartCoroutine(PlayVoiceoverDelayed());
        }
        
        StartCoroutine(AnimateTextAppearance());
    }
    
    private System.Collections.IEnumerator PlayVoiceoverDelayed()
    {
        yield return new WaitForSeconds(1f);
        voiceoverAudio.Play();
    }
    
    private System.Collections.IEnumerator AnimateTextAppearance()
    {
        // Start with both texts invisible
        if (missionText != null)
        {
            missionText.alpha = 0f;
        }

        bool showPhaseText = phaseCText != null && phaseCText.gameObject.activeInHierarchy;

        if (phaseCText != null)
        {
            phaseCText.alpha = 0f;
        }
        
        // Wait a moment before starting
        yield return new WaitForSeconds(0.5f);
        
        // Animate mission text first
        if (missionText != null)
        {
            float duration = 2f;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                missionText.alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
                yield return null;
            }
        }
        
        // Wait for reading mission text
        yield return new WaitForSeconds(2f);

        // Transition: Fade out mission text while fading in phase text
        if (showPhaseText)
        {
            yield return StartCoroutine(TransitionBetweenTexts());
        }
        
        // Wait before automatically starting the game
        yield return new WaitForSeconds(2f);
        
        // Automatically start the game
        BeginMinigameC();
    }
    
    private System.Collections.IEnumerator TransitionBetweenTexts()
    {
        if (missionText == null)
        {
            yield break;
        }

        if (phaseCText == null || !phaseCText.gameObject.activeInHierarchy)
        {
            yield break;
        }

        float transitionDuration = 1.5f; // Total transition time
        float elapsed = 0f;

        // Store original scales
        Vector3 missionOriginalScale = missionText != null ? missionText.transform.localScale : Vector3.one;
        Vector3 phaseOriginalScale = phaseCText != null ? phaseCText.transform.localScale : Vector3.one;
        
        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / transitionDuration;
            
            // Fade out mission text with scale down effect
            if (missionText != null)
            {
                missionText.alpha = Mathf.Lerp(1f, 0f, progress);
                float scaleMultiplier = Mathf.Lerp(1f, 0.8f, progress); // Slight scale down
                missionText.transform.localScale = missionOriginalScale * scaleMultiplier;
            }
            
            // Fade in phase text with scale up effect (with slight delay for smoother effect)
            if (phaseCText != null)
            {
                float phaseProgress = Mathf.Clamp01((progress - 0.3f) / 0.7f); // Start fading in at 30% progress
                phaseCText.alpha = Mathf.Lerp(0f, 1f, phaseProgress);
                float scaleMultiplier = Mathf.Lerp(0.8f, 1f, phaseProgress); // Scale up from 0.8 to 1
                phaseCText.transform.localScale = phaseOriginalScale * scaleMultiplier;
            }
            
            yield return null;
        }
        
        // Ensure final states
        if (missionText != null)
        {
            missionText.alpha = 0f;
            missionText.transform.localScale = missionOriginalScale * 0.8f;
        }
        
        if (phaseCText != null)
        {
            phaseCText.alpha = 1f;
            phaseCText.transform.localScale = phaseOriginalScale;
        }
    }
    
    // Removed ShowButtons and FadeInButton methods - no buttons needed for automatic transition
    
    private System.Collections.IEnumerator AnimateLogo()
    {
        while (true)
        {
            Vector3 originalPos = psycheLogo.transform.position;
            float time = Time.time;
            Vector3 newPos = originalPos + new Vector3(0, Mathf.Sin(time * 0.5f) * 10f, 0);
            psycheLogo.transform.position = newPos;
            yield return null;
        }
    }
    
    private System.Collections.IEnumerator AnimateFloatingPapers()
    {
        while (true)
        {
            floatingPapers.transform.Rotate(0, 0, 10f * Time.deltaTime);
            yield return null;
        }
    }
    
    private System.Collections.IEnumerator AnimateAsteroid()
    {
        while (true)
        {
            // Glowing effect for asteroid silhouette
            float glow = (Mathf.Sin(Time.time * 2f) + 1f) * 0.5f;
            yield return null;
        }
    }
    
    public void BeginMinigameC()
    {
        // Automatically load GameC_Main after intro sequence
        SceneManager.LoadScene("GameC_Main");
    }
}
