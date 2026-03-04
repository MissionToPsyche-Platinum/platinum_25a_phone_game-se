using System;
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
        // play background music on levels 1-3 & game over scenes
        if (scene.name == "MinigameD-Level1" || scene.name == "MinigameD-Level2" || scene.name == "MinigameD-Level3" 
            || scene.name == "MinigameD-Game-Lost" || scene.name == "MinigameD-Game-Won")
        {
            playBackground();
        } else
        {
            stopBackground();
        }

        // game lost and game won audio
        if (scene.name == "MinigameD-Game-Lost") {
            MinigameD_AudioManager.Instance.playGameLost();
        }
        if (scene.name == "MinigameD-Game-Won")
        {
            MinigameD_AudioManager.Instance.playGameWon();
        }
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
