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
}
