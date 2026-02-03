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

    private static readonly Color PanelBg = new Color(0.07f, 0.09f, 0.16f, 0.92f);
    private static readonly Color AccentGold = new Color(0.89f, 0.75f, 0.35f, 1f);
    private static readonly Color AccentCyan = new Color(0.45f, 0.72f, 0.88f, 1f);
    private static readonly Color TextPrimary = new Color(0.95f, 0.94f, 0.9f, 1f);
    private static readonly Color TextSecondary = new Color(0.75f, 0.78f, 0.82f, 1f);
    private static readonly Color StepDone = new Color(0.4f, 0.7f, 0.45f, 1f);
    private static readonly Color StepCurrent = new Color(0.89f, 0.75f, 0.35f, 1f);
    private static readonly Color StepPending = new Color(0.35f, 0.38f, 0.45f, 0.9f);

    private PhaseCAssemblyController controller;
    private GameObject guideRoot;
    private TMP_Text stepTitleText;
    private TMP_Text objectiveText;
    private TMP_Text talkToText;
    private TMP_Text controlsText;
    private List<Image> stepDots;
    private int lastStepNumber = -1;
    private Coroutine stepCompleteRoutine;

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
        }

        if (stepCompleteRoutine != null)
        {
            StopCoroutine(stepCompleteRoutine);
        }
    }

    private void Subscribe()
    {
        if (controller == null)
            controller = PhaseCAssemblyController.Instance;

        if (controller != null)
        {
            controller.StepChanged -= UpdateGuide;
            controller.StepChanged += UpdateGuide;
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
    }

    private GameObject CreateCanvas()
    {
        GameObject go = new GameObject(GuideCanvasName);
        Canvas c = go.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = 10;

        CanvasScaler scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        go.AddComponent<GraphicRaycaster>();
        return go;
    }

    private void CreatePanelContent(Transform parent)
    {
        GameObject panel = new GameObject(PanelName);
        panel.transform.SetParent(parent, false);
        Image panelImg = panel.AddComponent<Image>();
        panelImg.color = PanelBg;

        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0f, 1f);
        panelRect.anchorMax = new Vector2(1f, 1f);
        panelRect.pivot = new Vector2(0.5f, 1f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.offsetMin = new Vector2(0f, 0f);
        panelRect.offsetMax = new Vector2(0f, 0f);
        panelRect.sizeDelta = new Vector2(0f, 200f);

        // Accent bar at top
        GameObject accent = new GameObject("AccentBar");
        accent.transform.SetParent(panel.transform, false);
        Image accentImg = accent.AddComponent<Image>();
        accentImg.color = AccentGold;
        RectTransform accentRect = accent.GetComponent<RectTransform>();
        accentRect.anchorMin = new Vector2(0f, 1f);
        accentRect.anchorMax = new Vector2(1f, 1f);
        accentRect.pivot = new Vector2(0.5f, 1f);
        accentRect.anchoredPosition = Vector2.zero;
        accentRect.sizeDelta = new Vector2(0f, 4f);

        // Step progress strip (1–5 dots)
        GameObject dotsContainer = new GameObject("StepDots");
        dotsContainer.transform.SetParent(panel.transform, false);
        RectTransform dotsRect = dotsContainer.GetComponent<RectTransform>();
        if (dotsRect == null) dotsRect = dotsContainer.AddComponent<RectTransform>();
        dotsRect.anchorMin = new Vector2(0.5f, 1f);
        dotsRect.anchorMax = new Vector2(0.5f, 1f);
        dotsRect.pivot = new Vector2(0.5f, 1f);
        dotsRect.anchoredPosition = new Vector2(0f, -28f);
        dotsRect.sizeDelta = new Vector2(180f, 24f);

        stepDots = new List<Image>();
        int stepCount = 5;
        float dotSpacing = 36f;
        for (int i = 0; i < stepCount; i++)
        {
            GameObject dot = new GameObject("Dot" + i);
            dot.transform.SetParent(dotsContainer.transform, false);
            Image dotImg = dot.AddComponent<Image>();
            dotImg.color = StepPending;
            RectTransform dotRect = dot.GetComponent<RectTransform>();
            dotRect.anchorMin = new Vector2(0.5f, 0.5f);
            dotRect.anchorMax = new Vector2(0.5f, 0.5f);
            dotRect.pivot = new Vector2(0.5f, 0.5f);
            dotRect.anchoredPosition = new Vector2(-0.5f * (stepCount - 1) * dotSpacing + i * dotSpacing, 0f);
            dotRect.sizeDelta = new Vector2(14f, 14f);
            stepDots.Add(dotImg);
        }

        // Step title (e.g. "Step 1 of 5: Instrument Build")
        stepTitleText = CreateLabel(panel.transform, "StepTitle", 22f, true);
        RectTransform stepTitleRect = stepTitleText.GetComponent<RectTransform>();
        stepTitleRect.anchorMin = new Vector2(0f, 1f);
        stepTitleRect.anchorMax = new Vector2(1f, 1f);
        stepTitleRect.pivot = new Vector2(0f, 1f);
        stepTitleRect.anchoredPosition = new Vector2(0f, -56f);
        stepTitleRect.offsetMin = new Vector2(24f, -88f);
        stepTitleRect.offsetMax = new Vector2(-24f, -56f);
        stepTitleText.color = AccentCyan;

        // Objective (summary)
        objectiveText = CreateLabel(panel.transform, "Objective", 20f, false);
        RectTransform objectiveRect = objectiveText.GetComponent<RectTransform>();
        objectiveRect.anchorMin = new Vector2(0f, 1f);
        objectiveRect.anchorMax = new Vector2(1f, 1f);
        objectiveRect.pivot = new Vector2(0f, 1f);
        objectiveRect.anchoredPosition = new Vector2(0f, -96f);
        objectiveRect.offsetMin = new Vector2(24f, -132f);
        objectiveRect.offsetMax = new Vector2(-24f, -88f);
        objectiveText.color = TextPrimary;

        // Talk to: [NPC name]
        talkToText = CreateLabel(panel.transform, "TalkTo", 20f, true);
        RectTransform talkToRect = talkToText.GetComponent<RectTransform>();
        talkToRect.anchorMin = new Vector2(0f, 1f);
        talkToRect.anchorMax = new Vector2(0.6f, 1f);
        talkToRect.pivot = new Vector2(0f, 1f);
        talkToRect.anchoredPosition = new Vector2(0f, -140f);
        talkToRect.offsetMin = new Vector2(24f, -172f);
        talkToRect.offsetMax = new Vector2(-24f, -132f);
        talkToText.color = StepCurrent;

        // Controls hint
        controlsText = CreateLabel(panel.transform, "Controls", 16f, false);
        RectTransform controlsRect = controlsText.GetComponent<RectTransform>();
        controlsRect.anchorMin = new Vector2(0.6f, 1f);
        controlsRect.anchorMax = new Vector2(1f, 1f);
        controlsRect.pivot = new Vector2(1f, 1f);
        controlsRect.anchoredPosition = new Vector2(0f, -140f);
        controlsRect.offsetMin = new Vector2(24f, -172f);
        controlsRect.offsetMax = new Vector2(-24f, -132f);
        controlsText.alignment = TextAlignmentOptions.Right;
        controlsText.color = TextSecondary;
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
        t.color = TextPrimary;
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
                    stepDots[i].color = StepDone;
            }
            return;
        }

        stepTitleText.text = $"Step {stepInfo.StepNumber} of {stepInfo.StepCount}: {stepInfo.Title}";
        objectiveText.text = stepInfo.Summary;
        talkToText.text = $"Talk to: {stepInfo.CompletionNpc}";
        controlsText.text = "E or Space near NPC to talk";

        if (stepDots != null)
        {
            for (int i = 0; i < stepDots.Count; i++)
            {
                int oneBased = i + 1;
                if (oneBased < stepInfo.StepNumber)
                    stepDots[i].color = StepDone;
                else if (oneBased == stepInfo.StepNumber)
                    stepDots[i].color = StepCurrent;
                else
                    stepDots[i].color = StepPending;
            }
        }
    }
}
