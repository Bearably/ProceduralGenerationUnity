using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generator : MonoBehaviour
{
    public float radius = 0.5f;
    public static float staticRadius;
    public Vector2 regionSize = new Vector2(10, 10);
    public int rejectionSamples = 100;

    public static List<Vector2> points;

    private void Update()
    {
        if (staticRadius != radius)
        {
            staticRadius = radius;
        }

        //If enter key is pressed, new points are generated.
        if (Input.GetKeyDown(KeyCode.Return))
        {
            GenerateNewPoints();
        }
    }
    /// <summary>
    /// Generates new points from the given radius, region size and rejection samples.
    /// </summary>
    private void GenerateNewPoints()
    {
        //Uses the generate points script in the PoissonDiscSampling script with the specified radius, regionSize and rejection samples.
        points = PoissonDiscSampling.GeneratePoints(radius, regionSize, rejectionSamples);
    }
    /// <summary>
    /// Clamps the values from the editor to reduce errors.
    /// </summary>
    void checkValues()
    {
        //Clamps the values to reduce errors.
        if (radius < 0.05f) radius = 0.05f;
        if (radius > regionSize.x) radius = Mathf.Min(radius, regionSize.x);
        if (radius > regionSize.y) radius = Mathf.Min(radius, regionSize.y);
        if (regionSize.x < 0.5)
        {
            radius = 0.05f;
            regionSize.x = 0.5f;
        }
        if (regionSize.y < 0.5)
        {
            radius = 0.05f;
            regionSize.y = 0.5f;
        }
        if (rejectionSamples < 1) rejectionSamples = 1;


    }

    void OnValidate()
    {
        //Constantly checks the values to make sure they are not below the threshold.
        checkValues();
    }
}
