using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PolygonPrefab : MonoBehaviour
{
    LineRenderer lineRenderer;
    PolygonCollider2D polygonCollider;

    GameManager gameManager;
    Rigidbody2D polygonRigidbody;

    public Polygon polygon { get; protected set; }

    Vector2[] relativePoints;

    HashSet<UpgradeManager.UpgradeName> appliedUpgrades = new();

    Canvas infoCanvas;
    TMPro.TMP_Text healthText;
    TMPro.TMP_Text dmgText;

    int _maxHealth;
    public int maxHealth {
        get {
            return _maxHealth;
        }

        private set
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

        private set
        {
            _health = value;
            if (healthText is not null)
            {
                healthText.SetText(health.ToString());
            }
            SetColour();
        }
    }

    int _damage;
    public int damage
    {
        get
        {
            return _damage;
        }

        private set
        {
            _damage = value;
            if (dmgText is not null)
            {
                dmgText.SetText(damage.ToString());
            }
        }
    }

    Color healthyColour = Color.green;
    Color deadColour = Color.red;

    Color enemyHealthyColour = Color.magenta;
    Color enemyDeadColour = Color.black;

    Color ghostColour = Color.grey;
    Color rootColour = Color.cyan;

    float lineWidth = 0.1f;

    public bool enemy { get; private set; }

    private void Update()
    {
        if (enemy)
        {
            infoCanvas.transform.rotation = Quaternion.identity;
        }
    }

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        polygonCollider = GetComponent<PolygonCollider2D>();
        polygonRigidbody = GetComponent<Rigidbody2D>();
        gameManager = Helpers.GameManager();
        var canvasObj = gameObject.transform.Find("Canvas");
        infoCanvas = canvasObj.GetComponent<Canvas>();
        healthText = canvasObj.transform.Find("Health Text").GetComponent<TMPro.TMP_Text>();
        dmgText = canvasObj.transform.Find("Damage Text").GetComponent<TMPro.TMP_Text>();
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

    void SetRelativePoints()
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
            polygonRigidbody.position = polygon.centroid;
        } else
        {
            var oldRigidbody = polygonRigidbody;
            polygonRigidbody = null;
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

    void Draw()
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

    public bool TakeDamage(int damageTaken)
    {
        health -= damageTaken;
        return IsDead();
    }

    public bool IsDead()
    {
        return health <= 0;
    }

    void ApplyReinforce()
    {
        maxHealth += 2;
        health += 2;
    }

    public void ApplyUpgrade(Upgrade upgrade)
    {
        if (appliedUpgrades.Contains(upgrade.name)) return;
        switch(upgrade.name)
        {
            case UpgradeManager.UpgradeName.heartbeat:
                break;
            case UpgradeManager.UpgradeName.cooperation:
                break;
            case UpgradeManager.UpgradeName.reinforce:
                ApplyReinforce();
                break;
            case UpgradeManager.UpgradeName.warehouse:
                break;
            default:
                break;
        }
        appliedUpgrades.Add(upgrade.name);
    }

    public void ApplyAllUpgrades()
    {
        foreach(var upgrade in UpgradeManager.GetUpgradeList())
        {
            ApplyUpgrade(upgrade);
        }
    }
}
