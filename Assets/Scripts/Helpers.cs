using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Helpers
{
    public static float Round(float val, int precision)
    {
        var factor = Mathf.Pow(10, precision);
        return Mathf.Round(val * factor) / factor;
    }

    public static Vector2 RoundVec(Vector2 vec, int precision)
    {
        return new(Round(vec.x, precision), Round(vec.y, precision));
    }
}
