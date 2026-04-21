using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Mission timer for Minigame C. Counts down from 5 minutes (60 s for testing).
/// On expiry: inventory is cleared, all step progress resets to Step 1, and a small
/// "Time's Up" popup is shown. The player dismisses it and continues - no scene reload,
/// no game-over screen.
/// </summary>
public class MissionTimer : MonoBehaviour
{
    public static MissionTimer Instance { get; private set; }

    private const string TargetSceneName = "MinigameC";
    private const float TopUiSeamOverlap = 0f;
    private const float HubWidgetVerticalGap = 0f;

    [Header("Timer")]
    [SerializeField] private float startingTimeSeconds = 300f;
    [SerializeField] private float warningThresholdSeconds  = 60f;  // yellow below 1 min
    [SerializeField] private float criticalThresholdSeconds = 30f;  // red + pulse below 30 s
    [SerializeField] private float warningSoundStartElapsedSeconds = 285f; // 04:45 elapsed
    [SerializeField] private float warningSoundEndElapsedSeconds = 300f;   // 05:00 elapsed

    private float currentTime;
    private bool isRunning;
    private bool expiredHandled;   // prevents double-trigger on the same expiry

    // Timer widget
    private Canvas timerCanvas;
    private CanvasScaler timerScaler;
    private Image timerPanelBg;
    private TMP_Text timerLabel;       // "MISSION TIME"
    private TMP_Text timerText;        // "05:00"
    private Image progressBarFill;
    private RectTransform _timerWidgetRect;
    private RectTransform _hubBtnRect;
    private RectTransform _hintStripRect;
    private int _lastScreenWidth;
    private int _lastScreenHeight;
    private bool _isTipsPanelVisible;
    private float _lastTopUiBottom = -1f;

    // Hub confirmation dialog
    private GameObject _hubConfirmRoot;

    // Mission alert (startup banner)
    private GameObject missionAlertRoot;
    private CanvasGroup missionAlertGroup;

    // Time's-up popup
    private GameObject timeUpRoot;
    private CanvasGroup timeUpGroup;

    // Pulse
    private float pulseTime;

    /// <summary>Fired each time the timer reaches zero (can happen multiple times if player keeps playing).</summary>
    public event Action TimeExpired;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void EnsureTimer()
    {
        SceneManager.sceneLoaded += (scene, _) =>
        {
            if (scene.name != TargetSceneName) return;
            if (FindFirstObjectByType<MissionTimer>() != null) return;
            var go = new GameObject("MissionTimer");
            go.AddComponent<MissionTimer>();
        };
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        if (SceneManager.GetActiveScene().name != TargetSceneName) { Destroy(gameObject); return; }

        currentTime    = startingTimeSeconds;
        isRunning      = true;
        expiredHandled = false;
        BuildUI();
        UpdateTimerDisplay();
        StartCoroutine(ShowMissionAlertRoutine());

        var assembly = PhaseCAssemblyController.Instance;
        if (assembly != null)
            assembly.PhaseCComplete += OnPhaseCComplete;
    }

    private void OnDestroy()
    {
        var assembly = PhaseCAssemblyController.Instance;
        if (assembly != null)
            assembly.PhaseCComplete -= OnPhaseCComplete;
    }

    private void OnPhaseCComplete() => isRunning = false; // stop timer on victory

    private void Update()
    {
        float currentTopUiBottom = GetTopUiBottomRuntime();
        if (Screen.width != _lastScreenWidth || Screen.height != _lastScreenHeight)
        {
            _lastScreenWidth  = Screen.width;
            _lastScreenHeight = Screen.height;
            ApplyResponsiveLayout();
        }
        else if (Mathf.Abs(currentTopUiBottom - _lastTopUiBottom) > 0.25f)
        {
            // Keep widgets locked directly below live hint/tips layout changes.
            ApplyResponsiveLayout();
        }

        if (!isRunning) return;

        currentTime -= Time.deltaTime;
        if (currentTime < 0f) currentTime = 0f;
        UpdateWarningSoundWindow();
        UpdateTimerDisplay();

        // Pulse panel bg red when critical
        if (currentTime <= criticalThresholdSeconds && timerPanelBg != null)
        {
            pulseTime += Time.deltaTime * 5f;
            float p = (Mathf.Sin(pulseTime) + 1f) * 0.5f;
            timerPanelBg.color = Color.Lerp(
                new Color(0.22f, 0.04f, 0.04f, 0.94f),
                new Color(0.50f, 0.05f, 0.05f, 0.97f), p);
        }

        if (currentTime <= 0f && !expiredHandled)
        {
            expiredHandled = true;
            isRunning = false;
            MinigameCAudioManager.StopWarningAlarmLoop();
            OnTimeExpired();
        }
    }

    // ── Expiry: reset everything in place, show popup ────────────────────────

    private void OnTimeExpired()
    {
        // 1. Clear inventory
        var inv = FindFirstObjectByType<InventoryController>();
        if (inv != null) inv.ClearAllInventory();

        // 2. Reset all step progress back to Step 1
        var assembly = PhaseCAssemblyController.Instance;
        if (assembly != null) assembly.ResetProgress();

        // 3. Notify listeners
        TimeExpired?.Invoke();

        // 4. Pause and show popup
        MinigameCAudioManager.StopWarningAlarmLoop();
        Time.timeScale = 0f;
        ShowTimeUpPopup();
    }

    /// <summary>Called by the "Continue" button on the popup.</summary>
    public void OnTimeUpContinue()
    {
        if (timeUpRoot != null) timeUpRoot.SetActive(false);

        // Reset panel bg to normal (pulse state cleared)
        pulseTime = 0f;
        if (timerPanelBg != null) timerPanelBg.color = PhaseCUITheme.PanelBg;

        // Restart timer
        currentTime    = startingTimeSeconds;
        expiredHandled = false;
        isRunning      = true;
        Time.timeScale = 1f;
        MinigameCAudioManager.StopWarningAlarmLoop();

        UpdateTimerDisplay();
    }

    // ── Mission alert coroutine ──────────────────────────────────────────────

    private IEnumerator ShowMissionAlertRoutine()
    {
        if (missionAlertRoot == null || missionAlertGroup == null) yield break;

        missionAlertRoot.SetActive(true);
        missionAlertGroup.alpha = 1f;

        yield return new WaitForSeconds(4.5f);

        float elapsed = 0f;
        const float fadeDuration = 0.7f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            missionAlertGroup.alpha = 1f - (elapsed / fadeDuration);
            yield return null;
        }

        missionAlertRoot.SetActive(false);
    }

    // ── Timer display ────────────────────────────────────────────────────────

    private void UpdateTimerDisplay()
    {
        if (timerText == null) return;

        int minutes = Mathf.FloorToInt(currentTime / 60f);
        int seconds = Mathf.FloorToInt(currentTime % 60f);
        timerText.text = $"{minutes:00}:{seconds:00}";

        // Progress bar
        if (progressBarFill != null)
        {
            float ratio = currentTime / startingTimeSeconds;
            progressBarFill.fillAmount = ratio;
            progressBarFill.color = ratio > 0.33f
                ? new Color(0.32f, 0.76f, 0.45f, 1f)   // green
                : ratio > 0.15f
                    ? new Color(1f, 0.74f, 0.20f, 1f)   // yellow
                    : new Color(1f, 0.26f, 0.20f, 1f);  // red
        }

        // Text colour
        Color textColor = currentTime <= criticalThresholdSeconds
            ? new Color(1f, 0.30f, 0.25f, 1f)
            : currentTime <= warningThresholdSeconds
                ? new Color(1f, 0.85f, 0.30f, 1f)
                : PhaseCUITheme.TextTitle;

        timerText.color = textColor;
        if (timerLabel != null)
            timerLabel.color = new Color(textColor.r, textColor.g, textColor.b, 0.65f);

        // Restore normal panel bg once outside critical zone
        if (currentTime > criticalThresholdSeconds && timerPanelBg != null)
            timerPanelBg.color = PhaseCUITheme.PanelBg;
    }

    private void UpdateWarningSoundWindow()
    {
        if (startingTimeSeconds <= 0f)
            return;

        float elapsed = startingTimeSeconds - currentTime;
        bool inWindow = elapsed >= warningSoundStartElapsedSeconds
                        && elapsed < warningSoundEndElapsedSeconds;

        if (inWindow)
            MinigameCAudioManager.StartWarningAlarmLoop();
        else
            MinigameCAudioManager.StopWarningAlarmLoop();
    }

    // ── Popup ────────────────────────────────────────────────────────────────

    private void ShowTimeUpPopup()
    {
        if (timeUpRoot != null)
            timeUpRoot.SetActive(true);
    }

    // ── Responsive ───────────────────────────────────────────────────────────

    private void ApplyResponsiveLayout()
    {
        if (timerScaler != null)
            timerScaler.matchWidthOrHeight = PhaseCUITheme.CanvasMatchWidthOrHeight;

        float topUiBottom = GetTopUiBottomRuntime();
        _lastTopUiBottom = topUiBottom;

        float yOffset = -(topUiBottom - TopUiSeamOverlap);
        float timerW = GetTimerWidth();
        float hubW   = GetHubBtnWidth();
        float h      = GetWidgetHeight();

        if (_timerWidgetRect != null)
        {
            _timerWidgetRect.sizeDelta        = new Vector2(timerW, h);
            _timerWidgetRect.anchoredPosition = new Vector2(0f, yOffset);
        }
        if (_hubBtnRect != null)
        {
            _hubBtnRect.sizeDelta        = new Vector2(hubW, h);
            _hubBtnRect.anchoredPosition = new Vector2(0f, yOffset - h - HubWidgetVerticalGap);
        }
    }

    private static float GetTimerWidth()   => PhaseCUITheme.GetTimerWidgetWidth();
    private static float GetHubBtnWidth()  => PhaseCUITheme.GetHubWidgetWidth();
    private static float GetWidgetHeight() => PhaseCUITheme.GetTimerWidgetHeight();

    private float GetTopUiBottomRuntime()
    {
        float topUiBottom = GetInfoStripBottomYRuntime();
        if (_isTipsPanelVisible)
        {
            var hintUI = FindFirstObjectByType<PhaseCPersistentHintUI>();
            float tipsBottomY = hintUI != null ? hintUI.GetTipsPanelBottomY() : 0f;
            topUiBottom = Mathf.Max(topUiBottom, tipsBottomY);
        }
        return topUiBottom;
    }

    /// <summary>Called by PhaseCPersistentHintUI when Tips panel is toggled.</summary>
    public void OnTipsPanelToggled(bool isVisible)
    {
        _isTipsPanelVisible = isVisible;
        ApplyResponsiveLayout();
    }

    // ── UI construction ──────────────────────────────────────────────────────

    private void BuildUI()
    {
        GameObject canvasGo = new GameObject("MissionTimerCanvas");
        timerCanvas = canvasGo.AddComponent<Canvas>();
        timerCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        timerCanvas.sortingOrder = PhaseCUITheme.SortOrderTimer;
        timerScaler = canvasGo.AddComponent<CanvasScaler>();
        timerScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        timerScaler.referenceResolution = new Vector2(PhaseCUITheme.RefWidth, PhaseCUITheme.RefHeight);
        timerScaler.matchWidthOrHeight = PhaseCUITheme.CanvasMatchWidthOrHeight;
        canvasGo.AddComponent<GraphicRaycaster>();

        BuildTimerWidget(canvasGo.transform);
        BuildHubButton(canvasGo.transform);
        BuildHubConfirmDialog(canvasGo.transform);
        BuildMissionAlert(canvasGo.transform);
        BuildTimeUpPopup(canvasGo.transform);
    }

    /// <summary>Styled timer panel: label + MM:SS digits + draining progress bar.</summary>
    private void BuildTimerWidget(Transform parent)
    {
        float infoStripBottom = GetInfoStripBottomYRuntime();
        float timerW = GetTimerWidth();
        float h      = GetWidgetHeight();

        var panelGo = new GameObject("TimerWidget");
        panelGo.transform.SetParent(parent, false);
        timerPanelBg = panelGo.AddComponent<Image>();
        timerPanelBg.color = PhaseCUITheme.PanelBg;
        _timerWidgetRect = panelGo.GetComponent<RectTransform>();
        var pr = _timerWidgetRect;
        pr.anchorMin = new Vector2(1f, 1f);
        pr.anchorMax = new Vector2(1f, 1f);
        pr.pivot = new Vector2(1f, 1f);
        pr.anchoredPosition = new Vector2(0f, -(infoStripBottom - TopUiSeamOverlap));
        pr.sizeDelta = new Vector2(timerW, h);

        // "MISSION TIME" caption
        var labelGo = new GameObject("MissionTimeLabel");
        labelGo.transform.SetParent(panelGo.transform, false);
        timerLabel = labelGo.AddComponent<TextMeshProUGUI>();
        timerLabel.text = "MISSION\nTIME";
        timerLabel.fontSize = PhaseCUITheme.GetTimerCaptionFontSize();
        timerLabel.fontStyle = FontStyles.Bold;
        timerLabel.color = new Color(PhaseCUITheme.TextTitle.r, PhaseCUITheme.TextTitle.g, PhaseCUITheme.TextTitle.b, 0.75f);
        timerLabel.alignment = TextAlignmentOptions.Left;
        timerLabel.enableWordWrapping = false;
        var lRect = labelGo.GetComponent<RectTransform>();
        lRect.anchorMin = new Vector2(0f, 0f);
        lRect.anchorMax = new Vector2(0.55f, 1f);
        lRect.pivot = new Vector2(0f, 0.5f);
        lRect.offsetMin = new Vector2(8f, 4f);
        lRect.offsetMax = new Vector2(-4f, -4f);

        // MM:SS digits
        var textGo = new GameObject("TimerDigits");
        textGo.transform.SetParent(panelGo.transform, false);
        timerText = textGo.AddComponent<TextMeshProUGUI>();
        timerText.fontSize = PhaseCUITheme.GetTimerDigitsFontSize();
        timerText.fontStyle = FontStyles.Bold;
        timerText.color = PhaseCUITheme.TextTitle;
        timerText.alignment = TextAlignmentOptions.Right;
        var tRect = textGo.GetComponent<RectTransform>();
        tRect.anchorMin = new Vector2(0.55f, 0f);
        tRect.anchorMax = new Vector2(1f, 1f);
        tRect.pivot = new Vector2(1f, 0.5f);
        tRect.offsetMin = new Vector2(2f, 4f);
        tRect.offsetMax = new Vector2(-8f, -4f);

        progressBarFill = null;
    }

    /// <summary>Compact "Go to Central Hub" icon button placed to the left of the timer widget.</summary>
    private void BuildHubButton(Transform parent)
    {
        float infoStripBottom = GetInfoStripBottomYRuntime();
        float timerW = GetTimerWidth();
        float hubW   = GetHubBtnWidth();
        float h      = GetWidgetHeight();

        var btnGo = new GameObject("HubButton");
        btnGo.transform.SetParent(parent, false);

        // Background panel (same look as the timer)
        var bg = btnGo.AddComponent<Image>();
        bg.color = PhaseCUITheme.PanelBg;

        _hubBtnRect = btnGo.GetComponent<RectTransform>();
        _hubBtnRect.anchorMin = new Vector2(1f, 1f);
        _hubBtnRect.anchorMax = new Vector2(1f, 1f);
        _hubBtnRect.pivot     = new Vector2(1f, 1f);
        _hubBtnRect.sizeDelta        = new Vector2(hubW, h);
        _hubBtnRect.anchoredPosition = new Vector2(0f, -(infoStripBottom - TopUiSeamOverlap) - h - HubWidgetVerticalGap);

        // Left label: two-line title.
        var iconGo = new GameObject("HubIcon");
        iconGo.transform.SetParent(btnGo.transform, false);
        var iconTmp = iconGo.AddComponent<TextMeshProUGUI>();
        iconTmp.text      = "CENTRAL\nHUB";
        iconTmp.fontSize  = PhaseCUITheme.GetHubLabelFontSize();
        iconTmp.fontStyle = FontStyles.Bold;
        iconTmp.color     = new Color(PhaseCUITheme.TextTitle.r, PhaseCUITheme.TextTitle.g, PhaseCUITheme.TextTitle.b, 0.75f);
        iconTmp.alignment = TextAlignmentOptions.Left;
        iconTmp.enableWordWrapping = false;
        iconTmp.raycastTarget = false;
        var iRect = iconGo.GetComponent<RectTransform>();
        iRect.anchorMin = new Vector2(0f, 0f);
        iRect.anchorMax = new Vector2(0.55f, 1f);
        iRect.pivot = new Vector2(0f, 0.5f);
        iRect.offsetMin = new Vector2(8f, 4f);
        iRect.offsetMax = new Vector2(-4f, -4f);

        // Right value: hub icon.
        var lblGo = new GameObject("HubLabel");
        lblGo.transform.SetParent(btnGo.transform, false);
        var lblTmp = lblGo.AddComponent<TextMeshProUGUI>();
        lblTmp.text      = "⌂";
        lblTmp.fontSize  = PhaseCUITheme.GetHubIconFontSize();
        lblTmp.fontStyle = FontStyles.Bold;
        lblTmp.color     = PhaseCUITheme.AccentCyan;
        lblTmp.alignment = TextAlignmentOptions.Right;
        lblTmp.raycastTarget = false;
        var lRect = lblGo.GetComponent<RectTransform>();
        lRect.anchorMin = new Vector2(0.55f, 0f);
        lRect.anchorMax = new Vector2(1f, 1f);
        lRect.pivot = new Vector2(1f, 0.5f);
        lRect.offsetMin = new Vector2(2f, 4f);
        lRect.offsetMax = new Vector2(-8f, -4f);

        // Clickable button component on top
        var btn = btnGo.AddComponent<Button>();
        btn.targetGraphic = bg;
        var cb = btn.colors;
        cb.highlightedColor = new Color(0.25f, 0.45f, 0.65f, 1f);
        cb.pressedColor     = new Color(0.40f, 0.60f, 0.80f, 1f);
        btn.colors = cb;
        btn.onClick.AddListener(ShowHubConfirmDialog);
    }

    /// <summary>Modal confirmation dialog that appears when the player taps the Hub button.</summary>
    private void BuildHubConfirmDialog(Transform parent)
    {
        _hubConfirmRoot = new GameObject("HubConfirmDialog");
        _hubConfirmRoot.transform.SetParent(parent, false);
        _hubConfirmRoot.SetActive(false);

        Canvas hubPopupCanvas = _hubConfirmRoot.AddComponent<Canvas>();
        hubPopupCanvas.overrideSorting = true;
        hubPopupCanvas.sortingOrder = PhaseCUITheme.SortOrderHubConfirmPopup;
        _hubConfirmRoot.AddComponent<GraphicRaycaster>();

        // Full-screen dim that blocks interaction with the world behind it
        var dimImg = _hubConfirmRoot.AddComponent<Image>();
        dimImg.color = new Color(0f, 0f, 0f, 0.60f);
        dimImg.raycastTarget = true;
        var dimRect = _hubConfirmRoot.GetComponent<RectTransform>();
        dimRect.anchorMin = Vector2.zero;
        dimRect.anchorMax = Vector2.one;
        dimRect.offsetMin = dimRect.offsetMax = Vector2.zero;

        // Popup panel - centred
        var panelGo = new GameObject("ConfirmPanel");
        panelGo.transform.SetParent(_hubConfirmRoot.transform, false);
        panelGo.AddComponent<Image>().color = PhaseCUITheme.PanelBg;
        var pr = panelGo.GetComponent<RectTransform>();
        pr.anchorMin = pr.anchorMax = new Vector2(0.5f, 0.5f);
        pr.pivot = new Vector2(0.5f, 0.5f);
        pr.anchoredPosition = Vector2.zero;
        pr.sizeDelta = new Vector2(560f, 260f);

        AddAccentBar(panelGo.transform);

        // Border
        var borderGo = new GameObject("Border");
        borderGo.transform.SetParent(panelGo.transform, false);
        borderGo.AddComponent<Image>().color = PhaseCUITheme.PanelBorder;
        var bRect = borderGo.GetComponent<RectTransform>();
        bRect.anchorMin = Vector2.zero;
        bRect.anchorMax = Vector2.one;
        bRect.offsetMin = new Vector2(-1f, -1f);
        bRect.offsetMax = new Vector2(1f, 1f);
        borderGo.transform.SetAsFirstSibling();

        MakeText(panelGo.transform, "Title", "Return to Central Hub?",
            PhaseCUITheme.FontSizeTitle, PhaseCUITheme.TextTitle, TextAlignmentOptions.Center, true,
            new Vector2(0f, -36f), new Vector2(490f, 52f));

        MakeText(panelGo.transform, "Subtitle", "Your progress is saved automatically.",
            PhaseCUITheme.FontSizeBody, PhaseCUITheme.TextSecondary, TextAlignmentOptions.Center, false,
            new Vector2(0f, -102f), new Vector2(490f, 44f));

        // "Return" button (left)
        var returnBtn = MakeButton(panelGo.transform, "ReturnBtn", "Return", new Vector2(-90f, -182f));
        returnBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(200f, PhaseCUITheme.ButtonHeight);
        returnBtn.onClick.AddListener(OnHubConfirm);

        // "Cancel" button (right)
        var cancelBtn = MakeButton(panelGo.transform, "CancelBtn", "Cancel", new Vector2(90f, -182f));
        cancelBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(200f, PhaseCUITheme.ButtonHeight);
        cancelBtn.GetComponent<Image>().color = new Color(0.22f, 0.22f, 0.28f, 1f);
        cancelBtn.onClick.AddListener(OnHubCancel);
    }

    private void ShowHubConfirmDialog()
    {
        if (_hubConfirmRoot != null)
            _hubConfirmRoot.SetActive(true);
    }

    private void OnHubConfirm()
    {
        if (_hubConfirmRoot != null)
            _hubConfirmRoot.SetActive(false);
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene("CentralHub");
    }

    private void OnHubCancel()
    {
        if (_hubConfirmRoot != null)
            _hubConfirmRoot.SetActive(false);
    }

    /// <summary>Centered startup banner: tells player about the 5-min limit.</summary>
    private void BuildMissionAlert(Transform parent)
    {
        missionAlertRoot = new GameObject("MissionAlert");
        missionAlertRoot.transform.SetParent(parent, false);
        missionAlertRoot.SetActive(false);

        Canvas missionAlertCanvas = missionAlertRoot.AddComponent<Canvas>();
        missionAlertCanvas.overrideSorting = true;
        // Keep this briefing above all regular HUD/cutscene canvases so it is never hidden.
        missionAlertCanvas.sortingOrder = Mathf.Max(
            PhaseCUITheme.SortOrderMissionAlertPopup,
            PhaseCUITheme.SortOrderTipsPopup + 1);

        var rootImg = missionAlertRoot.AddComponent<Image>();
        rootImg.color = Color.clear;
        rootImg.raycastTarget = false;
        var rootRect = missionAlertRoot.GetComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = rootRect.offsetMax = Vector2.zero;

        missionAlertGroup = missionAlertRoot.AddComponent<CanvasGroup>();
        missionAlertGroup.blocksRaycasts = false;

        var alertPanel = new GameObject("AlertPanel");
        alertPanel.transform.SetParent(missionAlertRoot.transform, false);
        alertPanel.AddComponent<Image>().color = new Color(0.06f, 0.08f, 0.18f, 0.93f);
        var panelRect = alertPanel.GetComponent<RectTransform>();
        // Keep the panel centered, but scale width to 80% of screen.
        panelRect.anchorMin = new Vector2(0.1f, 0.5f);
        panelRect.anchorMax = new Vector2(0.9f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(0f, 160f);

        AddAccentBar(alertPanel.transform);

        var headerText = MakeText(alertPanel.transform, "AlertHeader", "MISSION BRIEFING",
            PhaseCUITheme.FontSizeCaption, PhaseCUITheme.AccentGold, TextAlignmentOptions.Center, true,
            new Vector2(0f, -10f), new Vector2(0f, 26f));
        var headerRect = headerText.GetComponent<RectTransform>();
        headerRect.anchorMin = new Vector2(0f, 1f);
        headerRect.anchorMax = new Vector2(1f, 1f);
        headerRect.pivot = new Vector2(0.5f, 1f);
        headerRect.anchoredPosition = new Vector2(0f, -12f);
        headerRect.offsetMin = new Vector2(PhaseCUITheme.PaddingPanel, -38f);
        headerRect.offsetMax = new Vector2(-PhaseCUITheme.PaddingPanel, -12f);

        var bodyGo = new GameObject("AlertBody");
        bodyGo.transform.SetParent(alertPanel.transform, false);
        var bodyText = bodyGo.AddComponent<TextMeshProUGUI>();
        bodyText.text = "Complete this mission within  <color=#E3C050><b>5 minutes</b></color>  or all inventory and progress will be reset!";
        bodyText.richText = true;
        bodyText.fontSize = PhaseCUITheme.FontSizeBody;
        bodyText.color = PhaseCUITheme.TextTitle;
        bodyText.alignment = TextAlignmentOptions.Center;
        bodyText.enableWordWrapping = true;
        var bodyRect = bodyGo.GetComponent<RectTransform>();
        bodyRect.anchorMin = new Vector2(0f, 1f);
        bodyRect.anchorMax = new Vector2(1f, 1f);
        bodyRect.pivot = new Vector2(0.5f, 1f);
        bodyRect.anchoredPosition = new Vector2(0f, -52f);
        bodyRect.offsetMin = new Vector2(PhaseCUITheme.PaddingPanel, -148f);
        bodyRect.offsetMax = new Vector2(-PhaseCUITheme.PaddingPanel, -52f);
    }

    /// <summary>Small centred popup shown when timer expires. Game is paused while it's visible.</summary>
    private void BuildTimeUpPopup(Transform parent)
    {
        // Semi-transparent full-screen dim (lets player see the map behind it)
        timeUpRoot = new GameObject("TimeUpPopup");
        timeUpRoot.transform.SetParent(parent, false);
        timeUpRoot.SetActive(false);

        Canvas timeUpCanvas = timeUpRoot.AddComponent<Canvas>();
        timeUpCanvas.overrideSorting = true;
        timeUpCanvas.sortingOrder = PhaseCUITheme.SortOrderTimeUpPopup;
        timeUpRoot.AddComponent<GraphicRaycaster>();

        var dimImg = timeUpRoot.AddComponent<Image>();
        dimImg.color = new Color(0f, 0f, 0f, 0.55f);
        dimImg.raycastTarget = true; // block clicks on world behind popup
        var dimRect = timeUpRoot.GetComponent<RectTransform>();
        dimRect.anchorMin = Vector2.zero;
        dimRect.anchorMax = Vector2.one;
        dimRect.offsetMin = dimRect.offsetMax = Vector2.zero;

        timeUpGroup = timeUpRoot.AddComponent<CanvasGroup>();

        // Popup panel - centred, reasonably compact
        var panelGo = new GameObject("PopupPanel");
        panelGo.transform.SetParent(timeUpRoot.transform, false);
        panelGo.AddComponent<Image>().color = PhaseCUITheme.PanelBg;
        var pr = panelGo.GetComponent<RectTransform>();
        pr.anchorMin = pr.anchorMax = new Vector2(0.5f, 0.5f);
        pr.pivot = new Vector2(0.5f, 0.5f);
        pr.anchoredPosition = Vector2.zero;
        pr.sizeDelta = new Vector2(560f, 280f);

        AddAccentBar(panelGo.transform);

        // Title: "TIME'S UP"
        MakeText(panelGo.transform, "Title", "TIME'S UP",
            PhaseCUITheme.FontSizeTitle, new Color(1f, 0.35f, 0.25f, 1f), TextAlignmentOptions.Center, true,
            new Vector2(0f, -30f), new Vector2(480f, 50f));

        // Message
        MakeText(panelGo.transform, "Message",
            "All collected items and step progress have been reset.\nPlease try again!",
            PhaseCUITheme.FontSizeBody, PhaseCUITheme.TextBody, TextAlignmentOptions.Center, false,
            new Vector2(0f, -90f), new Vector2(480f, 90f));

        // Continue button
        var btn = MakeButton(panelGo.transform, "ContinueBtn", "Continue", new Vector2(0f, -195f));
        btn.onClick.AddListener(OnTimeUpContinue);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static void AddAccentBar(Transform parent)
    {
        var go = new GameObject("AccentBar");
        go.transform.SetParent(parent, false);
        go.AddComponent<Image>().color = PhaseCUITheme.AccentGold;
        var r = go.GetComponent<RectTransform>();
        r.anchorMin = new Vector2(0f, 1f);
        r.anchorMax = new Vector2(1f, 1f);
        r.pivot = new Vector2(0.5f, 1f);
        r.anchoredPosition = Vector2.zero;
        r.sizeDelta = new Vector2(0f, PhaseCUITheme.AccentBarHeight);
    }

    private static TMP_Text MakeText(Transform parent, string name, string text, float fontSize,
        Color color, TextAlignmentOptions alignment, bool bold,
        Vector2 anchoredPos, Vector2 sizeDelta)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
        tmp.alignment = alignment;
        tmp.enableWordWrapping = true;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = sizeDelta;
        return tmp;
    }

    private static Button MakeButton(Transform parent, string name, string label, Vector2 anchoredPos)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<Image>().color = PhaseCUITheme.ButtonBg;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = new Vector2(PhaseCUITheme.ButtonWidthMin, PhaseCUITheme.ButtonHeight);
        var btn = go.AddComponent<Button>();

        var lblGo = new GameObject("Label");
        lblGo.transform.SetParent(go.transform, false);
        var tmp = lblGo.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = PhaseCUITheme.FontSizeButton;
        tmp.color = PhaseCUITheme.TextTitle;
        tmp.alignment = TextAlignmentOptions.Center;
        var lblRect = lblGo.GetComponent<RectTransform>();
        lblRect.anchorMin = Vector2.zero;
        lblRect.anchorMax = Vector2.one;
        lblRect.offsetMin = lblRect.offsetMax = Vector2.zero;
        return btn;
    }

    private float GetInfoStripBottomYRuntime()
    {
        if (_hintStripRect == null)
        {
            GameObject hintStrip = GameObject.Find("HintStrip");
            if (hintStrip != null)
                _hintStripRect = hintStrip.GetComponent<RectTransform>();
        }

        if (_hintStripRect != null)
            return -_hintStripRect.anchoredPosition.y + _hintStripRect.rect.height;

        return PhaseCUITheme.GetInfoStripBottomY();
    }

    // ── Public API ───────────────────────────────────────────────────────────

    /// <summary>Remaining time in seconds.</summary>
    public float GetRemainingTime() => currentTime;
    /// <summary>Whether the timer is currently counting down.</summary>
    public bool IsRunning => isRunning;
}
