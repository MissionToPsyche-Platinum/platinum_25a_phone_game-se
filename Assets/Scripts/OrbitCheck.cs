using UnityEngine;

public class OrbitCheck : MonoBehaviour
{
    [Header("Orbit Detection")]
    [SerializeField] private float orbitDuration = 0.5f;  //seconds needed inside orbit radius
    [SerializeField] private string spacecraftTag = "Spacecraft";

    private float timer = 0f;
    private GameManger gm;
    private InputDragLaunch dragLaunch;

    void Awake()
    {
#if UNITY_2023_1_OR_NEWER
        gm = FindFirstObjectByType<GameManger>();
        dragLaunch = FindFirstObjectByType<InputDragLaunch>();
#else
        gm = FindObjectOfType<GameManger>();
        dragLaunch = FindObjectOfType<InputDragLaunch>();
#endif
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag(spacecraftTag)) return;
        

        timer += Time.deltaTime;

            if (timer >= orbitDuration)
        {
            // stop drag sounds if somehow still playing
            AudioManager.Instance.StopStretch();

            // stop thrust sound whn orbit is achieved
            // (if thrust was still fading)
            AudioManager.Instance.StopThrust();

            // Play impact/achieve sound
            AudioManager.Instance.PlayImpact();

            // Measure how close spacecraft is to center (for accuracy scoring)
            float orbitError = Vector2.Distance(other.transform.position, transform.position);

            // Retrieve last launch power from InputDragLaunch
            float launchPower = dragLaunch != null ? dragLaunch.GetLastLaunchPower() : 0f;
            
            // Pass both to GameManager for score calculation
            gm.OrbitSuccess(orbitError, launchPower);
            enabled = false; // stop checking more
        }
        
    }


    private void OnTriggerExit2D(Collider2D other)
    {
        if(other.CompareTag(spacecraftTag)) 
        {
            // reset timer if spacecraft exits orbit area
            timer = 0f;
        }
    }
}
