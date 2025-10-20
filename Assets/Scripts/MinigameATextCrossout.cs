using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MinigameATextCrossout : MonoBehaviour
{
    private bool isCrossedOut = false;

    public void ToggleCrossout()
    {
        isCrossedOut = !isCrossedOut;
        UpdateTextAppearance();
    }

    private void UpdateTextAppearance()
    {
        TextMeshProUGUI textComponent = this.GetComponent<TextMeshProUGUI>();
        if (isCrossedOut)
        {
            textComponent.text = "<s>" + textComponent.text + "</s>";
        }
        else
        {
            textComponent.text = textComponent.text.Replace("<s>", "").Replace("</s>", "");
        }
    }
}
