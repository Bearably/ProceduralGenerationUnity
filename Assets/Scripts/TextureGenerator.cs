using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using DelaunatorSharp;
using System.Linq;
using DelaunatorSharp.Unity.Extensions;
using System;

public static class TextureGenerator
{
    public static Texture2D TextureFromColourMap(Color[] colourMap, int resolution)
    {
        Texture2D texture = new Texture2D(resolution, resolution);
        texture.filterMode = FilterMode.Point;
        texture.SetPixels(colourMap);
        texture.Apply();
        return texture;
    }

    public static Texture2D TextureFromNoiseMap(float[,] noiseMap, int resolution)
    {
        Color[] colourMap = new Color[resolution * resolution];
        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                colourMap[y * resolution + x] = Color.Lerp(Color.black, Color.white, noiseMap[x, y]);
            }
        }
        return TextureFromColourMap(colourMap, resolution);
    }
}
