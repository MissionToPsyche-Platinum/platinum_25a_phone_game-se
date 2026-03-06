using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinigameBPowerBalanceButtonToggle : MonoBehaviour
{
    [SerializeField] private AudioClipManager audioClipManager;

    private bool isToggled = false;

    public void ToggleButton()
    {
        isToggled = !isToggled;
        if (isToggled)
        {
            GetComponent<Image>().color = Color.white;
        }
        else
        {
            GetComponent<Image>().color = Color.black;
        }
        audioClipManager.PlayClick();
    }

    public bool GetToggleState()
    {
        return isToggled;
    }
}
