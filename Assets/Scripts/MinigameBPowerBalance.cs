using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MinigameBPowerBalance : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI LeftSourceNumber;
    [SerializeField] private TextMeshProUGUI MiddleSourceNumber;
    [SerializeField] private TextMeshProUGUI RightSourceNumber;
    [SerializeField] private TextMeshProUGUI LeftSinkNumber;
    [SerializeField] private TextMeshProUGUI MiddleSinkNumber;
    [SerializeField] private TextMeshProUGUI RightSinkNumber;

    [SerializeField] private TextMeshProUGUI LeftSinkScreen;
    [SerializeField] private TextMeshProUGUI MiddleSinkScreen;
    [SerializeField] private TextMeshProUGUI RightSinkScreen;

    [SerializeField] private MinigameBPowerBalanceButtonToggle LeftSourceButtonA;
    [SerializeField] private MinigameBPowerBalanceButtonToggle LeftSourceButtonB;
    [SerializeField] private MinigameBPowerBalanceButtonToggle LeftSourceButtonC;
    [SerializeField] private MinigameBPowerBalanceButtonToggle MiddleSourceButtonA;
    [SerializeField] private MinigameBPowerBalanceButtonToggle MiddleSourceButtonB;
    [SerializeField] private MinigameBPowerBalanceButtonToggle MiddleSourceButtonC;
    [SerializeField] private MinigameBPowerBalanceButtonToggle RightSourceButtonA;
    [SerializeField] private MinigameBPowerBalanceButtonToggle RightSourceButtonB;
    [SerializeField] private MinigameBPowerBalanceButtonToggle RightSourceButtonC;

    [SerializeField] private GameObject TutorialScreen;
    [SerializeField] private GameObject WinScreen;

    private float LeftSourceValue = 30;
    private float MiddleSourceValue = 15;
    private float RightSourceValue = 15;

    private float LeftSinkFinalValue = 25;
    private float MiddleSinkFinalValue = 25;
    private float RightSinkFinalValue = 10;

    private float LeftSinkCurrentValue = 0;
    private float MiddleSinkCurrentValue = 0;
    private float RightSinkCurrentValue = 0;

    // Start is called before the first frame update
    void Start()
    {
        LeftSourceNumber.text = LeftSourceValue.ToString();
        MiddleSourceNumber.text = MiddleSourceValue.ToString();
        RightSourceNumber.text = RightSourceValue.ToString();

        LeftSinkNumber.text = "A " + LeftSinkFinalValue.ToString();
        MiddleSinkNumber.text = "B " + MiddleSinkFinalValue.ToString();
        RightSinkNumber.text = "C " + RightSinkFinalValue.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        int leftOutputs = 0;
        if (LeftSourceButtonA.GetToggleState())
        {
            leftOutputs += 1;
        }
        if (LeftSourceButtonB.GetToggleState())
        {
            leftOutputs += 1;
        }
        if (LeftSourceButtonC.GetToggleState())
        {
            leftOutputs += 1;
        }
        float leftOutput = 0;
        if (leftOutputs != 0)
        {
            leftOutput = LeftSourceValue / leftOutputs;
        }
        int middleOutputs = 0;
        if (MiddleSourceButtonA.GetToggleState())
        {
            middleOutputs += 1;
        }
        if (MiddleSourceButtonB.GetToggleState())
        {
            middleOutputs += 1;
        }
        if (MiddleSourceButtonC.GetToggleState())
        {
            middleOutputs += 1;
        }
        float middleOutput = 0;
        if (middleOutputs != 0)
        {
            middleOutput = RightSourceValue / middleOutputs;
        }
        int rightOutputs = 0;
        if (RightSourceButtonA.GetToggleState())
        {
            rightOutputs += 1;
        }
        if (RightSourceButtonB.GetToggleState())
        {
            rightOutputs += 1;
        }
        if (RightSourceButtonC.GetToggleState())
        {
            rightOutputs += 1;
        }
        float rightOutput = 0;
        if (rightOutputs != 0)
        {
            rightOutput = RightSourceValue / rightOutputs;
        }

        LeftSinkCurrentValue = 0;
        if (LeftSourceButtonA.GetToggleState())
        {
            LeftSinkCurrentValue += leftOutput;
        } 
        if (MiddleSourceButtonA.GetToggleState())
        {
            LeftSinkCurrentValue += middleOutput;
        }
        if (RightSourceButtonA.GetToggleState())
        {
            LeftSinkCurrentValue += rightOutput;
        }

        MiddleSinkCurrentValue = 0;
        if (LeftSourceButtonB.GetToggleState())
        {
            MiddleSinkCurrentValue += leftOutput;
        }
        if (MiddleSourceButtonB.GetToggleState())
        {
            MiddleSinkCurrentValue += middleOutput;
        }
        if (RightSourceButtonB.GetToggleState())
        {
            MiddleSinkCurrentValue += rightOutput;
        }

        RightSinkCurrentValue = 0;
        if (LeftSourceButtonC.GetToggleState())
        {
            RightSinkCurrentValue += leftOutput;
        }
        if (MiddleSourceButtonC.GetToggleState())
        {
            RightSinkCurrentValue += middleOutput;
        }
        if (RightSourceButtonC.GetToggleState())
        {
            RightSinkCurrentValue += rightOutput;
        }


        LeftSinkScreen.text = LeftSinkCurrentValue.ToString();
        MiddleSinkScreen.text = MiddleSinkCurrentValue.ToString();
        RightSinkScreen.text = RightSinkCurrentValue.ToString();

        if (LeftSinkCurrentValue == LeftSinkFinalValue && MiddleSinkCurrentValue == MiddleSinkFinalValue && RightSinkCurrentValue == RightSinkFinalValue)
        {
            WinScreen.SetActive(true);
        }
    }

    public void CloseTutorial()
    {
        TutorialScreen.SetActive(false);
    }
}
