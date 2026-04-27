using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;
using MergeGame.Entities;
using MergeGame.Grid;
using MergeGame.Systems;
using MergeGame.Data;

public abstract class MergeGameTestBase
{
    protected readonly List<Object> _toCleanup = new List<Object>();
    protected GridManager _gridManager;
    protected AudioManager _audioManager;
    protected CollectionManager _collectionManager;
    protected ItemManager _itemManager;
    protected GameConfig _gameConfig;

    private const string GridManagerName = "GridManager";
    private const string CollectionManagerName = "CollectionManager";
    private const string AudioManagerName = "AudioManager";
    private const string ItemManagerName = "ItemManager";

    [SetUp]
    public void SetUp()
    {
        _gameConfig = Register(ScriptableObject.CreateInstance<GameConfig>());

        var gridManagerGo = new GameObject(GridManagerName);
        gridManagerGo.SetActive(false);
        _gridManager = gridManagerGo.AddComponent<GridManager>();
        _toCleanup.Add(gridManagerGo);
        _gridManager.Construct(_gameConfig);

        var collectionManagerGo = new GameObject(CollectionManagerName);
        collectionManagerGo.SetActive(false);
        _collectionManager = collectionManagerGo.AddComponent<CollectionManager>();
        _toCleanup.Add(collectionManagerGo);
        _collectionManager.Construct(_gameConfig);

        var audioManagerGo = new GameObject(AudioManagerName);
        _audioManager = audioManagerGo.AddComponent<AudioManager>();
        _audioManager.Construct(_gameConfig, _collectionManager);
        _toCleanup.Add(audioManagerGo);

        var itemManagerGo = new GameObject(ItemManagerName);
        itemManagerGo.SetActive(false);
        _itemManager = itemManagerGo.AddComponent<ItemManager>();
        _toCleanup.Add(itemManagerGo);
        _itemManager.Construct(_gridManager, null, _audioManager, _collectionManager, _gameConfig);

        gridManagerGo.SetActive(true);
        collectionManagerGo.SetActive(true);
        itemManagerGo.SetActive(true);

        _gridManager.Initialize();
        _itemManager.Initialize();
    }

    [TearDown]
    public void TearDown()
    {
        foreach (var obj in _toCleanup)
        {
            if (obj != null)
            {
                Object.DestroyImmediate(obj);
            }
        }

        _toCleanup.Clear();
    }

    protected T Register<T>(T obj) where T : Object
    {
        _toCleanup.Add(obj);
        return obj;
    }

    protected Tile CreateTile(string name, Vector3 position, Vector2Int gridPos)
    {
        var go = Register(new GameObject(name));
        go.AddComponent<SpriteRenderer>(); // picked up by Tile.Awake via GetComponent
        var tile = go.AddComponent<Tile>(); // Awake fires here, _renderer auto-resolved
        go.transform.position = position;
        tile.SetGridPosition(gridPos, _gridManager);
        return tile;
    }

    protected Item CreateItem(string name, MergeItemData data, Tile tile)
    {
        var go = Register(new GameObject(name));
        go.AddComponent<SpriteRenderer>();
        go.AddComponent<BoxCollider2D>();

        var item = go.AddComponent<Item>();
        item.Construct(_gridManager, _itemManager, _gameConfig);
        item.Initialize(data, tile);
        return item;
    }
}