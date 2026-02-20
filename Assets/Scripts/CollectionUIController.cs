using System.Collections.Generic;
using System.Text;
using UnityEngine;
using TMPro;

/// <summary>
/// UI controller that listens to CollectionManager events and displays collection counts.
/// Uses StringBuilder for efficient text building on mobile.
/// </summary>
public class CollectionUIController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text _collectionText;

    [Header("Display Settings")]
    [SerializeField] private MergeItemData[] _trackedItems; // Items to display in UI
    [SerializeField] private bool _showOnlyCollected = false; // Only show items with count > 0

    // Reusable StringBuilder to avoid allocations
    private StringBuilder _textBuilder = new StringBuilder();

    private CollectionManager _collectionManager;


    private void OnEnable()
    {
        // Subscribe to collection events
        _collectionManager = CollectionManager.Instance;

        if (_collectionManager == null)
        {
            Debug.LogWarning("CollectionManager not found during OnEnable — UI will not update.");
            return;
        }

        _collectionManager.OnItemCollected += HandleItemCollected;
        _collectionManager.OnCollectionChanged += HandleCollectionChanged;

        // Initial UI update
        UpdateUI();
    }

    private void OnDisable()
    {
        // Unsubscribe from events
        if (_collectionManager != null)
        {
            _collectionManager.OnItemCollected -= HandleItemCollected;
            _collectionManager.OnCollectionChanged -= HandleCollectionChanged;
        }
    }

    /// <summary>
    /// Called when a specific item is collected
    /// </summary>
    private void HandleItemCollected(MergeItemData itemData, int newCount)
    {
        // Show feedback message
        Debug.Log($"{itemData.itemName} collected! Total: {newCount}");
        
        // You can add popup/toast notification here
        // ShowCollectionPopup($"{itemData.itemName} collected!");

        // Update the full UI
        UpdateUI();
    }

    /// <summary>
    /// Called when any collection data changes
    /// </summary>
    private void HandleCollectionChanged()
    {
        UpdateUI();
    }

    /// <summary>
    /// Rebuild the collection text using StringBuilder
    /// </summary>
    private void UpdateUI()
    {
        if (_collectionText == null) return;

        _textBuilder.Clear();
        _textBuilder.AppendLine("=== Collection ===");

        if (_trackedItems != null && _trackedItems.Length > 0)
        {
            // Display specific tracked items
            foreach (var itemData in _trackedItems)
            {
                if (itemData == null) continue;

                int count = _collectionManager != null 
                    ? _collectionManager.GetCount(itemData) 
                    : 0;

                // Skip if showing only collected and count is 0
                if (_showOnlyCollected && count == 0) continue;

                _textBuilder.AppendLine($"{itemData.itemName}: {count}");
            }
        }
        else
        {
            // Display all collected items from the _collectionManager
            if (_collectionManager != null)
            {
                var allCounts = _collectionManager.GetAllCounts();
                
                if (allCounts.Count == 0)
                {
                    _textBuilder.AppendLine("No items collected yet");
                }
                else
                {
                    foreach (var kvp in allCounts)
                    {
                        if (kvp.Key == null) continue;
                        if (_showOnlyCollected && kvp.Value == 0) continue;

                        _textBuilder.AppendLine($"{kvp.Key.itemName}: {kvp.Value}");
                    }
                }
            }
        }

        _collectionText.text = _textBuilder.ToString();
    }

    /// <summary>
    /// Optional: Show a temporary popup when item is collected
    /// </summary>
    private void ShowCollectionPopup(string message)
    {
        // TODO: Implement floating text, toast, or popup
        // For now, just log
        Debug.Log($"[Popup] {message}");
    }
}
