using NUnit.Framework;
using UnityEngine;
using MergeGame.Data;

public class ItemTests : MergeGameTestBase
{
    [Test]
    public void ItemsWithSameData_MergeIntoNextItemInChain()
    {
        var level1 = Register(ScriptableObject.CreateInstance<MergeItemData>());
        level1.itemName = "Level 1";
        level1.level = 1;

        var level2 = Register(ScriptableObject.CreateInstance<MergeItemData>());
        level2.itemName = "Level 2";
        level2.level = 2;

        level1.nextItem = level2;

        var tile1 = CreateTile("Tile1", Vector3.zero, new Vector2Int(0, 0));
        var tile2 = CreateTile("Tile2", Vector3.right, new Vector2Int(1, 0));

        var item1 = CreateItem("Item1", level1, tile1);
        var item2 = CreateItem("Item2", level1, tile2);

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
        level1.itemName = "Level 1";
        level1.level = 1;

        var level2 = Register(ScriptableObject.CreateInstance<MergeItemData>());
        level2.itemName = "Level 2";
        level2.level = 2;

        var tile1 = CreateTile("Tile1", Vector3.zero, new Vector2Int(0, 0));
        var tile2 = CreateTile("Tile2", Vector3.right, new Vector2Int(1, 0));

        var item1 = CreateItem("Item1", level1, tile1);
        var item2 = CreateItem("Item2", level2, tile2);

        bool result = item1.OnDrop(tile2);

        Assert.IsFalse(result);
        Assert.AreEqual(item1, tile1.GetItem(), "Tile1 should still have item1");
        Assert.AreEqual(item2, tile2.GetItem(), "Tile2 should still have item2");
    }

    [Test]
    public void Item_DroppedOntoItself_ReturnsFalse()
    {
        var level1 = Register(ScriptableObject.CreateInstance<MergeItemData>());
        level1.itemName = "Level 1";
        level1.level = 1;

        var tile1 = CreateTile("Tile1", Vector3.zero, new Vector2Int(0, 0));
        var item1 = CreateItem("Item1", level1, tile1);

        bool result = item1.OnDrop(tile1);

        Assert.IsFalse(result);
        Assert.AreEqual(item1, tile1.GetItem(), "Tile1 should still have item1");
    }

    [Test]
    public void Item_DroppedOntoEmptyTile_MovesItem()
    {
        var level1 = Register(ScriptableObject.CreateInstance<MergeItemData>());
        level1.itemName = "Level 1";
        level1.level = 1;

        var tile1 = CreateTile("Tile1", Vector3.zero, new Vector2Int(0, 0));
        var tile2 = CreateTile("Tile2", Vector3.right, new Vector2Int(1, 0));

        var item1 = CreateItem("Item1", level1, tile1);

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
        maxLevel.itemName = "Max Level";
        maxLevel.level = 3;
        maxLevel.nextItem = null; // explicit for clarity

        var tile1 = CreateTile("Tile1", Vector3.zero, new Vector2Int(0, 0));
        var tile2 = CreateTile("Tile2", Vector3.right, new Vector2Int(1, 0));

        var item1 = CreateItem("Item1", maxLevel, tile1);
        var item2 = CreateItem("Item2", maxLevel, tile2);

        bool result = item1.OnDrop(tile2);

        Assert.IsTrue(result);
        Assert.AreEqual(maxLevel, item2.Data, "Target item should still have max level data");
        Assert.AreEqual(item2, tile2.GetItem(), "Tile2 should still have item2");
        Assert.IsFalse(tile1.HasItem(), "Tile1 should be empty after merge");
    }
}