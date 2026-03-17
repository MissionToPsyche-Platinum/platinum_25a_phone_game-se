using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioClipManager : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;

    [SerializeField] private AudioClip correct;
    [SerializeField] private AudioClip incorrect;
    [SerializeField] private AudioClip click;
    [SerializeField] private AudioClip coinPickUp;
    [SerializeField] private AudioClip coinDrop;
    [SerializeField] private AudioClip congrats;

    private float currentVolume = 0.2f;
    private const string PREF_KEY = "MasterVolume";

    private void Start()
    {
        currentVolume = PlayerPrefs.GetFloat(PREF_KEY, currentVolume);
    }

    public void PlayCorrect()
    {
        audioSource.volume = 1f * currentVolume;
        audioSource.PlayOneShot(correct);
    }

    public void PlayIncorrect()
    {
        audioSource.volume = 1f * currentVolume;
        audioSource.PlayOneShot(incorrect);
    }

    public void PlayClick()
    {
        audioSource.volume = 1f * currentVolume;
        audioSource.PlayOneShot(click);
    }

    public void PlayCoinPickUp()
    {
        audioSource.volume = 1f * currentVolume;
        audioSource.PlayOneShot(coinPickUp);
    }

    public void PlayCoinDrop()
    {
        audioSource.volume = 1f * currentVolume;
        audioSource.PlayOneShot(coinDrop);
    }

    public void PlayCongrats()
    {
        audioSource.volume = 0.2f * currentVolume;
        audioSource.PlayOneShot(congrats);
    }
}
