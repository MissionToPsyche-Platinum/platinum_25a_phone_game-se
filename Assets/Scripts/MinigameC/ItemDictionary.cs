using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemDictionary : MonoBehaviour
{
    public List<Item> itemPrefabs;

    private Dictionary<int, GameObject> itemDictionary;

    /// <summary>All Phase C item definitions: ID, display name, sprite resource path.</summary>
    private static readonly (int id, string name, string spritePath)[] AllItems =
    {
        (1,  "Magnetometer Parts",  "MinigameC/Items/item_magnetometer_parts_0"),
        (2,  "Wiring",              "MinigameC/Items/item_wiring_0"),
        (3,  "Camera Sensor",       "MinigameC/Items/item_camera_sensor"),
        (4,  "Spectrometer Core",   "MinigameC/Items/item_spectrometer_core"),
        (5,  "Insulation",          "MinigameC/Items/item_insulation"),
        (6,  "Metal Alloy",         "MinigameC/Items/item_metal_alloy"),
        (7,  "Circuit Board",       "MinigameC/Items/item_circuit_board"),
        (8,  "Coffee",              "MinigameC/Items/item_coffee"),
        (9,  "Energy Bar",          "MinigameC/Items/item_energy_bar"),
        (10, "Solar Cells",         "MinigameC/Items/item_solar_cells"),
        (11, "Battery",             "MinigameC/Items/item_battery"),
        (12, "Radio Antenna",       "MinigameC/Items/item_radio_antenna"),
        (13, "Laser Module",        "MinigameC/Items/item_laser_module"),
        (14, "Navigation System",   "MinigameC/Items/item_navigation_system"),
        (15, "Propellant",          "MinigameC/Items/item_propellant"),
    };

    void Awake()
    {
        itemDictionary = new Dictionary<int, GameObject>();

        // Assign IDs to scene-configured prefabs
        for (int i = 0; i < itemPrefabs.Count; i++)
        {
            if (itemPrefabs[i] != null)
            {
                itemPrefabs[i].ID = i + 1;
            }
        }

        // Register scene-configured prefabs
        foreach (Item item in itemPrefabs)
        {
            if (item != null)
                itemDictionary[item.ID] = item.gameObject;
        }

        // Auto-generate any missing items so all 15 IDs are available
        GenerateMissingItems();
    }

    private void GenerateMissingItems()
    {
        // Use first scene prefab as a template (for RectTransform size, CanvasGroup, etc.)
        GameObject template = itemPrefabs != null && itemPrefabs.Count > 0 && itemPrefabs[0] != null
            ? itemPrefabs[0].gameObject
            : null;

        foreach (var def in AllItems)
        {
            if (itemDictionary.ContainsKey(def.id)) continue;

            // Create a new item GameObject
            GameObject itemGo = new GameObject($"Item_{def.name.Replace(" ", "_")}");
            itemGo.tag = "Item";
            itemGo.layer = 5; // UI layer
            itemGo.SetActive(false); // Prefab template stays inactive

            // RectTransform (matches base item prefab)
            RectTransform rect = itemGo.AddComponent<RectTransform>();
            if (template != null)
            {
                RectTransform tRect = template.GetComponent<RectTransform>();
                if (tRect != null)
                {
                    rect.sizeDelta = tRect.sizeDelta;
                    rect.anchorMin = tRect.anchorMin;
                    rect.anchorMax = tRect.anchorMax;
                    rect.pivot = tRect.pivot;
                }
            }
            else
            {
                rect.sizeDelta = new Vector2(100f, 100f);
            }

            // CanvasRenderer + Image with sprite
            itemGo.AddComponent<CanvasRenderer>();
            Image img = itemGo.AddComponent<Image>();
            img.raycastTarget = true;
            img.color = Color.white;

            Sprite sprite = Resources.Load<Sprite>(def.spritePath);
            if (sprite != null)
            {
                img.sprite = sprite;
            }
            else
            {
                Debug.LogWarning($"ItemDictionary: Sprite not found at Resources/{def.spritePath} for {def.name}");
            }

            // CanvasGroup (for drag support)
            itemGo.AddComponent<CanvasGroup>();

            // Item component
            Item itemComp = itemGo.AddComponent<Item>();
            itemComp.ID = def.id;
            itemComp.displayName = def.name;

            // BoxCollider2D for pickup detection
            BoxCollider2D col = itemGo.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(1f, 1f);

            // Keep it alive across the session (don't destroy the template)
            DontDestroyOnLoad(itemGo);

            // Register
            itemDictionary[def.id] = itemGo;

            // Also add to itemPrefabs list so index-based lookups work
            while (itemPrefabs.Count < def.id)
                itemPrefabs.Add(null);
            itemPrefabs[def.id - 1] = itemComp;
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

    /// <summary>Returns display name for an item ID. Checks prefab displayName first, then master list.</summary>
    public string GetDisplayName(int itemId)
    {
        // Check live prefab first
        if (itemPrefabs != null)
        {
            int index = itemId - 1;
            if (index >= 0 && index < itemPrefabs.Count && itemPrefabs[index] != null)
            {
                if (!string.IsNullOrEmpty(itemPrefabs[index].displayName))
                    return itemPrefabs[index].displayName;
            }
        }

        // Fallback to master list
        foreach (var def in AllItems)
        {
            if (def.id == itemId) return def.name;
        }

        return $"Item {itemId}";
    }
}
