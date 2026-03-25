using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Always-visible bottom HUD strip showing contextual hints:
/// inventory shortcut + count, contextual action hint, and current task step.
///
/// Action hint priority (middle section):
///   1. Has all required items -> "Ready! Go to: [NPC]"
///   2. Inventory full but missing items -> "Inventory full - [I] then 1-4 to drop"
///   3. No items required for step -> "Talk to: [NPC]"
///   4. Default -> "Walk into items to collect"
///
/// The strip has a minimize toggle on the right; one click collapses it to a thin tab.
/// </summary>
public class PhaseCPersistentHintUI : MonoBehaviour
{
    private const string TargetSceneName = "MinigameC";
    private const string CanvasName = "PhaseCPersistentHintCanvas";

    private InventoryController inventoryController;
    private PhaseCAssemblyController assemblyController;
    private bool initialized;

    private Text inventoryHintText;
    private Text actionHintText;
    private Text taskHintText;

    // Dirty-check state to avoid per-frame Text writes
    private int lastItemCount = -1;
    private string lastActionHint = null;
    private Color lastActionColor;
    private bool lastInventoryFull = false;

    private PhaseCAssemblyController.StepInfo currentStepInfo;

    // Minimize / expand state
    private RectTransform stripRect;
    private GameObject contentRoot;
    private Text toggleLabel;
    private bool isMinimized = false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void EnsureHintUI()
    {
        SceneManager.sceneLoaded += (scene, _) =>
        {
            if (scene.name != TargetSceneName) return;
            if (FindFirstObjectByType<PhaseCPersistentHintUI>() != null) return;
            new GameObject("PhaseCPersistentHintUI").AddComponent<PhaseCPersistentHintUI>();
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
        inventoryController = FindFirstObjectByType<InventoryController>();
        assemblyController = PhaseCAssemblyController.Instance;

        CreateHintCanvas();
        initialized = true;

        if (assemblyController != null)
            assemblyController.StepChanged += OnStepChanged;

        currentStepInfo = assemblyController != null
            ? assemblyController.GetCurrentStepInfo()
            : PhaseCAssemblyController.StepInfo.Empty;

        ForceRefresh();
    }

    private void OnDestroy()
    {
        if (assemblyController != null)
            assemblyController.StepChanged -= OnStepChanged;
    }

    private void OnStepChanged(PhaseCAssemblyController.StepInfo info)
    {
        currentStepInfo = info;
        UpdateTaskHint(info);
        lastActionHint = null;
    }

    private void LateUpdate()
    {
        if (!initialized) return;

        int count = GetInventoryCount();
        bool isFull = inventoryController != null && inventoryController.IsInventoryFull();

        if (count != lastItemCount || isFull != lastInventoryFull)
            UpdateInventoryHint(count, isFull);

        EvaluateActionHint(isFull);
    }

    // --- Inventory hint ---

    private int GetInventoryCount()
    {
        if (inventoryController == null || inventoryController.inventoryPanel == null) return 0;
        int count = 0;
        foreach (Transform t in inventoryController.inventoryPanel.transform)
        {
            Slot s = t.GetComponent<Slot>();
            if (s != null && s.currentItem != null) count++;
        }
        return count;
    }

    private void UpdateInventoryHint(int count, bool isFull)
    {
        lastItemCount = count;
        lastInventoryFull = isFull;
        if (inventoryHintText == null) return;

        inventoryHintText.text = PhaseCUITheme.IsMobileScreen
            ? (count > 0 ? $"Bag ({count})" : "Bag")
            : (count > 0 ? $"[I] Inventory ({count})" : "[I] Inventory");
        inventoryHintText.color = isFull ? PhaseCUITheme.TextError : PhaseCUITheme.AccentCyan;
    }

    // --- Action hint (contextual middle section) ---

    private void EvaluateActionHint(bool isFull)
    {
        if (actionHintText == null) return;

        List<int> requiredIds = assemblyController != null
            ? assemblyController.GetCurrentStepRequiredItemIds()
            : null;

        bool hasRequiredItems = requiredIds != null && requiredIds.Count > 0;
        string npc = currentStepInfo.CompletionNpc;

        string hint;
        Color color;

        if (hasRequiredItems && inventoryController != null && inventoryController.HasAllItems(requiredIds))
        {
            hint = string.IsNullOrEmpty(npc) ? "Ready to deliver!" : $"Ready! Go to: {npc}";
            color = PhaseCUITheme.StepDone;
        }
        else if (isFull && hasRequiredItems)
        {
            hint = PhaseCUITheme.IsMobileScreen
                ? "Bag full - tap Bag to drop an item"
                : "Inventory full  [I] then 1-4 to drop";
            color = PhaseCUITheme.TextError;
        }
        else if (!hasRequiredItems && !string.IsNullOrEmpty(npc))
        {
            hint = $"Talk to: {npc}";
            color = PhaseCUITheme.AccentCyan;
        }
        else
        {
            hint = "Walk into items to collect";
            color = PhaseCUITheme.TextSecondary;
        }

        if (hint == lastActionHint && color == lastActionColor) return;

        lastActionHint = hint;
        lastActionColor = color;
        actionHintText.text = hint;
        actionHintText.color = color;
    }

    // --- Task hint ---

    private void UpdateTaskHint(PhaseCAssemblyController.StepInfo info)
    {
        if (taskHintText == null) return;
        if (info.StepCount == 0)
        {
            taskHintText.text = "";
            return;
        }
        if (info.StepNumber == 0)
        {
            taskHintText.text = !string.IsNullOrEmpty(info.Title) ? "Task: " + info.Title : "";
            return;
        }
        taskHintText.text = $"Task: Step {info.StepNumber}/{info.StepCount} - {info.Title}";
    }

    private void ForceRefresh()
    {
        int count = GetInventoryCount();
        bool isFull = inventoryController != null && inventoryController.IsInventoryFull();
        UpdateInventoryHint(count, isFull);
        UpdateTaskHint(currentStepInfo);
        lastActionHint = null;
        EvaluateActionHint(isFull);
    }

    // --- Minimize / expand ---

    private void ToggleMinimize()
    {
        isMinimized = !isMinimized;

        if (contentRoot != null)
            contentRoot.SetActive(!isMinimized);

        if (stripRect != null)
        {
            float height = isMinimized
                ? PhaseCUITheme.GetHintStripMinimizedHeight()
                : PhaseCUITheme.GetHintStripHeight();
            stripRect.sizeDelta = new Vector2(0f, height);
        }

        if (toggleLabel != null)
            toggleLabel.text = isMinimized ? "+" : "-";
    }

    // --- Canvas creation ---

    private void CreateHintCanvas()
    {
        GameObject existing = GameObject.Find(CanvasName);
        if (existing != null) Destroy(existing);

        GameObject canvasGo = new GameObject(CanvasName);
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 6;

        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(PhaseCUITheme.RefWidth, PhaseCUITheme.RefHeight);
        scaler.matchWidthOrHeight = PhaseCUITheme.CanvasMatchWidthOrHeight;

        canvasGo.AddComponent<GraphicRaycaster>();

        float stripHeight = PhaseCUITheme.GetHintStripHeight();

        // Strip background anchored to bottom of screen
        GameObject stripGo = new GameObject("HintStrip");
        stripGo.transform.SetParent(canvasGo.transform, false);
        Image stripBg = stripGo.AddComponent<Image>();
        stripBg.color = new Color(0.04f, 0.06f, 0.12f, 0.9f);
        stripBg.raycastTarget = false;
        stripRect = stripGo.GetComponent<RectTransform>();
        stripRect.anchorMin = new Vector2(0f, 0f);
        stripRect.anchorMax = new Vector2(1f, 0f);
        stripRect.pivot = new Vector2(0.5f, 0f);
        stripRect.sizeDelta = new Vector2(0f, stripHeight);
        stripRect.anchoredPosition = Vector2.zero;

        // Top border line
        GameObject topLineGo = new GameObject("TopLine");
        topLineGo.transform.SetParent(stripGo.transform, false);
        Image topLine = topLineGo.AddComponent<Image>();
        topLine.color = new Color(PhaseCUITheme.PanelBorder.r, PhaseCUITheme.PanelBorder.g, PhaseCUITheme.PanelBorder.b, 0.5f);
        topLine.raycastTarget = false;
        RectTransform topLineRect = topLineGo.GetComponent<RectTransform>();
        topLineRect.anchorMin = new Vector2(0f, 1f);
        topLineRect.anchorMax = new Vector2(1f, 1f);
        topLineRect.pivot = new Vector2(0.5f, 1f);
        topLineRect.sizeDelta = new Vector2(0f, 2f);
        topLineRect.anchoredPosition = Vector2.zero;

        // Toggle button on the far right (always visible)
        AddToggleButton(stripGo.transform);

        // Content root holds all three text sections (hidden when minimized)
        contentRoot = new GameObject("ContentRoot");
        contentRoot.transform.SetParent(stripGo.transform, false);
        RectTransform contentRootRect = contentRoot.AddComponent<RectTransform>();
        contentRootRect.anchorMin = new Vector2(0f, 0f);
        contentRootRect.anchorMax = new Vector2(0.9f, 1f);
        contentRootRect.offsetMin = Vector2.zero;
        contentRootRect.offsetMax = Vector2.zero;

        float padding = 16f;
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        int fontSize = PhaseCUITheme.GetHintFontSize();

        // Left section: Inventory hint (0 - 22% of content)
        GameObject invGo = new GameObject("InventoryHint");
        invGo.transform.SetParent(contentRoot.transform, false);
        inventoryHintText = invGo.AddComponent<Text>();
        inventoryHintText.text = "[I] Inventory";
        inventoryHintText.font = font;
        inventoryHintText.fontSize = fontSize;
        inventoryHintText.fontStyle = FontStyle.Bold;
        inventoryHintText.color = PhaseCUITheme.AccentCyan;
        inventoryHintText.alignment = TextAnchor.MiddleLeft;
        inventoryHintText.raycastTarget = false;
        RectTransform invRect = invGo.GetComponent<RectTransform>();
        invRect.anchorMin = new Vector2(0f, 0f);
        invRect.anchorMax = new Vector2(0.24f, 1f);
        invRect.offsetMin = new Vector2(padding, 0f);
        invRect.offsetMax = new Vector2(0f, 0f);

        AddSeparatorOnContent(contentRoot.transform, 0.24f);

        // Middle section: contextual action hint (24% - 60%)
        GameObject actionGo = new GameObject("ActionHint");
        actionGo.transform.SetParent(contentRoot.transform, false);
        actionHintText = actionGo.AddComponent<Text>();
        actionHintText.text = "Walk into items to collect";
        actionHintText.font = font;
        actionHintText.fontSize = fontSize;
        actionHintText.color = PhaseCUITheme.TextSecondary;
        actionHintText.alignment = TextAnchor.MiddleCenter;
        actionHintText.raycastTarget = false;
        RectTransform actionRect = actionGo.GetComponent<RectTransform>();
        actionRect.anchorMin = new Vector2(0.24f, 0f);
        actionRect.anchorMax = new Vector2(0.60f, 1f);
        actionRect.offsetMin = new Vector2(padding, 0f);
        actionRect.offsetMax = new Vector2(-padding, 0f);

        AddSeparatorOnContent(contentRoot.transform, 0.60f);

        // Right section: task hint (60% - 100%)
        GameObject taskGo = new GameObject("TaskHint");
        taskGo.transform.SetParent(contentRoot.transform, false);
        taskHintText = taskGo.AddComponent<Text>();
        taskHintText.text = "";
        taskHintText.font = font;
        taskHintText.fontSize = fontSize;
        taskHintText.color = PhaseCUITheme.AccentGold;
        taskHintText.alignment = TextAnchor.MiddleLeft;
        taskHintText.raycastTarget = false;
        taskHintText.horizontalOverflow = PhaseCUITheme.IsMobileScreen
            ? HorizontalWrapMode.Wrap
            : HorizontalWrapMode.Overflow;
        RectTransform taskRect = taskGo.GetComponent<RectTransform>();
        taskRect.anchorMin = new Vector2(0.60f, 0f);
        taskRect.anchorMax = new Vector2(1f, 1f);
        taskRect.offsetMin = new Vector2(padding, 0f);
        taskRect.offsetMax = new Vector2(-padding, 0f);
    }

    private void AddToggleButton(Transform parent)
    {
        // Semi-transparent pill on the right edge
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
        btnRect.anchorMin = new Vector2(1f, 0f);
        btnRect.anchorMax = new Vector2(1f, 1f);
        btnRect.pivot = new Vector2(1f, 0.5f);
        btnRect.sizeDelta = new Vector2(PhaseCUITheme.IsMobileScreen ? 56f : 44f, 0f);
        btnRect.anchoredPosition = Vector2.zero;

        // Label inside the button
        GameObject labelGo = new GameObject("Label");
        labelGo.transform.SetParent(btnGo.transform, false);
        toggleLabel = labelGo.AddComponent<Text>();
        toggleLabel.text = "-";
        toggleLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        toggleLabel.fontSize = PhaseCUITheme.IsMobileScreen ? 26 : 22;
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

    private static void AddSeparatorOnContent(Transform parent, float anchorX)
    {
        GameObject sep = new GameObject("Separator");
        sep.transform.SetParent(parent, false);
        Image sepImg = sep.AddComponent<Image>();
        sepImg.color = new Color(PhaseCUITheme.PanelBorder.r, PhaseCUITheme.PanelBorder.g, PhaseCUITheme.PanelBorder.b, 0.5f);
        sepImg.raycastTarget = false;
        RectTransform sepRect = sep.GetComponent<RectTransform>();
        sepRect.anchorMin = new Vector2(anchorX, 0.1f);
        sepRect.anchorMax = new Vector2(anchorX, 0.9f);
        sepRect.pivot = new Vector2(0.5f, 0.5f);
        sepRect.sizeDelta = new Vector2(1f, 0f);
        sepRect.anchoredPosition = Vector2.zero;
    }
}
