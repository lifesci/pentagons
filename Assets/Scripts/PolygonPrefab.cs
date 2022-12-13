using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PolygonPrefab : MonoBehaviour
{
    protected LineRenderer lineRenderer;
    protected PolygonCollider2D polygonCollider;
    public Polygon polygon { get; protected set; }

    protected Vector2[] relativePoints;

    int _maxHealth;
    public int maxHealth {
        get {
            return _maxHealth;
        }

        protected set
        {
            _maxHealth = value;
            SetColour();
        }
    }

    int _health;
    public int health
    {
        get
        {
            return _health;
        }

        set
        {
            _health = value;
            SetColour();
        }
    }

    public int damage { get; protected set; }

    protected Color healthyColour = Color.green;
    protected Color deadColour = Color.red;
    Color ghostColour = Color.grey;
    Color rootColour = Color.cyan;

    protected float lineWidth = 0.1f;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        polygonCollider = GetComponent<PolygonCollider2D>();
    }

    public void Initialize(Polygon polygon)
    {
        this.polygon = polygon;
        maxHealth = polygon.vertices;
        damage = polygon.vertices;
        health = maxHealth;
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
        Color colour;
        if (polygon.ghost)
        {
            colour = ghostColour;
        } else if (polygon.root)
        {
            colour = rootColour;
        } else
        {

            var distance = (float)Mathf.Max(health - 1, 0) / (maxHealth - 1);
            colour = Color.Lerp(deadColour, healthyColour, distance);
        }
        if (lineRenderer is not null)
        {
            lineRenderer.startColor = colour;
            lineRenderer.endColor = colour;
        }
    }

    public void UnGhost()
    {
        polygon.UnGhost();
        SetColour();
    }

    protected void Draw()
    {
        Vector3[] points3D = new Vector3[relativePoints.Length];
        for (var i = 0; i < relativePoints.Length; i++)
        {
            var relativePoint = relativePoints[i];

            // unit vector from point to centroid
            var relativeVec = -relativePoint.normalized;
            var adjustmentFactor = lineWidth/Mathf.Sin(Helpers.Deg2Rad(polygon.intAngle)/2)/2;
            var adjustment = relativeVec * adjustmentFactor;

            var point = relativePoint + adjustment;

            points3D[i] = new(point.x, point.y, 0);
        }
        lineRenderer.SetPositions(points3D);
    }

    public virtual bool TakeDamage(int damageTaken)
    {
        health -= damageTaken;
        return IsDead();
    }

    public virtual bool IsDead()
    {
        return health <= 0;
    }

    public void ApplyHeartbeat()
    {
        maxHealth += 2;
    }

    public void ApplyUpgrade(Upgrade upgrade)
    {
        if (!upgrade.active || upgrade.applied) return;
        switch(upgrade.name)
        {
            case UpgradeManager.UpgradeName.heartbeat:
                ApplyHeartbeat();
                break;
            case UpgradeManager.UpgradeName.cooperation:
                break;
            case UpgradeManager.UpgradeName.reinforce:
                break;
            case UpgradeManager.UpgradeName.warehouse:
                break;
            default:
                break;
        }
        upgrade.SetApplied();
        upgrade.SetActive();
    }

    public void ApplyAllUpgrades()
    {
        foreach(var upgrade in UpgradeManager.GetUpgradeList())
        {
            ApplyUpgrade(upgrade);
        }
    }
}
