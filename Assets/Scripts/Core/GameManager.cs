using UnityEngine;
using System;
using Zenject;
using MergeGame.Systems;
using MergeGame.Grid;
using MergeGame.Entities;
using MergeGame.Data;
using UnityEngine.InputSystem;
using MergeGame.MergeDebug;


namespace MergeGame.Core
{
    public class GameManager : MonoBehaviour
    {
        public event Action OnGameStarted;
        public event Action OnPlayAgain;

        private CollectionManager _collectionManager;
        private GridManager _gridManager;
        private GameConfig _gameConfig;
        private ItemManager _itemManager;
        private SpawnerManager _spawnerManager;

        [Inject]
        public void Construct(CollectionManager collectionManager, GridManager gridManager, GameConfig gameConfig, ItemManager itemManager, SpawnerManager spawnerManager)
        {
            _collectionManager = collectionManager;
            _gridManager = gridManager;
            _gameConfig = gameConfig;
            _itemManager = itemManager;
            _spawnerManager = spawnerManager;
        }

        private void Awake()
        {
            Application.targetFrameRate = _gameConfig.targetFrameRate;
        }

        private void Start()
        {
            _collectionManager.OnWinConditionMet += GameOver;
        }

        private void Update()
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
#if !UNITY_WEBGL
                Application.Quit();
#endif
            }
        }

        private void OnDestroy()
        {
            if (_collectionManager != null)
            {
                _collectionManager.OnWinConditionMet -= GameOver;
            }
        }

        /// <summary>
        /// Starts the game by loading the grid and placing the initial spawner.
        /// </summary>
        public void StartGame()
        {
            if (!LoadGame())
            {
                return;
            }
            
            _gridManager.SetGameObjectsActive(true);
            OnGameStarted?.Invoke();
        }

        /// <summary>
        /// Loads the game by generating the grid and placing the initial spawner.
        /// </summary>
        /// <returns>True if the game was loaded successfully, false otherwise.</returns>
        public bool LoadGame()
        {
            if (!_gridManager.GenerateGrid())
            {
                DebugController.LogError("Failed to generate grid. Aborting game start.");
                return false;
            }

            _spawnerManager.PlaceInitialSpawner();
            return true;
        }

        /// <summary>
        /// Resets the game by clearing items and the grid, then reloads.
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

            DebugController.Log("Game Over!");
        }
    }
}