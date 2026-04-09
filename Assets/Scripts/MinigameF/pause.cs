using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pause : MonoBehaviour
{
    [SerializeField] orbit space;

    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 0;
        space.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        Time.timeScale = 0;
        space.enabled = false;
    }

    public void setPause()
    {
        Time.timeScale = 0;
        space.enabled = false;
    }
}
