using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public static class Noise
{
    public static float[,] GenerateNoiseMap(int resolution, float scale, int octaves, float persistance, float lacunarity, Vector2 offset)
    {
        float[,] noiseMap = new float[resolution, resolution];

        Vector2[] octaveOffsets = new Vector2[octaves];
        for (int i = 0; i < octaves; i++)
        {
            float offsetX = offset.x;
            float offsetY = offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        if (scale <= 0)
        {
            scale = 0.03f;
        }

        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        for (int y = 0; y < resolution; y++)
        {
            for(int x = 0; x < resolution; x++)
            {
                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = x / scale * frequency + octaveOffsets[i].x;
                    float sampleY = y / scale * frequency + octaveOffsets[i].y;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                if (noiseHeight > maxNoiseHeight)
                {
                    maxNoiseHeight = noiseHeight;
                }
                else if (noiseHeight < minNoiseHeight)
                {
                    minNoiseHeight = noiseHeight;
                }
                noiseMap[x, y] = noiseHeight;
            }
        }
        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
                //vertices[y].z = noiseMap[x, y];
            }
        }
        //mesh.vertices = vertices;
        return noiseMap;
    }

    public static Mesh Displace(Mesh meshObject, float[,] noiseMap, Texture2D noiseTexture)
    {
        int posX;
        int posY;

        foreach (Vector2 uvVertex in meshObject.uv)
        {
            posX = Mathf.CeilToInt(uvVertex.x);
            posY = Mathf.CeilToInt(uvVertex.y);

            for (int i = 0; i < meshObject.vertices.Length; i++)
            {
                meshObject.vertices[i] = new Vector3(meshObject.vertices[i].x, meshObject.vertices[i].y, noiseTexture.GetPixel(posX, posY).grayscale);
            }
        }

        return meshObject;
    }
}


