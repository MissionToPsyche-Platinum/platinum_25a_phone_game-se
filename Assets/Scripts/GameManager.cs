using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Game Setup")]
    public Canvas gameCanvas;
    
    void Awake()
    {
        Debug.Log("GameManager Awake() called");
        
        // Find or create canvas
        if (gameCanvas == null)
        {
            gameCanvas = FindFirstObjectByType<Canvas>();
        }
        
        if (gameCanvas == null)
        {
            Debug.Log("No Canvas found! Creating one...");
            CreateCanvas();
        }
        
        Debug.Log($"Canvas found: {gameCanvas.name}");
    }
    
    void Start()
    {
        Debug.Log("GameManager Start() called");
        
        // Verify EventSystem is properly configured
        UnityEngine.EventSystems.EventSystem eventSystem = FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
        if (eventSystem == null)
        {
            Debug.LogError("No EventSystem found! Creating one...");
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystem = eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            
            // Try to use InputSystemUIInputModule for Unity 6000 compatibility
            try 
            {
                var inputModule = eventSystemObj.AddComponent(System.Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule,Unity.InputSystem"));
                Debug.Log("Added InputSystemUIInputModule for Unity 6000");
            }
            catch
            {
                // Fallback to StandaloneInputModule
                eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                Debug.Log("Added StandaloneInputModule as fallback");
            }
        }
        
        Debug.Log($"EventSystem status: {(eventSystem.enabled ? "Enabled" : "Disabled")}");
        Debug.Log($"EventSystem current: {UnityEngine.EventSystems.EventSystem.current != null}");
        
        // Verify GraphicRaycaster
        GraphicRaycaster raycaster = gameCanvas.GetComponent<GraphicRaycaster>();
        if (raycaster == null)
        {
            Debug.LogError("No GraphicRaycaster found on Canvas! Adding one...");
            raycaster = gameCanvas.gameObject.AddComponent<GraphicRaycaster>();
        }
        Debug.Log($"GraphicRaycaster enabled: {raycaster.enabled}");
        
        // Set up the game
        CreateGame();
        
        Debug.Log("Game setup completed");
    }
    
    void CreateCanvas()
    {
        GameObject canvasObj = new GameObject("GameCanvas");
        gameCanvas = canvasObj.AddComponent<Canvas>();
        gameCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // Add EventSystem if it doesn't exist
        if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            var eventSystem = eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            
            // Try to use InputSystemUIInputModule for Unity 6000 compatibility
            try 
            {
                var inputModule = eventSystemObj.AddComponent(System.Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule,Unity.InputSystem"));
                Debug.Log("Added InputSystemUIInputModule for Unity 6000");
            }
            catch
            {
                // Fallback to StandaloneInputModule
                eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                Debug.Log("Added StandaloneInputModule as fallback");
            }
        }
    }
    
    void CreateGame()
    {
        Debug.Log("Creating game UI elements...");
        
        if (gameCanvas == null)
        {
            Debug.LogError("Canvas is null! Cannot create game UI.");
            return;
        }
        
        // Create left panel for draggable items
        GameObject leftPanel = CreatePanel("LeftPanel", new Vector2(0, 0), new Vector2(0.3f, 1), new Color(0.8f, 0.8f, 0.8f, 0.3f));
        
        // Add vertical layout group to left panel for responsive spacing
        VerticalLayoutGroup layoutGroup = leftPanel.AddComponent<VerticalLayoutGroup>();
        layoutGroup.childAlignment = TextAnchor.MiddleCenter;
        layoutGroup.childControlHeight = false;
        layoutGroup.childControlWidth = false;
        layoutGroup.childForceExpandHeight = false;
        layoutGroup.childForceExpandWidth = false;
        layoutGroup.spacing = 20f;
        
        // Add content size fitter for responsive sizing
        ContentSizeFitter fitter = leftPanel.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        Debug.Log($"Created left panel: {leftPanel.name}");
        
        // Create center board for drop zones
        GameObject centerBoard = CreatePanel("CenterBoard", new Vector2(0.35f, 0.1f), new Vector2(0.65f, 0.9f), new Color(0.9f, 0.9f, 0.9f, 0.2f));
        Debug.Log($"Created center board: {centerBoard.name}");
        
        // Create 4 colored items and matching drop zones
        Color[] colors = { Color.red, Color.blue, Color.green, Color.yellow };
        
        for (int i = 0; i < 4; i++)
        {
            CreateDraggableItem(leftPanel, colors[i], i);
            CreateDropZone(centerBoard, colors[i], i);
            Debug.Log($"Created item {i} with color {colors[i]}");
        }
        
        Debug.Log("All UI elements created successfully");
    }
    
    GameObject CreatePanel(string name, Vector2 anchorMin, Vector2 anchorMax, Color color)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(gameCanvas.transform, false);
        
        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        
        Image bg = panel.AddComponent<Image>();
        bg.color = color;
        
        Debug.Log($"Created panel {name} with color {color}");
        return panel;
    }
    
    void CreateDraggableItem(GameObject parent, Color itemColor, int index)
    {
        GameObject item = new GameObject($"DraggableItem_{index}");
        item.transform.SetParent(parent.transform, false);
        
        RectTransform rect = item.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(80, 80);
        // Remove fixed positioning - let layout group handle positioning
        
        Image image = item.AddComponent<Image>();
        image.color = itemColor;
        image.raycastTarget = true; // Enable raycasting for drag detection
        
        // Add layout element for better control
        LayoutElement layoutElement = item.AddComponent<LayoutElement>();
        layoutElement.preferredWidth = 80;
        layoutElement.preferredHeight = 80;
        layoutElement.flexibleWidth = 0;
        layoutElement.flexibleHeight = 0;
        
        // Add draggable component
        DraggableItem dragComponent = item.AddComponent<DraggableItem>();
        dragComponent.itemColor = itemColor;
        
        Debug.Log($"Created draggable item {index} with color {itemColor}");
    }
    
    void CreateDropZone(GameObject parent, Color zoneColor, int index)
    {
        GameObject zone = new GameObject($"DropZone_{index}");
        zone.transform.SetParent(parent.transform, false);
        
        RectTransform rect = zone.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(100, 100);
        
        // Arrange in 2x2 grid
        float x = (index % 2) * 120 - 60;
        float y = (index / 2) * 120 - 60;
        rect.anchoredPosition = new Vector2(x, y);
        
        Image image = zone.AddComponent<Image>();
        Color outlineColor = zoneColor;
        outlineColor.a = 0.3f;
        image.color = outlineColor;
        image.raycastTarget = true; // Enable raycasting for drop detection
        
        // Add drop zone component
        DropZone dropComponent = zone.AddComponent<DropZone>();
        dropComponent.acceptedColor = zoneColor;
        
        Debug.Log($"Created drop zone {index} with color {zoneColor} at position {rect.anchoredPosition}");
    }
}