using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrientationManager : MonoBehaviour
{
    [SerializeField] private bool prefferedLandscape = true;
    bool currentOrientationIsLandscape;
    bool orientationPopupActive = true;

    [SerializeField] private GameObject OrientationPopup;


    void Update()
    {
        checkOrientation();        
    }

    void checkOrientation()
    {
        if (Screen.height < Screen.width)
        {
            currentOrientationIsLandscape = true;
        }
        else if (Screen.height > Screen.width)
        {
            currentOrientationIsLandscape = false;
        }

        if (currentOrientationIsLandscape != prefferedLandscape && orientationPopupActive)
        {
            OrientationPopup.SetActive(true);
        }
        else
        {
            OrientationPopup.SetActive(false);
        }
    }

    public void closePopup()
    {
        OrientationPopup.SetActive(false);
        orientationPopupActive = false;
    }
}
