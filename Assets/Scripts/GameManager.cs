using UnityEngine;
using System;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [SerializeField] private GridManager _gridManager;
    [SerializeField] private Spawner _spawnerPrefab; // generic prefab
    [SerializeField] private SpawnerData _testSpawnerData;
    [SerializeField] private MergeItemData[] _testItems; // ScriptableObjects for items

    public event Action OnGameStarted;
    public event Action OnPlayAgain;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        Application.targetFrameRate = 60;
    }

    void Start()
    {
        CollectionManager.Instance.OnGameOver += GameOver;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
#if !UNITY_WEBGL
            Application.Quit();
#endif
        }
    }

    private void PlaceTestSpawner()
    {
        Vector2Int pos = new Vector2Int(3, 3);
        Tile tile = _gridManager.GetTileAtPosition(pos);

        if (tile == null)
        {
            Debug.LogWarning("Tile 3,3 does not exist!");
            return;
        }

        Debug.Log($"Tile 3,3 found. HasItem={tile.HasItem()}, HasSpawner={tile.HasSpawner()}, IsEmpty={tile.IsEmpty}");

        if (!tile.IsEmpty)
        {
            Debug.LogWarning("Tile 3,3 is occupied!");
            return;
        }

        Spawner spawner = Instantiate(_spawnerPrefab);
        spawner.Initialize(_testSpawnerData, true);

        tile.PlaceSpawner(spawner);

        Debug.Log($"Spawner placed on Tile 3,3 at position {spawner.transform.position}");
    }


    public void StartGame()
    {
        LoadGame();
        _gridManager.SetGameObjectsActive(true);
        OnGameStarted?.Invoke();
    }

        public void LoadGame()
    {
        if (!_gridManager.GenerateGrid())
        {
            Debug.LogError("Failed to generate grid. Aborting game start.");
            return;
        }
        PlaceTestSpawner();
    }

    public void PlayAgain()
    {
        CollectionManager.Instance.ResetAllCounts();
        _gridManager.ClearGrid();
        OnPlayAgain?.Invoke();
    }

    public void GameOver()
    {
        _gridManager.SetGameObjectsActive(false);
        Debug.Log("Game Over!");
    }

    private void OnDestroy()
    {
        if (CollectionManager.Instance != null)
            CollectionManager.Instance.OnGameOver -= GameOver;
    }
}