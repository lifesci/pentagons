using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{
    PolygonFactory polygonFactory;
    EnemySpawner enemySpawner;

    // root polygon spawn points
    readonly Vector2 p0 = new(0, 0);
    readonly Vector2 p1 = new(1, 0);

    // free build time before enemies begin to spawn
    const int spawnDelay = 3;

    // all player polygons
    HashSet<PolygonPrefab> polygonClones = new();

    // all unique polygon lines
    HashSet<Line> linesSet = new();

    // counts of geometrically unique lines
    Dictionary<Vector2, int> linesCount = new();

    // contains collisions that occurred each frame, handled in update
    Dictionary<PolygonPrefab, HashSet<EnemyPrefab>> collisionRecord = new();

    // root polygon reference
    public PolygonPrefab root { get; private set; }

    // active ghost
    PolygonPrefab ghost;
    Line ghostLine;

    // player polygon vertices
    int vertices;

    // player level and experience
    int level = 1;
    int xp = 0;

    // current round
    int round = 0;

    // flags to start and initialize rounds
    bool startRound = false;
    bool roundInitialized = false;

    // number of active enemies
    int enemyCount = 0;

    // paused and game over flags
    bool paused;
    bool gameOver;
    bool calledGameOver;

    // number of polygons available to the player
    int _inventory;
    int inventory {
        get => _inventory;
        set
        {
            _inventory = value;
            if (inventoryText is not null)
            {
                inventoryText.SetText("Inventory: " + _inventory);
            }
        }
    }

    // in-game menu objects and controls
    [SerializeField] GameObject gameOverMenu;
    [SerializeField] GameObject pauseMenu;
    Button gameOverMainMenuButton;
    Button pauseMainMenuButton;
    Button resumeButton;
    TMPro.TMP_Text scoreText;

    // HUD objects
    TMPro.TMP_Text inventoryText;
    TMPro.TMP_Text roundText;
    Button startRoundButton;

    void Start()
    {
        // get menu objects
        gameOverMainMenuButton = GameObject.Find("Game Over Main Menu Button").GetComponent<Button>();
        pauseMainMenuButton = GameObject.Find("Pause Main Menu Button").GetComponent<Button>();
        resumeButton = GameObject.Find("Resume Button").GetComponent<Button>();
        scoreText = GameObject.Find("Score Text").GetComponent<TMPro.TMP_Text>();

        // get HUD objects
        inventoryText = GameObject.Find("Inventory Text").GetComponent<TMPro.TMP_Text>();
        startRoundButton = GameObject.Find("Start Round Button").GetComponent<Button>();

        // set button actions
        resumeButton.onClick.AddListener(UnPause);
        pauseMainMenuButton.onClick.AddListener(GoToMainMenu);
        gameOverMainMenuButton.onClick.AddListener(GoToMainMenu);
        startRoundButton.onClick.AddListener(StartRound);

        // init paused and game over flags
        paused = false;
        gameOver = false;
        calledGameOver = false;

        // deactivate paused and game over menus
        gameOverMenu.SetActive(gameOver);
        pauseMenu.SetActive(paused);

        // set vertices from menu selection; default to 5
        vertices = MainManager.Instance is null ? 5 : MainManager.Instance.polygonSize;

        // get polygon factory and enemy spawner
        polygonFactory = gameObject.GetComponent<PolygonFactory>();
        enemySpawner = gameObject.GetComponent<EnemySpawner>();

        // add root polygon
        AddRootPolygon(p0, p1);
    }

    void Update()
    {
        if (!(paused || gameOver))
        {
            HandleCollisions();
            if (
                Input.GetMouseButtonDown(0)
                && inventory > 0
                && EventSystem.current.currentSelectedGameObject != startRoundButton.gameObject
            )
            {
                AddPolygon();
                inventory--;
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
            CheckStartRound();
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

        if (gameOver && !calledGameOver)
        {
            GameOver();
            calledGameOver = true;
        }
    }

    void GameOver()
    {
        Time.timeScale = 0;
        gameOverMenu.SetActive(true);
        scoreText.SetText("reached round " + round);
    }

    void Pause()
    {
        Time.timeScale = 0;
        pauseMenu.SetActive(true);
        paused = true;
        if (startRoundButton.IsActive())
        {
            startRoundButton.interactable = false;
        }
    }

    void UnPause()
    {
        Time.timeScale = Helpers.timeScale;
        pauseMenu.SetActive(false);
        paused = false;
        if (startRoundButton.IsActive())
        {
            startRoundButton.interactable = true;
        }
    }

    void GoToMainMenu()
    {
        Time.timeScale = Helpers.timeScale;
        SceneManager.LoadScene("Menu Scene");
    }

    void CheckStartRound()
    {
        if (startRound)
        {
            startRoundButton.gameObject.SetActive(false);
            var enemyVertices = EnemyVertices();
            enemyCount = enemySpawner.SpawnRandom(enemyVertices);
            startRound = false;
            roundInitialized = false;
        }
        else if (!roundInitialized && enemyCount == 0 && !gameOver)
        {
            round++;
            var enemyVertices = EnemyVertices();
            inventory += enemyVertices / vertices + 1;
            startRoundButton.gameObject.SetActive(true);
            if (xp >= XPToNextLevel())
            {
                LevelUp();
            }
            roundInitialized = true;
        }
    }

    void StartRound()
    {
        startRound = true;
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
            xp += enemy.polygon.vertices;
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

    int EnemyVertices()
    {
        int roundSquare = Mathf.Max(round - 3, 0);
        return 20 * round + (roundSquare)*(roundSquare);
    }

    int XPToNextLevel()
    {
        var nextLevel = level + 1;
        return nextLevel * nextLevel + 40 * nextLevel;
    }

    void LevelUp()
    {

    }
}
