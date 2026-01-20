using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class BoundaryTrigger : MonoBehaviour
{
    private GameManger gm;
    void Awake()
    {
#if UNITY_2023_1_OR_NEWER
        gm = FindFirstObjectByType<GameManger>();
#else
        gm = FindObjectOfType<GameManger>();
#endif
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Spacecraft"))
        {
            gm.MissedOrbit();
        }
    }
}
