using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using DelaunatorSharp;
using System.Linq;
using DelaunatorSharp.Unity.Extensions;
using System;

public class DelaunayTriangulation : MonoBehaviour
{
    private Delaunator delaunator;
    private List<IPoint> points = new List<IPoint>();
    private Vector3[] UVPoints;
    private GameObject meshObject;
    public int resolution = 2048;
    private Texture2D texture;
    [SerializeField] bool drawTrianglePoints = true;
    [SerializeField] bool drawTriangleEdges = true;
    [SerializeField] bool createMesh = true;
    [SerializeField] float triangleEdgeWidth = 0.015f;
    [SerializeField] Material meshMaterial;
    [SerializeField] Material lineMaterial;
    private Transform TrianglesContainer;
    [SerializeField] Color triangleEdgeColor = Color.black;
    private Transform PointsContainer;
    public float scale = 20f;
    public int octaves = 1;
    public Vector2 offset;
    [Range(0,1)]
    public float persistance;
    public float lacunarity;
    private float sample;
    

    private void Start()
    {
        //Makes sure the scene is clear on start-up.
        Clear();
    }

    private void Update()
    {
        //Checks if the enter key has been pressed
        if (Input.GetKeyDown(KeyCode.Return))
        {
            //Calls the Clear function to make sure the scene is clear
            Clear();
            //Generates new points using the generator script and adds them to the Vector2 array "points"
            points = Generator.points.Select(point => new Vector2(point.x, point.y)).ToPoints().ToList();
            //Shows the count of generated points
            Debug.Log($"Generated Points Count {points.Count}");
            //Initialises a Vector3 array of UV vertices for a UV map.
            UVPoints = new Vector3[points.Count];
            //Runs through each vertex position in the points array and adds its position to the UV map.
            for (int x = 0; x < points.Count; x++)
            {
                UVPoints[x] = new Vector3((float)points[x].X, (float)points[x].Y, 0);
            }
            //Runs the triangulation function.
            Triangulate();
        }
    }
    /// <summary>
    /// Clears the scene, generates new points and edges and initiates a new Delaunator instance.
    /// </summary>
    private void Clear()
    {
        //Generates new points and edges.
        CreateNewPoints();
        CreateNewTrianglesContainer();

        if (meshObject != null)
        {
            //If there is a mesh in the mesh object gameobject it destroys it.
            Destroy(meshObject);
        }
        //Initiates a new delaunator instance.
        delaunator = null;
    }
    /// <summary>
    /// Creates a new point container.
    /// </summary>
    private void CreateNewPoints()
    {
        if (PointsContainer != null)
        {
            //If there are vertices contained in the points container it destroys the game object.
            Destroy(PointsContainer.gameObject);
        }
        //Initiates a new vertex container.
        PointsContainer = new GameObject(nameof(PointsContainer)).transform;
    }
    /// <summary>
    /// Creates a new triangles container to store the drawn edges.
    /// </summary>
    private void CreateNewTrianglesContainer()
    {
        if (TrianglesContainer != null)
        {
            //If the edge container is not empty, it destroys it.
            Destroy(TrianglesContainer.gameObject);
        }
        //Initiates a new edge container.
        TrianglesContainer = new GameObject(nameof(TrianglesContainer)).transform;
    }
    /// <summary>
    /// Creates a triangulated mesh from the generated points.
    /// </summary>
    private void Triangulate()
    {
        //Since a triangle can't have less than 3 vertices, it will not triangulate a mesh if it has less than 3 vertices.
        if (points.Count < 3) return;
        //Initiates a new delaunator instance.
        delaunator = new Delaunator(points.ToArray());
        CreateMesh();
        CreateTriangle();
    }
    /// <summary>
    /// Creates the triangles in the mesh to be drawn.
    /// </summary>
    private void CreateTriangle()
    {
        // It will not draw edges if there a Delaunator instace was not initialised.
        if (delaunator == null) return;

        delaunator.ForEachTriangleEdge(edge =>
        {
            if (drawTriangleEdges)
            {
                //Draws a line for each triangle edge within the mesh with a specified colour and width.
                CreateLine(TrianglesContainer, $"TriangleEdge {edge.Index}", new Vector3[] { edge.P.ToVector3(), edge.Q.ToVector3() }, triangleEdgeColor, triangleEdgeWidth, 0);
            }
        });
    }
    /// <summary>
    /// Draws each edge in a mesh from a container of triangles.
    /// </summary>
    /// <param name="container">The Transform containing the triangles</param>
    /// <param name="name">Name for the edges</param>
    /// <param name="points">The array of vertices</param>
    /// <param name="color">The colour for the edges</param>
    /// <param name="width">The width of each edge's line</param>
    /// <param name="order">The sorting order for Unity's line renderer</param>
    private void CreateLine(Transform container, string name, Vector3[] points, Color color, float width, int order = 1)
    {
        //Initialiases a new game object to store the drawn line in. Adds a line renderer so it draws the line, and adds it to the edge container.
        var lineGameObject = new GameObject(name);
        lineGameObject.transform.parent = container;
        var lineRenderer = lineGameObject.AddComponent<LineRenderer>();

        lineRenderer.SetPositions(points);

        //Sets the material and colours for the edges.
        lineRenderer.material = lineMaterial ?? new Material(Shader.Find("Standard"));
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width;
        lineRenderer.sortingOrder = order;
    }
    /// <summary>
    /// Creates a mesh with generated Delaunator points
    /// </summary>
    private void CreateMesh()
    {
        //Checks if a mesh can be made
        if (!createMesh) return;

        //Destroys the mesh object if it is not empty.
        if (meshObject != null)
        {
            Destroy(meshObject);
        }

        //Creates a new mesh with the generated points and edges.
        var mesh = new Mesh
        {
            vertices = delaunator.Points.ToVectors3(),
            triangles = delaunator.Triangles
        };

        //Recalculates the normals and bounds of the mesh (to ensure lighting is correct)
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        //Creates a new gameobject to add the generated mesh to, and gives it a perlin noise material for displacement.
        meshObject = new GameObject("TriangleMesh");
        var meshRenderer = meshObject.AddComponent<MeshRenderer>();
        var meshFilter = meshObject.AddComponent<MeshFilter>();
        meshObject.GetComponent<MeshRenderer>().material = meshMaterial;
        meshFilter.mesh = mesh;
        //Calculates mesh UVs to apply the perlin noise map with a scale of 500 (smaller values mean a larger UV map).
        mesh.uv = UvCalculator.CalculateUVs(UVPoints, 50);
        Texture2D NoiseTexture = new Texture2D(resolution, resolution);
        MapNoise(NoiseTexture, Noise.GenerateNoiseMap(resolution, scale, octaves, persistance, lacunarity, offset));
    }

    private void MapNoise(Texture2D texture, float[,] noiseMap)
    {
        Color[] colourMap = new Color[resolution * resolution];
        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                colourMap[y * resolution + x] = Color.Lerp(Color.black, Color.white, noiseMap[x, y]);
            }
        }
        texture.SetPixels(colourMap);
        texture.Apply();

        meshObject.GetComponent<Renderer>().material.mainTexture = texture;
    }

    private void CheckValues()
    {
        if (resolution < 1)
        {
            resolution = 1;
        }

        if (lacunarity < 1)
        {
            lacunarity = 1;
        }

        if (octaves < 0)
        {
            octaves = 0;
        }
    }
}

    //private Texture2D GenerateNoise(float freq)
    //{
    //    texture = new Texture2D(resolution, resolution);
    //    texture.name = "Procedural Texture";

    //    //Generates a Perlin Noise map for the texture
    //    for (int x = 0; x < resolution; x++)
    //    {
    //        for (int y = 0; y < resolution; y++)
    //        {
    //            float amplitude = 1;
    //            float noiseHeight = 0;
    //            Color color = CalculateColour(x, y, noiseHeight, amplitude, freq);
    //            texture.SetPixel(x, y, color);
    //        }
    //    }

    //    texture.Apply();
    //    return texture;
    //}

//    private Color CalculateColour(int x, int y, float noiseHeight, float amplitude, float freq)
//    {
//        float maxNoiseHeight = float.MaxValue;
//        float minNoiseHeight = float.MinValue;
//        for (int i = 0; i < octaves; i++)
//        {
//            float xCoord = (float)x / resolution / scale * freq;
//            float yCoord = (float)y / resolution / scale * freq;

//            sample = Mathf.PerlinNoise(xCoord, yCoord) * 2 - 1;
//            noiseHeight += sample * amplitude;
//            amplitude *= persistance;
//            freq *= lacunarity;
//        }

//        if (noiseHeight > maxNoiseHeight)
//        {
//            maxNoiseHeight = noiseHeight;
//        }
//        else if (noiseHeight < minNoiseHeight)
//        {
//            minNoiseHeight = noiseHeight;
//        }

//        return new Color(sample, sample, sample);
//    }
//}
