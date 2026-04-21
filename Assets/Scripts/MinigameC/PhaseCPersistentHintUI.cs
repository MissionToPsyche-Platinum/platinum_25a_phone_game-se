using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Thin contextual hints strip placed between the guide panel and the timer/hub widgets.
///
/// Left section: contextual action hint
///   1. Has all required items -> "Ready! Go to: [NPC]"
///   2. Inventory full but missing items -> "Inventory full - tap Bag to drop"
///   3. No items required for step -> "Talk to: [NPC]"
///   4. Default -> "Walk into items to collect"
///
/// Right section: static controls reminder
///   "[I / Bag] Inventory  |  Space: skip dialogue"
///
/// The strip is always shown expanded.
/// </summary>
public class PhaseCPersistentHintUI : MonoBehaviour
{
    private const string TargetSceneName = "MinigameC";
    private const string CanvasName = "PhaseCPersistentHintCanvas";

    private InventoryController inventoryController;
    private PhaseCAssemblyController assemblyController;
    private bool initialized;

    private Text actionHintText;
    private Text actionHintTextRef;
    private Text controlsHintTextRef;

    private string lastActionHint = null;
    private Color lastActionColor;

    private PhaseCAssemblyController.StepInfo currentStepInfo;

    private RectTransform stripRect;
    private RectTransform guidePanelRect;
    private GameObject contentRoot;
    private CanvasScaler canvasScaler;
    private int lastScreenWidth;
    private int lastScreenHeight;

    // Tips panel state
    private GameObject tipsBackdrop;
    private GameObject tipsOverlay;
    private bool isTipsVisible = false;
    private RectTransform tipsOverlayRect;
    private RectTransform tipsCloseRect;
    private Text tipsCloseLabel;
    private Text tipsTitleLabel;
    private Text tipsContentLabel;

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
        lastActionHint = null;
    }

    private void LateUpdate()
    {
        if (!initialized) return;

        if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
        {
            lastScreenWidth  = Screen.width;
            lastScreenHeight = Screen.height;
            ApplyResponsiveLayout();
        }

        bool isFull = inventoryController != null && inventoryController.IsInventoryFull();
        EvaluateActionHint(isFull);
    }

    private void ApplyResponsiveLayout()
    {
        if (canvasScaler != null)
            canvasScaler.matchWidthOrHeight = PhaseCUITheme.CanvasMatchWidthOrHeight;

        if (stripRect != null)
        {
            float h = PhaseCUITheme.GetInfoStripHeight();
            stripRect.sizeDelta        = new Vector2(0f, h);
            stripRect.anchoredPosition = new Vector2(0f, -GetGuideBottomY());
        }

        int fontSize = PhaseCUITheme.GetHintFontSize();
        if (actionHintTextRef != null) actionHintTextRef.fontSize = fontSize;
        if (controlsHintTextRef != null) controlsHintTextRef.fontSize = fontSize;
        RefreshTipsText();
        ApplyTipsOverlayLayout();
    }

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
                ? "Bag full : tap Bag to drop an item"
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

        lastActionHint  = hint;
        lastActionColor = color;
        actionHintText.text  = hint;
        actionHintText.color = color;
    }

    private void ForceRefresh()
    {
        bool isFull = inventoryController != null && inventoryController.IsInventoryFull();
        lastActionHint = null;
        EvaluateActionHint(isFull);
    }

    private void CreateHintCanvas()
    {
        GameObject existing = GameObject.Find(CanvasName);
        if (existing != null) Destroy(existing);

        GameObject canvasGo = new GameObject(CanvasName);
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = PhaseCUITheme.SortOrderHintStrip;

        canvasScaler = canvasGo.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(PhaseCUITheme.RefWidth, PhaseCUITheme.RefHeight);
        canvasScaler.matchWidthOrHeight = PhaseCUITheme.CanvasMatchWidthOrHeight;

        canvasGo.AddComponent<GraphicRaycaster>();

        float stripH = PhaseCUITheme.GetInfoStripHeight();

        // Strip: top-anchored, positioned immediately below the guide panel
        GameObject stripGo = new GameObject("HintStrip");
        stripGo.transform.SetParent(canvasGo.transform, false);
        Image stripBg = stripGo.AddComponent<Image>();
        stripBg.color = new Color(0.04f, 0.06f, 0.14f, 0.88f);
        stripBg.raycastTarget = false;
        stripRect = stripGo.GetComponent<RectTransform>();
        stripRect.anchorMin = new Vector2(0f, 1f);
        stripRect.anchorMax = new Vector2(1f, 1f);
        stripRect.pivot     = new Vector2(0.5f, 1f);
        stripRect.anchoredPosition = new Vector2(0f, -GetGuideBottomY());
        stripRect.sizeDelta = new Vector2(0f, stripH);

        // Thin bottom border
        GameObject botLineGo = new GameObject("BottomLine");
        botLineGo.transform.SetParent(stripGo.transform, false);
        Image botLine = botLineGo.AddComponent<Image>();
        botLine.color = new Color(PhaseCUITheme.PanelBorder.r, PhaseCUITheme.PanelBorder.g, PhaseCUITheme.PanelBorder.b, 0.35f);
        botLine.raycastTarget = false;
        RectTransform botLineRect = botLineGo.GetComponent<RectTransform>();
        botLineRect.anchorMin = new Vector2(0f, 0f);
        botLineRect.anchorMax = new Vector2(1f, 0f);
        botLineRect.pivot     = new Vector2(0.5f, 0f);
        botLineRect.sizeDelta = new Vector2(0f, 1f);
        botLineRect.anchoredPosition = Vector2.zero;

        // Content root fills the strip.
        contentRoot = new GameObject("ContentRoot");
        contentRoot.transform.SetParent(stripGo.transform, false);
        RectTransform contentRt = contentRoot.AddComponent<RectTransform>();
        contentRt.anchorMin = new Vector2(0f, 0f);
        contentRt.anchorMax = new Vector2(1f, 1f);
        contentRt.offsetMin = Vector2.zero;
        contentRt.offsetMax = Vector2.zero;

        float pad = 12f;
        Font  font     = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        int   fontSize = PhaseCUITheme.GetHintFontSize();
        const float splitX = 0.62f;

        // Left: contextual action hint
        GameObject actionGo = new GameObject("ActionHint");
        actionGo.transform.SetParent(contentRoot.transform, false);
        actionHintText     = actionGo.AddComponent<Text>();
        actionHintTextRef  = actionHintText;
        actionHintText.text      = "Walk into items to collect";
        actionHintText.font      = font;
        actionHintText.fontSize  = fontSize;
        actionHintText.color     = PhaseCUITheme.TextSecondary;
        actionHintText.alignment = TextAnchor.MiddleLeft;
        actionHintText.raycastTarget = false;
        RectTransform actionRect = actionGo.GetComponent<RectTransform>();
        actionRect.anchorMin = new Vector2(0f,     0f);
        actionRect.anchorMax = new Vector2(splitX, 1f);
        actionRect.offsetMin = new Vector2(pad,  0f);
        actionRect.offsetMax = new Vector2(-pad, 0f);

        // Separator
        AddSeparator(contentRoot.transform, splitX);

        // Right: controls reminder (static, smaller text) + Tips button
        string ctrlText = "[I] Inventory  |  Space: skip";
        GameObject ctrlGo = new GameObject("ControlsHint");
        ctrlGo.transform.SetParent(contentRoot.transform, false);
        Text ctrlLabel = ctrlGo.AddComponent<Text>();
        ctrlLabel.text      = ctrlText;
        ctrlLabel.font      = font;
        ctrlLabel.fontSize  = fontSize;
        ctrlLabel.color     = new Color(PhaseCUITheme.TextSecondary.r, PhaseCUITheme.TextSecondary.g, PhaseCUITheme.TextSecondary.b, 0.70f);
        ctrlLabel.alignment = TextAnchor.MiddleCenter;
        ctrlLabel.raycastTarget = false;
        controlsHintTextRef = ctrlLabel;
        RectTransform ctrlRect = ctrlGo.GetComponent<RectTransform>();
        ctrlRect.anchorMin = new Vector2(splitX, 0f);
        ctrlRect.anchorMax = new Vector2(0.93f, 1f);
        ctrlRect.offsetMin = new Vector2(pad,  0f);
        ctrlRect.offsetMax = new Vector2(0f, 0f);

        // Tips button (question mark icon) in right section
        AddTipsButton(contentRoot.transform, font, fontSize);

        CreateTipsOverlay(canvasGo.transform, stripH);
    }

    private void AddTipsButton(Transform parent, Font font, int fontSize)
    {
        GameObject btnGo = new GameObject("TipsBtn");
        btnGo.transform.SetParent(parent, false);

        Image btnBg = btnGo.AddComponent<Image>();
        btnBg.color = new Color(0.15f, 0.22f, 0.55f, 0.85f);

        Button btn = btnGo.AddComponent<Button>();
        btn.targetGraphic = btnBg;
        ColorBlock cb = btn.colors;
        cb.highlightedColor = new Color(0.30f, 0.50f, 0.75f, 1f);
        cb.pressedColor     = new Color(0.45f, 0.65f, 0.90f, 1f);
        btn.colors = cb;
        btn.onClick.AddListener(ToggleTips);

        RectTransform btnRect = btnGo.GetComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.93f, 0.15f);
        btnRect.anchorMax = new Vector2(0.995f, 0.85f);
        btnRect.offsetMin = new Vector2(2f, 0f);
        btnRect.offsetMax = new Vector2(0f, 0f);

        GameObject labelGo = new GameObject("Label");
        labelGo.transform.SetParent(btnGo.transform, false);
        Text labelText = labelGo.AddComponent<Text>();
        labelText.text = "?";
        labelText.font = font;
        labelText.fontSize = Mathf.Max(14, fontSize);
        labelText.fontStyle = FontStyle.Bold;
        labelText.color = Color.white;
        labelText.alignment = TextAnchor.MiddleCenter;
        labelText.raycastTarget = false;
        RectTransform labelRect = labelGo.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;
    }

    private void CreateTipsOverlay(Transform canvasParent, float stripH)
    {
        tipsBackdrop = new GameObject("TipsBackdrop");
        tipsBackdrop.transform.SetParent(canvasParent, false);
        tipsBackdrop.SetActive(false);

        Canvas backdropCanvas = tipsBackdrop.AddComponent<Canvas>();
        backdropCanvas.overrideSorting = true;
        backdropCanvas.sortingOrder = PhaseCUITheme.SortOrderTipsBackdrop;
        tipsBackdrop.AddComponent<GraphicRaycaster>();

        Image backdropBg = tipsBackdrop.AddComponent<Image>();
        backdropBg.color = new Color(0f, 0f, 0f, 0.30f);
        Button backdropBtn = tipsBackdrop.AddComponent<Button>();
        backdropBtn.targetGraphic = backdropBg;
        backdropBtn.onClick.AddListener(ToggleTips);
        RectTransform backdropRect = tipsBackdrop.GetComponent<RectTransform>();
        backdropRect.anchorMin = Vector2.zero;
        backdropRect.anchorMax = Vector2.one;
        backdropRect.offsetMin = Vector2.zero;
        backdropRect.offsetMax = Vector2.zero;

        tipsOverlay = new GameObject("TipsOverlay");
        tipsOverlay.transform.SetParent(canvasParent, false);
        tipsOverlay.SetActive(false);

        // Keep tips popup above all Phase C HUD panels.
        Canvas tipsCanvas = tipsOverlay.AddComponent<Canvas>();
        tipsCanvas.overrideSorting = true;
        tipsCanvas.sortingOrder = PhaseCUITheme.SortOrderTipsPopup;
        tipsOverlay.AddComponent<GraphicRaycaster>();

        Image bg = tipsOverlay.AddComponent<Image>();
        bg.color = new Color(0.05f, 0.08f, 0.18f, 0.96f);
        tipsOverlayRect = tipsOverlay.GetComponent<RectTransform>();
        ApplyTipsOverlayLayout();

        // Close button
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        GameObject closeBtn = new GameObject("CloseBtn");
        closeBtn.transform.SetParent(tipsOverlay.transform, false);
        Image closeBg = closeBtn.AddComponent<Image>();
        closeBg.color = new Color(0.3f, 0.1f, 0.1f, 0.8f);
        Button close = closeBtn.AddComponent<Button>();
        close.targetGraphic = closeBg;
        close.onClick.AddListener(ToggleTips);
        tipsCloseRect = closeBtn.GetComponent<RectTransform>();
        tipsCloseRect.anchorMin = new Vector2(1f, 1f);
        tipsCloseRect.anchorMax = new Vector2(1f, 1f);
        tipsCloseRect.pivot = new Vector2(1f, 1f);
        tipsCloseRect.sizeDelta = new Vector2(40f, 40f);
        tipsCloseRect.anchoredPosition = new Vector2(-10f, -10f);
        GameObject closeLabel = new GameObject("X");
        closeLabel.transform.SetParent(closeBtn.transform, false);
        tipsCloseLabel = closeLabel.AddComponent<Text>();
        tipsCloseLabel.text = "X";
        tipsCloseLabel.font = font;
        tipsCloseLabel.fontSize = 24;
        tipsCloseLabel.color = Color.white;
        tipsCloseLabel.alignment = TextAnchor.MiddleCenter;
        tipsCloseLabel.raycastTarget = false;
        RectTransform clRect = closeLabel.GetComponent<RectTransform>();
        clRect.anchorMin = Vector2.zero;
        clRect.anchorMax = Vector2.one;
        clRect.offsetMin = Vector2.zero;
        clRect.offsetMax = Vector2.zero;

        // Title
        GameObject titleGo = new GameObject("Title");
        titleGo.transform.SetParent(tipsOverlay.transform, false);
        tipsTitleLabel = titleGo.AddComponent<Text>();
        tipsTitleLabel.text = "Controls & Shortcuts";
        tipsTitleLabel.font = font;
        tipsTitleLabel.fontSize = (int)PhaseCUITheme.GetTipsTitleFontSize();
        tipsTitleLabel.fontStyle = FontStyle.Bold;
        tipsTitleLabel.color = PhaseCUITheme.AccentGold;
        tipsTitleLabel.alignment = TextAnchor.MiddleCenter;
        tipsTitleLabel.raycastTarget = false;
        RectTransform titleRect = titleGo.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 0.85f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.offsetMin = new Vector2(20f, 0f);
        titleRect.offsetMax = new Vector2(-20f, -10f);

        // Content
        string mobileControls =
            "<b>Shortcuts:</b>\n\n" +
            "  : <color=#73B8E0>I</color> : Open/Close Inventory\n" +
            "  : <color=#73B8E0>Space</color> or <color=#73B8E0>E</color> : Talk / Advance dialogue\n" +
            "  : <color=#73B8E0>Tap items</color> in world : Collect them\n" +
            "  : <color=#73B8E0>Tap NPCs</color> : Start conversation\n\n" +
            "<b>Tips:</b>\n" +
            "  : Walk <b>into</b> items to pick them up\n" +
            "  : When inventory is full, open Bag and tap an item to drop\n" +
            "  : The gold arrow points to your next objective\n" +
            "  : Talk to the NPC shown in the top panel to complete steps";

        string desktopControls =
            "<b>Shortcuts:</b>\n\n" +
            "  : <color=#73B8E0>I</color> : Open/Close Inventory\n" +
            "  : <color=#73B8E0>Space</color> or <color=#73B8E0>E</color> : Talk / Advance dialogue\n" +
            "  : <color=#73B8E0>Arrow Keys / WASD</color> : Move\n" +
            "  : <color=#73B8E0>1-4</color> while inventory open : Drop items\n" +
            "  : <color=#73B8E0>M</color> : Open Menu / Map\n\n" +
            "<b>Tips:</b>\n" +
            "  : Walk <b>into</b> items to pick them up\n" +
            "  : The gold arrow points to your next objective\n" +
            "  : Talk to the NPC shown in the top panel to complete steps\n" +
            "  : Elevated items appear around the facility : explore to find parts";

        string contentText = PhaseCUITheme.IsMobileScreen ? mobileControls : desktopControls;

        GameObject contentGo = new GameObject("Content");
        contentGo.transform.SetParent(tipsOverlay.transform, false);
        tipsContentLabel = contentGo.AddComponent<Text>();
        tipsContentLabel.text = contentText;
        tipsContentLabel.font = font;
        tipsContentLabel.fontSize = (int)PhaseCUITheme.GetTipsContentFontSize();
        tipsContentLabel.color = PhaseCUITheme.TextPrimary;
        tipsContentLabel.alignment = TextAnchor.UpperLeft;
        tipsContentLabel.lineSpacing = PhaseCUITheme.GetTipsLineSpacing();
        tipsContentLabel.supportRichText = true;
        tipsContentLabel.raycastTarget = false;
        RectTransform contentRect = contentGo.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 0f);
        contentRect.anchorMax = new Vector2(1f, 0.84f);
        contentRect.offsetMin = new Vector2(30f, 20f);
        contentRect.offsetMax = new Vector2(-30f, -10f);

        // Ensure the close button stays on top and receives clicks.
        closeBtn.transform.SetAsLastSibling();

        RefreshTipsText();
    }

    private void ApplyTipsOverlayLayout()
    {
        if (tipsOverlayRect == null) return;

        float topY = GetGuideBottomY() + PhaseCUITheme.GetTipsTopGap();
        float bottomPadding = PhaseCUITheme.GetTipsBottomPadding();
        float panelHeight = Mathf.Max(240f, PhaseCUITheme.RefHeight - topY - bottomPadding);
        float inset = PhaseCUITheme.GetTipsHorizontalInset();

        tipsOverlayRect.anchorMin = new Vector2(inset, 1f);
        tipsOverlayRect.anchorMax = new Vector2(1f - inset, 1f);
        tipsOverlayRect.pivot = new Vector2(0.5f, 1f);
        tipsOverlayRect.anchoredPosition = new Vector2(0f, -topY);
        tipsOverlayRect.sizeDelta = new Vector2(0f, panelHeight);

        if (tipsCloseRect != null)
        {
            float closeSize = PhaseCUITheme.GetTipsCloseButtonSize();
            tipsCloseRect.sizeDelta = new Vector2(closeSize, closeSize);
            tipsCloseRect.anchoredPosition = new Vector2(-10f, -10f);
        }
    }

    private void RefreshTipsText()
    {
        if (tipsTitleLabel != null)
            tipsTitleLabel.fontSize = (int)PhaseCUITheme.GetTipsTitleFontSize();

        if (tipsContentLabel != null)
        {
            tipsContentLabel.fontSize = PhaseCUITheme.GetHintFontSize();
            tipsContentLabel.lineSpacing = PhaseCUITheme.GetTipsLineSpacing();
        }

        if (tipsCloseLabel != null)
            tipsCloseLabel.fontSize = Mathf.RoundToInt(PhaseCUITheme.GetTipsCloseButtonSize() * 0.55f);
    }

    private float GetGuideBottomY()
    {
        if (guidePanelRect == null)
        {
            GameObject guidePanel = GameObject.Find("PhaseCGuidePanel");
            if (guidePanel != null)
                guidePanelRect = guidePanel.GetComponent<RectTransform>();
        }

        if (guidePanelRect != null)
            return -guidePanelRect.anchoredPosition.y + guidePanelRect.rect.height;

        return PhaseCUITheme.GetGuideBarBottomY();
    }

    private void ToggleTips()
    {
        isTipsVisible = !isTipsVisible;
        if (tipsBackdrop != null)
            tipsBackdrop.SetActive(isTipsVisible);
        if (tipsOverlay != null)
            tipsOverlay.SetActive(isTipsVisible);
        MinigameCAudioManager.PlayHintToggle();

        // Notify MissionTimer to adjust position if needed
        var timer = FindFirstObjectByType<MissionTimer>();
        if (timer != null)
            timer.OnTipsPanelToggled(isTipsVisible);
    }

    /// <summary>Returns the bottom Y position of the Tips overlay when visible.</summary>
    public float GetTipsPanelBottomY()
    {
        if (!isTipsVisible || tipsOverlayRect == null)
            return 0f;
        return -tipsOverlayRect.anchoredPosition.y + tipsOverlayRect.rect.height;
    }

    private static void AddSeparator(Transform parent, float anchorX)
    {
        GameObject sep = new GameObject("Separator");
        sep.transform.SetParent(parent, false);
        Image img = sep.AddComponent<Image>();
        img.color = new Color(PhaseCUITheme.PanelBorder.r, PhaseCUITheme.PanelBorder.g, PhaseCUITheme.PanelBorder.b, 0.4f);
        img.raycastTarget = false;
        RectTransform r = sep.GetComponent<RectTransform>();
        r.anchorMin = new Vector2(anchorX, 0.1f);
        r.anchorMax = new Vector2(anchorX, 0.9f);
        r.pivot     = new Vector2(0.5f, 0.5f);
        r.sizeDelta = new Vector2(1f, 0f);
        r.anchoredPosition = Vector2.zero;
    }
}
