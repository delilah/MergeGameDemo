using System.Collections.Generic;
using UnityEngine;

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

    [Header("Item Prefab")]
    [SerializeField] private Item _itemPrefab;

    private Dictionary<Vector2Int, Tile> _tiles;
    private GameObject _tilesParent;
    private Stack<Tile> _tilePool = new Stack<Tile>();
    private const string TILES_PARENT_NAME = "Tiles";
    private List<Vector2Int> _freeTilePositions = new List<Vector2Int>();

    public Dictionary<Vector2Int, Tile> Tiles => _tiles;
    public IReadOnlyList<Vector2Int> FreeTilePositions => _freeTilePositions;

    public void GenerateGrid()
    {
        if (_tilePrefab == null)
        {
            Debug.LogWarning("No tilePrefab assigned!");
            return;
        }

        if (_tiles == null) _tiles = new Dictionary<Vector2Int, Tile>();
        else _tiles.Clear();

        if (_tilesParent == null) _tilesParent = new GameObject(TILES_PARENT_NAME);

        // Pool existing tiles
        foreach (Transform child in _tilesParent.transform)
        {
            Tile tile = child.GetComponent<Tile>();
            if (tile != null)
            {
                tile.gameObject.SetActive(false);
                _tilePool.Push(tile);
            }
        }

        Camera mainCam = Camera.main ?? Camera.current;

        float screenHeight = mainCam.orthographicSize * 2f;
        float screenWidth = screenHeight * mainCam.aspect;

        float tileSize = Mathf.Min((screenWidth * 0.9f) / _width,
                                   (screenHeight * 0.9f) / _height);
        tileSize = Mathf.Clamp(tileSize, _minTileSize, _maxTileSize);

        float gridWidth = _width * tileSize;
        float gridHeight = _height * tileSize;
        Vector2 startPos = new Vector2(-gridWidth * 0.5f + tileSize * 0.5f,
                                       -gridHeight * 0.5f + tileSize * 0.5f);

        Vector3 tileScale = new Vector3(tileSize, tileSize, 1f);

        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                Tile tile = GetOrCreateTile();
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
    }

    private Tile GetOrCreateTile()
    {
        while (_tilePool.Count > 0)
        {
            Tile tile = _tilePool.Pop();
            if (tile != null)
            {
                tile.gameObject.SetActive(true);
                return tile;
            }
        }
        return Instantiate(_tilePrefab, _tilesParent.transform);
    }

    public Tile GetTileAtPosition(Vector2Int pos)
    {
        _tiles.TryGetValue(pos, out Tile tile);
        return tile;
    }

    public Tile GetTileAtWorldPosition(Vector3 worldPos)
    {
        foreach (Tile tile in _tiles.Values)
        {
            Vector3 pos = tile.transform.position;
            float halfSize = tile.transform.localScale.x * 0.5f;

            if (worldPos.x >= pos.x - halfSize && worldPos.x <= pos.x + halfSize &&
                worldPos.y >= pos.y - halfSize && worldPos.y <= pos.y + halfSize)
            {
                return tile;
            }
        }
        return null;
    }

    // ---------- SPAWN ITEM ----------
    public Item SpawnItem(MergeItemData data, Vector2Int gridPos)
    {
        if (_itemPrefab == null)
        {
            Debug.LogWarning("No Item Prefab assigned!");
            return null;
        }

        Tile tile = GetTileAtPosition(gridPos);
        if (tile == null)
        {
            Debug.LogWarning($"No tile at {gridPos}");
            return null;
        }

        if (tile.HasItem())
        {
            Debug.LogWarning($"Tile at {gridPos} is already occupied");
            return null;
        }

        Item newItem = Instantiate(_itemPrefab);        // Instantiate prefab
        newItem.Initialize(data, tile);                 // Set data and tile
        return newItem;
    }

    public void MarkTileOccupied(Vector2Int gridPos)
    {
        _freeTilePositions.Remove(gridPos);
    }

    public void MarkTileFree(Vector2Int gridPos)
    {
        if (!_freeTilePositions.Contains(gridPos))
        {
            _freeTilePositions.Add(gridPos);
        }
    }
}
