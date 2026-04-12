using System.Collections.Generic;
using System.Text;
using UnityEngine;
using TMPro;
using Zenject;
using MergeGame.Systems;
using MergeGame.Data;

namespace MergeGame.UI
{
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
        [SerializeField] private TMP_Text _scoreText;

        [Header("Cats")]
        [SerializeField] private TMP_Text[] _catScoreTexts;

        [Header("Effects")]
        [SerializeField] private ParticleSystem _collectParticlesPrefab;

        [Header("Display Settings")]
        [SerializeField] private bool _showOnlyCollected = false; // Only show items with count > 0

        // Reusable StringBuilder to avoid allocations
        private StringBuilder _textBuilder = new StringBuilder();

        private CatCollectionConfig _catsConfig;
        private CollectionManager _collectionManager;
        private GameConfig _gameConfig;

        [Inject]
        public void Construct(CollectionManager collectionManager, GameConfig gameConfig, CatCollectionConfig catsConfig)
        {
            _collectionManager = collectionManager;
            _gameConfig = gameConfig;
            _catsConfig = catsConfig;
        }

        private void OnEnable()
        {
            if (_collectionManager == null)
            {
                return;
            }

            _collectionManager.OnItemCollected += HandleItemCollected;
            _collectionManager.OnCollectionChanged += HandleCollectionChanged;

            // Initialize cat scores to "0" first
            if (_catScoreTexts != null)
            {
                for (int i = 0; i < _catScoreTexts.Length; i++)
                {
                    if (_catScoreTexts[i] != null)
                    {
                        _catScoreTexts[i].text = "0";
                    }
                }
            }

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
            if (_collectParticlesPrefab == null)
            {
                return;
            }

            ParticleSystem particles = Instantiate(_collectParticlesPrefab, position, Quaternion.identity);
            var main = particles.main;
            Destroy(particles.gameObject, main.duration + main.startLifetime.constantMax);
        }

        /// <summary>
        /// Rebuilds the collection text using StringBuilder.
        /// Displays total count of all collected items, followed by per-item breakdown.
        /// </summary>
        private void UpdateUI()
        {
            if (_collectionText == null || _scoreText == null)
            {
                return;
            }

            _textBuilder.Clear();
            _textBuilder.Append($"Collected: {_collectionManager.TotalCollected}");

            // Iterate registered items for name display — these are real MergeItemData references
            foreach (var kvp in _collectionManager.GetRegisteredItems())
            {
                if (_showOnlyCollected && _collectionManager.GetAllCounts()[kvp.Key] == 0)
                {
                    continue;
                }

                _textBuilder.AppendLine();
                _textBuilder.Append($"{kvp.Value.itemName}: {_collectionManager.GetAllCounts()[kvp.Key]}");
            }

            _collectionText.text = _textBuilder.ToString();
            _scoreText.text = _collectionManager.TotalCollected.ToString();

            UpdateCatScoresFromConfig();
        }

        private void UpdateCatScoresFromConfig()
        {
            if (_catScoreTexts == null || _catsConfig?.trackedItems == null)
            {
                return;
            }

            for (int i = 0; i < _catsConfig.trackedItems.Length; i++)
            {
                MergeItemData item = _catsConfig.trackedItems[i];

                if (item == null)
                {
                    continue;
                }

                int count = _collectionManager.GetCount(item);

                if (i < _catScoreTexts.Length && _catScoreTexts[i] != null)
                {
                    _catScoreTexts[i].text = count.ToString();
                }
            }
        }

        private void ShowCollectionPopup(string message)
        {
            // TODO: Implement floating text, toast, or popup
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"[Popup] {message}");
            #endif
        }
    }
}