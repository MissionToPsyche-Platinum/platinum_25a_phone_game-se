using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Audio manager for MinigameC. Auto-created on scene load.
/// Loads all clips from Resources/Sounds/MiniGameC/ and exposes static play methods.
/// </summary>
public class MinigameCAudioManager : MonoBehaviour
{
    private const string TargetSceneName = "MinigameC";
    private const string SoundPath = "Sounds/MiniGameC/";

    public static MinigameCAudioManager Instance { get; private set; }

    private AudioSource _musicSource;
    private AudioSource _sfxSource;
    private AudioSource _warningLoopSource;

    private AudioClip _backgroundMusic;
    private AudioClip _dialogueBoxOpenClose;
    private AudioClip _hintOpenClose;
    private AudioClip _inventoryClose;
    private AudioClip _inventoryOpen;
    private AudioClip _itemDrop;
    private AudioClip _itemFull;
    private AudioClip _itemPickup;
    private AudioClip _taskStageComplete;
    private AudioClip _timerAlarm;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Boot()
    {
        SceneManager.sceneLoaded += (scene, _) =>
        {
            if (scene.name != TargetSceneName) return;
            if (FindFirstObjectByType<MinigameCAudioManager>() != null) return;
            new GameObject("MinigameCAudioManager").AddComponent<MinigameCAudioManager>();
        };
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        if (SceneManager.GetActiveScene().name != TargetSceneName) { Destroy(gameObject); return; }

        _musicSource = gameObject.AddComponent<AudioSource>();
        _musicSource.loop = true;
        _musicSource.volume = 0.35f;

        _sfxSource = gameObject.AddComponent<AudioSource>();
        _sfxSource.loop = false;
        _sfxSource.volume = 0.8f;

        _warningLoopSource = gameObject.AddComponent<AudioSource>();
        _warningLoopSource.loop = true;
        _warningLoopSource.playOnAwake = false;
        _warningLoopSource.volume = 0.8f;

        LoadClips();
        PlayBackgroundMusic();

        var assembly = PhaseCAssemblyController.Instance;
        if (assembly != null)
            assembly.PhaseCComplete += StopBackgroundMusic;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        var assembly = PhaseCAssemblyController.Instance;
        if (assembly != null)
            assembly.PhaseCComplete -= StopBackgroundMusic;
    }

    private void LoadClips()
    {
        _backgroundMusic      = Resources.Load<AudioClip>(SoundPath + "background_music");
        _dialogueBoxOpenClose = Resources.Load<AudioClip>(SoundPath + "dialogue_box_open_close");
        _hintOpenClose        = Resources.Load<AudioClip>(SoundPath + "hint_open_close");
        _inventoryClose       = Resources.Load<AudioClip>(SoundPath + "inventory_close");
        _inventoryOpen        = Resources.Load<AudioClip>(SoundPath + "inventory_open");
        _itemDrop             = Resources.Load<AudioClip>(SoundPath + "item_drop");
        _itemFull             = Resources.Load<AudioClip>(SoundPath + "item_full");
        _itemPickup           = Resources.Load<AudioClip>(SoundPath + "item_pickup");
        _taskStageComplete    = Resources.Load<AudioClip>(SoundPath + "task_stage_complete");
        _timerAlarm           = Resources.Load<AudioClip>(SoundPath + "timer_alarm");
    }

    // ── Music ────────────────────────────────────────────────────────────────

    public static void PlayBackgroundMusic()
    {
        if (Instance == null || Instance._musicSource == null) return;
        if (Instance._backgroundMusic == null) return;
        Instance._musicSource.clip = Instance._backgroundMusic;
        Instance._musicSource.Play();
    }

    public static void StopBackgroundMusic()
    {
        if (Instance == null || Instance._musicSource == null) return;
        Instance._musicSource.Stop();
    }

    // ── SFX ──────────────────────────────────────────────────────────────────

    public static void PlayItemPickup()       => PlaySfx(Instance?._itemPickup);
    public static void PlayItemFull()         => PlaySfx(Instance?._itemFull);
    public static void PlayItemDrop()         => PlaySfx(Instance?._itemDrop);
    public static void PlayInventoryOpen()    => PlaySfx(Instance?._inventoryOpen);
    public static void PlayInventoryClose()   => PlaySfx(Instance?._inventoryClose);
    public static void PlayDialogueOpen()     => PlaySfx(Instance?._dialogueBoxOpenClose);
    public static void PlayDialogueClose()    => PlaySfx(Instance?._dialogueBoxOpenClose);
    public static void PlayStepComplete()     => PlaySfx(Instance?._taskStageComplete);
    public static void PlayTimerAlarm()       => PlaySfx(Instance?._timerAlarm);
    public static void PlayHintToggle()       => PlaySfx(Instance?._hintOpenClose);
    public static void StartWarningAlarmLoop()
    {
        if (Instance == null || Instance._warningLoopSource == null || Instance._timerAlarm == null) return;
        if (Instance._warningLoopSource.isPlaying) return;
        Instance._warningLoopSource.clip = Instance._timerAlarm;
        Instance._warningLoopSource.Play();
    }

    public static void StopWarningAlarmLoop()
    {
        if (Instance == null || Instance._warningLoopSource == null) return;
        if (!Instance._warningLoopSource.isPlaying) return;
        Instance._warningLoopSource.Stop();
    }

    private static void PlaySfx(AudioClip clip)
    {
        if (Instance == null || Instance._sfxSource == null || clip == null) return;
        Instance._sfxSource.PlayOneShot(clip);
    }
}
