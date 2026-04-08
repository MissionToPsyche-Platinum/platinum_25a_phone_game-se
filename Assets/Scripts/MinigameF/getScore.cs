using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class getScore : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField] displayScore pt;
    [SerializeField] TMP_Text tm;

    void Start()
    {
        tm.text = pt.getScore() + "";
    }

    // Update is called once per frame
    void Update()
    {
        tm.text = pt.getScore() + "";
    }
}
