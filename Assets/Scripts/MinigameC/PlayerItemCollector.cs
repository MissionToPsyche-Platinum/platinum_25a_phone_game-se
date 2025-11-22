using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerItemCollector : MonoBehaviour
{
    private InventoryController inventoryController;
    private ItemDictionary itemDictionary;

    void Start()
    {
        inventoryController = FindFirstObjectByType<InventoryController>();
        itemDictionary = FindFirstObjectByType<ItemDictionary>();
        
        if (inventoryController == null)
        {
            Debug.LogError("PlayerItemCollector: InventoryController not found!");
        }
        if (itemDictionary == null)
        {
            Debug.LogError("PlayerItemCollector: ItemDictionary not found!");
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"PlayerItemCollector: Collision detected with {collision.gameObject.name}, Tag: {collision.tag}");
        
        if (collision.CompareTag("Item"))
        {
            Debug.Log("PlayerItemCollector: Item tag matched!");
            Item item = collision.GetComponent<Item>();
            
            if (item == null)
            {
                Debug.LogWarning($"PlayerItemCollector: No Item component found on {collision.gameObject.name}");
                return;
            }
            
            if (inventoryController == null)
            {
                Debug.LogError("PlayerItemCollector: InventoryController is null!");
                return;
            }
            
            if (itemDictionary == null)
            {
                Debug.LogError("PlayerItemCollector: ItemDictionary is null!");
                return;
            }
            
            Debug.Log($"PlayerItemCollector: Item ID: {item.ID}");
            
            // Get the item prefab from ItemDictionary using the item's ID
            GameObject itemPrefab = itemDictionary.GetItemPrefab(item.ID);
            
            if (itemPrefab != null)
            {
                Debug.Log($"PlayerItemCollector: Found item prefab for ID {item.ID}, attempting to add to inventory...");
                bool itemAdded = inventoryController.AddItem(itemPrefab);
                
                if (itemAdded)
                {
                    Debug.Log($"PlayerItemCollector: Item {item.ID} successfully added to inventory!");
                    Destroy(collision.gameObject);
                }
                else
                {
                    Debug.LogWarning("PlayerItemCollector: Failed to add item - inventory might be full");
                }
            }
            else
            {
                Debug.LogWarning($"PlayerItemCollector: Could not find item prefab for ID: {item.ID}");
            }
        }
    }
}

