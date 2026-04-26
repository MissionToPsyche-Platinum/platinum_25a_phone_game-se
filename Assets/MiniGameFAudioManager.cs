using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MinigameFAudioManager : MonoBehaviour
{
    public static MinigameFAudioManager Instance { get; private set; }
    

    [SerializeField] private AudioSource succ;
    [SerializeField] private AudioSource fail;    

    [SerializeField] private AudioSource backgroundMusic;

    private float currentVolume = 0.2f;
    private const string PREF_KEY = "MasterVolume";

    private void Update()
    {
        currentVolume = PlayerPrefs.GetFloat(PREF_KEY, currentVolume);
        Debug.Log(currentVolume);
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

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)    {
        
        if (scene.name == "MiniGameF")
        {
            playBackground();
        }        
        else
        {
            stopBackground();
        }
        
    }

    

    public void playSucc()
    {
        succ.volume = 1f * currentVolume;
        if (succ != null) succ.PlayOneShot(succ.clip);
    }
    public void playFail()
    {
        fail.volume = 1f * currentVolume;
        if (fail != null) fail.PlayOneShot(fail.clip);
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
