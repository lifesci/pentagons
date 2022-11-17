using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    PolygonFactory polygonFactory;

    readonly Vector2 p0 = new(0, 0);
    readonly Vector2 p1 = new(1, 0);

    List<PolygonPrefab> polygonClones = new();
    Dictionary<Line, int> lineCounts = new();

    PolygonPrefab ghost;
    Vector2 ghostKey;

    // Start is called before the first frame update
    void Start()
    {
        polygonFactory = GameObject.Find("Polygon Factory").GetComponent<PolygonFactory>();
        AddPolygon(p0, p1);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            AddPolygon();
        }
        else if (Input.GetMouseButtonDown(1))
        {
            DeletePolygon();
        } else
        {
            AddGhostPolygon();
        }
    }

    void AddPolygon()
    {
        var mousePos = GetMousePos();
        var line = Helpers.ClosestFreeLine(mousePos, lineCounts);
        if (line is not null)
        {
            var polygonPrefab = polygonFactory.CreatePentagon(line);
            AddPolygonLines(polygonPrefab);
        }
    }

    void AddPolygon(Vector2 p0, Vector2 p1)
    {
        var polygonPrefab = polygonFactory.CreatePentagon(p0, p1);
        AddPolygonLines(polygonPrefab);
    }

    void AddPolygonLines(PolygonPrefab polygonPrefab)
    {
        var polygon = polygonPrefab.polygon;
        var lines = polygon.lines;
        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            if (!lineCounts.ContainsKey(line)) lineCounts[line] = 0;
            lineCounts[line]++;
        }
    }

    void DeletePolygon()
    {
        var mousePos = GetMousePos();
    }

    void AddGhostPolygon()
    {
        var mousePos = GetMousePos();
    }

    Vector2 GetMousePos()
    {
        var mousePos3D = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mousePos = new(mousePos3D.x, mousePos3D.y);
        return mousePos;
    }
}
