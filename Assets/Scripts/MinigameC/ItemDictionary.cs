using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDictionary : MonoBehaviour
{
    public List<Item> itemPrefabs;
    
    private Dictionary<int, GameObject> itemDictionary;

    void Awake()
    {
        // Initialize the dictionary
        itemDictionary = new Dictionary<int, GameObject>();
        
        // Auto-increment IDs (starting from 1)
        for (int i = 0; i < itemPrefabs.Count; i++)
        {
            if (itemPrefabs[i] != null)
            {
                itemPrefabs[i].ID = i + 1;
            }
        }
        
        // Populate the dictionary
        foreach (Item item in itemPrefabs)
        {
            itemDictionary[item.ID] = item.gameObject;
        }
    }

    public GameObject GetItemPrefab(int itemId)
    {
        if (itemDictionary.TryGetValue(itemId, out GameObject prefab))
        {
            return prefab;
        }
        else
        {
            Debug.LogWarning($"Item ID {itemId} could not be found in the dictionary");
            return null;
        }
    }
}
