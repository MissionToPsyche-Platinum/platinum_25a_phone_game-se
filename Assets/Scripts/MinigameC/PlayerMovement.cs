using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 3.33f;
    public Rigidbody2D rb;
    public Animator animator;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void FixedUpdate()
    {
        float horizontal = 0f, vertical = 0f;

        if (PhaseCMobileInput.Active)
        {
            horizontal = PhaseCMobileInput.Horizontal;
            vertical   = PhaseCMobileInput.Vertical;
        }

        // Keyboard/gamepad always supplements - covers editor testing and physical keyboards on mobile
        float kh = Input.GetAxis("Horizontal");
        float kv = Input.GetAxis("Vertical");
        if (Mathf.Abs(kh) > Mathf.Abs(horizontal)) horizontal = kh;
        if (Mathf.Abs(kv) > Mathf.Abs(vertical))   vertical   = kv;

        animator.SetFloat("Xinput", horizontal);
        animator.SetFloat("Yinput", vertical);
        
        // Side sprites face left by default, so flip when moving right
        if (horizontal != 0 && spriteRenderer != null)
        {
            spriteRenderer.flipX = horizontal > 0;
        }
        
        // Normalize to prevent faster diagonal movement
        Vector2 inputVector = new Vector2(horizontal, vertical);

        if (inputVector.magnitude > 0.1f)
        {
            inputVector = inputVector.normalized;
        }
        
        // Apply speed directly - no acceleration buildup
        rb.velocity = inputVector * speed;
    }
}
