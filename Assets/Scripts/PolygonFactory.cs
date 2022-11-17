using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PolygonFactory : MonoBehaviour
{
    [SerializeField] GameObject polygonPrefab;
    Vector3 position;
    Quaternion rotation;

    private void Awake()
    {
        position = polygonPrefab.transform.position;
        rotation = polygonPrefab.transform.rotation;
    }

    PolygonPrefab Create(int vertices, Vector2 p0, Vector2 p1)
    {
        var polygonObj = Instantiate(polygonPrefab, position, rotation);
        var polygon = polygonObj.GetComponent<PolygonPrefab>();
        polygon.Initialize(new Polygon(vertices, new Line(p1, p0)));
        return polygon;
    }
    PolygonPrefab Create(int vertices, Line line)
    {
        var polygonObj = Instantiate(polygonPrefab, position, rotation);
        var polygon = polygonObj.GetComponent<PolygonPrefab>();
        polygon.Initialize(new Polygon(vertices, line));
        return polygon;
    }

    public PolygonPrefab CreateFromVirtual(Polygon virtualPoly)
    {
        var polygonClone = Instantiate(polygonPrefab, position, rotation);
        var polygonScript = polygonClone.GetComponent<PolygonPrefab>();
        polygonScript.Initialize(virtualPoly);
        return polygonScript;
    }


    public PolygonPrefab CreatePentagon(Vector2 p0, Vector2 p1)
    {
        return Create(5, p0, p1);
    }
    public PolygonPrefab CreatePentagon(Line line)
    {
        return Create(5, line);
    }

    public Polygon CreateVirtualPentagon(Line line)
    {
        return new Polygon(5, line);
    }

}
