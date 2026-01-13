using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
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


   private void Awake()
   {
        // singleton pattern (one global AudioManager)
       if (Instance != null && Instance != this)
       {
           Destroy(gameObject);
           return;
       }
         Instance = this;
         DontDestroyOnLoad(gameObject); //persist across scenes
   }

   // --------------------------------
   // Gameplay Sound Methods
    public void PlayStretch()
    {
        if(stretchSound != null)
         stretchSound.Play(); 
    }

    public void StopStretch()
    {
        if(stretchSound != null)
         stretchSound.Stop(); 
    }

    public void PlayThrust()
    {
        if(thrustSound != null)
         thrustSound.Play(); 
    }

    public void StopThrust()
    {
        if(thrustSound != null)
         thrustSound.Stop(); 
    }

    public void PlaySuccess()
    {
        if(successSound != null)
         successSound.Play(); 
    }

    public void PlayFail()
    {
        if(failSound != null)
         failSound.Play(); 
    }

    public void PlayImpact()
    {
        if(impactSound != null)
         impactSound.Play(); 
    }

    //-------------------------
    // Background Sound
    // ------------------------

    public void PlayBackground()
    {
        if(backgroundSpaceSound != null && !backgroundSpaceSound.isPlaying)
         backgroundSpaceSound.Play();
    }

    public void StopBackground()
    {
        if(backgroundSpaceSound != null)
         backgroundSpaceSound.Stop();
    }

    // ------------------------
    // UI Sound Methods
    // ------------------------

    public void PlayButtonClick()
    {
        if(butttonClickSound != null)
         butttonClickSound.Play(); 
    }

    // ------------------------
    // Stop all sounds
    // ------------------------
    public void StopAllGameplaySounds()
    {
        StopStretch();
        StopThrust();
        if(impactSound != null)
            impactSound.Stop();
    }   

}
