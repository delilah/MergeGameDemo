// A tile is a cell, a square of the grid.
// Tile has colour and is free or occupied.
// Objects on a tile are Items. They are draggable.

using UnityEngine;
using MergeGame.Entities;

namespace MergeGame.Grid
{
    public class Tile : MonoBehaviour
    {
        [Header("Tile Settings")]
        [SerializeField] private Color _baseColor;
        [SerializeField] private Color _offsetColor;
        [SerializeField] private SpriteRenderer _renderer;
        [SerializeField] private GameObject _highlight;

        public bool HasItem() => _currentItem != null;
        public Item GetItem() => _currentItem;
        public bool HasSpawner() => _currentSpawner != null;
        public Spawner GetSpawner() => _currentSpawner;

        // True only when the tile has no item AND no spawner on it.
        public bool IsEmpty => _currentSpawner == null && _currentItem == null;

        private Item _currentItem;
        private Spawner _currentSpawner;
        private const float _spritePadding = .95f;
        private Vector2Int _gridPosition;
        private GridManager _gridManager;

        private void Awake()
        {
            if (_renderer == null)
                _renderer = GetComponent<SpriteRenderer>();
        }

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
            if (_highlight == null) return;
            _highlight.SetActive(on);
        }

        public void PlaceSpawner(Spawner spawner)
        {
            if (spawner == null)
            {
                Debug.LogWarning("Spawner is null, cannot place.");
                return;
            }

            _currentSpawner = spawner;

            PlaceObject(spawner.transform, Vector3.zero);
            spawner.SetTile(this);
            AdjustSortingAboveTile(spawner.GetComponent<SpriteRenderer>());

            if (_gridManager != null)
            {
                _gridManager.MarkTileOccupied(_gridPosition);
            }

            spawner.OnPlaced();
        }

        public void PlaceItem(Item item)
        {
            if (_gridManager == null)
            {
                Debug.LogError($"GridManager is null on tile {name}: was SetGridPosition called?");
                return;
            }

            _currentItem = item;
            if (item == null) return;

            PlaceObject(item.transform, new Vector3(0, 0.05f, 0));
            AdjustSortingAboveTile(item.GetComponent<SpriteRenderer>());

            if (_gridManager != null)
            {
                _gridManager.MarkTileOccupied(_gridPosition);
            }
        }

        public void RemoveItem()
        {
            if (_gridManager == null)
            {
                Debug.LogError($"GridManager is null on tile {name}: was SetGridPosition called?");
                return;
            }

            _currentItem = null;

            // Only mark free if there's no spawner occupying this tile
            if (_gridManager != null && !HasSpawner())
            {
                _gridManager.MarkTileFree(_gridPosition);
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
            if (sr == null || sr.sprite == null) return;

            Vector2 spriteSize = sr.sprite.bounds.size;
            float targetSize = tileSize * _spritePadding;
            float scaleFactor = targetSize / Mathf.Max(spriteSize.x, spriteSize.y);

            objTransform.localScale = new Vector3(scaleFactor, scaleFactor, 1f);
        }

        private void AdjustSortingAboveTile(SpriteRenderer sr)
        {
            if (sr == null) return;

            sr.sortingLayerID = _renderer.sortingLayerID;
            sr.sortingOrder = _renderer.sortingOrder + 1;
        }

        public Vector2Int GridPosition => _gridPosition;
    }
}