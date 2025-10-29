using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketController : MonoBehaviour
{
    public float moveSpeed = 7.5f;
    public float maxDragDistance = 250f; // drag sensitivity 
    public float smoothingSpeed = 10f; // higher = snappier, lower = smoother

    private int touchId = -1; // to track active touch
    private Vector2 startTouchPosition;
    private float smoothedInput = 0f;

    // screen boundaries
    private float minX, maxX;

    void Start()
    {
        // calculate screen bounds in world units
        float camDistance = Vector3.Distance(transform.position, Camera.main.transform.position);
        Vector2 screenBounds = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, camDistance));
        float spriteHalfWidth = GetComponent<SpriteRenderer>().bounds.extents.x;
        minX = -screenBounds.x + spriteHalfWidth;
        maxX = screenBounds.x - spriteHalfWidth;
    }

    void Update()
    {
        float moveInput = 0f;

        // handle touch input
        if (Input.touchCount > 0)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);

                if (touch.phase == TouchPhase.Began)
                {
                    // restrict touch to bottom half of screen (keeps user away from exit button)
                    if (touch.position.y < Screen.height * 0.5f && touchId == -1)
                    {
                        touchId = touch.fingerId;
                        startTouchPosition = touch.position;
                    }
                }

                if (touch.fingerId == touchId)
                {
                    if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
                    {
                        // calculate delta
                        Vector2 delta = touch.position - startTouchPosition;
                        float deltaX = delta.x;

                        // convert delta to normalized input
                        moveInput = Mathf.Clamp(deltaX / maxDragDistance, -1f, 1f);
                    }
                    else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                    {
                        touchId = -1;
                        moveInput = 0f;
                    }
                }
            }
        }
        else
        {
            // fallback to keyboard
            moveInput = Input.GetAxis("Horizontal");
        }

        smoothedInput = Mathf.Lerp(smoothedInput, moveInput, Time.deltaTime * smoothingSpeed);

        Vector3 move = new Vector3(smoothedInput, 0, 0);
        transform.position += move * moveSpeed * Time.deltaTime;

        // clamp position within bounds
        float clampedX = Mathf.Clamp(transform.position.x, minX, maxX);
        transform.position = new Vector3(clampedX, transform.position.y, transform.position.z);
    }
}
