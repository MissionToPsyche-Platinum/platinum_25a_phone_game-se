using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawn_test : MonoBehaviour
{
    [SerializeField] SpriteRenderer pointSR;
    // Start is called before the first frame update
    void Start()
    {
        pointSR.enabled = true;
        pointSR.color = Color.red;
    }

    // Update is called once per frame
    void Update()
    {
        
        
    }

    private void OnMouseDown()
    {
        pointSR.color = Color.blue;
        
    }
}
