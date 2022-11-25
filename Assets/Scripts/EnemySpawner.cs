using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    PolygonFactory polygonFactory;
    GameManager gameManager;

    Vector2 root;

    float time = 0;
    float interval = 1;
    float distance = 10;

    int capacity = 10;

    bool spawning = false;

    float forceMultiplier = 30;
    float torque = 30;

    int vertices = 3;

    // Start is called before the first frame update
    void Start()
    {
        polygonFactory = GameObject.Find("Polygon Factory").GetComponent<PolygonFactory>();
        gameManager = Helpers.GameManager();
        this.root = gameManager.root.polygon.centroid;
        spawning = true;
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        var spawned = 0;
        while (spawned < capacity)
        {
            yield return new WaitForSeconds(interval);
            var line = RandomSpawnLine();
            var enemyPrefab = polygonFactory.CreateEnemy(vertices, line);
            ApplyForce(enemyPrefab);
            spawned++;
        }
    }
    
    Line RandomSpawnLine()
    {
        var angle = Random.Range(0f, Helpers.fullAngle);
        Vector2 p0 = new(Mathf.Cos(angle) * distance, Mathf.Sin(angle) * distance);
        Vector2 p1 = new(p0.x, p0.y + 1);
        return new(p0, p1, null);
    }

    void ApplyForce(EnemyPrefab enemy)
    {
        var rigidBody = enemy.GetComponent<Rigidbody2D>();

        var force = (root - enemy.polygon.centroid).normalized*forceMultiplier;
        rigidBody.AddForce(force);
        rigidBody.AddTorque(torque);
    }
}
