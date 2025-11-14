using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinigameAToggle : MonoBehaviour
{
    [SerializeField] private GameObject check;
    [SerializeField] private GameObject x;
    private bool isCheck;

    [SerializeField] private GameObject tutorial2;
    [SerializeField] private GameObject tutorial3;
    [SerializeField] private GameObject tutorial5;
    [SerializeField] private GameObject tutorial6;

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

        if (tutorial2 != null && tutorial2.activeSelf)
        {
            tutorial2.SetActive(false);
            if (tutorial3 != null)
            {
                tutorial3.SetActive(true);
            }
        }
        else if (tutorial5 != null && tutorial5.activeSelf)
        {
            tutorial5.SetActive(false);
            if (tutorial6 != null)
            {
                tutorial6.SetActive(true);
            }
        }
    }

    public bool IsCheck()
    {
        return isCheck;
    }
}
