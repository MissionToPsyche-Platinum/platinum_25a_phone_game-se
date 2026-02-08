using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Shows an arrow at the screen edge pointing toward the current objective:
/// either the nearest required item to collect, or the NPC to talk to.
/// </summary>
public class ObjectiveArrowUI : MonoBehaviour
{
    private const string TargetSceneName = "MinigameC";
    private const string ArrowCanvasName = "ObjectiveArrowCanvas";

    [Header("Layout")]
    [SerializeField] [Range(0f, 0.5f)] private float edgeMargin = 0.08f;
    [SerializeField] private float arrowSize = 48f;
    [Tooltip("Smooth arrow position and rotation so it does not jump when the player moves.")]
    [SerializeField] private float smoothSpeed = 8f;

    private Camera mainCamera;
    private Transform playerTransform;
    private PhaseCAssemblyController controller;
    private RectTransform arrowRect;
    private Canvas arrowCanvas;
    private Image arrowImage;
    private bool initialized;

    private Transform lockedTarget;
    private int lockedStepIndex;
    private int lockedSubProgress;
    private Vector2 arrowCurrentPos;
    private float arrowCurrentAngleDeg;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureArrow()
    {
        Scene s = SceneManager.GetActiveScene();
        if (s.name != TargetSceneName) return;
        if (FindFirstObjectByType<ObjectiveArrowUI>() != null) return;
        var go = new GameObject("ObjectiveArrowUI");
        go.AddComponent<ObjectiveArrowUI>();
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
        mainCamera = Camera.main;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;
        controller = PhaseCAssemblyController.Instance;
        if (controller != null)
        {
            CreateArrowCanvas();
            initialized = arrowRect != null;
        }
    }

    private void LateUpdate()
    {
        if (!initialized)
        {
            if (controller == null) controller = PhaseCAssemblyController.Instance;
            if (controller != null && arrowCanvas == null)
            {
                CreateArrowCanvas();
                initialized = arrowRect != null;
            }
            if (!initialized) return;
        }
        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera == null) return;

        Vector3? targetWorld = GetTargetWorldPosition();
        if (!targetWorld.HasValue)
        {
            SetArrowVisible(false);
            lockedTarget = null;
            return;
        }

        Vector2 screenCenter = new Vector2(0.5f, 0.5f);
        Vector3 viewportPoint3D = mainCamera.WorldToViewportPoint(targetWorld.Value);
        Vector2 targetViewport = new Vector2(viewportPoint3D.x, viewportPoint3D.y);

        // If target is behind the camera, invert the direction
        // WorldToViewportPoint returns incorrect X/Y when Z is negative
        if (viewportPoint3D.z < 0)
        {
            targetViewport = screenCenter - (targetViewport - screenCenter);
        }

        // Check if target is on screen (within viewport bounds with some padding)
        bool isOnScreen = viewportPoint3D.z > 0 &&
                          targetViewport.x > edgeMargin && targetViewport.x < (1f - edgeMargin) &&
                          targetViewport.y > edgeMargin && targetViewport.y < (1f - edgeMargin);

        if (isOnScreen)
        {
            SetArrowVisible(false);
            return;
        }

        SetArrowVisible(true);
        Vector2 dir = targetViewport - screenCenter;
        if (dir.sqrMagnitude < 0.0001f) return;
        dir.Normalize();

        float margin = Mathf.Clamp01(edgeMargin);
        float minX = margin, maxX = 1f - margin, minY = margin, maxY = 1f - margin;
        Vector2 edgeViewport = ClampToViewportEdge(screenCenter, dir, minX, maxX, minY, maxY);
        Vector2 screenPoint = new Vector2(edgeViewport.x * Screen.width, edgeViewport.y * Screen.height);
        float angleDeg = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        if (arrowCanvas != null && arrowRect != null)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                arrowCanvas.transform as RectTransform, screenPoint, null, out Vector2 targetLocalPos);
            float dt = Time.deltaTime;
            float t = smoothSpeed > 0f ? Mathf.Clamp01(dt * smoothSpeed) : 1f;
            arrowCurrentPos = Vector2.Lerp(arrowCurrentPos, targetLocalPos, t);
            arrowCurrentAngleDeg = Mathf.LerpAngle(arrowCurrentAngleDeg, angleDeg, t);
            arrowRect.anchoredPosition = arrowCurrentPos;
            arrowRect.localEulerAngles = new Vector3(0f, 0f, arrowCurrentAngleDeg);
        }
    }

    private Vector3? GetTargetWorldPosition()
    {
        if (controller == null) return null;

        if (playerTransform == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerTransform = p.transform;
        }

        int stepIndex = controller.GetCurrentStepIndexForArrow();
        int subProgress = controller.GetCurrentStepSubProgressForArrow();
        bool stepOrProgressChanged = (stepIndex != lockedStepIndex || subProgress != lockedSubProgress);
        if (stepOrProgressChanged)
        {
            lockedStepIndex = stepIndex;
            lockedSubProgress = subProgress;
            lockedTarget = null;
        }

        if (lockedTarget == null)
        {
            Vector3 playerPos = playerTransform != null ? playerTransform.position : Vector3.zero;
            List<int> requiredIds = controller.GetCurrentStepRequiredItemIds();
            if (requiredIds != null && requiredIds.Count > 0)
            {
                GameObject[] items = GameObject.FindGameObjectsWithTag("Item");
                GameObject nearest = null;
                float nearestSq = float.MaxValue;
                foreach (GameObject go in items)
                {
                    if (go == null) continue;
                    Item item = go.GetComponent<Item>();
                    if (item == null) continue;
                    if (!requiredIds.Contains(item.ID)) continue;
                    float sq = (go.transform.position - playerPos).sqrMagnitude;
                    if (sq < nearestSq)
                    {
                        nearestSq = sq;
                        nearest = go;
                    }
                }
                if (nearest != null)
                {
                    lockedTarget = nearest.transform;
                    return lockedTarget.position;
                }
            }

            Vector3? npcPos = controller.GetCurrentStepCompletionNpcWorldPosition();
            if (npcPos.HasValue)
            {
                npc npcComponent = controller.GetCompletionNpcComponentForArrow();
                lockedTarget = npcComponent != null ? npcComponent.transform : null;
                return npcPos;
            }
            return null;
        }

        if (lockedTarget == null) return null;
        return lockedTarget.position;
    }

    private static Vector2 ClampToViewportEdge(Vector2 center, Vector2 dir, float minX, float maxX, float minY, float maxY)
    {
        float t = float.MaxValue;
        if (dir.x > 0.001f) t = Mathf.Min(t, (maxX - center.x) / dir.x);
        else if (dir.x < -0.001f) t = Mathf.Min(t, (minX - center.x) / dir.x);
        if (dir.y > 0.001f) t = Mathf.Min(t, (maxY - center.y) / dir.y);
        else if (dir.y < -0.001f) t = Mathf.Min(t, (minY - center.y) / dir.y);
        if (t <= 0 || t == float.MaxValue) return center;
        return center + dir * t;
    }

    private void SetArrowVisible(bool visible)
    {
        if (arrowImage != null) arrowImage.enabled = visible;
    }

    private void CreateArrowCanvas()
    {
        GameObject existing = GameObject.Find(ArrowCanvasName);
        if (existing != null) Destroy(existing);

        GameObject canvasGo = new GameObject(ArrowCanvasName);
        Canvas c = canvasGo.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = 9;

        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(PhaseCUITheme.RefWidth, PhaseCUITheme.RefHeight);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGo.AddComponent<GraphicRaycaster>();

        arrowCanvas = c;

        GameObject arrowGo = new GameObject("Arrow");
        arrowGo.transform.SetParent(canvasGo.transform, false);
        arrowImage = arrowGo.AddComponent<Image>();
        arrowImage.sprite = CreateArrowSprite();
        arrowImage.color = PhaseCUITheme.AccentGold;
        arrowImage.raycastTarget = false;

        arrowRect = arrowGo.GetComponent<RectTransform>();
        arrowRect.anchorMin = new Vector2(0.5f, 0.5f);
        arrowRect.anchorMax = new Vector2(0.5f, 0.5f);
        arrowRect.pivot = new Vector2(0.5f, 0.5f);
        arrowRect.sizeDelta = new Vector2(arrowSize, arrowSize);
        arrowRect.anchoredPosition = Vector2.zero;
    }

    private static Sprite CreateArrowSprite()
    {
        const int w = 32;
        const int h = 32;
        Texture2D tex = new Texture2D(w, h);
        Color clear = new Color(0, 0, 0, 0);
        Color white = Color.white;
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                tex.SetPixel(x, y, clear);

        int tipX = w - 2;
        int cy = h / 2;
        for (int y = 0; y < h; y++)
        {
            int xMax;
            if (y <= cy)
                xMax = cy > 0 ? (int)((float)tipX * y / cy) : 0;
            else
                xMax = (h - 1 - cy) > 0 ? (int)((float)tipX * (h - 1 - y) / (h - 1 - cy)) : 0;
            for (int x = 0; x <= xMax && x < w; x++)
                tex.SetPixel(x, y, white);
        }
        tex.Apply();
        tex.filterMode = FilterMode.Bilinear;
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f));
    }
}
