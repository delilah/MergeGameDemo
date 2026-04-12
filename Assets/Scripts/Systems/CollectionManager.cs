using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using MergeGame.Data;

namespace MergeGame.Systems
{
    /// <summary>
    /// Centralized manager for tracking collected final items.
    /// Uses Action events for decoupled architecture.
    /// </summary>
    public class CollectionManager : MonoBehaviour
    {
        /// <summary>
        /// Event fired when an item is collected.
        /// Parameters: (MergeItemData itemData, int newCountForThatItem, Vector3 worldPosition)
        /// </summary>
        public event Action<MergeItemData, int, Vector3> OnItemCollected;

        /// <summary>
        /// Event fired on any count change - useful for driving UI refreshes.
        /// </summary>
        public event Action OnCollectionChanged;
        public event Action OnWinConditionMet;

        // Dictionary to track count per item type, keyed by stable asset ID
        private readonly Dictionary<string, int> _collectionCounts = new Dictionary<string, int>();

        // Keeps MergeItemData references alive for display purposes
        private readonly Dictionary<string, MergeItemData> _registeredItems = new Dictionary<string, MergeItemData>();

        private GameConfig _gameConfig;
        private bool _gameOver;

        public int TotalCollected { get; private set; }

        [Inject]
        public void Construct(GameConfig gameConfig)
        {
            _gameConfig = gameConfig;
        }

        /// <summary>
        /// Register that an item has been collected.
        /// </summary>
        public void Collect(MergeItemData itemData, Vector3 worldPosition)
        {            
            if (_gameOver)
            {
                return;
            }

            if (itemData == null)
            {
                Debug.LogWarning("CollectionManager: Attempted to collect null item data");
                return;
            }

            if (_gameConfig == null)
            {
                Debug.LogWarning("CollectionManager: GameConfig is null. Cannot check if game is over.");
                return;
            }

            string id = itemData.Id;

            _collectionCounts.TryGetValue(id, out int currentCount);
            _collectionCounts[id] = currentCount + 1;
            _registeredItems[id] = itemData;

            TotalCollected++;

            OnItemCollected?.Invoke(itemData, currentCount + 1, worldPosition);
            OnCollectionChanged?.Invoke();

            CheckWinCondition();
        }

        /// <summary>
        /// Get the collection count for a specific item type.
        /// </summary>
        public int GetCount(MergeItemData itemData)
        {
            if (itemData == null)
            {
                return 0;
            }

            return _collectionCounts.TryGetValue(itemData.Id, out int count) ? count : 0;
        }

        /// <summary>
        /// Get all collected item counts keyed by stable asset ID (read-only).
        /// </summary>
        public IReadOnlyDictionary<string, int> GetAllCounts()
        {
            return _collectionCounts;
        }

        /// <summary>
        /// Get all registered MergeItemData references keyed by stable asset ID (read-only).
        /// Used by UI to display item names without needing a separate config lookup.
        /// </summary>
        public IReadOnlyDictionary<string, MergeItemData> GetRegisteredItems()
        {
            return _registeredItems;
        }

        /// <summary>
        /// Reset all collection counts (useful for new game).
        /// </summary>
        public void Reset()
        {
            _gameOver = false;
            _collectionCounts.Clear();
            _registeredItems.Clear();
            TotalCollected = 0;
            OnCollectionChanged?.Invoke();

#if UNITY_EDITOR
            Debug.Log("CollectionManager: All counts reset");
#endif
        }

        private void CheckWinCondition()
        {
            if (_gameOver || TotalCollected < _gameConfig.itemsToWin)
            {
                return;
            }

            _gameOver = true;
            OnWinConditionMet?.Invoke();
        }
    }
}