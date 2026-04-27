using NUnit.Framework;
using UnityEngine;
using MergeGame.Data;

public class ItemTests : MergeGameTestBase
{
    private const string Level1Name = "Level 1";
    private const string Level2Name = "Level 2";
    private const string Level3Name = "Level 3";
    private const string MaxLevelName = "Max Level";

    private const string Tile1Name = "Tile1";
    private const string Tile2Name = "Tile2";
    private const string Tile3Name = "Tile3";

    private const string Item1Name = "Item1";
    private const string Item2Name = "Item2";
    private const string Item3Name = "Item3";

    [Test]
    public void ItemsWithSameData_MergeIntoNextItemInChain()
    {
        var level1 = Register(ScriptableObject.CreateInstance<MergeItemData>());
        level1.itemName = Level1Name;
        level1.level = 1;

        var level2 = Register(ScriptableObject.CreateInstance<MergeItemData>());
        level2.itemName = Level2Name;
        level2.level = 2;

        level1.nextItem = level2;

        var tile1 = CreateTile(Tile1Name, Vector3.zero, new Vector2Int(0, 0));
        var tile2 = CreateTile(Tile2Name, Vector3.right, new Vector2Int(1, 0));

        var item1 = CreateItem(Item1Name, level1, tile1);
        var item2 = CreateItem(Item2Name, level1, tile2);

        Assert.AreEqual(item2, tile2.GetItem());

        bool merged = item1.OnDrop(tile2);

        Assert.IsTrue(merged, "OnDrop should report a successful merge");
        Assert.AreEqual(level2, item2.Data, "Target item should have been upgraded to level2");
        Assert.AreEqual(item2, tile2.GetItem(), "Tile2 should still reference the original target item");
        Assert.IsFalse(tile1.HasItem(), "Tile1 should no longer have an item after merge");
    }

    [Test]
    // Dropping an item onto a tile occupied by a different item should fail silently,
    // leaving both tiles unchanged.
    public void Item_DroppedOntoDifferentItem_ReturnsFalse()
    {
        var level1 = Register(ScriptableObject.CreateInstance<MergeItemData>());
        level1.itemName = Level1Name;
        level1.level = 1;

        var level2 = Register(ScriptableObject.CreateInstance<MergeItemData>());
        level2.itemName = Level2Name;
        level2.level = 2;

        var tile1 = CreateTile(Tile1Name, Vector3.zero, new Vector2Int(0, 0));
        var tile2 = CreateTile(Tile2Name, Vector3.right, new Vector2Int(1, 0));

        var item1 = CreateItem(Item1Name, level1, tile1);
        var item2 = CreateItem(Item2Name, level2, tile2);

        bool result = item1.OnDrop(tile2);

        Assert.IsFalse(result);
        Assert.AreEqual(item1, tile1.GetItem(), "Tile1 should still have item1");
        Assert.AreEqual(item2, tile2.GetItem(), "Tile2 should still have item2");
    }

    [Test]
    public void Item_DroppedOntoItself_ReturnsFalse()
    {
        var level1 = Register(ScriptableObject.CreateInstance<MergeItemData>());
        level1.itemName = Level1Name;
        level1.level = 1;

        var tile1 = CreateTile(Tile1Name, Vector3.zero, new Vector2Int(0, 0));
        var item1 = CreateItem(Item1Name, level1, tile1);

        bool result = item1.OnDrop(tile1);

        Assert.IsFalse(result);
        Assert.AreEqual(item1, tile1.GetItem(), "Tile1 should still have item1");
    }

    [Test]
    public void Item_DroppedOntoEmptyTile_MovesItem()
    {
        var level1 = Register(ScriptableObject.CreateInstance<MergeItemData>());
        level1.itemName = Level1Name;
        level1.level = 1;

        var tile1 = CreateTile(Tile1Name, Vector3.zero, new Vector2Int(0, 0));
        var tile2 = CreateTile(Tile2Name, Vector3.right, new Vector2Int(1, 0));

        var item1 = CreateItem(Item1Name, level1, tile1);

        bool result = item1.OnDrop(tile2);

        Assert.IsTrue(result);
        Assert.AreEqual(item1, tile2.GetItem(), "Tile2 should now have item1");
        Assert.IsFalse(tile1.HasItem(), "Tile1 should be empty after move");
    }

    [Test]
    // Merging two max-level items (no nextItem in chain) should still return true
    // but the target item should remain at the same level.
    public void Item_MergeMaxLevelItems_TargetRemainsAtSameLevel()
    {
        var maxLevel = Register(ScriptableObject.CreateInstance<MergeItemData>());
        maxLevel.itemName = MaxLevelName;
        maxLevel.level = 3;
        maxLevel.nextItem = null; // explicit for clarity

        var tile1 = CreateTile(Tile1Name, Vector3.zero, new Vector2Int(0, 0));
        var tile2 = CreateTile(Tile2Name, Vector3.right, new Vector2Int(1, 0));

        var item1 = CreateItem(Item1Name, maxLevel, tile1);
        var item2 = CreateItem(Item2Name, maxLevel, tile2);

        bool result = item1.OnDrop(tile2);

        Assert.IsTrue(result);
        Assert.AreEqual(maxLevel, item2.Data, "Target item should still have max level data");
        Assert.AreEqual(item2, tile2.GetItem(), "Tile2 should still have item2");
        Assert.IsFalse(tile1.HasItem(), "Tile1 should be empty after merge");
    }

    [Test]
    // Verifies that the merge chain works across multiple levels:
    // merging level1+level1 produces level2, then merging level2+level2 produces level3.
    public void Items_MergeChain_ProducesCorrectLevelSequence()
    {
        // --- Arrange ---
        var level1 = Register(ScriptableObject.CreateInstance<MergeItemData>());
        level1.itemName = Level1Name;
        level1.level = 1;

        var level2 = Register(ScriptableObject.CreateInstance<MergeItemData>());
        level2.itemName = Level2Name;
        level2.level = 2;

        var level3 = Register(ScriptableObject.CreateInstance<MergeItemData>());
        level3.itemName = Level3Name;
        level3.level = 3;

        level1.nextItem = level2;
        level2.nextItem = level3;

        var tile1 = CreateTile(Tile1Name, Vector3.zero, new Vector2Int(0, 0));
        var tile2 = CreateTile(Tile2Name, Vector3.right, new Vector2Int(1, 0));
        var tile3 = CreateTile(Tile3Name, Vector3.right * 2, new Vector2Int(2, 0));

        var item1 = CreateItem(Item1Name, level1, tile1);
        var item2 = CreateItem(Item2Name, level1, tile2);
        var item3 = CreateItem(Item3Name, level2, tile3);

        // --- Act & Assert: first merge level1 + level1 -> level2 ---
        item1.OnDrop(tile2);
        Assert.AreEqual(level2, item2.Data, "First merge should produce level2");

        // --- Act & Assert: second merge level2 + level2 -> level3 ---
        item2.OnDrop(tile3);
        Assert.AreEqual(level3, item3.Data, "Second merge should produce level3");

        // --- Assert: tile state ---
        Assert.IsFalse(tile1.HasItem(), "Tile1 should be empty");
        Assert.IsFalse(tile2.HasItem(), "Tile2 should be empty");
        Assert.AreEqual(item3, tile3.GetItem(), "Tile3 should have the final item");
    }
}