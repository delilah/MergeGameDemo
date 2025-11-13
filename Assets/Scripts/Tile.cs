// A tile is a cell, a square of the grid. 
// Tile has colour and is free or occupied.
// Objects on a tile are Items. They are draggable.


using UnityEngine;

public class Tile : MonoBehaviour
{
    [Header("Tile Settings")]
    [SerializeField] private Color _baseColor;
    [SerializeField] private Color _offsetColor;
    [SerializeField] private SpriteRenderer _renderer;
    [SerializeField] private GameObject _highlight;

    private Item _currentItem;
    private Spawner _currentSpawner;

    public bool HasItem() => _currentItem != null;
    public Item GetItem() => _currentItem;
    public bool HasSpawner() => _currentSpawner != null;
    public Spawner GetSpawner() => _currentSpawner;

    public bool IsEmpty => _currentSpawner == null;

    public void Init(bool isAlternateTile)
    {
        _renderer.color = isAlternateTile ? _offsetColor : _baseColor;
    }



public void PlaceSpawner(Spawner spawner)
{
    _currentSpawner = spawner;
    if (spawner == null)
    {
        Debug.Log("Spawner is null, cannot place.");
        return;
    }

    spawner.transform.SetParent(transform);
    spawner.transform.localPosition = new Vector3(0f, 0.0f, 0f);

    // Option A: ensure spawner renders above the tile via sorting layer/order
    var sr = spawner.GetComponent<SpriteRenderer>();
    if (sr != null)
    {
        sr.sortingLayerID = _renderer.sortingLayerID;
        sr.sortingOrder = _renderer.sortingOrder + 1;
    }

    Debug.Log($"Spawner {spawner.name} placed at local position {spawner.transform.localPosition}");

    float tileSize = transform.localScale.x;
    if (sr != null && sr.sprite != null)
    {
        Vector2 spriteSize = sr.sprite.bounds.size;
        float padding = 0.60f;
        float targetSize = tileSize * padding;
        float scaleFactor = targetSize / Mathf.Max(spriteSize.x, spriteSize.y);
        spawner.transform.localScale = new Vector3(scaleFactor, scaleFactor, 1f);
    }
}

public void PlaceItem(Item item)
{
    _currentItem = item;
    if (item == null) return;

    item.transform.SetParent(transform);
    item.transform.localPosition = Vector3.zero + new Vector3(0, 0.05f, 0);

    float tileSize = transform.localScale.x;
    SpriteRenderer sr = item.GetComponent<SpriteRenderer>();
    if (sr != null && sr.sprite != null)
    {
        Vector2 spriteSize = sr.sprite.bounds.size;
        float padding = 0.60f;
        float targetSize = tileSize * padding;
        float scaleFactor = targetSize / Mathf.Max(spriteSize.x, spriteSize.y);
        item.transform.localScale = new Vector3(scaleFactor, scaleFactor, 1f);
    }
}



    public void RemoveItem()
    {
        _currentItem = null;
    }


}
