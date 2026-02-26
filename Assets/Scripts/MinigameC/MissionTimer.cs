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

    [Header("Timer")]
    [SerializeField] private float startingTimeSeconds = 300f;
    [SerializeField] private float warningThresholdSeconds  = 60f;  // yellow below 1 min
    [SerializeField] private float criticalThresholdSeconds = 30f;  // red + pulse below 30 s

    private float currentTime;
    private bool isRunning;
    private bool expiredHandled;   // prevents double-trigger on the same expiry

    // Timer widget
    private Canvas timerCanvas;
    private Image timerPanelBg;
    private TMP_Text timerLabel;       // "MISSION TIME"
    private TMP_Text timerText;        // "05:00"
    private Image progressBarFill;

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

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureTimer()
    {
        if (SceneManager.GetActiveScene().name != TargetSceneName) return;
        if (FindFirstObjectByType<MissionTimer>() != null) return;
        var go = new GameObject("MissionTimer");
        go.AddComponent<MissionTimer>();
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
        if (!isRunning) return;

        currentTime -= Time.deltaTime;
        if (currentTime < 0f) currentTime = 0f;
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

    // ── Popup ────────────────────────────────────────────────────────────────

    private void ShowTimeUpPopup()
    {
        if (timeUpRoot != null)
            timeUpRoot.SetActive(true);
    }

    // ── UI construction ──────────────────────────────────────────────────────

    private void BuildUI()
    {
        GameObject canvasGo = new GameObject("MissionTimerCanvas");
        timerCanvas = canvasGo.AddComponent<Canvas>();
        timerCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        timerCanvas.sortingOrder = 12;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(PhaseCUITheme.RefWidth, PhaseCUITheme.RefHeight);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGo.AddComponent<GraphicRaycaster>();

        BuildTimerWidget(canvasGo.transform);
        BuildMissionAlert(canvasGo.transform);
        BuildTimeUpPopup(canvasGo.transform);
    }

    /// <summary>Styled timer panel: label + MM:SS digits + draining progress bar.</summary>
    private void BuildTimerWidget(Transform parent)
    {
        var panelGo = new GameObject("TimerWidget");
        panelGo.transform.SetParent(parent, false);
        timerPanelBg = panelGo.AddComponent<Image>();
        timerPanelBg.color = PhaseCUITheme.PanelBg;
        var pr = panelGo.GetComponent<RectTransform>();
        pr.anchorMin = new Vector2(1f, 1f);
        pr.anchorMax = new Vector2(1f, 1f);
        pr.pivot = new Vector2(1f, 1f);
        pr.anchoredPosition = new Vector2(-PhaseCUITheme.PaddingTight, -PhaseCUITheme.PaddingTight);
        pr.sizeDelta = new Vector2(220f, 90f);

        // Gold left accent bar
        var accentGo = new GameObject("AccentBar");
        accentGo.transform.SetParent(panelGo.transform, false);
        accentGo.AddComponent<Image>().color = PhaseCUITheme.AccentGold;
        var aRect = accentGo.GetComponent<RectTransform>();
        aRect.anchorMin = new Vector2(0f, 0f);
        aRect.anchorMax = new Vector2(0f, 1f);
        aRect.pivot = new Vector2(0f, 0.5f);
        aRect.anchoredPosition = Vector2.zero;
        aRect.sizeDelta = new Vector2(4f, 0f);

        // "MISSION TIME" caption
        var labelGo = new GameObject("MissionTimeLabel");
        labelGo.transform.SetParent(panelGo.transform, false);
        timerLabel = labelGo.AddComponent<TextMeshProUGUI>();
        timerLabel.text = "MISSION TIME";
        timerLabel.fontSize = PhaseCUITheme.FontSizeCaption;
        timerLabel.fontStyle = FontStyles.Bold;
        timerLabel.color = new Color(PhaseCUITheme.TextTitle.r, PhaseCUITheme.TextTitle.g, PhaseCUITheme.TextTitle.b, 0.6f);
        timerLabel.alignment = TextAlignmentOptions.Right;
        var lRect = labelGo.GetComponent<RectTransform>();
        lRect.anchorMin = new Vector2(0f, 1f);
        lRect.anchorMax = new Vector2(1f, 1f);
        lRect.pivot = new Vector2(0.5f, 1f);
        lRect.anchoredPosition = new Vector2(0f, -8f);
        lRect.offsetMin = new Vector2(14f, -30f);
        lRect.offsetMax = new Vector2(-10f, -8f);

        // MM:SS digits
        var textGo = new GameObject("TimerDigits");
        textGo.transform.SetParent(panelGo.transform, false);
        timerText = textGo.AddComponent<TextMeshProUGUI>();
        timerText.fontSize = 40f;
        timerText.fontStyle = FontStyles.Bold;
        timerText.color = PhaseCUITheme.TextTitle;
        timerText.alignment = TextAlignmentOptions.Right;
        var tRect = textGo.GetComponent<RectTransform>();
        tRect.anchorMin = new Vector2(0f, 1f);
        tRect.anchorMax = new Vector2(1f, 1f);
        tRect.pivot = new Vector2(0.5f, 1f);
        tRect.anchoredPosition = new Vector2(0f, -28f);
        tRect.offsetMin = new Vector2(14f, -76f);
        tRect.offsetMax = new Vector2(-10f, -28f);

        // Progress bar track
        var trackGo = new GameObject("ProgressTrack");
        trackGo.transform.SetParent(panelGo.transform, false);
        trackGo.AddComponent<Image>().color = new Color(0.08f, 0.10f, 0.18f, 0.85f);
        var trackRect = trackGo.GetComponent<RectTransform>();
        trackRect.anchorMin = new Vector2(0f, 0f);
        trackRect.anchorMax = new Vector2(1f, 0f);
        trackRect.pivot = new Vector2(0.5f, 0f);
        trackRect.anchoredPosition = Vector2.zero;
        trackRect.offsetMin = new Vector2(4f, 0f);
        trackRect.offsetMax = new Vector2(0f, 0f);
        trackRect.sizeDelta = new Vector2(-4f, 6f);

        // Filled progress bar
        var fillGo = new GameObject("ProgressFill");
        fillGo.transform.SetParent(trackGo.transform, false);
        progressBarFill = fillGo.AddComponent<Image>();
        progressBarFill.color = new Color(0.32f, 0.76f, 0.45f, 1f);
        progressBarFill.type = Image.Type.Filled;
        progressBarFill.fillMethod = Image.FillMethod.Horizontal;
        progressBarFill.fillOrigin = 0;
        progressBarFill.fillAmount = 1f;
        var fillRect = fillGo.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
    }

    /// <summary>Bottom-centre startup banner: tells player about the 5-min limit.</summary>
    private void BuildMissionAlert(Transform parent)
    {
        missionAlertRoot = new GameObject("MissionAlert");
        missionAlertRoot.transform.SetParent(parent, false);
        missionAlertRoot.SetActive(false);

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
        panelRect.anchorMin = new Vector2(0.5f, 0f);
        panelRect.anchorMax = new Vector2(0.5f, 0f);
        panelRect.pivot = new Vector2(0.5f, 0f);
        panelRect.anchoredPosition = new Vector2(0f, 24f);
        panelRect.sizeDelta = new Vector2(720f, 120f);

        AddAccentBar(alertPanel.transform);

        MakeText(alertPanel.transform, "AlertHeader", "MISSION BRIEFING",
            PhaseCUITheme.FontSizeCaption, PhaseCUITheme.AccentGold, TextAlignmentOptions.Center, true,
            new Vector2(0f, -10f), new Vector2(0f, 26f));

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
        bodyRect.anchoredPosition = new Vector2(0f, -40f);
        bodyRect.offsetMin = new Vector2(PhaseCUITheme.PaddingPanel, -100f);
        bodyRect.offsetMax = new Vector2(-PhaseCUITheme.PaddingPanel, -40f);
    }

    /// <summary>Small centred popup shown when timer expires. Game is paused while it's visible.</summary>
    private void BuildTimeUpPopup(Transform parent)
    {
        // Semi-transparent full-screen dim (lets player see the map behind it)
        timeUpRoot = new GameObject("TimeUpPopup");
        timeUpRoot.transform.SetParent(parent, false);
        timeUpRoot.SetActive(false);

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

    // ── Public API ───────────────────────────────────────────────────────────

    /// <summary>Remaining time in seconds.</summary>
    public float GetRemainingTime() => currentTime;
    /// <summary>Whether the timer is currently counting down.</summary>
    public bool IsRunning => isRunning;
}
