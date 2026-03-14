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
        public int TotalCollected { get; private set; }

        private bool _gameOver;

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

       // Dictionary to track count per item type
        private readonly Dictionary<MergeItemData, int> _collectionCounts = new Dictionary<MergeItemData, int>();

        private GameConfig _gameConfig;

        [Inject]
        public void Construct(GameConfig gameConfig)
        {
            _gameConfig = gameConfig;
        }
        
        /// <summary>
        /// Register that an item has been collected
        /// </summary>
        public void Collect(MergeItemData itemData, Vector3 worldPosition)
        {
            if (_gameOver) return;

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

            _collectionCounts.TryGetValue(itemData, out int currentCount);
            _collectionCounts[itemData] = currentCount + 1;

            TotalCollected++;

            #if UNITY_EDITOR
            Debug.Log($"CollectionManager: Collected {itemData.itemName}. Item count: {currentCount + 1}");
            #endif

            OnItemCollected?.Invoke(itemData, currentCount + 1, worldPosition);
            OnCollectionChanged?.Invoke();

            CheckWinCondition();
        }         

        private void CheckWinCondition()
        {
            if (!_gameOver && TotalCollected >= _gameConfig.itemsToWin)
            {
                _gameOver = true;
                OnWinConditionMet?.Invoke();
            }
        }

        /// <summary>
        /// Get the collection count for a specific item type
        /// </summary>
        public int GetCount(MergeItemData itemData)
        {
            if (itemData == null) return 0;

            return _collectionCounts.TryGetValue(itemData, out int count) ? count : 0;
        }

        /// <summary>
        /// Get all collected items and their counts (read-only)
        /// </summary>
        public IReadOnlyDictionary<MergeItemData, int> GetAllCounts()
        {
            return _collectionCounts;
        }

        /// <summary>
        /// Reset all collection counts (useful for new game)
        /// </summary>
        public void Reset()
        {
            _gameOver = false;
            _collectionCounts.Clear();
            TotalCollected = 0;
            OnCollectionChanged?.Invoke();

            #if UNITY_EDITOR
            Debug.Log("CollectionManager: All counts reset");
            #endif
        }
    }
}