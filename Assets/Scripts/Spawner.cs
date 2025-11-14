using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Spawner : MonoBehaviour, IPointerDownHandler
{
    public SpawnerData spawnerData;

    [Header("References")]
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Collider2D _collider;
    [SerializeField] private GridManager _gridManager;

    private Tile _tile;
    private float _nextAvailableTime = 0f;

    private static Spawner _active;

    public void Initialize(SpawnerData data)
    {
        spawnerData = data;
        if (_spriteRenderer != null && spawnerData != null && spawnerData.sprite != null)
        {
            _spriteRenderer.sprite = spawnerData.sprite;
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
        if (_gridManager == null) _gridManager = FindObjectOfType<GridManager>();
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
            Debug.Log("SpawnerData is null; cannot spawn.");
            return;
        }

        if (Time.time < _nextAvailableTime)
        {
            // On cooldown
            return;
        }

        if (_gridManager == null || _gridManager.Tiles == null)
        {
            Debug.Log("GridManager not ready; cannot spawn.");
            return;
        }

        // Validate spawn list
        List<MergeItemData> candidates = new List<MergeItemData>();
        if (spawnerData.spawnableItems != null)
        {
            for (int i = 0; i < spawnerData.spawnableItems.Length; i++)
            {
                if (spawnerData.spawnableItems[i] != null)
                    candidates.Add(spawnerData.spawnableItems[i]);
            }
        }
        if (candidates.Count == 0)
        {
            Debug.Log("Spawner has no valid spawnable items.");
            return;
        }

        // Collect free tiles (no item and no spawner)
        List<System.Collections.Generic.KeyValuePair<UnityEngine.Vector2Int, Tile>> free = new List<System.Collections.Generic.KeyValuePair<UnityEngine.Vector2Int, Tile>>();
        foreach (var kvp in _gridManager.Tiles)
        {
            Tile t = kvp.Value;
            if (t != null && !t.HasItem() && !t.HasSpawner())
            {
                free.Add(kvp);
            }
        }

        if (free.Count == 0)
        {
            Debug.Log("no available tiles");
            return;
        }

        // Pick random item and tile
        int itemIdx = Random.Range(0, candidates.Count);
        MergeItemData itemData = candidates[itemIdx];

        int tileIdx = Random.Range(0, free.Count);
        Vector2Int gridPos = free[tileIdx].Key;

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