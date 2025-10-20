using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinigameAMarkGrid : MonoBehaviour
{
    [SerializeField] private GameObject mark;
    private bool isMarked;
    [SerializeField] private MinigameAToggle toggle;
    [SerializeField] private bool check;

    public void ShowMark()
    {
        if (!isMarked)
        {
            if (toggle.IsCheck())
            {
                if (!check)
                {
                    mark.GetComponent<Image>().color = Color.red;
                }
            }
            else
            {
                if (check)
                {
                    mark.GetComponent<Image>().color = Color.red;
                }
            }
            mark.SetActive(true);
            //later, add points to player score
        }
    }
}
