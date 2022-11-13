using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    PolygonFactory polygonFactory;

    readonly Vector2 p0 = new(0, 0);
    readonly Vector2 p1 = new(1, 0);

    List<Polygon> polygons = new();
    HashSet<Vector2> usedMidPoints = new();

    // Start is called before the first frame update
    void Start()
    {
        polygonFactory = GameObject.Find("Polygon Factory").GetComponent<PolygonFactory>();
        AddPentagon(p0, p1);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var mousePos = GetMousePos();
            var midPoint = ClosestMidPoint(mousePos);
            var key = RoundVec(midPoint.point);
            if (!usedMidPoints.Contains(key))
            {
                var polygon = polygonFactory.CreatePentagon(midPoint.p1, midPoint.p0);
                polygons.Add(polygon);
                usedMidPoints.Add(RoundVec(midPoint.point));
            }
        }
        if (Input.GetMouseButtonDown(1))
        {
            var mousePos = GetMousePos();
            int ind = 0;
            foreach (var polygon in polygons)
            {
                if (polygon.ContainsPoint(mousePos))
                {
                    polygons.RemoveAt(ind);
                    Destroy(polygon.gameObject);
                    break;
                }
                ind++;
            }
        }
    }

    Vector2 GetMousePos()
    {
        var mousePos3D = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mousePos = new(mousePos3D.x, mousePos3D.y);
        return mousePos;
    }

    Vector2 RoundVec(Vector2 vec)
    {
        return new(Round(vec.x, 2), Round(vec.y, 2));
    }

    float Round(float val, int precision)
    {
        var factor = Mathf.Pow(10, precision);
        return Mathf.Round(val * precision)/precision;
    }

    void AddPentagon(Vector2 p0, Vector2 p1)
    {
        var polygon = polygonFactory.CreatePentagon(p0, p1);
        polygons.Add(polygon);
    }

    MidPoint GetMidPoint(Vector2 p0, Vector2 p1)
    {
        var midPoint = p0 + new Vector2((p1.x - p0.x) / 2, (p1.y - p0.y) / 2);
        return new MidPoint(midPoint, p0, p1);
    }

    MidPoint ClosestMidPoint(Vector2 pos)
    {
        var minDistance = float.MaxValue;
        MidPoint closestMidpoint = null;
        foreach (var polygon in polygons)
        {
            var midPoints = polygon.midPoints;
            for (var i = 0; i < midPoints.Length; i++)
            {
                var midPoint = midPoints[i];
                var distance = Vector2.Distance(pos, midPoint.point);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestMidpoint = midPoint;
                }
            }
        }
        return closestMidpoint;
    }
}
