using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Displays brief toast notifications when items are picked up or dropped.
/// Auto-created at scene load. Call ShowPickup / ShowDrop from other scripts.
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

    private GameObject _notifPanel;
    private CanvasGroup _canvasGroup;
    private Image _accentBar;
    private Image _dotImage;
    private Text _notifText;

    private const float NotifWidth = 310f;
    private const float NotifHeight = 50f;
    private const float BottomOffset = 52f;
    private const float SlideDistance = 10f;
    private const float AnimInDuration = 0.12f;
    private const float HoldDuration = 1.1f;
    private const float AnimOutDuration = 0.22f;

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
        scaler.matchWidthOrHeight = 0.5f;

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
        panelRect.sizeDelta = new Vector2(NotifWidth, NotifHeight);
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
