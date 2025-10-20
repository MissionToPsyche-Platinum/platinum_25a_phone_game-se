using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinigameAToggle : MonoBehaviour
{
    [SerializeField] private GameObject check;
    [SerializeField] private GameObject x;
    private bool isCheck;

    private void Start()
    {
        x.SetActive(false);
        isCheck = true;
    }

    public void Toggle()
    {
        if (isCheck)
        {
            check.SetActive(false);
            x.SetActive(true);
            isCheck = false;
            this.transform.Rotate(0,0,180);
        }
        else
        {
            check.SetActive(true);
            x.SetActive(false);
            isCheck = true;
            this.transform.Rotate(0, 0, 180);
        }
    }

    public bool IsCheck()
    {
        return isCheck;
    }
}
