using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Polygon
{
    readonly float intAngle;
    readonly float intComplement;
    readonly float edgeLength;
    const float fullAngle = 360;
    readonly int numPoints;
    readonly Vector2 refAngle = new(1, 0);
    readonly int vertices;

    // points start from the base and move counter-clockwise
    public Vector2[] points { get; private set; }
    public MidPoint[] midPoints { get; private set; }

    public Line[] lines { get; private set; }

    public Polygon(int vertices, Vector2 p0, Vector2 p1)
    {
        Line baseLine = new(p1, p0);

        // populate geometric properties
        numPoints = vertices;
        intAngle = (numPoints - 2) * 180f / numPoints;
        intComplement = 180 - intAngle;
        edgeLength = Vector2.Distance(p0, p1);

        ComputePoints(vertices, p0, p1);

    }
    public Polygon(int vertices, MidPoint midPoint)
    {
        var p0 = midPoint.p1;
        var p1 = midPoint.p0;

        numPoints = vertices;
        intAngle = (numPoints - 2) * 180f / numPoints;
        intComplement = 180 - intAngle;
        edgeLength = Vector2.Distance(p0, p1);

        ComputePoints(vertices, p0, p1);

        points[1] = p1;
        midPoints[0] = new(midPoint.point, p0, p1);
    }

    public Polygon(int vertices, Line line)
    {
        // populate geometric properties
        intAngle = (vertices - 2) * 180f / vertices;
        intComplement = Helpers.halfAngle - intAngle;
        edgeLength = line.length;
        this.vertices = vertices;

        // populate lines
        lines = new Line[vertices];
        lines[0] = new(line);
        var curAngle = lines[0].angle;
        Debug.Log(curAngle);
        for (var i = 1; i < vertices; i++)
        {
            curAngle = Rotate(curAngle);
            var start = lines[i - 1].p1;
            var end = NextPoint(start, curAngle);
            lines[i] = new(start, end);
        }

        // populate points
        points = new Vector2[vertices];
        for (var i = 0; i < vertices; i++) points[i] = lines[i].p0;

        // for (var i = 0; i < vertices; i++) Debug.Log(points[i]);
    }

    public void ComputePoints(int vertices, Vector2 p0, Vector2 p1)
    {
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
    }

    float Rotate(float angle)
    {
        return Helpers.UnsignedAngle(angle + intComplement);
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

    public MidPoint(MidPoint midPoint)
    {
        point = midPoint.point;
        p0 = midPoint.p0;
        p1 = midPoint.p1;
    }
}

public class Line
{
    public readonly Vector2 p0;
    public readonly Vector2 p1;
    public readonly float length;
    public readonly float angle;

    Vector2 midpoint;

    public Line(Vector2 p0, Vector2 p1)
    {
        this.p0 = p0;
        this.p1 = p1;

        // set midpoint
        Vector2 start;
        Vector2 end;
        if (p0.x != p1.x)
        {
            (start, end) = p0.x < p1.x ? (p0, p1) : (p1, p0);
        } else
        {
            (start, end) = p0.y < p1.y ? (p0, p1) : (p1, p0);
        }
        var xMid = (end.x - start.x) / 2;
        var yMid = (end.y - start.y) / 2;
        midpoint = new(start.x + xMid, start.y + yMid);

        // set length
        length = Vector2.Distance(start, end);

        // set angle
        angle = Helpers.UnsignedAngle(p0, p1);
    }

    public Line(Line line)
    {
        p0 = line.p1;
        p1 = line.p0;
        midpoint = line.midpoint;
        angle = Helpers.UnsignedAngle(line.angle + Helpers.halfAngle);
    }

    bool Equals(Line other)
    {
        if (other is null) return false;
        var same = p0 == other.p0 && p1 == other.p1;
        var reversed = p0 == other.p1 && p1 == other.p0;
        return same || reversed;
    }

    public override bool Equals(object obj) => Equals(obj as Line);

    public override int GetHashCode()
    {
        return midpoint.GetHashCode();
    }
}