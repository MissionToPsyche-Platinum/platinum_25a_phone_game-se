using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

/// <summary>
/// PlayMode integration test for MinigameC.
/// Simulates all 6 mission steps programmatically and verifies the
/// PhaseCComplete (win) event fires when all steps are completed.
///
/// How to run:
///   Window > General > Test Runner > PlayMode tab
///   Select MinigameCWinConditionTest > run
///
/// Prerequisite: "MinigameC" scene must be listed in File > Build Settings > Scenes In Build.
/// </summary>
public class MinigameCWinConditionTest
{
    private const string SceneName = "MinigameC";

    private PhaseCAssemblyController _assembly;
    private InventoryController      _inventory;
    private ItemDictionary           _itemDict;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        SceneManager.LoadScene(SceneName);
        yield return null;
        yield return null;
        yield return null;

        _assembly  = PhaseCAssemblyController.Instance;
        _inventory = Object.FindFirstObjectByType<InventoryController>();
        _itemDict  = Object.FindFirstObjectByType<ItemDictionary>();

        Assert.IsNotNull(_assembly,  "PhaseCAssemblyController singleton not found after scene load.");
        Assert.IsNotNull(_inventory, "InventoryController not found in MinigameC scene.");
        Assert.IsNotNull(_itemDict,  "ItemDictionary not found in MinigameC scene.");
    }

    [UnityTest]
    public IEnumerator MinigameC_AllStepsCompleted_FiresWinEvent()
    {
        bool winFired = false;
        _assembly.PhaseCComplete += () => winFired = true;

        // C1: Instrument Build - Dr. Sarah Chen
        yield return Deliver(new[] { 1, 2 }, "Dr. Sarah Chen");
        yield return Deliver(new[] { 3, 2 }, "Dr. Sarah Chen");
        yield return Deliver(new[] { 4, 5 }, "Dr. Sarah Chen");
        _assembly.NotifyDialogueClosed("Dr. Sarah Chen");
        yield return null;

        // C2: Communications - Dr. Priya Patel
        yield return Deliver(new[] { 12, 2, 7 }, "Dr. Priya Patel");
        yield return Deliver(new[] { 13, 3 }, "Dr. Priya Patel");
        _assembly.NotifyDialogueClosed("Dr. Priya Patel");
        yield return null;

        // C3: Spacecraft Bus - Dr. Marcus Rodriguez
        yield return Deliver(new[] { 6, 6 }, "Dr. Marcus Rodriguez");
        yield return Deliver(new[] { 10, 11, 2 }, "Dr. Marcus Rodriguez");
        _assembly.NotifyDialogueClosed("Dr. Marcus Rodriguez");
        yield return null;

        // C4: Critical Design Review - Dr. Priya Patel
        _assembly.NotifyDialogueClosed("Dr. Priya Patel");
        yield return null;

        // C5: Systems Integration Review - Dr. James Thompson
        yield return Deliver(new[] { 15, 6, 14 }, "Dr. James Thompson");
        _assembly.NotifyDialogueClosed("Dr. James Thompson");
        yield return null;

        // C6: KDP-D - Dr. Sarah Chen
        _assembly.NotifyDialogueClosed("Dr. Sarah Chen");
        yield return null;

        Assert.IsTrue(winFired,
            "PhaseCComplete event was not fired after all 6 steps completed.");
    }

    [UnityTest]
    public IEnumerator MinigameC_OnlyC1Completed_DoesNotFireWinEvent()
    {
        bool winFired = false;
        _assembly.PhaseCComplete += () => winFired = true;

        yield return Deliver(new[] { 1, 2 }, "Dr. Sarah Chen");
        yield return Deliver(new[] { 3, 2 }, "Dr. Sarah Chen");
        yield return Deliver(new[] { 4, 5 }, "Dr. Sarah Chen");
        _assembly.NotifyDialogueClosed("Dr. Sarah Chen");
        yield return null;

        Assert.IsFalse(winFired,
            "PhaseCComplete should not fire after only completing Step C1.");
    }

    [UnityTest]
    public IEnumerator MinigameC_AfterC1aDelivery_ItemsRemovedFromInventory()
    {
        AddItemToInventory(1);
        AddItemToInventory(2);
        yield return null;

        _assembly.GetDialogueLinesForNpc("Dr. Sarah Chen");
        yield return null;

        bool stillHasItems = _inventory.HasAllItems(new List<int> { 1, 2 });
        Assert.IsFalse(stillHasItems,
            "Inventory should not still contain the delivered items after a successful delivery.");
    }

    [UnityTest]
    public IEnumerator MinigameC_OnSceneLoad_StepIndexIsZero()
    {
        yield return null;
        Assert.AreEqual(0, _assembly.GetCurrentStepIndexForArrow(),
            "Step index should be 0 (C1: Instrument Build) at scene start.");
    }

    private IEnumerator Deliver(int[] itemIds, string npcName)
    {
        foreach (int id in itemIds)
            AddItemToInventory(id);

        yield return null;
        _assembly.GetDialogueLinesForNpc(npcName);
        yield return null;
    }

    private void AddItemToInventory(int itemId)
    {
        GameObject prefab = _itemDict.GetItemPrefab(itemId);
        Assert.IsNotNull(prefab, $"ItemDictionary is missing a prefab for item ID {itemId}.");

        bool added = _inventory.AddItem(prefab);
        Assert.IsTrue(added, $"InventoryController rejected item ID {itemId} (inventory may be full).");
    }
}
