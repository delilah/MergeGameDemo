using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

public class TestTilesScript
{
    private readonly List<Object> _toCleanup = new List<Object>();
    private GridManager _gridManager;

    [SetUp]
    public void SetUp()
    {
        var gridManagerGo = new GameObject("GridManager");
        _gridManager = gridManagerGo.AddComponent<GridManager>();
        _toCleanup.Add(gridManagerGo);

        var instanceField = typeof(GridManager).GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
        instanceField.SetValue(null, _gridManager);
    }

    [TearDown]
    public void TearDown()
    {
        var gridInstanceField = typeof(GridManager).GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
        gridInstanceField?.SetValue(null, null);

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
        item.Initialize(data, tile);
        return item;
    }
}