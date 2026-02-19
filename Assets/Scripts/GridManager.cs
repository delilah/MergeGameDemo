using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

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
    private Stack<Tile> _tilePool = new Stack<Tile>(); // pooling
    private const string TILES_PARENT_NAME = "Tiles";
    private const string ITEMS_PARENT_NAME = "Items";
    private List<Vector2Int> _freeTilePositions = new List<Vector2Int>();
    private Stack<Item> _itemPool = new Stack<Item>(); //pooling
    private GameObject _itemsParent;

    private float _tileSize;
    private Vector2 _startPos;

    public Dictionary<Vector2Int, Tile> Tiles => _tiles;
    public IReadOnlyList<Vector2Int> FreeTilePositions => _freeTilePositions;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        _tilesParent = new GameObject(TILES_PARENT_NAME);
        _itemsParent = new GameObject(ITEMS_PARENT_NAME);
    }

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

        if (_tiles == null) _tiles = new Dictionary<Vector2Int, Tile>();
        else _tiles.Clear();

        // clear free positions to avoid duplicates on regeneration
        _freeTilePositions.Clear();

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

        float screenHeight = mainCam.orthographicSize * 2f;
        float screenWidth = screenHeight * mainCam.aspect;

        float tileSize = Mathf.Min((screenWidth * 0.9f) / _width,
                                   (screenHeight * 0.9f) / _height);
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

        return true;
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
        if (_tileSize <= 0f) return null;

        int x = Mathf.RoundToInt((worldPos.x - _startPos.x) / _tileSize);
        int y = Mathf.RoundToInt((worldPos.y - _startPos.y) / _tileSize);

        return GetTileAtPosition(new Vector2Int(x, y));
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

        Item newItem = GetItemFromPool();
        newItem.Initialize(data, tile);
        return newItem;
    }

    private Item GetItemFromPool()
    {
        if (_itemPool.Count > 0)
        {
            Item item = _itemPool.Pop();
            item.gameObject.SetActive(true);
            return item;
        }

        Debug.Assert(_itemsParent != null, "ItemsParent is null — was Awake called?");

        return Instantiate(_itemPrefab, _itemsParent.transform);
    }

    public void ReturnItemToPool(Item item)
    {
        if (item == null) return;

        item.gameObject.SetActive(false);
        item.transform.SetParent(_itemsParent != null ? _itemsParent.transform : transform);
        _itemPool.Push(item);
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
