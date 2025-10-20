using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameC_MainSceneBuilder : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private float orthographicSize = 5.5f;
    [SerializeField] private Color cameraBackground = new Color(0.07f, 0.09f, 0.12f);

    [Header("Layout Settings")]
    [Range(0.2f, 0.6f)]
    [SerializeField] private float leftPanelWidthRatio = 0.38f;
    [SerializeField] private float panelGap = 0.5f;
    [SerializeField] private Color leftPanelColor = new Color(0.08f, 0.13f, 0.22f, 0.95f);
    [SerializeField] private Color rightPanelColor = new Color(0.06f, 0.1f, 0.18f, 0.95f);
    [SerializeField] private Color dividerColor = new Color(0.03f, 0.05f, 0.1f, 0.85f);

    [Header("Parts Shelf")]
    [SerializeField] private int placeholderPartCount = 5;
    [SerializeField] private Vector2 partSize = new Vector2(0.85f, 0.48f);
    [SerializeField] private Color partBaseColor = new Color(0.28f, 0.62f, 0.88f);
    [SerializeField, Range(0f, 0.6f)] private float partColorVariation = 0.2f;

    [Header("Spacecraft Outline")]
    [SerializeField] private float outlineScale = 2.4f;
    [SerializeField] private float outlineWidth = 0.12f;
    [SerializeField] private Color outlineColor = new Color(0.85f, 0.65f, 0.22f);

    [Header("HUD Settings")]
    [SerializeField] private Color hudPanelColor = new Color(0.05f, 0.08f, 0.15f, 0.92f);
    [SerializeField] private TMP_FontAsset defaultFont;
    [SerializeField] private Color statsBarColor = new Color(0.04f, 0.08f, 0.16f, 0.94f);
    [SerializeField] private Color statsChipColor = new Color(0.08f, 0.14f, 0.24f, 0.96f);
    [SerializeField] private Color statsLabelColor = new Color(0.55f, 0.72f, 0.88f, 0.95f);
    [SerializeField] private Color statsValueColor = new Color(0.95f, 0.98f, 1f, 1f);
    [SerializeField] private Color statsBarHighlightTop = new Color(0.12f, 0.2f, 0.32f, 0.45f);
    [SerializeField] private Color statsBarHighlightBottom = new Color(0.02f, 0.04f, 0.08f, 0.45f);
    [SerializeField] private Color statsChipHighlightTop = new Color(0.15f, 0.26f, 0.42f, 0.4f);
    [SerializeField] private Color statsChipHighlightBottom = new Color(0.04f, 0.08f, 0.16f, 0.5f);
    [SerializeField] private Color statsChipAccentColor = new Color(0.28f, 0.68f, 0.96f, 1f);
    [SerializeField] private Color infoPanelHighlightTop = new Color(0.1f, 0.16f, 0.26f, 0.55f);
    [SerializeField] private Color infoPanelHighlightBottom = new Color(0.02f, 0.04f, 0.08f, 0.65f);
    [SerializeField] private Color accentStripeColor = new Color(0.35f, 0.68f, 0.92f, 0.85f);

    private Camera mainCamera;
    private readonly Dictionary<(Color top, Color bottom), Sprite> gradientCache = new();
    private RectTransform infoPanelRect;

    private void Awake()
    {
        ConfigureCamera();
        EnsureEventSystem();
        BuildHud();
        BuildPlaySpace();
    }

    private void ConfigureCamera()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            GameObject cameraObj = new GameObject("Main Camera");
            mainCamera = cameraObj.AddComponent<Camera>();
            cameraObj.tag = "MainCamera";
        }

        mainCamera.transform.position = new Vector3(0f, 0f, -10f);
        mainCamera.transform.rotation = Quaternion.identity;
        mainCamera.orthographic = true;
        mainCamera.orthographicSize = orthographicSize;
        mainCamera.backgroundColor = cameraBackground;
        mainCamera.clearFlags = CameraClearFlags.SolidColor;
    }

    private void EnsureEventSystem()
    {
        if (FindAnyObjectByType<EventSystem>() != null)
        {
            return;
        }

        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<StandaloneInputModule>();
    }

    private void BuildHud()
    {
        GameObject canvasObj = new GameObject("HUDCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.pixelPerfect = true;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        // Create sprites programmatically instead of using built-in resources (WebGL compatibility)
        Sprite uiSprite = CreateRoundedSprite();

        RectTransform statsBar = CreateHudPanel(canvas.transform, "StatsBar", new Vector2(0.03f, 0.91f), new Vector2(0.97f, 0.987f));
        statsBar.pivot = new Vector2(0.5f, 1f);
        Image statsBackground = statsBar.gameObject.AddComponent<Image>();
        ConfigurePanelImage(statsBackground, Color.clear, null);
        statsBackground.raycastTarget = false;

        HorizontalLayoutGroup statsLayout = statsBar.gameObject.AddComponent<HorizontalLayoutGroup>();
        statsLayout.childAlignment = TextAnchor.MiddleCenter;
        statsLayout.spacing = 36f;
        statsLayout.padding = new RectOffset(48, 48, 24, 24);
        statsLayout.childControlWidth = true;
        statsLayout.childControlHeight = true;
        statsLayout.childForceExpandWidth = true;
        statsLayout.childForceExpandHeight = true;

        // Stats header removed - cleaner design without it

        CreateStatChip(statsBar, uiSprite, "Completion", "0%");
        CreateStatChip(statsBar, uiSprite, "Modules", "0 / " + placeholderPartCount);
        CreateStatChip(statsBar, uiSprite, "Time", "00:00");

        CreateMissionBriefPopup(canvas.transform, uiSprite);
    }

    private RectTransform CreateHudPanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform));
        panel.transform.SetParent(parent, false);
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return rect;
    }

    private TMP_Text CreateHudText(Transform parent, string content)
    {
        GameObject textObj = new GameObject("Text", typeof(RectTransform));
        textObj.transform.SetParent(parent, false);
        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        if (defaultFont != null)
        {
            tmp.font = defaultFont;
        }

        tmp.text = content;
        tmp.color = Color.white;
        tmp.enableWordWrapping = true;
        tmp.rectTransform.anchorMin = Vector2.zero;
        tmp.rectTransform.anchorMax = Vector2.one;
        tmp.rectTransform.offsetMin = Vector2.zero;
        tmp.rectTransform.offsetMax = Vector2.zero;
        return tmp;
    }

    private void ConfigurePanelImage(Image image, Color color, Sprite sprite)
    {
        image.color = color;
        if (sprite != null)
        {
            image.sprite = sprite;
            image.type = Image.Type.Sliced;
        }
    }

    private Image AddGradientOverlay(RectTransform parent, string name, Color topColor, Color bottomColor, float alphaMultiplier = 1f)
    {
        RectTransform rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.SetAsFirstSibling();

        LayoutElement layoutElement = rect.gameObject.AddComponent<LayoutElement>();
        layoutElement.ignoreLayout = true;

        Image image = rect.gameObject.AddComponent<Image>();
        image.sprite = GetGradientSprite(topColor, bottomColor);
        image.type = Image.Type.Simple;
        image.preserveAspect = false;
        image.color = new Color(1f, 1f, 1f, alphaMultiplier);
        image.raycastTarget = false;
        return image;
    }

    private Image CreateAccentStripe(Transform parent, string name, Color color, float height, Vector2 anchorMin, Vector2 anchorMax, float leftInset = 0f, float rightInset = 0f)
    {
        RectTransform rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = new Vector2(leftInset, -height);
        rect.offsetMax = new Vector2(-rightInset, 0f);
        rect.SetSiblingIndex(1);

        LayoutElement layoutElement = rect.gameObject.AddComponent<LayoutElement>();
        layoutElement.ignoreLayout = true;

        Image stripe = rect.gameObject.AddComponent<Image>();
        stripe.color = color;
        stripe.raycastTarget = false;
        return stripe;
    }

    private void CreateMissionBriefPopup(Transform canvasTransform, Sprite uiSprite)
    {
        // Create background overlay (semi-transparent dark background)
        GameObject overlayObj = new GameObject("MissionBriefOverlay", typeof(RectTransform));
        overlayObj.transform.SetParent(canvasTransform, false);
        RectTransform overlayRect = overlayObj.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;

        Image overlayImage = overlayObj.AddComponent<Image>();
        overlayImage.color = new Color(0f, 0f, 0f, 0.75f);
        overlayImage.raycastTarget = true; // Block clicks behind popup

        // Create popup panel (centered)
        infoPanelRect = new GameObject("MissionBriefPopup", typeof(RectTransform)).GetComponent<RectTransform>();
        infoPanelRect.SetParent(overlayRect, false);
        infoPanelRect.anchorMin = new Vector2(0.5f, 0.5f);
        infoPanelRect.anchorMax = new Vector2(0.5f, 0.5f);
        infoPanelRect.pivot = new Vector2(0.5f, 0.5f);
        infoPanelRect.anchoredPosition = Vector2.zero;
        infoPanelRect.sizeDelta = new Vector2(900f, 500f);

        Image popupBackground = infoPanelRect.gameObject.AddComponent<Image>();
        ConfigurePanelImage(popupBackground, new Color(0.06f, 0.1f, 0.18f, 0.98f), uiSprite);

        AddGradientOverlay(infoPanelRect, "PopupGradient", infoPanelHighlightTop, infoPanelHighlightBottom, 1f);
        CreateAccentStripe(infoPanelRect, "PopupAccentTop", accentStripeColor, 5f, new Vector2(0f, 1f), new Vector2(1f, 1f), 0f, 0f);

        Shadow popupShadow = infoPanelRect.gameObject.AddComponent<Shadow>();
        popupShadow.effectColor = new Color(0f, 0f, 0f, 0.8f);
        popupShadow.effectDistance = new Vector2(0f, 10f);

        // Create header
        RectTransform header = new GameObject("Header", typeof(RectTransform)).GetComponent<RectTransform>();
        header.SetParent(infoPanelRect, false);
        header.anchorMin = new Vector2(0f, 1f);
        header.anchorMax = new Vector2(1f, 1f);
        header.pivot = new Vector2(0.5f, 1f);
        header.anchoredPosition = Vector2.zero;
        header.sizeDelta = new Vector2(0f, 90f);

        Image headerBg = header.gameObject.AddComponent<Image>();
        headerBg.color = new Color(0.04f, 0.08f, 0.14f, 0.9f);
        headerBg.raycastTarget = false;

        // Create separate child for header text
        GameObject headerTextObj = new GameObject("HeaderText", typeof(RectTransform));
        headerTextObj.transform.SetParent(header, false);
        TMP_Text headerText = headerTextObj.AddComponent<TextMeshProUGUI>();
        if (defaultFont != null)
        {
            headerText.font = defaultFont;
        }
        headerText.text = "MISSION BRIEF";
        headerText.fontSize = 48f;
        headerText.fontStyle = FontStyles.Bold;
        headerText.alignment = TextAlignmentOptions.Center;
        headerText.color = new Color(0.9f, 0.95f, 1f, 1f);
        headerText.raycastTarget = false;
        headerText.rectTransform.anchorMin = Vector2.zero;
        headerText.rectTransform.anchorMax = Vector2.one;
        headerText.rectTransform.offsetMin = new Vector2(50f, 0f);
        headerText.rectTransform.offsetMax = new Vector2(-50f, 0f);

        // Create content area
        RectTransform content = new GameObject("Content", typeof(RectTransform)).GetComponent<RectTransform>();
        content.SetParent(infoPanelRect, false);
        content.anchorMin = new Vector2(0f, 0f);
        content.anchorMax = new Vector2(1f, 1f);
        content.pivot = new Vector2(0.5f, 0.5f);
        content.anchoredPosition = Vector2.zero;
        content.offsetMin = new Vector2(50f, 100f);  // Bottom offset for "Got It" button
        content.offsetMax = new Vector2(-50f, -110f); // Top offset for header

        TMP_Text contentText = content.gameObject.AddComponent<TextMeshProUGUI>();
        if (defaultFont != null)
        {
            contentText.font = defaultFont;
        }
        contentText.text = "Drag parts from the left shelf into the outline on the right.\n\n" +
                          "Align each module carefully until it snaps into place.\n\n" +
                          "Complete all 5 modules to finish the spacecraft assembly.";
        contentText.fontSize = 34f;
        contentText.fontStyle = FontStyles.Normal;
        contentText.alignment = TextAlignmentOptions.Center;
        contentText.color = new Color(0.85f, 0.92f, 1f, 0.95f);
        contentText.lineSpacing = 1.4f;
        contentText.enableWordWrapping = true;
        contentText.raycastTarget = false;
        contentText.rectTransform.anchorMin = Vector2.zero;
        contentText.rectTransform.anchorMax = Vector2.one;
        contentText.rectTransform.offsetMin = Vector2.zero;
        contentText.rectTransform.offsetMax = Vector2.zero;

        // Create close button
        CreateInfoPanelCloseButton(infoPanelRect, uiSprite);

        // Create "Got It" button at bottom
        RectTransform gotItButton = new GameObject("GotItButton", typeof(RectTransform)).GetComponent<RectTransform>();
        gotItButton.SetParent(infoPanelRect, false);
        gotItButton.anchorMin = new Vector2(0.5f, 0f);
        gotItButton.anchorMax = new Vector2(0.5f, 0f);
        gotItButton.pivot = new Vector2(0.5f, 0f);
        gotItButton.anchoredPosition = new Vector2(0f, 20f);
        gotItButton.sizeDelta = new Vector2(250f, 60f);

        Button button = gotItButton.gameObject.AddComponent<Button>();
        Image buttonBg = gotItButton.gameObject.AddComponent<Image>();
        ConfigurePanelImage(buttonBg, new Color(0.25f, 0.65f, 0.92f, 1f), uiSprite);
        buttonBg.raycastTarget = true;

        AddGradientOverlay(gotItButton, "ButtonGradient",
            new Color(0.3f, 0.7f, 1f, 0.4f),
            new Color(0.15f, 0.4f, 0.7f, 0.4f), 1f);

        button.targetGraphic = buttonBg;
        button.transition = Selectable.Transition.ColorTint;
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.15f, 1.15f, 1.15f, 1f);
        colors.pressedColor = new Color(0.85f, 0.9f, 1f, 1f);
        colors.selectedColor = colors.normalColor;
        colors.disabledColor = new Color(0.7f, 0.7f, 0.7f, 0.5f);
        colors.fadeDuration = 0.15f;
        button.colors = colors;

        Shadow buttonShadow = gotItButton.gameObject.AddComponent<Shadow>();
        buttonShadow.effectColor = new Color(0f, 0f, 0f, 0.5f);
        buttonShadow.effectDistance = new Vector2(0f, -4f);

        // Create separate child for button text
        GameObject buttonTextObj = new GameObject("ButtonText", typeof(RectTransform));
        buttonTextObj.transform.SetParent(gotItButton, false);
        TMP_Text buttonText = buttonTextObj.AddComponent<TextMeshProUGUI>();
        if (defaultFont != null)
        {
            buttonText.font = defaultFont;
        }
        buttonText.text = "GOT IT!";
        buttonText.fontSize = 38f;
        buttonText.fontStyle = FontStyles.Bold;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.color = new Color(1f, 1f, 1f, 1f);
        buttonText.raycastTarget = false;
        buttonText.rectTransform.anchorMin = Vector2.zero;
        buttonText.rectTransform.anchorMax = Vector2.one;
        buttonText.rectTransform.offsetMin = Vector2.zero;
        buttonText.rectTransform.offsetMax = Vector2.zero;

        button.onClick.AddListener(CloseInfoPanel);

        // Store reference to overlay for closing
        infoPanelRect = overlayRect;
    }

    private void CreateInfoPanelCloseButton(RectTransform infoPanel, Sprite buttonSprite)
    {
        RectTransform buttonRect = new GameObject("CloseButton", typeof(RectTransform)).GetComponent<RectTransform>();
        buttonRect.SetParent(infoPanel, false);
        buttonRect.anchorMin = new Vector2(1f, 1f);
        buttonRect.anchorMax = new Vector2(1f, 1f);
        buttonRect.pivot = new Vector2(1f, 1f);
        buttonRect.anchoredPosition = new Vector2(-28f, -22f);
        buttonRect.sizeDelta = new Vector2(48f, 48f);

        LayoutElement layoutElement = buttonRect.gameObject.AddComponent<LayoutElement>();
        layoutElement.ignoreLayout = true;

        // Add button FIRST before image
        Button button = buttonRect.gameObject.AddComponent<Button>();

        Image buttonImage = buttonRect.gameObject.AddComponent<Image>();
        ConfigurePanelImage(buttonImage, new Color(0.18f, 0.29f, 0.45f, 0.95f), buttonSprite);
        buttonImage.raycastTarget = true; // Ensure it can receive clicks

        AddGradientOverlay(buttonRect, "CloseButtonGradient", statsChipHighlightTop, statsChipHighlightBottom, 1f);

        button.targetGraphic = buttonImage;
        button.transition = Selectable.Transition.ColorTint;
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f);
        colors.pressedColor = new Color(0.85f, 0.9f, 1f, 1f);
        colors.selectedColor = colors.normalColor;
        colors.disabledColor = new Color(0.7f, 0.7f, 0.7f, 0.5f);
        colors.fadeDuration = 0.15f;
        button.colors = colors;

        Shadow shadow = buttonRect.gameObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.4f);
        shadow.effectDistance = new Vector2(0f, -3f);

        // Create separate child GameObject for the text label
        GameObject labelObj = new GameObject("Label", typeof(RectTransform));
        labelObj.transform.SetParent(buttonRect, false);
        TextMeshProUGUI label = labelObj.AddComponent<TextMeshProUGUI>();
        if (defaultFont != null)
        {
            label.font = defaultFont;
        }

        label.text = "X";
        label.fontSize = 32f;
        label.fontStyle = FontStyles.Bold;
        label.alignment = TextAlignmentOptions.Center;
        label.color = new Color(0.92f, 0.97f, 1f, 0.96f);
        label.raycastTarget = false;
        label.rectTransform.anchorMin = Vector2.zero;
        label.rectTransform.anchorMax = Vector2.one;
        label.rectTransform.offsetMin = Vector2.zero;
        label.rectTransform.offsetMax = Vector2.zero;

        button.onClick.AddListener(CloseInfoPanel);

        Debug.Log("Close button created with click listener attached");
    }

    private void CloseInfoPanel()
    {
        if (infoPanelRect != null)
        {
            infoPanelRect.gameObject.SetActive(false);
        }
    }

    private void CreateStatChip(Transform parent, Sprite panelSprite, string label, string value)
    {
        GameObject chip = new GameObject(label + "Chip", typeof(RectTransform));
        chip.transform.SetParent(parent, false);
        RectTransform rect = chip.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);

        Image background = chip.AddComponent<Image>();
        ConfigurePanelImage(background, statsChipColor, panelSprite);
        AddGradientOverlay(rect, "ChipGradient", statsChipHighlightTop, statsChipHighlightBottom, 1f);

        Shadow chipShadow = chip.AddComponent<Shadow>();
        chipShadow.effectColor = new Color(0f, 0f, 0f, 0.5f);
        chipShadow.effectDistance = new Vector2(0f, -5f);

        LayoutElement layout = chip.AddComponent<LayoutElement>();
        layout.preferredWidth = 0f;
        layout.flexibleWidth = 1f;
        layout.preferredHeight = 100f;

        RectTransform content = new GameObject("Content", typeof(RectTransform)).GetComponent<RectTransform>();
        content.SetParent(chip.transform, false);
        content.anchorMin = Vector2.zero;
        content.anchorMax = Vector2.one;
        content.offsetMin = new Vector2(28f, 0f);
        content.offsetMax = new Vector2(-28f, 0f);

        TMP_Text combinedText = CreateHudText(content, $"<color=#{ColorUtility.ToHtmlStringRGB(statsLabelColor)}>{label.ToUpperInvariant()}:</color> <color=#{ColorUtility.ToHtmlStringRGB(statsValueColor)}>{value}</color>");
        combinedText.richText = true;
        combinedText.fontSize = 40f;
        combinedText.fontStyle = FontStyles.Bold;
        combinedText.alignment = TextAlignmentOptions.Center;
        combinedText.enableWordWrapping = false;
        combinedText.margin = Vector4.zero;
        combinedText.characterSpacing = 1f;
    }

    private TMP_Text CreateLayoutText(Transform parent, string content)
    {
        GameObject textObj = new GameObject("Text", typeof(RectTransform));
        textObj.transform.SetParent(parent, false);
        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        if (defaultFont != null)
        {
            tmp.font = defaultFont;
        }

        tmp.text = content;
        tmp.color = Color.white;
        tmp.enableWordWrapping = false;

        RectTransform rect = tmp.rectTransform;
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return tmp;
    }

    private Sprite CreateRoundedSprite()
    {
        const int size = 64;
        const int border = 8;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear,
            hideFlags = HideFlags.HideAndDontSave,
            name = "RoundedSprite"
        };

        Color clear = new Color(1f, 1f, 1f, 0f);
        Color white = Color.white;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                // Distance from edge for rounded corners
                float dx = Mathf.Max(0, border - x, x - (size - border - 1));
                float dy = Mathf.Max(0, border - y, y - (size - border - 1));
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                float alpha = dist < border ? 1f : 0f;
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        texture.Apply(false, true);

        return Sprite.Create(
            texture,
            new Rect(0f, 0f, size, size),
            new Vector2(0.5f, 0.5f),
            100f,
            0,
            SpriteMeshType.FullRect,
            new Vector4(border, border, border, border)
        );
    }

    private Sprite GetGradientSprite(Color topColor, Color bottomColor)
    {
        (Color top, Color bottom) key = (topColor, bottomColor);
        if (gradientCache.TryGetValue(key, out Sprite cachedSprite) && cachedSprite != null)
        {
            return cachedSprite;
        }

        const int textureHeight = 64;
        Texture2D texture = new Texture2D(1, textureHeight, TextureFormat.RGBA32, false)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear,
            hideFlags = HideFlags.HideAndDontSave,
            name = $"Gradient_{topColor}_{bottomColor}"
        };

        for (int y = 0; y < textureHeight; y++)
        {
            float t = y / (textureHeight - 1f);
            Color color = Color.Lerp(bottomColor, topColor, t);
            texture.SetPixel(0, y, color);
        }

        texture.Apply(false, true);

        Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, textureHeight), new Vector2(0.5f, 0.5f), textureHeight, 0, SpriteMeshType.FullRect);
        sprite.hideFlags = HideFlags.HideAndDontSave;
        gradientCache[key] = sprite;
        return sprite;
    }

    private void BuildPlaySpace()
    {
        float halfHeight = mainCamera.orthographicSize;
        float halfWidth = halfHeight * mainCamera.aspect;
        float totalWidth = halfWidth * 2f;

        float effectiveGap = Mathf.Clamp(panelGap, 0.2f, totalWidth * 0.2f);
        float leftWidth = totalWidth * Mathf.Clamp01(leftPanelWidthRatio);
        float rightWidth = totalWidth - leftWidth - effectiveGap;
        rightWidth = Mathf.Max(rightWidth, totalWidth * 0.25f);

        float leftCenterX = -halfWidth + (leftWidth * 0.5f);
        float rightCenterX = halfWidth - (rightWidth * 0.5f);
        float panelHeight = halfHeight * 1.8f;

        BuildPanelBackdrop("PartsShelf", leftCenterX, panelHeight, leftWidth, leftPanelColor);
        BuildPanelBackdrop("AssemblyPad", rightCenterX, panelHeight, rightWidth, rightPanelColor);
        BuildPanelDivider(leftCenterX + (leftWidth * 0.5f) + (effectiveGap * 0.5f), panelHeight, Mathf.Max(effectiveGap * 0.45f, 0.2f));

        PopulatePartsShelf(leftCenterX, panelHeight, leftWidth);
        BuildSpacecraftOutline(rightCenterX);
    }

    private void BuildPanelDivider(float centerX, float height, float width)
    {
        GameObject divider = GameObject.CreatePrimitive(PrimitiveType.Quad);
        divider.name = "PanelDivider";
        divider.transform.SetParent(transform, false);
        divider.transform.position = new Vector3(centerX, 0f, 1.25f);
        divider.transform.localScale = new Vector3(width, height * 1.02f, 1f);

        Shader shader = Shader.Find("Unlit/Color");
        if (shader == null)
        {
            shader = Shader.Find("Sprites/Default");
        }

        Material material = new Material(shader);
        material.color = dividerColor;
        MeshRenderer renderer = divider.GetComponent<MeshRenderer>();
        renderer.material = material;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;

        Collider collider = divider.GetComponent<Collider>();
        if (collider != null)
        {
            Destroy(collider);
        }

        GameObject glow = new GameObject("DividerGlow");
        glow.transform.SetParent(divider.transform, false);
        LineRenderer line = glow.AddComponent<LineRenderer>();
        line.useWorldSpace = false;
        line.loop = false;
        line.positionCount = 2;
        line.SetPosition(0, new Vector3(0f, height * 0.48f, 0f));
        line.SetPosition(1, new Vector3(0f, -height * 0.48f, 0f));
        line.widthMultiplier = Mathf.Max(width * 0.18f, 0.04f);
        line.numCapVertices = 8;
        line.numCornerVertices = 3;

        Shader lineShader = Shader.Find("Sprites/Default");
        if (lineShader == null)
        {
            lineShader = Shader.Find("Unlit/Color");
        }

        line.material = new Material(lineShader);
        Color glowColor = new Color(accentStripeColor.r, accentStripeColor.g, accentStripeColor.b, 0.6f);
        line.startColor = glowColor;
        line.endColor = glowColor;
        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        line.receiveShadows = false;
    }

    private void BuildPanelBackdrop(string name, float centerX, float height, float width, Color color)
    {
        GameObject backdrop = GameObject.CreatePrimitive(PrimitiveType.Quad);
        backdrop.name = name + "_Backdrop";
        backdrop.transform.SetParent(transform, false);
        backdrop.transform.position = new Vector3(centerX, 0f, 1.5f);
        backdrop.transform.localScale = new Vector3(width, height, 1f);
        backdrop.transform.rotation = Quaternion.Euler(0f, 0f, 0f);

        Shader shader = Shader.Find("Unlit/Color");
        if (shader == null)
        {
            shader = Shader.Find("Sprites/Default");
        }

        Material material = new Material(shader);
        material.color = color;
        MeshRenderer renderer = backdrop.GetComponent<MeshRenderer>();
        renderer.material = material;

        Collider backdropCollider = backdrop.GetComponent<Collider>();
        if (backdropCollider != null)
        {
            Destroy(backdropCollider);
        }

        GameObject highlight = GameObject.CreatePrimitive(PrimitiveType.Quad);
        highlight.name = name + "_Highlight";
        highlight.transform.SetParent(backdrop.transform, false);
        highlight.transform.localPosition = new Vector3(0f, -height * 0.55f, -0.02f);
        highlight.transform.localScale = new Vector3(width * 0.92f, height * 0.3f, 1f);

        MeshRenderer highlightRenderer = highlight.GetComponent<MeshRenderer>();
        Material highlightMaterial = new Material(shader);
        Color highlightColor = Color.Lerp(color, Color.white, 0.12f);
        highlightColor.a = Mathf.Clamp01(color.a * 0.5f);
        highlightMaterial.color = highlightColor;
        highlightRenderer.material = highlightMaterial;
        highlightRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        highlightRenderer.receiveShadows = false;

        Collider highlightCollider = highlight.GetComponent<Collider>();
        if (highlightCollider != null)
        {
            Destroy(highlightCollider);
        }
    }

    private void PopulatePartsShelf(float centerX, float height, float width)
    {
        GameObject shelfRoot = new GameObject("PartsShelf");
        shelfRoot.transform.SetParent(transform, false);
        shelfRoot.transform.position = new Vector3(centerX, 0f, 0f);

        float usableHeight = height * 0.8f;
        float top = usableHeight * 0.5f;
        float spacing = placeholderPartCount > 1 ? usableHeight / (placeholderPartCount - 1) : 0f;

        for (int i = 0; i < placeholderPartCount; i++)
        {
            PrimitiveType primitiveType = partPrimitiveOptions[i % partPrimitiveOptions.Length];
            GameObject part = GameObject.CreatePrimitive(primitiveType);
            part.name = $"PlaceholderPart_{i + 1}";
            part.transform.SetParent(shelfRoot.transform, false);

            float y = top - (spacing * i);
            part.transform.localPosition = new Vector3(0f, y, 0f);

            Vector3 baseScale = new Vector3(partSize.x, partSize.y, partSize.x * 0.55f);
            switch (primitiveType)
            {
                case PrimitiveType.Cylinder:
                    baseScale = new Vector3(partSize.x * 0.85f, partSize.y * 1.4f, partSize.x * 0.85f);
                    break;
                case PrimitiveType.Capsule:
                    baseScale = new Vector3(partSize.x * 0.75f, partSize.y * 1.45f, partSize.x * 0.75f);
                    break;
            }

            part.transform.localScale = baseScale;
            part.transform.localRotation = Quaternion.Euler(0f, Mathf.Lerp(-18f, 18f, i / Mathf.Max(1f, placeholderPartCount - 1f)), 0f);

            MeshRenderer renderer = part.GetComponent<MeshRenderer>();
            Shader shader = Shader.Find("Standard");
            if (shader == null)
            {
                shader = Shader.Find("Unlit/Color");
            }

            Material material = new Material(shader);
            Color paletteColor = partPalette.Length > 0 ? partPalette[i % partPalette.Length] : partBaseColor;
            float variation = Random.Range(-partColorVariation, partColorVariation);
            float blend = Mathf.Clamp01(0.5f + variation);
            Color finalColor = Color.Lerp(paletteColor, partBaseColor, blend);
            material.color = finalColor;
            if (material.HasProperty("_Glossiness"))
            {
                material.SetFloat("_Glossiness", 0.22f);
            }
            if (material.HasProperty("_Metallic"))
            {
                material.SetFloat("_Metallic", 0.15f);
            }
            renderer.material = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;

            Collider collider = part.GetComponent<Collider>();
            if (collider != null)
            {
                collider.isTrigger = true;
            }
        }
    }

    private void BuildSpacecraftOutline(float centerX)
    {
        GameObject outlineRoot = new GameObject("SpacecraftOutline");
        outlineRoot.transform.SetParent(transform, false);
        outlineRoot.transform.position = new Vector3(centerX, 0f, 0f);

        LineRenderer lineRenderer = outlineRoot.AddComponent<LineRenderer>();
        lineRenderer.useWorldSpace = false;
        lineRenderer.loop = true;
        lineRenderer.positionCount = spacecraftShape.Count;
        lineRenderer.widthMultiplier = outlineWidth;
        lineRenderer.numCornerVertices = 5;
        lineRenderer.numCapVertices = 5;
        Shader outlineShader = Shader.Find("Sprites/Default");
        if (outlineShader == null)
        {
            outlineShader = Shader.Find("Unlit/Color");
        }

        lineRenderer.material = new Material(outlineShader);
        lineRenderer.startColor = outlineColor;
        lineRenderer.endColor = outlineColor;

        for (int i = 0; i < spacecraftShape.Count; i++)
        {
            lineRenderer.SetPosition(i, spacecraftShape[i] * outlineScale);
        }

        GameObject placeholderBody = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        placeholderBody.name = "SpacecraftCore";
        placeholderBody.transform.SetParent(outlineRoot.transform, false);
        placeholderBody.transform.localPosition = Vector3.zero;
        placeholderBody.transform.localScale = new Vector3(outlineScale * 0.5f, outlineScale, outlineScale * 0.5f);

        MeshRenderer renderer = placeholderBody.GetComponent<MeshRenderer>();
        Shader shader = Shader.Find("Standard");
        if (shader == null)
        {
            shader = Shader.Find("Unlit/Color");
        }

        Material material = new Material(shader);
        material.color = new Color(0.8f, 0.82f, 0.86f, 0.6f);
        renderer.material = material;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;

        Collider collider = placeholderBody.GetComponent<Collider>();
        if (collider != null)
        {
            collider.isTrigger = true;
        }
    }

    private static readonly Color[] partPalette =
    {
        new Color(0.26f, 0.6f, 0.88f),
        new Color(0.3f, 0.68f, 0.92f),
        new Color(0.22f, 0.55f, 0.82f),
        new Color(0.28f, 0.64f, 0.9f)
    };

    private static readonly PrimitiveType[] partPrimitiveOptions =
    {
        PrimitiveType.Cube,
        PrimitiveType.Cylinder,
        PrimitiveType.Capsule
    };

    private static readonly List<Vector3> spacecraftShape = new List<Vector3>
    {
        new Vector3(0f, 1.2f, 0f),
        new Vector3(0.55f, 0.8f, 0f),
        new Vector3(0.75f, 0.25f, 0f),
        new Vector3(0.45f, -0.9f, 0f),
        new Vector3(0f, -1.4f, 0f),
        new Vector3(-0.45f, -0.9f, 0f),
        new Vector3(-0.75f, 0.25f, 0f),
        new Vector3(-0.55f, 0.8f, 0f)
    };
}
