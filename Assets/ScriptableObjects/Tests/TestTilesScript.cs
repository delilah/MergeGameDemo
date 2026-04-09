using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;
using MergeGame.Entities;
using MergeGame.Grid;
using MergeGame.Systems;
using MergeGame.Data;

public class TestTilesScript
{
    private readonly List<Object> _toCleanup = new List<Object>();
    private GridManager _gridManager;
    private AudioManager _audioManager;
    private CollectionManager _collectionManager;
    private ItemManager _itemManager;
    private GameConfig _gameConfig;

    [SetUp]
    public void SetUp()
    {
        // Create GameConfig
        _gameConfig = Register(ScriptableObject.CreateInstance<GameConfig>());
        _gameConfig.itemsToWin = 10;

        // Initialize SfxConfig to prevent null reference in AudioManager
        _gameConfig.sfx = new SfxConfig();

        // Create GridManager and inject BEFORE Awake()
        var gridManagerGo = new GameObject("GridManager");
        _gridManager = gridManagerGo.AddComponent<GridManager>();
        _toCleanup.Add(gridManagerGo);
        
        // Manual injection BEFORE Unity calls Awake
        var gridConfigField = typeof(GridManager).GetField("_gameConfig", BindingFlags.NonPublic | BindingFlags.Instance);
        gridConfigField?.SetValue(_gridManager, _gameConfig);

        // Create CollectionManager and inject
        var collectionManagerGo = new GameObject("CollectionManager");
        _collectionManager = collectionManagerGo.AddComponent<CollectionManager>();
        _toCleanup.Add(collectionManagerGo);
        
        var collectionConfigField = typeof(CollectionManager).GetField("_gameConfig", BindingFlags.NonPublic | BindingFlags.Instance);
        collectionConfigField?.SetValue(_collectionManager, _gameConfig);

        // Create AudioManager and inject
        var audioManagerGo = new GameObject("AudioManager");
        _audioManager = audioManagerGo.AddComponent<AudioManager>();
        var sfxSource = audioManagerGo.AddComponent<AudioSource>();
        var musicSource = audioManagerGo.AddComponent<AudioSource>();
        
        var audioConfigField = typeof(AudioManager).GetField("_config", BindingFlags.NonPublic | BindingFlags.Instance);
        audioConfigField?.SetValue(_audioManager, _gameConfig);
        var audioCollectionField = typeof(AudioManager).GetField("_collectionManager", BindingFlags.NonPublic | BindingFlags.Instance);
        audioCollectionField?.SetValue(_audioManager, _collectionManager);
        var sfxSourceField = typeof(AudioManager).GetField("_sfxSource", BindingFlags.NonPublic | BindingFlags.Instance);
        sfxSourceField?.SetValue(_audioManager, sfxSource);
        var musicSourceField = typeof(AudioManager).GetField("_musicSource", BindingFlags.NonPublic | BindingFlags.Instance);
        musicSourceField?.SetValue(_audioManager, musicSource);
        
        _toCleanup.Add(audioManagerGo);

        // Create ItemManager and inject BEFORE Awake()
        var itemManagerGo = new GameObject("ItemManager");
        _itemManager = itemManagerGo.AddComponent<ItemManager>();
        _toCleanup.Add(itemManagerGo);
        
        // Manual injection BEFORE Unity calls Awake
        var imGridField = typeof(ItemManager).GetField("_gridManager", BindingFlags.NonPublic | BindingFlags.Instance);
        imGridField?.SetValue(_itemManager, _gridManager);
        var imAudioField = typeof(ItemManager).GetField("_audioManager", BindingFlags.NonPublic | BindingFlags.Instance);
        imAudioField?.SetValue(_itemManager, _audioManager);
        var imCollectionField = typeof(ItemManager).GetField("_collectionManager", BindingFlags.NonPublic | BindingFlags.Instance);
        imCollectionField?.SetValue(_itemManager, _collectionManager);
        var imContainerField = typeof(ItemManager).GetField("_container", BindingFlags.NonPublic | BindingFlags.Instance);
        imContainerField?.SetValue(_itemManager, null); // Not used in tests
        var imGameConfigField = typeof(ItemManager).GetField("_gameConfig", BindingFlags.NonPublic | BindingFlags.Instance);
        imGameConfigField?.SetValue(_itemManager, _gameConfig);

        // Now manually call Awake after injection is complete
        var gridAwakeMethod = typeof(GridManager).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance);
        gridAwakeMethod?.Invoke(_gridManager, null);
        
        var itemAwakeMethod = typeof(ItemManager).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance);
        itemAwakeMethod?.Invoke(_itemManager, null);
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

    [Test]
    public void ItemsWithSameData_MergeIntoNextItemInChain()
    {
        // --- Arrange: create merge chain data ---
        var level1 = Register(ScriptableObject.CreateInstance<MergeItemData>());
        level1.itemName = "Level 1";
        level1.level = 1;

        var level2 = Register(ScriptableObject.CreateInstance<MergeItemData>());
        level2.itemName = "Level 2";
        level2.level = 2;

        // chain: level1 -> level2
        level1.nextItem = level2;

        // --- Arrange: create tiles ---
        var tile1 = CreateTile("Tile1", Vector3.zero, new Vector2Int(0, 0));
        var tile2 = CreateTile("Tile2", Vector3.right, new Vector2Int(1, 0));

        // --- Arrange: create items on those tiles ---
        var item1 = CreateItem("Item1", level1, tile1);
        var item2 = CreateItem("Item2", level1, tile2);

        // Sanity: tile2 has item2
        Assert.AreEqual(item2, tile2.GetItem());

        // --- Act: simulate dropping item1 onto tile2 (where item2 lives) ---
        bool merged = item1.OnDrop(tile2);

        // --- Assert ---
        Assert.IsTrue(merged, "OnDrop should report a successful merge");

        // target item should now have the NEXT data in the chain
        Assert.AreEqual(level2, item2.Data, "Target item should have been upgraded to level2");

        // tile2 should still have item2, not item1
        Assert.AreEqual(item2, tile2.GetItem(), "Tile2 should still reference the original target item");

        // tile1 should be empty
        Assert.IsFalse(tile1.HasItem(), "Tile1 should no longer have an item after merge");
    }

    private T Register<T>(T obj) where T : Object
    {
        _toCleanup.Add(obj);
        return obj;
    }

    private Tile CreateTile(string name, Vector3 position, Vector2Int gridPos)
    {
        var go = Register(new GameObject(name));
        var tile = go.AddComponent<Tile>();

        var tileRenderer = go.AddComponent<SpriteRenderer>();
        var rendererField = typeof(Tile).GetField("_renderer", BindingFlags.NonPublic | BindingFlags.Instance);
        rendererField.SetValue(tile, tileRenderer);

        go.transform.position = position;

        // Required: set grid position and gridManager reference so Tile internals work
        tile.SetGridPosition(gridPos, _gridManager);

        return tile;
    }

    private Item CreateItem(string name, MergeItemData data, Tile tile)
    {
        var go = Register(new GameObject(name));
        go.AddComponent<SpriteRenderer>();
        go.AddComponent<BoxCollider2D>();

        var item = go.AddComponent<Item>();

        var gridManagerField = typeof(Item).GetField("_gridManager", BindingFlags.NonPublic | BindingFlags.Instance);
        gridManagerField?.SetValue(item, _gridManager);
        var itemManagerField = typeof(Item).GetField("_itemManager", BindingFlags.NonPublic | BindingFlags.Instance);
        itemManagerField?.SetValue(item, _itemManager);

        item.Initialize(data, tile);
        return item;
    }
}