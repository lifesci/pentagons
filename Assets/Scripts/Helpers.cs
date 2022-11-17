using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Helpers
{
    public const float fullAngle = 360;
    public const float halfAngle = 180;

    public static float Round(float val, int precision)
    {
        var factor = Mathf.Pow(10, precision);
        return Mathf.Round(val * factor) / factor;
    }

    public static Vector2 RoundVec(Vector2 vec, int precision)
    {
        return new(Round(vec.x, precision), Round(vec.y, precision));
    }

    public static float UnsignedAngle(float angle)
    {
        var unsignedAngle = angle % fullAngle;
        if (unsignedAngle < 0) unsignedAngle += fullAngle;
        return unsignedAngle;
    }

    public static float UnsignedAngle(Vector2 p0, Vector2 p1)
    {
        var signedAngle = Vector2.SignedAngle(Vector2.right, p1 - p0);
        return UnsignedAngle(signedAngle);
    }

    public static float Deg2Rad(float deg)
    {
        return deg * Mathf.PI / 180;
    }

    public static Line ClosestFreeLine(Vector2 position, Dictionary<Line, int> lineCounts)
    {
        Line closestFreeLine = null;
        float minDistance = float.MaxValue;
        foreach(var item in lineCounts)
        {
            var line = item.Key;
            var count = item.Value;

            // a free line only appears once
            if (count != 1) continue;

            var distance = Vector2.Distance(position, line.midpoint);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestFreeLine = line;
            }
        }
        return closestFreeLine;
    }
}
