using UnityEngine;

public class collectPoints : MonoBehaviour
{
    public float points = 0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()  
    {
        
     
    }

    public void OnMouseDown()
    {
        Destroy(gameObject);
        points++;
        Debug.Log("Points added: " + points);
    }

    
}
