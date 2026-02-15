using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Enhances the appearance of a part item dropped in the game world.
///
/// Design:
///   • ItemDictionary.ApplySprites() assigns a small 48×48 colored disc (PPU=48)
///     to the item's SpriteRenderer, giving a natural world size of 1×1 unit.
///   • This component scales every world item to worldScale (default 0.5) so items
///     appear as 0.5×0.5 world-unit discs — consistent regardless of original art.
///   • A matching collider is resized so pickup works reliably at the displayed size.
///   • Glow halo, pulse, bob, and floating name label complete the look.
///
/// Auto-attached:  WorldItemEnhancerManager scans every 0.5 s and calls Initialize()
///                 on any tagged "Item" object that is not inside an inventory slot.
/// </summary>
public class PhaseCWorldItemUI : MonoBehaviour
{
    private const string TargetScene = "MinigameC";

    [Header("Scale")]
    [Tooltip("Desired display size of each item in world units.")]
    [SerializeField] private float worldScale = 0.8f;

    [Header("Animation")]
    [SerializeField] private float pulseSpeed  = 2.0f;
    [SerializeField] private float pulseAmount = 0.10f;
    [SerializeField] private float bobSpeed    = 1.5f;
    [SerializeField] private float bobAmount   = 0.12f;

    // Runtime state
    private SpriteRenderer mainRenderer;
    private SpriteRenderer glowRenderer;
    private GameObject     labelRoot;
    private Item           itemData;
    private Vector3        basePosition;
    private float          timeOffset;
    private float          scaleFactor;   // computed: worldScale / sprite's natural size
    private bool           ready;

    // Cached ItemDictionary reference
    private static ItemDictionary _dict;
    private static ItemDictionary Dict =>
        _dict != null ? _dict : (_dict = Object.FindFirstObjectByType<ItemDictionary>());

    // ─── Bootstrap ───────────────────────────────────────────────────────────

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void BootEnhancer()
    {
        if (SceneManager.GetActiveScene().name != TargetScene) return;
        new GameObject("WorldItemEnhancer").AddComponent<WorldItemEnhancerManager>();
    }

    // ─── Initialization ──────────────────────────────────────────────────────

    public void Initialize()
    {
        if (ready) return;

        itemData     = GetComponent<Item>();
        mainRenderer = GetComponent<SpriteRenderer>();

        if (itemData == null || mainRenderer == null)
        {
            Destroy(this);
            return;
        }

        timeOffset   = Random.value * Mathf.PI * 2f;
        basePosition = transform.position;

        // Compute scaleFactor so the sprite renders at exactly worldScale world units,
        // regardless of its original PPU or texture resolution.
        // Art sprites are 2048 px at PPU=100 → natural size 20.48 units → scale ≈ 0.039.
        // Fallback disc sprites are 48 px at PPU=48 → natural size 1 unit → scale = worldScale.
        Sprite s = mainRenderer.sprite;
        if (s != null && s.pixelsPerUnit > 0f && s.rect.width > 0f)
            scaleFactor = worldScale / (s.rect.width / s.pixelsPerUnit);
        else
            scaleFactor = worldScale;

        transform.localScale = Vector3.one * scaleFactor;

        // Resize collider to give a world pickup radius = worldScale × 0.5.
        // In local space: radius = worldPickupRadius / scaleFactor.
        SetupCollider();

        // Visual effects
        BuildGlow();
        BuildLabel();

        ready = true;
    }

    // ─── Collider ────────────────────────────────────────────────────────────

    private void SetupCollider()
    {
        // World pickup radius = half the displayed size; convert to local space.
        float localR = (scaleFactor > 0f) ? (worldScale * 0.5f / scaleFactor) : 0.5f;

        CircleCollider2D cc = GetComponent<CircleCollider2D>();
        if (cc != null) { cc.radius = localR; return; }

        BoxCollider2D bc = GetComponent<BoxCollider2D>();
        if (bc != null) { bc.size = Vector2.one * localR * 2f; return; }

        CircleCollider2D nc = gameObject.AddComponent<CircleCollider2D>();
        nc.isTrigger = true;
        nc.radius    = localR;
    }

    // ─── Update ──────────────────────────────────────────────────────────────

    private void Update()
    {
        if (!ready) return;

        float t = Time.time + timeOffset;

        // Pulsing scale (around the computed scaleFactor, not worldScale directly)
        float pulse = 1f + Mathf.Sin(t * pulseSpeed) * pulseAmount;
        transform.localScale = Vector3.one * (scaleFactor * pulse);

        // Bobbing position
        float bob = Mathf.Sin(t * bobSpeed) * bobAmount;
        transform.position = basePosition + new Vector3(0f, bob, 0f);

        // Glow alpha flicker
        if (glowRenderer != null)
        {
            float a = 0.28f + Mathf.Sin(t * pulseSpeed * 0.65f) * 0.12f;
            Color gc = glowRenderer.color;
            gc.a = a;
            glowRenderer.color = gc;
        }

        // Keep label upright
        if (labelRoot != null)
            labelRoot.transform.rotation = Quaternion.identity;
    }

    // ─── Glow ────────────────────────────────────────────────────────────────

    private void BuildGlow()
    {
        GameObject go = new GameObject("Glow");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = Vector3.zero;
        go.transform.localScale    = Vector3.one * 2.4f;

        glowRenderer = go.AddComponent<SpriteRenderer>();
        glowRenderer.sprite       = MakeRadialGlowSprite();
        glowRenderer.sortingOrder = mainRenderer.sortingOrder - 1;

        // Get the item colour from the dictionary (defaults to white if not found)
        Color ic = Dict != null ? Dict.GetItemColor(itemData.ID) : Color.white;
        glowRenderer.color = new Color(ic.r, ic.g, ic.b, 0.30f);
    }

    // ─── Floating label ──────────────────────────────────────────────────────

    private void BuildLabel()
    {
        labelRoot = new GameObject("Label");
        labelRoot.transform.SetParent(transform, false);
        // Position above disc; local offset compensates for worldScale so it stays above
        labelRoot.transform.localPosition = new Vector3(0f, 1.1f, 0f);
        labelRoot.transform.localScale    = Vector3.one * 0.36f;

        TextMesh tm = labelRoot.AddComponent<TextMesh>();
        tm.text          = GetItemDisplayName();
        tm.fontSize      = 32;
        tm.characterSize = 0.15f;
        tm.anchor        = TextAnchor.MiddleCenter;
        tm.alignment     = TextAlignment.Center;
        tm.color         = Color.white;
        tm.font          = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        MeshRenderer mr = labelRoot.GetComponent<MeshRenderer>();
        if (mr != null) mr.sortingOrder = mainRenderer.sortingOrder + 10;

        // Semi-transparent background pill
        GameObject bg = new GameObject("LabelBg");
        bg.transform.SetParent(labelRoot.transform, false);
        bg.transform.localPosition = new Vector3(0f, 0f, 0.01f);

        SpriteRenderer bgSr = bg.AddComponent<SpriteRenderer>();
        bgSr.sprite       = MakeRoundedRectSprite();
        bgSr.color        = new Color(0.05f, 0.08f, 0.16f, 0.82f);
        bgSr.sortingOrder = mainRenderer.sortingOrder + 9;

        float tw = tm.text.Length * 0.082f + 0.3f;
        bg.transform.localScale = new Vector3(tw, 0.38f, 1f);
    }

    private string GetItemDisplayName()
    {
        if (itemData != null && !string.IsNullOrEmpty(itemData.displayName))
            return itemData.displayName;
        return Dict != null ? Dict.GetDisplayName(itemData.ID) : $"Item {itemData?.ID}";
    }

    // ─── Procedural sprites ──────────────────────────────────────────────────

    private static Sprite MakeRadialGlowSprite()
    {
        const int size = 64;
        Texture2D tex  = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        float cx = (size - 1) * 0.5f, r = size * 0.5f;
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float d = Mathf.Sqrt((x - cx) * (x - cx) + (y - cx) * (y - cx));
            float a = Mathf.Clamp01(1f - d / r);
            a = a * a;
            tex.SetPixel(x, y, new Color(1f, 1f, 1f, a * 0.7f));
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    private static Sprite MakeRoundedRectSprite()
    {
        Texture2D tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
        Color[]   px  = new Color[16];
        for (int i = 0; i < 16; i++) px[i] = Color.white;
        tex.SetPixels(px);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4);
    }
}

// ─── Manager: auto-attaches PhaseCWorldItemUI to world items ─────────────────

/// <summary>
/// Scans the scene every 0.5 s.  Any active GameObject tagged "Item" that is NOT
/// inside an inventory slot gets a PhaseCWorldItemUI component added automatically.
/// </summary>
public class WorldItemEnhancerManager : MonoBehaviour
{
    private float nextCheck;
    private const float Interval = 0.5f;

    private void Update()
    {
        if (Time.time < nextCheck) return;
        nextCheck = Time.time + Interval;
        EnhanceWorldItems();
    }

    private void EnhanceWorldItems()
    {
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Item"))
        {
            if (go == null) continue;
            if (go.GetComponent<PhaseCWorldItemUI>() != null) continue;

            // Skip items that are sitting inside an inventory slot
            if (go.transform.parent != null &&
                go.transform.parent.GetComponent<Slot>() != null) continue;

            PhaseCWorldItemUI ui = go.AddComponent<PhaseCWorldItemUI>();
            ui.Initialize();
        }
    }
}
