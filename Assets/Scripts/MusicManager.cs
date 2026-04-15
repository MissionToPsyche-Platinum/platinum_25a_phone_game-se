using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [SerializeField] private AudioSource musicSource;

    private float currentVolume = 0.2f;
    private const string PREF_MUSIC_KEY = "MusicVolume";

    void Start()
    {
        currentVolume = PlayerPrefs.GetFloat(PREF_MUSIC_KEY, currentVolume);
    }

    void Update()
    {
        musicSource.volume = currentVolume;
    }

    public void UpdateVolume()
    {
        currentVolume = PlayerPrefs.GetFloat(PREF_MUSIC_KEY, currentVolume);
    }
    public void StopMusic()
    {
        musicSource.Stop();
    }
}
