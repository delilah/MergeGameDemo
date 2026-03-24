using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using Zenject;
using MergeGame.Grid;
using MergeGame.Systems;
using MergeGame.Data;

namespace MergeGame.Entities
{
    public class Spawner : MonoBehaviour, IPointerDownHandler
    {
        [SerializeField] private SpawnerData _spawnerData;

        [Header("References")]
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Collider2D _collider;

        private Tile _tile;
        private float _nextAvailableTime = 0f;
        private List<MergeItemData> _spawnableCandidates = new List<MergeItemData>();
        private bool _playIntroAnimation;
        private Tween _introTween;
        private Tween _touchTween;
        private Vector3 _baseScale;

        private int _spawnCount = 0;
        private bool _isRecharging = false;
        private float _rechargeTimer = 0f;

        private AudioManager _audioManager;
        private GridManager _gridManager;       // needed for FreeTilePositions
        private ItemManager _itemManager;       // handles spawning
        private SpawnerManager _spawnerManager; // handles selection state
        private EnergyManager _energyManager;

        [Inject]
        public void Construct(AudioManager audioManager, GridManager gridManager, ItemManager itemManager, SpawnerManager spawnerManager, EnergyManager energyManager)
        {
            _audioManager = audioManager;
            _gridManager = gridManager;
            _itemManager = itemManager;
            _spawnerManager = spawnerManager;
            _energyManager = energyManager;
        }

        /// <summary>
        /// Initializes the spawner with the given data. Must be called after instantiation.
        /// </summary>
        public void Initialize(SpawnerData data, bool playIntroAnimation = false)
        {
            _spawnerData = data;
            if (_spriteRenderer != null && _spawnerData != null && _spawnerData.sprite != null)
            {
                _spriteRenderer.sprite = _spawnerData.sprite;
            }

            PrecomputeSpawnableCandidates();

            _playIntroAnimation = playIntroAnimation;
        }

        private void PrecomputeSpawnableCandidates()
        {
            _spawnableCandidates.Clear();
            if (_spawnerData != null && _spawnerData.spawnableItems != null)
            {
                for (int i = 0; i < _spawnerData.spawnableItems.Length; i++)
                {
                    if (_spawnerData.spawnableItems[i] != null)
                    {
                        _spawnableCandidates.Add(_spawnerData.spawnableItems[i]);
                    }
                }
            }
        }

        private void Awake()
        {
            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (_collider == null)
            {
                _collider = GetComponent<Collider2D>();
            }

            if (_collider == null)
            {
                _collider = GetComponentInChildren<Collider2D>();
            }

            if (_collider == null)
            {
                _collider = gameObject.AddComponent<BoxCollider2D>();
            }

            _tile = GetComponentInParent<Tile>();

            EnsureColliderSized();
        }

        private void Update()
        {
            if (_isRecharging)
            {
                _rechargeTimer += Time.deltaTime;

                if (_rechargeTimer >= _spawnerData.rechargeDuration)
                {
                    _isRecharging = false;
                    _rechargeTimer = 0f;
                    _spawnCount = 0;
                    IntroSpawnerAnimation();
                }
            }
        }

        /// <summary>
        /// Called by Tile after the spawner is placed on the grid.
        /// Sets the base scale used for animations.
        /// </summary>
        public void OnPlaced()
        {
            _baseScale = transform.localScale;

            if (_playIntroAnimation)
            {
                IntroSpawnerAnimation();
            }
        }

        public void IntroSpawnerAnimation()
        {
            Vector3 baseScale = transform.localScale;
            _introTween = transform.DOScale(baseScale + Vector3.one * 0.05f, 0.5f)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine);
        }

        public void StopIntroSpawnerAnimation()
        {
            if (_introTween != null && _introTween.IsActive())
            {
                _introTween.Kill();
                _introTween = null;
            }
        }

        public void OnSpawnerTouchAnimation()
        {
            if (_isRecharging)
            {
                return;
            }
            
            if (_touchTween != null && _touchTween.IsActive())
            {
                _touchTween.Kill();
            }

            transform.localScale = _baseScale; // reset scale before animating so it doesn't grow indefinitely if player furiously clicks

            _touchTween = transform.DOScale(_baseScale + Vector3.one * 0.05f, 0.1f)
                    .SetEase(Ease.OutQuad)
                    .OnComplete(() =>
                        transform.DOScale(_baseScale, 0.1f)
                            .SetEase(Ease.InQuad));
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_isRecharging)
            {
                return;
            }
            
            StopIntroSpawnerAnimation();
            OnSpawnerTouchAnimation();

            _audioManager.PlaySpawnerClick();

            if (!_spawnerManager.IsActive(this))
            {
                _spawnerManager.SetActiveSpawner(this);
                SetSelected(true);
                return;
            }

            // Already selected: attempt to spawn
            TrySpawn();
        }

        public void SetSelected(bool on)
        {
            if (_tile != null)
            {
                _tile.SetHighlight(on);
            }
        }

        private void TrySpawn()
        {
            if (_spawnerData == null)
            {
                Debug.LogWarning("SpawnerData is null; cannot spawn.");
                return;
            }

            if (_isRecharging)
            {
                Debug.Log("Spawner is recharging.");
                return;
            }

            if (Time.time < _nextAvailableTime)
            {
                return;
            }

            if (_itemManager == null)
            {
                Debug.LogWarning("ItemManager not ready; cannot spawn.");
                return;
            }

            if (_spawnableCandidates.Count == 0)
            {
                Debug.LogWarning("Spawner has no valid spawnable items.");
                return;
            }

            if (_gridManager == null)
            {
                Debug.LogWarning("GridManager not ready; cannot spawn.");
                return;
            }

            IReadOnlyList<Vector2Int> freeTiles = _gridManager.FreeTilePositions;
            if (freeTiles.Count == 0)
            {
                Debug.Log("No available tiles");
                return;
            }

            if (!_energyManager.TrySpendEnergy(EnergyManager.EnergyCost.Base))
            {
                Debug.Log("Not enough energy");
                return;
            }

            int itemIdx = Random.Range(0, _spawnableCandidates.Count);
            MergeItemData itemData = _spawnableCandidates[itemIdx];

            int tileIdx = Random.Range(0, freeTiles.Count);
            Vector2Int gridPos = freeTiles[tileIdx];

            _itemManager.SpawnItem(itemData, gridPos);

            _spawnCount++;

            // Start cooldown
            _nextAvailableTime = Time.time + Mathf.Max(0f, _spawnerData.spawnInterval);

            // Start recharging if max spawn count reached
            if (_spawnCount >= _spawnerData.maxSpawnCount)
            {
                _isRecharging = true;
                _rechargeTimer = 0f;
                Debug.Log("Spawner exhausted, recharging.");
            }
        }

        public void SetTile(Tile tile)
        {
            _tile = tile;
        }

        private void EnsureColliderSized()
        {
            var box = _collider as BoxCollider2D;
            if (box != null && _spriteRenderer != null && _spriteRenderer.sprite != null)
            {
                Vector2 localSize = _spriteRenderer.sprite.bounds.size;
                if (localSize.x <= 0.01f || localSize.y <= 0.01f)
                {
                    localSize = new Vector2(1f, 1f);
                }
                box.size = localSize;
                box.offset = Vector2.zero;
                box.enabled = true;
            }
        }

        private void OnDisable()
        {
            transform.DOKill();
            _spawnerManager.ClearActiveSpawner(this);
            SetSelected(false);
        }

        // NOTE: added OnDestroy to clear active spawner if this spawner is destroyed.
        // OnDisable is not reliably called on destroyed objects during scene reloads,
        // so without this _spawnerManager could hold a stale reference to a destroyed spawner.
        private void OnDestroy()
        {
            transform.DOKill();
            _spawnerManager.ClearActiveSpawner(this);
        }
    }
}