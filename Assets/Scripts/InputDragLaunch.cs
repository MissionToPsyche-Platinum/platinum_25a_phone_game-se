using UnityEngine;
using UnityEngine.UI;

public class InputDragLaunch : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera mainCam;
    [SerializeField] private Rigidbody2D spacecraftRb;
    [SerializeField] private TrajectoryPreview preview;
    [SerializeField] private Image powerBarFill;

    [Header("Tuning")]
    [Tooltip("Maximum distance (world units) we consider for full power.")]
    [SerializeField] private float maxDragDistance = 4f;
    [Tooltip("Scale factor to convert drag distance to launch speed.")]
    [SerializeField] private float dragToSpeed = 6f;
    [Tooltip("How close you must click to the ship to start a drag.")]
    [SerializeField] private float clickRadius = 1.5f;


    private bool dragging;
    private Vector2 dragStart;


    void Awake()
    {
        if (!mainCam) mainCam = Camera.main;
        // freeze rotation for clean slingshot feel 
        if (spacecraftRb) spacecraftRb.freezeRotation = true;
    }


    void Update()
    {
        // Mouse (also works on dexktop). Touch support is below.
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 world = mainCam.ScreenToWorldPoint(Input.mousePosition);
            if (Vector2.Distance(world, spacecraftRb.position) <= clickRadius)
            {
                dragging = true;
                dragStart = world;
            }
        }


        if (dragging && Input.GetMouseButton(0))
        {
            Vector2 world = mainCam.ScreenToWorldPoint(Input.mousePosition);
            Vector2 dragVec = dragStart - world;  // pull-back vector
            float dist = Mathf.Clamp(dragVec.magnitude, 0f, maxDragDistance);

            // preview velocity (direction of drag, scaled)
            Vector2 initialVelocity = dragVec.normalized * (dist * dragToSpeed);
            preview.ShowPreview(spacecraftRb.position, initialVelocity);

            // Ui power bar
            if (powerBarFill) powerBarFill.fillAmount = dist / maxDragDistance;
        }

        if (dragging && Input.GetMouseButtonUp(0))
        {
            LaunchFromPointer(Input.mousePosition);
        }

        // simple touch support
        if (Input.touchCount > 0)
        {
            var t = Input.GetTouch(0);
            Vector2 world = mainCam.ScreenToWorldPoint(t.position);

            if (t.phase == TouchPhase.Began)
            {
                if (Vector2.Distance(world, spacecraftRb.position) <= clickRadius)
                {
                    dragging = true;
                    dragStart = world;
                }
            }

            else if (dragging && (t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary))
            {
                Vector2 dragVec = dragStart - world;  // pull-back vector
                float dist = Mathf.Clamp(dragVec.magnitude, 0f, maxDragDistance);

                // preview velocity (direction of drag, scaled)
                Vector2 initialVelocity = dragVec.normalized * (dist * dragToSpeed);
                preview.ShowPreview(spacecraftRb.position, initialVelocity);

                // Ui power bar
                if (powerBarFill) powerBarFill.fillAmount = dist / maxDragDistance;
            }

            else if (dragging && (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled))
            {
                LaunchFromPointer(t.position);
            }
        }

    }
    
    private void LaunchFromPointer(Vector2 pointerPos)
    {
        dragging = false;

        Vector2 world = mainCam.ScreenToWorldPoint(pointerPos);
        Vector2 dragVec = dragStart - world;  // pull-back vector
        float dist = Mathf.Clamp(dragVec.magnitude, 0f, maxDragDistance);

        // final launch velocity
        Vector2 launchVelocity = dragVec.normalized * (dist * dragToSpeed);

        // reset power bar
        preview.Clear();
        if (powerBarFill) powerBarFill.fillAmount = 0f;

        // apply velocity to rigidbody
        spacecraftRb.velocity = launchVelocity;
    }

}
