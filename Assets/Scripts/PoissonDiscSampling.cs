using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PoissonDiscSampling
{
    public static List<Vector2> GeneratePoints(float radius, Vector2 sampleRegionSize, int numSamplesBeforeRejection = 30)
    {
        //Defines the cell radius using Pythagoras' Theorem.
        float cellSize = radius / Mathf.Sqrt(2);
        //Generates a 2D grid array to store the positions of vertices.
        int[,] grid = new int[Mathf.CeilToInt(sampleRegionSize.x / cellSize), Mathf.CeilToInt(sampleRegionSize.y / cellSize)];
        //Creates a list of Vector2 coordinates for the points and the spawnpoints.
        List<Vector2> points = new List<Vector2>();
        List<Vector2> spawnPoints = new List<Vector2>();
        //Adds vertex spawn points.
        spawnPoints.Add(sampleRegionSize / 2);
        
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

    static void addVertex(Vector2 vertex, float cellSize, List<Vector2> points, List<Vector2> spawnPoints, int[,] grid)
    {
        //Adds the vertex to the points array, adds the vertex as a spawnpoint and adds an index for the vertex with its position.
        points.Add(vertex);
        spawnPoints.Add(vertex);
        grid[(int)(vertex.x / cellSize), (int)(vertex.y / cellSize)] = points.Count;
    }
}
