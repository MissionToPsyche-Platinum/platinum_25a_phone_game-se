using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Enhances the appearance of items dropped in the world.
/// Adds glow effect, pulsing animation, floating label, and increases visual size.
/// Auto-attaches to spawned items.
/// </summary>
public class PhaseCWorldItemUI : MonoBehaviour
{
    private const string TargetSceneName = "MinigameC";

    [Header("Visual Settings")]
    [SerializeField] private float worldScale = 1.8f;
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float pulseAmount = 0.12f;
    [SerializeField] private float bobSpeed = 1.5f;
    [SerializeField] private float bobAmount = 0.15f;

    private SpriteRenderer mainSprite;
    private SpriteRenderer glowSprite;
    private GameObject labelObject;
    private TextMesh labelText;
    private Item itemComponent;
    private Vector3 startPosition;
    private float timeOffset;
    private bool isInitialized;

    private static Material glowMaterial;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void SetupItemEnhancer()
    {
        if (SceneManager.GetActiveScene().name != TargetSceneName) return;

        // Create a manager to watch for new items
        GameObject manager = new GameObject("WorldItemEnhancer");
        manager.AddComponent<WorldItemEnhancerManager>();
    }

    public void Initialize()
    {
        if (isInitialized) return;

        itemComponent = GetComponent<Item>();
        mainSprite = GetComponent<SpriteRenderer>();

        if (itemComponent == null || mainSprite == null)
        {
            Destroy(this);
            return;
        }

        timeOffset = Random.value * Mathf.PI * 2f;
        startPosition = transform.position;

        // Scale up the item
        transform.localScale = Vector3.one * worldScale;

        // Create glow effect
        CreateGlowEffect();

        // Create floating label
        CreateLabel();

        // Set item color based on type
        ApplyItemColor();

        isInitialized = true;
    }

    private void CreateGlowEffect()
    {
        GameObject glowGo = new GameObject("Glow");
        glowGo.transform.SetParent(transform, false);
        glowGo.transform.localPosition = Vector3.zero;
        glowGo.transform.localScale = Vector3.one * 2.5f;

        glowSprite = glowGo.AddComponent<SpriteRenderer>();
        glowSprite.sprite = CreateGlowSprite();
        glowSprite.sortingOrder = mainSprite.sortingOrder - 1;
        glowSprite.color = GetGlowColor(itemComponent.ID);
    }

    private void CreateLabel()
    {
        labelObject = new GameObject("Label");
        labelObject.transform.SetParent(transform, false);
        labelObject.transform.localPosition = new Vector3(0f, 0.8f, 0f);
        labelObject.transform.localScale = Vector3.one * 0.4f;

        labelText = labelObject.AddComponent<TextMesh>();
        labelText.text = GetDisplayName();
        labelText.fontSize = 32;
        labelText.characterSize = 0.15f;
        labelText.anchor = TextAnchor.MiddleCenter;
        labelText.alignment = TextAlignment.Center;
        labelText.color = Color.white;
        labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // Add renderer settings
        MeshRenderer labelRenderer = labelObject.GetComponent<MeshRenderer>();
        if (labelRenderer != null)
        {
            labelRenderer.sortingOrder = mainSprite.sortingOrder + 10;
        }

        // Background for label
        GameObject bgGo = new GameObject("LabelBg");
        bgGo.transform.SetParent(labelObject.transform, false);
        bgGo.transform.localPosition = new Vector3(0f, 0f, 0.01f);

        SpriteRenderer bgSprite = bgGo.AddComponent<SpriteRenderer>();
        bgSprite.sprite = CreateLabelBgSprite();
        bgSprite.color = new Color(0.05f, 0.08f, 0.15f, 0.85f);
        bgSprite.sortingOrder = mainSprite.sortingOrder + 9;

        float textWidth = labelText.text.Length * 0.08f + 0.3f;
        bgGo.transform.localScale = new Vector3(textWidth, 0.35f, 1f);
    }

    private string GetDisplayName()
    {
        if (itemComponent != null && !string.IsNullOrEmpty(itemComponent.displayName))
        {
            return itemComponent.displayName;
        }

        // Fallback names based on ID
        switch (itemComponent?.ID)
        {
            case 1: return "Metal Alloy";
            case 2: return "Wiring";
            case 3: return "Solar Cells";
            case 4: return "Insulation";
            case 5: return "Magnetometer";
            case 6: return "Camera Sensor";
            case 7: return "Spectrometer";
            case 8: return "Circuit Board";
            case 9: return "Battery";
            case 10: return "Thermal Blanket";
            case 11: return "Battery";
            case 12: return "Radio Antenna";
            case 13: return "Laser Module";
            case 14: return "Navigation";
            case 15: return "Propellant";
            default: return $"Item {itemComponent?.ID}";
        }
    }

    private void ApplyItemColor()
    {
        if (mainSprite == null || itemComponent == null) return;

        Color itemColor = GetItemColor(itemComponent.ID);
        mainSprite.color = itemColor;
    }

    private Color GetItemColor(int itemId)
    {
        switch (itemId)
        {
            case 1: return new Color(0.7f, 0.75f, 0.9f, 1f);
            case 2: return new Color(0.95f, 0.7f, 0.35f, 1f);
            case 3: return new Color(0.4f, 0.6f, 0.95f, 1f);
            case 4: return new Color(0.75f, 0.65f, 0.85f, 1f);
            case 5: return new Color(0.5f, 0.9f, 0.7f, 1f);
            case 6: return new Color(0.4f, 0.8f, 0.9f, 1f);
            case 7: return new Color(0.6f, 0.95f, 0.6f, 1f);
            case 8: return new Color(0.3f, 0.8f, 0.5f, 1f);
            case 9: return new Color(0.95f, 0.85f, 0.3f, 1f);
            case 10: return new Color(0.85f, 0.85f, 0.9f, 1f);
            case 11: return new Color(0.8f, 0.6f, 0.95f, 1f);
            case 12: return new Color(0.95f, 0.7f, 0.8f, 1f);
            case 13: return new Color(0.9f, 0.5f, 0.95f, 1f);
            case 14: return new Color(0.7f, 0.8f, 0.98f, 1f);
            case 15: return new Color(0.98f, 0.6f, 0.55f, 1f);
            default: return new Color(0.7f, 0.85f, 0.95f, 1f);
        }
    }

    private Color GetGlowColor(int itemId)
    {
        Color baseColor = GetItemColor(itemId);
        return new Color(baseColor.r, baseColor.g, baseColor.b, 0.35f);
    }

    private void Update()
    {
        if (!isInitialized) return;

        float time = Time.time + timeOffset;

        // Pulsing scale
        float pulse = 1f + Mathf.Sin(time * pulseSpeed) * pulseAmount;
        transform.localScale = Vector3.one * worldScale * pulse;

        // Bobbing motion
        float bob = Mathf.Sin(time * bobSpeed) * bobAmount;
        transform.position = startPosition + new Vector3(0f, bob, 0f);

        // Glow pulsing
        if (glowSprite != null)
        {
            float glowAlpha = 0.25f + Mathf.Sin(time * pulseSpeed * 0.7f) * 0.15f;
            Color glowColor = glowSprite.color;
            glowColor.a = glowAlpha;
            glowSprite.color = glowColor;
        }

        // Keep label upright and facing camera
        if (labelObject != null)
        {
            labelObject.transform.rotation = Quaternion.identity;
        }
    }

    private static Sprite CreateGlowSprite()
    {
        int size = 64;
        Texture2D tex = new Texture2D(size, size);
        Color clear = new Color(1f, 1f, 1f, 0f);
        float center = size / 2f;
        float maxDist = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                float alpha = 1f - Mathf.Clamp01(dist / maxDist);
                alpha = alpha * alpha; // Quadratic falloff
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha * 0.6f));
            }
        }

        tex.Apply();
        tex.filterMode = FilterMode.Bilinear;
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 64f);
    }

    private static Sprite CreateLabelBgSprite()
    {
        int w = 32;
        int h = 16;
        Texture2D tex = new Texture2D(w, h);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                tex.SetPixel(x, y, Color.white);
            }
        }

        tex.Apply();
        tex.filterMode = FilterMode.Point;
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 16f);
    }
}

/// <summary>
/// Watches for new items in the scene and adds the world item UI enhancement.
/// </summary>
public class WorldItemEnhancerManager : MonoBehaviour
{
    private float checkInterval = 0.5f;
    private float nextCheck;

    private void Update()
    {
        if (Time.time < nextCheck) return;
        nextCheck = Time.time + checkInterval;

        EnhanceNewItems();
    }

    private void EnhanceNewItems()
    {
        GameObject[] items = GameObject.FindGameObjectsWithTag("Item");
        foreach (GameObject itemGo in items)
        {
            if (itemGo == null) continue;
            if (itemGo.GetComponent<PhaseCWorldItemUI>() != null) continue;

            // Only enhance items in the world, not in inventory
            if (itemGo.transform.parent != null)
            {
                // Check if parent is a slot (inventory item)
                Slot slot = itemGo.transform.parent.GetComponent<Slot>();
                if (slot != null) continue;
            }

            PhaseCWorldItemUI enhancer = itemGo.AddComponent<PhaseCWorldItemUI>();
            enhancer.Initialize();
        }
    }
}
