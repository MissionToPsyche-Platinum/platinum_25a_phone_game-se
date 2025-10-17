using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketController : MonoBehaviour
{
    public float moveSpeed = 5f;

    // screen boundaries
    private float minX;
    private float maxX;

    void Start()
    {
        // calculate screen boundaries based on the camera
        float camDistance = Vector3.Distance(transform.position, Camera.main.transform.position);
        Vector2 screenBounds = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, camDistance));

        float spriteHalfWidth = GetComponent<SpriteRenderer>().bounds.extents.x;

        minX = -screenBounds.x + spriteHalfWidth;
        maxX = screenBounds.x - spriteHalfWidth;
    }

    void Update()
    {
        float moveInput = Input.GetAxis("Horizontal"); // gets input from arrow keys or A/D
        Vector3 move = new Vector3(moveInput, 0, 0);
        transform.position += move * moveSpeed * Time.deltaTime;

        // clamp position within screen bounds
        float clampedX = Mathf.Clamp(transform.position.x, minX, maxX);
        transform.position = new Vector3(clampedX, transform.position.y, transform.position.z);
    }
}
