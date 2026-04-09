using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Clean, single-panel inventory UI for Phase C. Replaces the tab-based inventory.
/// Auto-creates on scene load; hides legacy inventory tab.
/// Toggled on/off with I key (hidden by default).
/// </summary>
public class PhaseCInventoryUI : MonoBehaviour
{
    private const string TargetSceneName = "MinigameC";
    private const string CanvasName = "PhaseCInventoryCanvas";
    private const int SlotCount = 4;
    private const float SlotSize = 80f;
    private const float SlotSpacing = 10f;
    private const int SlotsPerRow = 4;

    private Canvas canvas;
    private GameObject canvasObject;
    private GameObject overlayObject;
    private GameObject panelObject;
    private RectTransform slotsContainer;
    private List<RectTransform> slots = new List<RectTransform>();
    private List<Image> slotImages = new List<Image>();
    private InventoryController legacyController;
    private ItemDictionary itemDictionary;
    private bool initialized;
    private Text statusText;

    // Track items in our slots (by item ID, 0 = empty)
    private int[] slotItemIds = new int[SlotCount];
    private GameObject[] slotItemObjects = new GameObject[SlotCount];
    private int[] slotItemCounts = new int[SlotCount]; // Track quantity for stacked items

    // Dirty-check cache: avoids rebuilding UI every frame when nothing changed
    private Dictionary<int, int> _lastSyncedCounts = new Dictionary<int, int>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void EnsureInventoryUI()
    {
        SceneManager.sceneLoaded += (scene, _) =>
        {
            if (scene.name != TargetSceneName) return;
            if (FindFirstObjectByType<PhaseCInventoryUI>() != null) return;
            new GameObject("PhaseCInventoryUI").AddComponent<PhaseCInventoryUI>();
        };
    }

    private void Awake()
    {
        if (SceneManager.GetActiveScene().name != TargetSceneName)
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        HideLegacyInventoryTab();
        itemDictionary = FindFirstObjectByType<ItemDictionary>();
        legacyController = FindFirstObjectByType<InventoryController>();
        CreateInventoryCanvas();

        // Start hidden - player presses I to open
        canvasObject.SetActive(false);
        initialized = true;
    }

    private static readonly KeyCode[] DropKeys = { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4 };

    private void Update()
    {
        if (!initialized) return;

        if (Input.GetKeyDown(KeyCode.I))
        {
            bool show = !canvasObject.activeSelf;
            canvasObject.SetActive(show);

            if (show)
            {
                SyncFromLegacyInventory();
            }
        }

        // Press 1-4 while inventory is open to drop the item in that slot
        if (canvasObject.activeSelf && legacyController != null)
        {
            for (int i = 0; i < SlotCount; i++)
            {
                if (Input.GetKeyDown(DropKeys[i]) && slotItemIds[i] != 0)
                {
                    int droppedId = slotItemIds[i];
                    bool dropped = legacyController.DropItem(droppedId);
                    if (dropped)
                    {
                        string dropName = itemDictionary != null
                            ? itemDictionary.GetDisplayName(droppedId)
                            : $"Item {droppedId}";
                        Color dropColor = itemDictionary != null
                            ? itemDictionary.GetItemColor(droppedId)
                            : PhaseCUITheme.AccentGold;
                        PhaseCItemFeedbackUI.ShowDrop(dropName, dropColor);
                    }
                }
            }
        }
    }

    private void HideLegacyInventoryTab()
    {
        // Only hide the inventory-specific tab and page, keep the rest of the tab menu working
        GameObject inventoryTab = GameObject.Find("InventoryTab");
        if (inventoryTab != null) inventoryTab.SetActive(false);

        GameObject inventoryPage = GameObject.Find("InventoryPage");
        if (inventoryPage != null) inventoryPage.SetActive(false);
    }

    private void CreateInventoryCanvas()
    {
        // Destroy existing if any
        GameObject existing = GameObject.Find(CanvasName);
        if (existing != null) Destroy(existing);

        // Create canvas
        GameObject canvasGo = new GameObject(CanvasName);
        canvasObject = canvasGo;
        canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 15;

        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(PhaseCUITheme.RefWidth, PhaseCUITheme.RefHeight);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGo.AddComponent<GraphicRaycaster>();

        // Semi-transparent overlay background
        overlayObject = new GameObject("Overlay");
        overlayObject.transform.SetParent(canvasGo.transform, false);
        Image overlayImg = overlayObject.AddComponent<Image>();
        overlayImg.color = new Color(0f, 0f, 0f, 0.5f);
        overlayImg.raycastTarget = true;
        RectTransform overlayRect = overlayObject.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;

        // Calculate panel size based on slots
        float gridWidth = (SlotSize * SlotsPerRow) + (SlotSpacing * (SlotsPerRow - 1));
        int rowCount = Mathf.CeilToInt((float)SlotCount / SlotsPerRow);
        float gridHeight = (SlotSize * rowCount) + (SlotSpacing * (rowCount - 1));
        float panelPadding = 24f;
        float titleHeight = 40f;
        float statusHeight = 28f;
        float hintHeight = 28f;
        float panelWidth = gridWidth + panelPadding * 2;
        float panelHeight = titleHeight + gridHeight + statusHeight + hintHeight + panelPadding * 2;

        // Centered panel
        panelObject = new GameObject("InventoryPanel");
        panelObject.transform.SetParent(canvasGo.transform, false);

        Image panelBg = panelObject.AddComponent<Image>();
        panelBg.color = PhaseCUITheme.PanelBg;
        panelBg.raycastTarget = true;

        RectTransform panelRect = panelObject.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(panelWidth, panelHeight);
        panelRect.anchoredPosition = Vector2.zero;

        // Border outline
        GameObject borderGo = new GameObject("Border");
        borderGo.transform.SetParent(panelObject.transform, false);
        Image borderImg = borderGo.AddComponent<Image>();
        borderImg.color = PhaseCUITheme.PanelBorder;
        borderImg.raycastTarget = false;
        RectTransform borderRect = borderGo.GetComponent<RectTransform>();
        borderRect.anchorMin = Vector2.zero;
        borderRect.anchorMax = Vector2.one;
        borderRect.offsetMin = new Vector2(-2f, -2f);
        borderRect.offsetMax = new Vector2(2f, 2f);
        borderGo.transform.SetAsFirstSibling();

        // Title
        GameObject titleGo = new GameObject("Title");
        titleGo.transform.SetParent(panelObject.transform, false);
        Text titleText = titleGo.AddComponent<Text>();
        titleText.text = "INVENTORY";
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.fontSize = (int)PhaseCUITheme.GuideStepTitleSize;
        titleText.fontStyle = FontStyle.Bold;
        titleText.color = PhaseCUITheme.AccentGold;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.raycastTarget = false;
        RectTransform titleRect = titleGo.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.sizeDelta = new Vector2(0f, titleHeight);
        titleRect.anchoredPosition = new Vector2(0f, -panelPadding);

        // Slots container - centered inside panel, below title
        GameObject slotsGo = new GameObject("Slots");
        slotsGo.transform.SetParent(panelObject.transform, false);
        slotsContainer = slotsGo.AddComponent<RectTransform>();
        slotsContainer.anchorMin = new Vector2(0.5f, 1f);
        slotsContainer.anchorMax = new Vector2(0.5f, 1f);
        slotsContainer.pivot = new Vector2(0.5f, 1f);
        slotsContainer.sizeDelta = new Vector2(gridWidth, gridHeight);
        slotsContainer.anchoredPosition = new Vector2(0f, -(panelPadding + titleHeight));

        // Create slot grid
        for (int i = 0; i < SlotCount; i++)
        {
            CreateSlot(i, gridWidth, gridHeight);
        }

        // Status text - shown in red when inventory is full
        GameObject statusGo = new GameObject("StatusText");
        statusGo.transform.SetParent(panelObject.transform, false);
        statusText = statusGo.AddComponent<Text>();
        statusText.text = "";
        statusText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        statusText.fontSize = (int)PhaseCUITheme.GuideCaptionSize;
        statusText.fontStyle = FontStyle.Bold;
        statusText.color = PhaseCUITheme.TextError;
        statusText.alignment = TextAnchor.MiddleCenter;
        statusText.raycastTarget = false;
        RectTransform statusRect = statusGo.GetComponent<RectTransform>();
        statusRect.anchorMin = new Vector2(0f, 0f);
        statusRect.anchorMax = new Vector2(1f, 0f);
        statusRect.pivot = new Vector2(0.5f, 0f);
        statusRect.sizeDelta = new Vector2(0f, statusHeight);
        statusRect.anchoredPosition = new Vector2(0f, hintHeight + 4f);

        // Hint text at bottom
        GameObject hintGo = new GameObject("Hint");
        hintGo.transform.SetParent(panelObject.transform, false);
        Text hintText = hintGo.AddComponent<Text>();
        hintText.text = "Press I to close  |  Press 1-4 to drop item";
        hintText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        hintText.fontSize = (int)PhaseCUITheme.FontSizeBadge;
        hintText.color = PhaseCUITheme.TextSecondary;
        hintText.alignment = TextAnchor.MiddleCenter;
        hintText.raycastTarget = false;
        RectTransform hintRect = hintGo.GetComponent<RectTransform>();
        hintRect.anchorMin = new Vector2(0f, 0f);
        hintRect.anchorMax = new Vector2(1f, 0f);
        hintRect.pivot = new Vector2(0.5f, 0f);
        hintRect.sizeDelta = new Vector2(0f, hintHeight);
        hintRect.anchoredPosition = new Vector2(0f, 6f);
    }

    private void CreateSlot(int index, float containerWidth, float containerHeight)
    {
        GameObject slotGo = new GameObject($"Slot_{index}");
        slotGo.transform.SetParent(slotsContainer, false);

        Image slotBg = slotGo.AddComponent<Image>();
        slotBg.color = new Color(0.12f, 0.14f, 0.22f, 0.9f);
        slotBg.raycastTarget = true;

        RectTransform slotRect = slotGo.GetComponent<RectTransform>();
        int row = index / SlotsPerRow;
        int col = index % SlotsPerRow;

        // Position slots from top-left of the container
        float x = col * (SlotSize + SlotSpacing);
        float y = -(row * (SlotSize + SlotSpacing));

        slotRect.anchorMin = new Vector2(0f, 1f);
        slotRect.anchorMax = new Vector2(0f, 1f);
        slotRect.pivot = new Vector2(0f, 1f);
        slotRect.sizeDelta = new Vector2(SlotSize, SlotSize);
        slotRect.anchoredPosition = new Vector2(x, y);

        // Inner border highlight
        GameObject innerBorderGo = new GameObject("InnerBorder");
        innerBorderGo.transform.SetParent(slotGo.transform, false);
        Image innerBorder = innerBorderGo.AddComponent<Image>();
        innerBorder.color = new Color(PhaseCUITheme.PanelBorder.r, PhaseCUITheme.PanelBorder.g, PhaseCUITheme.PanelBorder.b, 0.4f);
        innerBorder.raycastTarget = false;
        RectTransform innerRect = innerBorderGo.GetComponent<RectTransform>();
        innerRect.anchorMin = Vector2.zero;
        innerRect.anchorMax = Vector2.one;
        innerRect.offsetMin = new Vector2(2f, 2f);
        innerRect.offsetMax = new Vector2(-2f, -2f);

        // Slot number (subtle)
        GameObject numGo = new GameObject("SlotNum");
        numGo.transform.SetParent(slotGo.transform, false);
        Text numText = numGo.AddComponent<Text>();
        numText.text = (index + 1).ToString();
        numText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        numText.fontSize = 12;
        numText.color = new Color(0.4f, 0.45f, 0.55f, 0.5f);
        numText.alignment = TextAnchor.LowerRight;
        numText.raycastTarget = false;
        RectTransform numRect = numGo.GetComponent<RectTransform>();
        numRect.anchorMin = Vector2.zero;
        numRect.anchorMax = Vector2.one;
        numRect.offsetMin = new Vector2(4f, 4f);
        numRect.offsetMax = new Vector2(-4f, -4f);

        slots.Add(slotRect);
        slotImages.Add(slotBg);
    }

    private void LateUpdate()
    {
        if (!initialized) return;
        if (canvasObject == null || !canvasObject.activeSelf) return;

        if (HasInventoryChanged())
            SyncFromLegacyInventory();
    }

    /// <summary>Returns true if the legacy inventory contents differ from the last synced state.</summary>
    private bool HasInventoryChanged()
    {
        if (legacyController == null || legacyController.inventoryPanel == null) return false;

        var current = new Dictionary<int, int>();
        foreach (Transform slotTransform in legacyController.inventoryPanel.transform)
        {
            Slot s = slotTransform.GetComponent<Slot>();
            if (s == null || s.currentItem == null) continue;
            Item it = s.currentItem.GetComponent<Item>();
            if (it == null) continue;
            current.TryGetValue(it.ID, out int c);
            current[it.ID] = c + 1;
        }

        if (current.Count != _lastSyncedCounts.Count) return true;
        foreach (var kv in current)
            if (!_lastSyncedCounts.TryGetValue(kv.Key, out int prev) || prev != kv.Value) return true;
        return false;
    }

    private void SyncFromLegacyInventory()
    {
        if (legacyController == null || legacyController.inventoryPanel == null) return;

        // Clear display
        for (int i = 0; i < SlotCount; i++)
        {
            if (slotItemObjects[i] != null)
            {
                Destroy(slotItemObjects[i]);
                slotItemObjects[i] = null;
            }
            slotItemIds[i] = 0;
            slotItemCounts[i] = 0;

            // Reset slot color
            if (i < slotImages.Count)
            {
                slotImages[i].color = new Color(0.12f, 0.14f, 0.22f, 0.9f);
            }
        }

        // Count items from legacy inventory and stack them
        Dictionary<int, int> itemCounts = new Dictionary<int, int>();
        List<Item> uniqueItems = new List<Item>();
        
        foreach (Transform slotTransform in legacyController.inventoryPanel.transform)
        {
            Slot legacySlot = slotTransform.GetComponent<Slot>();
            if (legacySlot != null && legacySlot.currentItem != null)
            {
                Item item = legacySlot.currentItem.GetComponent<Item>();
                if (item != null)
                {
                    if (!itemCounts.ContainsKey(item.ID))
                    {
                        itemCounts[item.ID] = 0;
                        uniqueItems.Add(item);
                    }
                    itemCounts[item.ID]++;
                }
            }
        }

        // Display stacked items
        int slotIndex = 0;
        foreach (Item item in uniqueItems)
        {
            if (slotIndex >= SlotCount) break;

            int count = itemCounts[item.ID];
            slotItemIds[slotIndex] = item.ID;
            slotItemCounts[slotIndex] = count;
            CreateItemDisplay(slotIndex, item, count);
            slotIndex++;
        }

        // Update full-inventory status banner
        if (statusText != null && legacyController != null)
        {
            statusText.text = legacyController.IsInventoryFull()
                ? "INVENTORY FULL - Only 4 unique items allowed"
                : "";
        }

        // Cache current state so LateUpdate can skip redundant rebuilds
        _lastSyncedCounts.Clear();
        foreach (var kv in itemCounts)
            _lastSyncedCounts[kv.Key] = kv.Value;
    }

    private void CreateItemDisplay(int slotIndex, Item item, int count = 1)
    {
        if (slotIndex < 0 || slotIndex >= slots.Count) return;

        RectTransform slotRect = slots[slotIndex];

        // Container for both icon and label
        GameObject containerGo = new GameObject($"Item_{item.ID}");
        containerGo.transform.SetParent(slotRect, false);
        RectTransform containerRect = containerGo.AddComponent<RectTransform>();
        containerRect.anchorMin = Vector2.zero;
        containerRect.anchorMax = Vector2.one;
        containerRect.offsetMin = Vector2.zero;
        containerRect.offsetMax = Vector2.zero;

        // Item icon area (top portion of slot, with padding)
        GameObject iconGo = new GameObject("Icon");
        iconGo.transform.SetParent(containerGo.transform, false);
        Image itemIcon = iconGo.AddComponent<Image>();
        itemIcon.raycastTarget = false;
        itemIcon.preserveAspect = true;
        RectTransform iconRect = iconGo.GetComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0f, 0.25f);
        iconRect.anchorMax = new Vector2(1f, 1f);
        iconRect.offsetMin = new Vector2(6f, 0f);
        iconRect.offsetMax = new Vector2(-6f, -6f);

        Sprite itemSprite = GetItemSprite(item);
        if (itemSprite != null)
        {
            itemIcon.sprite = itemSprite;
            itemIcon.color = Color.white;
        }
        else
        {
            itemIcon.color = GetItemColor(item.ID);
        }

        // Quantity badge (top-right corner, shown if count > 1)
        if (count > 1)
        {
            // Dark drop-shadow backing for readability against any background
            GameObject shadowGo = new GameObject("QuantityBadgeShadow");
            shadowGo.transform.SetParent(containerGo.transform, false);
            Image shadowBg = shadowGo.AddComponent<Image>();
            shadowBg.color = new Color(0f, 0f, 0f, 0.55f);
            shadowBg.raycastTarget = false;
            RectTransform shadowRect = shadowGo.GetComponent<RectTransform>();
            shadowRect.anchorMin = new Vector2(1f, 1f);
            shadowRect.anchorMax = new Vector2(1f, 1f);
            shadowRect.pivot = new Vector2(1f, 1f);
            shadowRect.sizeDelta = new Vector2(28f, 20f);
            shadowRect.anchoredPosition = new Vector2(-1f, -1f);

            // Main badge
            GameObject badgeGo = new GameObject("QuantityBadge");
            badgeGo.transform.SetParent(containerGo.transform, false);
            Image badgeBg = badgeGo.AddComponent<Image>();
            badgeBg.color = new Color(0.9f, 0.3f, 0.3f, 0.95f);
            badgeBg.raycastTarget = false;
            RectTransform badgeRect = badgeGo.GetComponent<RectTransform>();
            badgeRect.anchorMin = new Vector2(1f, 1f);
            badgeRect.anchorMax = new Vector2(1f, 1f);
            badgeRect.pivot = new Vector2(1f, 1f);
            badgeRect.sizeDelta = new Vector2(26f, 18f);
            badgeRect.anchoredPosition = new Vector2(-3f, -3f);

            GameObject badgeTextGo = new GameObject("QuantityText");
            badgeTextGo.transform.SetParent(badgeGo.transform, false);
            Text badgeText = badgeTextGo.AddComponent<Text>();
            badgeText.text = "x" + count;
            badgeText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            badgeText.fontSize = (int)PhaseCUITheme.FontSizeBadge;
            badgeText.fontStyle = FontStyle.Bold;
            badgeText.color = Color.white;
            badgeText.alignment = TextAnchor.MiddleCenter;
            badgeText.raycastTarget = false;
            RectTransform badgeTextRect = badgeTextGo.GetComponent<RectTransform>();
            badgeTextRect.anchorMin = Vector2.zero;
            badgeTextRect.anchorMax = Vector2.one;
            badgeTextRect.offsetMin = Vector2.zero;
            badgeTextRect.offsetMax = Vector2.zero;
        }

        // Item name label (bottom portion of slot)
        GameObject labelGo = new GameObject("Label");
        labelGo.transform.SetParent(containerGo.transform, false);
        Text labelText = labelGo.AddComponent<Text>();
        labelText.text = GetShortItemName(item);
        labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        labelText.fontSize = (int)PhaseCUITheme.FontSizeBadge;
        labelText.color = PhaseCUITheme.TextPrimary;
        labelText.alignment = TextAnchor.MiddleCenter;
        labelText.raycastTarget = false;
        labelText.horizontalOverflow = HorizontalWrapMode.Overflow;
        RectTransform labelRect = labelGo.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0f, 0f);
        labelRect.anchorMax = new Vector2(1f, 0.25f);
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        slotItemObjects[slotIndex] = containerGo;

        // Highlight occupied slot
        if (slotIndex < slotImages.Count)
        {
            slotImages[slotIndex].color = new Color(0.18f, 0.22f, 0.32f, 0.95f);
        }
    }

    private Sprite GetItemSprite(Item item)
    {
        if (item == null) return null;

        Image img = item.GetComponent<Image>();
        if (img != null && img.sprite != null) return img.sprite;

        SpriteRenderer sr = item.GetComponent<SpriteRenderer>();
        if (sr != null && sr.sprite != null) return sr.sprite;

        if (itemDictionary != null)
        {
            // Try to get sprite from ItemDictionary's prefab
            GameObject prefab = itemDictionary.GetItemPrefab(item.ID);
            if (prefab != null)
            {
                Image prefabImg = prefab.GetComponent<Image>();
                if (prefabImg != null && prefabImg.sprite != null) return prefabImg.sprite;

                SpriteRenderer prefabSr = prefab.GetComponent<SpriteRenderer>();
                if (prefabSr != null && prefabSr.sprite != null) return prefabSr.sprite;
            }

            // Fallback: Load sprite directly from Resources using naming convention
            string[] possibleNames = new string[]
            {
                $"MinigameC/Items/item_{item.displayName.ToLower().Replace(" ", "_")}_0",
                $"MinigameC/Items/item_{item.displayName.ToLower().Replace(" ", "_")}"
            };
            
            foreach (string spritePath in possibleNames)
            {
                Sprite loadedSprite = Resources.Load<Sprite>(spritePath);
                if (loadedSprite != null) return loadedSprite;
            }
        }

        return null;
    }

    private string GetShortItemName(Item item)
    {
        if (!string.IsNullOrEmpty(item.displayName))
        {
            string name = item.displayName;
            if (name.Length > 10)
            {
                if (name.Contains(" "))
                {
                    string[] parts = name.Split(' ');
                    if (parts.Length >= 2)
                        return parts[0].Substring(0, Mathf.Min(4, parts[0].Length)) + ".";
                }
                return name.Substring(0, 8) + "..";
            }
            return name;
        }
        return $"#{item.ID}";
    }

    private Color GetItemColor(int itemId)
    {
        switch (itemId)
        {
            case 1: return new Color(0.5f, 0.6f, 0.8f, 1f);
            case 2: return new Color(0.8f, 0.6f, 0.3f, 1f);
            case 3: return new Color(0.3f, 0.5f, 0.9f, 1f);
            case 4: return new Color(0.6f, 0.5f, 0.7f, 1f);
            case 5: return new Color(0.4f, 0.8f, 0.6f, 1f);
            case 6: return new Color(0.3f, 0.7f, 0.8f, 1f);
            case 7: return new Color(0.5f, 0.9f, 0.5f, 1f);
            case 8: return new Color(0.9f, 0.7f, 0.3f, 1f);
            case 9: return new Color(0.85f, 0.55f, 0.25f, 1f);
            case 10: return new Color(0.95f, 0.8f, 0.4f, 1f);
            case 11: return new Color(0.7f, 0.5f, 0.9f, 1f);
            case 12: return new Color(0.9f, 0.6f, 0.7f, 1f);
            case 13: return new Color(0.8f, 0.4f, 0.9f, 1f);
            case 14: return new Color(0.6f, 0.7f, 0.95f, 1f);
            case 15: return new Color(0.95f, 0.5f, 0.5f, 1f);
            default: return PhaseCUITheme.AccentCyan;
        }
    }
}
