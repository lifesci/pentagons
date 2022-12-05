using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    PolygonFactory polygonFactory;
    EnemySpawner enemySpawner;

    readonly Vector2 p0 = new(0, 0);
    readonly Vector2 p1 = new(1, 0);

    // free build time before enemies begin to spawn
    const int spawnDelay = 3;

    HashSet<PolygonPrefab> polygonClones = new();
    HashSet<Line> linesSet = new();
    Dictionary<Vector2, int> linesCount = new();

    Dictionary<PolygonPrefab, HashSet<EnemyPrefab>> collisionRecord = new();

    public PolygonPrefab root { get; private set; }

    PolygonPrefab ghost;
    Line ghostLine;

    int vertices;

    int level;
    bool startLevel;

    int enemyCount;

    bool paused;
    bool gameOver;

    float timeScale;

    // in-game menu objects and controls
    [SerializeField] GameObject gameOverMenu;
    [SerializeField] GameObject pauseMenu;
    Button gameOverMainMenuButton;
    Button pauseMainMenuButton;
    Button resumeButton;
    TMPro.TMP_Text scoreText;

    void Start()
    {
        // get menu objects
        gameOverMainMenuButton = GameObject.Find("Game Over Main Menu Button").GetComponent<Button>();
        pauseMainMenuButton = GameObject.Find("Pause Main Menu Button").GetComponent<Button>();
        resumeButton = GameObject.Find("Resume Button").GetComponent<Button>();
        scoreText = GameObject.Find("Score Text").GetComponent<TMPro.TMP_Text>();

        // set button actions
        resumeButton.onClick.AddListener(UnPause);
        pauseMainMenuButton.onClick.AddListener(GoToMainMenu);
        gameOverMainMenuButton.onClick.AddListener(GoToMainMenu);

        timeScale = Time.timeScale;
        paused = false;
        gameOver = false;

        gameOverMenu.SetActive(gameOver);
        pauseMenu.SetActive(paused);

        level = 1;
        startLevel = true;
        vertices = MainManager.Instance is null ? 5 : MainManager.Instance.polygonSize;
        polygonFactory = GameObject.Find("Polygon Factory").GetComponent<PolygonFactory>();
        enemySpawner = GameObject.Find("Enemy Spawner").GetComponent<EnemySpawner>();
        AddRootPolygon(p0, p1);
    }

    void Update()
    {
        if (!(paused || gameOver))
        {
            HandleCollisions();
            if (Input.GetMouseButtonDown(0))
            {
                AddPolygon();
                DestroyGhost();
            }
            else if (Input.GetMouseButtonDown(1))
            {
                DeleteClosestPolygon();
                DestroyGhost();
            } else
            {
                AddGhostPolygon();
            }
            CheckStartLevel();
        }

        if (!gameOver && Input.GetKeyDown(KeyCode.Escape))
        {
            if (!paused)
            {
                Pause();
            } else
            {
                UnPause();
            }
        }

        if (gameOver) GameOver();
    }

    void GameOver()
    {
        Time.timeScale = 0;
        gameOverMenu.SetActive(true);
        scoreText.SetText("score: 0");
    }

    void Pause()
    {
        Time.timeScale = 0;
        pauseMenu.SetActive(true);
        paused = true;
    }

    void UnPause()
    {
        Time.timeScale = timeScale;
        pauseMenu.SetActive(false);
        paused = false;
    }

    void GoToMainMenu()
    {
        SceneManager.LoadScene("Menu Scene");
    }

    void CheckStartLevel()
    {
        if (startLevel)
        {
            Debug.Log("Starting Level " + level);
            var enemyVertices = EnemyVertices(level);
            enemyCount = enemySpawner.SpawnRandom(enemyVertices, 3);
            startLevel = false;
            level++;
        }
        else if (!startLevel && enemyCount == 0)
        {
            startLevel = true;
        }
    }

    void AddPolygon()
    {
        // do nothing if ghost is not set
        if (ghost is null) return;

        // convert ghost to placed polygon
        var newPrefab = ghost;
        UnsetGhost();
        AddPolygonLines(newPrefab);
        newPrefab.UnGhost();
    }

    void AddRootPolygon(Vector2 p0, Vector2 p1)
    {
        var polygonPrefab = polygonFactory.Create(vertices, p0, p1, linesSet, root: true);
        polygonPrefab.polygon.SetRoot(true);
        AddPolygonLines(polygonPrefab);
        root = polygonPrefab;
    }

    void AddPolygonLines(PolygonPrefab polygonPrefab)
    {
        polygonClones.Add(polygonPrefab);
        var polygon = polygonPrefab.polygon;
        var lines = polygon.lines;
        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            var countKey = line.midpoint;
            linesSet.Add(line);
            if (!linesCount.ContainsKey(countKey)) linesCount[countKey] = 0;
            linesCount[countKey]++;
        }
    }

    void RemovePolygonLines(Polygon polygon)
    {
        var lines = polygon.lines;
        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            var countKey = line.midpoint;
            linesSet.Remove(line);
            linesCount[countKey]--;
            if (linesCount[countKey] == 0)
            {
                linesCount.Remove(countKey);
            }
        }
    }

    void RemovePolygonNeighbours(Polygon polygon)
    {
        foreach(var neighbour in polygon.neighbours)
        {
            neighbour.RemoveNeighbour(polygon);
        }
    }

    void DeleteClosestPolygon()
    {
        var mousePos = GetMousePos();
        PolygonPrefab prefabToRemove = null;
        foreach (var polygonPrefab in polygonClones)
        {
            var polygon = polygonPrefab.polygon;
            if (polygon.ContainsPoint(mousePos))
            {
                prefabToRemove = polygonPrefab;
                break;
            }
        }

        // root object cannot be removed manually
        if (prefabToRemove is not null && !prefabToRemove.polygon.root)
        {
            DeletePolygon(prefabToRemove);
        }

        // remove any polygons not reachable from the root
        RemoveUnreachable();
    }

    void DeletePolygon(PolygonPrefab prefab)
    {
        polygonClones.Remove(prefab);
        RemovePolygonLines(prefab.polygon);
        RemovePolygonNeighbours(prefab.polygon);
        Destroy(prefab.gameObject);
    }

    void DestroyGhost()
    {
        if (ghost is not null)
        {
            var oldGhost = ghost;
            ghost = null;
            ghostLine = null;
            Destroy(oldGhost.gameObject);
        }
    }

    void UnsetGhost()
    {
        ghost = null;
        ghostLine = null;
    }

    void AddGhostPolygon()
    {
        var mousePos = GetMousePos();
        var line = Helpers.ClosestFreeLine(mousePos, linesSet, linesCount);
        if (line is null || line == ghostLine) return;
        DestroyGhost();
        var ghostPolygon = polygonFactory.CreateVirtualGhost(vertices, line, linesSet);
        foreach (var polygonPrefab in polygonClones)
        {
            if (ghostPolygon.Intersects(polygonPrefab.polygon)) return;
        }
        ghost = polygonFactory.CreateGhost(ghostPolygon);
        ghostLine = line;
    }

    Vector2 GetMousePos()
    {
        var mousePos3D = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mousePos = new(mousePos3D.x, mousePos3D.y);
        return mousePos;
    }

    void RemoveUnreachable()
    {
        var reachablePolygons = GetReachablePolygons();
        List<PolygonPrefab> toRemove = new();
        foreach (var polygonPrefab in polygonClones)
        {
            if (!reachablePolygons.Contains(polygonPrefab.polygon))
            {
                toRemove.Add(polygonPrefab);
            }
        }

        foreach (var prefab in toRemove)
        {
            DeletePolygon(prefab);
        }
    }

    HashSet<Polygon> GetReachablePolygons()
    {
        // get set of polygons reachable from the root using BFS
        Queue<Polygon> toVisit = new();
        HashSet<Polygon> visited = new();

        toVisit.Enqueue(root.polygon);

        while(toVisit.Count > 0)
        {
            var curPolygon = toVisit.Dequeue();
            visited.Add(curPolygon);
            foreach (var neighbour in curPolygon.neighbours)
            {
                if (!visited.Contains(neighbour))
                {
                    toVisit.Enqueue(neighbour);
                }
            }
        }

        return visited;
    }

    void HandleCollisions()
    {
        Queue<PolygonPrefab> deadPolygons = new();

        HashSet<EnemyPrefab> deadEnemiesSet = new();
        Queue<EnemyPrefab> deadEnemies = new();

        foreach (var collision in collisionRecord)
        {
            var polygon = collision.Key;
            foreach (var enemy in collision.Value)
            {
                // ignore dead blocks
                if (polygon.IsDead()) break;
                if (enemy.IsDead()) continue;

                // determine collision damage
                polygon.TakeDamage(enemy.polygon.vertices);
                enemy.TakeDamage(polygon.polygon.vertices);

                // keep track of dead entities
                if (enemy.IsDead() && !deadEnemiesSet.Contains(enemy))
                {
                    deadEnemiesSet.Add(enemy);
                    deadEnemies.Enqueue(enemy);
                }
            }
            if (polygon.IsDead()) deadPolygons.Enqueue(polygon);
        }

        // reset collision record for next frame;
        collisionRecord.Clear();

        // destroy enemies
        enemyCount -= deadEnemies.Count;
        while (deadEnemies.Count > 0)
        {
            var enemy = deadEnemies.Dequeue();
            Destroy(enemy.gameObject);
        }

        var deletedPolygons = deadPolygons.Count > 0;

        // destroy polygons
        while (deadPolygons.Count > 0)
        {
            var polygon = deadPolygons.Dequeue();
            if (polygon.polygon.root)
            {
                Debug.Log("Game Over");
                gameOver = true;
            }
            else
            {
                DeletePolygon(polygon);
            }
        }

        if (deletedPolygons)
        {
            RemoveUnreachable();
            DestroyGhost();
        }
    }

    public void RecordCollision(PolygonPrefab polygon, EnemyPrefab enemy)
    {
        if (!collisionRecord.ContainsKey(polygon)) collisionRecord[polygon] = new();
        collisionRecord[polygon].Add(enemy);
    }

    int EnemyVertices(int level)
    {
        return 20 * level;
    }
}
