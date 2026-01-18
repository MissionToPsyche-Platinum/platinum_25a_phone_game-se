using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class getScore : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField] public PointTracker pt;
    [SerializeField] TMP_Text tm;

    void Start()
    {
        tm.text = pt.score + "";
    }

    // Update is called once per frame
    void Update()
    {
        tm.text = pt.score + "";
    }
}
