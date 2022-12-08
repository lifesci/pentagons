using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    PolygonFactory polygonFactory;
    GameManager gameManager;

    const float interval = 1;
    const float distance = 10;

    const float forceMultiplier = 30;
    const float torque = 30;

    const int minVertices = 3;
    const int maxVertices = 12;

    // Start is called before the first frame update
    void Start()
    {
        polygonFactory = gameObject.GetComponent<PolygonFactory>();
        gameManager = gameObject.GetComponent<GameManager>();
    }

    public int SpawnRandom(int totalVertices, int delay)
    {
        List<int> enemySizes = new();
        var remainingVertices = totalVertices;
        while (remainingVertices > 0)
        {
            int vertices;
            if (remainingVertices/minVertices < 2)
            {
                vertices = remainingVertices;
            } else
            {
                if (remainingVertices <= maxVertices)
                {
                    var maxChoice = remainingVertices - minVertices;
                    var choice = Random.Range(minVertices, maxChoice + 2);
                    if (choice > maxChoice)
                    {
                        choice = remainingVertices;
                    }
                    vertices = choice;
                } else
                {
                    var maxChoice = Mathf.Min(remainingVertices - minVertices, maxVertices);
                    vertices = Random.Range(minVertices, maxChoice);
                }
            }
            enemySizes.Add(vertices);
            remainingVertices -= vertices;
        }

        StartCoroutine(SpawnRoutine(enemySizes, delay));
        return enemySizes.Count;
    }

    IEnumerator SpawnRoutine(List<int> enemySizes, int delay)
    {
        yield return new WaitForSeconds(delay);
        foreach (var vertices in enemySizes)
        {
            yield return new WaitForSeconds(interval);
            var line = RandomSpawnLine();
            var enemyPrefab = polygonFactory.CreateEnemy(vertices, line);
            ApplyForce(enemyPrefab);
        }
    }
    
    Line RandomSpawnLine()
    {
        var angle = Random.Range(0f, Helpers.fullAngle);
        Vector2 p0 = new(Mathf.Cos(angle) * distance, Mathf.Sin(angle) * distance);
        Vector2 p1 = new(p0.x, p0.y + 1);
        return new(p0, p1, null);
    }

    Vector2 GetCentre() {
        return gameManager.root.polygon.centroid;
    }

    void ApplyForce(EnemyPrefab enemy)
    {
        var rigidBody = enemy.GetComponent<Rigidbody2D>();

        var force = (GetCentre() - enemy.polygon.centroid).normalized*forceMultiplier;
        rigidBody.AddForce(force);
        rigidBody.AddTorque(torque);
    }
}
