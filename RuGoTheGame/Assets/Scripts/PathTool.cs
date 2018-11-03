﻿using System;
using UnityEngine;
using System.Collections.Generic;


public class PathTool : MonoBehaviour
{
    private const float yLevelTolerance = 0.001f;
    private const float gadgetDistance = 0.04f;

    private Action<Vector3[]> pathCompleteCallBack;
    private List<Vector3> drawingPath;
    private LineRenderer pathVisualizer;


    void Start()
    {
        EmptyPath();
        SetUpVisualizer();
        this.enabled = false;
    }


    void Update()
    {
        if (drawingPath.Count == 1 && pathVisualizer.enabled)
        {
            VisualizeStraightLine();
        }

        if (drawingPath.Count == 0 && RuGoInteraction.Instance.IsConfirmPressed)
        {
            StorePointPosition();
            VisualizeStartingPoint();
        }
        else if (drawingPath.Count != 0 && RuGoInteraction.Instance.IsConfirmHeld)
        {
            StorePointPosition();
            VisualizeNewPoint();
        }
        else if (drawingPath.Count > 1 && RuGoInteraction.Instance.IsConfirmReleased)
        {
            StorePointPosition();
            EqualizePointDistances();
            pathCompleteCallBack(drawingPath.ToArray());
            Deactivate();
        }
    }


    /// <summary>
    /// Stores mouse click positions in drawingPath vector.
    /// </summary>
    private void StorePointPosition()
    {
        Ray ray = RuGoInteraction.Instance.SelectorRay;
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (drawingPath.Count != 0)
            {
                Vector3 previousPoint = drawingPath[drawingPath.Count - 1];
                float previousY = previousPoint.y;
                float gap = Vector3.Distance(hit.point, previousPoint);

                if (Math.Abs(hit.point.y - previousY) < yLevelTolerance &&
                    gap >= gadgetDistance)
                {
                    drawingPath.Add(hit.point);
                }
            }
            else
            {
                drawingPath.Add(hit.point);
            }
        }
    }


    /// <summary>
    /// Equalizes the distance between each point in the drawing path.
    /// </summary>
    private void EqualizePointDistances()
    {
        List<Vector3> newPoints = new List<Vector3>();

        if (drawingPath.Count > 0)
        {
            newPoints.Add(drawingPath[0]);
            float leftover = 0f;

            Vector3 previousPoint = drawingPath[0];
            for (int i = 1; i < drawingPath.Count; i++)
            {
                float segmentLength = Vector3.Distance(drawingPath[i], drawingPath[i - 1]);
                Vector3 segmentVector = (drawingPath[i] - drawingPath[i - 1]) / segmentLength;

                if (leftover == 0)
                {
                    int count = (int)(segmentLength / gadgetDistance);
                    for (int n = 0; n < count; n++)
                    {
                        Vector3 newPoint = previousPoint + segmentVector * gadgetDistance;
                        newPoints.Add(newPoint);
                        previousPoint = newPoint;
                    }
                    leftover = segmentLength - count * gadgetDistance;
                }
                else if (Vector3.Distance(previousPoint, drawingPath[i]) > gadgetDistance)
                {
                    float angle_a = CalculateAngle(drawingPath[i - 1] - previousPoint, drawingPath[i] - drawingPath[i - 1]);
                    float side_b = Vector3.Distance(drawingPath[i - 1], previousPoint);
                    float side_c = CalculateSide(gadgetDistance, side_b, angle_a);
                    float remaining_segment = segmentLength - side_c;

                    Vector3 newPoint = drawingPath[i - 1] + side_c * segmentVector;
                    newPoints.Add(newPoint);
                    previousPoint = newPoint;

                    int count = (int)(remaining_segment / gadgetDistance);
                    for (int n = 0; n < count; n++)
                    {
                        newPoint = previousPoint + segmentVector * gadgetDistance;
                        newPoints.Add(newPoint);
                        previousPoint = newPoint;
                    }
                    leftover = remaining_segment - count * gadgetDistance;
                }
            }
        }
        drawingPath = newPoints;
    }


    /// <summary>
    /// Calculates the third side of the triangle using SSA method.
    /// </summary>
    /// <returns>The side.</returns>
    /// <param name="side_a">Side A of the triangle.</param>
    /// <param name="side_b">Side B of the triangle.</param>
    /// <param name="angle_a">Angle a of the triangle.</param>
    private float CalculateSide(float side_a, float side_b, float angle_a)
    {
        float sin_of_angle_a = Mathf.Sin(angle_a * Mathf.PI / 180);
        float angle_b = Mathf.Asin(side_b * sin_of_angle_a / side_a) * 180 / Mathf.PI;
        float angle_c = angle_a + angle_b;

        return Mathf.Sin(angle_c * Mathf.PI / 180) * side_a / sin_of_angle_a;
    }


    /// <summary>
    /// Calculates the angle between two vectors.
    /// </summary>
    /// <returns>The angle between two vectors.</returns>
    /// <param name="from">Vector 1.</param>
    /// <param name="to">Vector 2.</param>
    private float CalculateAngle(Vector3 from, Vector3 to)
    {
        return 180 - Vector3.Angle(from, to);
    }


    /// <summary>
    /// Sets up line renderer for path visualization.
    /// </summary>
    private void SetUpVisualizer()
    {
        pathVisualizer = gameObject.AddComponent<LineRenderer>();
        pathVisualizer.material = new Material(Shader.Find("Unlit/Texture"));
        pathVisualizer.startColor = Color.white;
        pathVisualizer.endColor = Color.white;
        pathVisualizer.startWidth = 0.02f;
        pathVisualizer.endWidth = 0.02f;
        pathVisualizer.enabled = false;
    }


    /// <summary>
    /// Marks the starting point of the Line Renderer.
    /// </summary>
    private void VisualizeStartingPoint()
    {
        pathVisualizer.positionCount = 2;
        pathVisualizer.SetPosition(0, drawingPath[0]);
        pathVisualizer.SetPosition(1, drawingPath[0]);
        pathVisualizer.enabled = true;
    }


    /// <summary>
    /// Visualizes the straight line from starting point to the mouse cursor.
    /// </summary>
    private void VisualizeStraightLine()
    {
        Ray ray = RuGoInteraction.Instance.SelectorRay;
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            pathVisualizer.SetPosition(1, hit.point);
        }
    }


    /// <summary>
    /// Visualizes a newly added point on the drawing path.
    /// </summary>
    private void VisualizeNewPoint()
    {
        pathVisualizer.positionCount += 1;
        pathVisualizer.SetPosition(pathVisualizer.positionCount - 1, drawingPath[drawingPath.Count - 1]);
    }


    /// <summary>
    /// Activates the Path Tool.
    /// </summary>
    /// <param name="createGadgetsAlongPath">The function to call back upon finishing drawing.</param>
    public void Activate(Action<Vector3[]> createGadgetsAlongPath)
    {
        pathCompleteCallBack = createGadgetsAlongPath;
        this.enabled = true;
    }


    /// <summary>
    /// Deactivates the Path Tool.
    /// </summary>
    public void Deactivate()
    {
        if (this.enabled)
        {
            this.enabled = false;
            ResetVisualizer();
            EmptyPath();
        }
    }


    /// <summary>
    /// Resets the visualizer.
    /// </summary>
    private void ResetVisualizer()
    {
        pathVisualizer.enabled = false;
        pathVisualizer.positionCount = 0;
    }


    /// <summary>
    /// Empties the drawing path buffer.
    /// </summary>
    private void EmptyPath()
    {
        drawingPath = new List<Vector3>();
    }
}