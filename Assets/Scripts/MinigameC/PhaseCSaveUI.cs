using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Save/Load popup for Phase C. Toggled with P key (hidden by default).
/// Auto-creates on scene load. Calls SaveController for save/load.
/// </summary>
public class PhaseCSaveUI : MonoBehaviour
{
    private const string TargetSceneName = "MinigameC";
    private const string CanvasName = "PhaseCSaveCanvas";

    private GameObject canvasObject;
    private SaveController saveController;
    private TMP_Text statusText;
    private bool initialized;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void EnsureSaveUI()
    {
        SceneManager.sceneLoaded += (scene, _) =>
        {
            if (scene.name != TargetSceneName) return;
            if (FindFirstObjectByType<PhaseCSaveUI>() != null) return;
            new GameObject("PhaseCSaveUI").AddComponent<PhaseCSaveUI>();
        };
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
        saveController = FindFirstObjectByType<SaveController>();
        CreateSaveCanvas();
        canvasObject.SetActive(false);
        initialized = true;
    }

    private void Update()
    {
        if (!initialized) return;

        if (Input.GetKeyDown(KeyCode.P))
        {
            canvasObject.SetActive(!canvasObject.activeSelf);
        }
    }

    private void CreateSaveCanvas()
    {
        GameObject existing = GameObject.Find(CanvasName);
        if (existing != null) Destroy(existing);

        // Canvas
        GameObject canvasGo = new GameObject(CanvasName);
        canvasObject = canvasGo;
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 16;

        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(PhaseCUITheme.RefWidth, PhaseCUITheme.RefHeight);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGo.AddComponent<GraphicRaycaster>();

        // Overlay background
        GameObject overlayGo = new GameObject("Overlay");
        overlayGo.transform.SetParent(canvasGo.transform, false);
        Image overlayImg = overlayGo.AddComponent<Image>();
        overlayImg.color = new Color(0f, 0f, 0f, 0.5f);
        overlayImg.raycastTarget = true;
        RectTransform overlayRect = overlayGo.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;

        // Centered panel
        float panelWidth = 400f;
        float panelHeight = 300f;

        GameObject panelGo = new GameObject("SavePanel");
        panelGo.transform.SetParent(canvasGo.transform, false);
        Image panelBg = panelGo.AddComponent<Image>();
        panelBg.color = PhaseCUITheme.PanelBg;
        panelBg.raycastTarget = true;

        RectTransform panelRect = panelGo.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(panelWidth, panelHeight);
        panelRect.anchoredPosition = Vector2.zero;

        // Border
        GameObject borderGo = new GameObject("Border");
        borderGo.transform.SetParent(panelGo.transform, false);
        Image borderImg = borderGo.AddComponent<Image>();
        borderImg.color = PhaseCUITheme.PanelBorder;
        borderImg.raycastTarget = false;
        RectTransform borderRect = borderGo.GetComponent<RectTransform>();
        borderRect.anchorMin = Vector2.zero;
        borderRect.anchorMax = Vector2.one;
        borderRect.offsetMin = new Vector2(-2f, -2f);
        borderRect.offsetMax = new Vector2(2f, 2f);
        borderGo.transform.SetAsFirstSibling();

        // Title
        GameObject titleGo = new GameObject("Title");
        titleGo.transform.SetParent(panelGo.transform, false);
        TMP_Text titleText = titleGo.AddComponent<TextMeshProUGUI>();
        titleText.text = "SAVE / LOAD";
        titleText.fontSize = 24;
        titleText.fontStyle = FontStyles.Bold;
        titleText.color = PhaseCUITheme.AccentGold;
        titleText.alignment = TextAlignmentOptions.Center;
        RectTransform titleRect = titleGo.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.sizeDelta = new Vector2(0f, 50f);
        titleRect.anchoredPosition = new Vector2(0f, -20f);

        // Save button
        CreateButton(panelGo.transform, "SaveButton", "Save Game", new Vector2(0f, 20f), OnSaveClicked);

        // Load button
        CreateButton(panelGo.transform, "LoadButton", "Load Game", new Vector2(0f, -50f), OnLoadClicked);

        // Status text
        GameObject statusGo = new GameObject("Status");
        statusGo.transform.SetParent(panelGo.transform, false);
        statusText = statusGo.AddComponent<TextMeshProUGUI>();
        statusText.text = "";
        statusText.fontSize = 16;
        statusText.color = PhaseCUITheme.TextSecondary;
        statusText.alignment = TextAlignmentOptions.Center;
        RectTransform statusRect = statusGo.GetComponent<RectTransform>();
        statusRect.anchorMin = new Vector2(0f, 0f);
        statusRect.anchorMax = new Vector2(1f, 0f);
        statusRect.pivot = new Vector2(0.5f, 0f);
        statusRect.sizeDelta = new Vector2(0f, 50f);
        statusRect.anchoredPosition = new Vector2(0f, 40f);

        // Hint text
        GameObject hintGo = new GameObject("Hint");
        hintGo.transform.SetParent(panelGo.transform, false);
        TMP_Text hintText = hintGo.AddComponent<TextMeshProUGUI>();
        hintText.text = "Press P to close";
        hintText.fontSize = 14;
        hintText.color = PhaseCUITheme.TextSecondary;
        hintText.alignment = TextAlignmentOptions.Center;
        RectTransform hintRect = hintGo.GetComponent<RectTransform>();
        hintRect.anchorMin = new Vector2(0f, 0f);
        hintRect.anchorMax = new Vector2(1f, 0f);
        hintRect.pivot = new Vector2(0.5f, 0f);
        hintRect.sizeDelta = new Vector2(0f, 24f);
        hintRect.anchoredPosition = new Vector2(0f, 10f);
    }

    private void CreateButton(Transform parent, string name, string label, Vector2 position, UnityEngine.Events.UnityAction onClick)
    {
        GameObject btnGo = new GameObject(name);
        btnGo.transform.SetParent(parent, false);

        Image btnImg = btnGo.AddComponent<Image>();
        btnImg.color = PhaseCUITheme.ButtonBg;
        btnImg.raycastTarget = true;

        Button btn = btnGo.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.highlightedColor = PhaseCUITheme.ButtonHighlight;
        colors.pressedColor = PhaseCUITheme.ButtonPressed;
        btn.colors = colors;
        btn.onClick.AddListener(onClick);

        RectTransform btnRect = btnGo.GetComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.5f, 0.5f);
        btnRect.anchorMax = new Vector2(0.5f, 0.5f);
        btnRect.pivot = new Vector2(0.5f, 0.5f);
        btnRect.sizeDelta = new Vector2(PhaseCUITheme.ButtonWidthMin, PhaseCUITheme.ButtonHeight);
        btnRect.anchoredPosition = position;

        GameObject labelGo = new GameObject("Label");
        labelGo.transform.SetParent(btnGo.transform, false);
        TMP_Text btnText = labelGo.AddComponent<TextMeshProUGUI>();
        btnText.text = label;
        btnText.fontSize = PhaseCUITheme.FontSizeButton;
        btnText.fontStyle = FontStyles.Bold;
        btnText.alignment = TextAlignmentOptions.Center;
        btnText.color = Color.white;
        RectTransform lblRect = labelGo.GetComponent<RectTransform>();
        lblRect.anchorMin = Vector2.zero;
        lblRect.anchorMax = Vector2.one;
        lblRect.offsetMin = Vector2.zero;
        lblRect.offsetMax = Vector2.zero;
    }

    private void OnSaveClicked()
    {
        if (saveController != null)
        {
            saveController.SaveGame();
            if (statusText != null) statusText.text = "Game saved!";
        }
        else
        {
            if (statusText != null) statusText.text = "Save system not found.";
        }
    }

    private void OnLoadClicked()
    {
        if (saveController != null)
        {
            saveController.LoadGame();
            if (statusText != null) statusText.text = "Game loaded!";
        }
        else
        {
            if (statusText != null) statusText.text = "Save system not found.";
        }
    }
}
