using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PolygonPrefab : MonoBehaviour
{
    protected LineRenderer lineRenderer;
    protected PolygonCollider2D polygonCollider;
    public Polygon polygon { get; protected set; }

    protected Vector2[] relativePoints;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        polygonCollider = GetComponent<PolygonCollider2D>();
    }

    public void Initialize(Polygon polygon)
    {
        this.polygon = polygon;
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
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.startColor = Color.black;
        lineRenderer.endColor = Color.black;
    }

    public void SetColour(Color colour)
    {
        lineRenderer.startColor = colour;
        lineRenderer.endColor = colour;
    }

    protected void Draw()
    {
        Vector3[] points3D = new Vector3[relativePoints.Length];
        for (var i = 0; i < relativePoints.Length; i++)
        {
            var point = relativePoints[i];
            points3D[i] = new(point.x, point.y, 0);
        }
        lineRenderer.SetPositions(points3D);
    }
}
