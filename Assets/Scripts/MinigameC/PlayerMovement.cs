using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
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
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        animator.SetFloat("Xinput", horizontal);
        animator.SetFloat("Yinput", vertical);
        
        // Flip sprite for left/right movement
        // Side sprites face left by default, so flip when moving right
        if (horizontal != 0 && spriteRenderer != null)
        {
            spriteRenderer.flipX = horizontal > 0;
        }
        
        rb.velocity = new Vector2(horizontal, vertical) * speed;
    }
}
