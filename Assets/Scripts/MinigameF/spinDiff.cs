using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spinDiff : MonoBehaviour
{

    public int stage = 0;

    [SerializeField] Transform circle;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        rotateCirc(stage);
        
    }

    void rotateCirc(int stage)
    {
        float spin = stage * 0.1f;
        circle.Rotate(0, 0, spin);
    }

    public void setStage(int stage)
    {
        this.stage = stage;
    }
}
