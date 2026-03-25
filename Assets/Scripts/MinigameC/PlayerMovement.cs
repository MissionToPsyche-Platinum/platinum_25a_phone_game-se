using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 0.4f;
    public Rigidbody2D rb;
    public Animator animator;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        // Get SpriteRenderer component if not assigned
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void FixedUpdate()
    {
        float horizontal, vertical;

        if (PhaseCMobileInput.Active)
        {
            horizontal = PhaseCMobileInput.Horizontal;
            vertical   = PhaseCMobileInput.Vertical;
        }
        else
        {
            horizontal = Input.GetAxis("Horizontal");
            vertical   = Input.GetAxis("Vertical");
        }

        animator.SetFloat("Xinput", horizontal);
        animator.SetFloat("Yinput", vertical);
        
        // Flip sprite for left/right movement
        // Side sprites face left by default, so flip when moving right
        if (horizontal != 0 && spriteRenderer != null)
        {
            spriteRenderer.flipX = horizontal > 0;
        }
        
        // Create movement vector and normalize to prevent faster diagonal movement
        Vector2 inputVector = new Vector2(horizontal, vertical);
        
        // Normalize to ensure consistent speed in all directions (including diagonals)
        if (inputVector.magnitude > 0.1f)
        {
            inputVector = inputVector.normalized;
        }
        
        // Apply speed directly - no acceleration buildup
        rb.velocity = inputVector * speed;
    }
}
