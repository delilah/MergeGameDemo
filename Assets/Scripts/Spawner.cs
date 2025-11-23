using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Spawner : MonoBehaviour, IPointerDownHandler
{
    public SpawnerData spawnerData;

    [Header("References")]
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Collider2D _collider;
    
    private GridManager _gridManager;

    private Tile _tile;
    private float _nextAvailableTime = 0f;
    private List<MergeItemData> _spawnableCandidates = new List<MergeItemData>();

    private static Spawner _active;

    public void Initialize(SpawnerData data)
    {
        spawnerData = data;
        if (_spriteRenderer != null && spawnerData != null && spawnerData.sprite != null)
        {
            _spriteRenderer.sprite = spawnerData.sprite;
        }

        PrecomputeSpawnableCandidates();
    }

    private void PrecomputeSpawnableCandidates()
    {
        _spawnableCandidates.Clear();
        if (spawnerData != null && spawnerData.spawnableItems != null)
        {
            for (int i = 0; i < spawnerData.spawnableItems.Length; i++)
            {
                if (spawnerData.spawnableItems[i] != null)
                {
                    _spawnableCandidates.Add(spawnerData.spawnableItems[i]);
                }
            }
        }
    }

    private void Awake()
    {
        if (_spriteRenderer == null) _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_collider == null) _collider = GetComponent<Collider2D>();
        if (_collider == null) _collider = GetComponentInChildren<Collider2D>();
        if (_collider == null)
        {
            // Ensure a collider exists on this object for pointer events
            _collider = gameObject.AddComponent<BoxCollider2D>();
        }
        
        // Use singleton instead of FindObjectOfType
        _gridManager = GridManager.Instance;
        _tile = GetComponentInParent<Tile>();

        EnsureColliderSized();
    }

    private void OnDisable()
    {
        if (_active == this)
        {
            SetSelected(false);
            _active = null;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log($"Spawner tapped: {name}");
        if (_active != this)
        {
            // Select this spawner
            if (_active != null)
            {
                _active.SetSelected(false);
            }
            _active = this;
            SetSelected(true);
            return;
        }

        // Already selected: attempt to spawn
        TrySpawn();
    }

    private void SetSelected(bool on)
    {
        if (_tile != null)
        {
            _tile.SetHighlight(on);
        }
    }

    private void TrySpawn()
    {
        if (spawnerData == null)
        {
            Debug.LogWarning("SpawnerData is null; cannot spawn.");
            return;
        }

        if (Time.time < _nextAvailableTime)
        {
            // On cooldown
            return;
        }

        if (_gridManager == null)
        {
            Debug.LogWarning("GridManager not ready; cannot spawn.");
            return;
        }

        if (_spawnableCandidates.Count == 0)
        {
            Debug.LogWarning("Spawner has no valid spawnable items.");
            return;
        }

        IReadOnlyList<Vector2Int> freeTiles = _gridManager.FreeTilePositions;
        if (freeTiles.Count == 0)
        {
            Debug.Log("No available tiles");
            return;
        }

        // Pick random item and tile
        int itemIdx = Random.Range(0, _spawnableCandidates.Count);
        MergeItemData itemData = _spawnableCandidates[itemIdx];

        int tileIdx = Random.Range(0, freeTiles.Count);
        Vector2Int gridPos = freeTiles[tileIdx];

        // Spawn via GridManager
        Debug.Log($"Spawning {itemData.name} at {gridPos}");
        _gridManager.SpawnItem(itemData, gridPos);

        // Start cooldown
        _nextAvailableTime = Time.time + Mathf.Max(0f, spawnerData.spawnCooldown);
    }

    // Assigned by Tile when this spawner is placed under it
    public void SetTile(Tile tile)
    {
        _tile = tile;
    }

    private void EnsureColliderSized()
    {
        // If we control a BoxCollider2D, size it to match sprite for reliable pointer hits
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
}