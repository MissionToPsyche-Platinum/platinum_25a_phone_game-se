
using UnityEngine;
using TMPro;

public class GameManger : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text messageText;

    [Header("Controls")]
    [SerializeField] private InputDragLaunch dragLaunch;

    private bool gameEnded = false;

    public void OrbitSuccess()
    {
        if (gameEnded) return;
        gameEnded = true;

        ShowMessage("Orbit Achieved!");
        DisableControl();
    }

    public void MissedOrbit()
    {
        if (gameEnded) return;
        gameEnded = true;
        ShowMessage("Missed Orbit! Try Again.");
        DisableControl();
    }

    private void ShowMessage(string msg)
    {
        if (messageText != null)
        { 
            messageText.text = msg;
            messageText.gameObject.SetActive(true);
        }
    }
    
    private void DisableControl()
    {
        if (dragLaunch != null)
        {
            dragLaunch.enabled = false;
        }
    }
}
