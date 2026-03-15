using UnityEngine;
using System;
using Zenject;
using MergeGame.Systems;
using MergeGame.Grid;
using MergeGame.Entities;
using MergeGame.Data;

namespace MergeGame.Core
{
    public class GameManager : MonoBehaviour
    {    
        [SerializeField] private Spawner _spawnerPrefab; // generic prefab
        [SerializeField] private SpawnerData _initialSpawnerData;

        public event Action OnGameStarted;
        public event Action OnPlayAgain;

        private CollectionManager _collectionManager;
        private GridManager _gridManager;
        private DiContainer _container;
        private GameConfig _gameConfig;
        private ItemManager _itemManager;

        [Inject]
        public void Construct(CollectionManager collectionManager, GridManager gridManager, DiContainer container, GameConfig gameConfig, ItemManager itemManager)
        {
            _collectionManager = collectionManager;
            _gridManager = gridManager;
            _container = container;
            _gameConfig = gameConfig;
            _itemManager = itemManager;
        }

        private void Awake()
        {
            Application.targetFrameRate = 60;
        }

        private void Start()
        {
            _collectionManager.OnWinConditionMet += GameOver;
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

        /// <summary>
        /// Places the test spawner at the configured position.
        /// </summary>
        private void PlaceInitialSpawner()
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
            spawner.Initialize(_initialSpawnerData, true);

            tile.PlaceSpawner(spawner);
        }

        /// <summary>
        /// Starts the game by loading the grid and placing the test spawner.
        /// </summary>
        public void StartGame()
        {
            if (!LoadGame()) return;
            _gridManager.SetGameObjectsActive(true);
            OnGameStarted?.Invoke();
        }

        /// <summary>
        /// Loads the game by generating the grid and placing the test spawner.
        /// </summary>
        /// <returns>True if the game was loaded successfully, false otherwise.</returns>
        public bool LoadGame()
        {
            if (_spawnerPrefab == null)
            {
                Debug.LogError("GameManager: _spawnerPrefab is not assigned. Aborting game start.");
                return false;
            }

            if (_initialSpawnerData == null)
            {
                Debug.LogError("GameManager: _initialSpawnerData is not assigned. Aborting game start.");
                return false;
            }

            if (!_gridManager.GenerateGrid())
            {
                Debug.LogError("Failed to generate grid. Aborting game start.");
                return false;
            }

            PlaceInitialSpawner();
            return true;
        }

        /// <summary>
        /// Resets the game by clearing the grid and resetting all counts.
        /// </summary>
        public void PlayAgain()
        {
            _collectionManager.Reset();
            _itemManager.ClearAll();  // return all items to pool first
            _gridManager.ClearGrid(); // then clear the grid
            OnPlayAgain?.Invoke();
        }

        /// <summary>
        /// Handles the game over event by deactivating game objects.
        /// </summary>
        public void GameOver()
        {
            _gridManager.SetGameObjectsActive(false);
            Debug.Log("Game Over!");
        }

        private void OnDestroy()
        {
            if (_collectionManager != null)
            {
                _collectionManager.OnWinConditionMet -= GameOver;
            }
        }
    }
}