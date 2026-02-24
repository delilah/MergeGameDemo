using UnityEngine;
using System;
using Zenject;


public class GameManager : MonoBehaviour
{    
    [SerializeField] private Spawner _spawnerPrefab; // generic prefab
    [SerializeField] private SpawnerData _testSpawnerData;
    [SerializeField] private MergeItemData[] _testItems; // ScriptableObjects for items

    public event Action OnGameStarted;
    public event Action OnPlayAgain;

    private CollectionManager _collectionManager;
    private GridManager _gridManager;
    private DiContainer _container;

    [Inject]
    public void Construct(CollectionManager collectionManager, GridManager gridManager, DiContainer container)
    {
        _collectionManager = collectionManager;
        _gridManager = gridManager;
        _container = container;
    }

    private void Awake()
    {
        Application.targetFrameRate = 60;
    }

    void Start()
    {
        _collectionManager.OnGameOver += GameOver;
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

        Spawner spawner = _container.InstantiatePrefabForComponent<Spawner>(_spawnerPrefab);
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
        _collectionManager.ResetAllCounts();
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
        if (_collectionManager != null)
            _collectionManager.OnGameOver -= GameOver;
    }
}