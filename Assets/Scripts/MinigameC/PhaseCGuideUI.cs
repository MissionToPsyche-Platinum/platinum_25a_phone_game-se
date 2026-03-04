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
    private TMP_Text talkToText;
    private TMP_Text controlsText;
    private List<Image> stepDots;
    private int lastStepNumber = -1;
    private Coroutine stepCompleteRoutine;

    private GameObject storyMomentRoot;
    private TMP_Text storyMomentTitle;
    private TMP_Text storyMomentBody;
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
        c.sortingOrder = 10;

        CanvasScaler scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(PhaseCUITheme.RefWidth, PhaseCUITheme.RefHeight);
        scaler.matchWidthOrHeight = 0.5f;

        go.AddComponent<GraphicRaycaster>();
        return go;
    }

    private void CreatePanelContent(Transform parent)
    {
        GameObject panel = new GameObject(PanelName);
        panel.transform.SetParent(parent, false);
        Image panelImg = panel.AddComponent<Image>();
        panelImg.color = PhaseCUITheme.PanelBg;

        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0f, 1f);
        panelRect.anchorMax = new Vector2(1f, 1f);
        panelRect.pivot = new Vector2(0.5f, 1f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.offsetMin = new Vector2(0f, 0f);
        panelRect.offsetMax = new Vector2(0f, 0f);
        panelRect.sizeDelta = new Vector2(0f, 220f);

        // Accent bar at top
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

        // Step progress strip (1-6 dots)
        const int stepCount = 6;
        GameObject dotsContainer = new GameObject("StepDots");
        dotsContainer.transform.SetParent(panel.transform, false);
        RectTransform dotsRect = dotsContainer.GetComponent<RectTransform>();
        if (dotsRect == null) dotsRect = dotsContainer.AddComponent<RectTransform>();
        dotsRect.anchorMin = new Vector2(0.5f, 1f);
        dotsRect.anchorMax = new Vector2(0.5f, 1f);
        dotsRect.pivot = new Vector2(0.5f, 1f);
        dotsRect.anchoredPosition = new Vector2(0f, -32f);
        dotsRect.sizeDelta = new Vector2(stepCount * PhaseCUITheme.GuideDotSpacing + 8f, 28f);

        stepDots = new List<Image>();
        for (int i = 0; i < stepCount; i++)
        {
            GameObject dot = new GameObject("Dot" + i);
            dot.transform.SetParent(dotsContainer.transform, false);
            Image dotImg = dot.AddComponent<Image>();
            dotImg.color = PhaseCUITheme.StepPending;
            RectTransform dotRect = dot.GetComponent<RectTransform>();
            dotRect.anchorMin = new Vector2(0.5f, 0.5f);
            dotRect.anchorMax = new Vector2(0.5f, 0.5f);
            dotRect.pivot = new Vector2(0.5f, 0.5f);
            dotRect.anchoredPosition = new Vector2(-0.5f * (stepCount - 1) * PhaseCUITheme.GuideDotSpacing + i * PhaseCUITheme.GuideDotSpacing, 0f);
            dotRect.sizeDelta = new Vector2(PhaseCUITheme.GuideDotSize, PhaseCUITheme.GuideDotSize);
            stepDots.Add(dotImg);
        }

        // Step title (e.g. "Step 1 of 5: Instrument Build")
        stepTitleText = CreateLabel(panel.transform, "StepTitle", PhaseCUITheme.GuideStepTitleSize, true);
        RectTransform stepTitleRect = stepTitleText.GetComponent<RectTransform>();
        stepTitleRect.anchorMin = new Vector2(0f, 1f);
        stepTitleRect.anchorMax = new Vector2(1f, 1f);
        stepTitleRect.pivot = new Vector2(0f, 1f);
        stepTitleRect.anchoredPosition = new Vector2(0f, -60f);
        stepTitleRect.offsetMin = new Vector2(PhaseCUITheme.PaddingTight, -96f);
        stepTitleRect.offsetMax = new Vector2(-PhaseCUITheme.PaddingTight, -60f);
        stepTitleText.color = PhaseCUITheme.AccentCyan;

        // Objective (summary)
        objectiveText = CreateLabel(panel.transform, "Objective", PhaseCUITheme.GuideObjectiveSize, false);
        RectTransform objectiveRect = objectiveText.GetComponent<RectTransform>();
        objectiveRect.anchorMin = new Vector2(0f, 1f);
        objectiveRect.anchorMax = new Vector2(1f, 1f);
        objectiveRect.pivot = new Vector2(0f, 1f);
        objectiveRect.anchoredPosition = new Vector2(0f, -102f);
        objectiveRect.offsetMin = new Vector2(PhaseCUITheme.PaddingTight, -142f);
        objectiveRect.offsetMax = new Vector2(-PhaseCUITheme.PaddingTight, -96f);
        objectiveText.color = PhaseCUITheme.TextPrimary;

        // Talk to: [NPC name]
        talkToText = CreateLabel(panel.transform, "TalkTo", PhaseCUITheme.GuideObjectiveSize, true);
        RectTransform talkToRect = talkToText.GetComponent<RectTransform>();
        talkToRect.anchorMin = new Vector2(0f, 1f);
        talkToRect.anchorMax = new Vector2(0.58f, 1f);
        talkToRect.pivot = new Vector2(0f, 1f);
        talkToRect.anchoredPosition = new Vector2(0f, -148f);
        talkToRect.offsetMin = new Vector2(PhaseCUITheme.PaddingTight, -182f);
        talkToRect.offsetMax = new Vector2(-PhaseCUITheme.PaddingTight, -142f);
        talkToText.color = PhaseCUITheme.StepCurrent;

        // Controls hint
        controlsText = CreateLabel(panel.transform, "Controls", PhaseCUITheme.GuideCaptionSize, false);
        RectTransform controlsRect = controlsText.GetComponent<RectTransform>();
        controlsRect.anchorMin = new Vector2(0.58f, 1f);
        controlsRect.anchorMax = new Vector2(1f, 1f);
        controlsRect.pivot = new Vector2(1f, 1f);
        controlsRect.anchoredPosition = new Vector2(0f, -148f);
        controlsRect.offsetMin = new Vector2(PhaseCUITheme.PaddingTight, -182f);
        controlsRect.offsetMax = new Vector2(-PhaseCUITheme.PaddingTight, -142f);
        controlsText.alignment = TextAlignmentOptions.Right;
        controlsText.color = PhaseCUITheme.TextSecondary;
    }

    private void CreateStoryMomentPanel(Transform parent)
    {
        storyMomentRoot = new GameObject("StoryMomentOverlay");
        storyMomentRoot.transform.SetParent(parent, false);
        storyMomentRoot.SetActive(false);

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

        storyMomentBody = CreateLabel(panel.transform, "StoryMomentBody", PhaseCUITheme.StoryMomentBodySize, false);
        storyMomentBody.color = PhaseCUITheme.TextPrimary;
        storyMomentBody.alignment = TextAlignmentOptions.Center;
        storyMomentBody.enableWordWrapping = true;
        RectTransform bodyRect = storyMomentBody.GetComponent<RectTransform>();
        bodyRect.anchorMin = new Vector2(0f, 1f);
        bodyRect.anchorMax = new Vector2(1f, 1f);
        bodyRect.pivot = new Vector2(0.5f, 1f);
        bodyRect.anchoredPosition = new Vector2(0f, -110f);
        bodyRect.offsetMin = new Vector2(PhaseCUITheme.PaddingPanel, -280f);
        bodyRect.offsetMax = new Vector2(-PhaseCUITheme.PaddingPanel, -100f);

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

    private void UpdateGuide(PhaseCAssemblyController.StepInfo stepInfo)
    {
        if (stepTitleText == null || objectiveText == null || talkToText == null || controlsText == null)
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
        if (stepTitleText != null)
            stepTitleText.text = $"Step {completedStep} complete!";
        if (objectiveText != null)
            objectiveText.text = "Well done. Moving to the next objective.";
        if (talkToText != null)
            talkToText.text = "";
        if (controlsText != null)
            controlsText.text = "";

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
                yield break;
            }
        }

        ApplyStepContent(nextStepInfo);
        lastStepNumber = nextStepInfo.StepNumber;
    }

    private void ApplyStepContent(PhaseCAssemblyController.StepInfo stepInfo)
    {
        if (stepInfo.StepCount == 0)
        {
            stepTitleText.text = "Phase C guide loading...";
            objectiveText.text = "";
            talkToText.text = "";
            controlsText.text = "";
            return;
        }

        if (stepInfo.StepNumber == 0)
        {
            stepTitleText.text = stepInfo.Title;
            objectiveText.text = stepInfo.Summary;
            talkToText.text = "";
            controlsText.text = "";
            if (stepDots != null)
            {
                for (int i = 0; i < stepDots.Count; i++)
                    stepDots[i].color = PhaseCUITheme.StepDone;
            }
            return;
        }

        stepTitleText.text = $"Step {stepInfo.StepNumber} of {stepInfo.StepCount}: {stepInfo.Title}";
        objectiveText.text = !string.IsNullOrEmpty(stepInfo.CollectObjective) ? stepInfo.CollectObjective : stepInfo.Summary;
        talkToText.text = $"Talk to: {stepInfo.CompletionNpc}";
        controlsText.text = "E/Space: talk | I: inventory | P: save | Tab: menu";

        if (stepDots != null)
        {
            for (int i = 0; i < stepDots.Count; i++)
            {
                int oneBased = i + 1;
                if (oneBased < stepInfo.StepNumber)
                    stepDots[i].color = PhaseCUITheme.StepDone;
                else if (oneBased == stepInfo.StepNumber)
                    stepDots[i].color = PhaseCUITheme.StepCurrent;
                else
                    stepDots[i].color = PhaseCUITheme.StepPending;
            }
        }
    }
}
