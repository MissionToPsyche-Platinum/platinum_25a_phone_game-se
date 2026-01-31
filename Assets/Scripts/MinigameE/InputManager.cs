
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public Rigidbody2D spacecraft;
    public float launchForce = 10f;

    Vector2 dragStart, dragEnd;
    bool dragging;

    void Update() 
    {
        if(Input.GetMouseButtonDown(0))
        {
            dragging = true;
            dragStart = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
        else if(Input.GetMouseButton(0) && dragging) 
        {
            dragEnd = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            // (optional) update a trajectory preview here
        }
        else if (Input.GetMouseButtonUp(0) && dragging)
        {
            dragging = false;
            Vector2 dir = (dragStart - dragEnd).normalized;
            float dist = Vector2.Distance(dragStart, dragEnd);
            spacecraft.AddForce(dir * dist * launchForce, ForceMode2D.Impulse);
        }
    }
}
