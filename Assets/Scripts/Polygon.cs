
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Polygon : MonoBehaviour
{
    bool initialized = false;
    float intAngle;
    float intComplement;
    float edgeLength;
    const float fullAngle = 360;
    int numPoints;
    readonly Vector2 refAngle = new(1, 0);

    // points start from the base and move counter-clockwise
    public Vector2[] points { get; private set; }
    public MidPoint[] midPoints { get; private set; }

    LineRenderer lineRenderer;

    private void Initialize(int vertices, Vector2 p0, Vector2 p1)
    {
        if (initialized) return;

        numPoints = vertices;
        intAngle = (numPoints - 2) * 180f / numPoints;
        intComplement = 180 - intAngle;
        edgeLength = Vector2.Distance(p0, p1);

        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.loop = true;
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.startColor = Color.black;
        lineRenderer.endColor = Color.black;
        lineRenderer.positionCount = numPoints;

        initialized = true;
    }

    public void ComputeAndDraw(int vertices, Vector2 p0, Vector2 p1)
    {
        Initialize(vertices, p0, p1);
        var angles = new float[numPoints];
        points = new Vector2[numPoints];
        midPoints = new MidPoint[numPoints];

        angles[0] = UnsignedAngle(Vector2.SignedAngle(refAngle, p1 - p0));
        for (var i = 1; i < angles.Length; i++)
        {
            angles[i] = Rotate(angles[i - 1]);
        }

        points[0] = p0;
        midPoints[0] = MidPoint(p0, angles[0]);
        for (var i = 1; i < points.Length; i++)
        {
            points[i] = NextPoint(points[i - 1], angles[i - 1]);
            midPoints[i] = MidPoint(points[i], angles[i]);
        }

        Draw();
    }

    public void SetColour(Color color)
    {
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
    }

    float Rotate(float angle)
    {
        return UnsignedAngle(angle + intComplement);
    }

    float UnsignedAngle(float angle)
    {
        angle %= fullAngle;
        if (angle < 0)
        {
            angle += fullAngle;
        }
        return angle;
    }

    Vector2 NextPoint(Vector2 start, float angle)
    {
        angle = Deg2Rad(angle);
        return new Vector2(start.x + Mathf.Cos(angle)*edgeLength, start.y + Mathf.Sin(angle)*edgeLength);
    }

    MidPoint MidPoint(Vector2 start, float angle)
    {
        var end = NextPoint(start, angle);
        Vector2 midPoint = start + new Vector2((end.x - start.x)/2, (end.y - start.y)/2);
        return new MidPoint(midPoint, start, end);
    }

    void Draw()
    {
        Vector3[] points3D = new Vector3[points.Length];
        for (var i = 0; i < points.Length; i++)
        {
            var point = points[i];
            points3D[i] = new(point.x, point.y, 0);
        }
        lineRenderer.SetPositions(points3D);
    }

    float Deg2Rad(float deg)
    {
        return deg * Mathf.PI / 180f;
    }

    public bool ContainsPoint(Vector2 point)
    {
        var intersections = 0;
        float m0 = 0;
        float b0 = point.y;
        for (var i = 0; i < points.Length; i++)
        {
            var start = points[i];
            var end = points[(i + 1) % points.Length];
            var diff = end - start;

            var m1 = diff.y/diff.x;
            var b1 = start.y - m1 * start.x;

            if ((m0 - m1) == 0) continue;

            var x = (b1 - b0) / (m0 - m1);

            var xMin = Mathf.Min(start.x, end.x);
            var xMax = Mathf.Max(start.x, end.x);

            if (x < xMax && x > xMin && x > point.x)
            {
                intersections++;
            }
        }
        return (intersections % 2 == 1);
    }

    void PrintArr<T>(T[] arr)
    {
        for (var i = 0; i < arr.Length; i++)
        {
            Debug.Log(arr[i]);
        }
    }
}

public class MidPoint
{
    public readonly Vector2 point;
    public readonly Vector2 p0;
    public readonly Vector2 p1;

    public MidPoint(Vector2 Point, Vector2 P0, Vector2 P1)
    {
        point = Point;
        p0 = P0;
        p1 = P1;
    }
}
