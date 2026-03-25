using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Zenject;
using MergeGame.Grid;
using MergeGame.Systems;
using MergeGame.Data;

namespace MergeGame.Entities
{
    public class ItemManager : MonoBehaviour
    {
        [SerializeField] private Item _itemPrefab;

        private GridManager _gridManager;
        private DiContainer _container;
        private AudioManager _audioManager;
        private CollectionManager _collectionManager;
        private GameConfig _gameConfig;

        private ObjectPool<Item> _itemPool;
        private GameObject _itemsParent;
        private List<Item> _activeItems = new List<Item>();

        [Inject]
        public void Construct(GridManager gridManager, DiContainer container, AudioManager audioManager, CollectionManager collectionManager, GameConfig gameConfig)
        {
            _gridManager = gridManager;
            _container = container;
            _audioManager = audioManager;
            _collectionManager = collectionManager;
            _gameConfig = gameConfig;
        }

        private void Awake()
        {
            _itemsParent = new GameObject(_gameConfig.itemsParentName);

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
        /// Merges the source item into the target item, producing the next item in the chain.
        /// Uses the item's own mergeSound and mergeSoundPitch if assigned,
        /// otherwise falls back to the level-based sound at default pitch.
        /// </summary>
        /// <param name="source">The item being dragged.</param>
        /// <param name="target">The item being merged into.</param>
        public void MergeItems(Item source, Item target)
        {
            if (source == null)
            {
                Debug.LogWarning("ItemManager: Cannot merge — source item is null.");
                return;
            }

            if (target == null)
            {
                Debug.LogWarning("ItemManager: Cannot merge — target item is null.");
                return;
            }

            if (target.Data == null)
            {
                Debug.LogWarning("ItemManager: Cannot merge — target item has no data.");
                return;
            }

            Tile targetTile = target.CurrentTile;

            // Use item-specific merge sound and pitch if assigned,
            // otherwise fall back to level-based sound at default pitch
            AudioClip mergeClip = source.Data.mergeSound != null
                ? source.Data.mergeSound
                : _audioManager.GetMergeSoundForLevel(source.Data.level);

            float pitch = source.Data.mergeSound != null
                ? source.Data.mergeSoundPitch
                : 1f;

            _audioManager.PlaySfx(mergeClip, pitch);

            source.ReturnToPool();

            if (target.Data.nextItem != null)
            {
                target.Initialize(target.Data.nextItem, targetTile);
            }
            else
            {
                target.SetTile(targetTile);
            }
        }

        /// <summary>
        /// Collects a final-tier item, triggering collection tracking and returning it to the pool.
        /// </summary>
        /// <param name="item">The item to collect.</param>
        public void CollectItem(Item item)
        {
            if (item == null)
            {
                Debug.LogWarning("ItemManager: Cannot collect a null item.");
                return;
            }

            _collectionManager.Collect(item.Data, item.transform.position);
            Debug.Log($"{item.Data.itemName} collected!");
            item.ReturnToPool();
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