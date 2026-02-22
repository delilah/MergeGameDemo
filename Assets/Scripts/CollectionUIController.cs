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
    // Assigned to DynamicCanvas, separated from StaticCanvas to avoid
    // triggering full Canvas rebuilds on static elements when collection updates.
    // Updated via StringBuilder to avoid per-frame string allocations on mobile.
    [SerializeField] private TMP_Text _collectionText;
    // [SerializeField] private TMP_Text _itemCollectedPopupText; // TODO: implement floating text

    [Header("Effects")]
    [SerializeField] private ParticleSystem _collectParticlesPrefab;

    [Header("Display Settings")]
    [SerializeField] private MergeItemData[] _trackedItems; // Items to display in UI
    [SerializeField] private bool _showOnlyCollected = false; // Only show items with count > 0

    // Reusable StringBuilder to avoid allocations
    private StringBuilder _textBuilder = new StringBuilder();

    private CollectionManager _collectionManager;


    private void OnEnable()
    {
        _collectionManager = CollectionManager.Instance;
        if (_collectionManager == null) return;

        _collectionManager.OnItemCollected += HandleItemCollected;
        _collectionManager.OnCollectionChanged += HandleCollectionChanged;

        UpdateUI(); 
    }

    private void OnDisable()
    {
        if (_collectionManager != null)
        {
            _collectionManager.OnItemCollected -= HandleItemCollected;
            _collectionManager.OnCollectionChanged -= HandleCollectionChanged;
        }
    }

    private void HandleItemCollected(MergeItemData itemData, int newCount, Vector3 position)
    {
        ShowCollectionPopup($"{itemData.itemName} collected!");
        SpawnCollectParticles(position);

        UpdateUI();
    }

    private void HandleCollectionChanged()
    {
        UpdateUI();
    }


    private void SpawnCollectParticles(Vector3 position)
    {
        if (_collectParticlesPrefab == null) return;
        Instantiate(_collectParticlesPrefab, position, Quaternion.identity);
    }

    /// <summary>
    /// Rebuilds the collection text using StringBuilder.
    /// Displays total count of all collected items, followed by per-item breakdown.
    /// </summary>
    private void UpdateUI()
    {
        if (_collectionText == null) return;

        _textBuilder.Clear();

        if (_trackedItems != null && _trackedItems.Length > 0)
        {
            // Display specific tracked items
            int total = 0;
            foreach (var itemData in _trackedItems)
            {
                if (itemData == null) continue;
                int count = _collectionManager != null ? _collectionManager.GetCount(itemData) : 0;
                total += count;
            }

            _textBuilder.Append($" {total}");

            foreach (var itemData in _trackedItems)
            {
                if (itemData == null) continue;
                int count = _collectionManager != null ? _collectionManager.GetCount(itemData) : 0;
                if (_showOnlyCollected && count == 0) continue;
                _textBuilder.AppendLine();
                _textBuilder.Append($"{itemData.itemName}: {count}");
            }
        }
        else
        {
            // Display all collected items from CollectionManager
            if (_collectionManager != null)
            {
                var allCounts = _collectionManager.GetAllCounts();
                int total = 0;
                foreach (var kvp in allCounts) total += kvp.Value;

                _textBuilder.Append($"Collected: {total}");

                foreach (var kvp in allCounts)
                {
                    if (kvp.Key == null) continue;
                    if (_showOnlyCollected && kvp.Value == 0) continue;
                    _textBuilder.AppendLine();
                    _textBuilder.Append($"{kvp.Key.itemName}: {kvp.Value}");
                }
            }
        }

        _collectionText.text = _textBuilder.ToString();
    }

    private void ShowCollectionPopup(string message)
    {
        // TODO: Implement floating text, toast, or popup
        Debug.Log($"[Popup] {message}");
    }
}