using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class clickpenalty : MonoBehaviour
{

    [SerializeField] public PointTracker pt;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnMouseDown()
    {
        int penalty = -2;
        pt.addPoints(penalty);
    }
}
