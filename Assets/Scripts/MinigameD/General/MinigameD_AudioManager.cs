using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public class MinigameD_AudioManager : MonoBehaviour
{
    public static MinigameD_AudioManager Instance { get; private set; }

    [SerializeField] private AudioSource buttonSound;
    [SerializeField] private AudioSource boostSound;
    [SerializeField] private AudioSource penaltySound;

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

    public void boostRing()
    {
        if (boostSound != null) boostSound.PlayOneShot(boostSound.clip);
    }
    public void penaltyRing()
    {
        if (penaltySound != null) penaltySound.PlayOneShot(penaltySound.clip);
    }
}
