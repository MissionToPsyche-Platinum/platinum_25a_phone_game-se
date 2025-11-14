using UnityEngine;

public class BoundaryTrigger : MonoBehaviour
{
    private GameManger gm;
    void Awake()
    {
        gm = FindObjectOfType<GameManger>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Spacecraft"))
        {
            gm.MissedOrbit();
        }
    }
}
