using UnityEngine;

public class OrbitCheck : MonoBehaviour
{
    [Header("Orbit Detection")]
    [SerializeField] private float orbitDuration = 0.5f;  //seconds needed inside orbit radius
    [SerializeField] private string spacecraftTag = "Spacecraft";

    private float timer = 0f;
    private bool insideOrbit = false;
    private GameManger gm;

    void Awake()
    {
#if UNITY_2023_1_OR_NEWER
        gm = FindFirstObjectByType<GameManger>();
#else
        gm = FindObjectOfType<GameManger>();
#endif
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag(spacecraftTag)) return;
        

        timer += Time.deltaTime;

            if (timer >= orbitDuration)
            {
                gm.OrbitSuccess();
                enabled = false; // stop checking more
            }
        
    }


    private void OnTriggerExit2D(Collider2D other)
    {
        if(other.CompareTag(spacecraftTag))
        {
            timer = 0f; // reset timer
        }

    }
}
