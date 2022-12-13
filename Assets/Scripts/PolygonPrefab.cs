using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PolygonPrefab : MonoBehaviour
{
    protected LineRenderer lineRenderer;
    protected PolygonCollider2D polygonCollider;

    GameManager gameManager;
    Rigidbody2D rigidbody;

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

    Color enemyHealthyColour = Color.magenta;
    Color enemyDeadColour = Color.black;

    Color ghostColour = Color.grey;
    Color rootColour = Color.cyan;

    protected float lineWidth = 0.1f;

    public bool enemy { get; private set; }

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        polygonCollider = GetComponent<PolygonCollider2D>();
        rigidbody = GetComponent<Rigidbody2D>();
        gameManager = Helpers.GameManager();
    }

    public void Initialize(Polygon polygon, bool enemy)
    {
        this.polygon = polygon;
        this.enemy = enemy;
        maxHealth = polygon.vertices;
        damage = polygon.vertices;
        health = maxHealth;
        gameObject.transform.position = polygon.centroid;

        SetRelativePoints();
        InitializeCollider();
        InitializeRenderer();
        InitializeRigidbody();
        InitializeTag();

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
        if (enemy)
        {
            polygonCollider.isTrigger = true;
        }
    }

    public void InitializeRenderer()
    {
        lineRenderer.positionCount = relativePoints.Length;
        lineRenderer.loop = true;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        SetColour();
    }

    void InitializeRigidbody()
    {
        if (enemy)
        {
            rigidbody.position = polygon.centroid;
        } else
        {
            var oldRigidbody = rigidbody;
            rigidbody = null;
            Destroy(oldRigidbody);
        }
    }

    void InitializeTag()
    {
        if (enemy)
        {
            gameObject.tag = "Enemy";
        } else
        {
            gameObject.tag = "Player";
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // only recognize collisions recognized by enemy polygons
        if (!enemy) return;

        var gameObject = collision.gameObject;

        // only recognize collisions against player polygons
        if (gameObject.CompareTag("Player"))
        {
            var polygonPrefab = gameObject.GetComponent<PolygonPrefab>();

            // ignore collisions with ghosts
            if (!polygonPrefab.polygon.ghost)
            {
                gameManager.RecordCollision(polygonPrefab, this);
            }
        }
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
            var startColour = enemy ? enemyDeadColour : deadColour;
            var endColour = enemy ? enemyHealthyColour : healthyColour;
            colour = Color.Lerp(startColour, endColour, distance);
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
