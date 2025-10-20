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

    private Vector3 logoRestPosition;
    private Vector3 floatingPapersRestPosition;
    private Quaternion floatingPapersRestRotation;
    private Vector3 asteroidRestScale;
    
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
        StyleTextElements();
    }
    
    private void SetupMobileTextSizes()
    {
        // Mobile-optimized text sizes
        if (missionText != null)
        {
            missionText.fontSize = 22f; // Mobile-friendly size
            missionText.lineSpacing = 1.2f;
        }

        if (phaseCText != null)
        {
            phaseCText.fontSize = 32f; // Mobile-friendly size
        }
    }

    private void StyleTextElements()
    {
        if (missionText != null)
        {
            missionText.fontStyle = FontStyles.UpperCase | FontStyles.Bold;
            missionText.outlineWidth = 0.12f;
            missionText.outlineColor = new Color(0f, 0f, 0f, 0.45f);
            missionText.enableVertexGradient = true;
            missionText.characterSpacing = 4f;
            missionText.colorGradient = new VertexGradient(
                new Color(0.86f, 0.95f, 1f),
                new Color(0.72f, 0.88f, 1f),
                new Color(0.56f, 0.74f, 0.98f),
                new Color(0.72f, 0.88f, 1f));
            ApplySoftShadow(missionText, new Vector2(2.2f, -2.2f), 0.55f);
        }

        if (phaseCText != null)
        {
            phaseCText.fontStyle = FontStyles.SmallCaps | FontStyles.Bold;
            phaseCText.outlineWidth = 0.08f;
            phaseCText.outlineColor = new Color(0f, 0f, 0f, 0.4f);
            phaseCText.enableVertexGradient = true;
            phaseCText.characterSpacing = 6f;
            phaseCText.colorGradient = new VertexGradient(
                new Color(1f, 0.78f, 0.36f),
                new Color(0.98f, 0.66f, 0.24f),
                new Color(0.9f, 0.54f, 0.18f),
                new Color(0.98f, 0.66f, 0.24f));
            ApplySoftShadow(phaseCText, new Vector2(1.8f, -1.8f), 0.45f);
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

    private void ApplySoftShadow(TextMeshProUGUI text, Vector2 offset, float alpha)
    {
        Shadow shadow = text.GetComponent<Shadow>();
        if (shadow == null)
        {
            shadow = text.gameObject.AddComponent<Shadow>();
        }

        shadow.effectColor = new Color(0f, 0f, 0f, alpha);
        shadow.effectDistance = offset;
        shadow.useGraphicAlpha = true;
    }
    
    // Removed SetupButtons method - no buttons needed for automatic transition
    
    private void SetupVisuals()
    {
        if (psycheLogo != null)
        {
            logoRestPosition = psycheLogo.transform.position;
            StartCoroutine(AnimateLogo());
        }

        if (floatingPapers != null)
        {
            floatingPapersRestPosition = floatingPapers.transform.position;
            floatingPapersRestRotation = floatingPapers.transform.rotation;
            StartCoroutine(AnimateFloatingPapers());
        }

        if (psycheAsteroidSilhouette != null)
        {
            asteroidRestScale = psycheAsteroidSilhouette.transform.localScale;
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
        float amplitude = 10f;
        float speed = 0.75f;
        while (true)
        {
            float offset = Mathf.Sin(Time.time * speed) * amplitude;
            psycheLogo.transform.position = logoRestPosition + new Vector3(0f, offset, 0f);
            yield return null;
        }
    }

    private System.Collections.IEnumerator AnimateFloatingPapers()
    {
        float bobAmplitude = 4f;
        float bobSpeed = 1.4f;
        float swayAmplitude = 12f;
        float swaySpeed = 0.7f;
        while (true)
        {
            float bobOffset = Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;
            floatingPapers.transform.position = floatingPapersRestPosition + new Vector3(0f, bobOffset, 0f);

            float swayAngle = Mathf.Sin(Time.time * swaySpeed) * swayAmplitude;
            floatingPapers.transform.rotation = floatingPapersRestRotation * Quaternion.Euler(0f, 0f, swayAngle);
            yield return null;
        }
    }

    private System.Collections.IEnumerator AnimateAsteroid()
    {
        float pulseSpeed = 1.8f;
        float pulseAmplitude = 0.05f;
        while (true)
        {
            // Glowing effect for asteroid silhouette
            float glow = (Mathf.Sin(Time.time * 2f) + 1f) * 0.5f;
            if (psycheAsteroidSilhouette != null)
            {
                SpriteRenderer spriteRenderer = psycheAsteroidSilhouette.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    Color spriteColor = spriteRenderer.color;
                    spriteColor.a = Mathf.Lerp(0.45f, 0.75f, glow);
                    spriteRenderer.color = spriteColor;
                }
                else
                {
                    Image image = psycheAsteroidSilhouette.GetComponent<Image>();
                    if (image != null)
                    {
                        Color imageColor = image.color;
                        imageColor.a = Mathf.Lerp(0.45f, 0.75f, glow);
                        image.color = imageColor;
                    }
                }

                float scaleOffset = 1f + (Mathf.Sin(Time.time * pulseSpeed) * pulseAmplitude);
                psycheAsteroidSilhouette.transform.localScale = asteroidRestScale * scaleOffset;
            }
            yield return null;
        }
    }
    
    public void BeginMinigameC()
    {
        // Automatically load GameC_Main after intro sequence
        SceneManager.LoadScene("GameC_Main");
    }
}
