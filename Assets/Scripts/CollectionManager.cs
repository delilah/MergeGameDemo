using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;


/// <summary>
/// Centralized manager for tracking collected final items.
/// Uses Action events for decoupled architecture.
/// </summary>
public class CollectionManager : MonoBehaviour
{
    public int TotalCollected { get; private set; }

    // Dictionary to track count per item type
    private Dictionary<MergeItemData, int> _collectionCounts = new Dictionary<MergeItemData, int>();

    /// <summary>
    /// Event fired when an item is collected.
    /// Parameters: (MergeItemData itemData, int newTotalCount)
    /// </summary>
    public event Action<MergeItemData, int, Vector3> OnItemCollected;

    /// <summary>
    /// Event fired when any collection count changes (useful for UI refresh)
    /// </summary>
    public event Action OnCollectionChanged;
    public event Action OnGameOver;

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
        if (itemData == null)
        {
            Debug.LogWarning("CollectionManager: Attempted to collect null item data");
            return;
        }

        _collectionCounts.TryGetValue(itemData, out int currentCount);
        _collectionCounts[itemData] = currentCount + 1;

        TotalCollected++;

        Debug.Log($"CollectionManager: Collected {itemData.itemName}. Total: {currentCount + 1}");

        OnItemCollected?.Invoke(itemData, currentCount + 1, worldPosition);
        OnCollectionChanged?.Invoke();

        if (_gameConfig == null)
        {
            Debug.LogWarning("CollectionManager: GameConfig is null. Cannot check if game is over.");
            return;
        }

        if (TotalCollected >= _gameConfig.itemsToWin)
        {
            OnGameOver?.Invoke();
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
    public void ResetAllCounts()
    {
        _collectionCounts.Clear();
        TotalCollected = 0;
        OnCollectionChanged?.Invoke();
        Debug.Log("CollectionManager: All counts reset");
    }

    /// <summary>
    /// Reset count for a specific item
    /// </summary>
    public void ResetCount(MergeItemData itemData)
    {
        if (itemData != null && _collectionCounts.ContainsKey(itemData))
        {
            _collectionCounts.Remove(itemData);
            OnCollectionChanged?.Invoke();
        }
    }
}
