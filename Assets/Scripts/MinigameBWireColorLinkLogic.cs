using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class MinigameBWireColorLinkLogic : MonoBehaviour
{
    [SerializeField] private GameObject[] minigameBgrid;
    [SerializeField] private GameObject endScreen;

    [SerializeField] private Sprite halfWire;
    [SerializeField] private Sprite fullWire;
    [SerializeField] private Sprite bentWire;

    [SerializeField] private Sprite circle;
    [SerializeField] private Sprite block;

    private int[] grid = new int[36];

    private ArrayList wire1;
    private ArrayList wire2;
    private ArrayList wire3;

    private int[] wire1ends = new int[2];
    private int[] wire2ends = new int[2];
    private int[] wire3ends = new int[2];

    private int lastWirePlaced;

    private Boolean dragging = false;

    void Start()
    {
        for (int i = 0; i < minigameBgrid.Length; i++)
        {
            grid[i] = 0;
            minigameBgrid[i].GetComponent<Image>().color = new Color32(255, 255, 255, 0);
        }
        //generate nodes
        int index = 10;
        grid[index] = 1;
        minigameBgrid[index].GetComponent<Image>().sprite = circle;
        minigameBgrid[index].GetComponent<Image>().color = new Color32(243, 242, 244, 255);
        wire1ends[0] = index;
        index = 30;
        grid[index] = 1;
        minigameBgrid[index].GetComponent<Image>().sprite = circle;
        minigameBgrid[index].GetComponent<Image>().color = new Color32(243, 242, 244, 255);
        wire1ends[1] = index;
        index = 7;
        grid[index] = 2;
        minigameBgrid[index].GetComponent<Image>().sprite = circle;
        minigameBgrid[index].GetComponent<Image>().color = new Color32(196, 192, 198, 255);
        wire2ends[0] = index;
        index = 31;
        grid[index] = 2;
        minigameBgrid[index].GetComponent<Image>().sprite = circle;
        minigameBgrid[index].GetComponent<Image>().color = new Color32(196, 192, 198, 255);
        wire2ends[1] = index;
        index = 27;
        grid[index] = 3;
        minigameBgrid[index].GetComponent<Image>().sprite = circle;
        minigameBgrid[index].GetComponent<Image>().color = new Color32(136, 129, 142, 255);
        wire3ends[0] = index;
        index = 35;
        grid[index] = 3;
        minigameBgrid[index].GetComponent<Image>().sprite = circle;
        minigameBgrid[index].GetComponent<Image>().color = new Color32(136, 129, 142, 255);
        wire3ends[1] = index;
        //generate blocks
        index = 9;
        grid[index] = -1;
        minigameBgrid[index].GetComponent<Image>().sprite = block;
        minigameBgrid[index].GetComponent<Image>().color = new Color32(18, 3, 29, 255);
        index = 25;
        grid[index] = -1;
        minigameBgrid[index].GetComponent<Image>().sprite = block;
        minigameBgrid[index].GetComponent<Image>().color = new Color32(18, 3, 29, 255);
        index = 34;
        grid[index] = -1;
        minigameBgrid[index].GetComponent<Image>().sprite = block;
        minigameBgrid[index].GetComponent<Image>().color = new Color32(18, 3, 29, 255);
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
            Debug.Log("Not hovering over anything");
            return;
        }
        Collider2D current = colliders.GetValue(0) as Collider2D;
        for (int i=0; i < minigameBgrid.Length; i++)
        {
            if (minigameBgrid[i].GetComponent<Collider2D>().Equals(current) && i != lastWirePlaced)
            {
                Debug.Log("Hovering over " + i);
                if (grid[i] > 0)
                {
                    if (lastWirePlaced == -1)
                    {
                        AddToLine(i, grid[i]);
                        lastWirePlaced = i;
                        dragging = true;
                        Debug.Log("Started placing wire at " + i);
                    }
                    else
                    {
                        switch (grid[i])
                        {
                            case 1:
                                if (wire1.Contains(lastWirePlaced))
                                {
                                    AddToLine(i, grid[i]);
                                    lastWirePlaced = i;
                                }
                                break;
                            case 2:
                                if (wire2.Contains(lastWirePlaced))
                                {
                                    AddToLine(i, grid[i]);
                                    lastWirePlaced = i;
                                }
                                break;
                            case 3:
                                if (wire3.Contains(lastWirePlaced))
                                {
                                    AddToLine(i, grid[i]);
                                    lastWirePlaced = i;
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }
                else if (grid[i] == 0)
                {
                    if (lastWirePlaced != -1 && (lastWirePlaced == i +1 || lastWirePlaced == i - 1 || lastWirePlaced == 1 + 6 || lastWirePlaced == i - 6))
                    {
                        AddToLine(i, grid[i]);
                        lastWirePlaced = i;
                        minigameBgrid[i].GetComponent<Image>().sprite = halfWire;
                        minigameBgrid[i].GetComponent<Image>().color = new Color32(255, 255, 255, 255);
                        Debug.Log("Placed wire at " + i);
                    }
                    else if (lastWirePlaced == -1)
                    {
                        AddToLine(i, grid[i]);
                        minigameBgrid[lastWirePlaced].GetComponent<Image>().sprite = fullWire;
                        lastWirePlaced = i;
                        minigameBgrid[i].GetComponent<Image>().sprite = halfWire;
                        minigameBgrid[i].GetComponent<Image>().color = new Color32(255, 255, 255, 255);
                    }
                }
                else
                {
                    //clear line
                    if (wire1.Contains(i))
                    {
                        ClearLine(1);
                    }
                    else if (wire2.Contains(i))
                    {
                        ClearLine(2);
                    }
                    else if (wire3.Contains(i))
                    {
                        ClearLine(3);
                    }
                }
            }
        }

    }

    public void dropWire()
    {
        if (lastWirePlaced == -1)
            return;
        if (grid[lastWirePlaced] == -1)
        {
            int wires = 0;
            if (wire1.Contains(wire1ends[1]) && wire1.Contains(wire1ends[0]))
            {
                wires++;
                minigameBgrid[lastWirePlaced].GetComponent<Image>().sprite = fullWire;
            }
            else
            {
                ClearLine(1);
            }
            if (wire2.Contains(wire2ends[1]) && wire2.Contains(wire2ends[0]))
            {
                wires++;
                minigameBgrid[lastWirePlaced].GetComponent<Image>().sprite = fullWire;
            }
            else
            {
                ClearLine(2);
            }
            if (wire3.Contains(wire3ends[1]) && wire3.Contains(wire3ends[0]))
            {
                wires++;
                minigameBgrid[lastWirePlaced].GetComponent<Image>().sprite = fullWire;
            }
            else
            {
                ClearLine(3);
            }
            if (wires >= 3)
            {
                endScreen.SetActive(true);
            }
        }
        lastWirePlaced = -1;
        dragging = false;
    }

    private void AddToLine(int index, int wireNumber)
    {
        switch (wireNumber)
        {
            case 1:
                wire1.Add(index);
                break;
            case 2:
                wire2.Add(index);
                break;
            case 3:
                wire3.Add(index);
                break;
            default:
                break;
        }
        grid[index] = -1;
    }

    private void ClearLine(int wireNumber)
    {
        switch (wireNumber)
        {
            case 1:
                foreach (int index in wire1)
                {
                    grid[index] = 0;
                }
                wire1.Clear();
                break;
            case 2:
                foreach (int index in wire2)
                {
                    grid[index] = 0;
                }
                wire2.Clear();
                break;
            case 3:
                foreach (int index in wire3)
                {
                    grid[index] = 0;
                }
                wire3.Clear();
                break;
            default:
                break;
        }
    }
}
