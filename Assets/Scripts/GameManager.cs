using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    PolygonFactory polygonFactory;

    readonly Vector2 p0 = new(0, 0);
    readonly Vector2 p1 = new(1, 0);

    List<PolygonPrefab> polygonClones = new();
    Dictionary<Vector2, (MidPoint, int)> midPointCounts = new();

    PolygonPrefab ghost;
    Vector2 ghostKey;

    // Start is called before the first frame update
    void Start()
    {
        polygonFactory = GameObject.Find("Polygon Factory").GetComponent<PolygonFactory>();
        polygonFactory.CreatePentagon(p0, p1);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleAddPolygon();
        }
        else if (Input.GetMouseButtonDown(1))
        {
            HandleDeletePolygon();
        } else
        {
            HandleGhostPolygon();
        }
    }

    void HandleAddPolygon()
    {
        var mousePos = GetMousePos();
        if (ghost is not null)
        {
            polygonClones.Add(ghost);
            UpdateMidPointCounts(ghost.polygon, add: true);
            ghost.SetColour(Color.black);
            ghost = null;
            ghostKey = Vector2.zero;
        }
    }

    void HandleDeletePolygon()
    {
        var mousePos = GetMousePos();
        int ind = 0;
        foreach (var polygonClone in polygonClones)
        {
            var contains = polygonClone.polygon.ContainsPoint(mousePos);
            if (contains)
            {
                UpdateMidPointCounts(polygonClone.polygon, add: false);
                polygonClones.RemoveAt(ind);
                Destroy(polygonClone.gameObject);
                break;
            }
            ind++;
        }
    }

    void HandleGhostPolygon()
    {
        var mousePos = GetMousePos();
        var midPoint = ClosestMidPoint(mousePos);
        if (midPoint is null) return;
        var key = midPoint.point;
        if (key != ghostKey)
        {
            if (ghost is not null)
            {
                var oldGameObj = ghost.gameObject;
                ghost = null;
                Destroy(oldGameObj);
            }

            var virtualShape = polygonFactory.CreateVirtualPentagon(midPoint);
            ghost = polygonFactory.CreateFromVirtual(virtualShape);
            ghost.SetColour(Color.grey);
            ghostKey = key;
        }
    }

    Vector2 GetMousePos()
    {
        var mousePos3D = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mousePos = new(mousePos3D.x, mousePos3D.y);
        return mousePos;
    }

    void AddPentagon(Vector2 p0, Vector2 p1)
    {
        var polygonClone = polygonFactory.CreatePentagon(p0, p1);
        polygonClones.Add(polygonClone);
        UpdateMidPointCounts(polygonClone.polygon, add: true);
    }

    void UpdateMidPointCounts(Polygon polygon, bool add)
    {
        var mod = add ? 1 : -1;
        int count;
        for(var i = 0; i < polygon.midPoints.Length; i++)
        {
            var midPoint = polygon.midPoints[i];
            if (!midPointCounts.ContainsKey(midPoint.point)) {
                count = mod;
                midPointCounts.Add(midPoint.point, (midPoint, count));
            } else
            {
                (var existingMidPoint, var existingCount) = midPointCounts[midPoint.point];
                count = existingCount + mod;
                midPointCounts[midPoint.point] = (existingMidPoint, count);
            }
            if (count <= 0)
            {
                midPointCounts.Remove(midPoint.point);
            }
        }
    }

    MidPoint ClosestMidPoint(Vector2 pos)
    {
        var minDistance = float.MaxValue;
        MidPoint closestMidpoint = null;
        foreach (var item in midPointCounts)
        {
            var distance = Vector2.Distance(pos, item.Key);
            (var midPoint, var count) = item.Value;
            if (count == 1 && distance < minDistance)
            {
                minDistance = distance;
                closestMidpoint = midPoint;
            }
        }
        return closestMidpoint;
    }
}
