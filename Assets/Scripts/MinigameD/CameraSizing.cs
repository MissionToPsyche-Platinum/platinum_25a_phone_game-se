using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSizing : MonoBehaviour
{
    public Camera mainCamera;
    public SpriteRenderer background;

    public float targetAspectRatio = 9f / 16f; // phone size

    void Start()
    {
        AdjustCameraToDeviceAndAspect();
    }

    void AdjustCameraToDeviceAndAspect()
    {
        if (mainCamera == null || background == null)
        {
            Debug.LogError("Assign camera and background");
            return;
        }

        Vector3 backgroundSize = background.bounds.size;
        float backgroundHeight = backgroundSize.y;
        float backgroundWidth = backgroundSize.x;

        mainCamera.orthographicSize = backgroundHeight / 2f;
        float desiredWidth = mainCamera.orthographicSize * 2f * targetAspectRatio;

        float aspectRatio = (float)Screen.width / Screen.height;
        float cameraWidthAtCurrentSize = 2f * mainCamera.orthographicSize * aspectRatio;

        if (backgroundWidth < desiredWidth)
        {
            mainCamera.orthographicSize = backgroundWidth / (2f * aspectRatio);
        }
    }
}
