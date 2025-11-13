using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GridManager _gridManager;
    [SerializeField] private Spawner _spawnerPrefab; // generic prefab
    [SerializeField] private SpawnerData _testSpawnerData;
    [SerializeField] private MergeItemData[] _testItems; // ScriptableObjects for items

    void Start()
    {
        _gridManager.GenerateGrid();
        PlaceTestSpawner();
        PlaceTestItems();
    }

private void PlaceTestSpawner()
{
    Vector2Int pos = new Vector2Int(3, 3);
    Tile tile = _gridManager.GetTileAtPosition(pos);

    if (tile == null)
    {
        Debug.LogWarning("Tile 3,3 does not exist!");
        return;
    }

    Debug.Log($"Tile 3,3 found. HasItem={tile.HasItem()}, HasSpawner={tile.HasSpawner()}, IsEmpty={tile.IsEmpty}");

    if (!tile.IsEmpty)
    {
        Debug.LogWarning("Tile 3,3 is occupied!");
        return;
    }

    Spawner spawner = Instantiate(_spawnerPrefab);
    spawner.Initialize(_testSpawnerData);

    tile.PlaceSpawner(spawner);

    Debug.Log($"Spawner placed on Tile 3,3 at position {spawner.transform.position}");
}


    private void PlaceTestItems()
    {
        // Place items on specific tiles for testing purposes, TODO: remove this
        Vector2Int[] positions = new Vector2Int[]
        {
            new Vector2Int(0, 0),
            new Vector2Int(1, 1),
            new Vector2Int(2, 2)
        };

        for (int i = 0; i < positions.Length && i < _testItems.Length; i++)
        {
            MergeItemData data = _testItems[i];
            _gridManager.SpawnItem(data, positions[i]);
        }
    }
}
