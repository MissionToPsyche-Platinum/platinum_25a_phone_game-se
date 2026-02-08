using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryController : MonoBehaviour
{
    public GameObject inventoryPanel;
    public GameObject slotPrefab;
    public int slotCount;
    public GameObject[] itemPrefabs;

    private ItemDictionary itemDictionary;

    /// <summary>Returns true if inventory contains at least the required count of each item ID (supports duplicates, e.g. Metal Alloy x2).</summary>
    public bool HasAllItems(IReadOnlyList<int> itemIds)
    {
        if (itemIds == null || itemIds.Count == 0) return true;
        if (inventoryPanel == null) return false;

        var needCount = new Dictionary<int, int>();
        foreach (int id in itemIds)
        {
            needCount.TryGetValue(id, out int c);
            needCount[id] = c + 1;
        }

        var haveCount = new Dictionary<int, int>();
        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot == null || slot.currentItem == null) continue;
            Item item = slot.currentItem.GetComponent<Item>();
            if (item == null) continue;
            haveCount.TryGetValue(item.ID, out int c);
            haveCount[item.ID] = c + 1;
        }

        foreach (var kv in needCount)
            if (!haveCount.TryGetValue(kv.Key, out int have) || have < kv.Value) return false;
        return true;
    }

    /// <summary>Removes the required count of each item ID from inventory (supports duplicates). Returns false if any are missing.</summary>
    public bool RemoveItems(IReadOnlyList<int> itemIds)
    {
        if (itemIds == null || itemIds.Count == 0) return true;
        if (inventoryPanel == null) return false;

        var toRemove = new Dictionary<int, int>();
        foreach (int id in itemIds)
        {
            toRemove.TryGetValue(id, out int c);
            toRemove[id] = c + 1;
        }

        var slotsById = new Dictionary<int, List<(Transform slot, GameObject item)>>();
        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot == null || slot.currentItem == null) continue;
            Item item = slot.currentItem.GetComponent<Item>();
            if (item == null) continue;
            int id = item.ID;
            if (!toRemove.ContainsKey(id)) continue;
            if (!slotsById.TryGetValue(id, out var list)) { list = new List<(Transform, GameObject)>(); slotsById[id] = list; }
            list.Add((slotTransform, slot.currentItem));
        }

        foreach (var kv in toRemove)
        {
            if (!slotsById.TryGetValue(kv.Key, out var list) || list.Count < kv.Value) return false;
        }

        foreach (var kv in toRemove)
        {
            var list = slotsById[kv.Key];
            for (int i = 0; i < kv.Value; i++)
            {
                var (slotTransform, itemObj) = list[i];
                Slot slot = slotTransform.GetComponent<Slot>();
                if (slot != null) slot.currentItem = null;
                Destroy(itemObj);
            }
        }
        return true;
    }

    // Start is called before the first frame update
    void Start()
    {
        itemDictionary = FindFirstObjectByType<ItemDictionary>();

        // for (int i = 0; i < slotCount; i++)
        // {
        //     GameObject slotObj = Instantiate(slotPrefab, inventoryPanel.transform);
        //     slotObj.transform.localScale = Vector3.one;
        //     Slot slot = slotObj.GetComponent<Slot>();

        //     if (i < itemPrefabs.Length)
        //     {
        //         GameObject item = Instantiate(itemPrefabs[i], slot.transform);
        //         item.transform.localScale = Vector3.one;
        //         item.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        //         slot.currentItem = item;
        //     }
        // }
    }

    public bool AddItem(GameObject itemPrefab)
    {
        if (inventoryPanel == null)
        {
            Debug.LogError("InventoryController.AddItem: inventoryPanel is null!");
            return false;
        }
        
        if (itemPrefab == null)
        {
            Debug.LogError("InventoryController.AddItem: itemPrefab is null!");
            return false;
        }
        
        Debug.Log($"InventoryController.AddItem: Attempting to add item. Inventory panel has {inventoryPanel.transform.childCount} slots");
        
        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            
            if (slot == null)
            {
                Debug.LogWarning($"InventoryController.AddItem: Slot component not found on {slotTransform.name}");
                continue;
            }
            
            if (slot.currentItem == null)
            {
                Debug.Log($"InventoryController.AddItem: Found empty slot at {slotTransform.name}, adding item...");
                GameObject newItem = Instantiate(itemPrefab, slot.transform);
                
                // Reset scale to (1,1,1) to ensure inventory items are not affected by scene item scaling
                newItem.transform.localScale = Vector3.one;
                
                RectTransform rectTransform = newItem.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.anchoredPosition = Vector2.zero;
                }
                else
                {
                    Debug.LogWarning("InventoryController.AddItem: Item prefab doesn't have RectTransform component");
                }
                
                slot.currentItem = newItem;
                Debug.Log($"InventoryController.AddItem: Item successfully added to slot {slotTransform.name}");
                return true;
            }
        }
        
        Debug.Log("InventoryController.AddItem: Inventory is full - no empty slots found");
        return false;
    }

    public List<InventorySaveData> GetInventoryItems()
    {
        List<InventorySaveData> invData = new List<InventorySaveData>();

        if (inventoryPanel == null)
        {
            return invData;
        }

        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();

            if (slot != null && slot.currentItem != null)
            {
                Item item = slot.currentItem.GetComponent<Item>();
                if (item != null)
                {
                    invData.Add(new InventorySaveData
                    {
                        ItemID = item.ID,
                        SlotIndex = slotTransform.GetSiblingIndex()
                    });
                }
            }
        }

        return invData;
    }

    public void SetInventoryItems(List<InventorySaveData> inventorySaveData)
    {
        if (inventoryPanel == null || slotPrefab == null)
        {
            Debug.LogWarning("InventoryPanel or SlotPrefab is not assigned!");
            return;
        }

        // Clear out inventory panel slots
        foreach (Transform child in inventoryPanel.transform)
        {
            Destroy(child.gameObject);
        }

        // Create new slots
        for (int i = 0; i < slotCount; i++)
        {
            GameObject slotObj = Instantiate(slotPrefab, inventoryPanel.transform);
            if (slotObj == null)
            {
                Debug.LogError($"Failed to instantiate slot prefab at index {i}. Check if SlotPrefab is properly assigned and has all required components.");
            }
        }

        // Populate slots with saved items
        if (inventorySaveData != null && itemDictionary != null)
        {
            foreach (InventorySaveData data in inventorySaveData)
            {
                if (data != null && data.SlotIndex < slotCount && data.SlotIndex >= 0)
                {
                    if (inventoryPanel.transform.childCount > data.SlotIndex)
                    {
                        Slot slot = inventoryPanel.transform.GetChild(data.SlotIndex).GetComponent<Slot>();
                        if (slot != null)
                        {
                            GameObject itemPrefab = itemDictionary.GetItemPrefab(data.ItemID);

                            if (itemPrefab != null)
                            {
                                GameObject item = Instantiate(itemPrefab, slot.transform);
                                item.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                                slot.currentItem = item;
                            }
                        }
                    }
                }
            }
        }
    }
}
