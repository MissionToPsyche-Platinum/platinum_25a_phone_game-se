using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

/// <summary>
/// PlayMode failure test for MinigameC timer expiry.
/// Simulates the exact time over trigger condition and verifies that inventory
/// is cleared and step progress is reset to the beginning.
///
/// How to run:
///   Window > General > Test Runner > PlayMode tab
///   Select MinigameCTimerExpiryTest > run
///
/// Prerequisite: "MinigameC" scene must be listed in File > Build Settings > Scenes In Build.
/// </summary>
public class MinigameCTimerExpiryTest
{
    private const string SceneName = "MinigameC";

    private PhaseCAssemblyController _assembly;
    private InventoryController      _inventory;
    private ItemDictionary           _itemDict;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        LogAssert.ignoreFailingMessages = true;
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

    [TearDown]
    public void TearDown()
    {
        Time.timeScale = 1f;
        LogAssert.ignoreFailingMessages = false;
    }

    [UnityTest]
    public IEnumerator MinigameC_TimerExpiry_ResetsProgressAndClearsInventory()
    {
        MissionTimer timer = MissionTimer.Instance;
        Assert.IsNotNull(timer, "MissionTimer not found in scene.");

        // Add items and advance step C1 partially so inventory and progress are non-empty
        yield return Deliver(new[] { 1, 2 }, "Dr. Sarah Chen");
        AddItemToInventory(3);
        yield return null;

        bool timeExpiredFired = false;
        timer.TimeExpired += () => timeExpiredFired = true;

        // Force currentTime to zero via reflection to trigger the expiry condition
        FieldInfo currentTimeField = typeof(MissionTimer).GetField(
            "currentTime", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.IsNotNull(currentTimeField, "Could not find MissionTimer.currentTime via reflection.");
        currentTimeField.SetValue(timer, 0f);

        // Wait for Update() to detect currentTime <= 0 and call OnTimeExpired()
        yield return null;
        yield return null;

        Assert.IsTrue(timeExpiredFired, "TimeExpired event did not fire when timer reached zero.");
        Assert.IsFalse(timer.IsRunning, "Timer should have stopped after expiry.");
        Assert.AreEqual(0, _assembly.GetCurrentStepIndexForArrow(),
            "Step index should be reset to 0 after timer expiry.");
        Assert.IsFalse(_inventory.HasAllItems(new List<int> { 3 }),
            "Inventory should be cleared after timer expiry.");
    }

    [UnityTest]
    public IEnumerator MinigameC_TimerExpiry_AfterContinue_TimerRestartsAndIsRunning()
    {
        MissionTimer timer = MissionTimer.Instance;
        Assert.IsNotNull(timer, "MissionTimer not found in scene.");

        // Force timer to expire using the same trigger condition
        FieldInfo currentTimeField = typeof(MissionTimer).GetField(
            "currentTime", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.IsNotNull(currentTimeField, "Could not find MissionTimer.currentTime via reflection.");
        currentTimeField.SetValue(timer, 0f);

        // Wait for Update() to detect currentTime <= 0 and call OnTimeExpired()
        yield return null;
        yield return null;

        Assert.IsFalse(timer.IsRunning, "Timer should be stopped after expiry.");
        Assert.AreEqual(0f, Time.timeScale, "Game should be paused after expiry.");

        // Simulate player pressing the Continue button on the popup
        timer.OnTimeUpContinue();
        yield return null;

        Assert.IsTrue(timer.IsRunning, "Timer should be running again after Continue.");
        Assert.AreEqual(1f, Time.timeScale, "Game should be unpaused after Continue.");
        Assert.Greater(timer.GetRemainingTime(), 0f, "Timer should have restarted with remaining time above zero.");
    }

    [UnityTest]
    public IEnumerator MinigameC_TimerExpiry_SecondExpiry_FiresTimeExpiredAgain()
    {
        MissionTimer timer = MissionTimer.Instance;
        Assert.IsNotNull(timer, "MissionTimer not found in scene.");

        FieldInfo currentTimeField = typeof(MissionTimer).GetField(
            "currentTime", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.IsNotNull(currentTimeField, "Could not find MissionTimer.currentTime via reflection.");

        int expiredCount = 0;
        timer.TimeExpired += () => expiredCount++;

        // First expiry
        currentTimeField.SetValue(timer, 0f);
        yield return null;
        yield return null;

        Assert.AreEqual(1, expiredCount, "TimeExpired should have fired once after first expiry.");

        // Player presses Continue - timer restarts
        timer.OnTimeUpContinue();
        yield return null;

        // Second expiry
        currentTimeField.SetValue(timer, 0f);
        yield return null;
        yield return null;

        Assert.AreEqual(2, expiredCount, "TimeExpired should have fired again after second expiry.");
        Assert.AreEqual(0, _assembly.GetCurrentStepIndexForArrow(),
            "Step index should be reset to 0 after second expiry.");
    }

    [UnityTest]
    public IEnumerator MinigameC_TimerExpiry_AfterContinue_TimeFullyRestored()
    {
        MissionTimer timer = MissionTimer.Instance;
        Assert.IsNotNull(timer, "MissionTimer not found in scene.");

        FieldInfo currentTimeField = typeof(MissionTimer).GetField(
            "currentTime", BindingFlags.NonPublic | BindingFlags.Instance);
        FieldInfo startingTimeField = typeof(MissionTimer).GetField(
            "startingTimeSeconds", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.IsNotNull(currentTimeField, "Could not find MissionTimer.currentTime via reflection.");
        Assert.IsNotNull(startingTimeField, "Could not find MissionTimer.startingTimeSeconds via reflection.");

        float startingTime = (float)startingTimeField.GetValue(timer);

        // Force expiry
        currentTimeField.SetValue(timer, 0f);
        yield return null;
        yield return null;

        // Player continues
        timer.OnTimeUpContinue();
        yield return null;

        Assert.AreEqual(startingTime, timer.GetRemainingTime(), 0.01f,
            "Remaining time should be fully restored to the starting value after Continue.");
    }

    [UnityTest]
    public IEnumerator MinigameC_TimerExpiry_WinCondition_StopsTimer()
    {
        MissionTimer timer = MissionTimer.Instance;
        Assert.IsNotNull(timer, "MissionTimer not found in scene.");

        Assert.IsTrue(timer.IsRunning, "Timer should be running at scene start.");

        // Complete all 6 steps to trigger PhaseCComplete
        yield return Deliver(new[] { 1, 2 }, "Dr. Sarah Chen");
        yield return Deliver(new[] { 3, 2 }, "Dr. Sarah Chen");
        yield return Deliver(new[] { 4, 5 }, "Dr. Sarah Chen");
        _assembly.NotifyDialogueClosed("Dr. Sarah Chen");
        yield return null;

        yield return Deliver(new[] { 12, 2, 7 }, "Dr. Priya Patel");
        yield return Deliver(new[] { 13, 3 }, "Dr. Priya Patel");
        _assembly.NotifyDialogueClosed("Dr. Priya Patel");
        yield return null;

        yield return Deliver(new[] { 6, 6 }, "Dr. Marcus Rodriguez");
        yield return Deliver(new[] { 10, 11, 2 }, "Dr. Marcus Rodriguez");
        _assembly.NotifyDialogueClosed("Dr. Marcus Rodriguez");
        yield return null;

        _assembly.NotifyDialogueClosed("Dr. Priya Patel");
        yield return null;

        yield return Deliver(new[] { 15, 6, 14 }, "Dr. James Thompson");
        _assembly.NotifyDialogueClosed("Dr. James Thompson");
        yield return null;

        _assembly.NotifyDialogueClosed("Dr. Sarah Chen");
        yield return null;

        Assert.IsFalse(timer.IsRunning,
            "Timer should stop when PhaseCComplete fires.");
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
