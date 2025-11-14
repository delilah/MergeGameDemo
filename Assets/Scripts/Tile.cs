// A tile is a cell, a square of the grid. 
// Tile has colour and is free or occupied.
// Objects on a tile are Items. They are draggable.


using UnityEngine;

public class Tile : MonoBehaviour
{
    [Header("Tile Settings")]
    [SerializeField] private Color _baseColor;
    [SerializeField] private Color _offsetColor;
    [SerializeField] private SpriteRenderer _renderer;
    [SerializeField] private GameObject _highlight;

    private Item _currentItem;
    private Spawner _currentSpawner;
    private const float _spritePadding = .95f; // 1: aspect ratio tile, 1.5: tile has a bigger transparent bg so we enlarge it to show it fully in the tile
    private Vector2Int _gridPosition;
    private GridManager _gridManager;


    public bool HasItem() => _currentItem != null;
    public Item GetItem() => _currentItem;
    public bool HasSpawner() => _currentSpawner != null;
    public Spawner GetSpawner() => _currentSpawner;

    public bool IsEmpty => _currentSpawner == null;

    public void Init(bool isAlternateTile)
    {
        _renderer.color = isAlternateTile ? _offsetColor : _baseColor;
    }

    public void SetGridPosition(Vector2Int gridPos, GridManager gridManager)
    {
        _gridPosition = gridPos;
        _gridManager = gridManager;
    }

    public void SetHighlight(bool on)
    {
        if (_highlight == null)
        {
            Debug.Log($"[Tile] SetHighlight called but _highlight is null on {name}");
            return;
        }
        bool before = _highlight.activeSelf;
        _highlight.SetActive(on);
        bool after = _highlight.activeSelf;
        Debug.Log($"[Tile] SetHighlight({on}) on {name} | before={before} after={after}");
    }

    public void PlaceSpawner(Spawner spawner)
    {
        _currentSpawner = spawner;
        if (spawner == null)
        {
            Debug.Log("Spawner is null, cannot place.");
            return;
        }

        PlaceObject(spawner.transform, Vector3.zero);

        // Spawner-specific logic as we need the tile for the highlight
        spawner.SetTile(this);
        AdjustSortingAboveTile(spawner.GetComponent<SpriteRenderer>());

        if (_gridManager != null)
        {
            _gridManager.MarkTileOccupied(_gridPosition);
        }

        Debug.Log($"Spawner {spawner.name} placed at local position {spawner.transform.localPosition}");
    }

    public void PlaceItem(Item item)
    {
        _currentItem = item;
        if (item == null) return;

        PlaceObject(item.transform, new Vector3(0, 0.05f, 0));
        AdjustSortingAboveTile(item.GetComponent<SpriteRenderer>());

        if (_gridManager != null)
        {
            _gridManager.MarkTileOccupied(_gridPosition);
        }
    }

    private void PlaceObject(Transform objTransform, Vector3 localOffset)
    {
        objTransform.SetParent(transform);
        objTransform.localPosition = localOffset;

        ScaleToTile(objTransform);
    }

    private void ScaleToTile(Transform objTransform)
    {
        objTransform.localScale = Vector3.one;

        float tileSize = transform.localScale.x;

        SpriteRenderer sr = objTransform.GetComponent<SpriteRenderer>();
        if (sr == null || sr.sprite == null)
            return;

        Vector2 spriteSize = sr.sprite.bounds.size;
        float targetSize = tileSize * _spritePadding;
        float scaleFactor = targetSize / Mathf.Max(spriteSize.x, spriteSize.y);

        objTransform.localScale = new Vector3(scaleFactor, scaleFactor, 1f);
    }

    private void AdjustSortingAboveTile(SpriteRenderer sr)
    {
        if (sr == null) return;

        sr.sortingLayerID = _renderer.sortingLayerID;
        sr.sortingOrder   = _renderer.sortingOrder + 1;
    }

    public void RemoveItem()
    {
        _currentItem = null;
        if (_gridManager != null && !HasSpawner())
        {
            _gridManager.MarkTileFree(_gridPosition);
        }
    }

    public Vector2Int GridPosition => _gridPosition;


}
