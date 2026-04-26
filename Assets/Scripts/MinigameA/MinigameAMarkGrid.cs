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

    [SerializeField] private GameObject currentTutorial;
    [SerializeField] private GameObject nextTutorial;

    [SerializeField] private AudioClipManager audioClipManager;

    public void setCheck(bool isCheck)
    {
        check = isCheck;
    }

    public void ShowMark()
    {
        if (!isMarked)
        {
            if (toggle.IsCheck())
            {
                if (!check)
                {
                    mark.GetComponent<Image>().color = Color.red;
                    audioClipManager.PlayIncorrect();
                }
                else
                {
                    mark.GetComponent<Image>().color = Color.white;
                    audioClipManager.PlayCorrect();
                }
            }
            else
            {
                if (check)
                {
                    mark.GetComponent<Image>().color = Color.red;
                    audioClipManager.PlayIncorrect();
                }
                else
                {
                    audioClipManager.PlayCorrect();
                }
            }
            mark.SetActive(true);
            isMarked = true;

            if (currentTutorial != null)
            {
                currentTutorial.SetActive(false);
            }
            if (nextTutorial != null)
            {
                nextTutorial.SetActive(true);
            }
        }
    }
}
