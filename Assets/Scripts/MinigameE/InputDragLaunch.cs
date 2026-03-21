using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InputDragLaunch : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera mainCam;
    [SerializeField] private Rigidbody2D spacecraftRb;
    [SerializeField] private TrajectoryPreview preview;
    [SerializeField] private GameObject powerBarRoot;
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

    private float lastLaunchPower = 0f;


    // --------------------------------
    // variables added for measuring angle and speed
    // --------------------------------

    private Vector2 currentAimVelocity = Vector2.zero;
    private float currentAimSpeed = 0f;
    private float currentAimAngleDeg = 0f;
    private bool hasLastShot = false;
    public float CurrentAimSpeed => currentAimSpeed;
    public float CurrentAimAngleDeg => currentAimAngleDeg;
    public bool IsDragging => dragging;
    public bool HasLastShot => hasLastShot;

    void Awake()
    {
        if (!mainCam) mainCam = Camera.main;
        // freeze rotation for clean slingshot feel 
        if (spacecraftRb) spacecraftRb.freezeRotation = true;

        // Hide UI at start
        if (powerBarRoot) powerBarRoot.SetActive(false);
        if(powerBarFill) powerBarFill.fillAmount = 0f;
    }

    void Update()
    {
        HandleMouseInput();
        HandleTouchInput();
    }


    private void HandleMouseInput()
    {

        // Block game input if clicking or dragging on UI
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        // Mouse (also works on dexktop). Touch support is below.
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 world = mainCam.ScreenToWorldPoint(Input.mousePosition);
            if (Vector2.Distance(world, spacecraftRb.position) <= clickRadius)
            {
                dragging = true;
                hasLastShot = false;
                dragStart = world;

                // Show power UI when dragging starts
                if (powerBarRoot) powerBarRoot.SetActive(true);

                // Play Stretch Sound
                AudioManager.Instance.PlayStretch();
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

            currentAimVelocity = initialVelocity;
            currentAimSpeed = initialVelocity.magnitude;
            currentAimAngleDeg = Mathf.Atan2(initialVelocity.y, initialVelocity.x) * Mathf.Rad2Deg;

            // Ui power bar
            if (powerBarFill) powerBarFill.fillAmount = dist / maxDragDistance;
        }

        if (dragging && Input.GetMouseButtonUp(0))
        {
            LaunchFromPointer(Input.mousePosition);
        }
    }

    //------------------------------
    // Touch Input
    //------------------------------
    private void HandleTouchInput()
    {
        if (Input.touchCount > 0)
        {
             if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId)) return;
             
            var t = Input.GetTouch(0);
            Vector2 world = mainCam.ScreenToWorldPoint(t.position);

            if (t.phase ==TouchPhase.Began)
            {
                if (Vector2.Distance(world, spacecraftRb.position) <= clickRadius)
                {
                    dragging = true;
                    hasLastShot = false;
                    dragStart = world;

                    // show power UI when dragging starts
                    if (powerBarRoot) powerBarRoot.SetActive(true);

                    // Play Stretch Sound
                    AudioManager.Instance.PlayStretch();
                }
            }

            else if (dragging && (t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary))
            {
                Vector2 dragVec = dragStart - world;  // pull-back vector
                float dist = Mathf.Clamp(dragVec.magnitude, 0f, maxDragDistance);

                // preview velocity (direction of drag, scaled)
                Vector2 initialVelocity = dragVec.normalized * (dist * dragToSpeed);
                preview.ShowPreview(spacecraftRb.position, initialVelocity);

            currentAimVelocity = initialVelocity;
            currentAimSpeed = initialVelocity.magnitude;
            currentAimAngleDeg = Mathf.Atan2(initialVelocity.y, initialVelocity.x) * Mathf.Rad2Deg;

                // Ui power bar
                if (powerBarFill) powerBarFill.fillAmount = dist / maxDragDistance;
            }

            else if (dragging && (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled))
            {
                LaunchFromPointer(t.position);
            }
        }

    }

    // --------------------------------
    // Launch Logic
    // --------------------------------

    private void LaunchFromPointer(Vector2 pointerPos)
    {
        dragging = false;

        // Stop stretch sound
        AudioManager.Instance.StopStretch();

        // Play thrust sound
        AudioManager.Instance.StopAllGameplaySounds();
        AudioManager.Instance.PlayThrust();

        Vector2 world = mainCam.ScreenToWorldPoint(pointerPos);
        Vector2 dragVec = dragStart - world;  // pull-back vector
        float dist = Mathf.Clamp(dragVec.magnitude, 0f, maxDragDistance);

        // normalized power (0 to 1)
        lastLaunchPower = dist / maxDragDistance;

        // freeze last shot flag
        hasLastShot = true;

        // calculate force
        Vector2 launchForce = dragVec.normalized * (dist * dragToSpeed);

        // reset UI and preview
        preview.Clear();
        if (powerBarFill) powerBarFill.fillAmount = 0f;
        if (powerBarRoot) powerBarRoot.SetActive(false);

        // clear previous motion
        spacecraftRb.velocity = Vector2.zero;
        spacecraftRb.angularVelocity = 0f;

        // launch with physics
        spacecraftRb.AddForce(launchForce, ForceMode2D.Impulse);

        //Enable gravity after launch
        GravityPull gravity = FindAnyObjectByType<GravityPull>();
        if (gravity != null)
        {
            gravity.SetTarget(spacecraftRb);
        }

        // disable launch while moving
        this.enabled = false;

        Debug.Log($"[InputDragLaunch] Launched with power: {lastLaunchPower:F2}");
    }
    
    public float GetLastLaunchPower()
    {
        return lastLaunchPower;
    }

}
