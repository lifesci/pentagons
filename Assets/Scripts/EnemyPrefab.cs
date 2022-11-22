using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPrefab : PolygonPrefab
{
    public Rigidbody2D rigidBody { get; private set; }

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        polygonCollider = GetComponent<PolygonCollider2D>();
        rigidBody = GetComponent<Rigidbody2D>();
    }

    public new void Initialize(Polygon polygon)
    {
        this.polygon = polygon;

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
}
