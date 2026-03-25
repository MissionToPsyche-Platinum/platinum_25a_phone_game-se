using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Always-visible panel showing items required for the current step/sub-delivery.
/// Displays item icon, name, and collected status. Auto-updates when step changes.
/// Has a minimize toggle in the title bar: one click collapses to title only.
/// </summary>
public class PhaseCRequiredItemsUI : MonoBehaviour
{
    private const string TargetSceneName = "MinigameC";
    private const string CanvasName = "PhaseCRequiredItemsCanvas";
    private const float TitleBarHeight = 40f;

    private GameObject canvasObject;
    private GameObject panelObject;
    private GameObject contentContainer;
    private GameObject bodyRoot;
    private Text titleText;
    private Text npcText;
    private Text toggleLabel;
    private RectTransform panelRect;
    private ItemDictionary itemDictionary;
    private InventoryController inventoryController;
    private PhaseCAssemblyController controller;
    private bool initialized;
    private bool isPanelMinimized = false;
    private float lastExpandedHeight = 200f;

    private List<int> lastRequiredIds = new List<int>();
    private List<GameObject> itemRows = new List<GameObject>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void EnsureUI()
    {
        SceneManager.sceneLoaded += (scene, _) =>
        {
            if (scene.name != TargetSceneName) return;
            if (FindFirstObjectByType<PhaseCRequiredItemsUI>() != null) return;
            new GameObject("PhaseCRequiredItemsUI").AddComponent<PhaseCRequiredItemsUI>();
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

        RefreshDisplay();
    }

    // --- Minimize / expand ---

    private void ToggleMinimize()
    {
        isPanelMinimized = !isPanelMinimized;

        if (bodyRoot != null)
            bodyRoot.SetActive(!isPanelMinimized);

        if (panelRect != null)
        {
            float height = isPanelMinimized ? TitleBarHeight : lastExpandedHeight;
            panelRect.sizeDelta = new Vector2(PhaseCUITheme.GetRequiredPanelWidthExpanded(), height);
        }

        if (toggleLabel != null)
            toggleLabel.text = isPanelMinimized ? "+" : "-";
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
        scaler.matchWidthOrHeight = PhaseCUITheme.CanvasMatchWidthOrHeight;

        canvasGo.AddComponent<GraphicRaycaster>();

        // Panel anchored to the right-center of screen
        panelObject = new GameObject("RequiredItemsPanel");
        panelObject.transform.SetParent(canvasGo.transform, false);

        Image panelBg = panelObject.AddComponent<Image>();
        panelBg.color = new Color(0.07f, 0.09f, 0.16f, 0.88f);
        panelBg.raycastTarget = false;

        panelRect = panelObject.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(1f, 0.5f);
        panelRect.anchorMax = new Vector2(1f, 0.5f);
        panelRect.pivot = new Vector2(1f, 0.5f);
        float panelWidth = PhaseCUITheme.GetRequiredPanelWidthExpanded();
        panelRect.sizeDelta = new Vector2(panelWidth, 200f);
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

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        int itemFontSize = PhaseCUITheme.GetRequiredItemFontSize();
        int titleFontSize = itemFontSize + 3;

        // Title bar: title text + toggle button side by side
        GameObject titleRowGo = new GameObject("TitleRow");
        titleRowGo.transform.SetParent(panelObject.transform, false);
        RectTransform titleRowRect = titleRowGo.AddComponent<RectTransform>();
        titleRowRect.anchorMin = new Vector2(0f, 1f);
        titleRowRect.anchorMax = new Vector2(1f, 1f);
        titleRowRect.pivot = new Vector2(0.5f, 1f);
        titleRowRect.sizeDelta = new Vector2(0f, TitleBarHeight);
        titleRowRect.anchoredPosition = new Vector2(0f, -2f);

        // Title text (left side of title row)
        GameObject titleGo = new GameObject("Title");
        titleGo.transform.SetParent(titleRowGo.transform, false);
        titleText = titleGo.AddComponent<Text>();
        titleText.text = "REQUIRED ITEMS";
        titleText.font = font;
        titleText.fontSize = titleFontSize;
        titleText.fontStyle = FontStyle.Bold;
        titleText.color = PhaseCUITheme.AccentGold;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.raycastTarget = false;
        RectTransform titleRect = titleGo.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 0f);
        titleRect.anchorMax = new Vector2(0.8f, 1f);
        titleRect.offsetMin = new Vector2(6f, 0f);
        titleRect.offsetMax = new Vector2(0f, 0f);

        // Toggle button (right side of title row)
        AddPanelToggleButton(titleRowGo.transform, font, titleFontSize);

        // Thin divider under title
        GameObject divGo = new GameObject("TitleDiv");
        divGo.transform.SetParent(panelObject.transform, false);
        Image divImg = divGo.AddComponent<Image>();
        divImg.color = new Color(PhaseCUITheme.PanelBorder.r, PhaseCUITheme.PanelBorder.g, PhaseCUITheme.PanelBorder.b, 0.4f);
        divImg.raycastTarget = false;
        RectTransform divRect = divGo.GetComponent<RectTransform>();
        divRect.anchorMin = new Vector2(0f, 1f);
        divRect.anchorMax = new Vector2(1f, 1f);
        divRect.pivot = new Vector2(0.5f, 1f);
        divRect.sizeDelta = new Vector2(-8f, 1f);
        divRect.anchoredPosition = new Vector2(0f, -(TitleBarHeight + 2f));

        // Body root: NPC hint + item rows (hidden when minimized)
        bodyRoot = new GameObject("Body");
        bodyRoot.transform.SetParent(panelObject.transform, false);
        RectTransform bodyRootRect = bodyRoot.AddComponent<RectTransform>();
        bodyRootRect.anchorMin = new Vector2(0f, 0f);
        bodyRootRect.anchorMax = new Vector2(1f, 1f);
        bodyRootRect.offsetMin = Vector2.zero;
        bodyRootRect.offsetMax = Vector2.zero;

        // NPC text
        GameObject npcGo = new GameObject("NpcText");
        npcGo.transform.SetParent(bodyRoot.transform, false);
        npcText = npcGo.AddComponent<Text>();
        npcText.text = "";
        npcText.font = font;
        npcText.fontSize = itemFontSize - 1;
        npcText.color = PhaseCUITheme.TextSecondary;
        npcText.alignment = TextAnchor.MiddleCenter;
        npcText.raycastTarget = false;
        RectTransform npcRect = npcGo.GetComponent<RectTransform>();
        npcRect.anchorMin = new Vector2(0f, 1f);
        npcRect.anchorMax = new Vector2(1f, 1f);
        npcRect.pivot = new Vector2(0.5f, 1f);
        npcRect.sizeDelta = new Vector2(0f, 22f);
        npcRect.anchoredPosition = new Vector2(0f, -(TitleBarHeight + 6f));

        // Content container for item rows
        GameObject contentGo = new GameObject("Content");
        contentGo.transform.SetParent(bodyRoot.transform, false);
        contentContainer = contentGo;
        RectTransform contentRect = contentGo.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.sizeDelta = new Vector2(0f, 0f);
        contentRect.anchoredPosition = new Vector2(0f, -(TitleBarHeight + 30f));
    }

    private void AddPanelToggleButton(Transform parent, Font font, int fontSize)
    {
        GameObject btnGo = new GameObject("ToggleBtn");
        btnGo.transform.SetParent(parent, false);

        Image btnBg = btnGo.AddComponent<Image>();
        btnBg.color = new Color(0.2f, 0.28f, 0.42f, 0.9f);

        Button btn = btnGo.AddComponent<Button>();
        btn.targetGraphic = btnBg;
        ColorBlock cb = btn.colors;
        cb.highlightedColor = new Color(0.35f, 0.5f, 0.7f, 1f);
        cb.pressedColor = new Color(0.5f, 0.65f, 0.85f, 1f);
        btn.colors = cb;
        btn.onClick.AddListener(ToggleMinimize);

        RectTransform btnRect = btnGo.GetComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.8f, 0.1f);
        btnRect.anchorMax = new Vector2(1f, 0.9f);
        btnRect.offsetMin = new Vector2(2f, 0f);
        btnRect.offsetMax = new Vector2(-4f, 0f);

        GameObject labelGo = new GameObject("Label");
        labelGo.transform.SetParent(btnGo.transform, false);
        toggleLabel = labelGo.AddComponent<Text>();
        toggleLabel.text = "-";
        toggleLabel.font = font;
        toggleLabel.fontSize = fontSize;
        toggleLabel.fontStyle = FontStyle.Bold;
        toggleLabel.color = PhaseCUITheme.AccentCyan;
        toggleLabel.alignment = TextAnchor.MiddleCenter;
        toggleLabel.raycastTarget = false;
        RectTransform labelRect = labelGo.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;
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

        if (requiredIds == null || requiredIds.Count == 0)
        {
            panelObject.SetActive(false);
            return;
        }

        panelObject.SetActive(true);

        if (npcText != null && !string.IsNullOrEmpty(stepInfo.CompletionNpc))
            npcText.text = "Bring to: " + stepInfo.CompletionNpc;

        Dictionary<int, int> inventoryCounts = GetInventoryCounts();

        Dictionary<int, int> requiredCounts = new Dictionary<int, int>();
        foreach (int id in requiredIds)
        {
            requiredCounts.TryGetValue(id, out int c);
            requiredCounts[id] = c + 1;
        }

        foreach (GameObject row in itemRows)
        {
            if (row != null) Destroy(row);
        }
        itemRows.Clear();

        float rowHeight = 40f;
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

        // Resize panel to fit content (only apply when expanded)
        float newHeight = TitleBarHeight + 34f + (rowIndex * rowHeight) + 12f;
        lastExpandedHeight = newHeight;

        if (!isPanelMinimized && panelRect != null)
            panelRect.sizeDelta = new Vector2(PhaseCUITheme.GetRequiredPanelWidthExpanded(), newHeight);
    }

    private GameObject CreateItemRow(int itemId, int needed, int have, bool collected, float yOffset)
    {
        int itemFontSize = PhaseCUITheme.GetRequiredItemFontSize();

        GameObject rowGo = new GameObject($"ItemRow_{itemId}");
        rowGo.transform.SetParent(contentContainer.transform, false);

        RectTransform rowRect = rowGo.AddComponent<RectTransform>();
        rowRect.anchorMin = new Vector2(0f, 1f);
        rowRect.anchorMax = new Vector2(1f, 1f);
        rowRect.pivot = new Vector2(0.5f, 1f);
        rowRect.sizeDelta = new Vector2(0f, 38f);
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
        iconBgRect.sizeDelta = new Vector2(34f, 34f);
        iconBgRect.anchoredPosition = new Vector2(10f, 0f);

        // Item icon
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

        // Item name + count + status
        string itemName = itemDictionary.GetDisplayName(itemId);
        string countStr = needed > 1 ? $" x{needed}" : "";
        string statusStr = collected ? "  [OK]" : $"  ({have}/{needed})";

        GameObject nameGo = new GameObject("Name");
        nameGo.transform.SetParent(rowGo.transform, false);
        Text nameText = nameGo.AddComponent<Text>();
        nameText.text = itemName + countStr + statusStr;
        nameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        nameText.fontSize = itemFontSize;
        nameText.color = collected
            ? new Color(0.5f, 0.8f, 0.55f, 0.8f)
            : PhaseCUITheme.TextPrimary;
        nameText.fontStyle = collected ? FontStyle.Italic : FontStyle.Normal;
        nameText.alignment = TextAnchor.MiddleLeft;
        nameText.raycastTarget = false;
        nameText.horizontalOverflow = PhaseCUITheme.IsMobileScreen
            ? HorizontalWrapMode.Wrap
            : HorizontalWrapMode.Overflow;
        RectTransform nameRect = nameGo.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0f, 0f);
        nameRect.anchorMax = new Vector2(1f, 1f);
        nameRect.offsetMin = new Vector2(50f, 0f);
        nameRect.offsetMax = new Vector2(-8f, 0f);

        // Checkmark for collected items
        if (collected)
        {
            GameObject checkGo = new GameObject("Check");
            checkGo.transform.SetParent(iconBgGo.transform, false);
            Text checkText = checkGo.AddComponent<Text>();
            checkText.text = "v";
            checkText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            checkText.fontSize = itemFontSize + 2;
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
        if (prefab != null)
        {
            Image img = prefab.GetComponent<Image>();
            if (img != null && img.sprite != null) return img.sprite;

            SpriteRenderer sr = prefab.GetComponent<SpriteRenderer>();
            if (sr != null && sr.sprite != null) return sr.sprite;
        }

        string itemName = itemDictionary.GetDisplayName(itemId).ToLower().Replace(" ", "_");
        string[] possibleNames = new string[]
        {
            $"MinigameC/Items/item_{itemName}_0",
            $"MinigameC/Items/item_{itemName}"
        };

        foreach (string spritePath in possibleNames)
        {
            Sprite loadedSprite = Resources.Load<Sprite>(spritePath);
            if (loadedSprite != null) return loadedSprite;
        }

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
