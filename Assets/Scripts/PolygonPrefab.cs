using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PolygonPrefab : MonoBehaviour
{
    protected LineRenderer lineRenderer;
    protected PolygonCollider2D polygonCollider;
    public Polygon polygon { get; protected set; }

    protected Vector2[] relativePoints;

    public int health;

    protected Color healthyColour;
    protected Color deadColour;

    protected float lineWidth = 0.1f;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        polygonCollider = GetComponent<PolygonCollider2D>();

        healthyColour = Color.green;
        deadColour = Color.red;
    }

    public void Initialize(Polygon polygon)
    {
        this.polygon = polygon;
        health = polygon.vertices;
        gameObject.transform.position = polygon.centroid;

        SetRelativePoints();
        InitializeCollider();
        InitializeRenderer();

        Draw();
    }

    protected void SetRelativePoints()
    {
        relativePoints = new Vector2[polygon.points.Length];
        for (var i = 0; i < polygon.points.Length; i++)
        {
            relativePoints[i] = polygon.points[i] - polygon.centroid;
        }
    }

    public void InitializeCollider()
    {
        polygonCollider.offset = Vector2.zero;
        polygonCollider.isTrigger = true;
        polygonCollider.pathCount = 1;
        polygonCollider.SetPath(0, relativePoints);
    }

    public void InitializeRenderer()
    {
        lineRenderer.positionCount = relativePoints.Length;
        lineRenderer.loop = true;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        SetColour();
    }

    public void SetColour()
    {
        var distance = (float)Mathf.Max(health - 1, 0) / (polygon.vertices - 1);
        var colour = Color.Lerp(deadColour, healthyColour, distance);
        lineRenderer.startColor = colour;
        lineRenderer.endColor = colour;
    }

    protected void Draw()
    {
        Vector3[] points3D = new Vector3[relativePoints.Length];
        for (var i = 0; i < relativePoints.Length; i++)
        {
            var relativePoint = relativePoints[i];

            // unit vector from point to centroid
            var relativeVec = -relativePoint.normalized;
            var adjustmentFactor = lineWidth/ Mathf.Sin(Helpers.Deg2Rad(polygon.intAngle)/ 2)/2;
            var adjustment = relativeVec * adjustmentFactor;

            var point = relativePoint + adjustment;

            points3D[i] = new(point.x, point.y, 0);
        }
        lineRenderer.SetPositions(points3D);
    }

    public virtual bool TakeDamage(int damage)
    {
        health -= damage;
        SetColour();
        return IsDead();
    }

    public virtual bool IsDead()
    {
        return health <= 0;
    }
}
