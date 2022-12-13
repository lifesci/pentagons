using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PolygonFactory : MonoBehaviour
{
    [SerializeField] GameObject polygonPrefab;
    [SerializeField] GameObject enemyPrefab;

    Vector3 position;
    Quaternion rotation;

    Vector3 enemyPosition;
    Quaternion enemyRotation;

    private void Awake()
    {
        position = polygonPrefab.transform.position;
        rotation = polygonPrefab.transform.rotation;

        enemyPosition = polygonPrefab.transform.position;
        enemyRotation = polygonPrefab.transform.rotation;
    }

    public PolygonPrefab Create(int vertices, Vector2 p0, Vector2 p1, HashSet<Line> linesSet, bool root)
    {
        var polygonObj = Instantiate(polygonPrefab, position, rotation);
        var polygon = polygonObj.GetComponent<PolygonPrefab>();
        polygon.Initialize(new Polygon(vertices, new Line(p1, p0, null), linesSet, root: root), enemy: false);
        return polygon;
    }

    public PolygonPrefab CreateFromVirtual(Polygon virtualPoly)
    {
        var polygonClone = Instantiate(polygonPrefab, position, rotation);
        var polygonScript = polygonClone.GetComponent<PolygonPrefab>();
        polygonScript.Initialize(virtualPoly, enemy: false);
        return polygonScript;
    }

    public Polygon CreateVirtualGhost(int vertices, Line line, HashSet<Line> linesSet)
    {
        return new Polygon(vertices, line, linesSet, ghost: true);
    }

    public PolygonPrefab CreateGhost(Polygon virtualPoly)
    {
        virtualPoly.SetGhost(true);
        var polygonPrefab = CreateFromVirtual(virtualPoly);
        return polygonPrefab;
    }

    public PolygonPrefab CreateEnemy(int vertices, Line line)
    {
        var enemyObj = Instantiate(polygonPrefab, position, rotation);
        var enemy = enemyObj.GetComponent<PolygonPrefab>();
        enemy.Initialize(new Polygon(vertices, line, new()), enemy: true);
        return enemy;
    }
}
