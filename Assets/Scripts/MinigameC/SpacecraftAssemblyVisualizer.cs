using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Permanent HUD panel (bottom-left corner) that shows the spacecraft being built
/// as the player completes Phase C steps.  Auto-created via RuntimeInitializeOnLoadMethod.
///
/// State mapping (StepChanged.StepNumber is the NEXT/current step):
///   StepNumber = 1  (nothing done yet)   → sprite state 0  (empty pad)
///   StepNumber = 2  (C1 done)            → sprite state 1  (bus frame)
///   StepNumber = 3  (C2 done)            → sprite state 2  (bus + power)
///   StepNumber = 4  (C3 done)            → sprite state 3  (+ magnetometer)
///   StepNumber = 5  (C4 done)            → sprite state 4  (+ all instruments)
///   StepNumber = 6  (C5 done)            → sprite state 5  (+ comms)
///   PhaseCComplete  (all done)           → sprite state 6  (complete spacecraft)
/// </summary>
public class SpacecraftAssemblyVisualizer : MonoBehaviour
{
    private const string TargetScene = "MinigameC";
    private const string LibraryPath = "MinigameC/SpacecraftSpriteLibrary";

    // HUD panel dimensions - responsive values pulled from PhaseCUITheme at runtime.
    private const float EdgeMargin  = 14f;

    private const float FadeDuration = 0.5f;
    private const float BounceScale  = 1.12f;

    private SpacecraftSpriteLibrary library;
    private Image                   spacecraftImage;
    private Text                    stepLabel;
    private int                     currentState;
    private Coroutine               transitionCoroutine;

    // Minimize / expand state
    private bool          _isMinimized;
    private RectTransform _panelRect;
    private GameObject    _bodyRoot;
    private Text          _toggleLabel;
    private Text          _titleText;
    private CanvasScaler  _canvasScaler;

    // Track the last screen size so we only re-flow on actual resizes.
    private int _lastScreenWidth;
    private int _lastScreenHeight;

    // ─── Bootstrap ───────────────────────────────────────────────────────────

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Boot()
    {
        SceneManager.sceneLoaded += (scene, _) =>
        {
            if (scene.name != TargetScene) return;
            new GameObject("SpacecraftAssemblyVisualizer")
                .AddComponent<SpacecraftAssemblyVisualizer>();
        };
    }

    // ─── Lifecycle ───────────────────────────────────────────────────────────

    private void Start()
    {
        library = Resources.Load<SpacecraftSpriteLibrary>(LibraryPath);
        if (library == null)
        {
            Debug.LogWarning("SpacecraftAssemblyVisualizer: SpacecraftSpriteLibrary not found at " +
                             "Resources/" + LibraryPath + ". HUD disabled.");
            enabled = false;
            return;
        }

        BuildHUD();
        SetImageImmediate(0);
        StartCoroutine(WaitAndSubscribe());
    }

    private void Update()
    {
        if (Screen.width == _lastScreenWidth && Screen.height == _lastScreenHeight)
            return;

        _lastScreenWidth  = Screen.width;
        _lastScreenHeight = Screen.height;
        ApplyResponsiveLayout();
    }

    private IEnumerator WaitAndSubscribe()
    {
        // Let PhaseCAssemblyController.Start() run before querying it
        yield return null;

        PhaseCAssemblyController ctrl = PhaseCAssemblyController.Instance;
        if (ctrl == null)
        {
            Debug.LogWarning("SpacecraftAssemblyVisualizer: PhaseCAssemblyController not found.");
            yield break;
        }

        ctrl.StepChanged    += OnStepChanged;
        ctrl.PhaseCComplete += OnPhaseCComplete;

        // Sync to current save-game progress immediately
        PhaseCAssemblyController.StepInfo info = ctrl.GetCurrentStepInfo();
        if (info.StepNumber > 0)
        {
            int state = Mathf.Clamp(info.StepNumber - 1, 0, 6);
            SetImageImmediate(state);
            UpdateStepLabel(info.StepNumber, info.StepCount);
        }
    }

    private void OnDestroy()
    {
        if (PhaseCAssemblyController.Instance != null)
        {
            PhaseCAssemblyController.Instance.StepChanged    -= OnStepChanged;
            PhaseCAssemblyController.Instance.PhaseCComplete -= OnPhaseCComplete;
        }
    }

    // ─── Assembly events ─────────────────────────────────────────────────────

    private void OnStepChanged(PhaseCAssemblyController.StepInfo info)
    {
        int target = Mathf.Clamp(info.StepNumber - 1, 0, 6);
        UpdateStepLabel(info.StepNumber, info.StepCount);
        if (target != currentState)
            TransitionTo(target);
    }

    private void OnPhaseCComplete()
    {
        TransitionTo(6);
        if (stepLabel != null) stepLabel.text = "Complete!";
    }

    // ─── Minimize / expand ───────────────────────────────────────────────────

    private void ToggleMinimize()
    {
        _isMinimized = !_isMinimized;

        if (_bodyRoot != null)
            _bodyRoot.SetActive(!_isMinimized);

        if (_panelRect != null)
        {
            float h = _isMinimized
                ? PhaseCUITheme.GetAssemblyTitleBarHeight()
                : PhaseCUITheme.GetBottomPanelHeight();
            _panelRect.anchorMin = new Vector2(0f, 0f);
            _panelRect.anchorMax = new Vector2(PhaseCUITheme.GetLeftBottomPanelAnchorMaxX(), 0f);
            _panelRect.offsetMin = new Vector2(EdgeMargin, EdgeMargin);
            _panelRect.offsetMax = new Vector2(-6f, h + EdgeMargin);
        }

        if (_toggleLabel != null)
            _toggleLabel.text = _isMinimized ? "+" : "-";
    }

    // ─── HUD construction ────────────────────────────────────────────────────

    private void BuildHUD()
    {
        Font builtinFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // ── Canvas ──────────────────────────────────────────────────────────
        GameObject canvasGo = new GameObject("SpacecraftHUDCanvas");
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        _canvasScaler = canvasGo.AddComponent<CanvasScaler>();
        _canvasScaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        _canvasScaler.referenceResolution = new Vector2(1920f, 1080f);
        _canvasScaler.matchWidthOrHeight  = PhaseCUITheme.CanvasMatchWidthOrHeight;
        canvasGo.AddComponent<GraphicRaycaster>();

        // ── Background panel (bottom 0–40% of screen width) ──────────────────
        float panelH    = PhaseCUITheme.GetBottomPanelHeight();
        float titleBarH = PhaseCUITheme.GetAssemblyTitleBarHeight();

        GameObject panelGo = new GameObject("SpacecraftPanel");
        panelGo.transform.SetParent(canvasGo.transform, false);

        Image panelBg = panelGo.AddComponent<Image>();
        panelBg.color = new Color(0.07f, 0.09f, 0.16f, 0.88f);
        panelBg.raycastTarget = false;

        _panelRect           = panelGo.GetComponent<RectTransform>();
        _panelRect.anchorMin = new Vector2(0f, 0f);
        _panelRect.anchorMax = new Vector2(PhaseCUITheme.GetLeftBottomPanelAnchorMaxX(), 0f);
        _panelRect.pivot     = new Vector2(0f, 0f);
        _panelRect.offsetMin = new Vector2(EdgeMargin, EdgeMargin);
        _panelRect.offsetMax = new Vector2(-6f, panelH + EdgeMargin);

        // Border to match Required Items panel styling
        GameObject borderGo = new GameObject("Border");
        borderGo.transform.SetParent(panelGo.transform, false);
        Image borderImg = borderGo.AddComponent<Image>();
        borderImg.color = PhaseCUITheme.PanelBorder;
        borderImg.raycastTarget = false;
        RectTransform borderRect = borderGo.GetComponent<RectTransform>();
        borderRect.anchorMin = Vector2.zero;
        borderRect.anchorMax = Vector2.one;
        borderRect.offsetMin = new Vector2(-1f, -1f);
        borderRect.offsetMax = new Vector2(1f, 1f);
        borderGo.transform.SetAsFirstSibling();

        // ── Title row (always visible - contains label + toggle button) ───────
        GameObject titleRowGo = new GameObject("TitleRow");
        titleRowGo.transform.SetParent(panelGo.transform, false);
        RectTransform titleRowRect    = titleRowGo.AddComponent<RectTransform>();
        titleRowRect.anchorMin        = new Vector2(0f, 1f);
        titleRowRect.anchorMax        = new Vector2(1f, 1f);
        titleRowRect.pivot            = new Vector2(0.5f, 1f);
        titleRowRect.sizeDelta        = new Vector2(0f, titleBarH);
        titleRowRect.anchoredPosition = new Vector2(0f, -2f);

        // Title label (left portion of title row)
        GameObject titleGo = new GameObject("Title");
        titleGo.transform.SetParent(titleRowGo.transform, false);
        _titleText          = titleGo.AddComponent<Text>();
        Text title          = _titleText;
        title.text          = "BUILD STATUS: ASSEMBLY";
        title.font          = builtinFont;
        title.fontSize      = Mathf.Max(16, PhaseCUITheme.GetAssemblyTitleFont() - 2);
        title.fontStyle     = FontStyle.Bold;
        title.color         = new Color(0.9f, 0.85f, 0.4f);
        title.alignment     = TextAnchor.MiddleCenter;
        title.raycastTarget = false;
        RectTransform titleRect    = titleGo.GetComponent<RectTransform>();
        titleRect.anchorMin        = new Vector2(0f, 0f);
        titleRect.anchorMax        = new Vector2(0.8f, 1f);
        titleRect.offsetMin        = new Vector2(4f, 0f);
        titleRect.offsetMax        = Vector2.zero;

        // Toggle button (right portion of title row)
        AddToggleButton(titleRowGo.transform, builtinFont);

        // ── Body root (hidden when minimized) ────────────────────────────────
        _bodyRoot = new GameObject("Body");
        _bodyRoot.transform.SetParent(panelGo.transform, false);
        RectTransform bodyRootRect = _bodyRoot.AddComponent<RectTransform>();
        bodyRootRect.anchorMin = new Vector2(0f, 0f);
        bodyRootRect.anchorMax = new Vector2(1f, 1f);
        bodyRootRect.offsetMin = new Vector2(0f, 0f);
        bodyRootRect.offsetMax = new Vector2(0f, -(titleBarH + 4f));

        // ── Spacecraft image ─────────────────────────────────────────────────
        GameObject imgGo = new GameObject("SpacecraftImage");
        imgGo.transform.SetParent(_bodyRoot.transform, false);

        spacecraftImage                 = imgGo.AddComponent<Image>();
        spacecraftImage.preserveAspect = true;

        RectTransform imgRect  = imgGo.GetComponent<RectTransform>();
        imgRect.anchorMin      = new Vector2(0.05f, 0.18f);
        imgRect.anchorMax      = new Vector2(0.95f, 0.92f);
        imgRect.offsetMin      = Vector2.zero;
        imgRect.offsetMax      = Vector2.zero;

        // ── Step label ───────────────────────────────────────────────────────
        GameObject stepGo = new GameObject("StepLabel");
        stepGo.transform.SetParent(_bodyRoot.transform, false);

        stepLabel           = stepGo.AddComponent<Text>();
        stepLabel.text      = "Step 1 / 6";
        stepLabel.font      = builtinFont;
        stepLabel.fontSize  = PhaseCUITheme.GetAssemblyStepFont();
        stepLabel.color     = new Color(0.85f, 0.85f, 0.85f, 1f);
        stepLabel.alignment = TextAnchor.MiddleCenter;
        stepLabel.raycastTarget = false;

        RectTransform stepRect    = stepGo.GetComponent<RectTransform>();
        stepRect.anchorMin        = new Vector2(0f, 0f);
        stepRect.anchorMax        = new Vector2(1f, 0f);
        stepRect.pivot            = new Vector2(0.5f, 0f);
        stepRect.anchoredPosition = new Vector2(0f, 5f);
        stepRect.sizeDelta        = new Vector2(0f, 22f);
    }

    /// <summary>
    /// Refresh the panel dimensions, fonts and canvas match setting so the
    /// HUD reshapes itself when the screen is resized or the device rotates.
    /// </summary>
    private void ApplyResponsiveLayout()
    {
        if (_canvasScaler != null)
            _canvasScaler.matchWidthOrHeight = PhaseCUITheme.CanvasMatchWidthOrHeight;

        float panelH    = PhaseCUITheme.GetBottomPanelHeight();
        float titleBarH = PhaseCUITheme.GetAssemblyTitleBarHeight();
        float displayH  = _isMinimized ? titleBarH : panelH;

        if (_panelRect != null)
        {
            _panelRect.anchorMin = new Vector2(0f, 0f);
            _panelRect.anchorMax = new Vector2(PhaseCUITheme.GetLeftBottomPanelAnchorMaxX(), 0f);
            _panelRect.offsetMin = new Vector2(EdgeMargin, EdgeMargin);
            _panelRect.offsetMax = new Vector2(-6f, displayH + EdgeMargin);
        }

        if (_bodyRoot != null)
        {
            RectTransform bodyRect = _bodyRoot.GetComponent<RectTransform>();
            if (bodyRect != null)
                bodyRect.offsetMax = new Vector2(0f, -(titleBarH + 4f));
        }

        if (_panelRect != null)
        {
            Transform titleRow = _panelRect.Find("TitleRow");
            if (titleRow != null)
            {
                RectTransform titleRowRect = titleRow as RectTransform;
                if (titleRowRect != null)
                    titleRowRect.sizeDelta = new Vector2(0f, titleBarH);
            }
        }

        if (_titleText   != null) _titleText.fontSize   = PhaseCUITheme.GetAssemblyTitleFont();
        if (_toggleLabel != null) _toggleLabel.fontSize = PhaseCUITheme.GetAssemblyTitleFont();
        if (stepLabel    != null) stepLabel.fontSize    = PhaseCUITheme.GetAssemblyStepFont();
    }

    private void AddToggleButton(Transform parent, Font font)
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
        _toggleLabel = labelGo.AddComponent<Text>();
        _toggleLabel.text = "-";
        _toggleLabel.font = font;
        _toggleLabel.fontSize = PhaseCUITheme.GetAssemblyTitleFont();
        _toggleLabel.fontStyle = FontStyle.Bold;
        _toggleLabel.color = PhaseCUITheme.AccentCyan;
        _toggleLabel.alignment = TextAnchor.MiddleCenter;
        _toggleLabel.raycastTarget = false;
        RectTransform labelRect = labelGo.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;
    }

    // ─── Sprite helpers ──────────────────────────────────────────────────────

    private void SetImageImmediate(int stateIndex)
    {
        currentState = stateIndex;
        if (spacecraftImage == null || library == null) return;
        spacecraftImage.sprite = library.GetSpriteForStep(stateIndex);
        spacecraftImage.color  = Color.white;
    }

    private void UpdateStepLabel(int stepNumber, int stepCount)
    {
        if (stepLabel != null)
            stepLabel.text = $"Step {Mathf.Min(stepNumber, stepCount)} / {stepCount}";
    }

    private void TransitionTo(int targetState)
    {
        if (transitionCoroutine != null) StopCoroutine(transitionCoroutine);
        transitionCoroutine = StartCoroutine(FadeAndSwap(targetState));
    }

    private IEnumerator FadeAndSwap(int targetState)
    {
        float half = FadeDuration * 0.5f;
        RectTransform imgRT = spacecraftImage != null
            ? spacecraftImage.GetComponent<RectTransform>() : null;

        // Fade out
        for (float t = 0f; t < half; t += Time.deltaTime)
        {
            if (spacecraftImage != null)
                spacecraftImage.color = Color.Lerp(Color.white, Color.clear, t / half);
            yield return null;
        }

        // Swap sprite
        currentState = targetState;
        if (spacecraftImage != null && library != null)
        {
            spacecraftImage.sprite = library.GetSpriteForStep(targetState);
            spacecraftImage.color  = Color.clear;
        }

        // Fade in + scale bounce
        for (float t = 0f; t < half; t += Time.deltaTime)
        {
            float p = t / half;
            if (spacecraftImage != null)
                spacecraftImage.color = Color.Lerp(Color.clear, Color.white, p);
            if (imgRT != null)
                imgRT.localScale = Vector3.one * Mathf.Lerp(BounceScale, 1f, p);
            yield return null;
        }

        if (spacecraftImage != null) spacecraftImage.color = Color.white;
        if (imgRT != null)           imgRT.localScale      = Vector3.one;
        transitionCoroutine = null;
    }
}
