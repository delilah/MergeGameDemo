using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;
using MergeGame.Grid;
using MergeGame.Interfaces;
using MergeGame.Data;

namespace MergeGame.Entities
{
    public class Item : MonoBehaviour, IDraggable, IPointerDownHandler
    {
        [Header("References")]
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private BoxCollider2D _collider;

        [Header("Item Data")]
        [SerializeField] private MergeItemData _data;

        private Tile _currentTile;
        private Vector3 _startPosition;
        private int _originalSortingOrder;
        private Vector3 _pointerOffset;
        private Camera _mainCamera;

        private GridManager _gridManager;   // needed for drag-and-drop (GetTileAtWorldPosition)
        private ItemManager _itemManager;   // handles merge, collect, and pool return
        private GameConfig _gameConfig;

        public MergeItemData Data => _data;
        public Tile CurrentTile => _currentTile;

        [Inject]
        public void Construct(GridManager gridManager, ItemManager itemManager, GameConfig gameConfig)
        {
            _gridManager = gridManager;
            _itemManager = itemManager;
            _gameConfig = gameConfig;
        }

        private void Awake()
        {
            _mainCamera = Camera.main;

            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (_collider == null)
            {
                _collider = GetComponent<BoxCollider2D>();
            }

            if (_collider == null)
            {
                _collider = gameObject.AddComponent<BoxCollider2D>();
            }

            EnsureColliderSized();
        }

        /// <summary>
        /// Initializes the item with the given data and places it on the given tile.
        /// Must be called after instantiation.
        /// </summary>
        public void Initialize(MergeItemData data, Tile tile)
        {
            if (tile == null)
            {
                Debug.LogError("Item: Cannot initialize with a null tile.");
                return;
            }

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

        // Called by ItemManager.ClearAll to detach the item from its tile before returning to pool
        public void ClearTile()
        {
            _currentTile = null;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            bool isFinal = _data != null && _data.isFinal;

            if (isFinal)
            {
                _itemManager.CollectItem(this);
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
                _spriteRenderer.sortingOrder = _originalSortingOrder + _gameConfig.dragSortingOrderOffset;
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
            Tile targetTile = _gridManager.GetTileAtWorldPosition(transform.position);

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
            if (targetTile == null)
            {
                return false;
            }

            if (targetTile.HasSpawner())
            {
                return false;
            }

            Item targetItem = targetTile.GetItem();
            if (targetItem == null)
            {
                _currentTile?.RemoveItem();
                SetTile(targetTile);
                return true;
            }

            // If dropping on own tile (e.g., from clamping at grid edges), reject the drop
            if (targetItem == this)
            {
                return false;
            }

            if (targetItem.Data == this.Data)
            {
                _itemManager.MergeItems(this, targetItem);
                return true;
            }

            return false;
        }

        public void ReturnToStartPosition()
        {
            transform.position = _startPosition;
        }

        public void ReturnToPool()
        {
            _currentTile?.RemoveItem();
            ClearTile();
            _itemManager.ReturnItemToPool(this);
        }

        private void EnsureColliderSized()
        {
            if (_collider == null)
            {
                return;
            }

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
    }
}