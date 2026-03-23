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

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // play background music on levels 1-3
        if (scene.name == "MinigameD-Level1" || scene.name == "MinigameD-Level2" || scene.name == "MinigameD-Level3")
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

    public void buttonClick()
    {
        if (buttonSound != null) buttonSound.Play();
    }

    public void playBoostRing()
    {
        if (boostRingSound != null) boostRingSound.PlayOneShot(boostRingSound.clip);
    }
    public void playPenaltyRing()
    {
        if (penaltyRingSound != null) penaltyRingSound.PlayOneShot(penaltyRingSound.clip);
    }
    public void playJumpRing()
    {
        if (jumpRingSound != null) jumpRingSound.PlayOneShot(jumpRingSound.clip);
    }
    public void playShieldRing()
    {
        if (shieldRingSound != null) shieldRingSound.PlayOneShot(shieldRingSound.clip);
    }
    public void playGameWon()
    {
        if (gameWonSound != null && !gameWonSound.isPlaying) gameWonSound.PlayOneShot(gameWonSound.clip);
    }
    public void playGameLost()
    {
        if (gameLostSound != null && !gameLostSound.isPlaying) gameLostSound.PlayOneShot(gameLostSound.clip);
    }
    public void playBackground()
    {
        if (backgroundMusic != null && !backgroundMusic.isPlaying) backgroundMusic.Play();
    }

    public void stopBackground()
    {
        if (backgroundMusic != null) backgroundMusic.Stop();
    }
}
