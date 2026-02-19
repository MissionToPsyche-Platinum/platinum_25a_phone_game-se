using System;
using System.Runtime.CompilerServices;
using UnityEngine;

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
        if (gameWonSound != null) gameWonSound.PlayOneShot(gameWonSound.clip);
    }
    public void playGameLost()
    {
        if (gameLostSound != null) gameLostSound.PlayOneShot(gameLostSound.clip);
    }
}
