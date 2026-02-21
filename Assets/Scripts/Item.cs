using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;


public class Item : MonoBehaviour, IDraggable, IPointerDownHandler
{
    public static UnityEvent<Item> OnItemReturnRequested = new UnityEvent<Item>();

    [Header("References")]
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private BoxCollider2D _collider;

    [Header("Item Data")]
    [SerializeField] private MergeItemData _data;
    public MergeItemData Data => _data;

    private Tile _currentTile;
    private Vector3 _startPosition;
    private int _originalSortingOrder;
    private Vector3 _pointerOffset;
    private Camera _mainCamera;

    private const int DRAG_SORTING_ORDER_OFFSET = 20;

    

    public void Initialize(MergeItemData data, Tile tile)
    {
        _data = data;

        if (_spriteRenderer != null && _data.sprite != null)
        {
            _spriteRenderer.sprite = _data.sprite;
            EnsureColliderSized();
        }

        if (_spriteRenderer != null)
        {
            _spriteRenderer.sortingOrder = _originalSortingOrder;
        }

        SetTile(tile);
    }

    public void SetTile(Tile tile)
    {
        _currentTile = tile;
        _startPosition = tile.transform.position;
        transform.position = _startPosition;

        tile.PlaceItem(this);
    }

    public void ClearTile()
    {
        _currentTile = null;
    }

    public Tile CurrentTile => _currentTile;

    private void Awake()
    {
        _mainCamera = Camera.main;

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
        if (_collider == null) return;

        if (_spriteRenderer != null && _spriteRenderer.sprite != null)
        {
            Vector2 localSize = _spriteRenderer.sprite.bounds.size;
            if (localSize.x <= 0.01f || localSize.y <= 0.01f)
            {
                localSize = new Vector2(1f, 1f);
            }
            _collider.size = localSize;
            _collider.offset = Vector2.zero;
            _collider.enabled = true;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        bool isFinal = _data != null && _data.isFinal;

        if (isFinal)
        {
            if (CollectionManager.Instance != null)
            {
                CollectionManager.Instance.Collect(_data, transform.position);
            }

            Debug.Log($"{_data.itemName} collected!");
            ReturnToPool();
        }
        else
        {
            Debug.Log($"Non-final item clicked: {name} (Data: {_data.name}, next: {_data.nextItem?.name})");
        }
    }

    public void OnPickUp()
    {
        _startPosition = transform.position;
        if (_spriteRenderer != null)
        {
            _originalSortingOrder = _spriteRenderer.sortingOrder;
            _spriteRenderer.sortingOrder = _originalSortingOrder + DRAG_SORTING_ORDER_OFFSET;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        OnPickUp();

        Vector3 worldPos = _mainCamera.ScreenToWorldPoint(eventData.position);
        worldPos.z = 0;
        _pointerOffset = transform.position - worldPos;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector3 worldPos = _mainCamera.ScreenToWorldPoint(eventData.position);
        worldPos.z = 0;
        transform.position = worldPos + _pointerOffset;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Tile targetTile = GridManager.Instance != null
            ? GridManager.Instance.GetTileAtWorldPosition(transform.position)
            : null;

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

        if (targetItem.Data == this.Data)
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

    private void MergeWith(Item targetItem)
    {
        Tile targetTile = targetItem.CurrentTile;

        ReturnToPool();

        if (targetItem.Data.nextItem != null)
        {
            targetItem.Initialize(targetItem.Data.nextItem, targetTile);
        }
        else
        {
            targetItem.SetTile(targetTile);
        }
    }

    

    private void ReturnToPool()
    {
        _currentTile?.RemoveItem();
        ClearTile();
        OnItemReturnRequested.Invoke(this);
    }

}