using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Polygon
{
    // geometric properties
    readonly float intAngle;
    readonly float intComplement;
    readonly float edgeLength;
    readonly int vertices;
    readonly Vector2 centroid;

    // bounding box
    public readonly float bboxXMin = float.MaxValue;
    public readonly float bboxXMax = float.MinValue;
    public readonly float bboxYMin = float.MaxValue;
    public readonly float bboxYMax = float.MinValue;

    public Line[] lines { get; private set; }
    public Vector2[] points { get; private set; }

    public Polygon(int vertices, Line line, HashSet<Line> linesSet)
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
            Line nextLine = SnapToGrid(start, end, linesSet);
            lines[i] = nextLine;
        }

        // populate points, center point, and bounding box
        points = new Vector2[vertices];
        var xSum = 0f;
        var ySum = 0f;
        for (var i = 0; i < vertices; i++)
        {
            var point = lines[i].p0;
            points[i] = point;

            xSum += point.x;
            ySum += point.y;

            var x = point.x;
            var y = point.y;
            bboxXMin = Mathf.Min(bboxXMin, x);
            bboxXMax = Mathf.Max(bboxXMax, x);
            bboxYMin = Mathf.Min(bboxYMin, y);
            bboxYMax = Mathf.Max(bboxYMax, y);
        }

        centroid = new(xSum / vertices, ySum / vertices);
    }

    Line SnapToGrid(Vector2 start, Vector2 end, HashSet<Line> linesSet)
    {
        foreach (var existingLine in linesSet)
        {
            if (Vector2.Distance(start, existingLine.p0) < Helpers.epsilon)
            {
                start = existingLine.p0;
            }
            if (Vector2.Distance(start, existingLine.p1) < Helpers.epsilon)
            {
                start = existingLine.p1;
            }
            if (Vector2.Distance(end, existingLine.p0) < Helpers.epsilon)
            {
                end = existingLine.p0;
            }
            if (Vector2.Distance(end, existingLine.p1) < Helpers.epsilon)
            {
                end = existingLine.p1;
            }
        }
        return new(start, end);
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
        // check bounding box
        if (!BBoxContainsPoint(point)) return false;

        // check polygon
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

    bool BBoxContainsPoint(Vector2 point)
    {
        var containsX = point.x > bboxXMin && point.x < bboxXMax;
        var containsY = point.y > bboxYMin && point.y < bboxYMax;
        return containsX && containsY;
    }

    public bool Intersects(Polygon polygon)
    {
        for (var i = 0; i < points.Length; i++)
        {
            if (ContainsPoint(points[i])) return true;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return centroid.GetHashCode();
    }
}

public class Line
{
    public readonly Vector2 p0;
    public readonly Vector2 p1;
    public readonly float length;
    public readonly float angle;

    public readonly Vector2 midpoint;

    bool negativeHash;

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
            negativeHash = true;
        } else
        {
            (start, end) = p0.y < p1.y ? (p0, p1) : (p1, p0);
            negativeHash = false;
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
}
