using UnityEngine;
using UnityEngine.EventSystems;

public class Item : MonoBehaviour, IDraggable, IPointerDownHandler
{
    [Header("References")]
    [SerializeField] private GridManager _gridManager;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private BoxCollider2D _collider;

    [Header("Item Data")]
    [SerializeField] private MergeItemData _data;
    public MergeItemData Data => _data;

    private Tile _currentTile;
    private Vector3 _startPosition;
    private int _originalSortingOrder;
    private Vector3 _pointerOffset;

    /// <summary>
    /// Initialize item with data and assign to a tile
    /// </summary>
    public void Initialize(MergeItemData data, Tile tile)
    {
        _data = data;

        if (_spriteRenderer != null && _data.sprite != null)
        {
            _spriteRenderer.sprite = _data.sprite;
            EnsureColliderSized();
        }

        SetTile(tile);
    }

    /// <summary>
    /// Assign this item to a tile
    /// </summary>
    public void SetTile(Tile tile)
    {
        _currentTile = tile;
        _startPosition = tile.transform.position;
        transform.position = _startPosition;

        tile.PlaceItem(this);
    }

    /// <summary>
    /// Clear current tile reference
    /// </summary>
    public void ClearTile()
    {
        _currentTile = null;
    }

    public Tile CurrentTile => _currentTile;

    private void Awake()
    {
        if (_gridManager == null)
        {
            _gridManager = FindObjectOfType<GridManager>();
        }

        if (_spriteRenderer == null)
            _spriteRenderer = GetComponent<SpriteRenderer>();

        if (_collider == null)
            _collider = GetComponent<BoxCollider2D>();

        if (_collider == null)
            _collider = gameObject.AddComponent<BoxCollider2D>();
        EnsureColliderSized();
    }

    private void EnsureColliderSized()
    {
        // Resize collider to match sprite so it can receive pointer events (BoxCollider2D size is in local space)
        if (_collider == null) return;

        if (_spriteRenderer != null && _spriteRenderer.sprite != null)
        {
            // Sprite bounds are already in local space of the renderer, which matches the collider's local space
            Vector2 localSize = _spriteRenderer.sprite.bounds.size;
            if (localSize.x <= 0.01f || localSize.y <= 0.01f)
            {
                // Fallback sensible size
                localSize = new Vector2(1f, 1f);
            }
            _collider.size = localSize;
            _collider.offset = Vector2.zero;
            _collider.enabled = true;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log($"Item clicked: {name}");
    }

    public void OnPickUp()
    {
        _startPosition = transform.position;
        if (_spriteRenderer != null)
        {
            _originalSortingOrder = _spriteRenderer.sortingOrder;
            _spriteRenderer.sortingOrder = _originalSortingOrder + 20;
        }
    }

    /// <summary>
    /// Called when drag begins
    /// </summary>
    public void OnBeginDrag(PointerEventData eventData)
    {
        OnPickUp();
        
        // Cache offset between pointer and item center for smooth dragging
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(eventData.position);
        worldPos.z = 0;
        _pointerOffset = transform.position - worldPos;
    }

    /// <summary>
    /// Drag movement - follows pointer/finger exactly
    /// </summary>
    public void OnDrag(PointerEventData eventData)
    {
        // Direct screen-to-world conversion for 1:1 tracking
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(eventData.position);
        worldPos.z = 0;
        transform.position = worldPos + _pointerOffset;
    }

    /// <summary>
    /// Called when drag ends (finger lifted / mouse released)
    /// </summary>
    public void OnEndDrag(PointerEventData eventData)
    {
        Tile targetTile = _gridManager != null ? _gridManager.GetTileAtWorldPosition(transform.position) : null;

        bool dropped = OnDrop(targetTile);
        if (!dropped)
        {
            ReturnToStartPosition();
        }
        if (_spriteRenderer != null)
        {
            _spriteRenderer.sortingOrder = _originalSortingOrder;
        }
    }

    public bool OnDrop(Tile targetTile)
    {
        if (targetTile == null) return false;
        if (targetTile.HasSpawner()) return false;

        Item targetItem = targetTile.GetItem();
        if (targetItem == null)
        {
            _currentTile?.RemoveItem();
            SetTile(targetTile);
            return true;
        }

        if (targetItem != null && targetItem.Data == this.Data)
        {
            MergeWith(targetItem);
            return true;
        }

        return false;
    }

    public void ReturnToStartPosition()
    {
        transform.position = _startPosition;
    }

    /// <summary>
    /// Merge this item into the target item
    /// </summary>
    private void MergeWith(Item targetItem)
    {
        Tile targetTile = targetItem._currentTile;

        // Clear original tile
        _currentTile?.RemoveItem();

        // Destroy dragged item, TODO evaluate pooling?
        Destroy(this.gameObject);

        // Merge into next in the chain
        if (targetItem.Data.nextItem != null)
        {
            targetItem.Initialize(targetItem.Data.nextItem, targetTile);
        }
        else
        {
            // Already final level, keep as is
            targetItem.SetTile(targetTile);
        }
    }
}
