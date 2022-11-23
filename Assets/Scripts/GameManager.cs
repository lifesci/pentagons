using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    PolygonFactory polygonFactory;

    readonly Vector2 p0 = new(0, 0);
    readonly Vector2 p1 = new(1, 0);

    HashSet<PolygonPrefab> polygonClones = new();
    HashSet<Line> linesSet = new();
    Dictionary<Vector2, int> linesCount = new();

    public PolygonPrefab root { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        polygonFactory = GameObject.Find("Polygon Factory").GetComponent<PolygonFactory>();
        AddRootPolygon(p0, p1);
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
            DeleteClosestPolygon();
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

    void AddRootPolygon(Vector2 p0, Vector2 p1)
    {
        var polygonPrefab = polygonFactory.CreatePentagon(p0, p1, linesSet);
        polygonPrefab.polygon.SetRoot(true);
        AddPolygonLines(polygonPrefab);
        root = polygonPrefab;
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

    void RemovePolygonNeighbours(Polygon polygon)
    {
        foreach(var neighbour in polygon.neighbours)
        {
            neighbour.neighbours.Remove(polygon);
        }
    }

    void DeleteClosestPolygon()
    {
        var mousePos = GetMousePos();
        PolygonPrefab prefabToRemove = null;
        foreach (var polygonPrefab in polygonClones)
        {
            var polygon = polygonPrefab.polygon;
            if (polygon.ContainsPoint(mousePos))
            {
                prefabToRemove = polygonPrefab;
                break;
            }
        }

        // root object cannot be removed manually
        if (prefabToRemove is not null && !prefabToRemove.polygon.root)
        {
            DeletePolygon(prefabToRemove);
        }

        // remove any polygons not reachable from the root
        RemoveUnreachable();
    }

    void DeletePolygon(PolygonPrefab prefab)
    {
        polygonClones.Remove(prefab);
        RemovePolygonLines(prefab.polygon);
        RemovePolygonNeighbours(prefab.polygon);
        Destroy(prefab.gameObject);
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

    void RemoveUnreachable()
    {
        var reachablePolygons = GetReachablePolygons();
        List<PolygonPrefab> toRemove = new();
        foreach (var polygonPrefab in polygonClones)
        {
            if (!reachablePolygons.Contains(polygonPrefab.polygon))
            {
                toRemove.Add(polygonPrefab);
            }
        }

        foreach (var prefab in toRemove)
        {
            DeletePolygon(prefab);
        }
    }

    // get set of polygons reachable from the root
    HashSet<Polygon> GetReachablePolygons()
    {
        Queue<Polygon> toVisit = new();
        HashSet<Polygon> visited = new();

        toVisit.Enqueue(root.polygon);

        while(toVisit.Count > 0)
        {
            var curPolygon = toVisit.Dequeue();
            visited.Add(curPolygon);
            foreach (var neighbour in curPolygon.neighbours)
            {
                if (!visited.Contains(neighbour))
                {
                    toVisit.Enqueue(neighbour);
                }
            }
        }

        return visited;
    }

    public void HandleCollision(PolygonPrefab polygon, EnemyPrefab enemy)
    {
        polygon.health -= enemy.polygon.vertices;
        enemy.health -= polygon.polygon.vertices;

        if (enemy.health <= 0) Destroy(enemy.gameObject);

        if (polygon.health <= 0)
        {
            DeletePolygon(polygon);
            RemoveUnreachable();
        }
    }
}
