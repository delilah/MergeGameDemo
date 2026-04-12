using UnityEngine;
using Zenject;
using MergeGame.Grid;
using MergeGame.Data;
using MergeGame.MergeDebug;

namespace MergeGame.Entities
{
    public class SpawnerManager : MonoBehaviour
    {
        [SerializeField] private Spawner _spawnerPrefab;
        [SerializeField] private SpawnerData _initialSpawnerData;

        private Spawner _activeSpawner;

        private GridManager _gridManager;
        private DiContainer _container;
        private GameConfig _gameConfig;

        [Inject]
        public void Construct(GridManager gridManager, DiContainer container, GameConfig gameConfig)
        {
            _gridManager = gridManager;
            _container = container;
            _gameConfig = gameConfig;
        }

        /// <summary>
        /// Places the initial spawner at the configured position.
        /// </summary>
        public void PlaceInitialSpawner()
        {
            if (_spawnerPrefab == null)
            {
                DebugController.LogError("SpawnerManager: _spawnerPrefab is not assigned. Aborting spawner placement.");                
                return;
            }

            if (_initialSpawnerData == null)
            {
                DebugController.LogError("SpawnerManager: _initialSpawnerData is not assigned. Aborting spawner placement.");                
                return;
            }

            Vector2Int pos = _gameConfig.spawnerPlacementStart;
            Tile tile = _gridManager.GetTileAtPosition(pos);

            if (tile == null)
            {
                DebugController.LogWarning("SpawnerManager: Tile does not exist!");
                return;
            }

            if (!tile.IsEmpty)
            {
                DebugController.LogWarning("SpawnerManager: Tile is occupied!");                
                return;
            }

            Spawner spawner = _container.InstantiatePrefabForComponent<Spawner>(_spawnerPrefab);
            spawner.Initialize(_initialSpawnerData, true);
            tile.PlaceSpawner(spawner);
        }

        /// <summary>
        /// Sets the given spawner as the active (selected) spawner.
        /// Deselects the previously active spawner if any.
        /// </summary>
        /// <param name="spawner">The spawner to set as active.</param>
        public void SetActiveSpawner(Spawner spawner)
        {
            if (_activeSpawner != null && _activeSpawner != spawner)
            {
                _activeSpawner.SetSelected(false);
            }

            _activeSpawner = spawner;
        }

        /// <summary>
        /// Clears the active spawner if it matches the given spawner.
        /// </summary>
        /// <param name="spawner">The spawner to clear.</param>
        public void ClearActiveSpawner(Spawner spawner)
        {
            if (_activeSpawner == spawner)
            {
                _activeSpawner = null;
            }
        }

        /// <summary>
        /// Returns whether the given spawner is currently active.
        /// </summary>
        /// <param name="spawner">The spawner to check.</param>
        public bool IsActive(Spawner spawner) => _activeSpawner == spawner;
    }
}