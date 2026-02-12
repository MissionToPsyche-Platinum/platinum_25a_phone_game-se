using UnityEngine;

public class MinigameD_AudioManager : MonoBehaviour
{
    public static MinigameD_AudioManager Instance { get; private set; }

    [SerializeField] private AudioSource buttonSound;

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

    public void playButton()
    {
        buttonSound.Play();
    }
}
