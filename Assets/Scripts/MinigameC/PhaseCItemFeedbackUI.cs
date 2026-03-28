using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Displays brief toast notifications when items are picked up or dropped,
/// and a prominent delivery confirmation panel when items are assembled and handed to an NPC.
/// Auto-created at scene load. Call ShowPickup / ShowDrop / ShowDelivery from other scripts.
/// Notifications are queued so rapid events do not overlap.
/// </summary>
public class PhaseCItemFeedbackUI : MonoBehaviour
{
    private const string TargetSceneName = "MinigameC";
    private const string CanvasName = "PhaseCItemFeedbackCanvas";

    private static PhaseCItemFeedbackUI _instance;

    private bool _initialized;
    private Queue<NotifData> _queue = new Queue<NotifData>();
    private bool _showing;

    // Pickup/drop toast
    private GameObject _notifPanel;
    private CanvasGroup _canvasGroup;
    private Image _accentBar;
    private Image _dotImage;
    private Text _notifText;

    // Delivery confirmation panel
    private GameObject _deliveryPanel;
    private CanvasGroup _deliveryCG;
    private Text _deliveryTitle;
    private Text _deliverySub;
    private Coroutine _deliveryRoutine;
    private List<GameObject> _deliverySparkles = new List<GameObject>();
    private Coroutine _sparkleRoutine;

    private const float NotifHeight = 50f;
    private const float BottomOffset = 52f;
    private const float SlideDistance = 10f;
    private const float AnimInDuration = 0.12f;
    private const float HoldDuration = 1.1f;
    private const float AnimOutDuration = 0.22f;

    private const float DeliveryHeight = 86f;
    private const float DeliverySlideIn = 12f;
    private const float DeliveryInDuration = 0.2f;
    private const float DeliveryHoldDuration = 2.4f;
    private const float DeliveryOutDuration = 0.3f;

    private struct NotifData
    {
        public string Message;
        public Color TextColor;
        public Color DotColor;
    }

    // Bootstrap

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void EnsureFeedbackUI()
    {
        SceneManager.sceneLoaded += (scene, _) =>
        {
            if (scene.name != TargetSceneName) return;
            if (FindFirstObjectByType<PhaseCItemFeedbackUI>() != null) return;
            new GameObject("PhaseCItemFeedbackUI").AddComponent<PhaseCItemFeedbackUI>();
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
    }

    private void Start()
    {
        BuildCanvas();
        _initialized = true;
    }

    // Public API

    /// <summary>Shows a green "Picked Up: [name]" toast at the bottom of the screen.</summary>
    public static void ShowPickup(string itemName, Color itemColor)
    {
        if (_instance == null || !_instance._initialized) return;
        _instance.Enqueue(new NotifData
        {
            Message = "Picked Up: " + itemName,
            TextColor = PhaseCUITheme.StepDone,
            DotColor = itemColor
        });
    }

    /// <summary>Shows a gold "Dropped: [name]" toast at the bottom of the screen.</summary>
    public static void ShowDrop(string itemName, Color itemColor)
    {
        if (_instance == null || !_instance._initialized) return;
        _instance.Enqueue(new NotifData
        {
            Message = "Dropped: " + itemName,
            TextColor = PhaseCUITheme.AccentGold,
            DotColor = itemColor
        });
    }

    /// <summary>Shows a prominent delivery confirmation panel when a component is assembled and delivered to an NPC.</summary>
    public static void ShowDelivery(string componentName, string npcName)
    {
        if (_instance == null || !_instance._initialized) return;
        if (_instance._deliveryRoutine != null)
            _instance.StopCoroutine(_instance._deliveryRoutine);
        _instance._deliveryRoutine = _instance.StartCoroutine(_instance.AnimateDelivery(componentName, npcName));
    }

    // Queue handling

    private void Enqueue(NotifData data)
    {
        _queue.Enqueue(data);
        if (!_showing) StartCoroutine(ProcessQueue());
    }

    private IEnumerator ProcessQueue()
    {
        _showing = true;
        while (_queue.Count > 0)
            yield return StartCoroutine(Animate(_queue.Dequeue()));
        _showing = false;
    }

    private IEnumerator AnimateDelivery(string componentName, string npcName)
    {
        if (_deliveryPanel == null) yield break;

        _deliveryTitle.text = componentName + " assembled";
        _deliverySub.text = "Delivered to: " + npcName;

        _deliveryPanel.SetActive(true);
        RectTransform rt = _deliveryPanel.GetComponent<RectTransform>();
        rt.localScale = Vector3.one;

        float deliveryTopOffset = PhaseCUITheme.GetDeliveryTopOffset();
        // Start fully off-screen above the top edge
        float fromY = 0f;
        const float InDuration = 0.3f;

        for (float t = 0f; t < InDuration; t += Time.deltaTime)
        {
            float p = t / InDuration;
            float eased = EaseOutCubic(p);
            // Scale bounce: 0.85 -> 1.08 in first 60%, then 1.08 -> 1.0 in last 40%
            float scale = p < 0.6f
                ? Mathf.Lerp(0.85f, 1.08f, p / 0.6f)
                : Mathf.Lerp(1.08f, 1.0f, (p - 0.6f) / 0.4f);
            _deliveryCG.alpha = Mathf.Min(1f, p / 0.4f);
            rt.anchoredPosition = new Vector2(0f, Mathf.Lerp(fromY, deliveryTopOffset, eased));
            rt.localScale = new Vector3(scale, scale, 1f);
            yield return null;
        }
        _deliveryCG.alpha = 1f;
        rt.anchoredPosition = new Vector2(0f, deliveryTopOffset);
        rt.localScale = Vector3.one;

        // Launch sparkles after panel lands
        if (_sparkleRoutine != null) StopCoroutine(_sparkleRoutine);
        _sparkleRoutine = StartCoroutine(AnimateDeliverySparkles(rt));

        yield return new WaitForSeconds(DeliveryHoldDuration);

        // Kill sparkles before fading out
        StopAndClearSparkles();

        for (float t = 0f; t < DeliveryOutDuration; t += Time.deltaTime)
        {
            _deliveryCG.alpha = 1f - (t / DeliveryOutDuration);
            yield return null;
        }
        _deliveryCG.alpha = 0f;
        rt.localScale = Vector3.one;
        _deliveryPanel.SetActive(false);
        _deliveryRoutine = null;
    }

    private IEnumerator AnimateDeliverySparkles(RectTransform panelRect)
    {
        const int Count = 6;
        float panelW = panelRect.sizeDelta.x;
        float panelH = panelRect.sizeDelta.y;

        // Reuse or create sparkle GameObjects
        while (_deliverySparkles.Count < Count)
        {
            GameObject s = new GameObject("DeliverySparkle");
            s.transform.SetParent(_deliveryPanel.transform, false);
            Image img = s.AddComponent<Image>();
            img.sprite = MakeDiscSprite();
            img.raycastTarget = false;
            RectTransform srt = s.GetComponent<RectTransform>();
            srt.sizeDelta = new Vector2(8f, 8f);
            srt.pivot = new Vector2(0.5f, 0.5f);
            _deliverySparkles.Add(s);
        }

        Color[] sparkleColors = { PhaseCUITheme.AccentGold, PhaseCUITheme.AccentCyan, Color.white };
        float[] lifetimes = new float[Count];
        float[] velX = new float[Count];
        float[] velY = new float[Count];
        Vector2[] startPos = new Vector2[Count];

        for (int i = 0; i < Count; i++)
        {
            float spawnX = Random.Range(-panelW * 0.4f, panelW * 0.4f);
            float spawnY = Random.Range(-panelH * 0.3f, panelH * 0.3f);
            startPos[i] = new Vector2(spawnX, spawnY);
            lifetimes[i] = Random.Range(1.2f, 2.0f);
            velX[i] = Random.Range(-30f, 30f);
            velY[i] = Random.Range(40f, 90f);

            RectTransform srt = _deliverySparkles[i].GetComponent<RectTransform>();
            srt.anchorMin = new Vector2(0.5f, 0.5f);
            srt.anchorMax = new Vector2(0.5f, 0.5f);
            srt.anchoredPosition = startPos[i];

            Image img = _deliverySparkles[i].GetComponent<Image>();
            img.color = sparkleColors[i % sparkleColors.Length];
            _deliverySparkles[i].SetActive(true);
        }

        float[] elapsed = new float[Count];
        bool anyAlive = true;
        while (anyAlive)
        {
            anyAlive = false;
            for (int i = 0; i < Count; i++)
            {
                if (elapsed[i] >= lifetimes[i]) continue;
                elapsed[i] += Time.deltaTime;
                float p = elapsed[i] / lifetimes[i];
                RectTransform srt = _deliverySparkles[i].GetComponent<RectTransform>();
                srt.anchoredPosition = new Vector2(
                    startPos[i].x + velX[i] * elapsed[i],
                    startPos[i].y + velY[i] * elapsed[i]);
                Image img = _deliverySparkles[i].GetComponent<Image>();
                Color c = img.color;
                img.color = new Color(c.r, c.g, c.b, 1f - p);
                if (elapsed[i] < lifetimes[i]) anyAlive = true;
            }
            yield return null;
        }

        foreach (var s in _deliverySparkles) if (s != null) s.SetActive(false);
        _sparkleRoutine = null;
    }

    private void StopAndClearSparkles()
    {
        if (_sparkleRoutine != null)
        {
            StopCoroutine(_sparkleRoutine);
            _sparkleRoutine = null;
        }
        foreach (var s in _deliverySparkles)
            if (s != null) s.SetActive(false);
    }

    private static float EaseOutCubic(float p) => 1f - (1f - p) * (1f - p) * (1f - p);

    private IEnumerator Animate(NotifData data)
    {
        if (_notifPanel == null) yield break;

        _notifText.text = data.Message;
        _notifText.color = data.TextColor;
        _dotImage.color = data.DotColor;
        _accentBar.color = data.TextColor;

        _notifPanel.SetActive(true);
        RectTransform rt = _notifPanel.GetComponent<RectTransform>();

        // Fade in + slide up from hint strip
        float fromY = BottomOffset - SlideDistance;
        for (float t = 0f; t < AnimInDuration; t += Time.deltaTime)
        {
            float p = t / AnimInDuration;
            _canvasGroup.alpha = p;
            rt.anchoredPosition = new Vector2(0f, Mathf.Lerp(fromY, BottomOffset, p));
            yield return null;
        }
        _canvasGroup.alpha = 1f;
        rt.anchoredPosition = new Vector2(0f, BottomOffset);

        yield return new WaitForSeconds(HoldDuration);

        // Fade out
        for (float t = 0f; t < AnimOutDuration; t += Time.deltaTime)
        {
            _canvasGroup.alpha = 1f - (t / AnimOutDuration);
            yield return null;
        }
        _canvasGroup.alpha = 0f;
        _notifPanel.SetActive(false);
    }

    // Canvas construction

    private void BuildCanvas()
    {
        GameObject existing = GameObject.Find(CanvasName);
        if (existing != null) Destroy(existing);

        GameObject canvasGo = new GameObject(CanvasName);
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;

        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(PhaseCUITheme.RefWidth, PhaseCUITheme.RefHeight);
        scaler.matchWidthOrHeight = PhaseCUITheme.CanvasMatchWidthOrHeight;

        canvasGo.AddComponent<GraphicRaycaster>();

        // Notification panel anchored to bottom-center
        _notifPanel = new GameObject("NotifPanel");
        _notifPanel.transform.SetParent(canvasGo.transform, false);

        _canvasGroup = _notifPanel.AddComponent<CanvasGroup>();
        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;

        Image bgImg = _notifPanel.AddComponent<Image>();
        bgImg.color = new Color(0.05f, 0.08f, 0.14f, 0.93f);
        bgImg.raycastTarget = false;

        RectTransform panelRect = _notifPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0f);
        panelRect.anchorMax = new Vector2(0.5f, 0f);
        panelRect.pivot = new Vector2(0.5f, 0f);
        panelRect.sizeDelta = new Vector2(PhaseCUITheme.GetNotifWidth(), NotifHeight);
        panelRect.anchoredPosition = new Vector2(0f, BottomOffset);

        // Outer border
        GameObject borderGo = new GameObject("Border");
        borderGo.transform.SetParent(_notifPanel.transform, false);
        Image borderImg = borderGo.AddComponent<Image>();
        borderImg.color = new Color(PhaseCUITheme.PanelBorder.r, PhaseCUITheme.PanelBorder.g, PhaseCUITheme.PanelBorder.b, 0.5f);
        borderImg.raycastTarget = false;
        RectTransform borderRect = borderGo.GetComponent<RectTransform>();
        borderRect.anchorMin = Vector2.zero;
        borderRect.anchorMax = Vector2.one;
        borderRect.offsetMin = new Vector2(-1f, -1f);
        borderRect.offsetMax = new Vector2(1f, 1f);
        borderGo.transform.SetAsFirstSibling();

        // Left accent bar (4px wide, color changes per notification type)
        GameObject barGo = new GameObject("AccentBar");
        barGo.transform.SetParent(_notifPanel.transform, false);
        _accentBar = barGo.AddComponent<Image>();
        _accentBar.color = PhaseCUITheme.AccentCyan;
        _accentBar.raycastTarget = false;
        RectTransform barRect = barGo.GetComponent<RectTransform>();
        barRect.anchorMin = new Vector2(0f, 0f);
        barRect.anchorMax = new Vector2(0f, 1f);
        barRect.pivot = new Vector2(0f, 0.5f);
        barRect.sizeDelta = new Vector2(4f, 0f);
        barRect.anchoredPosition = Vector2.zero;

        // Item color dot
        GameObject dotGo = new GameObject("ItemDot");
        dotGo.transform.SetParent(_notifPanel.transform, false);
        _dotImage = dotGo.AddComponent<Image>();
        _dotImage.sprite = MakeDiscSprite();
        _dotImage.color = Color.white;
        _dotImage.raycastTarget = false;
        RectTransform dotRect = dotGo.GetComponent<RectTransform>();
        dotRect.anchorMin = new Vector2(0f, 0.5f);
        dotRect.anchorMax = new Vector2(0f, 0.5f);
        dotRect.pivot = new Vector2(0.5f, 0.5f);
        dotRect.sizeDelta = new Vector2(18f, 18f);
        dotRect.anchoredPosition = new Vector2(22f, 0f);

        // Notification text
        GameObject textGo = new GameObject("NotifText");
        textGo.transform.SetParent(_notifPanel.transform, false);
        _notifText = textGo.AddComponent<Text>();
        _notifText.text = "";
        _notifText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        _notifText.fontSize = (int)PhaseCUITheme.GuideCaptionSize;
        _notifText.fontStyle = FontStyle.Bold;
        _notifText.color = PhaseCUITheme.TextPrimary;
        _notifText.alignment = TextAnchor.MiddleLeft;
        _notifText.raycastTarget = false;
        RectTransform textRect = textGo.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(40f, 0f);
        textRect.offsetMax = new Vector2(-10f, 0f);

        _notifPanel.SetActive(false);

        BuildDeliveryPanel(canvasGo.transform);
    }

    private void BuildDeliveryPanel(Transform canvasParent)
    {
        _deliveryPanel = new GameObject("DeliveryPanel");
        _deliveryPanel.transform.SetParent(canvasParent, false);

        _deliveryCG = _deliveryPanel.AddComponent<CanvasGroup>();
        _deliveryCG.alpha = 0f;
        _deliveryCG.interactable = false;
        _deliveryCG.blocksRaycasts = false;

        Image bg = _deliveryPanel.AddComponent<Image>();
        bg.color = new Color(0.05f, 0.07f, 0.13f, 0.96f);
        bg.raycastTarget = false;

        RectTransform rt = _deliveryPanel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.sizeDelta = new Vector2(PhaseCUITheme.GetDeliveryWidth(), DeliveryHeight);
        rt.anchoredPosition = new Vector2(0f, PhaseCUITheme.GetDeliveryTopOffset());

        // Border (subtle blue, matches other panels)
        GameObject borderGo = new GameObject("Border");
        borderGo.transform.SetParent(_deliveryPanel.transform, false);
        Image borderImg = borderGo.AddComponent<Image>();
        borderImg.color = new Color(PhaseCUITheme.PanelBorder.r, PhaseCUITheme.PanelBorder.g, PhaseCUITheme.PanelBorder.b, 0.6f);
        borderImg.raycastTarget = false;
        RectTransform borderRect = borderGo.GetComponent<RectTransform>();
        borderRect.anchorMin = Vector2.zero;
        borderRect.anchorMax = Vector2.one;
        borderRect.offsetMin = new Vector2(-1f, -1f);
        borderRect.offsetMax = new Vector2(1f, 1f);
        borderGo.transform.SetAsFirstSibling();

        // Left gold accent bar
        GameObject barGo = new GameObject("AccentBar");
        barGo.transform.SetParent(_deliveryPanel.transform, false);
        Image barImg = barGo.AddComponent<Image>();
        barImg.color = PhaseCUITheme.AccentGold;
        barImg.raycastTarget = false;
        RectTransform barRect = barGo.GetComponent<RectTransform>();
        barRect.anchorMin = new Vector2(0f, 0f);
        barRect.anchorMax = new Vector2(0f, 1f);
        barRect.pivot = new Vector2(0f, 0.5f);
        barRect.sizeDelta = new Vector2(6f, 0f);
        barRect.anchoredPosition = Vector2.zero;

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // Component name (title line)
        GameObject titleGo = new GameObject("DeliveryTitle");
        titleGo.transform.SetParent(_deliveryPanel.transform, false);
        _deliveryTitle = titleGo.AddComponent<Text>();
        _deliveryTitle.text = "";
        _deliveryTitle.font = font;
        _deliveryTitle.fontSize = 20;
        _deliveryTitle.fontStyle = FontStyle.Bold;
        _deliveryTitle.color = PhaseCUITheme.TextPrimary;
        _deliveryTitle.alignment = TextAnchor.MiddleLeft;
        _deliveryTitle.raycastTarget = false;
        RectTransform titleRect = titleGo.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 0.5f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.offsetMin = new Vector2(20f, 0f);
        titleRect.offsetMax = new Vector2(-12f, 0f);

        // "Delivered to: NPC" subtitle
        GameObject subGo = new GameObject("DeliverySub");
        subGo.transform.SetParent(_deliveryPanel.transform, false);
        _deliverySub = subGo.AddComponent<Text>();
        _deliverySub.text = "";
        _deliverySub.font = font;
        _deliverySub.fontSize = 15;
        _deliverySub.color = PhaseCUITheme.AccentCyan;
        _deliverySub.alignment = TextAnchor.MiddleLeft;
        _deliverySub.raycastTarget = false;
        RectTransform subRect = subGo.GetComponent<RectTransform>();
        subRect.anchorMin = new Vector2(0f, 0f);
        subRect.anchorMax = new Vector2(1f, 0.5f);
        subRect.offsetMin = new Vector2(20f, 0f);
        subRect.offsetMax = new Vector2(-12f, 0f);

        _deliveryPanel.SetActive(false);
    }

    // Generates a soft white disc sprite that can be tinted via Image.color
    private static Sprite MakeDiscSprite()
    {
        const int sz = 32;
        Texture2D tex = new Texture2D(sz, sz, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        float cx = (sz - 1) * 0.5f;
        float r = sz * 0.5f;
        for (int y = 0; y < sz; y++)
        for (int x = 0; x < sz; x++)
        {
            float d = Mathf.Sqrt((x - cx) * (x - cx) + (y - cx) * (y - cx));
            float solid = d < r * 0.7f ? 1f : Mathf.Clamp01((r - d) / (r * 0.3f));
            tex.SetPixel(x, y, new Color(1f, 1f, 1f, solid));
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, sz, sz), new Vector2(0.5f, 0.5f), sz);
    }
}
