using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
   public static AudioManager Instance {get; private set;}  // Singleton instance;

   [Header ("Gameplay Sounds")]
   [SerializeField] private AudioSource stretchSound;       //Drag stretching sound
   [SerializeField] private AudioSource thrustSound;       //Drag thrust sound
   [SerializeField] private AudioSource successSound;      //Orbit success
   [SerializeField] private AudioSource failSound;         //Orbit fail
   [SerializeField] private AudioSource impactSound;   //impact sound
   [SerializeField] private AudioSource backgroundSpaceSound; //Background space sound

    [Header ("UI Sounds")]
   [SerializeField] private AudioSource butttonClickSound;   //Button click sound

    private const string PREF_KEY = "MasterVolume"; //Key for PlayerPrefs

    [Header("Default Volume")]
    [Range(0f, 1f)]
    [SerializeField] private float defaultMasterVolume = 0.7f; //Default volume

    private float masterVolume = 1f; //Current master volume

    private readonly Dictionary<AudioSource, float> baseVolumes = new Dictionary<AudioSource, float>(); //Store original volumes

   private void Awake()
   {
        // singleton pattern (one global AudioManager)
       if (Instance != null && Instance != this) { Destroy(gameObject); return; }
         Instance = this;

         CacheBaseVolumes(); //Store original volumes

        masterVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(PREF_KEY, defaultMasterVolume)); //Load saved volume or use default
        ApplyMasterVolumeToAllSouces();
   }

   private void OnDestroy()
   {
       if (Instance == this) { Instance = null; }
   }

   private void CacheBaseVolumes()
   {
       CacheVolume(stretchSound);
         CacheVolume(thrustSound);
            CacheVolume(successSound);
            CacheVolume(failSound);
            CacheVolume(impactSound);
            CacheVolume(backgroundSpaceSound);
            CacheVolume(butttonClickSound);
   }

   private void CacheVolume(AudioSource src)
    {
        if (src == null) return;
        if (!baseVolumes.ContainsKey(src))
        {
           baseVolumes.Add(src, src.volume);
        }
    }

    public void SetMasterVolume(float v)
    {
        masterVolume = Mathf.Clamp01(v); //Ensure volume is between 0 and 1
        PlayerPrefs.SetFloat(PREF_KEY, masterVolume);
        PlayerPrefs.Save();
        ApplyMasterVolumeToAllSouces();
    }
    
    public float GetMasterVolume()
    {
        return masterVolume;
    }

    private void ApplyMasterVolumeToAllSouces()
    {
        foreach (var kvp in baseVolumes)
        {
            AudioSource src = kvp.Key;
            float baseVol = kvp.Value;

            if (src != null)
            {
                src.volume = baseVol * masterVolume; //Apply master volume to each source
            }
        }
    }

   // --------------------------------
   // Gameplay Sound Methods
    public void PlayStretch(){ if(stretchSound != null) stretchSound.Play(); }

    public void StopStretch(){ if(stretchSound != null) stretchSound.Stop(); }

    public void PlayThrust(){ if(thrustSound != null) thrustSound.Play(); }

    public void StopThrust() { if(thrustSound != null) thrustSound.Stop(); }

    public void PlaySuccess() { if(successSound != null) successSound.Play(); }

    public void PlayFail() { if(failSound != null) failSound.Play(); }

    public void PlayImpact() { if(impactSound != null) impactSound.Play(); }

    //-------------------------
    // Background Sound
    // ------------------------

    public void PlayBackground()
    { if(backgroundSpaceSound != null && !backgroundSpaceSound.isPlaying) backgroundSpaceSound.Play();}

    public void StopBackground(){ if(backgroundSpaceSound != null) backgroundSpaceSound.Stop();}

    // ------------------------
    // UI Sound Methods
    // ------------------------

    public void PlayButtonClick() { if(butttonClickSound != null) butttonClickSound.Play(); }

    // ------------------------
    // Stop all sounds
    // ------------------------
    public void StopAllGameplaySounds()
    {
        StopStretch();
        StopThrust();
        if(impactSound != null)
            impactSound.Stop();
        if(successSound != null)
            successSound.Stop();
        if(failSound != null)            
            failSound.Stop();
    }   

}
