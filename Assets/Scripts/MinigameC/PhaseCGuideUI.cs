using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Production-ready step-by-step guide: progress strip, current objective,
/// who to talk to, controls hint, and step-complete feedback.
/// </summary>
public class PhaseCGuideUI : MonoBehaviour
{
    private const string GuideCanvasName = "PhaseCGuideCanvas";
    private const string PanelName = "PhaseCGuidePanel";

    private PhaseCAssemblyController controller;
    private GameObject guideRoot;
    private TMP_Text stepTitleText;
    private TMP_Text objectiveText;
    private int lastStepNumber = -1;
    private Coroutine stepCompleteRoutine;

    private RectTransform _guidePanelRect;
    private GameObject    _guideBodyRoot;

    // Responsive tracking
    private CanvasScaler  _canvasScaler;
    private int           _lastScreenWidth;
    private int           _lastScreenHeight;

    private GameObject storyMomentRoot;
    private TMP_Text storyMomentTitle;
    private TMP_Text storyMomentBody;
    private ScrollRect storyMomentBodyScroll;
    private Button storyMomentButton;
    private PhaseCAssemblyController.StepInfo? pendingStepAfterStoryMoment;
    /// <summary>Story moments where the player discovers new information about the Psyche mission after completing each step.</summary>
    private static readonly (string title, string body)[] StoryMoments =
    {
        (
            "Science Payload Locked In",
            "You've helped lock in Psyche's three science instruments: the Magnetometer (to search for an ancient magnetic field at the asteroid), the Multispectral Imager (to map the surface in visible and near-infrared light), and the Gamma-Ray and Neutron Spectrometer (to reveal elemental composition). This instrument suite is the heart of the mission's science. Next: X-band radio and laser communication. "
        ),
        (
            "X-Band Radio and Laser Communication Locked In",
            "You've helped Dr. Priya Patel lock in both communication systems. X-band radio is the primary link for commanding the spacecraft and receiving data from deep space. Laser communication will send high-rate data back to Earth. With these set, the team can move to the spacecraft bus. Next: see Dr. Marcus Rodriguez. "
        ),
        (
            "Spacecraft Bus Complete: May 2020",
            "The spacecraft bus is the main structure that holds every subsystem, science instruments, and communications. With it complete, the team can move to Critical Design Review (CDR), which verifies that the full design meets every requirement before systems are integrated. You're on track for Phase C."
        ),
        (
            "Critical Design Review Passed",
            "CDR confirms the design is ready for the next phase. X-band and laser communication were already locked in; the CDR verified those plans. The next milestone is the Systems Integration Review, which checks that all subsystems can work together as one."
        ),
        (
            "Systems Integration Review: January 2021",
            "The Systems Integration Review is complete. It confirms that all subsystems can be combined and tested as a whole. Phase C closes with Key Decision Point D (KDP-D): NASA's formal approval to proceed beyond Phase C. Talk to Dr. Sarah Chen to finalize the milestone."
        )
    };

    private static readonly (string title, string body) FinalStoryMoment = (
        "Key Decision Point D: Phase C Complete",
        "NASA has approved the mission to proceed. Phase C (Final Design & Subsystem Fabrication) is complete: instruments locked in, X-band radio and laser communication built, spacecraft bus built, Critical Design Review and Systems Integration Review passed. You've discovered how the real Psyche mission reached this milestone on the way to the asteroid."
    );

    private void Awake()
    {
        controller = PhaseCAssemblyController.Instance;
    }

    private void Start()
    {
        EnsureGuide();
        Subscribe();
        UpdateGuide(controller != null ? controller.GetCurrentStepInfo() : PhaseCAssemblyController.StepInfo.Empty);
    }

    private void Update()
    {
        if (Screen.width != _lastScreenWidth || Screen.height != _lastScreenHeight)
        {
            _lastScreenWidth  = Screen.width;
            _lastScreenHeight = Screen.height;
            ApplyResponsiveLayout();
        }
    }

    private void ApplyResponsiveLayout()
    {
        if (_canvasScaler != null)
            _canvasScaler.matchWidthOrHeight = PhaseCUITheme.CanvasMatchWidthOrHeight;

        if (_guidePanelRect != null)
        {
            float topOffset = PhaseCUITheme.GetSafeAreaTopOffset() + PhaseCUITheme.GetGuideExtraTopPadding();
            _guidePanelRect.anchoredPosition = new Vector2(0f, -topOffset);
        }

        if (stepTitleText != null)
        {
            stepTitleText.fontSize = PhaseCUITheme.GetGuideStepTitleFontSize();
            RectTransform st = stepTitleText.GetComponent<RectTransform>();
            if (st != null)
            {
                st.offsetMin = new Vector2(PhaseCUITheme.PaddingTight, PhaseCUITheme.GetGuideStepTitleBottom());
                st.offsetMax = new Vector2(-PhaseCUITheme.PaddingTight, PhaseCUITheme.GetGuideStepTitleY());
            }
        }
        if (objectiveText != null)
        {
            objectiveText.fontSize = PhaseCUITheme.GetGuideObjectiveFontSize();
        }

        RefreshGuidePanelLayout();
    }

    private void OnDestroy()
    {
        if (controller != null)
        {
            controller.StepChanged -= UpdateGuide;
            controller.PhaseCComplete -= OnPhaseCComplete;
        }

        if (stepCompleteRoutine != null)
        {
            StopCoroutine(stepCompleteRoutine);
        }
    }

    private void OnPhaseCComplete()
    {
        if (storyMomentRoot == null || storyMomentTitle == null || storyMomentBody == null)
            return;
        pendingStepAfterStoryMoment = null;
        storyMomentTitle.text = FinalStoryMoment.title;
        storyMomentBody.text = FinalStoryMoment.body;
        storyMomentRoot.SetActive(true);
        RefreshStoryMomentBodyLayout();
    }

    private void Subscribe()
    {
        if (controller == null)
            controller = PhaseCAssemblyController.Instance;

        if (controller != null)
        {
            controller.StepChanged -= UpdateGuide;
            controller.StepChanged += UpdateGuide;
            controller.PhaseCComplete -= OnPhaseCComplete;
            controller.PhaseCComplete += OnPhaseCComplete;
        }
    }

    private void EnsureGuide()
    {
        GameObject existing = GameObject.Find(GuideCanvasName);
        if (existing != null)
        {
            Destroy(existing);
        }

        guideRoot = CreateCanvas();
        CreatePanelContent(guideRoot.transform);
        CreateStoryMomentPanel(guideRoot.transform);
    }

    private GameObject CreateCanvas()
    {
        GameObject go = new GameObject(GuideCanvasName);
        Canvas c = go.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = PhaseCUITheme.SortOrderGuide;

        _canvasScaler = go.AddComponent<CanvasScaler>();
        _canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        _canvasScaler.referenceResolution = new Vector2(PhaseCUITheme.RefWidth, PhaseCUITheme.RefHeight);
        _canvasScaler.matchWidthOrHeight = PhaseCUITheme.CanvasMatchWidthOrHeight;

        go.AddComponent<GraphicRaycaster>();
        return go;
    }

    private void CreatePanelContent(Transform parent)
    {
        GameObject panel = new GameObject(PanelName);
        panel.transform.SetParent(parent, false);
        Image panelImg = panel.AddComponent<Image>();
        panelImg.color = PhaseCUITheme.PanelBg;

        _guidePanelRect = panel.GetComponent<RectTransform>();
        _guidePanelRect.anchorMin = new Vector2(0f, 1f);
        _guidePanelRect.anchorMax = new Vector2(1f, 1f);
        _guidePanelRect.pivot = new Vector2(0.5f, 1f);
        // Shift down by safe-area inset + extra buffer to clear Dynamic Island / notch.
        float topOffset = PhaseCUITheme.GetSafeAreaTopOffset() + PhaseCUITheme.GetGuideExtraTopPadding();
        _guidePanelRect.anchoredPosition = new Vector2(0f, -topOffset);
        _guidePanelRect.offsetMin = new Vector2(0f, 0f);
        _guidePanelRect.offsetMax = new Vector2(0f, 0f);
        _guidePanelRect.sizeDelta = new Vector2(0f, PhaseCUITheme.GetGuidePanelHeight());

        // Body root - contains step title and objective.
        _guideBodyRoot = new GameObject("Body");
        _guideBodyRoot.transform.SetParent(panel.transform, false);
        RectTransform bodyRect = _guideBodyRoot.AddComponent<RectTransform>();
        bodyRect.anchorMin = Vector2.zero;
        bodyRect.anchorMax = Vector2.one;
        bodyRect.offsetMin = Vector2.zero;
        bodyRect.offsetMax = Vector2.zero;

        // Step title (e.g. "Step 1 of 5: Instrument Build") - takes full width.
        stepTitleText = CreateLabel(_guideBodyRoot.transform, "StepTitle", PhaseCUITheme.GetGuideStepTitleFontSize(), true);
        stepTitleText.fontStyle = FontStyles.Bold | FontStyles.UpperCase;
        stepTitleText.overflowMode  = TextOverflowModes.Ellipsis;
        stepTitleText.enableWordWrapping = false;
        RectTransform stepTitleRect = stepTitleText.GetComponent<RectTransform>();
        stepTitleRect.anchorMin = new Vector2(0f, 1f);
        stepTitleRect.anchorMax = new Vector2(1f, 1f);
        stepTitleRect.pivot     = new Vector2(0f, 1f);
        stepTitleRect.offsetMin = new Vector2(PhaseCUITheme.PaddingTight, PhaseCUITheme.GetGuideStepTitleBottom());
        stepTitleRect.offsetMax = new Vector2(-PhaseCUITheme.PaddingTight, PhaseCUITheme.GetGuideStepTitleY());
        stepTitleText.color = PhaseCUITheme.AccentGold;

        // Objective (summary / "Collect: ...") - word-wraps inside its rect, never overflows
        objectiveText = CreateLabel(_guideBodyRoot.transform, "Objective", PhaseCUITheme.GetGuideObjectiveFontSize(), false);
        objectiveText.alignment      = TextAlignmentOptions.TopLeft;
        objectiveText.overflowMode   = TextOverflowModes.Ellipsis;
        objectiveText.enableWordWrapping = true;
        RectTransform objectiveRect = objectiveText.GetComponent<RectTransform>();
        objectiveRect.anchorMin = new Vector2(0f, 1f);
        objectiveRect.anchorMax = new Vector2(1f, 1f);
        objectiveRect.pivot     = new Vector2(0.5f, 1f);
        float objPad = PhaseCUITheme.PaddingTight;
        objectiveRect.offsetMin = new Vector2(objPad, PhaseCUITheme.GetGuideObjectiveBottom());
        objectiveRect.offsetMax = new Vector2(-objPad, PhaseCUITheme.GetGuideObjectiveY());
        objectiveText.color = PhaseCUITheme.TextPrimary;

        RefreshGuidePanelLayout();
    }

    private void CreateStoryMomentPanel(Transform parent)
    {
        storyMomentRoot = new GameObject("StoryMomentOverlay");
        storyMomentRoot.transform.SetParent(parent, false);
        storyMomentRoot.SetActive(false);

        Canvas popupCanvas = storyMomentRoot.AddComponent<Canvas>();
        popupCanvas.overrideSorting = true;
        popupCanvas.sortingOrder = PhaseCUITheme.SortOrderStoryMomentPopup;
        storyMomentRoot.AddComponent<GraphicRaycaster>();

        Image bg = storyMomentRoot.AddComponent<Image>();
        bg.color = PhaseCUITheme.OverlayDark;
        RectTransform bgRect = storyMomentRoot.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        GameObject panel = new GameObject("StoryMomentPanel");
        panel.transform.SetParent(storyMomentRoot.transform, false);
        Image panelImg = panel.AddComponent<Image>();
        panelImg.color = PhaseCUITheme.PanelBg;
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.15f, 0.3f);
        panelRect.anchorMax = new Vector2(0.85f, 0.7f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        GameObject accent = new GameObject("AccentBar");
        accent.transform.SetParent(panel.transform, false);
        Image accentImg = accent.AddComponent<Image>();
        accentImg.color = PhaseCUITheme.AccentGold;
        RectTransform accentRect = accent.GetComponent<RectTransform>();
        accentRect.anchorMin = new Vector2(0f, 1f);
        accentRect.anchorMax = new Vector2(1f, 1f);
        accentRect.pivot = new Vector2(0.5f, 1f);
        accentRect.anchoredPosition = Vector2.zero;
        accentRect.sizeDelta = new Vector2(0f, PhaseCUITheme.AccentBarHeight);

        float titleBlock = PhaseCUITheme.GetStoryMomentTitleBlockHeight();
        float footerBlock = PhaseCUITheme.GetStoryMomentFooterHeight();

        // Scrollable body fills space between title block and footer (drawn before title so title stays on top).
        GameObject scrollRoot = new GameObject("StoryMomentBodyScroll");
        scrollRoot.transform.SetParent(panel.transform, false);
        RectTransform scrollRootRt = scrollRoot.AddComponent<RectTransform>();
        scrollRootRt.SetAsFirstSibling();
        scrollRootRt.anchorMin = Vector2.zero;
        scrollRootRt.anchorMax = Vector2.one;
        scrollRootRt.offsetMin = new Vector2(0f, footerBlock);
        scrollRootRt.offsetMax = new Vector2(0f, -titleBlock);

        storyMomentBodyScroll = scrollRoot.AddComponent<ScrollRect>();
        storyMomentBodyScroll.horizontal = false;
        storyMomentBodyScroll.vertical = true;
        storyMomentBodyScroll.movementType = ScrollRect.MovementType.Clamped;
        storyMomentBodyScroll.scrollSensitivity = 28f;
        storyMomentBodyScroll.inertia = true;

        GameObject viewportGo = new GameObject("Viewport");
        viewportGo.transform.SetParent(scrollRoot.transform, false);
        RectTransform viewportRt = viewportGo.AddComponent<RectTransform>();
        viewportRt.anchorMin = Vector2.zero;
        viewportRt.anchorMax = Vector2.one;
        viewportRt.offsetMin = new Vector2(PhaseCUITheme.PaddingTight, 4f);
        viewportRt.offsetMax = new Vector2(-PhaseCUITheme.PaddingTight, -4f);
        viewportGo.AddComponent<RectMask2D>();
        Image viewportHit = viewportGo.AddComponent<Image>();
        viewportHit.color = new Color(0f, 0f, 0f, 0.01f);
        viewportHit.raycastTarget = true;
        storyMomentBodyScroll.viewport = viewportRt;

        GameObject contentGo = new GameObject("Content");
        contentGo.transform.SetParent(viewportGo.transform, false);
        RectTransform contentRt = contentGo.AddComponent<RectTransform>();
        contentRt.anchorMin = new Vector2(0f, 1f);
        contentRt.anchorMax = new Vector2(1f, 1f);
        contentRt.pivot = new Vector2(0.5f, 1f);
        contentRt.anchoredPosition = Vector2.zero;
        contentRt.sizeDelta = Vector2.zero;

        storyMomentBody = contentGo.AddComponent<TextMeshProUGUI>();
        storyMomentBody.enableWordWrapping = true;
        storyMomentBody.fontSize = PhaseCUITheme.GetStoryMomentBodyFontSize();
        storyMomentBody.fontStyle = FontStyles.Normal;
        storyMomentBody.alignment = TextAlignmentOptions.Top;
        storyMomentBody.color = PhaseCUITheme.TextPrimary;
        storyMomentBody.raycastTarget = true;

        ContentSizeFitter bodyFitter = contentGo.AddComponent<ContentSizeFitter>();
        bodyFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        bodyFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        storyMomentBodyScroll.content = contentRt;

        storyMomentTitle = CreateLabel(panel.transform, "StoryMomentTitle", PhaseCUITheme.StoryMomentTitleSize, true);
        storyMomentTitle.color = PhaseCUITheme.AccentGold;
        storyMomentTitle.alignment = TextAlignmentOptions.Center;
        RectTransform titleRect = storyMomentTitle.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0f, -PhaseCUITheme.PaddingPanel);
        titleRect.offsetMin = new Vector2(PhaseCUITheme.PaddingTight, -100f);
        titleRect.offsetMax = new Vector2(-PhaseCUITheme.PaddingTight, -PhaseCUITheme.PaddingPanel);

        GameObject btnObj = new GameObject("StoryMomentContinue");
        btnObj.transform.SetParent(panel.transform, false);
        storyMomentButton = btnObj.AddComponent<Button>();
        Image btnImg = btnObj.AddComponent<Image>();
        btnImg.color = PhaseCUITheme.ButtonBg;
        RectTransform btnRect = btnObj.GetComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.5f, 0f);
        btnRect.anchorMax = new Vector2(0.5f, 0f);
        btnRect.pivot = new Vector2(0.5f, 0f);
        btnRect.anchoredPosition = new Vector2(0f, PhaseCUITheme.PaddingWide);
        btnRect.sizeDelta = new Vector2(PhaseCUITheme.ButtonWidthMin, PhaseCUITheme.ButtonHeight);
        GameObject btnLabel = new GameObject("Label");
        btnLabel.transform.SetParent(btnObj.transform, false);
        TMP_Text btnText = btnLabel.AddComponent<TextMeshProUGUI>();
        btnText.text = "Continue";
        btnText.fontSize = PhaseCUITheme.FontSizeButton;
        btnText.fontStyle = FontStyles.Bold;
        btnText.alignment = TextAlignmentOptions.Center;
        btnText.color = Color.white;
        RectTransform lblRect = btnLabel.GetComponent<RectTransform>();
        lblRect.anchorMin = Vector2.zero;
        lblRect.anchorMax = Vector2.one;
        lblRect.offsetMin = Vector2.zero;
        lblRect.offsetMax = Vector2.zero;
        ColorBlock btnColors = storyMomentButton.colors;
        btnColors.highlightedColor = PhaseCUITheme.ButtonHighlight;
        btnColors.pressedColor = PhaseCUITheme.ButtonPressed;
        storyMomentButton.colors = btnColors;
        storyMomentButton.onClick.AddListener(OnStoryMomentContinue);
    }

    /// <summary>Recompute TMP preferred height for the scroll content and snap to the top.</summary>
    private void RefreshStoryMomentBodyLayout()
    {
        if (storyMomentBody == null)
            return;
        RectTransform bodyRt = storyMomentBody.rectTransform;
        storyMomentBody.ForceMeshUpdate(true);
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(bodyRt);
        if (storyMomentBodyScroll != null)
            storyMomentBodyScroll.verticalNormalizedPosition = 1f;
    }

    private void OnStoryMomentContinue()
    {
        if (storyMomentRoot != null)
            storyMomentRoot.SetActive(false);
        if (pendingStepAfterStoryMoment.HasValue)
        {
            PhaseCAssemblyController.StepInfo next = pendingStepAfterStoryMoment.Value;
            ApplyStepContent(next);
            lastStepNumber = next.StepNumber;
            pendingStepAfterStoryMoment = null;
        }
    }

    private static TMP_Text CreateLabel(Transform parent, string name, float fontSize, bool bold)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        TMP_Text t = go.AddComponent<TextMeshProUGUI>();
        t.enableWordWrapping = true;
        t.fontSize = fontSize;
        t.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
        t.alignment = TextAlignmentOptions.TopLeft;
        t.color = PhaseCUITheme.TextPrimary;
        return t;
    }

    private void RefreshGuidePanelLayout()
    {
        if (_guidePanelRect == null || stepTitleText == null || objectiveText == null)
            return;

        Canvas.ForceUpdateCanvases();

        float panelWidth = _guidePanelRect.rect.width;
        if (panelWidth < 1f)
            panelWidth = PhaseCUITheme.RefWidth;

        float sidePad = PhaseCUITheme.PaddingTight;
        float titleRightPad = sidePad;
        float topPad = 6f;
        float between = 14f;
        float bottomPad = 8f;

        float titleWidth = Mathf.Max(120f, panelWidth - sidePad - titleRightPad);
        float objectiveWidth = Mathf.Max(120f, panelWidth - (sidePad * 2f));

        float preferredTitleHeight = Mathf.Max(
            PhaseCUITheme.GetGuideStepTitleFontSize() + 4f,
            stepTitleText.GetPreferredValues(stepTitleText.text ?? string.Empty, titleWidth, 0f).y);
        float preferredObjectiveHeight = objectiveText.GetPreferredValues(objectiveText.text ?? string.Empty, objectiveWidth, 0f).y;

        float minHeight = PhaseCUITheme.GetGuideTitleBarHeight() + 8f;
        float maxHeight = PhaseCUITheme.GetGuidePanelHeight();
        float desiredHeight = topPad + preferredTitleHeight + between + preferredObjectiveHeight + bottomPad;
        float panelHeight = Mathf.Clamp(desiredHeight, minHeight, maxHeight);

        float availableObjectiveHeight = Mathf.Max(0f, panelHeight - topPad - preferredTitleHeight - between - bottomPad);
        float objectiveHeight = Mathf.Min(preferredObjectiveHeight, availableObjectiveHeight);
        bool objectiveClipped = preferredObjectiveHeight > availableObjectiveHeight + 0.5f;
        objectiveText.overflowMode = objectiveClipped ? TextOverflowModes.Ellipsis : TextOverflowModes.Overflow;

        _guidePanelRect.sizeDelta = new Vector2(0f, panelHeight);

        RectTransform stepTitleRect = stepTitleText.rectTransform;
        stepTitleRect.anchorMin = new Vector2(0f, 1f);
        stepTitleRect.anchorMax = new Vector2(1f, 1f);
        stepTitleRect.pivot = new Vector2(0f, 1f);
        stepTitleRect.offsetMin = new Vector2(sidePad, -(topPad + preferredTitleHeight));
        stepTitleRect.offsetMax = new Vector2(-titleRightPad, -topPad);

        RectTransform objectiveRect = objectiveText.rectTransform;
        float objectiveTop = topPad + preferredTitleHeight + between;
        objectiveRect.anchorMin = new Vector2(0f, 1f);
        objectiveRect.anchorMax = new Vector2(1f, 1f);
        objectiveRect.pivot = new Vector2(0.5f, 1f);
        objectiveRect.offsetMin = new Vector2(sidePad, -(objectiveTop + objectiveHeight));
        objectiveRect.offsetMax = new Vector2(-sidePad, -objectiveTop);
    }

    private void UpdateGuide(PhaseCAssemblyController.StepInfo stepInfo)
    {
        if (stepTitleText == null || objectiveText == null)
            return;

        // Step complete feedback: if we advanced (not first load), show brief "Step X complete!" then update
        if (stepInfo.StepCount > 0 && stepInfo.StepNumber > lastStepNumber && lastStepNumber >= 1)
        {
            if (stepCompleteRoutine != null)
                StopCoroutine(stepCompleteRoutine);
            int completedStep = lastStepNumber;
            stepCompleteRoutine = StartCoroutine(ShowStepCompleteThenUpdate(completedStep, stepInfo));
            lastStepNumber = stepInfo.StepNumber;
            return;
        }

        lastStepNumber = stepInfo.StepNumber;
        ApplyStepContent(stepInfo);
    }

    private IEnumerator ShowStepCompleteThenUpdate(int completedStep, PhaseCAssemblyController.StepInfo nextStepInfo)
    {
        float normalStepTitleSize = PhaseCUITheme.GetGuideStepTitleFontSize();
        if (stepTitleText != null)
        {
            stepTitleText.fontSize = normalStepTitleSize + 6f;
            stepTitleText.text = $"Step {completedStep} complete!";
        }
        if (objectiveText != null)
            objectiveText.text = "Well done. Moving to the next objective.";
        RefreshGuidePanelLayout();

        yield return new WaitForSeconds(1.8f);

        stepCompleteRoutine = null;

        if (completedStep >= 1 && completedStep <= 5 && storyMomentRoot != null && storyMomentTitle != null && storyMomentBody != null)
        {
            int index = completedStep - 1;
            if (index >= 0 && index < StoryMoments.Length)
            {
                storyMomentTitle.text = StoryMoments[index].title;
                storyMomentBody.text = StoryMoments[index].body;
                pendingStepAfterStoryMoment = nextStepInfo;
                storyMomentRoot.SetActive(true);
                RefreshStoryMomentBodyLayout();
                yield break;
            }
        }

        if (stepTitleText != null)
            stepTitleText.fontSize = normalStepTitleSize;
        ApplyStepContent(nextStepInfo);
        lastStepNumber = nextStepInfo.StepNumber;
    }

    private void ApplyStepContent(PhaseCAssemblyController.StepInfo stepInfo)
    {
        if (stepInfo.StepCount == 0)
        {
            stepTitleText.text = "Phase C guide loading...";
            objectiveText.text = "";
            RefreshGuidePanelLayout();
            return;
        }

        if (stepInfo.StepNumber == 0)
        {
            stepTitleText.text = stepInfo.Title;
            objectiveText.text = stepInfo.Summary;
            RefreshGuidePanelLayout();
            return;
        }

        stepTitleText.text = $"Step {stepInfo.StepNumber} of {stepInfo.StepCount}: {stepInfo.Title}";
        objectiveText.text = !string.IsNullOrEmpty(stepInfo.CollectObjective) ? stepInfo.CollectObjective : stepInfo.Summary;
        RefreshGuidePanelLayout();
    }
}
