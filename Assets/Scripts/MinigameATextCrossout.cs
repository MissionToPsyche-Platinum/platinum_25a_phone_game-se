using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MinigameATextCrossout : MonoBehaviour
{
    private bool isCrossedOut = false;

    [SerializeField] private GameObject currentTutorial;
    [SerializeField] private GameObject nextTutorial;

    public void ToggleCrossout()
    {
        isCrossedOut = !isCrossedOut;
        UpdateTextAppearance();

        if (currentTutorial != null)
        {
            currentTutorial.SetActive(false);
        }
        if (nextTutorial != null)
        {
            nextTutorial.SetActive(true);
        }
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
