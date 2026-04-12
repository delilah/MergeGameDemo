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

        // Create GridManager and inject
        var gridManagerGo = new GameObject("GridManager");
        gridManagerGo.SetActive(false); // Disable to prevent Awake()
        _gridManager = gridManagerGo.AddComponent<GridManager>();
        _toCleanup.Add(gridManagerGo);
        
        _gridManager.Construct(_gameConfig);

        // Create CollectionManager and inject
        var collectionManagerGo = new GameObject("CollectionManager");
        collectionManagerGo.SetActive(false); // Disable to prevent Awake()
        _collectionManager = collectionManagerGo.AddComponent<CollectionManager>();
        _toCleanup.Add(collectionManagerGo);
        
        _collectionManager.Construct(_gameConfig);

        // Create AudioManager and inject
        var audioManagerGo = new GameObject("AudioManager");
        _audioManager = audioManagerGo.AddComponent<AudioManager>();
        var sfxSource = audioManagerGo.AddComponent<AudioSource>();
        var musicSource = audioManagerGo.AddComponent<AudioSource>();
        
        _audioManager.Construct(_gameConfig, _collectionManager);
        
        // Set AudioSource components (still need reflection for these as they're not in Construct)
        var sfxSourceField = typeof(AudioManager).GetField("_sfxSource", BindingFlags.NonPublic | BindingFlags.Instance);
        sfxSourceField?.SetValue(_audioManager, sfxSource);
        var musicSourceField = typeof(AudioManager).GetField("_musicSource", BindingFlags.NonPublic | BindingFlags.Instance);
        musicSourceField?.SetValue(_audioManager, musicSource);
        
        _toCleanup.Add(audioManagerGo);

        // Create ItemManager and inject BEFORE Awake()
        var itemManagerGo = new GameObject("ItemManager");
        itemManagerGo.SetActive(false); // Disable to prevent Awake()
        _itemManager = itemManagerGo.AddComponent<ItemManager>();
        _toCleanup.Add(itemManagerGo);
        
        _itemManager.Construct(_gridManager, null, _audioManager, _collectionManager, _gameConfig);

        // Re-enable GameObjects before calling Awake
        gridManagerGo.SetActive(true);
        collectionManagerGo.SetActive(true);
        itemManagerGo.SetActive(true);
        
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
        tile.SetGridPosition(gridPos, _gridManager); 

        return tile;
    }

    private Item CreateItem(string name, MergeItemData data, Tile tile)
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