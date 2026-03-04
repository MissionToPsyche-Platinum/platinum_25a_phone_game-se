using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Plays full-screen cinematic cutscenes when major assembly milestones are reached:
/// Bus Integration Complete (step 3→4), Design Review Passed (step 4→5),
/// and Phase C Complete (final step). Auto-bootstraps via RuntimeInitializeOnLoadMethod.
/// </summary>
public class AssemblyCutsceneController : MonoBehaviour
{
    public static AssemblyCutsceneController Instance { get; private set; }

    private const string TargetSceneName = "MinigameC";
    private const string LibraryPath = "MinigameC/SpacecraftSpriteLibrary";
    private const int CanvasSortOrder = 200;

    // Animation durations (in unscaled seconds)
    private const float OverlayFadeIn = 0.5f;
    private const float SpacecraftFadeIn = 0.4f;
    private const float PartAnimDuration = 0.6f;
    private const float TitleFadeIn = 0.4f;
    private const float SubtitleFadeIn = 0.3f;
    private const float FadeOut = 0.4f;
    private const float PartStaggerDelay = 0.12f;

    // Sparkle settings
    private const int SparklePoolSize = 12;
    private const float SparkleCycleDuration = 0.8f;
    private const float SparkleSpawnInterval = 0.15f;
    private const float SparkleRadius = 220f;

    private SpacecraftSpriteLibrary spriteLibrary;
    private GameObject cutsceneRoot;
    private CanvasGroup overlayGroup;
    private Image darkOverlay;
    private Image spacecraftImage;
    private TMP_Text titleText;
    private TMP_Text subtitleText;
    private TMP_Text skipPromptText;
    private readonly List<Image> partImages = new List<Image>();
    private readonly List<Image> sparklePool = new List<Image>();

    private Coroutine activeCutscene;
    private bool cutsceneActive;
    private bool skipRequested;
    private bool phaseCCompleteTriggered;

    // Milestone definitions: (title, subtitle, spacecraftSpriteStep, partNames, completedStep, totalSteps)
    private static readonly (string title, string subtitle, int spriteStep, string[] parts, int completedStep, int totalSteps)[] Milestones =
    {
        (
            "BUS INTEGRATION COMPLETE",
            "Spacecraft bus structure, solar arrays, and power systems are fully assembled.",
            3,
            new[] { "bus_frame", "solar_panel_left", "solar_panel_right", "battery_pack" },
            3, 6
        ),
        (
            "DESIGN REVIEW PASSED",
            "Critical Design Review confirms all instruments and communications meet requirements.",
            4,
            new[] { "magnetometer", "imager", "spectrometer", "radio_antenna", "laser_module" },
            4, 6
        ),
        (
            "PHASE C COMPLETE",
            "All subsystems verified. NASA approves proceeding to the next mission phase.",
            6,
            new[] { "propulsion" },
            6, 6
        )
    };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Boot()
    {
        SceneManager.sceneLoaded += (scene, _) =>
        {
            if (scene.name != TargetSceneName) return;
            if (FindFirstObjectByType<AssemblyCutsceneController>() == null)
                new GameObject("AssemblyCutsceneController").AddComponent<AssemblyCutsceneController>();
        };
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (SceneManager.GetActiveScene().name != TargetSceneName)
        {
            Destroy(gameObject);
            return;
        }

        spriteLibrary = Resources.Load<SpacecraftSpriteLibrary>(LibraryPath);

        StartCoroutine(SubscribeWhenReady());
    }

    private IEnumerator SubscribeWhenReady()
    {
        // Wait until PhaseCAssemblyController is available
        while (PhaseCAssemblyController.Instance == null)
            yield return null;

        // Wait an extra frame so we don't catch the initial NotifyStepChanged from Start()
        yield return null;

        PhaseCAssemblyController.Instance.StepChanged += OnStepChanged;
        PhaseCAssemblyController.Instance.PhaseCComplete += OnPhaseCComplete;
    }

    private void OnDestroy()
    {
        if (PhaseCAssemblyController.Instance != null)
        {
            PhaseCAssemblyController.Instance.StepChanged -= OnStepChanged;
            PhaseCAssemblyController.Instance.PhaseCComplete -= OnPhaseCComplete;
        }
        if (Instance == this) Instance = null;
    }

    private void Update()
    {
        if (cutsceneActive && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.E)))
            skipRequested = true;
    }

    private void OnStepChanged(PhaseCAssemblyController.StepInfo info)
    {
        // Step 3 just completed → StepNumber transitions to 4
        if (info.StepNumber == 4 && !cutsceneActive)
            TriggerCutscene(0);
        // Step 4 just completed → StepNumber transitions to 5
        else if (info.StepNumber == 5 && !cutsceneActive)
            TriggerCutscene(1);
    }

    private void OnPhaseCComplete()
    {
        if (!cutsceneActive && !phaseCCompleteTriggered)
        {
            phaseCCompleteTriggered = true;
            TriggerCutscene(2);
        }
    }

    private void TriggerCutscene(int milestoneIndex)
    {
        if (cutsceneActive || milestoneIndex < 0 || milestoneIndex >= Milestones.Length) return;
        activeCutscene = StartCoroutine(PlayCutscene(milestoneIndex));
    }

    private IEnumerator PlayCutscene(int milestoneIndex)
    {
        cutsceneActive = true;
        skipRequested = false;
        var milestone = Milestones[milestoneIndex];

        PauseController.SetPause(true);
        BuildCutsceneUI(milestone);

        // -- Phase 1: Fade in overlay --
        yield return FadeCanvasGroup(overlayGroup, 0f, 1f, OverlayFadeIn);

        // -- Phase 2: Fade in spacecraft --
        spacecraftImage.sprite = spriteLibrary != null ? spriteLibrary.GetSpriteForStep(milestone.spriteStep) : null;
        yield return FadeImage(spacecraftImage, 0f, 1f, SpacecraftFadeIn);

        // -- Phase 3: Animate parts in from edges --
        yield return AnimatePartsIn(milestone.parts);

        // -- Phase 4: Fade in title --
        yield return FadeTMPText(titleText, 0f, 1f, TitleFadeIn);

        // -- Phase 5: Fade in subtitle --
        yield return FadeTMPText(subtitleText, 0f, 1f, SubtitleFadeIn);

        // -- Phase 6: Start sparkles and show skip prompt --
        Coroutine sparkleRoutine = StartCoroutine(SparkleLoop());
        skipPromptText.gameObject.SetActive(true);
        StartCoroutine(PulsePrompt());

        // -- Phase 7: Wait for dismiss --
        skipRequested = false; // Reset after skip-to-end, now wait for dismiss
        while (!skipRequested)
            yield return null;

        // Stop sparkles
        if (sparkleRoutine != null) StopCoroutine(sparkleRoutine);

        // -- Phase 8: Fade out --
        yield return FadeCanvasGroup(overlayGroup, 1f, 0f, FadeOut);

        // Cleanup
        PauseController.SetPause(false);
        DestroyCutsceneUI();
        cutsceneActive = false;
        activeCutscene = null;
    }

    // ===================== UI CONSTRUCTION =====================

    private void BuildCutsceneUI((string title, string subtitle, int spriteStep, string[] parts, int completedStep, int totalSteps) milestone)
    {
        // Root canvas
        cutsceneRoot = new GameObject("AssemblyCutsceneCanvas");
        cutsceneRoot.transform.SetParent(transform);

        Canvas canvas = cutsceneRoot.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = CanvasSortOrder;

        CanvasScaler scaler = cutsceneRoot.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(PhaseCUITheme.RefWidth, PhaseCUITheme.RefHeight);
        scaler.matchWidthOrHeight = 0.5f;

        cutsceneRoot.AddComponent<GraphicRaycaster>();

        // Canvas group for master fade
        overlayGroup = cutsceneRoot.AddComponent<CanvasGroup>();
        overlayGroup.alpha = 0f;

        // Dark overlay background
        GameObject bgGo = CreateChild(cutsceneRoot.transform, "DarkOverlay");
        darkOverlay = bgGo.AddComponent<Image>();
        darkOverlay.color = PhaseCUITheme.OverlayDark;
        StretchFill(bgGo);

        // ---- Cinematic letterbox bars ----
        Color letterboxColor = new Color(0f, 0f, 0f, 0.85f);
        const float letterboxHeight = 80f;

        GameObject topBar = CreateChild(cutsceneRoot.transform, "LetterboxTop");
        Image topBarImg = topBar.AddComponent<Image>();
        topBarImg.color = letterboxColor;
        RectTransform topBarRect = topBar.GetComponent<RectTransform>();
        topBarRect.anchorMin = new Vector2(0f, 1f);
        topBarRect.anchorMax = new Vector2(1f, 1f);
        topBarRect.pivot = new Vector2(0.5f, 1f);
        topBarRect.anchoredPosition = Vector2.zero;
        topBarRect.sizeDelta = new Vector2(0f, letterboxHeight);

        GameObject bottomBar = CreateChild(cutsceneRoot.transform, "LetterboxBottom");
        Image bottomBarImg = bottomBar.AddComponent<Image>();
        bottomBarImg.color = letterboxColor;
        RectTransform bottomBarRect = bottomBar.GetComponent<RectTransform>();
        bottomBarRect.anchorMin = new Vector2(0f, 0f);
        bottomBarRect.anchorMax = new Vector2(1f, 0f);
        bottomBarRect.pivot = new Vector2(0.5f, 0f);
        bottomBarRect.anchoredPosition = Vector2.zero;
        bottomBarRect.sizeDelta = new Vector2(0f, letterboxHeight);

        // ---- Top banner info area (milestone badge + accent line + title + subtitle) ----

        // Milestone badge: "STEP 3 OF 6 COMPLETE"
        GameObject badgeGo = CreateChild(cutsceneRoot.transform, "MilestoneBadge");
        TMP_Text badgeText = badgeGo.AddComponent<TextMeshProUGUI>();
        badgeText.text = milestone.completedStep < milestone.totalSteps
            ? $"STEP {milestone.completedStep} OF {milestone.totalSteps} COMPLETE"
            : "MISSION MILESTONE";
        badgeText.fontSize = PhaseCUITheme.FontSizeBadge;
        badgeText.fontStyle = FontStyles.SmallCaps;
        badgeText.alignment = TextAlignmentOptions.Center;
        badgeText.color = PhaseCUITheme.AccentCyanMuted;
        RectTransform badgeRect = badgeGo.GetComponent<RectTransform>();
        badgeRect.anchorMin = new Vector2(0.5f, 1f);
        badgeRect.anchorMax = new Vector2(0.5f, 1f);
        badgeRect.pivot = new Vector2(0.5f, 1f);
        badgeRect.anchoredPosition = new Vector2(0f, -(letterboxHeight + 20f));
        badgeRect.sizeDelta = new Vector2(500f, 24f);

        // Gold accent line below badge
        GameObject accentLine = CreateChild(cutsceneRoot.transform, "AccentLine");
        Image accentImg = accentLine.AddComponent<Image>();
        accentImg.color = PhaseCUITheme.AccentGold;
        RectTransform accentRect = accentLine.GetComponent<RectTransform>();
        accentRect.anchorMin = new Vector2(0.5f, 1f);
        accentRect.anchorMax = new Vector2(0.5f, 1f);
        accentRect.pivot = new Vector2(0.5f, 0.5f);
        accentRect.anchoredPosition = new Vector2(0f, -(letterboxHeight + 50f));
        accentRect.sizeDelta = new Vector2(400f, 3f);

        // Title text: large gold, top-center below accent line
        GameObject titleGo = CreateChild(cutsceneRoot.transform, "TitleText");
        titleText = titleGo.AddComponent<TextMeshProUGUI>();
        titleText.text = milestone.title;
        titleText.fontSize = 44f;
        titleText.fontStyle = FontStyles.Bold;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = SetAlpha(PhaseCUITheme.AccentGold, 0f);
        titleText.enableWordWrapping = true;
        RectTransform titleRect = titleGo.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0f, -(letterboxHeight + 62f));
        titleRect.sizeDelta = new Vector2(900f, 56f);

        // Subtitle text: description below title
        GameObject subGo = CreateChild(cutsceneRoot.transform, "SubtitleText");
        subtitleText = subGo.AddComponent<TextMeshProUGUI>();
        subtitleText.text = milestone.subtitle;
        subtitleText.fontSize = PhaseCUITheme.FontSizeBodySmall;
        subtitleText.alignment = TextAlignmentOptions.Center;
        subtitleText.color = SetAlpha(PhaseCUITheme.TextSecondary, 0f);
        subtitleText.enableWordWrapping = true;
        RectTransform subRect = subGo.GetComponent<RectTransform>();
        subRect.anchorMin = new Vector2(0.5f, 1f);
        subRect.anchorMax = new Vector2(0.5f, 1f);
        subRect.pivot = new Vector2(0.5f, 1f);
        subRect.anchoredPosition = new Vector2(0f, -(letterboxHeight + 122f));
        subRect.sizeDelta = new Vector2(750f, 60f);

        // Second accent line below subtitle (divider before spacecraft area)
        GameObject dividerLine = CreateChild(cutsceneRoot.transform, "DividerLine");
        Image dividerImg = dividerLine.AddComponent<Image>();
        dividerImg.color = new Color(PhaseCUITheme.AccentGold.r, PhaseCUITheme.AccentGold.g, PhaseCUITheme.AccentGold.b, 0.4f);
        RectTransform dividerRect = dividerLine.GetComponent<RectTransform>();
        dividerRect.anchorMin = new Vector2(0.5f, 1f);
        dividerRect.anchorMax = new Vector2(0.5f, 1f);
        dividerRect.pivot = new Vector2(0.5f, 0.5f);
        dividerRect.anchoredPosition = new Vector2(0f, -(letterboxHeight + 192f));
        dividerRect.sizeDelta = new Vector2(300f, 2f);

        // ---- Spacecraft image (center-lower area, 420x420) ----
        GameObject scGo = CreateChild(cutsceneRoot.transform, "SpacecraftImage");
        spacecraftImage = scGo.AddComponent<Image>();
        spacecraftImage.preserveAspect = true;
        spacecraftImage.color = new Color(1f, 1f, 1f, 0f); // start invisible
        RectTransform scRect = scGo.GetComponent<RectTransform>();
        scRect.anchorMin = new Vector2(0.5f, 0.5f);
        scRect.anchorMax = new Vector2(0.5f, 0.5f);
        scRect.pivot = new Vector2(0.5f, 0.5f);
        scRect.anchoredPosition = new Vector2(0f, -60f);
        scRect.sizeDelta = new Vector2(420f, 420f);

        // ---- Part images (start invisible, positioned during animation) ----
        partImages.Clear();
        if (spriteLibrary != null)
        {
            foreach (string partName in milestone.parts)
            {
                Sprite partSprite = spriteLibrary.GetPartSprite(partName);
                if (partSprite == null) continue;

                GameObject partGo = CreateChild(cutsceneRoot.transform, "Part_" + partName);
                Image partImg = partGo.AddComponent<Image>();
                partImg.sprite = partSprite;
                partImg.preserveAspect = true;
                partImg.color = new Color(1f, 1f, 1f, 0f);
                RectTransform partRect = partGo.GetComponent<RectTransform>();
                partRect.anchorMin = new Vector2(0.5f, 0.5f);
                partRect.anchorMax = new Vector2(0.5f, 0.5f);
                partRect.pivot = new Vector2(0.5f, 0.5f);
                partRect.sizeDelta = new Vector2(120f, 120f);
                partRect.localScale = Vector3.zero;
                partImages.Add(partImg);
            }
        }

        // ---- Skip prompt (bottom center, above letterbox, starts hidden) ----
        GameObject promptGo = CreateChild(cutsceneRoot.transform, "SkipPrompt");
        skipPromptText = promptGo.AddComponent<TextMeshProUGUI>();
        skipPromptText.text = "Press Space / E to continue";
        skipPromptText.fontSize = PhaseCUITheme.FontSizeBodySmall;
        skipPromptText.alignment = TextAlignmentOptions.Center;
        skipPromptText.color = PhaseCUITheme.TextSecondary;
        skipPromptText.fontStyle = FontStyles.Italic;
        RectTransform promptRect = promptGo.GetComponent<RectTransform>();
        promptRect.anchorMin = new Vector2(0.5f, 0f);
        promptRect.anchorMax = new Vector2(0.5f, 0f);
        promptRect.pivot = new Vector2(0.5f, 0f);
        promptRect.anchoredPosition = new Vector2(0f, letterboxHeight + 12f);
        promptRect.sizeDelta = new Vector2(500f, 36f);
        promptGo.SetActive(false);

        // ---- Sparkle pool ----
        sparklePool.Clear();
        for (int i = 0; i < SparklePoolSize; i++)
        {
            GameObject sparkGo = CreateChild(cutsceneRoot.transform, "Sparkle_" + i);
            Image sparkImg = sparkGo.AddComponent<Image>();
            sparkImg.color = new Color(1f, 1f, 1f, 0f);
            RectTransform sparkRect = sparkGo.GetComponent<RectTransform>();
            sparkRect.anchorMin = new Vector2(0.5f, 0.5f);
            sparkRect.anchorMax = new Vector2(0.5f, 0.5f);
            sparkRect.pivot = new Vector2(0.5f, 0.5f);
            sparkRect.sizeDelta = new Vector2(16f, 16f);
            sparkRect.localScale = Vector3.zero;
            sparkGo.SetActive(false);
            sparklePool.Add(sparkImg);
        }
    }

    private void DestroyCutsceneUI()
    {
        partImages.Clear();
        sparklePool.Clear();
        if (cutsceneRoot != null) Destroy(cutsceneRoot);
        cutsceneRoot = null;
        overlayGroup = null;
        darkOverlay = null;
        spacecraftImage = null;
        titleText = null;
        subtitleText = null;
        skipPromptText = null;
    }

    // ===================== ANIMATION COROUTINES =====================

    private IEnumerator FadeCanvasGroup(CanvasGroup group, float from, float to, float duration)
    {
        if (skipRequested)
        {
            group.alpha = to;
            yield break;
        }

        float elapsed = 0f;
        group.alpha = from;
        while (elapsed < duration)
        {
            if (skipRequested)
            {
                group.alpha = to;
                yield break;
            }
            elapsed += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }
        group.alpha = to;
    }

    private IEnumerator FadeImage(Image img, float fromAlpha, float toAlpha, float duration)
    {
        if (skipRequested)
        {
            img.color = SetAlpha(img.color, toAlpha);
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (skipRequested)
            {
                img.color = SetAlpha(img.color, toAlpha);
                yield break;
            }
            elapsed += Time.unscaledDeltaTime;
            float a = Mathf.Lerp(fromAlpha, toAlpha, Mathf.Clamp01(elapsed / duration));
            img.color = SetAlpha(img.color, a);
            yield return null;
        }
        img.color = SetAlpha(img.color, toAlpha);
    }

    private IEnumerator FadeTMPText(TMP_Text text, float fromAlpha, float toAlpha, float duration)
    {
        if (skipRequested)
        {
            text.color = SetAlpha(text.color, toAlpha);
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (skipRequested)
            {
                text.color = SetAlpha(text.color, toAlpha);
                yield break;
            }
            elapsed += Time.unscaledDeltaTime;
            float a = Mathf.Lerp(fromAlpha, toAlpha, Mathf.Clamp01(elapsed / duration));
            text.color = SetAlpha(text.color, a);
            yield return null;
        }
        text.color = SetAlpha(text.color, toAlpha);
    }

    private IEnumerator AnimatePartsIn(string[] partNames)
    {
        if (partImages.Count == 0) yield break;

        // Spacecraft is at y=-60 from center; parts orbit around that
        const float spacecraftOffsetY = -60f;

        // Calculate target positions arranged around the spacecraft
        float angleStep = 360f / partImages.Count;
        float radius = 200f;
        Vector2[] targets = new Vector2[partImages.Count];
        for (int i = 0; i < partImages.Count; i++)
        {
            float angle = (90f + angleStep * i) * Mathf.Deg2Rad;
            targets[i] = new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius + spacecraftOffsetY);
        }

        if (skipRequested)
        {
            // Snap all parts to final state
            for (int i = 0; i < partImages.Count; i++)
            {
                partImages[i].color = SetAlpha(partImages[i].color, 1f);
                partImages[i].rectTransform.anchoredPosition = targets[i];
                partImages[i].rectTransform.localScale = Vector3.one;
            }
            yield break;
        }

        // Stagger animate each part
        float totalDuration = PartAnimDuration + PartStaggerDelay * (partImages.Count - 1);
        float elapsed = 0f;

        // Set starting positions (from edges)
        Vector2[] startPositions = new Vector2[partImages.Count];
        for (int i = 0; i < partImages.Count; i++)
        {
            float angle = (90f + angleStep * i) * Mathf.Deg2Rad;
            startPositions[i] = new Vector2(Mathf.Cos(angle) * 600f, Mathf.Sin(angle) * 600f + spacecraftOffsetY);
            partImages[i].rectTransform.anchoredPosition = startPositions[i];
        }

        while (elapsed < totalDuration)
        {
            if (skipRequested)
            {
                for (int i = 0; i < partImages.Count; i++)
                {
                    partImages[i].color = SetAlpha(partImages[i].color, 1f);
                    partImages[i].rectTransform.anchoredPosition = targets[i];
                    partImages[i].rectTransform.localScale = Vector3.one;
                }
                yield break;
            }

            elapsed += Time.unscaledDeltaTime;

            for (int i = 0; i < partImages.Count; i++)
            {
                float partStart = i * PartStaggerDelay;
                float partElapsed = elapsed - partStart;
                if (partElapsed < 0f) continue;

                float t = Mathf.Clamp01(partElapsed / PartAnimDuration);
                // Ease out with slight overshoot for bounce feel
                float eased = 1f - Mathf.Pow(1f - t, 3f);
                float scale = t < 0.7f
                    ? Mathf.Lerp(0f, 1.15f, t / 0.7f)
                    : Mathf.Lerp(1.15f, 1f, (t - 0.7f) / 0.3f);

                partImages[i].color = SetAlpha(partImages[i].color, eased);
                partImages[i].rectTransform.anchoredPosition = Vector2.Lerp(startPositions[i], targets[i], eased);
                partImages[i].rectTransform.localScale = Vector3.one * scale;
            }

            yield return null;
        }

        // Ensure final state
        for (int i = 0; i < partImages.Count; i++)
        {
            partImages[i].color = SetAlpha(partImages[i].color, 1f);
            partImages[i].rectTransform.anchoredPosition = targets[i];
            partImages[i].rectTransform.localScale = Vector3.one;
        }
    }

    private IEnumerator SparkleLoop()
    {
        int sparkleIndex = 0;
        float timer = 0f;
        while (true)
        {
            timer += Time.unscaledDeltaTime;
            if (timer >= SparkleSpawnInterval)
            {
                timer -= SparkleSpawnInterval;
                StartCoroutine(AnimateSingleSparkle(sparklePool[sparkleIndex]));
                sparkleIndex = (sparkleIndex + 1) % SparklePoolSize;
            }
            yield return null;
        }
    }

    private IEnumerator AnimateSingleSparkle(Image sparkle)
    {
        sparkle.gameObject.SetActive(true);

        // Random position around spacecraft center (spacecraft sits at y=-60)
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float dist = Random.Range(80f, SparkleRadius);
        RectTransform rt = sparkle.rectTransform;
        rt.anchoredPosition = new Vector2(Mathf.Cos(angle) * dist, Mathf.Sin(angle) * dist - 60f);
        rt.localRotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));

        // Random color: gold, cyan, or white
        Color[] sparkleColors = { PhaseCUITheme.AccentGold, PhaseCUITheme.AccentCyan, Color.white };
        Color baseColor = sparkleColors[Random.Range(0, sparkleColors.Length)];

        float elapsed = 0f;
        while (elapsed < SparkleCycleDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / SparkleCycleDuration);

            // Scale: 0 → 1 → 0
            float scale = t < 0.5f
                ? Mathf.Lerp(0f, 1f, t * 2f)
                : Mathf.Lerp(1f, 0f, (t - 0.5f) * 2f);
            rt.localScale = Vector3.one * scale;

            // Alpha: matches scale
            sparkle.color = SetAlpha(baseColor, scale);

            // Slow rotation
            rt.Rotate(0f, 0f, Time.unscaledDeltaTime * 90f);

            yield return null;
        }

        rt.localScale = Vector3.zero;
        sparkle.color = SetAlpha(baseColor, 0f);
        sparkle.gameObject.SetActive(false);
    }

    private IEnumerator PulsePrompt()
    {
        float pulseSpeed = 2f;
        float elapsed = 0f;
        while (cutsceneActive && skipPromptText != null)
        {
            elapsed += Time.unscaledDeltaTime;
            float alpha = 0.5f + 0.5f * Mathf.Sin(elapsed * pulseSpeed * Mathf.PI);
            skipPromptText.color = SetAlpha(PhaseCUITheme.TextSecondary, alpha);
            yield return null;
        }
    }

    // ===================== HELPERS =====================

    private static GameObject CreateChild(Transform parent, string name)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        return go;
    }

    private static void StretchFill(GameObject go)
    {
        RectTransform rt = go.GetComponent<RectTransform>();
        if (rt == null) rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static Color SetAlpha(Color c, float a)
    {
        return new Color(c.r, c.g, c.b, a);
    }
}
