using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Temporary input that converts a drag into an initial velocity,
/// and asked TrajectoryPreview to draw a line while dragging.
/// </summary>

public class InputDragPreview : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera mainCam;
    [SerializeField] private Rigidbody2D spacecraftRb;
    [SerializeField] private TrajectoryPreview preview;

    [Header("Tunning")]
    [SerializeField] private float forceToVelocity = 6.0f; //scale drag distance -> velocity 

    private bool dragging = false;
    private Vector2 dragStart;

    private void Awake()
    {
        if (mainCam == null) mainCam = Camera.main;
        if (preview != null && spacecraftRb != null) preview.Bind(spacecraftRb);

    }

    private void Update()
    {

        // Press down? begin dragging if click/touch started near spacecraft
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 world = mainCam.ScreenToWorldPoint(Input.mousePosition);
            // Optional: reuire click within small radius of he ship
            if (Vector2.Distance(world, spacecraftRb.position) < 2.0f)
            {
                dragging = true;
                dragStart = world;
            }
        }


        //while dragging, show trajectory
        if (dragging && Input.GetMouseButtonDown(0))
        {
            Vector2 world = mainCam.ScreenToWorldPoint(Input.mousePosition);
            Vector2 dragVector = dragStart - world; //sling (pull back)
            Vector2 initialVelocity = dragVector * forceToVelocity; //units/sec
            preview.ShowPreview(spacecraftRb.position, initialVelocity);
        }

        // Release -> clear preview
        if (dragging && Input.GetMouseButtonUp(0))
        {
            dragging = false;
            preview.Clear();
        }
    }
    
  
}
