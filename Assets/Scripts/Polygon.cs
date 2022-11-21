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

    public bool root { get; private set; } = false;

    public Line[] lines { get; private set; }
    public Vector2[] points { get; private set; }
    public HashSet<Polygon> neighbours { get; private set; } = new();

    public Polygon(int vertices, Line line, HashSet<Line> linesSet)
    {
        // populate geometric properties
        intAngle = (vertices - 2) * 180f / vertices;
        intComplement = Helpers.halfAngle - intAngle;
        edgeLength = line.length;
        this.vertices = vertices;

        // populate lines
        lines = new Line[vertices];
        lines[0] = new(line, this);
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

        // set centroid
        centroid = new(xSum / vertices, ySum / vertices);

        // update neighbours
        var neighbour = line.polygon;
        if (neighbour is not null)
        {
            AddNeighbour(neighbour);
            neighbour.AddNeighbour(this);
        }
    }

    public void SetRoot(bool val)
    {
        root = val;
    }

    void AddNeighbour(Polygon polygon)
    {
        neighbours.Add(polygon);
    }

    void RemoveNeighbour(Polygon polygon)
    {
        if (neighbours.Contains(polygon))
        {
            neighbours.Remove(polygon);
        }
    }

    Line SnapToGrid(Vector2 start, Vector2 end, HashSet<Line> linesSet)
    {
        Polygon mappedStart = null;
        Polygon mappedEnd = null;
        foreach (var existingLine in linesSet)
        {
            if (Vector2.Distance(start, existingLine.p0) < Helpers.epsilon)
            {
                start = existingLine.p0;
                mappedStart = existingLine.polygon;
            }
            if (Vector2.Distance(start, existingLine.p1) < Helpers.epsilon)
            {
                start = existingLine.p1;
                mappedStart = existingLine.polygon;
            }
            if (Vector2.Distance(end, existingLine.p0) < Helpers.epsilon)
            {
                end = existingLine.p0;
                mappedEnd = existingLine.polygon;
            }
            if (Vector2.Distance(end, existingLine.p1) < Helpers.epsilon)
            {
                end = existingLine.p1;
                mappedEnd = existingLine.polygon;
            }

            // found matching line
            if (mappedStart is not null && mappedStart == mappedEnd) break;
        }

        // detect neighbour
        if ((mappedStart is not null) && (mappedStart == mappedEnd))
        {
            AddNeighbour(mappedStart);
            mappedStart.AddNeighbour(this);
        }

        return new(start, end, this);
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

        var setMeasure = false;
        var positive = true;

        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            var measure = (point.x - line.p0.x) * (line.p1.y - line.p0.y) - (point.y - line.p0.y) * (line.p1.x - line.p0.x);

            // point is on line
            if (measure == 0) return false;

            if (!setMeasure)
            {
                positive = measure > 0;
                setMeasure = true;
            } else
            {
                if (!((measure > 0) == positive))
                {
                    return false;
                }
            }
        }
        return true;
    }

    bool BBoxContainsPoint(Vector2 point)
    {
        var containsX = point.x > bboxXMin && point.x < bboxXMax;
        var containsY = point.y > bboxYMin && point.y < bboxYMax;
        return containsX && containsY;
    }

    public bool Intersects(Polygon polygon)
    {
        for (var i = 0; i < polygon.points.Length; i++)
        {
            if (ContainsPoint(polygon.points[i])) return true;
        }
        return false;
    }
}

public class Line
{
    public readonly Vector2 p0;
    public readonly Vector2 p1;
    public readonly float length;
    public readonly float angle;

    public readonly Vector2 midpoint;

    public readonly Polygon polygon;

    bool negativeHash;

    public Line(Vector2 p0, Vector2 p1, Polygon polygon)
    {
        this.polygon = polygon;

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

    public Line(Line line, Polygon polygon)
    {
        this.polygon = polygon;
        p0 = line.p1;
        p1 = line.p0;
        midpoint = line.midpoint;
        angle = Helpers.UnsignedAngle(line.angle + Helpers.halfAngle);
        length = line.length;
    }
}
