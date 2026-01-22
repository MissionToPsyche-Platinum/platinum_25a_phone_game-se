using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PhaseCGuideUI : MonoBehaviour
{
    private const string GuideCanvasName = "PhaseCGuideCanvas";
    private const string GuideTextName = "PhaseCGuideText";

    private PhaseCAssemblyController controller;
    private TMP_Text guideText;

    private void Awake()
    {
        controller = PhaseCAssemblyController.Instance;
    }

    private void Start()
    {
        EnsureGuideText();
        Subscribe();
        UpdateGuide(controller != null ? controller.GetCurrentStepInfo() : PhaseCAssemblyController.StepInfo.Empty);
    }

    private void OnDestroy()
    {
        if (controller != null)
        {
            controller.StepChanged -= UpdateGuide;
        }
    }

    private void Subscribe()
    {
        if (controller == null)
        {
            controller = PhaseCAssemblyController.Instance;
        }

        if (controller != null)
        {
            controller.StepChanged -= UpdateGuide;
            controller.StepChanged += UpdateGuide;
        }
    }

    private void EnsureGuideText()
    {
        GameObject existingCanvas = GameObject.Find(GuideCanvasName);
        if (existingCanvas == null)
        {
            existingCanvas = CreateCanvas();
        }

        Transform existingText = existingCanvas.transform.Find(GuideTextName);
        if (existingText != null)
        {
            guideText = existingText.GetComponent<TMP_Text>();
        }

        if (guideText == null)
        {
            guideText = CreateGuideText(existingCanvas.transform);
        }
    }

    private GameObject CreateCanvas()
    {
        GameObject canvasObject = new GameObject(GuideCanvasName);
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasObject.AddComponent<GraphicRaycaster>();
        return canvasObject;
    }

    private TMP_Text CreateGuideText(Transform parent)
    {
        GameObject panelObject = new GameObject("PhaseCGuidePanel");
        panelObject.transform.SetParent(parent, false);
        Image panelImage = panelObject.AddComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.6f);

        RectTransform panelRect = panelObject.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0f, 1f);
        panelRect.anchorMax = new Vector2(0f, 1f);
        panelRect.pivot = new Vector2(0f, 1f);
        panelRect.anchoredPosition = new Vector2(24f, -24f);
        panelRect.sizeDelta = new Vector2(720f, 180f);

        GameObject textObject = new GameObject(GuideTextName);
        textObject.transform.SetParent(panelObject.transform, false);
        TMP_Text text = textObject.AddComponent<TextMeshProUGUI>();
        text.enableWordWrapping = true;
        text.fontSize = 26f;
        text.alignment = TextAlignmentOptions.TopLeft;

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0f, 0f);
        textRect.anchorMax = new Vector2(1f, 1f);
        textRect.offsetMin = new Vector2(16f, 12f);
        textRect.offsetMax = new Vector2(-16f, -12f);

        return text;
    }

    private void UpdateGuide(PhaseCAssemblyController.StepInfo stepInfo)
    {
        if (guideText == null)
        {
            return;
        }

        if (stepInfo.StepCount == 0)
        {
            guideText.text = "Phase C assembly guide loading...";
            return;
        }

        guideText.text =
            $"Phase C Step {stepInfo.StepNumber}/{stepInfo.StepCount}: {stepInfo.Title}\n" +
            $"Talk to: {stepInfo.CompletionNpc}\n" +
            $"Goal: {stepInfo.Summary}\n" +
            "Press E or Space near the NPC to continue.";
    }
}
