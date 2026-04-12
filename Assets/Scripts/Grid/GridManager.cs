using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Zenject;
using MergeGame.Data;


namespace MergeGame.Grid
{
    public class GridManager : MonoBehaviour
    {
        [Header("Grid Settings")]
        [SerializeField] private int _width = 6;
        [SerializeField] private int _height = 6;
        [SerializeField] private Tile _tilePrefab;
        [SerializeField] private Transform _camera;

        [Header("Tile Size Constraints")]
        [SerializeField] private float _maxTileSize = 1.5f;
        [SerializeField] private float _minTileSize = 0.5f;
        [SerializeField] [Range(0f, 1f)] private float _gridScreenCoverage = 0.9f;


        private Dictionary<Vector2Int, Tile> _tiles;
        private GameObject _tilesParent;
        private ObjectPool<Tile> _tilePool;
        private List<Vector2Int> _freeTilePositions = new List<Vector2Int>();
        private float _tileSize;
        private Vector2 _startPos;
        private GameConfig _gameConfig;

        public Dictionary<Vector2Int, Tile> Tiles => _tiles;
        public IReadOnlyList<Vector2Int> FreeTilePositions => _freeTilePositions;

        [Inject]
        public void Construct(GameConfig gameConfig)
        {
            _gameConfig = gameConfig;
        }

        private void Awake() => Initialize();

        public void Initialize()
        {
            _tilesParent = new GameObject(_gameConfig.tilesParentName);

            _tilePool = new ObjectPool<Tile>(
                createFunc: () => Instantiate(_tilePrefab, _tilesParent.transform),
                actionOnGet: tile => tile.gameObject.SetActive(true),
                actionOnRelease: tile => tile.gameObject.SetActive(false),
                actionOnDestroy: tile => Destroy(tile.gameObject)
            );
        }

        /// <summary>
        /// Generates the grid by creating tiles and setting their positions.
        /// </summary>
        /// <returns>True if the grid was generated successfully, false otherwise.</returns>
        public bool GenerateGrid()
        {
            Camera mainCam = Camera.main;
            if (mainCam == null)
            {
                Debug.LogError("No MainCamera found. Make sure your camera is tagged as MainCamera");
                return false;
            }

            if (_tilePrefab == null)
            {
                Debug.LogWarning("No tilePrefab assigned!");
                return false;
            }

            if (_tiles == null)
            {
                _tiles = new Dictionary<Vector2Int, Tile>();
            }
            else
            {
                _tiles.Clear();
            }

            // clear free positions to avoid duplicates on regeneration
            _freeTilePositions.Clear();

            // Pool existing tiles: SetActive(false) is handled by actionOnRelease
            foreach (Transform child in _tilesParent.transform)
            {
                Tile tile = child.GetComponent<Tile>();
                if (tile != null)
                {
                    _tilePool.Release(tile);
                }
            }

            float screenHeight = mainCam.orthographicSize * 2f;
            float screenWidth = screenHeight * mainCam.aspect;

            float tileSize = Mathf.Min((screenWidth * _gridScreenCoverage) / _width,
                                    (screenHeight * _gridScreenCoverage) / _height);
            tileSize = Mathf.Clamp(tileSize, _minTileSize, _maxTileSize);

            float gridWidth = _width * tileSize;
            float gridHeight = _height * tileSize;
            Vector2 startPos = new Vector2(-gridWidth * 0.5f + tileSize * 0.5f,
                                        -gridHeight * 0.5f + tileSize * 0.5f);

            _tileSize = tileSize;
            _startPos = startPos;

            Vector3 tileScale = new Vector3(tileSize, tileSize, 1f);

            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    Tile tile = _tilePool.Get();
                    tile.transform.localPosition = new Vector3(startPos.x + x * tileSize, startPos.y + y * tileSize, 0);
                    tile.transform.localScale = tileScale;
                    tile.Init((x % 2 != y % 2));

    #if UNITY_EDITOR
                    tile.gameObject.name = $"Tile {x} {y}";
    #endif

                    Vector2Int gridPos = new Vector2Int(x, y);
                    _tiles[gridPos] = tile;
                    tile.SetGridPosition(gridPos, this);
                    _freeTilePositions.Add(gridPos);
                }
            }

            if (_camera != null)
            {
                _camera.position = new Vector3(startPos.x + (gridWidth - tileSize) * 0.5f,
                                            startPos.y + (gridHeight - tileSize) * 0.5f,
                                            _camera.position.z);
            }

            return true;
        }

        public Tile GetTileAtPosition(Vector2Int pos)
        {
            _tiles.TryGetValue(pos, out Tile tile);
            return tile;
        }

        /// <summary>
        /// Gets the tile at the given world position.
        /// </summary>
        /// <param name="worldPos">The world position.</param>
        /// <returns>The tile at the given world position, or null if the position is outside the grid.</returns>
        public Tile GetTileAtWorldPosition(Vector3 worldPos)
        {
            if (_tileSize <= 0f)
            {
                return null;
            }

            int x = Mathf.RoundToInt((worldPos.x - _startPos.x) / _tileSize);
            int y = Mathf.RoundToInt((worldPos.y - _startPos.y) / _tileSize);

            // clamp to grid bounds instead of returning null on edges
            x = Mathf.Clamp(x, 0, _width - 1);
            y = Mathf.Clamp(y, 0, _height - 1);

            return GetTileAtPosition(new Vector2Int(x, y));
        }

        /// <summary>
        /// Marks a tile as occupied.
        /// </summary>
        /// <param name="gridPos">The grid position.</param>
        public void MarkTileOccupied(Vector2Int gridPos)
        {
            _freeTilePositions.Remove(gridPos);
        }

        /// <summary>
        /// Marks a tile as free.
        /// </summary>
        /// <param name="gridPos">The grid position.</param>
        public void MarkTileFree(Vector2Int gridPos)
        {
            if (!_freeTilePositions.Contains(gridPos))
            {
                _freeTilePositions.Add(gridPos);
            }
        }

        /// <summary>
        /// Sets the active state of the grid game objects.
        /// </summary>
        /// <param name="active">Whether the game objects should be active.</param>
        public void SetGameObjectsActive(bool active)
        {
            _tilesParent.SetActive(active);
        }

        /// <summary>
        /// Clears the grid by destroying spawners. Items are handled by ItemManager.
        /// </summary>
        public void ClearGrid()
        {
            if (_tiles == null)
            {
                return;
            }

            foreach (Tile tile in _tiles.Values)
            {
                if (tile == null)
                {
                    continue;
                }

                if (tile.HasSpawner())
                {
                    // Spawners are not pooled yet, there is only one. Destroy directly.
                    // TODO: pool spawners when multiple spawner support is added.
                    Destroy(tile.GetSpawner().gameObject);
                }
            }
        }
    }
}