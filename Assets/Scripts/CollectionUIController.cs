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

    private void OnEnable()
    {
        // Subscribe to collection events
        if (CollectionManager.Instance != null)
        {
            CollectionManager.Instance.OnItemCollected += HandleItemCollected;
            CollectionManager.Instance.OnCollectionChanged += HandleCollectionChanged;
        }

        // Initial UI update
        UpdateUI();
    }

    private void OnDisable()
    {
        // Unsubscribe from events
        if (CollectionManager.Instance != null)
        {
            CollectionManager.Instance.OnItemCollected -= HandleItemCollected;
            CollectionManager.Instance.OnCollectionChanged -= HandleCollectionChanged;
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

                int count = CollectionManager.Instance != null 
                    ? CollectionManager.Instance.GetCount(itemData) 
                    : 0;

                // Skip if showing only collected and count is 0
                if (_showOnlyCollected && count == 0) continue;

                _textBuilder.AppendLine($"{itemData.itemName}: {count}");
            }
        }
        else
        {
            // Display all collected items from the manager
            if (CollectionManager.Instance != null)
            {
                var allCounts = CollectionManager.Instance.GetAllCounts();
                
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
