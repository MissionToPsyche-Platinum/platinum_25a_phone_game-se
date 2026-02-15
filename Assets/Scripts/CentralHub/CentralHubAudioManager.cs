using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

public class CentralHubAudioManager : MonoBehaviour
{
    [Header("Audio Clips")]
    [SerializeField] private AudioClip hubBackgroundLoop;
    [SerializeField] private AudioClip clickSfx;

    [Header("Volumes")]
    [Range(0f, 1f)][SerializeField] private float bgVolume = 0.2f;
    [Range(0f, 1f)][SerializeField] private float clickVolume = 0.75f;

    [Header("Ducking")]
    [Range(0f, 1f)][SerializeField] private float duckedByMultiplier = 0.4f;
    [SerializeField] private float fadeDownSeconds = 0.15f;
    [SerializeField] private float holdSeconds = 0.20f;
    [SerializeField] private float fadeUpSeconds = 0.25f;

    private AudioSource bySource;
    private AudioSource sfxSource;

    private Coroutine duckRountine;

    private void Awake()
    {
        bySource = gameObject.AddComponent<AudioSource>();
        bySource.clip = hubBackgroundLoop;
        bySource.loop = true;
        bySource.playOnAwake = false;
        bySource.spatialBlend = 0f;
        bySource.volume = bgVolume;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;
        sfxSource.spatialBlend = 0f;
        sfxSource.volume = 1f;

    }

    private void Start()
    {
        if(hubBackgroundLoop != null)
        {
            bySource.Play();
        }
        else
        {
            Debug.LogWarning("No background loop assigned to CentralHubAudioManager.");
        }
    }


    public void PlayClick()
    {
        if (clickSfx == null) return;
        if (duckRountine != null)
        {
            StopCoroutine(duckRountine);
        }
        duckRountine = StartCoroutine(DuckBackground());

        sfxSource.PlayOneShot(clickSfx, clickVolume);
    }

    public void StopBackground()
    {
        if (bySource.isPlaying)
        {
            bySource.Stop();
        }
    }

    public void SetBackgroundVolume(float volume)
    {
        bgVolume = Mathf.Clamp01(volume);
        bySource.volume = bgVolume;
    }

    private IEnumerator DuckBackground()
    {
        float normal = bgVolume;
        float ducked = normal * duckedByMultiplier;

        yield return FadeVolume(bySource, bySource.volume, ducked, fadeDownSeconds);

        yield return new WaitForSeconds(holdSeconds);

        yield return FadeVolume(bySource, bySource.volume, normal, fadeUpSeconds);
    }

    private IEnumerator FadeVolume(AudioSource source, float from, float to, float seconds)
    {
        if (source == null) yield break;

        if (seconds <= 0f)
        {
            source.volume = to;
            yield break;
        }

        float t = 0f;
        while (t < seconds)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / seconds);
            source.volume = Mathf.Lerp(from, to, p);
            yield return null;
        }
        source.volume = to;
    }
}