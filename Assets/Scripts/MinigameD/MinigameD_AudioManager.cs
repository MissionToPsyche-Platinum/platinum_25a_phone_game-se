using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MinigameD_AudioManager : MonoBehaviour
{
    public static MinigameD_AudioManager Instance { get; private set; }

    [SerializeField] private AudioSource buttonSound;

    [SerializeField] private AudioSource boostRingSound;
    [SerializeField] private AudioSource penaltyRingSound;
    [SerializeField] private AudioSource jumpRingSound;
    [SerializeField] private AudioSource shieldRingSound;

    [SerializeField] private AudioSource gameLostSound;
    [SerializeField] private AudioSource gameWonSound;

    [SerializeField] private AudioSource backgroundMusic;

    private float currentVolume = 0.2f;
    private const string PREF_KEY = "MasterVolume";

    private void Update()
    {
        currentVolume = PlayerPrefs.GetFloat(PREF_KEY, currentVolume);
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // play background music on levels 1-3 & educational content
        if (scene.name == "MinigameD-Level1" || scene.name == "MinigameD-Level2" || scene.name == "MinigameD-Level3" 
            || scene.name == "MinigameD-Writing")
        {
            playBackground();
        }
        else if (scene.name == "MinigameD-Game-Lost") 
        {
            StartCoroutine(fadeOut(backgroundMusic, 1));
        } 
        else if (scene.name == "MinigameD-Game-Won")
        {
            StartCoroutine(fadeOut(backgroundMusic, 3));
        }
        else
        {
            stopBackground();
        }

        // game lost and game won audio
        if (scene.name == "MinigameD-Game-Lost") {
            MinigameD_AudioManager.Instance.playGameLost();
        }
        if (scene.name == "MinigameD-Game-Won")
        {
            StartCoroutine(delayCall(3));
        }
    }

    private IEnumerator delayCall(float duration)
    {
        yield return new WaitForSeconds(duration); 
        MinigameD_AudioManager.Instance.playGameWon();
    }

    private IEnumerator fadeOut(AudioSource audio, float duration)
    {
        float startVolume = audio.volume;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            audio.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }

        audio.volume = 0f;
        audio.Stop();
        audio.volume = startVolume;
    }

    public static void playButtonSound()
    {
        if (Instance != null)
        {
            Instance.buttonClick();
        }
    }

    public void buttonClick()
    {
        buttonSound.volume = 1f * currentVolume;
        if (buttonSound != null) buttonSound.Play();
    }

    public void playBoostRing()
    {
        boostRingSound.volume = 1f * currentVolume;
        if (boostRingSound != null) boostRingSound.PlayOneShot(boostRingSound.clip);
    }
    public void playPenaltyRing()
    {
        penaltyRingSound.volume = 1f * currentVolume;
        if (penaltyRingSound != null) penaltyRingSound.PlayOneShot(penaltyRingSound.clip);
    }
    public void playJumpRing()
    {
        jumpRingSound.volume = 1f * currentVolume;
        if (jumpRingSound != null) jumpRingSound.PlayOneShot(jumpRingSound.clip);
    }
    public void playShieldRing()
    {
        shieldRingSound.volume = 1f * currentVolume;
        if (shieldRingSound != null) shieldRingSound.PlayOneShot(shieldRingSound.clip);
    }
    public void playGameWon()
    {
        gameWonSound.volume = 1f * currentVolume;
        if (gameWonSound != null && !gameWonSound.isPlaying) gameWonSound.PlayOneShot(gameWonSound.clip);
    }
    public void playGameLost()
    {
        gameLostSound.volume = 1f * currentVolume;
        if (gameLostSound != null && !gameLostSound.isPlaying) gameLostSound.PlayOneShot(gameLostSound.clip);
    }
    public void playBackground()
    {
        backgroundMusic.volume = 1f * currentVolume;
        if (backgroundMusic != null && !backgroundMusic.isPlaying) backgroundMusic.Play();
    }

    public void stopBackground()
    {
        if (backgroundMusic != null) backgroundMusic.Stop();
    }
}
