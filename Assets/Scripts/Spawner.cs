using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

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




    private static Spawner _active;

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
        if (_spriteRenderer == null) _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_collider == null) _collider = GetComponent<Collider2D>();
        if (_collider == null) _collider = GetComponentInChildren<Collider2D>();
        if (_collider == null)
        {
            _collider = gameObject.AddComponent<BoxCollider2D>();
        }

        _tile = GetComponentInParent<Tile>();

        EnsureColliderSized();
    }

    public void OnPlaced()
    {
        _baseScale = transform.localScale;

        if (_playIntroAnimation)
            IntroSpawnerAnimation();
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
        if (_touchTween != null && _touchTween.IsActive())
            _touchTween.Kill();

        transform.localScale = _baseScale; // reset scale before animating so it doesn't grow indefinitely if player furiously clicks

        _touchTween = transform.DOScale(_baseScale + Vector3.one * 0.05f, 0.1f)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                    transform.DOScale(_baseScale, 0.1f)
                        .SetEase(Ease.InQuad));
    }


    public void OnPointerDown(PointerEventData eventData)
    {
        StopIntroSpawnerAnimation();
        OnSpawnerTouchAnimation();

        if (_active != this)
        {
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
        if (_spawnerData == null)
        {
            Debug.LogWarning("SpawnerData is null; cannot spawn.");
            return;
        }

        if (Time.time < _nextAvailableTime)
        {
            return;
        }

        GridManager gridManager = GridManager.Instance;
        if (gridManager == null)
        {
            Debug.LogWarning("GridManager not ready; cannot spawn.");
            return;
        }

        if (_spawnableCandidates.Count == 0)
        {
            Debug.LogWarning("Spawner has no valid spawnable items.");
            return;
        }

        IReadOnlyList<Vector2Int> freeTiles = gridManager.FreeTilePositions;
        if (freeTiles.Count == 0)
        {
            Debug.Log("No available tiles");
            return;
        }

        int itemIdx = Random.Range(0, _spawnableCandidates.Count);
        MergeItemData itemData = _spawnableCandidates[itemIdx];

        int tileIdx = Random.Range(0, freeTiles.Count);
        Vector2Int gridPos = freeTiles[tileIdx];

        gridManager.SpawnItem(itemData, gridPos);

        // Start cooldown
        _nextAvailableTime = Time.time + Mathf.Max(0f, _spawnerData.spawnCooldown);
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
        if (_active == this)
        {
            SetSelected(false);
            _active = null;
        }
    }

    // NOTE: added OnDestroy to clear _active if this spawner is destroyed.
    // OnDisable is not reliably called on destroyed objects during scene reloads,
    // so without this _active could hold a stale reference to a destroyed spawner.
    private void OnDestroy()
    {
        transform.DOKill();
        if (_active == this)
            _active = null;
    }
}