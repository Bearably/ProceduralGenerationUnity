using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DelaunatorSharp;
using System.Linq;
using DelaunatorSharp.Unity.Extensions;
using System;

public class DelaunayTriangulation : MonoBehaviour
{
    private Delaunator delaunator;
    private List<IPoint> points = new List<IPoint>();
    private GameObject meshObject;

    private float radius;
    private int numSamplesBeforeRejection;
    private Vector2 regionSize;

    [SerializeField] bool drawTrianglePoints = true;
    [SerializeField] bool drawTriangleEdges = true;
    [SerializeField] bool drawVoronoiPoints = true;
    [SerializeField] bool drawVoronoiEdges = true;
    [SerializeField] bool drawHull = true;
    [SerializeField] bool createMesh = true;

    [SerializeField] float voronoiEdgeWidth = 0.01f;
    [SerializeField] float triangleEdgeWidth = 0.015f;

    [SerializeField] Material meshMaterial;
    [SerializeField] Material lineMaterial;

    private Transform VoronoiContainer;
    private Transform TrianglesContainer;

    [SerializeField] Color triangleEdgeColor = Color.black;
    [SerializeField] Color hullColor = Color.magenta;
    [SerializeField] Color voronoiColor = Color.white;

    [SerializeField] GameObject trianglePointPrefab;
    [SerializeField] GameObject voronoiPointPrefab;

    private Transform PointsContainer;

    private void Start()
    {
        Clear();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            points = Generator.points.Select(point => new Vector2(point.x, point.y)).ToPoints().ToList();
            Debug.Log($"Generated Points Count {points.Count}");
            Triangulate();
        }

        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            Clear();
        }
    }

    private void Clear()
    {
        CreateNewPoints();
        CreateNewTrianglesContainer();

        if (meshObject != null)
        {
            Destroy(meshObject);
        }

        delaunator = null;
    }

    private void CreateNewPoints()
    {
        if (PointsContainer != null)
        {
            Destroy(PointsContainer.gameObject);
        }

        PointsContainer = new GameObject(nameof(PointsContainer)).transform;
    }

    private void CreateNewTrianglesContainer()
    {
        if (TrianglesContainer != null)
        {
            Destroy(TrianglesContainer.gameObject);
        }

        TrianglesContainer = new GameObject(nameof(TrianglesContainer)).transform;
    }

    private void Triangulate()
    {
        if (points.Count < 3) return;

        delaunator = new Delaunator(points.ToArray());
        CreateTriangle();
        CreateMesh();
    }

    private void CreateTriangle()
    {
        if (delaunator == null) return;

        delaunator.ForEachTriangleEdge(edge =>
        {
            if (drawTriangleEdges)
            {
                CreateLine(TrianglesContainer, $"TriangleEdge - {edge.Index}", new Vector3[] { edge.P.ToVector3(), edge.Q.ToVector3() }, triangleEdgeColor, triangleEdgeWidth, 0);
            }

            if (drawTrianglePoints)
            {
                var pointGameObject = Instantiate(trianglePointPrefab, PointsContainer);
                pointGameObject.transform.SetPositionAndRotation(edge.P.ToVector3(), Quaternion.identity);
            }
        });
    }

    private void CreateLine(Transform container, string name, Vector3[] points, Color color, float width, int order = 1)
    {
        var lineGameObject = new GameObject(name);
        lineGameObject.transform.parent = container;
        var lineRenderer = lineGameObject.AddComponent<LineRenderer>();

        lineRenderer.SetPositions(points);

        lineRenderer.material = lineMaterial ?? new Material(Shader.Find("Standard"));
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width;
        lineRenderer.sortingOrder = order;
    }

    private void CreateMesh()
    {
        if (!createMesh) return;

        if (meshObject != null)
        {
            Destroy(meshObject);
        }

        var mesh = new Mesh
        {
            vertices = delaunator.Points.ToVectors3(),
            triangles = delaunator.Triangles
        };

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        meshObject = new GameObject("TriangleMesh");
        var meshRenderer = meshObject.AddComponent<MeshRenderer>();
        meshRenderer.GetComponent<Renderer>().material = meshMaterial;
        var meshFilter = meshObject.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;
    }
}
