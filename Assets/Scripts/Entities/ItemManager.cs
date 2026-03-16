using UnityEngine;
using UnityEngine.Pool;
using Zenject;
using MergeGame.Grid;
using MergeGame.Data;
using System.Collections.Generic;

namespace MergeGame.Entities
{
   public class ItemManager : MonoBehaviour
    {
        private const string ITEMS_PARENT_NAME = "Items";

        [SerializeField] private Item _itemPrefab;

        private GridManager _gridManager;
        private DiContainer _container;

        private ObjectPool<Item> _itemPool;
        private GameObject _itemsParent;
        private List<Item> _activeItems = new List<Item>();

        [Inject]
        public void Construct(GridManager gridManager, DiContainer container)
        {
            _gridManager = gridManager;
            _container = container;
        }

        private void Awake()
        {
            _itemsParent = new GameObject(ITEMS_PARENT_NAME);

            _itemPool = new ObjectPool<Item>(
                createFunc: () => _container.InstantiatePrefabForComponent<Item>(_itemPrefab, _itemsParent.transform),
                actionOnGet: item => item.gameObject.SetActive(true),
                actionOnRelease: item => item.gameObject.SetActive(false),
                actionOnDestroy: item => Destroy(item.gameObject)
            );
        }

        /// <summary>
        /// Spawns an item at the given grid position.
        /// </summary>
        /// <param name="data">The item data.</param>
        /// <param name="gridPos">The grid position.</param>
        /// <returns>The spawned item, or null if the position is invalid or occupied.</returns>
        public Item SpawnItem(MergeItemData data, Vector2Int gridPos)
        {
            if (_itemPrefab == null)
            {
                Debug.LogWarning("No Item Prefab assigned!");
                return null;
            }

            Tile tile = _gridManager.GetTileAtPosition(gridPos);
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

            Item newItem = _itemPool.Get();
            _activeItems.Add(newItem);
            newItem.Initialize(data, tile);
            return newItem;
        }

        /// <summary>
        /// Returns an item to the pool.
        /// </summary>
        /// <param name="item">The item to return.</param>
        public void ReturnItemToPool(Item item)
        {
            if (item == null)
            {
                return;
            }

            _activeItems.Remove(item);
            item.transform.SetParent(_itemsParent.transform);
            _itemPool.Release(item); // handles SetActive(false) automatically
        }

        /// <summary>
        /// Returns all active items to the pool. 
        /// Call before ClearGrid on reset.
        /// </summary>
        public void ClearAll()
        {
            foreach (Item item in _activeItems)
            {
                if (item == null)
                {
                    continue;
                }
                item.CurrentTile?.RemoveItem();
                item.ClearTile();
                item.transform.SetParent(_itemsParent.transform);
                _itemPool.Release(item);
            }
            _activeItems.Clear();
        }
    }
}