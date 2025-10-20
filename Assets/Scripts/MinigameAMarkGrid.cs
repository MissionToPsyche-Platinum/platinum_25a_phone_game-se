using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinigameAMarkGrid : MonoBehaviour
{
    [SerializeField] private GameObject mark;
    private bool isMarked;

    public void ShowMark()
    {
        if (!isMarked)
        {
            mark.SetActive(true);
            //later, add points to player score
        }
    }
}
