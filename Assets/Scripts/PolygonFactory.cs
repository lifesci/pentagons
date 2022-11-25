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

    public PolygonPrefab Create(int vertices, Vector2 p0, Vector2 p1, HashSet<Line> linesSet)
    {
        var polygonObj = Instantiate(polygonPrefab, position, rotation);
        var polygon = polygonObj.GetComponent<PolygonPrefab>();
        polygon.Initialize(new Polygon(vertices, new Line(p1, p0, null), linesSet));
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

    public Polygon CreateVirtual(int vertices, Line line, HashSet<Line> linesSet)
    {
        return new Polygon(vertices, line, linesSet);
    }

    public EnemyPrefab CreateEnemy(int vertices, Line line)
    {
        var enemyObj = Instantiate(enemyPrefab, position, rotation);
        var enemy = enemyObj.GetComponent<EnemyPrefab>();
        enemy.Initialize(new Polygon(vertices, line, new()));
        return enemy;
    }
}
