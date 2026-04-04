using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Shows an engaging opening sequence when Minigame C starts: story context,
/// player role, and team introductions. Disables player movement until the
/// player dismisses the opening.
/// </summary>
public class PhaseCOpeningController : MonoBehaviour
{
    public static PhaseCOpeningController Instance { get; private set; }

    /// <summary>Set to true before reloading the scene to skip the opening once (e.g. on retry).</summary>
    public static bool SkipNextOpening = false;

    private const string TargetSceneName = "MinigameC";
    private const string OpeningCanvasName = "PhaseCOpeningCanvas";
    private const string PlayerPrefsOpeningSeen = "PhaseCOpeningSeen";

    // Use shared Phase C design tokens

    [Tooltip("If true, opening is shown every time the scene loads. If false, only first time (per PlayerPrefs).")]
    [SerializeField] private bool alwaysShowOpening = true;

    private GameObject openingRoot;
    private Canvas openingCanvas;
    private TMP_Text titleText;
    private TMP_Text bodyText;
    private Button continueButton;
    private TMP_Text buttonLabel;
    private List<Image> progressDots;
    private int currentPanelIndex;
    private PlayerMovement playerMovement;
    private readonly List<(string title, string body)> panels = new List<(string, string)>();
    private Texture2D gradientTexture;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void EnsureController()
    {
        SceneManager.sceneLoaded += (scene, _) =>
        {
            if (scene.name != TargetSceneName) return;
            if (FindFirstObjectByType<PhaseCOpeningController>() == null)
            {
                new GameObject("PhaseCOpeningController").AddComponent<PhaseCOpeningController>();
            }
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

        BuildPanelContent();

        // Retry path: skip opening so the player goes straight back into the game.
        if (SkipNextOpening)
        {
            SkipNextOpening = false;
            EnablePlayerMovement();
            return;
        }

        bool alreadySeen = !alwaysShowOpening && PlayerPrefs.GetInt(PlayerPrefsOpeningSeen, 0) == 1;
        if (alreadySeen)
        {
            EnablePlayerMovement();
            return;
        }

        CachePlayerMovement();
        DisablePlayerMovement();
        CreateOpeningUI();
        ShowPanel(0);
    }

    private void BuildPanelContent()
    {
        panels.Clear();
        panels.Add(("Welcome", "You're joining the Psyche mission team.\n\nThis experience follows Phase C (May 2019 to January 2021): the period when the spacecraft was designed and built to journey to Psyche, a metal-rich asteroid in the main belt between Mars and Jupiter."));
        panels.Add(("The Mission", "We're building a spacecraft to explore Psyche.\n\nYour role: work with the team through the instrument suite, spacecraft bus completion, Critical Design Review (CDR), Systems Integration Review (SIR), and Phase C approval (KDP-D)."));
        panels.Add(("How to Play", "Move with WASD or arrow keys. Talk to team members by pressing E or Space when near them.\n\nFollow the guide at the top. Complete each step by speaking to the right team member. Use your inventory (Tab) to gather and use materials."));
        panels.Add(("Your Team", "Dr. Sarah Chen - Instrument Lead\nDr. Marcus Rodriguez - Bus Lead\nDr. Priya Patel - Review Lead\nDr. James Thompson - Integration Lead"));
        panels.Add(("Welcome Aboard", "Let's build something remarkable."));
    }

    private void CachePlayerMovement()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerMovement = player.GetComponent<PlayerMovement>();
    }

    private void DisablePlayerMovement()
    {
        if (playerMovement != null)
            playerMovement.enabled = false;
    }

    private void EnablePlayerMovement()
    {
        if (playerMovement != null)
            playerMovement.enabled = true;
    }

    private static Texture2D CreateGradientTexture(int width, int height, Color top, Color bottom)
    {
        var tex = new Texture2D(width, height);
        for (int y = 0; y < height; y++)
        {
            float t = y / (float)(height - 1);
            Color c = Color.Lerp(bottom, top, t);
            for (int x = 0; x < width; x++)
                tex.SetPixel(x, y, c);
        }
        tex.Apply();
        return tex;
    }

    private void CreateOpeningUI()
    {
        openingRoot = new GameObject(OpeningCanvasName);
        openingRoot.transform.SetParent(transform);
        openingRoot.SetActive(true);

        openingCanvas = openingRoot.AddComponent<Canvas>();
        openingCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        openingCanvas.sortingOrder = 100;
        openingCanvas.enabled = true;

        CanvasScaler scaler = openingRoot.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(PhaseCUITheme.RefWidth, PhaseCUITheme.RefHeight);
        scaler.matchWidthOrHeight = PhaseCUITheme.CanvasMatchWidthOrHeight;

        openingRoot.AddComponent<GraphicRaycaster>();

        // Full-screen gradient background
        GameObject bgObject = new GameObject("OpeningBackground");
        bgObject.transform.SetParent(openingRoot.transform, false);
        Image bgImage = bgObject.AddComponent<Image>();
        gradientTexture = CreateGradientTexture(4, 256, PhaseCUITheme.BackgroundTop, PhaseCUITheme.BackgroundBottom);
        bgImage.sprite = Sprite.Create(gradientTexture, new Rect(0, 0, gradientTexture.width, gradientTexture.height), new Vector2(0.5f, 0.5f));
        bgImage.type = Image.Type.Simple;
        bgImage.color = Color.white;

        RectTransform bgRect = bgObject.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        // Mission badge at top
        GameObject badgeObject = new GameObject("MissionBadge");
        badgeObject.transform.SetParent(openingRoot.transform, false);
        TMP_Text badgeText = badgeObject.AddComponent<TextMeshProUGUI>();
        badgeText.text = "NASA PSYCHE MISSION · PHASE C";
        badgeText.fontSize = PhaseCUITheme.GetOpeningBadgeFontSize();
        badgeText.fontStyle = FontStyles.SmallCaps;
        badgeText.alignment = TextAlignmentOptions.Center;
        badgeText.color = PhaseCUITheme.AccentCyanMuted;

        RectTransform badgeRect = badgeObject.GetComponent<RectTransform>();
        badgeRect.anchorMin = new Vector2(0.05f, 1f);
        badgeRect.anchorMax = new Vector2(0.95f, 1f);
        badgeRect.pivot = new Vector2(0.5f, 1f);
        badgeRect.anchoredPosition = new Vector2(0f, -20f);
        badgeRect.sizeDelta = new Vector2(0f, 34f);

        // Panel border (frame behind panel)
        GameObject panelBorderObj = new GameObject("PanelBorder");
        panelBorderObj.transform.SetParent(openingRoot.transform, false);
        Image borderBg = panelBorderObj.AddComponent<Image>();
        borderBg.color = new Color(PhaseCUITheme.PanelBorder.r, PhaseCUITheme.PanelBorder.g, PhaseCUITheme.PanelBorder.b, 0.6f);
        RectTransform borderRect = panelBorderObj.GetComponent<RectTransform>();
        borderRect.anchorMin = new Vector2(0.1f, 0.16f);
        borderRect.anchorMax = new Vector2(0.9f, 0.8f);
        borderRect.offsetMin = new Vector2(-4f, -4f);
        borderRect.offsetMax = new Vector2(4f, 4f);

        // Center panel container
        GameObject panelObject = new GameObject("OpeningPanel");
        panelObject.transform.SetParent(openingRoot.transform, false);
        Image panelImage = panelObject.AddComponent<Image>();
        panelImage.color = PhaseCUITheme.PanelBg;

        RectTransform panelRect = panelObject.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.12f, 0.18f);
        panelRect.anchorMax = new Vector2(0.88f, 0.78f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        // Accent bar at top of panel
        GameObject accentBar = new GameObject("AccentBar");
        accentBar.transform.SetParent(panelObject.transform, false);
        Image accentImage = accentBar.AddComponent<Image>();
        accentImage.color = PhaseCUITheme.AccentGold;

        RectTransform accentRect = accentBar.GetComponent<RectTransform>();
        accentRect.anchorMin = new Vector2(0f, 1f);
        accentRect.anchorMax = new Vector2(1f, 1f);
        accentRect.pivot = new Vector2(0.5f, 1f);
        accentRect.anchoredPosition = Vector2.zero;
        accentRect.sizeDelta = new Vector2(0f, PhaseCUITheme.AccentBarHeight);

        // Title text - fixed strip below accent, centered, no overlap with body
        GameObject titleObject = new GameObject("OpeningTitle");
        titleObject.transform.SetParent(panelObject.transform, false);
        titleText = titleObject.AddComponent<TextMeshProUGUI>();
        titleText.enableWordWrapping = true;
        titleText.fontSize = PhaseCUITheme.GetOpeningTitleFontSize();
        titleText.fontStyle = FontStyles.Bold;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = PhaseCUITheme.AccentGold;
        titleText.overflowMode = TextOverflowModes.Overflow;

        RectTransform titleRect = titleObject.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.05f, 1f);
        titleRect.anchorMax = new Vector2(0.95f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0f, -PhaseCUITheme.PaddingTight);
        titleRect.sizeDelta = new Vector2(0f, PhaseCUITheme.IsMobileScreen ? 68f : 56f);

        // Body text - fills middle area only, below title and above dots/button, centered
        GameObject bodyObject = new GameObject("OpeningBody");
        bodyObject.transform.SetParent(panelObject.transform, false);
        bodyText = bodyObject.AddComponent<TextMeshProUGUI>();
        bodyText.enableWordWrapping = true;
        bodyText.fontSize = PhaseCUITheme.GetOpeningBodyFontSize();
        bodyText.lineSpacing = PhaseCUITheme.LineSpacingBody;
        bodyText.alignment = TextAlignmentOptions.Center;
        bodyText.color = PhaseCUITheme.TextBody;
        bodyText.overflowMode = TextOverflowModes.Overflow;

        RectTransform bodyRect = bodyObject.GetComponent<RectTransform>();
        bodyRect.anchorMin = new Vector2(0.05f, 0.22f);
        bodyRect.anchorMax = new Vector2(0.95f, 0.78f);
        bodyRect.pivot = new Vector2(0.5f, 0.5f);
        bodyRect.anchoredPosition = Vector2.zero;
        bodyRect.offsetMin = new Vector2(PhaseCUITheme.PaddingPanel, PhaseCUITheme.PaddingTight);
        bodyRect.offsetMax = new Vector2(-PhaseCUITheme.PaddingPanel, -PhaseCUITheme.PaddingTight);

        // Progress dots
        progressDots = new List<Image>();
        GameObject dotsContainer = new GameObject("ProgressDots");
        dotsContainer.transform.SetParent(panelObject.transform, false);
        RectTransform dotsRect = dotsContainer.AddComponent<RectTransform>();
        dotsRect.anchorMin = new Vector2(0.5f, 0f);
        dotsRect.anchorMax = new Vector2(0.5f, 0f);
        dotsRect.pivot = new Vector2(0.5f, 0f);
        dotsRect.anchoredPosition = new Vector2(0f, PhaseCUITheme.PaddingWide);
        int panelCount = panels.Count;
        dotsRect.sizeDelta = new Vector2(Mathf.Max(120f, (panelCount - 1) * PhaseCUITheme.DotSpacing + 24f), 24f);

        for (int i = 0; i < panelCount; i++)
        {
            GameObject dotObj = new GameObject("Dot" + i);
            dotObj.transform.SetParent(dotsContainer.transform, false);
            Image dotImage = dotObj.AddComponent<Image>();
            dotImage.color = PhaseCUITheme.DotInactive;
            RectTransform dotRect = dotObj.GetComponent<RectTransform>();
            dotRect.anchorMin = new Vector2(0.5f, 0.5f);
            dotRect.anchorMax = new Vector2(0.5f, 0.5f);
            dotRect.pivot = new Vector2(0.5f, 0.5f);
            float startX = -0.5f * (panelCount - 1) * PhaseCUITheme.DotSpacing;
            dotRect.anchoredPosition = new Vector2(startX + i * PhaseCUITheme.DotSpacing, 0f);
            dotRect.sizeDelta = new Vector2(PhaseCUITheme.DotSize, PhaseCUITheme.DotSize);
            progressDots.Add(dotImage);
        }

        // Continue / Begin button with glow
        GameObject buttonGlow = new GameObject("ButtonGlow");
        buttonGlow.transform.SetParent(panelObject.transform, false);
        Image glowImage = buttonGlow.AddComponent<Image>();
        glowImage.color = PhaseCUITheme.ButtonGlow;

        RectTransform glowRect = buttonGlow.GetComponent<RectTransform>();
        glowRect.anchorMin = new Vector2(0.5f, 0f);
        glowRect.anchorMax = new Vector2(0.5f, 0f);
        glowRect.pivot = new Vector2(0.5f, 0f);
        glowRect.anchoredPosition = new Vector2(0f, 32f);
        glowRect.sizeDelta = new Vector2(PhaseCUITheme.ButtonWidthMin + 24f, PhaseCUITheme.ButtonHeight + 12f);

        GameObject buttonObject = new GameObject("OpeningButton");
        buttonObject.transform.SetParent(panelObject.transform, false);
        continueButton = buttonObject.AddComponent<Button>();

        Image buttonImage = buttonObject.AddComponent<Image>();
        buttonImage.color = PhaseCUITheme.ButtonBg;

        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0f);
        buttonRect.anchorMax = new Vector2(0.5f, 0f);
        buttonRect.pivot = new Vector2(0.5f, 0f);
        buttonRect.anchoredPosition = new Vector2(0f, 36f);
        buttonRect.sizeDelta = new Vector2(PhaseCUITheme.ButtonWidthMin, PhaseCUITheme.ButtonHeight);

        // Button border accent
        GameObject buttonBorder = new GameObject("ButtonBorder");
        buttonBorder.transform.SetParent(buttonObject.transform, false);
        buttonBorder.transform.SetAsFirstSibling();
        Image borderImage = buttonBorder.AddComponent<Image>();
        borderImage.color = PhaseCUITheme.ButtonAccent;
        RectTransform buttonBorderRect = buttonBorder.GetComponent<RectTransform>();
        buttonBorderRect.anchorMin = Vector2.zero;
        buttonBorderRect.anchorMax = Vector2.one;
        buttonBorderRect.offsetMin = new Vector2(-2f, -2f);
        buttonBorderRect.offsetMax = new Vector2(2f, 2f);

        GameObject labelObject = new GameObject("ButtonLabel");
        labelObject.transform.SetParent(buttonObject.transform, false);
        buttonLabel = labelObject.AddComponent<TextMeshProUGUI>();
        buttonLabel.text = "Continue";
        buttonLabel.fontSize = PhaseCUITheme.GetOpeningButtonFontSize();
        buttonLabel.fontStyle = FontStyles.Bold;
        buttonLabel.alignment = TextAlignmentOptions.Center;
        buttonLabel.color = Color.white;

        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        // Button hover/press feedback (touch-friendly)
        ColorBlock colors = continueButton.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = PhaseCUITheme.ButtonHighlight;
        colors.pressedColor = PhaseCUITheme.ButtonPressed;
        continueButton.colors = colors;

        continueButton.onClick.AddListener(OnContinueClicked);
    }

    private void ShowPanel(int index)
    {
        if (index < 0 || index >= panels.Count)
            return;

        currentPanelIndex = index;
        var (title, body) = panels[index];
        titleText.text = title;
        bodyText.text = body;
        bodyText.alignment = TextAlignmentOptions.Center;

        bool isLastPanel = index == panels.Count - 1;
        buttonLabel.text = isLastPanel ? "Begin" : "Continue";

        for (int i = 0; i < progressDots.Count; i++)
            progressDots[i].color = i == index ? PhaseCUITheme.AccentGold : PhaseCUITheme.DotInactive;
    }

    private void OnContinueClicked()
    {
        if (currentPanelIndex < panels.Count - 1)
        {
            ShowPanel(currentPanelIndex + 1);
            return;
        }

        PlayerPrefs.SetInt(PlayerPrefsOpeningSeen, 1);
        PlayerPrefs.Save();
        CloseOpening();
    }

    private void CloseOpening()
    {
        if (gradientTexture != null)
            Destroy(gradientTexture);
        gradientTexture = null;
        if (openingRoot != null)
            Destroy(openingRoot);
        openingRoot = null;
        openingCanvas = null;
        titleText = null;
        bodyText = null;
        continueButton = null;
        buttonLabel = null;
        progressDots = null;
        EnablePlayerMovement();
    }

    private void OnDestroy()
    {
        if (gradientTexture != null)
            Destroy(gradientTexture);
        if (Instance == this)
            Instance = null;
    }
}
