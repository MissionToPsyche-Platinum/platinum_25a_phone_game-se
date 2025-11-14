using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Draws a predicted trajectory while the player is dragging to aim.
/// This is a LIGHTWEIGHT preview: uses a simple fake gravity downward so we see a curve,
/// (we will replace with central gravity to Psyche in a later task.)
/// </summary>

[RequireComponent(typeof(LineRenderer))]
public class TrajectoryPreview : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D spacecraftRb;

    [Header("Line Settings")]
    [SerializeField] private int segmentCount = 30;   // how many dots/segments
    [SerializeField] private float timeStep = 0.05f; // simulation step (sec)
    [SerializeField] private float fakeGravity = -2.0f; // temporary fake downward gravity (y)

    private LineRenderer lr;

    private void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.positionCount = 0;
        lr.useWorldSpace = true;
    }

    /// <summary>
    /// call this every frame while dragging 
    /// startPos: world position where ship will start
    /// initialVelocity: world-space velocity we want to preview (Dervied from drag)
    /// </summary>

    public void ShowPreview(Vector2 startPos, Vector2 initialVelocity)
    {
        if (spacecraftRb == null) return;

        List<Vector3> points = new List<Vector3>(segmentCount);
        Vector2 pos = startPos;
        Vector2 vel = initialVelocity;  //v0 (units/sec)

        for (int i = 0; i < segmentCount; i++)
        {
            points.Add(pos);
            // basic kinematics with fake downward gravity (temp)
            vel += new Vector2(0f, fakeGravity) * timeStep; // v = v0 + at
            pos += vel * timeStep;                        // s = s0 + vt
        }

        lr.positionCount = points.Count;
        lr.SetPositions(points.ToArray());
    }

    /// <summary> Clear the line after release. </summary>

    public void Clear()
    {
        lr.positionCount = 0;
    }
    
    public void Bind(Rigidbody2D rb) 
    {spacecraftRb = rb;}
        
}
