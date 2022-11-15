using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PolygonPrefab : MonoBehaviour
{
    LineRenderer lineRenderer;
    public Polygon polygon { get; private set; }

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    public void Initialize(Polygon Polygon)
    {
        polygon = Polygon;

        lineRenderer.positionCount = polygon.points.Length;
        lineRenderer.loop = true;
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.startColor = Color.black;
        lineRenderer.endColor = Color.black;
        Draw();
    }

    public void SetColour(Color colour)
    {
        lineRenderer.startColor = colour;
        lineRenderer.endColor = colour;
    }

    void Draw()
    {
        Vector3[] points3D = new Vector3[polygon.points.Length];
        for (var i = 0; i < polygon.points.Length; i++)
        {
            var point = polygon.points[i];
            points3D[i] = new(point.x, point.y, 0);
        }
        lineRenderer.SetPositions(points3D);
    }
}
