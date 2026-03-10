using UnityEngine;
using System;
using Zenject;
using MergeGame.Systems;
using MergeGame.Grid;
using MergeGame.Entities;

namespace MergeGame.Core
{
        
    public class GameManager : MonoBehaviour
    {    
        [SerializeField] private Spawner _spawnerPrefab; // generic prefab
        [SerializeField] private SpawnerData _testSpawnerData;

        public event Action OnGameStarted;
        public event Action OnPlayAgain;

        private CollectionManager _collectionManager;
        private GridManager _gridManager;
        private DiContainer _container;
        private GameConfig _gameConfig;

        [Inject]
        public void Construct(CollectionManager collectionManager, GridManager gridManager, DiContainer container, GameConfig gameConfig)
        {
            _collectionManager = collectionManager;
            _gridManager = gridManager;
            _container = container;
            _gameConfig = gameConfig;

        }

        private void Awake()
        {
            Application.targetFrameRate = 60;
        }

        private void Start()
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
            Vector2Int pos = _gameConfig.spawnerPlacementStart;
            Tile tile = _gridManager.GetTileAtPosition(pos);

            if (tile == null)
            {
                Debug.LogWarning("Tile does not exist!");
                return;
            }

            Debug.Log($"Tile found. HasItem={tile.HasItem()}, HasSpawner={tile.HasSpawner()}, IsEmpty={tile.IsEmpty}");

            if (!tile.IsEmpty)
            {
                Debug.LogWarning("Tile is occupied!");
                return;
            }

            Spawner spawner = _container.InstantiatePrefabForComponent<Spawner>(_spawnerPrefab);
            spawner.Initialize(_testSpawnerData, true);

            tile.PlaceSpawner(spawner);
        }

        public void StartGame()
        {
            if (!LoadGame()) return;
            _gridManager.SetGameObjectsActive(true);
            OnGameStarted?.Invoke();
        }

        public bool LoadGame()
        {
            if (!_gridManager.GenerateGrid())
            {
                Debug.LogError("Failed to generate grid. Aborting game start.");
                return false;
            }
            PlaceTestSpawner();
            return true;
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
            {
                _collectionManager.OnGameOver -= GameOver;
            }
        }
    }
}