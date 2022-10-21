using System.Collections;
using System.Collections.Generic;
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
    public int resolution = 1024;
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
            UVPoints = new Vector3[Generator.points.Count];
            for (int x = 0; x < Generator.points.Count; x++)
            {
                UVPoints[x] = new Vector3(Generator.points[x].x, Generator.points[x].y, 0);
            }
            points = Generator.points.Select(point => new Vector2(point.x, point.y)).ToPoints().ToList();
			//Shows the count of generated points
            Debug.Log($"Generated Points Count {points.Count}");
			//Runs the triangulation function.
            Triangulate();
        }
    }

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

    private void Triangulate()
    {
        //Since a triangle can't have less than 3 vertices, it will not triangulate a mesh if it has less than 3 vertices.
        if (points.Count < 3) return;
        //Initiates a new delaunator instance.
        delaunator = new Delaunator(points.ToArray());
        CreateMesh();
        CreateTriangle();
    }

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
        meshFilter.mesh = mesh;

        UvCalculator.CalculateUVs(UVPoints, 1);

        texture = new Texture2D(resolution, resolution, TextureFormat.RGB24, true);
        texture.name = "Procedural Texture";
        meshObject.GetComponent<MeshRenderer>().material.mainTexture = texture;
        FillTexture();
    }
    private void FillTexture()
    {
        float stepSize = 1f / resolution;
        //Fills each pixel with a red colour.
        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                texture.SetPixel(x, y, new Color(x * stepSize, y * stepSize, 0f));
            }
        }
        //Applies the texture.
        texture.Apply();
    }
}
