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
