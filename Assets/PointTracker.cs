using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointTracker : MonoBehaviour
{
    public PointTracker pt;
    public float score = 0;
    [SerializeField] TextMesh  tm;

    // Start is called before the first frame update
    void Start()
    {
        tm.text = "Points: " + score;
        
    }

    // Update is called once per frame
    void Update()
    {
        tm.text = "Points: " + score;
    }
    
    public PointTracker getPointTracker()
    {
        return pt;
    }

    public void addPoints(float pointsAdded)
    {
        score += pointsAdded;
    }
}
