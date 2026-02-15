using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class difficultyStart : MonoBehaviour
{
    public int difficulty = 0;
    // Start is called before the first frame update
    void Start()
    {
        setDiff.Instance.setDif(difficulty);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void setDif(int diff)
    {
        difficulty = diff;
    }
}
