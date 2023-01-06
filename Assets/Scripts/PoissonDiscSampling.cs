using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PoissonDiscSampling
{
    /// <summary>
    /// Generates new vertices using Poisson Disc Sampling.
    /// </summary>
    /// <param name="radius">The minimum distance between each vertex</param>
    /// <param name="sampleRegionSize">The region vertices can be sampled within</param>
    /// <param name="numSamplesBeforeRejection">The amount of times a vertex position is sampled</param>
    /// <returns>Returns a list of Vector2 positions of sampled vertices.</returns>
    public static List<Vector2> GeneratePoints(float radius, Vector2 sampleRegionSize, int numSamplesBeforeRejection = 30)
    {
        //Defines the cell radius using Pythagoras' Theorem.
        float cellSize = radius / Mathf.Sqrt(2);
        float borderSizeX = sampleRegionSize.x / radius;
        float borderSizeY = sampleRegionSize.y / radius;
        float borderIncY = borderSizeY / Mathf.FloorToInt(borderSizeY);
        float borderIncX = borderSizeX / Mathf.FloorToInt(borderSizeX);
        //Generates a 2D grid array to store the positions of vertices.
        int[,] grid = new int[Mathf.CeilToInt(sampleRegionSize.x / cellSize), Mathf.CeilToInt(sampleRegionSize.y / cellSize)];
        //Creates a list of Vector2 coordinates for the points and the spawnpoints.
        List<Vector2> points = new List<Vector2>();
        List<Vector2> spawnPoints = new List<Vector2>();
        //Adds vertex spawn points.
        spawnPoints.Add(sampleRegionSize / 2);

        Vector2 borderPoint1 = new Vector2(0, 0);
        Vector2 borderPoint2 = new Vector2(0, sampleRegionSize.y);
        Vector2 borderPoint3 = new Vector2(sampleRegionSize.x, 0);
        Vector2 borderPoint4 = new Vector2(sampleRegionSize.x, sampleRegionSize.y);
        //Debug.Log(borderSizeX);
        //Debug.Log(borderSizeY);
        Vector2 subdivisionBottom = new Vector2(borderPoint1.x + borderIncX / (borderSizeX / sampleRegionSize.x), borderPoint1.y);
        Vector2 subdivisionBottom2 = new Vector2(borderPoint2.x + borderIncX / (borderSizeX / sampleRegionSize.x), borderPoint2.y);
        points.Add(subdivisionBottom);
        spawnPoints.Add(subdivisionBottom);
        grid[(int)(subdivisionBottom.x / cellSize), (int)(subdivisionBottom.y / cellSize)] = points.Count;
        points.Add(subdivisionBottom2);
        spawnPoints.Add(subdivisionBottom);
        grid[(int)(subdivisionBottom2.x / cellSize), (int)(subdivisionBottom2.y / cellSize)] = points.Count;
        Vector2 subdivisionTop = new Vector2(0, borderPoint3.y + borderIncY / (borderSizeY / sampleRegionSize.y));
        Vector2 subdivisionTop2 = new Vector2(borderPoint4.x, borderPoint4.y - borderIncY / (borderSizeY / sampleRegionSize.y));
        points.Add(subdivisionTop);
        spawnPoints.Add(subdivisionTop);
        grid[(int)(subdivisionTop.x / cellSize), (int)(subdivisionTop.y / cellSize)] = points.Count;
        points.Add(subdivisionTop2);
        spawnPoints.Add(subdivisionTop2);
        grid[(int)(subdivisionTop2.x / cellSize), (int)(subdivisionTop2.y / cellSize)] = points.Count;


        for (float subdivisionCountX = 0; subdivisionCountX < borderSizeX - borderIncX; subdivisionCountX += borderIncX)
        {
            subdivisionBottom = new Vector2(subdivisionBottom.x + borderIncX / (borderSizeX / sampleRegionSize.x), subdivisionBottom.y);
            subdivisionBottom2 = new Vector2(subdivisionBottom2.x + borderIncX / (borderSizeX / sampleRegionSize.x), subdivisionBottom2.y);
            points.Add(subdivisionBottom);
            spawnPoints.Add(subdivisionBottom);
            grid[(int)(subdivisionBottom.x / cellSize), (int)(subdivisionBottom.y / cellSize)] = points.Count;
            points.Add(subdivisionBottom2);
            spawnPoints.Add(subdivisionBottom2);
            grid[(int)(subdivisionBottom2.x / cellSize), (int)(subdivisionBottom2.y / cellSize)] = points.Count;
        }

        for (float subdivisionCountY = 0; subdivisionCountY < borderSizeY - borderIncY; subdivisionCountY += borderIncY)
        {
            subdivisionTop = new Vector2(0, subdivisionTop.y + borderIncY / (borderSizeY / sampleRegionSize.y));
            subdivisionTop2 = new Vector2(subdivisionTop2.x, subdivisionTop2.y - borderIncY / (borderSizeY / sampleRegionSize.y));
            points.Add(subdivisionTop);
            spawnPoints.Add(subdivisionTop);
            grid[(int)(subdivisionTop.x / cellSize), (int)(subdivisionTop.y / cellSize)] = points.Count;
            points.Add(subdivisionTop2);
            spawnPoints.Add(subdivisionTop2);
            grid[(int)(subdivisionTop2.x / cellSize), (int)(subdivisionTop2.y / cellSize)] = points.Count;
        }

        points.Add(borderPoint1);
        points.Add(borderPoint2);
        points.Add(borderPoint3);
        points.Add(borderPoint4);
        spawnPoints.Add(borderPoint1);
        spawnPoints.Add(borderPoint2);
        spawnPoints.Add(borderPoint3);
        spawnPoints.Add(borderPoint4);

        //Adds a loop that runs until there are no more spawnpoints left.
        while (spawnPoints.Count > 0)
        {
            //Generates a random vertex position to test if a vertex can be spawned there
            int spawnIndex = Random.Range(0, spawnPoints.Count);
            Vector2 spawnCentre = spawnPoints[spawnIndex];
            bool candidateAccepted = false;
            //Runs a loop that samples as many times as specified, and if no results are found then it removes the spawnpoint.
            for (int i = 0; i < numSamplesBeforeRejection; i++)
            {
                float angle = Random.value * Mathf.PI * 2;
                Vector2 direction = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
                Vector2 candidate = spawnCentre + direction * Random.Range(radius, 2 * radius);
                if (isValid(candidate, sampleRegionSize, cellSize, radius, points, grid))
                {
                    //If the vertex position is a valid position, it adds that vertex.
                    addVertex(candidate, cellSize, points, spawnPoints, grid);
                    candidateAccepted = true;
                    break;
                }
            }
            if (!candidateAccepted)
            {
                //If the candidate wasn't accepted, the spawn point doesn't work, so it is removed.
                spawnPoints.RemoveAt(spawnIndex);
            }
        }
        //Returns the array of Vector2 coordinates of vertices.
        return points;
    }
    /// <summary>
    /// Checks if a vertex position meets a specific criteria.
    /// </summary>
    /// <param name="candidate">The vertex position being checked</param>
    /// <param name="sampleRegionSize">The region size the vertices are sampled within</param>
    /// <param name="cellSize">The cell radius defined through Pythagoras' Theorem</param>
    /// <param name="radius">The minimum distance between each vertex</param>
    /// <param name="points">The list of Vector2 positions for sampled vertices</param>
    /// <param name="grid">A 2D grid array storing the positions of vertices</param>
    /// <returns>Returns a boolean value stating if a vertex is an acceptable candidate.</returns>
    static bool isValid(Vector2 candidate, Vector2 sampleRegionSize, float cellSize, float radius, List<Vector2> points, int[,] grid)
    {
        if (candidate.x >= 0 && candidate.x < sampleRegionSize.x && candidate.y >= 0 && candidate.y < sampleRegionSize.y)
        {
            int cellX = (int)(candidate.x / cellSize);
            int cellY = (int)(candidate.y / cellSize);
            int searchStartX = Mathf.Max(0, cellX - 2);
            int searchEndX = Mathf.Min(cellX + 2, grid.GetLength(0) - 1);
            int searchStartY = Mathf.Max(0, cellY - 2);
            int searchEndY = Mathf.Min(cellY + 2, grid.GetLength(1) - 1);

            for (int x = searchStartX; x <= searchEndX; x++)
            {
                for (int y = searchStartY; y <= searchEndY; y++)
                {
                    int pointIndex = grid[x, y] - 1;
                    if (pointIndex != -1)
                    {
                        float squareDistance = (candidate - points[pointIndex]).sqrMagnitude;
                        if (squareDistance < radius * radius)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }
        return false;
    }
    /// <summary>
    /// Adds a vertex to the list of Vector2 positions of sampled vertices, list of spawn points and adds an index for a vertex along with its position.
    /// </summary>
    /// <param name="vertex">A vertex position desired to be added</param>
    /// <param name="cellSize">The cell radius defined through Pythagoras' Theorem</param>
    /// <param name="points">The list of Vector2 positions of sampled vertices</param>
    /// <param name="spawnPoints">The list of spawn points used by vertices so no other vertex will spawn there</param>
    /// <param name="grid">2D grid array storing the positions of vertices</param>
    static void addVertex(Vector2 vertex, float cellSize, List<Vector2> points, List<Vector2> spawnPoints, int[,] grid)
    {
        //Adds the vertex to the points array, adds the vertex as a spawnpoint and adds an index for the vertex with its position.
        points.Add(vertex);
        spawnPoints.Add(vertex);
        grid[(int)(vertex.x / cellSize), (int)(vertex.y / cellSize)] = points.Count;
    }
}
