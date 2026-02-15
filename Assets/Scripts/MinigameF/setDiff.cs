using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class setDiff : MonoBehaviour
{

    public static setDiff Instance { get; private set; }
    private int diff = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    public int getDiff()
    {
        return diff;
    }

    public void setDif(int diff)
    {
        Instance.diff = diff;
    }
}
