using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Clean, single-panel inventory UI for Phase C. Replaces the tab-based system.
/// Auto-creates on scene load; hides legacy tabs if present.
/// </summary>
public class PhaseCInventoryUI : MonoBehaviour
{
    private const string TargetSceneName = "MinigameC";
    private const string CanvasName = "PhaseCInventoryCanvas";
    private const int SlotCount = 12;
    private const float SlotSize = 80f;
    private const float SlotSpacing = 12f;
    private const float SlotCornerRadius = 8f;
    private const int SlotsPerRow = 6;

    private Canvas canvas;
    private RectTransform panelRect;
    private RectTransform slotsContainer;
    private List<RectTransform> slots = new List<RectTransform>();
    private List<Image> slotImages = new List<Image>();
    private InventoryController legacyController;
    private ItemDictionary itemDictionary;
    private bool initialized;

    // Track items in our slots (by item ID, 0 = empty)
    private int[] slotItemIds = new int[SlotCount];
    private GameObject[] slotItemObjects = new GameObject[SlotCount];

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureInventoryUI()
    {
        if (SceneManager.GetActiveScene().name != TargetSceneName) return;
        if (FindFirstObjectByType<PhaseCInventoryUI>() != null) return;

        GameObject go = new GameObject("PhaseCInventoryUI");
        go.AddComponent<PhaseCInventoryUI>();
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
        HideLegacyTabUI();
        itemDictionary = FindFirstObjectByType<ItemDictionary>();
        legacyController = FindFirstObjectByType<InventoryController>();
        CreateInventoryCanvas();
        initialized = true;
    }

    private void HideLegacyTabUI()
    {
        // Find and hide legacy tab UI elements by name
        string[] elementsToHide = { "Tabs", "PlayerTab", "InventoryTab" };

        foreach (string elementName in elementsToHide)
        {
            GameObject element = GameObject.Find(elementName);
            if (element != null)
            {
                element.SetActive(false);
            }
        }

        // Also find TabController and disable its pages
        TabController[] tabControllers = FindObjectsByType<TabController>(FindObjectsSortMode.None);
        foreach (TabController tabCtrl in tabControllers)
        {
            if (tabCtrl == null) continue;

            // Hide all tab pages
            if (tabCtrl.pages != null)
            {
                foreach (GameObject page in tabCtrl.pages)
                {
                    if (page != null)
                    {
                        page.SetActive(false);
                    }
                }
            }

            // Hide tab images/buttons
            if (tabCtrl.tabImages != null)
            {
                foreach (Image tabImg in tabCtrl.tabImages)
                {
                    if (tabImg != null)
                    {
                        tabImg.gameObject.SetActive(false);
                    }
                }
            }

            // Hide the tabs container itself
            tabCtrl.gameObject.SetActive(false);
        }

        // Keep legacy InventoryPanel hidden but functional (for InventoryController to work)
        // The InventoryController.inventoryPanel reference should still work even if hidden
    }

    private void CreateInventoryCanvas()
    {
        // Destroy existing if any
        GameObject existing = GameObject.Find(CanvasName);
        if (existing != null) Destroy(existing);

        // Create canvas
        GameObject canvasGo = new GameObject(CanvasName);
        canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 5;

        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(PhaseCUITheme.RefWidth, PhaseCUITheme.RefHeight);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGo.AddComponent<GraphicRaycaster>();

        // Create main panel (bottom of screen, horizontal bar)
        GameObject panelGo = new GameObject("InventoryBar");
        panelGo.transform.SetParent(canvasGo.transform, false);

        Image panelBg = panelGo.AddComponent<Image>();
        panelBg.color = PhaseCUITheme.PanelBg;
        panelBg.raycastTarget = true;

        panelRect = panelGo.GetComponent<RectTransform>();
        float panelWidth = (SlotSize * SlotsPerRow) + (SlotSpacing * (SlotsPerRow + 1)) + 40f;
        float panelHeight = (SlotSize * 2) + (SlotSpacing * 3) + 60f;
        panelRect.anchorMin = new Vector2(0.5f, 0f);
        panelRect.anchorMax = new Vector2(0.5f, 0f);
        panelRect.pivot = new Vector2(0.5f, 0f);
        panelRect.sizeDelta = new Vector2(panelWidth, panelHeight);
        panelRect.anchoredPosition = new Vector2(0f, 20f);

        // Add border outline
        GameObject borderGo = new GameObject("Border");
        borderGo.transform.SetParent(panelGo.transform, false);
        Image borderImg = borderGo.AddComponent<Image>();
        borderImg.color = PhaseCUITheme.PanelBorder;
        borderImg.raycastTarget = false;
        RectTransform borderRect = borderGo.GetComponent<RectTransform>();
        borderRect.anchorMin = Vector2.zero;
        borderRect.anchorMax = Vector2.one;
        borderRect.offsetMin = new Vector2(-2f, -2f);
        borderRect.offsetMax = new Vector2(2f, 2f);
        borderGo.transform.SetAsFirstSibling();

        // Title label
        GameObject titleGo = new GameObject("Title");
        titleGo.transform.SetParent(panelGo.transform, false);
        Text titleText = titleGo.AddComponent<Text>();
        titleText.text = "INVENTORY";
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.fontSize = 18;
        titleText.fontStyle = FontStyle.Bold;
        titleText.color = PhaseCUITheme.AccentGold;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.raycastTarget = false;
        RectTransform titleRect = titleGo.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.sizeDelta = new Vector2(0f, 30f);
        titleRect.anchoredPosition = new Vector2(0f, -8f);

        // Create slots container
        GameObject slotsGo = new GameObject("Slots");
        slotsGo.transform.SetParent(panelGo.transform, false);
        slotsContainer = slotsGo.AddComponent<RectTransform>();
        slotsContainer.anchorMin = new Vector2(0.5f, 0f);
        slotsContainer.anchorMax = new Vector2(0.5f, 0f);
        slotsContainer.pivot = new Vector2(0.5f, 0f);
        float containerWidth = (SlotSize * SlotsPerRow) + (SlotSpacing * (SlotsPerRow - 1));
        float containerHeight = (SlotSize * 2) + SlotSpacing;
        slotsContainer.sizeDelta = new Vector2(containerWidth, containerHeight);
        slotsContainer.anchoredPosition = new Vector2(0f, SlotSpacing + 10f);

        // Create slot grid
        for (int i = 0; i < SlotCount; i++)
        {
            CreateSlot(i);
        }
    }

    private void CreateSlot(int index)
    {
        GameObject slotGo = new GameObject($"Slot_{index}");
        slotGo.transform.SetParent(slotsContainer, false);

        // Slot background
        Image slotBg = slotGo.AddComponent<Image>();
        slotBg.color = new Color(0.12f, 0.14f, 0.22f, 0.9f);
        slotBg.raycastTarget = true;

        RectTransform slotRect = slotGo.GetComponent<RectTransform>();
        int row = index / SlotsPerRow;
        int col = index % SlotsPerRow;
        float x = col * (SlotSize + SlotSpacing) - (slotsContainer.sizeDelta.x / 2f) + (SlotSize / 2f);
        float y = (1 - row) * (SlotSize + SlotSpacing) + (SlotSize / 2f);
        slotRect.anchorMin = new Vector2(0.5f, 0f);
        slotRect.anchorMax = new Vector2(0.5f, 0f);
        slotRect.pivot = new Vector2(0.5f, 0.5f);
        slotRect.sizeDelta = new Vector2(SlotSize, SlotSize);
        slotRect.anchoredPosition = new Vector2(x, y);

        // Inner border highlight
        GameObject innerBorderGo = new GameObject("InnerBorder");
        innerBorderGo.transform.SetParent(slotGo.transform, false);
        Image innerBorder = innerBorderGo.AddComponent<Image>();
        innerBorder.color = new Color(0.3f, 0.4f, 0.5f, 0.4f);
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

        // Sync with legacy inventory controller if it has items
        SyncFromLegacyInventory();
    }

    private void SyncFromLegacyInventory()
    {
        if (legacyController == null || legacyController.inventoryPanel == null) return;

        // Clear our display first
        for (int i = 0; i < SlotCount; i++)
        {
            if (slotItemObjects[i] != null)
            {
                Destroy(slotItemObjects[i]);
                slotItemObjects[i] = null;
            }
            slotItemIds[i] = 0;
        }

        // Read from legacy inventory
        int slotIndex = 0;
        foreach (Transform slotTransform in legacyController.inventoryPanel.transform)
        {
            if (slotIndex >= SlotCount) break;

            Slot legacySlot = slotTransform.GetComponent<Slot>();
            if (legacySlot != null && legacySlot.currentItem != null)
            {
                Item item = legacySlot.currentItem.GetComponent<Item>();
                if (item != null)
                {
                    slotItemIds[slotIndex] = item.ID;
                    CreateItemDisplay(slotIndex, item);
                }
            }
            slotIndex++;
        }
    }

    private void CreateItemDisplay(int slotIndex, Item item)
    {
        if (slotIndex < 0 || slotIndex >= slots.Count) return;

        RectTransform slotRect = slots[slotIndex];

        // Create item container
        GameObject itemGo = new GameObject($"Item_{item.ID}");
        itemGo.transform.SetParent(slotRect, false);

        RectTransform itemRect = itemGo.AddComponent<RectTransform>();
        itemRect.anchorMin = Vector2.zero;
        itemRect.anchorMax = Vector2.one;
        itemRect.offsetMin = new Vector2(6f, 18f);
        itemRect.offsetMax = new Vector2(-6f, -6f);

        // Item icon (colored square with glow effect)
        Image itemIcon = itemGo.AddComponent<Image>();
        itemIcon.color = GetItemColor(item.ID);
        itemIcon.raycastTarget = false;

        // Item name label
        GameObject labelGo = new GameObject("Label");
        labelGo.transform.SetParent(slotRect, false);
        Text labelText = labelGo.AddComponent<Text>();
        labelText.text = GetShortItemName(item);
        labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        labelText.fontSize = 11;
        labelText.color = PhaseCUITheme.TextPrimary;
        labelText.alignment = TextAnchor.MiddleCenter;
        labelText.raycastTarget = false;
        labelText.horizontalOverflow = HorizontalWrapMode.Overflow;
        RectTransform labelRect = labelGo.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0f, 0f);
        labelRect.anchorMax = new Vector2(1f, 0f);
        labelRect.pivot = new Vector2(0.5f, 0f);
        labelRect.sizeDelta = new Vector2(0f, 16f);
        labelRect.anchoredPosition = new Vector2(0f, 2f);

        // Store reference
        slotItemObjects[slotIndex] = itemGo;

        // Highlight the slot
        if (slotIndex < slotImages.Count)
        {
            slotImages[slotIndex].color = new Color(0.18f, 0.22f, 0.32f, 0.95f);
        }
    }

    private string GetShortItemName(Item item)
    {
        if (!string.IsNullOrEmpty(item.displayName))
        {
            // Shorten long names
            string name = item.displayName;
            if (name.Length > 10)
            {
                // Try to abbreviate
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
        // Assign distinct colors based on item category
        switch (itemId)
        {
            // Basic materials (1-4): Blue tones
            case 1: return new Color(0.5f, 0.6f, 0.8f, 1f);   // Metal Alloy
            case 2: return new Color(0.8f, 0.6f, 0.3f, 1f);   // Wiring
            case 3: return new Color(0.3f, 0.5f, 0.9f, 1f);   // Solar Cells
            case 4: return new Color(0.6f, 0.5f, 0.7f, 1f);   // Insulation

            // Instruments (5-7): Green/Cyan tones
            case 5: return new Color(0.4f, 0.8f, 0.6f, 1f);   // Magnetometer
            case 6: return new Color(0.3f, 0.7f, 0.8f, 1f);   // Camera Sensor
            case 7: return new Color(0.5f, 0.9f, 0.5f, 1f);   // Spectrometer

            // Comm systems (8-10): Gold/Orange tones
            case 8: return new Color(0.9f, 0.7f, 0.3f, 1f);   // Circuit Board
            case 9: return new Color(0.85f, 0.55f, 0.25f, 1f); // Battery
            case 10: return new Color(0.95f, 0.8f, 0.4f, 1f);  // Thermal Blanket

            // Higher items: Purple/Pink tones
            case 11: return new Color(0.7f, 0.5f, 0.9f, 1f);   // Battery
            case 12: return new Color(0.9f, 0.6f, 0.7f, 1f);   // Radio Antenna
            case 13: return new Color(0.8f, 0.4f, 0.9f, 1f);   // Laser Module
            case 14: return new Color(0.6f, 0.7f, 0.95f, 1f);  // Navigation
            case 15: return new Color(0.95f, 0.5f, 0.5f, 1f);  // Propellant

            default: return PhaseCUITheme.AccentCyan;
        }
    }
}
