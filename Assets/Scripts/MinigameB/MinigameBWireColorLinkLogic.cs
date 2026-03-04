using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class MinigameBWireColorLinkLogic : MonoBehaviour
{
    [SerializeField] private AudioClipManager audioClipManager;

    [SerializeField] private GameObject[] minigameBgrid;
    [SerializeField] private GameObject endScreen;
    [SerializeField] private GameObject tutorialScreen;

    [SerializeField] private Sprite halfWire;
    [SerializeField] private Sprite fullWire;
    [SerializeField] private Sprite bentWire;

    [SerializeField] private Sprite circle;
    [SerializeField] private Sprite block;

    private int[] grid = new int[36];

    private ArrayList wire1 = new ArrayList();
    private ArrayList wire2 = new ArrayList();
    private ArrayList wire3 = new ArrayList();

    private int[] wire1ends = new int[2];
    private int[] wire2ends = new int[2];
    private int[] wire3ends = new int[2];

    private int lastWirePlaced = -1;
    private int currentWire = -1;

    private bool dragging = false;


    private const string PREF_TUT_KEY = "TutorialsOn";

    private bool safeIndex(int index)
    {
        return (index >= 0 && index < 36 && !wire1.Contains(index) && !wire2.Contains(index) && !wire3.Contains(index));
    }

    private bool safeAroundIndex(int index)
    {
        return (safeIndex(index + 1) || safeIndex(index - 1) || safeIndex(index + 6) || safeIndex(index - 6));
    }

    void Start()
    {
        bool showTutorial = PlayerPrefs.GetInt(PREF_TUT_KEY, 1) == 1;
        if (showTutorial)
        {
            tutorialScreen.SetActive(true);
        }
        else
        {
            tutorialScreen.SetActive(false);
        }

        for (int i = 0; i < minigameBgrid.Length; i++)
        {
            grid[i] = 0;
            minigameBgrid[i].GetComponent<Image>().color = new Color32(255, 255, 255, 0);
        }
        //generate nodes
        Random.InitState(DateTime.Now.Millisecond);
        //generate wire 1
        int length = Random.Range(1, 11);
        int index = Random.Range(0,36);
        grid[index] = 1;
        minigameBgrid[index].GetComponent<Image>().sprite = circle;
        minigameBgrid[index].GetComponent<Image>().color = new Color32(243, 242, 244, 255);
        wire1ends[0] = index;
        wire1.Add(index);
        for (int i = 0; i < length; i++)
        {
            int direction = Random.Range(0, 4);
            switch (direction)
            {
                case 0:
                    //up
                    if (index% 6 == 5)
                    {
                        i -= 1;
                    } else 
                    {
                        index += 1;
                        direction = 1;
                    }
                    break;
                case 1:
                    //right
                    index += 6;
                    direction = 6;
                    break;
                case 2:
                    //down
                    if (index % 6 == 0)
                    {
                        i -= 1;
                    } else 
                    {
                        index -= 1;
                        direction = -1;
                    }
                    break;
                case 3:
                    //left
                    index -= 6;
                    direction = -6;
                    break;
                default:
                    break;
            }
            if (!safeIndex(index))
            {
                i -= 1;
                index -= direction;
            }
            else
            {
                wire1.Add(index);
                //show generated wires for debugging
                //minigameBgrid[index].GetComponent<Image>().sprite = halfWire;
                //minigameBgrid[index].GetComponent<Image>().color = new Color32(255, 255, 255, 255);
            }
            if (!safeAroundIndex(index))
            {
                break;
            }
        }
        grid[index] = 1;
        minigameBgrid[index].GetComponent<Image>().sprite = circle;
        minigameBgrid[index].GetComponent<Image>().color = new Color32(243, 242, 244, 255);
        wire1ends[1] = index;

        //generate wire 2
        length = Random.Range(1, 11);
        index = Random.Range(0, 36);
        while (wire1.Contains(index))
        {
            index = Random.Range(0, 36);
        }
        if (safeAroundIndex(index))
        {
            wire1.Add(index);
        }
        else 
        {
            while (wire1.Contains(index))
            {
                index = Random.Range(0, 36);
            }
            wire1.Add(index);
        }
        grid[index] = 2;
        minigameBgrid[index].GetComponent<Image>().sprite = circle;
        minigameBgrid[index].GetComponent<Image>().color = new Color32(196, 192, 198, 255);
        wire2ends[0] = index;
        wire2.Add(index);
        for (int i = 0; i < length; i++)
        {
            int direction = Random.Range(0, 4);
            switch (direction)
            {
                case 0:
                    //up
                    if (index % 6 == 5)
                    {
                        i -= 1;
                    } else 
                    {
                        index += 1;
                        direction = 1;
                    }
                    break;
                case 1:
                    //right
                    index += 6;
                    direction = 6;
                    break;
                case 2:
                    //down
                    if (index % 6 == 0)
                    {
                        i -= 1;
                    } else 
                    {
                        index -= 1;
                        direction = -1;
                    }
                    break;
                case 3:
                    //left
                    index -= 6;
                    direction = -6;
                    break;
                default:
                    break;
            }
            if (!safeIndex(index))
            {
                i -= 1;
                index -= direction;
            }
            else 
            { 
                wire2.Add(index);
                //show generated wires for debugging
                //minigameBgrid[index].GetComponent<Image>().sprite = fullWire;
                //minigameBgrid[index].GetComponent<Image>().color = new Color32(255, 255, 255, 255);
            }
            if (!safeAroundIndex(index))
            {
                break;
            }
        }
        grid[index] = 2;
        minigameBgrid[index].GetComponent<Image>().sprite = circle;
        minigameBgrid[index].GetComponent<Image>().color = new Color32(196, 192, 198, 255);
        wire2ends[1] = index;

        //generate wire 3
        length = Random.Range(2, 11);
        index = Random.Range(0, 36);
        while (wire1.Contains(index) || wire2.Contains(index))
        {
            index = Random.Range(0, 36);
        }
        if (safeAroundIndex(index))
        {
            wire2.Add(index);
        } else{
            while (wire1.Contains(index) || wire2.Contains(index))
            {
                index = Random.Range(0, 36);
            }
            if (safeAroundIndex(index))
            {
                wire2.Add(index);
            } else {
                while (wire1.Contains(index) || wire2.Contains(index))
                {
                    index = Random.Range(0, 36);
                }
                wire2.Add(index);
            }
        }
        grid[index] = 3;
        minigameBgrid[index].GetComponent<Image>().sprite = circle;
        minigameBgrid[index].GetComponent<Image>().color = new Color32(136, 129, 142, 255);
        wire3ends[0] = index;
        wire3.Add(index);
        for (int i = 0; i < length; i++)
        {
            int direction = Random.Range(0, 4);
            switch (direction)
            {
                case 0:
                    //up
                    if (index % 6 == 5)
                    {
                        i -= 1;
                    } else 
                    {
                        index += 1;
                        direction = 1;
                    }
                    break;
                case 1:
                    //right
                    index += 6;
                    direction = 6;
                    break;
                case 2:
                    //down
                    if (index % 6 == 0)
                    {
                        i -= 1;
                    } else
                    {
                        index -= 1;
                        direction = -1;
                    }
                    break;
                case 3:
                    //left
                    index -= 6;
                    direction = -6;
                    break;
                default:
                    break;
            }
            if (!safeIndex(index))
            {
                i -= 1;
                index -= direction;
            }
            else
            {
                wire3.Add(index);
                //show generated wires for debugging
                //minigameBgrid[index].GetComponent<Image>().sprite = bentWire;
                //minigameBgrid[index].GetComponent<Image>().color = new Color32(255, 255, 255, 255);
            }
            if (!safeAroundIndex(index)) 
            {
                break;
            }
        }
        grid[index] = 3;
        minigameBgrid[index].GetComponent<Image>().sprite = circle;
        minigameBgrid[index].GetComponent<Image>().color = new Color32(136, 129, 142, 255);
        wire3ends[1] = index;

        //generate blocks
        int blocks = Random.Range(3, 7);
        for (int i = 0; i < blocks; i++)
        {
            index = Random.Range(0, 36);
            while (wire1.Contains(index) || wire2.Contains(index) || wire3.Contains(index) || grid[index] == -1)
            {
                index = Random.Range(0, 36);
            }
            grid[index] = -1;
            minigameBgrid[index].GetComponent<Image>().sprite = block;
            minigameBgrid[index].GetComponent<Image>().color = new Color32(18, 3, 29, 255);
        }
        wire1.Clear();
        wire2.Clear();
        wire3.Clear();
    }

    // Update is called once per frame
    void Update()
    {
        if(dragging)
        {
            PlaceWire();
        }
    }

    public void PlaceWire()
    {
        Vector2 mousePosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        Collider2D[] colliders = Physics2D.OverlapPointAll(mousePosition);
        if (colliders.Length == 0)
        {
            return;
        }
        Collider2D current = colliders.GetValue(0) as Collider2D;
        for (int i=0; i < minigameBgrid.Length; i++)
        {
            if (minigameBgrid[i].GetComponent<Collider2D>().Equals(current) && i != lastWirePlaced)
            {
                if (grid[i] > 0)
                {
                    if (lastWirePlaced == -1)
                    {
                        audioClipManager.PlayClick();
                        AddToLine(i, grid[i]);
                        lastWirePlaced = i;
                        dragging = true;
                        currentWire = grid[i];
                    }
                    else
                    {
                        if (grid[lastWirePlaced] == -1)
                        {
                            if (currentWire == grid[i])
                            {
                                AddToLine(i, grid[i]);
                                if (lastWirePlaced - i == 1)
                                { //down
                                    if (minigameBgrid[lastWirePlaced].transform.rotation.eulerAngles.z == 180f)
                                    {
                                        minigameBgrid[lastWirePlaced].GetComponent<Image>().sprite = fullWire;
                                    }
                                    else if (minigameBgrid[lastWirePlaced].transform.rotation.eulerAngles.z == 90f)
                                    {
                                        minigameBgrid[lastWirePlaced].transform.Rotate(0f, 0f, -90f);
                                        minigameBgrid[lastWirePlaced].GetComponent<Image>().sprite = bentWire;
                                    }
                                    else if (minigameBgrid[lastWirePlaced].transform.rotation.eulerAngles.z == 270f)
                                    {
                                        minigameBgrid[lastWirePlaced].GetComponent<Image>().sprite = bentWire;
                                    }
                                }
                                else if (lastWirePlaced - i == -1)
                                {//up
                                    if (minigameBgrid[lastWirePlaced].transform.rotation.eulerAngles.z == 0f)
                                    {
                                        minigameBgrid[lastWirePlaced].GetComponent<Image>().sprite = fullWire;
                                    }
                                    else if (minigameBgrid[lastWirePlaced].transform.rotation.eulerAngles.z == 90f)
                                    {
                                        minigameBgrid[lastWirePlaced].GetComponent<Image>().sprite = bentWire;
                                    }
                                    else if (minigameBgrid[lastWirePlaced].transform.rotation.eulerAngles.z == 270f)
                                    {
                                        minigameBgrid[lastWirePlaced].transform.Rotate(0f, 0f, -90f);
                                        minigameBgrid[lastWirePlaced].GetComponent<Image>().sprite = bentWire;
                                    }
                                }
                                else if (lastWirePlaced - i == 6)
                                { // left
                                    if (minigameBgrid[lastWirePlaced].transform.rotation.eulerAngles.z == 90f)
                                    {
                                        minigameBgrid[lastWirePlaced].GetComponent<Image>().sprite = fullWire;
                                    }
                                    else if (minigameBgrid[lastWirePlaced].transform.rotation.eulerAngles.z == 0f)
                                    {
                                        minigameBgrid[lastWirePlaced].transform.Rotate(0f, 0f, -90f);
                                        minigameBgrid[lastWirePlaced].GetComponent<Image>().sprite = bentWire;
                                    }
                                    else if (minigameBgrid[lastWirePlaced].transform.rotation.eulerAngles.z == 180f)
                                    {
                                        minigameBgrid[lastWirePlaced].GetComponent<Image>().sprite = bentWire;
                                    }
                                }
                                else if (lastWirePlaced - i == -6)
                                {// right
                                    if (minigameBgrid[lastWirePlaced].transform.rotation.eulerAngles.z == 270f)
                                    {
                                        minigameBgrid[lastWirePlaced].GetComponent<Image>().sprite = fullWire;
                                    }
                                    else if (minigameBgrid[lastWirePlaced].transform.rotation.eulerAngles.z == 0f)
                                    {
                                        minigameBgrid[lastWirePlaced].GetComponent<Image>().sprite = bentWire;
                                    }
                                    else if (minigameBgrid[lastWirePlaced].transform.rotation.eulerAngles.z == 180f)
                                    {
                                        minigameBgrid[lastWirePlaced].transform.Rotate(0f, 0f, -90f);
                                        minigameBgrid[lastWirePlaced].GetComponent<Image>().sprite = bentWire;
                                    }
                                }
                                lastWirePlaced = i;
                            }
                            else
                            {
                                ClearLine(currentWire);
                                audioClipManager.PlayIncorrect();
                            }
                        }
                    }
                }
                else if (grid[i] == 0)
                {
                    if (lastWirePlaced != -1 && (lastWirePlaced == i + 1 || lastWirePlaced == i - 1 || lastWirePlaced == i + 6 || lastWirePlaced == i - 6))
                    {
                        AddToLine(i, currentWire);
                        if (grid[lastWirePlaced] == -1)
                        {
                            if (lastWirePlaced - i == 1)
                            { //down
                                if (minigameBgrid[lastWirePlaced].transform.rotation.eulerAngles.z == 180f)
                                {
                                    minigameBgrid[lastWirePlaced].GetComponent<Image>().sprite = fullWire;
                                }
                                else if (minigameBgrid[lastWirePlaced].transform.rotation.eulerAngles.z == 90f)
                                {
                                    minigameBgrid[lastWirePlaced].transform.Rotate(0f, 0f, -90f);
                                    minigameBgrid[lastWirePlaced].GetComponent<Image>().sprite = bentWire;
                                }
                                else if (minigameBgrid[lastWirePlaced].transform.rotation.eulerAngles.z == 270f)
                                {
                                    minigameBgrid[lastWirePlaced].GetComponent<Image>().sprite = bentWire;
                                }
                            }
                            else if (lastWirePlaced - i == -1) 
                            {//up
                                if (minigameBgrid[lastWirePlaced].transform.rotation.eulerAngles.z == 0f)
                                {
                                    minigameBgrid[lastWirePlaced].GetComponent<Image>().sprite = fullWire;
                                }
                                else if (minigameBgrid[lastWirePlaced].transform.rotation.eulerAngles.z == 90f)
                                {
                                    minigameBgrid[lastWirePlaced].GetComponent<Image>().sprite = bentWire;
                                }
                                else if (minigameBgrid[lastWirePlaced].transform.rotation.eulerAngles.z == 270f)
                                {
                                    minigameBgrid[lastWirePlaced].transform.Rotate(0f, 0f, -90f);
                                    minigameBgrid[lastWirePlaced].GetComponent<Image>().sprite = bentWire;
                                }
                            }
                            else if (lastWirePlaced - i == 6)
                            { // left
                                if (minigameBgrid[lastWirePlaced].transform.rotation.eulerAngles.z == 90f)
                                {
                                    minigameBgrid[lastWirePlaced].GetComponent<Image>().sprite = fullWire;
                                }
                                else if (minigameBgrid[lastWirePlaced].transform.rotation.eulerAngles.z == 0f)
                                {
                                    minigameBgrid[lastWirePlaced].transform.Rotate(0f, 0f, -90f);
                                    minigameBgrid[lastWirePlaced].GetComponent<Image>().sprite = bentWire;
                                }
                                else if (minigameBgrid[lastWirePlaced].transform.rotation.eulerAngles.z == 180f)
                                {
                                    minigameBgrid[lastWirePlaced].GetComponent<Image>().sprite = bentWire;
                                }
                            }
                            else if (lastWirePlaced - i == -6) 
                            {// right
                                if (minigameBgrid[lastWirePlaced].transform.rotation.eulerAngles.z == 270f)
                                {
                                    minigameBgrid[lastWirePlaced].GetComponent<Image>().sprite = fullWire;
                                }
                                else if (minigameBgrid[lastWirePlaced].transform.rotation.eulerAngles.z == 0f)
                                {
                                    minigameBgrid[lastWirePlaced].GetComponent<Image>().sprite = bentWire;
                                }
                                else if (minigameBgrid[lastWirePlaced].transform.rotation.eulerAngles.z == 180f)
                                {
                                    minigameBgrid[lastWirePlaced].transform.Rotate(0f, 0f, -90f);
                                    minigameBgrid[lastWirePlaced].GetComponent<Image>().sprite = bentWire;
                                }
                            }
                        }
                        minigameBgrid[i].GetComponent<Image>().sprite = halfWire;
                        minigameBgrid[i].GetComponent<Image>().color = new Color32(255, 255, 255, 255);
                        if (lastWirePlaced - i == 1)
                            minigameBgrid[i].transform.Rotate(0f, 0f, 180f);
                        else if (lastWirePlaced - i == -1)
                            minigameBgrid[i].transform.Rotate(0f, 0f, 0f);
                        else if (lastWirePlaced - i == 6)
                            minigameBgrid[i].transform.Rotate(0f, 0f, 90f);
                        else if (lastWirePlaced - i == -6)
                            minigameBgrid[i].transform.Rotate(0f, 0f, 270f);
                        grid[i] = -1;
                        lastWirePlaced = i;
                    }
                }
                else
                {
                    //clear line
                    ClearLine(currentWire);
                    audioClipManager.PlayIncorrect();
                }
            }
        }

    }

    public void dropWire()
    {
        if (lastWirePlaced == -1) 
        {
            return;
        }
        else 
        {
            int wires = 0;
            if (wire1.Contains(wire1ends[1]) && wire1.Contains(wire1ends[0]))
            {
                wires++;
            }
            else
            {
                ClearLine(1);
            }
            if (wire2.Contains(wire2ends[1]) && wire2.Contains(wire2ends[0]))
            {
                wires++;
            }
            else
            {
                ClearLine(2);
            }
            if (wire3.Contains(wire3ends[1]) && wire3.Contains(wire3ends[0]))
            {
                wires++;
            }
            else
            {
                ClearLine(3);
            }
            if (wires >= 3)
            {
                audioClipManager.PlayCongrats();
                endScreen.SetActive(true);
            }
        }
        lastWirePlaced = -1;
        currentWire = -1;
        dragging = false;
    }

    private void AddToLine(int index, int wireNumber)
    {
        switch (wireNumber)
        {
            case 1:
                if (!wire1.Contains(index))
                    wire1.Add(index);
                else{
                    ClearLine(1);
                    audioClipManager.PlayIncorrect();
                }
                break;
            case 2:
                if (!wire2.Contains(index))
                    wire2.Add(index);
                else{
                    ClearLine(2);
                    audioClipManager.PlayIncorrect();
                }
                break;
            case 3:
                if (!wire3.Contains(index))
                    wire3.Add(index);
                else{
                    ClearLine(3);
                    audioClipManager.PlayIncorrect();
                }
                break;
            default:
                break;
        }
        if (grid[index] == 0)
            grid[index] = -1;
    }

    private void ClearLine(int wireNumber)
    {
        switch (wireNumber)
        {
            case 1:
                foreach (int index in wire1)
                {
                    if (grid[index] == -1){
                        grid[index] = 0;
                        minigameBgrid[index].GetComponent<Image>().sprite = null;
                        minigameBgrid[index].GetComponent<Image>().color = new Color32(255, 255, 255, 0);
                        minigameBgrid[index].transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                    }
                }
                wire1.Clear();
                break;
            case 2:
                foreach (int index in wire2)
                {
                    if (grid[index] == -1)
                    {
                        grid[index] = 0;
                        minigameBgrid[index].GetComponent<Image>().sprite = null;
                        minigameBgrid[index].GetComponent<Image>().color = new Color32(255, 255, 255, 0);
                        minigameBgrid[index].transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                    }
                }
                wire2.Clear();
                break;
            case 3:
                foreach (int index in wire3)
                {
                    if (grid[index] == -1)
                    {
                        grid[index] = 0;
                        minigameBgrid[index].GetComponent<Image>().sprite = null;
                        minigameBgrid[index].GetComponent<Image>().color = new Color32(255, 255, 255, 0);
                        minigameBgrid[index].transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                    }
                }
                wire3.Clear();
                break;
            default:
                break;
        }
    }

    public void CloseTutorial()
    {
        tutorialScreen.SetActive(false);
    }
}
