using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Always-visible panel showing items required for the current step/sub-delivery.
/// Displays item icon, name, and collected status. Auto-updates when step changes.
/// </summary>
public class PhaseCRequiredItemsUI : MonoBehaviour
{
    private const string TargetSceneName = "MinigameC";
    private const string CanvasName = "PhaseCRequiredItemsCanvas";

    private GameObject canvasObject;
    private GameObject panelObject;
    private GameObject contentContainer;
    private Text titleText;
    private Text npcText;
    private ItemDictionary itemDictionary;
    private InventoryController inventoryController;
    private PhaseCAssemblyController controller;
    private bool initialized;

    private List<int> lastRequiredIds = new List<int>();
    private List<GameObject> itemRows = new List<GameObject>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureUI()
    {
        if (SceneManager.GetActiveScene().name != TargetSceneName) return;
        if (FindFirstObjectByType<PhaseCRequiredItemsUI>() != null) return;

        GameObject go = new GameObject("PhaseCRequiredItemsUI");
        go.AddComponent<PhaseCRequiredItemsUI>();
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
        itemDictionary = FindFirstObjectByType<ItemDictionary>();
        inventoryController = FindFirstObjectByType<InventoryController>();
        controller = PhaseCAssemblyController.Instance;
        CreateCanvas();
        initialized = true;
        RefreshDisplay();
    }

    private void Update()
    {
        if (!initialized) return;

        if (controller == null)
            controller = PhaseCAssemblyController.Instance;

        // Refresh every frame to keep collected status up to date
        RefreshDisplay();
    }

    private void CreateCanvas()
    {
        GameObject existing = GameObject.Find(CanvasName);
        if (existing != null) Destroy(existing);

        GameObject canvasGo = new GameObject(CanvasName);
        canvasObject = canvasGo;
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 8;

        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(PhaseCUITheme.RefWidth, PhaseCUITheme.RefHeight);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGo.AddComponent<GraphicRaycaster>();

        // Panel on the right side of the screen
        panelObject = new GameObject("RequiredItemsPanel");
        panelObject.transform.SetParent(canvasGo.transform, false);

        Image panelBg = panelObject.AddComponent<Image>();
        panelBg.color = new Color(0.07f, 0.09f, 0.16f, 0.85f);
        panelBg.raycastTarget = false;

        RectTransform panelRect = panelObject.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(1f, 0.5f);
        panelRect.anchorMax = new Vector2(1f, 0.5f);
        panelRect.pivot = new Vector2(1f, 0.5f);
        panelRect.sizeDelta = new Vector2(260f, 200f); // Will resize dynamically
        panelRect.anchoredPosition = new Vector2(-16f, 0f);

        // Border
        GameObject borderGo = new GameObject("Border");
        borderGo.transform.SetParent(panelObject.transform, false);
        Image borderImg = borderGo.AddComponent<Image>();
        borderImg.color = PhaseCUITheme.PanelBorder;
        borderImg.raycastTarget = false;
        RectTransform borderRect = borderGo.GetComponent<RectTransform>();
        borderRect.anchorMin = Vector2.zero;
        borderRect.anchorMax = Vector2.one;
        borderRect.offsetMin = new Vector2(-1f, -1f);
        borderRect.offsetMax = new Vector2(1f, 1f);
        borderGo.transform.SetAsFirstSibling();

        // Title
        GameObject titleGo = new GameObject("Title");
        titleGo.transform.SetParent(panelObject.transform, false);
        titleText = titleGo.AddComponent<Text>();
        titleText.text = "REQUIRED ITEMS";
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.fontSize = 14;
        titleText.fontStyle = FontStyle.Bold;
        titleText.color = PhaseCUITheme.AccentGold;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.raycastTarget = false;
        RectTransform titleRect = titleGo.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.sizeDelta = new Vector2(0f, 26f);
        titleRect.anchoredPosition = new Vector2(0f, -6f);

        // NPC text
        GameObject npcGo = new GameObject("NpcText");
        npcGo.transform.SetParent(panelObject.transform, false);
        npcText = npcGo.AddComponent<Text>();
        npcText.text = "";
        npcText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        npcText.fontSize = 12;
        npcText.color = PhaseCUITheme.TextSecondary;
        npcText.alignment = TextAnchor.MiddleCenter;
        npcText.raycastTarget = false;
        RectTransform npcRect = npcGo.GetComponent<RectTransform>();
        npcRect.anchorMin = new Vector2(0f, 1f);
        npcRect.anchorMax = new Vector2(1f, 1f);
        npcRect.pivot = new Vector2(0.5f, 1f);
        npcRect.sizeDelta = new Vector2(0f, 18f);
        npcRect.anchoredPosition = new Vector2(0f, -30f);

        // Content container for item rows
        GameObject contentGo = new GameObject("Content");
        contentGo.transform.SetParent(panelObject.transform, false);
        contentContainer = contentGo;
        RectTransform contentRect = contentGo.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.sizeDelta = new Vector2(0f, 0f);
        contentRect.anchoredPosition = new Vector2(0f, -52f);
    }

    private void RefreshDisplay()
    {
        if (controller == null || itemDictionary == null || panelObject == null)
        {
            if (panelObject != null) panelObject.SetActive(false);
            return;
        }

        List<int> requiredIds = controller.GetCurrentStepRequiredItemIds();
        PhaseCAssemblyController.StepInfo stepInfo = controller.GetCurrentStepInfo();

        // Hide panel if no items to collect
        if (requiredIds == null || requiredIds.Count == 0)
        {
            panelObject.SetActive(false);
            return;
        }

        panelObject.SetActive(true);

        // Update NPC text
        if (npcText != null && !string.IsNullOrEmpty(stepInfo.CompletionNpc))
        {
            npcText.text = "Bring to: " + stepInfo.CompletionNpc;
        }

        // Count how many of each item the player has in inventory
        Dictionary<int, int> inventoryCounts = GetInventoryCounts();

        // Build required item count map (some items appear multiple times)
        Dictionary<int, int> requiredCounts = new Dictionary<int, int>();
        foreach (int id in requiredIds)
        {
            requiredCounts.TryGetValue(id, out int c);
            requiredCounts[id] = c + 1;
        }

        // Clear old rows
        foreach (GameObject row in itemRows)
        {
            if (row != null) Destroy(row);
        }
        itemRows.Clear();

        // Create item rows
        float rowHeight = 36f;
        float yOffset = 0f;
        int rowIndex = 0;

        foreach (KeyValuePair<int, int> kv in requiredCounts)
        {
            int itemId = kv.Key;
            int needed = kv.Value;
            inventoryCounts.TryGetValue(itemId, out int have);
            bool collected = have >= needed;

            GameObject row = CreateItemRow(itemId, needed, have, collected, yOffset);
            itemRows.Add(row);

            yOffset -= rowHeight;
            rowIndex++;
        }

        // Resize panel to fit content
        float panelHeight = 56f + (rowIndex * rowHeight) + 10f;
        RectTransform panelRect = panelObject.GetComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(260f, panelHeight);
    }

    private GameObject CreateItemRow(int itemId, int needed, int have, bool collected, float yOffset)
    {
        GameObject rowGo = new GameObject($"ItemRow_{itemId}");
        rowGo.transform.SetParent(contentContainer.transform, false);

        RectTransform rowRect = rowGo.AddComponent<RectTransform>();
        rowRect.anchorMin = new Vector2(0f, 1f);
        rowRect.anchorMax = new Vector2(1f, 1f);
        rowRect.pivot = new Vector2(0.5f, 1f);
        rowRect.sizeDelta = new Vector2(0f, 34f);
        rowRect.anchoredPosition = new Vector2(0f, yOffset);

        // Item icon background
        GameObject iconBgGo = new GameObject("IconBg");
        iconBgGo.transform.SetParent(rowGo.transform, false);
        Image iconBgImg = iconBgGo.AddComponent<Image>();
        iconBgImg.color = collected
            ? new Color(0.2f, 0.4f, 0.25f, 0.7f)
            : new Color(0.15f, 0.17f, 0.25f, 0.7f);
        iconBgImg.raycastTarget = false;
        RectTransform iconBgRect = iconBgGo.GetComponent<RectTransform>();
        iconBgRect.anchorMin = new Vector2(0f, 0.5f);
        iconBgRect.anchorMax = new Vector2(0f, 0.5f);
        iconBgRect.pivot = new Vector2(0f, 0.5f);
        iconBgRect.sizeDelta = new Vector2(30f, 30f);
        iconBgRect.anchoredPosition = new Vector2(10f, 0f);

        // Item icon (try to get sprite from prefab)
        GameObject iconGo = new GameObject("Icon");
        iconGo.transform.SetParent(iconBgGo.transform, false);
        Image iconImg = iconGo.AddComponent<Image>();
        iconImg.raycastTarget = false;
        iconImg.preserveAspect = true;

        Sprite sprite = GetItemSprite(itemId);
        if (sprite != null)
        {
            iconImg.sprite = sprite;
            iconImg.color = collected ? new Color(1f, 1f, 1f, 0.6f) : Color.white;
        }
        else
        {
            iconImg.color = collected
                ? new Color(0.4f, 0.7f, 0.5f, 0.6f)
                : PhaseCUITheme.AccentCyan;
        }

        RectTransform iconRect = iconGo.GetComponent<RectTransform>();
        iconRect.anchorMin = Vector2.zero;
        iconRect.anchorMax = Vector2.one;
        iconRect.offsetMin = new Vector2(3f, 3f);
        iconRect.offsetMax = new Vector2(-3f, -3f);

        // Item name + count
        string itemName = itemDictionary.GetDisplayName(itemId);
        string countStr = needed > 1 ? $" x{needed}" : "";
        string statusStr = collected ? "  [OK]" : $"  ({have}/{needed})";

        GameObject nameGo = new GameObject("Name");
        nameGo.transform.SetParent(rowGo.transform, false);
        Text nameText = nameGo.AddComponent<Text>();
        nameText.text = itemName + countStr + statusStr;
        nameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        nameText.fontSize = 13;
        nameText.color = collected
            ? new Color(0.5f, 0.8f, 0.55f, 0.8f)
            : PhaseCUITheme.TextPrimary;
        nameText.fontStyle = collected ? FontStyle.Italic : FontStyle.Normal;
        nameText.alignment = TextAnchor.MiddleLeft;
        nameText.raycastTarget = false;
        nameText.horizontalOverflow = HorizontalWrapMode.Overflow;
        RectTransform nameRect = nameGo.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0f, 0f);
        nameRect.anchorMax = new Vector2(1f, 1f);
        nameRect.offsetMin = new Vector2(46f, 0f);
        nameRect.offsetMax = new Vector2(-8f, 0f);

        // Checkmark for collected items
        if (collected)
        {
            GameObject checkGo = new GameObject("Check");
            checkGo.transform.SetParent(iconBgGo.transform, false);
            Text checkText = checkGo.AddComponent<Text>();
            checkText.text = "v";
            checkText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            checkText.fontSize = 16;
            checkText.fontStyle = FontStyle.Bold;
            checkText.color = new Color(0.3f, 0.9f, 0.4f, 1f);
            checkText.alignment = TextAnchor.MiddleCenter;
            checkText.raycastTarget = false;
            RectTransform checkRect = checkGo.GetComponent<RectTransform>();
            checkRect.anchorMin = Vector2.zero;
            checkRect.anchorMax = Vector2.one;
            checkRect.offsetMin = Vector2.zero;
            checkRect.offsetMax = Vector2.zero;
        }

        return rowGo;
    }

    private Sprite GetItemSprite(int itemId)
    {
        if (itemDictionary == null) return null;

        GameObject prefab = itemDictionary.GetItemPrefab(itemId);
        if (prefab == null) return null;

        Image img = prefab.GetComponent<Image>();
        if (img != null && img.sprite != null) return img.sprite;

        SpriteRenderer sr = prefab.GetComponent<SpriteRenderer>();
        if (sr != null && sr.sprite != null) return sr.sprite;

        return null;
    }

    private Dictionary<int, int> GetInventoryCounts()
    {
        var counts = new Dictionary<int, int>();
        if (inventoryController == null || inventoryController.inventoryPanel == null)
            return counts;

        foreach (Transform slotTransform in inventoryController.inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot == null || slot.currentItem == null) continue;
            Item item = slot.currentItem.GetComponent<Item>();
            if (item == null) continue;
            counts.TryGetValue(item.ID, out int c);
            counts[item.ID] = c + 1;
        }

        return counts;
    }
}
