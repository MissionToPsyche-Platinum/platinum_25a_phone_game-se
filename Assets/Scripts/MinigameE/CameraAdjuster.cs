
using UnityEngine;

public class CameraAdjuster : MonoBehaviour
{
    public float baseSize = 5f;
    public float referenceAspect = 16f / 9f;

    void Start()
    {
        float currentAspect = (float)Screen.width / Screen.height;
        Camera cam = GetComponent<Camera>();

        if (currentAspect < referenceAspect)
        {
           cam.orthographicSize = baseSize * (referenceAspect / currentAspect);
        }
        else
        {
            cam.orthographicSize = baseSize;
        }
    }
}
