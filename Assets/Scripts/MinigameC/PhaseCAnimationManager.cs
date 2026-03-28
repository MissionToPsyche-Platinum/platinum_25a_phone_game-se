using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages transitions and animations for MinigameC:
///   - Item pickup: colored ring ripple expands from player position
///   - NPC dialogue: cinematic letterbox bars slide in/out
///   - Step complete: gold banner slides in from the left
/// Auto-created at scene load. Call static methods from other scripts.
/// </summary>
public class PhaseCAnimationManager : MonoBehaviour
{
    private const string TargetSceneName = "MinigameC";

    private static PhaseCAnimationManager _instance;
    public static PhaseCAnimationManager Instance => _instance;

    private bool _initialized;

    // Canvas
    private Canvas _animCanvas;
    private RectTransform _canvasRect;

    // Ripple pool
    private const int RipplePoolSize = 8;
    private Image[] _rippleImages;
    private CanvasGroup[] _rippleCGs;
    private Coroutine[] _rippleCoroutines;
    private int _rippleIndex;

    // Letterbox
    private GameObject _letterboxRoot;
    private CanvasGroup _letterboxCG;
    private RectTransform _topBarRect;
    private RectTransform _bottomBarRect;
    private Coroutine _letterboxCoroutine;
    private float _barHeight;

    // Step banner
    private GameObject _bannerPanel;
    private CanvasGroup _bannerCG;
    private RectTransform _bannerRect;
    private Text _bannerTitle;
    private Text _bannerSubtitle;
    private Coroutine _bannerCoroutine;
    private float _bannerWidth;

    // ---- Bootstrap ----

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Boot()
    {
        SceneManager.sceneLoaded += (scene, _) =>
        {
            if (scene.name != TargetSceneName) return;
            if (FindFirstObjectByType<PhaseCAnimationManager>() != null) return;
            new GameObject("PhaseCAnimationManager").AddComponent<PhaseCAnimationManager>();
        };
    }

    private void Awake()
    {
        if (SceneManager.GetActiveScene().name != TargetSceneName)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
    }

    private void OnDestroy()
    {
        if (_instance == this) _instance = null;
        UnsubscribeFromEvents();
    }

    private void Start()
    {
        BuildCanvas();
        _initialized = true;
        StartCoroutine(SubscribeWhenReady());
    }

    private IEnumerator SubscribeWhenReady()
    {
        yield return null; // Wait one frame for other singletons
        PhaseCAssemblyController ctrl = PhaseCAssemblyController.Instance;
        if (ctrl != null)
            ctrl.StepChanged += OnStepChanged;
    }

    private void UnsubscribeFromEvents()
    {
        PhaseCAssemblyController ctrl = PhaseCAssemblyController.Instance;
        if (ctrl != null)
            ctrl.StepChanged -= OnStepChanged;
    }

    // ---- Public API ----

    /// <summary>Spawns a colored ring ripple at the player's world position.</summary>
    public static void TriggerItemPickup(Vector3 worldPos, Color color)
    {
        if (_instance == null || !_instance._initialized) return;
        _instance.StartRipple(worldPos, color);
    }

    /// <summary>Slides cinematic letterbox bars in from the screen edges.</summary>
    public static void TriggerDialogueStart()
    {
        if (_instance == null || !_instance._initialized) return;
        if (_instance._letterboxCoroutine != null)
            _instance.StopCoroutine(_instance._letterboxCoroutine);
        _instance._letterboxCoroutine = _instance.StartCoroutine(_instance.SlideLetterboxIn());
    }

    /// <summary>Slides cinematic letterbox bars back off the screen edges.</summary>
    public static void TriggerDialogueEnd()
    {
        if (_instance == null || !_instance._initialized) return;
        if (_instance._letterboxCoroutine != null)
            _instance.StopCoroutine(_instance._letterboxCoroutine);
        _instance._letterboxCoroutine = _instance.StartCoroutine(_instance.SlideLetterboxOut());
    }

    // ---- Event handlers ----

    private void OnStepChanged(PhaseCAssemblyController.StepInfo info)
    {
        // Only show banner for steps 1 and 2 completing (steps 3-5 have full cutscenes)
        if (info.StepNumber != 2 && info.StepNumber != 3) return;
        if (_bannerCoroutine != null) StopCoroutine(_bannerCoroutine);
        _bannerCoroutine = StartCoroutine(ShowStepBanner(info));
    }

    // ---- Ripple ----

    private void StartRipple(Vector3 worldPos, Color color)
    {
        int slot = _rippleIndex;
        _rippleIndex = (_rippleIndex + 1) % RipplePoolSize;

        if (_rippleCoroutines[slot] != null)
        {
            StopCoroutine(_rippleCoroutines[slot]);
            ResetRippleSlot(slot);
        }
        _rippleCoroutines[slot] = StartCoroutine(PlayRipple(slot, worldPos, color));
    }

    private void ResetRippleSlot(int slot)
    {
        _rippleCGs[slot].alpha = 0f;
        _rippleImages[slot].rectTransform.sizeDelta = Vector2.zero;
        _rippleImages[slot].gameObject.SetActive(false);
    }

    private IEnumerator PlayRipple(int slot, Vector3 worldPos, Color color)
    {
        // Convert world position to canvas local position
        Camera cam = Camera.main;
        if (cam == null) yield break;

        Vector3 screenPos = cam.WorldToScreenPoint(worldPos);
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvasRect, screenPos, null, out Vector2 localPos))
            yield break;

        RectTransform rt = _rippleImages[slot].rectTransform;
        rt.anchoredPosition = localPos;
        _rippleImages[slot].color = new Color(color.r, color.g, color.b, 0.85f);
        _rippleCGs[slot].alpha = 1f;
        _rippleImages[slot].gameObject.SetActive(true);

        const float Duration = 0.5f;
        const float StartSize = 20f;
        const float EndSize = 120f;

        for (float t = 0f; t < Duration; t += Time.deltaTime)
        {
            float p = t / Duration;
            float size = Mathf.Lerp(StartSize, EndSize, p);
            float alpha = Mathf.Lerp(0.85f, 0f, p);
            rt.sizeDelta = new Vector2(size, size);
            _rippleImages[slot].color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }

        ResetRippleSlot(slot);
        _rippleCoroutines[slot] = null;
    }

    // ---- Letterbox ----

    private IEnumerator SlideLetterboxIn()
    {
        _letterboxCG.alpha = 1f;
        Vector2 topStart = _topBarRect.anchoredPosition;
        Vector2 bottomStart = _bottomBarRect.anchoredPosition;
        Vector2 topTarget = Vector2.zero;
        Vector2 bottomTarget = Vector2.zero;

        const float Duration = 0.25f;
        for (float t = 0f; t < Duration; t += Time.deltaTime)
        {
            float p = EaseOutCubic(t / Duration);
            _topBarRect.anchoredPosition = Vector2.Lerp(topStart, topTarget, p);
            _bottomBarRect.anchoredPosition = Vector2.Lerp(bottomStart, bottomTarget, p);
            yield return null;
        }
        _topBarRect.anchoredPosition = topTarget;
        _bottomBarRect.anchoredPosition = bottomTarget;
        _letterboxCoroutine = null;
    }

    private IEnumerator SlideLetterboxOut()
    {
        Vector2 topStart = _topBarRect.anchoredPosition;
        Vector2 bottomStart = _bottomBarRect.anchoredPosition;
        Vector2 topTarget = new Vector2(0f, _barHeight);
        Vector2 bottomTarget = new Vector2(0f, -_barHeight);

        const float Duration = 0.2f;
        for (float t = 0f; t < Duration; t += Time.deltaTime)
        {
            float p = EaseInCubic(t / Duration);
            _topBarRect.anchoredPosition = Vector2.Lerp(topStart, topTarget, p);
            _bottomBarRect.anchoredPosition = Vector2.Lerp(bottomStart, bottomTarget, p);
            yield return null;
        }
        _topBarRect.anchoredPosition = topTarget;
        _bottomBarRect.anchoredPosition = bottomTarget;
        _letterboxCG.alpha = 0f;
        _letterboxCoroutine = null;
    }

    // ---- Step banner ----

    private IEnumerator ShowStepBanner(PhaseCAssemblyController.StepInfo info)
    {
        int completedStep = info.StepNumber - 1;
        _bannerTitle.text = "STEP " + completedStep + " COMPLETE";
        _bannerSubtitle.text = info.Title;

        _bannerPanel.SetActive(true);
        float hiddenX = -(_bannerWidth + 20f);
        float visibleX = 20f;
        float exitX = PhaseCUITheme.RefWidth + 20f;

        // Slide in from left
        const float InDuration = 0.35f;
        for (float t = 0f; t < InDuration; t += Time.deltaTime)
        {
            float p = EaseOutCubic(t / InDuration);
            float alphaP = Mathf.Min(1f, p * 3f);
            _bannerCG.alpha = alphaP;
            _bannerRect.anchoredPosition = new Vector2(Mathf.Lerp(hiddenX, visibleX, p), 0f);
            yield return null;
        }
        _bannerCG.alpha = 1f;
        _bannerRect.anchoredPosition = new Vector2(visibleX, 0f);

        yield return new WaitForSeconds(2f);

        // Slide out to right
        const float OutDuration = 0.3f;
        for (float t = 0f; t < OutDuration; t += Time.deltaTime)
        {
            float p = EaseInCubic(t / OutDuration);
            _bannerCG.alpha = 1f - p;
            _bannerRect.anchoredPosition = new Vector2(Mathf.Lerp(visibleX, exitX, p), 0f);
            yield return null;
        }
        _bannerCG.alpha = 0f;
        _bannerPanel.SetActive(false);
        _bannerCoroutine = null;
    }

    // ---- Canvas construction ----

    private void BuildCanvas()
    {
        GameObject canvasGo = new GameObject("PhaseCAnimationCanvas");
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9;

        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(PhaseCUITheme.RefWidth, PhaseCUITheme.RefHeight);
        scaler.matchWidthOrHeight = PhaseCUITheme.CanvasMatchWidthOrHeight;

        canvasGo.AddComponent<GraphicRaycaster>();
        _animCanvas = canvas;
        _canvasRect = canvasGo.GetComponent<RectTransform>();

        BuildRipplePool(canvasGo.transform);
        BuildLetterbox(canvasGo.transform);
        BuildStepBanner(canvasGo.transform);
    }

    private void BuildRipplePool(Transform parent)
    {
        Sprite ringSprite = MakeRingSprite();
        _rippleImages = new Image[RipplePoolSize];
        _rippleCGs = new CanvasGroup[RipplePoolSize];
        _rippleCoroutines = new Coroutine[RipplePoolSize];

        for (int i = 0; i < RipplePoolSize; i++)
        {
            GameObject go = new GameObject("Ripple_" + i);
            go.transform.SetParent(parent, false);

            CanvasGroup cg = go.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            cg.interactable = false;
            cg.blocksRaycasts = false;

            Image img = go.AddComponent<Image>();
            img.sprite = ringSprite;
            img.color = Color.white;
            img.raycastTarget = false;

            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = Vector2.zero;

            go.SetActive(false);
            _rippleImages[i] = img;
            _rippleCGs[i] = cg;
        }
    }

    private void BuildLetterbox(Transform parent)
    {
        _barHeight = PhaseCUITheme.RefHeight * 0.08f; // ~86px at 1080p reference

        _letterboxRoot = new GameObject("LetterboxRoot");
        _letterboxRoot.transform.SetParent(parent, false);

        _letterboxCG = _letterboxRoot.AddComponent<CanvasGroup>();
        _letterboxCG.alpha = 0f;
        _letterboxCG.interactable = false;
        _letterboxCG.blocksRaycasts = false;

        RectTransform rootRect = _letterboxRoot.GetComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        // Top bar
        GameObject topGo = new GameObject("TopBar");
        topGo.transform.SetParent(_letterboxRoot.transform, false);
        Image topImg = topGo.AddComponent<Image>();
        topImg.color = Color.black;
        topImg.raycastTarget = false;
        _topBarRect = topGo.GetComponent<RectTransform>();
        _topBarRect.anchorMin = new Vector2(0f, 1f);
        _topBarRect.anchorMax = new Vector2(1f, 1f);
        _topBarRect.pivot = new Vector2(0.5f, 1f);
        _topBarRect.sizeDelta = new Vector2(0f, _barHeight);
        _topBarRect.anchoredPosition = new Vector2(0f, _barHeight); // off-screen above

        // Bottom bar
        GameObject bottomGo = new GameObject("BottomBar");
        bottomGo.transform.SetParent(_letterboxRoot.transform, false);
        Image bottomImg = bottomGo.AddComponent<Image>();
        bottomImg.color = Color.black;
        bottomImg.raycastTarget = false;
        _bottomBarRect = bottomGo.GetComponent<RectTransform>();
        _bottomBarRect.anchorMin = new Vector2(0f, 0f);
        _bottomBarRect.anchorMax = new Vector2(1f, 0f);
        _bottomBarRect.pivot = new Vector2(0.5f, 0f);
        _bottomBarRect.sizeDelta = new Vector2(0f, _barHeight);
        _bottomBarRect.anchoredPosition = new Vector2(0f, -_barHeight); // off-screen below
    }

    private void BuildStepBanner(Transform parent)
    {
        _bannerWidth = Mathf.Min(500f, PhaseCUITheme.RefWidth * 0.35f);

        _bannerPanel = new GameObject("StepBannerPanel");
        _bannerPanel.transform.SetParent(parent, false);

        _bannerCG = _bannerPanel.AddComponent<CanvasGroup>();
        _bannerCG.alpha = 0f;
        _bannerCG.interactable = false;
        _bannerCG.blocksRaycasts = false;

        Image bg = _bannerPanel.AddComponent<Image>();
        bg.color = PhaseCUITheme.PanelBg;
        bg.raycastTarget = false;

        _bannerRect = _bannerPanel.GetComponent<RectTransform>();
        _bannerRect.anchorMin = new Vector2(0f, 0.5f);
        _bannerRect.anchorMax = new Vector2(0f, 0.5f);
        _bannerRect.pivot = new Vector2(0f, 0.5f);
        _bannerRect.sizeDelta = new Vector2(_bannerWidth, 80f);
        _bannerRect.anchoredPosition = new Vector2(-(_bannerWidth + 20f), 0f);

        // Gold left accent bar
        GameObject barGo = new GameObject("AccentBar");
        barGo.transform.SetParent(_bannerPanel.transform, false);
        Image barImg = barGo.AddComponent<Image>();
        barImg.color = PhaseCUITheme.AccentGold;
        barImg.raycastTarget = false;
        RectTransform barRect = barGo.GetComponent<RectTransform>();
        barRect.anchorMin = new Vector2(0f, 0f);
        barRect.anchorMax = new Vector2(0f, 1f);
        barRect.pivot = new Vector2(0f, 0.5f);
        barRect.sizeDelta = new Vector2(6f, 0f);
        barRect.anchoredPosition = Vector2.zero;

        // Border
        GameObject borderGo = new GameObject("Border");
        borderGo.transform.SetParent(_bannerPanel.transform, false);
        Image borderImg = borderGo.AddComponent<Image>();
        borderImg.color = new Color(PhaseCUITheme.PanelBorder.r, PhaseCUITheme.PanelBorder.g, PhaseCUITheme.PanelBorder.b, 0.5f);
        borderImg.raycastTarget = false;
        RectTransform borderRect = borderGo.GetComponent<RectTransform>();
        borderRect.anchorMin = Vector2.zero;
        borderRect.anchorMax = Vector2.one;
        borderRect.offsetMin = new Vector2(-1f, -1f);
        borderRect.offsetMax = new Vector2(1f, 1f);
        borderGo.transform.SetAsFirstSibling();

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // Title: "STEP N COMPLETE"
        GameObject titleGo = new GameObject("BannerTitle");
        titleGo.transform.SetParent(_bannerPanel.transform, false);
        _bannerTitle = titleGo.AddComponent<Text>();
        _bannerTitle.font = font;
        _bannerTitle.fontSize = 20;
        _bannerTitle.fontStyle = FontStyle.Bold;
        _bannerTitle.color = PhaseCUITheme.AccentGold;
        _bannerTitle.alignment = TextAnchor.MiddleLeft;
        _bannerTitle.raycastTarget = false;
        RectTransform titleRect = titleGo.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 0.5f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.offsetMin = new Vector2(16f, 0f);
        titleRect.offsetMax = new Vector2(-10f, 0f);

        // Subtitle: step title
        GameObject subGo = new GameObject("BannerSubtitle");
        subGo.transform.SetParent(_bannerPanel.transform, false);
        _bannerSubtitle = subGo.AddComponent<Text>();
        _bannerSubtitle.font = font;
        _bannerSubtitle.fontSize = 15;
        _bannerSubtitle.color = PhaseCUITheme.TextPrimary;
        _bannerSubtitle.alignment = TextAnchor.MiddleLeft;
        _bannerSubtitle.raycastTarget = false;
        RectTransform subRect = subGo.GetComponent<RectTransform>();
        subRect.anchorMin = new Vector2(0f, 0f);
        subRect.anchorMax = new Vector2(1f, 0.5f);
        subRect.offsetMin = new Vector2(16f, 0f);
        subRect.offsetMax = new Vector2(-10f, 0f);

        _bannerPanel.SetActive(false);
    }

    // ---- Sprite generation ----

    private static Sprite MakeRingSprite()
    {
        const int Sz = 64;
        Texture2D tex = new Texture2D(Sz, Sz, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        float cx = (Sz - 1) * 0.5f;
        float outerR = Sz * 0.5f;
        float innerR = Sz * 0.375f; // 8px ring at 64px = 25% hollow

        for (int y = 0; y < Sz; y++)
        for (int x = 0; x < Sz; x++)
        {
            float d = Mathf.Sqrt((x - cx) * (x - cx) + (y - cx) * (y - cx));
            float outer = Mathf.Clamp01((outerR - d) / 2f);
            float inner = Mathf.Clamp01((d - innerR) / 2f);
            tex.SetPixel(x, y, new Color(1f, 1f, 1f, outer * inner));
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, Sz, Sz), new Vector2(0.5f, 0.5f), Sz);
    }

    // ---- Easing ----

    private static float EaseOutCubic(float p) => 1f - (1f - p) * (1f - p) * (1f - p);
    private static float EaseInCubic(float p) => p * p * p;
}
