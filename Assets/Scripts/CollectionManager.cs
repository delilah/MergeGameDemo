using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Centralized manager for tracking collected final items.
/// Uses Action events for decoupled architecture.
/// </summary>
public class CollectionManager : MonoBehaviour
{
    public static CollectionManager Instance { get; private set; }

    // Dictionary to track count per item type
    private Dictionary<MergeItemData, int> _collectionCounts = new Dictionary<MergeItemData, int>();

    /// <summary>
    /// Event fired when an item is collected.
    /// Parameters: (MergeItemData itemData, int newTotalCount)
    /// </summary>
    public event Action<MergeItemData, int> OnItemCollected;

    /// <summary>
    /// Event fired when any collection count changes (useful for UI refresh)
    /// </summary>
    public event Action OnCollectionChanged;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Register that an item has been collected
    /// </summary>
    public void Collect(MergeItemData itemData)
    {
        if (itemData == null)
        {
            Debug.LogWarning("CollectionManager: Attempted to collect null item data");
            return;
        }

        _collectionCounts.TryGetValue(itemData, out int currentCount);
        _collectionCounts[itemData] = currentCount + 1;

        Debug.Log($"CollectionManager: Collected {itemData.itemName}. Total: {currentCount + 1}");

        OnItemCollected?.Invoke(itemData, currentCount + 1);
        OnCollectionChanged?.Invoke();
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
            // _collectionCounts[itemData] = 0;
            _collectionCounts.Remove(itemData);
            OnCollectionChanged?.Invoke();
        }
    }
}
