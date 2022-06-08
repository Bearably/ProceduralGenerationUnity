using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generator : MonoBehaviour
{
    [SerializeField] public float radius = 0.8f;
    [SerializeField] public Vector2 regionSize = new Vector2(8, 10);
    [SerializeField] public int rejectionSamples = 30;
    [SerializeField] public float displayRadius = 0.03f;

    public static List<Vector2> points;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            GenerateNewPoints();
        }
    }

    private void GenerateNewPoints()
    {
        points = PoissonDiscSampling.GeneratePoints(radius, regionSize, rejectionSamples);
    }

    void checkRadius()
    {
        if (radius < 0.1) radius = Mathf.Max(radius, 0.1f);
    }

    void OnValidate()
    {
        checkRadius();
        GenerateNewPoints();
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(regionSize / 2, regionSize);
        if (points != null)
        {
            foreach (Vector2 point in points)
            {
                Gizmos.DrawSphere(point, displayRadius);
            }
        }
    }
}
