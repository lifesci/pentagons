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

    PolygonPrefab Create(int edges, Vector2 p0, Vector2 p1)
    {
        var polygonObj = Instantiate(polygonPrefab, position, rotation);
        var polygon = polygonObj.GetComponent<PolygonPrefab>();
        polygon.Initialize(new Polygon(edges, p0, p1));
        return polygon;
    }

    Polygon CreateVirtual(int edges, MidPoint midPoint)
    {
        return new Polygon(edges, midPoint);
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

    public Polygon CreateVirtualPentagon(MidPoint midPoint)
    {
        return CreateVirtual(5, midPoint);
    }

}
