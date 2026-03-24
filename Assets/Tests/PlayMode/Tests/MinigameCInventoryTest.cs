using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

/// <summary>
/// PlayMode tests for InventoryController item collection logic in MinigameC.
/// Covers: multiple items, inventory full, duplicate items, item drop, and related scenarios.
///
/// How to run:
///   Window > General > Test Runner > PlayMode tab
///   Select MinigameCInventoryTest > run
///
/// Prerequisite: "MinigameC" scene must be listed in File > Build Settings > Scenes In Build.
/// </summary>
public class MinigameCInventoryTest
{
    private const string SceneName = "MinigameC";

    private InventoryController _inventory;
    private ItemDictionary      _itemDict;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        LogAssert.ignoreFailingMessages = true;
        SceneManager.LoadScene(SceneName);
        yield return null;
        yield return null;
        yield return null;

        _inventory = Object.FindFirstObjectByType<InventoryController>();
        _itemDict  = Object.FindFirstObjectByType<ItemDictionary>();

        Assert.IsNotNull(_inventory, "InventoryController not found in MinigameC scene.");
        Assert.IsNotNull(_itemDict,  "ItemDictionary not found in MinigameC scene.");
    }

    [TearDown]
    public void TearDown()
    {
        LogAssert.ignoreFailingMessages = false;
    }

    [UnityTest]
    public IEnumerator Inventory_MultipleItems_AllAddedSuccessfully()
    {
        bool added1 = AddItem(1);
        bool added2 = AddItem(2);
        bool added3 = AddItem(3);
        yield return null;

        Assert.IsTrue(added1, "Item 1 (Magnetometer Parts) should be added.");
        Assert.IsTrue(added2, "Item 2 (Wiring) should be added.");
        Assert.IsTrue(added3, "Item 3 (Camera Sensor) should be added.");
        Assert.IsTrue(_inventory.HasAllItems(new List<int> { 1, 2, 3 }),
            "Inventory should contain all three added items.");
    }

    [UnityTest]
    public IEnumerator Inventory_Full_FifthUniqueTypeRejected()
    {
        AddItem(1);
        AddItem(2);
        AddItem(3);
        AddItem(4);
        yield return null;

        Assert.IsTrue(_inventory.IsInventoryFull(),
            "Inventory should be full after 4 unique item types.");

        bool added5 = AddItem(5);
        yield return null;

        Assert.IsFalse(added5,
            "Adding a 5th unique item type should be rejected when inventory is full.");
    }

    [UnityTest]
    public IEnumerator Inventory_DuplicateItems_SameTypeTwiceAccepted()
    {
        bool first  = AddItem(6);
        bool second = AddItem(6);
        yield return null;

        Assert.IsTrue(first,  "First Metal Alloy should be accepted.");
        Assert.IsTrue(second, "Second Metal Alloy (duplicate) should also be accepted.");
        Assert.IsTrue(_inventory.HasAllItems(new List<int> { 6, 6 }),
            "Inventory should contain two Metal Alloy items.");
    }

    [UnityTest]
    public IEnumerator Inventory_DropItem_RemovesOneInstance()
    {
        AddItem(1);
        yield return null;

        Assert.IsTrue(_inventory.HasAllItems(new List<int> { 1 }),
            "Item 1 should be in inventory before drop.");

        bool dropped = _inventory.DropItem(1);
        yield return null;

        Assert.IsTrue(dropped, "DropItem should return true for an item that exists.");
        Assert.IsFalse(_inventory.HasAllItems(new List<int> { 1 }),
            "Item 1 should no longer be in inventory after drop.");
    }

    [UnityTest]
    public IEnumerator Inventory_DropItem_NonExistentItem_ReturnsFalse()
    {
        yield return null;

        bool dropped = _inventory.DropItem(7);

        Assert.IsFalse(dropped,
            "DropItem should return false when the item is not in inventory.");
    }

    [UnityTest]
    public IEnumerator Inventory_Full_DropThenAdd_NewTypeAccepted()
    {
        AddItem(1);
        AddItem(2);
        AddItem(3);
        AddItem(4);
        yield return null;

        Assert.IsTrue(_inventory.IsInventoryFull(), "Inventory should be full.");

        _inventory.DropItem(1);
        yield return null;

        Assert.IsFalse(_inventory.IsInventoryFull(),
            "Inventory should no longer be full after dropping one item type.");

        bool added = AddItem(5);
        yield return null;

        Assert.IsTrue(added,
            "A new unique item type should be accepted after making room by dropping.");
    }

    [UnityTest]
    public IEnumerator Inventory_HasAllItems_DuplicateRequirement_ReturnsTrueWhenBothPresent()
    {
        AddItem(6);
        AddItem(6);
        yield return null;

        Assert.IsTrue(_inventory.HasAllItems(new List<int> { 6, 6 }),
            "HasAllItems should return true when two Metal Alloy items are required and both are present.");
    }

    private bool AddItem(int itemId)
    {
        GameObject prefab = _itemDict.GetItemPrefab(itemId);
        Assert.IsNotNull(prefab, $"ItemDictionary is missing a prefab for item ID {itemId}.");
        return _inventory.AddItem(prefab);
    }
}
