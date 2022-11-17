using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Polygon
{
    readonly float intAngle;
    readonly float intComplement;
    readonly float edgeLength;
    readonly int vertices;

    // points start from the base and move counter-clockwise

    public Line[] lines { get; private set; }
    public Vector2[] points { get; private set; }

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
        for (var i = 1; i < vertices; i++)
        {
            curAngle = Rotate(curAngle);
            var start = lines[i - 1].p1;
            var end = NextPoint(start, curAngle);
            lines[i] = new(start, end);
        }

        // populate points
        points = new Vector2[vertices];
        for (var i = 0; i < vertices; i++)
        {
            points[i] = lines[i].p0;
        }
    }

    float Rotate(float angle)
    {
        return Helpers.UnsignedAngle(angle + intComplement);
    }

    Vector2 NextPoint(Vector2 start, float angle)
    {
        angle = Helpers.Deg2Rad(angle);
        return new Vector2(start.x + Mathf.Cos(angle)*edgeLength, start.y + Mathf.Sin(angle)*edgeLength);
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

public class Line
{
    public readonly Vector2 p0;
    public readonly Vector2 p1;
    public readonly float length;
    public readonly float angle;

    public readonly Vector2 midpoint;

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
        length = line.length;
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
