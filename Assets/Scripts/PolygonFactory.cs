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

    PolygonPrefab Create(int vertices, Vector2 p0, Vector2 p1, HashSet<Line> linesSet)
    {
        var polygonObj = Instantiate(polygonPrefab, position, rotation);
        var polygon = polygonObj.GetComponent<PolygonPrefab>();
        polygon.Initialize(new Polygon(vertices, new Line(p1, p0), linesSet));
        return polygon;
    }
    PolygonPrefab Create(int vertices, Line line, HashSet<Line> linesSet)
    {
        var polygonObj = Instantiate(polygonPrefab, position, rotation);
        var polygon = polygonObj.GetComponent<PolygonPrefab>();
        polygon.Initialize(new Polygon(vertices, line, linesSet));
        return polygon;
    }

    public PolygonPrefab CreateFromVirtual(Polygon virtualPoly)
    {
        var polygonClone = Instantiate(polygonPrefab, position, rotation);
        var polygonScript = polygonClone.GetComponent<PolygonPrefab>();
        polygonScript.Initialize(virtualPoly);
        return polygonScript;
    }


    public PolygonPrefab CreatePentagon(Vector2 p0, Vector2 p1, HashSet<Line> linesSet)
    {
        return Create(5, p0, p1, linesSet);
    }
    public PolygonPrefab CreatePentagon(Line line, HashSet<Line> linesSet)
    {
        return Create(5, line, linesSet);
    }

    public Polygon CreateVirtualPentagon(Line line, HashSet<Line> linesSet)
    {
        return new Polygon(5, line, linesSet);
    }

}
