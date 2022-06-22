using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PoissonDiscSampling
{
    public static List<Vector2> GeneratePoints(float radius, Vector2 sampleRegionSize, int numSamplesBeforeRejection = 30)
    {

        float borderSizeX = sampleRegionSize.x / radius;
        float borderSizeY = sampleRegionSize.y / radius;
        float borderIncY = borderSizeY / Mathf.FloorToInt(borderSizeY);
        float borderIncX = borderSizeX / Mathf.FloorToInt(borderSizeX);
        float cellSize = radius / Mathf.Sqrt(2);
        int[,] grid = new int[Mathf.CeilToInt(sampleRegionSize.x / cellSize), Mathf.CeilToInt(sampleRegionSize.y / cellSize)];
        List<Vector2> points = new List<Vector2>();
        List<Vector2> spawnPoints = new List<Vector2>();
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

        while (spawnPoints.Count > 0)
        {
            int spawnIndex = Random.Range(0, spawnPoints.Count);
            Vector2 spawnCentre = spawnPoints[spawnIndex];
            bool candidateAccepted = false;
            for (int i = 0; i < numSamplesBeforeRejection; i++)
            {
                float angle = Random.value * Mathf.PI * 2;
                Vector2 direction = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
                Vector2 candidate = spawnCentre + direction * Random.Range(radius, 2 * radius);
                if (isValid(candidate, sampleRegionSize, cellSize, radius, points, grid))
                {
                    points.Add(candidate);
                    spawnPoints.Add(candidate);
                    grid[(int)(candidate.x / cellSize), (int)(candidate.y / cellSize)] = points.Count;
                    candidateAccepted = true;
                    break;
                }
            }
            if (!candidateAccepted)
            {
                spawnPoints.RemoveAt(spawnIndex);
            }
        }
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
}
