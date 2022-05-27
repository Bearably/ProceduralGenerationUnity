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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            points = Generator.points.Select(point => new Vector2(point.x, point.y)).ToPoints().ToList();
            Debug.Log($"Generated Points Count {points.Count}");
            Triangulate();
        }
    }

    private void Triangulate()
    {
        delaunator = new Delaunator(points.ToArray());
        CreateTriangle();
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
}
