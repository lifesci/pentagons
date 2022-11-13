using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PolygonFactory : MonoBehaviour
{
    [SerializeField] GameObject polygonPrefab;

    Polygon Create(int edges, Vector2 p0, Vector2 p1)
    {
        var position = polygonPrefab.transform.position;
        var rotation = polygonPrefab.transform.rotation;
        var polygonObj = Instantiate(polygonPrefab, position, rotation);
        var polygon = polygonObj.GetComponent<Polygon>();
        polygon.ComputeAndDraw(edges, p0, p1);
        return polygon;
    }

    public Polygon CreatePentagon(Vector2 p0, Vector2 p1)
    {
        return Create(5, p0, p1);
    }
}
