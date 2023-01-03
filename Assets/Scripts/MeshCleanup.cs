using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshCleanup
{
    public static void Cleanup(Mesh mesh, Vector2 regionSize, Vector2 cutoffRegion)
    {
        int[] triangles = mesh.triangles;
        Vector3[] vertices = mesh.vertices;
        Vector2 r1 = new Vector2(regionSize.x - cutoffRegion.x, regionSize.y - cutoffRegion.y);
        Vector2 r2 = new Vector2(regionSize.x, regionSize.y - cutoffRegion.y);
        Vector2 r3 = new Vector2(regionSize.x - cutoffRegion.x, regionSize.y);
        Vector2 r4 = new Vector2(cutoffRegion.x, cutoffRegion.y);
        for (int i = 0; i < triangles.Length; i = i + 3)
        {
            // Check if the triangle and rectangle intersect
            if (IsTriangleRectangleIntersecting(vertices[i], vertices[i + 1], vertices[i + 2], r1, r2, r3, r4))
            {
                //Triangle and rectangle are intersecting
                Debug.Log(triangles[i/3] + "is Intersecting");
            }
            else
            {
                Debug.Log(triangles[i/3] + " is Not Intersecting");
                //Triangle and rectangle are not intersecting
            }
        }

    }

    static bool IsTriangleRectangleIntersecting(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 r1, Vector2 r2, Vector2 r3, Vector2 r4)
    {
        // Check if any of the triangle vertices are inside the rectangle
        if (IsPointInsideRectangle(p1, r1, r2, r3, r4)) return true;
        if (IsPointInsideRectangle(p2, r1, r2, r3, r4)) return true;
        if (IsPointInsideRectangle(p3, r1, r2, r3, r4)) return true;

        // Check if any of the rectangle vertices are inside the triangle
        if (IsPointInsideTriangle(r1, p1, p2, p3)) return true;
        if (IsPointInsideTriangle(r2, p1, p2, p3)) return true;
        if (IsPointInsideTriangle(r3, p1, p2, p3)) return true;
        if (IsPointInsideTriangle(r4, p1, p2, p3)) return true;

        // Check if any of the sides of the rectangle intersect with the triangle
        if (IsLineSegmentTriangleIntersecting(r1, r2, p1, p2, p3)) return true;
        if (IsLineSegmentTriangleIntersecting(r2, r3, p1, p2, p3)) return true;
        if (IsLineSegmentTriangleIntersecting(r3, r4, p1, p2, p3)) return true;
        if (IsLineSegmentTriangleIntersecting(r4, r1, p1, p2, p3)) return true;

        // Check if any of the sides of the triangle intersect with the rectangle
        if (IsLineSegmentRectangleIntersecting(p1, p2, r1, r2, r3, r4)) return true;
        if (IsLineSegmentRectangleIntersecting(p2, p3, r1, r2, r3, r4)) return true;
        if (IsLineSegmentRectangleIntersecting(p3, p1, r1, r2, r3, r4)) return true;

        return false;
    }

    static bool IsPointInsideRectangle(Vector2 point, Vector2 r1, Vector2 r2, Vector2 r3, Vector2 r4)
    {
        // Check if the point is inside the rectangle by checking if it's within the x and y range of the rectangle
        if (point.x >= Mathf.Min(r1.x, Mathf.Min(r2.x, Mathf.Min(r3.x, r4.x))) &&
            point.x <= Mathf.Max(r1.x, Mathf.Max(r2.x, Mathf.Max(r3.x, r4.x))) &&
            point.y >= Mathf.Min(r1.y, Mathf.Min(r2.y, Mathf.Min(r3.y, r4.y))) &&
            point.y <= Mathf.Max(r1.y, Mathf.Max(r2.y, Mathf.Max(r3.y, r4.y))))
        {
            return true;
        }
        return false;
    }
    static bool IsLineSegmentTriangleIntersecting(Vector2 p1, Vector2 p2, Vector2 t1, Vector2 t2, Vector2 t3)
    {
        // Check if the line segment and triangle intersect by checking if the line segment intersects with any of the triangle sides
        if (IsLineSegmentIntersecting(p1, p2, t1, t2)) return true;
        if (IsLineSegmentIntersecting(p1, p2, t2, t3)) return true;
        if (IsLineSegmentIntersecting(p1, p2, t3, t1)) return true;

        return false;
    }
    static bool IsLineSegmentIntersecting(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
    {
        // Check if the line segments intersect by using the line segment intersection formula
        float denominator = (p4.y - p3.y) * (p2.x - p1.x) - (p4.x - p3.x) * (p2.y - p1.y);
        float ua = (p4.x - p3.x) * (p1.y - p3.y) - (p4.y - p3.y) * (p1.x - p3.x);
        float ub = (p2.x - p1.x) * (p1.y - p3.y) - (p2.y - p1.y) * (p1.x - p3.x);

        if (denominator == 0)
        {
            // The line segments are parallel
            if (ua == 0 && ub == 0)
            {
                // The line segments are collinear
                return true;
            }
            return false;
        }

        // The line segments are not parallel
        ua /= denominator;
        ub /= denominator;

        if (ua >= 0 && ua <= 1 && ub >= 0 && ub <= 1)
        {
            // The line segments intersect
            return true;
        }
        return false;
    }

    static bool IsLineSegmentRectangleIntersecting(Vector2 p1, Vector2 p2, Vector2 r1, Vector2 r2, Vector2 r3, Vector2 r4)
    {
        // Check if the line segment and rectangle intersect by checking if the line segment intersects with any of the rectangle sides
        if (IsLineSegmentIntersecting(p1, p2, r1, r2)) return true;
        if (IsLineSegmentIntersecting(p1, p2, r2, r3)) return true;
        if (IsLineSegmentIntersecting(p1, p2, r3, r4)) return true;
        if (IsLineSegmentIntersecting(p1, p2, r4, r1)) return true;

        return false;
    }

    static bool IsPointInsideTriangle(Vector2 point, Vector2 t1, Vector2 t2, Vector2 t3)
    {
        // Check if the point is inside the triangle by using barycentric coordinates
        float denominator = ((t2.y - t3.y) * (t1.x - t3.x) + (t3.x - t2.x) * (t1.y - t3.y));
        float a = ((t2.y - t3.y) * (point.x - t3.x) + (t3.x - t2.x) * (point.y - t3.y)) / denominator;
        float b = ((t3.y - t1.y) * (point.x - t3.x) + (t1.x - t3.x) * (point.y - t3.y)) / denominator;
        float c = 1 - a - b;

        if (a >= 0 && a <= 1 && b >= 0 && b <= 1 && c >= 0 && c <= 1)
        {
            // The point is inside the triangle
            return true;
        }
        return false;
    }

}
