using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Virtual joystick for MinigameC on mobile/touch devices.
/// Auto-creates on scene load when a touch screen is detected.
/// PlayerMovement reads PhaseCMobileInput.Horizontal and .Vertical
/// instead of Input.GetAxis when on mobile.
/// </summary>
public class PhaseCMobileInput : MonoBehaviour
{
    private const string TargetSceneName = "MinigameC";
    private const string CanvasName = "PhaseCMobileInputCanvas";

    private static PhaseCMobileInput _instance;

    public static float Horizontal { get; private set; }
    public static float Vertical   { get; private set; }
    public static bool  Active     { get; private set; }

    private RectTransform _baseRect;
    private RectTransform _stickRect;
    private Canvas        _canvas;

    private int   _touchId = -1;
    private bool  _isDragging;
    private float _radius;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void EnsureMobileInput()
    {
        SceneManager.sceneLoaded += (scene, _) =>
        {
            if (scene.name != TargetSceneName) return;
            if (!PhaseCUITheme.IsMobileScreen) return;
            if (FindFirstObjectByType<PhaseCMobileInput>() != null) return;
            new GameObject("PhaseCMobileInput").AddComponent<PhaseCMobileInput>();
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
        Active = true;
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
            Active = false;
        }
    }

    private void Start()
    {
        BuildJoystick();
    }

    private void Update()
    {
        if (_baseRect == null) return;

        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);

            if (!_isDragging && touch.phase == TouchPhase.Began)
            {
                Vector2 localPoint;
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        _baseRect, touch.position, _canvas.worldCamera, out localPoint))
                {
                    if (localPoint.magnitude <= _radius * 1.5f)
                    {
                        _isDragging = true;
                        _touchId = touch.fingerId;
                    }
                }
            }

            if (_isDragging && touch.fingerId == _touchId)
            {
                if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    ResetStick();
                    break;
                }

                Vector2 localPoint;
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        _baseRect, touch.position, _canvas.worldCamera, out localPoint))
                {
                    Vector2 clamped = Vector2.ClampMagnitude(localPoint, _radius);
                    _stickRect.anchoredPosition = clamped;
                    Horizontal = clamped.x / _radius;
                    Vertical   = clamped.y / _radius;
                }
            }
        }

        if (Input.touchCount == 0 && _isDragging)
            ResetStick();
    }

    private void ResetStick()
    {
        _isDragging = false;
        _touchId    = -1;
        Horizontal  = 0f;
        Vertical    = 0f;
        if (_stickRect != null)
            _stickRect.anchoredPosition = Vector2.zero;
    }

    private void BuildJoystick()
    {
        GameObject existing = GameObject.Find(CanvasName);
        if (existing != null) Destroy(existing);

        GameObject canvasGo = new GameObject(CanvasName);
        _canvas = canvasGo.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 20;

        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(PhaseCUITheme.RefWidth, PhaseCUITheme.RefHeight);
        scaler.matchWidthOrHeight = PhaseCUITheme.CanvasMatchWidthOrHeight;

        canvasGo.AddComponent<GraphicRaycaster>();

        float hintStripHeight = PhaseCUITheme.GetHintStripHeight();
        float joystickSize = 140f;
        _radius = joystickSize * 0.35f;
        float margin = 24f;

        // Outer ring (base)
        GameObject baseGo = new GameObject("JoystickBase");
        baseGo.transform.SetParent(canvasGo.transform, false);
        Image baseImg = baseGo.AddComponent<Image>();
        baseImg.color = new Color(0.1f, 0.12f, 0.2f, 0.55f);
        baseImg.sprite = MakeDiscSprite(64);
        baseImg.raycastTarget = true;

        _baseRect = baseGo.GetComponent<RectTransform>();
        _baseRect.anchorMin = new Vector2(0f, 0f);
        _baseRect.anchorMax = new Vector2(0f, 0f);
        _baseRect.pivot     = new Vector2(0.5f, 0f);
        _baseRect.sizeDelta = new Vector2(joystickSize, joystickSize);
        _baseRect.anchoredPosition = new Vector2(margin + joystickSize * 0.5f, hintStripHeight + margin);

        // Inner ring border
        GameObject ringGo = new GameObject("Ring");
        ringGo.transform.SetParent(baseGo.transform, false);
        Image ringImg = ringGo.AddComponent<Image>();
        ringImg.color = new Color(PhaseCUITheme.AccentCyan.r, PhaseCUITheme.AccentCyan.g, PhaseCUITheme.AccentCyan.b, 0.5f);
        ringImg.sprite = MakeDiscSprite(64);
        ringImg.raycastTarget = false;
        RectTransform ringRect = ringGo.GetComponent<RectTransform>();
        ringRect.anchorMin = Vector2.zero;
        ringRect.anchorMax = Vector2.one;
        ringRect.offsetMin = new Vector2(4f, 4f);
        ringRect.offsetMax = new Vector2(-4f, -4f);

        // Stick knob
        GameObject stickGo = new GameObject("Stick");
        stickGo.transform.SetParent(baseGo.transform, false);
        Image stickImg = stickGo.AddComponent<Image>();
        stickImg.color = new Color(PhaseCUITheme.AccentCyan.r, PhaseCUITheme.AccentCyan.g, PhaseCUITheme.AccentCyan.b, 0.85f);
        stickImg.sprite = MakeDiscSprite(32);
        stickImg.raycastTarget = false;

        _stickRect = stickGo.GetComponent<RectTransform>();
        _stickRect.anchorMin = new Vector2(0.5f, 0.5f);
        _stickRect.anchorMax = new Vector2(0.5f, 0.5f);
        _stickRect.pivot     = new Vector2(0.5f, 0.5f);
        _stickRect.sizeDelta = new Vector2(joystickSize * 0.45f, joystickSize * 0.45f);
        _stickRect.anchoredPosition = Vector2.zero;
    }

    private static Sprite MakeDiscSprite(int size)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        float cx = (size - 1) * 0.5f;
        float r  = size * 0.5f;
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float d = Mathf.Sqrt((x - cx) * (x - cx) + (y - cx) * (y - cx));
            float a = d < r * 0.75f ? 1f : Mathf.Clamp01((r - d) / (r * 0.25f));
            tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
}
