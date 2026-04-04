using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Permanent HUD panel (top-right corner) that shows the spacecraft being built
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

    // HUD panel dimensions — responsive values pulled from PhaseCUITheme at runtime.
    private const float EdgeMargin  = 14f;

    private const float FadeDuration = 0.5f;
    private const float BounceScale  = 1.12f;

    private SpacecraftSpriteLibrary library;
    private Image                   spacecraftImage;
    private Text                    stepLabel;
    private int                     currentState;
    private Coroutine               transitionCoroutine;

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

    // ─── HUD construction ────────────────────────────────────────────────────

    private void BuildHUD()
    {
        Font builtinFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // ── Canvas ──────────────────────────────────────────────────────────
        GameObject canvasGo = new GameObject("SpacecraftHUDCanvas");
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight  = PhaseCUITheme.CanvasMatchWidthOrHeight;
        canvasGo.AddComponent<GraphicRaycaster>();

        // ── Background panel (bottom-left, above hint strip) ─────────────────
        float panelW      = PhaseCUITheme.GetAssemblyPanelWidth();
        float panelH      = PhaseCUITheme.GetAssemblyPanelHeight();
        float bottomStart = PhaseCUITheme.GetHintStripHeight() + EdgeMargin;

        GameObject panelGo = new GameObject("SpacecraftPanel");
        panelGo.transform.SetParent(canvasGo.transform, false);

        Image panelBg = panelGo.AddComponent<Image>();
        panelBg.color = new Color(0.05f, 0.08f, 0.18f, 0.88f);

        RectTransform panelRect = panelGo.GetComponent<RectTransform>();
        panelRect.anchorMin        = new Vector2(0f, 0f);
        panelRect.anchorMax        = new Vector2(0f, 0f);
        panelRect.pivot            = new Vector2(0f, 0f);
        panelRect.anchoredPosition = new Vector2(EdgeMargin, bottomStart);
        panelRect.sizeDelta        = new Vector2(panelW, panelH);

        // ── Title ────────────────────────────────────────────────────────────
        GameObject titleGo = new GameObject("Title");
        titleGo.transform.SetParent(panelGo.transform, false);

        Text title        = titleGo.AddComponent<Text>();
        title.text        = "ASSEMBLY";
        title.font        = builtinFont;
        title.fontSize    = PhaseCUITheme.GetAssemblyTitleFont();
        title.fontStyle   = FontStyle.Bold;
        title.color       = new Color(0.9f, 0.85f, 0.4f);
        title.alignment   = TextAnchor.MiddleCenter;

        RectTransform titleRect    = titleGo.GetComponent<RectTransform>();
        titleRect.anchorMin        = new Vector2(0f, 1f);
        titleRect.anchorMax        = new Vector2(1f, 1f);
        titleRect.pivot            = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0f, -5f);
        titleRect.sizeDelta        = new Vector2(0f, 20f);

        // ── Spacecraft image ─────────────────────────────────────────────────
        GameObject imgGo = new GameObject("SpacecraftImage");
        imgGo.transform.SetParent(panelGo.transform, false);

        spacecraftImage                 = imgGo.AddComponent<Image>();
        spacecraftImage.preserveAspect = true;

        RectTransform imgRect  = imgGo.GetComponent<RectTransform>();
        imgRect.anchorMin      = new Vector2(0.05f, 0.18f);
        imgRect.anchorMax      = new Vector2(0.95f, 0.88f);
        imgRect.offsetMin      = Vector2.zero;
        imgRect.offsetMax      = Vector2.zero;

        // ── Step label ───────────────────────────────────────────────────────
        GameObject stepGo = new GameObject("StepLabel");
        stepGo.transform.SetParent(panelGo.transform, false);

        stepLabel           = stepGo.AddComponent<Text>();
        stepLabel.text      = "Step 1 / 6";
        stepLabel.font      = builtinFont;
        stepLabel.fontSize  = PhaseCUITheme.GetAssemblyStepFont();
        stepLabel.color     = new Color(0.85f, 0.85f, 0.85f, 1f);
        stepLabel.alignment = TextAnchor.MiddleCenter;

        RectTransform stepRect    = stepGo.GetComponent<RectTransform>();
        stepRect.anchorMin        = new Vector2(0f, 0f);
        stepRect.anchorMax        = new Vector2(1f, 0f);
        stepRect.pivot            = new Vector2(0.5f, 0f);
        stepRect.anchoredPosition = new Vector2(0f, 5f);
        stepRect.sizeDelta        = new Vector2(0f, 18f);
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
