using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPrefab : PolygonPrefab
{
    public Rigidbody2D rigidBody { get; private set; }
    public GameManager gameManager;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        polygonCollider = GetComponent<PolygonCollider2D>();
        rigidBody = GetComponent<Rigidbody2D>();
        gameManager = Helpers.GameManager();
    }

    public new void Initialize(Polygon polygon)
    {
        this.polygon = polygon;
        health = polygon.vertices;
        gameObject.transform.position = polygon.centroid;

        SetRelativePoints();
        InitializeRigidBody();
        InitializeCollider();
        InitializeRenderer();

        Draw();
    }

    void InitializeRigidBody()
    {
        rigidBody.position = polygon.centroid;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var gameObject = collision.gameObject;

        // only recognize collisions against polygons owned by the player
        if (gameObject.CompareTag("Player"))
        {
            var polygon = gameObject.GetComponent<PolygonPrefab>();
            gameManager.HandleCollision(polygon, this);
        }
    }
}
