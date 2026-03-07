using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

/// <summary>
/// Always-visible "Return to Hub" button fixed to the bottom-right corner (above the hint strip).
/// Shows a tooltip on hover with context. Auto-created at scene load.
/// </summary>
public class PhaseCReturnHubUI : MonoBehaviour
{
    private const string TargetSceneName = "MinigameC";
    private const string HubSceneName = "CentralHub";
    private const string CanvasName = "PhaseCReturnHubCanvas";

    // Sits above the bottom hint strip (38px)
    private const float BottomPadding = 48f;
    private const float RightPadding = 16f;
    private const float ButtonWidth = 140f;
    private const float ButtonHeight = 48f;

    private const float TooltipWidth = 240f;
    private const float TooltipHeight = 72f;

    private GameObject _tooltip;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void EnsureReturnHubUI()
    {
        SceneManager.sceneLoaded += (scene, _) =>
        {
            if (scene.name != TargetSceneName) return;
            if (FindFirstObjectByType<PhaseCReturnHubUI>() != null) return;
            new GameObject("PhaseCReturnHubUI").AddComponent<PhaseCReturnHubUI>();
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
        BuildCanvas();
    }

    private void BuildCanvas()
    {
        GameObject existing = GameObject.Find(CanvasName);
        if (existing != null) Destroy(existing);

        GameObject canvasGo = new GameObject(CanvasName);
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 15;

        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(PhaseCUITheme.RefWidth, PhaseCUITheme.RefHeight);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGo.AddComponent<GraphicRaycaster>();

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // --- Tooltip (hidden by default, appears above button on hover) ---
        _tooltip = BuildTooltip(canvasGo.transform, font);
        _tooltip.SetActive(false);

        // --- Button ---
        GameObject btnGo = new GameObject("HubButton");
        btnGo.transform.SetParent(canvasGo.transform, false);

        Image btnImg = btnGo.AddComponent<Image>();
        btnImg.color = new Color(0.07f, 0.10f, 0.20f, 0.95f);

        Button btn = btnGo.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(0.80f, 0.90f, 1f, 1f);
        colors.pressedColor = new Color(0.55f, 0.72f, 0.92f, 1f);
        btn.colors = colors;
        btn.onClick.AddListener(ReturnToHub);

        RectTransform btnRect = btnGo.GetComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(1f, 0f);
        btnRect.anchorMax = new Vector2(1f, 0f);
        btnRect.pivot = new Vector2(1f, 0f);
        btnRect.sizeDelta = new Vector2(ButtonWidth, ButtonHeight);
        btnRect.anchoredPosition = new Vector2(-RightPadding, BottomPadding);

        // Border
        GameObject borderGo = new GameObject("Border");
        borderGo.transform.SetParent(btnGo.transform, false);
        Image borderImg = borderGo.AddComponent<Image>();
        borderImg.color = new Color(PhaseCUITheme.AccentCyan.r, PhaseCUITheme.AccentCyan.g, PhaseCUITheme.AccentCyan.b, 0.45f);
        borderImg.raycastTarget = false;
        RectTransform borderRect = borderGo.GetComponent<RectTransform>();
        borderRect.anchorMin = Vector2.zero;
        borderRect.anchorMax = Vector2.one;
        borderRect.offsetMin = new Vector2(-1f, -1f);
        borderRect.offsetMax = new Vector2(1f, 1f);
        borderGo.transform.SetAsFirstSibling();

        // Left accent bar
        GameObject barGo = new GameObject("AccentBar");
        barGo.transform.SetParent(btnGo.transform, false);
        Image barImg = barGo.AddComponent<Image>();
        barImg.color = PhaseCUITheme.AccentCyan;
        barImg.raycastTarget = false;
        RectTransform barRect = barGo.GetComponent<RectTransform>();
        barRect.anchorMin = new Vector2(0f, 0f);
        barRect.anchorMax = new Vector2(0f, 1f);
        barRect.pivot = new Vector2(0f, 0.5f);
        barRect.sizeDelta = new Vector2(4f, 0f);
        barRect.anchoredPosition = Vector2.zero;

        // Icon label (arrow + text)
        GameObject labelGo = new GameObject("Label");
        labelGo.transform.SetParent(btnGo.transform, false);
        Text labelText = labelGo.AddComponent<Text>();
        labelText.text = "< Central Hub";
        labelText.font = font;
        labelText.fontSize = 16;
        labelText.fontStyle = FontStyle.Bold;
        labelText.color = PhaseCUITheme.AccentCyan;
        labelText.alignment = TextAnchor.MiddleCenter;
        labelText.raycastTarget = false;
        RectTransform labelRect = labelGo.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(8f, 0f);
        labelRect.offsetMax = Vector2.zero;

        // Hover handler
        HubButtonHover hover = btnGo.AddComponent<HubButtonHover>();
        hover.tooltip = _tooltip;
    }

    private GameObject BuildTooltip(Transform parent, Font font)
    {
        GameObject tip = new GameObject("HubTooltip");
        tip.transform.SetParent(parent, false);

        Image tipBg = tip.AddComponent<Image>();
        tipBg.color = new Color(0.05f, 0.08f, 0.16f, 0.97f);
        tipBg.raycastTarget = false;

        RectTransform tipRect = tip.GetComponent<RectTransform>();
        tipRect.anchorMin = new Vector2(1f, 0f);
        tipRect.anchorMax = new Vector2(1f, 0f);
        tipRect.pivot = new Vector2(1f, 0f);
        tipRect.sizeDelta = new Vector2(TooltipWidth, TooltipHeight);
        // Sits just above the button
        tipRect.anchoredPosition = new Vector2(-RightPadding, BottomPadding + ButtonHeight + 6f);

        // Border
        GameObject borderGo = new GameObject("Border");
        borderGo.transform.SetParent(tip.transform, false);
        Image borderImg = borderGo.AddComponent<Image>();
        borderImg.color = new Color(PhaseCUITheme.PanelBorder.r, PhaseCUITheme.PanelBorder.g, PhaseCUITheme.PanelBorder.b, 0.7f);
        borderImg.raycastTarget = false;
        RectTransform borderRect = borderGo.GetComponent<RectTransform>();
        borderRect.anchorMin = Vector2.zero;
        borderRect.anchorMax = Vector2.one;
        borderRect.offsetMin = new Vector2(-1f, -1f);
        borderRect.offsetMax = new Vector2(1f, 1f);
        borderGo.transform.SetAsFirstSibling();

        // Top accent bar
        GameObject accentGo = new GameObject("AccentBar");
        accentGo.transform.SetParent(tip.transform, false);
        Image accentImg = accentGo.AddComponent<Image>();
        accentImg.color = PhaseCUITheme.AccentCyan;
        accentImg.raycastTarget = false;
        RectTransform accentRect = accentGo.GetComponent<RectTransform>();
        accentRect.anchorMin = new Vector2(0f, 1f);
        accentRect.anchorMax = new Vector2(1f, 1f);
        accentRect.pivot = new Vector2(0.5f, 1f);
        accentRect.sizeDelta = new Vector2(0f, 3f);
        accentRect.anchoredPosition = Vector2.zero;

        // Title line
        GameObject titleGo = new GameObject("Title");
        titleGo.transform.SetParent(tip.transform, false);
        Text titleText = titleGo.AddComponent<Text>();
        titleText.text = "Return to Central Hub";
        titleText.font = font;
        titleText.fontSize = 15;
        titleText.fontStyle = FontStyle.Bold;
        titleText.color = PhaseCUITheme.TextPrimary;
        titleText.alignment = TextAnchor.MiddleLeft;
        titleText.raycastTarget = false;
        RectTransform titleRect = titleGo.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 0.5f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.offsetMin = new Vector2(12f, 0f);
        titleRect.offsetMax = new Vector2(-8f, -3f);

        // Subtitle line
        GameObject subGo = new GameObject("Sub");
        subGo.transform.SetParent(tip.transform, false);
        Text subText = subGo.AddComponent<Text>();
        subText.text = "Progress is saved automatically";
        subText.font = font;
        subText.fontSize = 12;
        subText.color = PhaseCUITheme.TextSecondary;
        subText.alignment = TextAnchor.MiddleLeft;
        subText.raycastTarget = false;
        RectTransform subRect = subGo.GetComponent<RectTransform>();
        subRect.anchorMin = new Vector2(0f, 0f);
        subRect.anchorMax = new Vector2(1f, 0.5f);
        subRect.offsetMin = new Vector2(12f, 3f);
        subRect.offsetMax = new Vector2(-8f, 0f);

        return tip;
    }

    private void ReturnToHub()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(HubSceneName);
    }
}

/// <summary>Handles pointer enter/exit to show or hide the hub tooltip.</summary>
public class HubButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject tooltip;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltip != null) tooltip.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltip != null) tooltip.SetActive(false);
    }
}
