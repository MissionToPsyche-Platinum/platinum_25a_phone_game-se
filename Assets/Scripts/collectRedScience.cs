using UnityEngine;

public class collectPoints : MonoBehaviour
{
    public float pointsAdded = 0f;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()  
    {
        
     
    }


    //detects click of a mouse or touchscreen input and collects the science of the redbubble and adds 1 point
    //works with mouse and touch screen
    public void OnMouseDown()
    {
        Destroy(gameObject);
        pointsAdded = 1;
        Debug.Log("Points added: " + pointsAdded);
    }    

    
}
