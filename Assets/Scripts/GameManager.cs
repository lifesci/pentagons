using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    PolygonFactory polygonFactory;

    readonly Vector2 p0 = new(0, 0);
    readonly Vector2 p1 = new(1, 0);

    List<PolygonPrefab> polygonClones = new();
    HashSet<Line> linesSet = new();
    Dictionary<Vector2, int> linesCount = new();

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
        var line = Helpers.ClosestFreeLine(mousePos, linesSet, linesCount);
        if (line is not null)
        {
            var virtualPolygon = polygonFactory.CreateVirtualPentagon(line, linesSet);

            // intersection not allowed
            foreach (var existingClone in polygonClones)
            {
                if (existingClone.polygon.Intersects(virtualPolygon)) return;
            }

            var polygonPrefab = polygonFactory.CreateFromVirtual(virtualPolygon);
            AddPolygonLines(polygonPrefab);
        }
    }

    void AddPolygon(Vector2 p0, Vector2 p1)
    {
        var polygonPrefab = polygonFactory.CreatePentagon(p0, p1, linesSet);
        AddPolygonLines(polygonPrefab);
    }

    void AddPolygonLines(PolygonPrefab polygonPrefab)
    {
        polygonClones.Add(polygonPrefab);
        var polygon = polygonPrefab.polygon;
        var lines = polygon.lines;
        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            var countKey = line.midpoint;
            linesSet.Add(line);
            if (!linesCount.ContainsKey(countKey)) linesCount[countKey] = 0;
            linesCount[line.midpoint]++;
        }
    }

    void RemovePolygonLines(Polygon polygon)
    {
        var lines = polygon.lines;
        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            var countKey = line.midpoint;
            linesSet.Remove(line);
            linesCount[countKey]--;
            if (linesCount[countKey] == 0)
            {
                linesCount.Remove(countKey);
            }
        }
    }

    void DeletePolygon()
    {
        // not allowed to delete last polygon
        if (polygonClones.Count == 1) return;

        var mousePos = GetMousePos();
        var indexToRemove = -1;
        var index = 0;
        PolygonPrefab prefabToRemove = null;
        foreach (var polygonPrefab in polygonClones)
        {
            var polygon = polygonPrefab.polygon;
            if (polygon.ContainsPoint(mousePos))
            {
                indexToRemove = index;
                prefabToRemove = polygonPrefab;
                break;
            }
            index++;
        }
        if (prefabToRemove is not null)
        {
            polygonClones.RemoveAt(indexToRemove);
            RemovePolygonLines(prefabToRemove.polygon);
            Destroy(prefabToRemove.gameObject);
        }
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
