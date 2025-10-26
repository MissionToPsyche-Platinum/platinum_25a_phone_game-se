using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SpaceCraftPart : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public enum PartType
    {
        SolarPanel,
        Antenna,
        Thruster,
        Sensor,
        Battery,
        Core
    }
    
    [Header("Part Settings")]
    [SerializeField] private string partName;
    [SerializeField] private PartType partType;
    [SerializeField] private Sprite partIcon;
    [SerializeField] private Color partColor = Color.white;
    
    [Header("Drag Settings")]
    [SerializeField] private float dragAlpha = 0.6f;
    public Canvas parentCanvas; // Reference to parent Canvas for proper drag behavior
    
    private Vector3 originalPosition;
    private Transform originalParent;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Image partImage;
    private bool isDragging = false;
    private bool isPlaced = false;
    
    public string PartName => partName;
    public PartType Type => partType;
    public bool IsPlaced => isPlaced;
    
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        partImage = GetComponent<Image>();
        canvasGroup = GetComponent<CanvasGroup>();
        
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        // Auto-find parent Canvas if not assigned
        if (parentCanvas == null)
        {
            parentCanvas = GetComponentInParent<Canvas>();
            if (parentCanvas == null)
            {
                Debug.LogError($"No Canvas found for {gameObject.name}! Drag functionality may not work properly.");
            }
        }
        
        // Store original position and parent
        originalPosition = rectTransform.localPosition;
        originalParent = transform.parent;
        
        // Set up visual properties
        if (partImage != null)
        {
            partImage.sprite = partIcon;
            partImage.color = partColor;
        }
    }
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isPlaced) return;
        
        isDragging = true;
        
        // Make semi-transparent while dragging
        canvasGroup.alpha = dragAlpha;
        canvasGroup.blocksRaycasts = false;
        
        // Move to front (highest sibling index)
        transform.SetAsLastSibling();
        
        // Notify drag manager
        DragDropManager.Instance?.OnBeginDrag(this);
        
        Debug.Log($"Started dragging {partName}");
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        
        // Follow mouse position using Canvas scale factor
        if (parentCanvas != null)
        {
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentCanvas.transform as RectTransform, 
                eventData.position, 
                eventData.pressEventCamera, 
                out localPoint);
            
            rectTransform.anchoredPosition = localPoint;
        }
        else
        {
            // Fallback method
            Vector3 worldPosition;
            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
                rectTransform, eventData.position, eventData.pressEventCamera, out worldPosition))
            {
                transform.position = worldPosition;
            }
        }
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        
        isDragging = false;
        
        // Reset visual properties
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        
        // Check if dropped on valid drop zone
        bool droppedOnValidZone = false;
        
        // Raycast to find drop zones
        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        
        foreach (var result in results)
        {
            DropZone dropZone = result.gameObject.GetComponent<DropZone>();
            if (dropZone != null && dropZone.CanAcceptPart(this))
            {
                dropZone.OnPartDropped(this);
                droppedOnValidZone = true;
                isPlaced = true;
                break;
            }
        }
        
        // If not dropped on valid zone, return to original position
        if (!droppedOnValidZone)
        {
            ReturnToOriginalPosition();
        }
        
        // Notify drag manager
        DragDropManager.Instance?.OnEndDrag(this, droppedOnValidZone);
        
        Debug.Log($"Ended dragging {partName}, placed: {droppedOnValidZone}");
    }
    
    public void ReturnToOriginalPosition()
    {
        if (isPlaced) return;
        
        // Return to original parent and position
        transform.SetParent(originalParent);
        rectTransform.localPosition = originalPosition;
        isPlaced = false;
        
        Debug.Log($"Returned {partName} to original position");
    }
    
    public void PlacePart(Transform newParent, Vector3 newPosition)
    {
        transform.SetParent(newParent);
        rectTransform.localPosition = newPosition;
        isPlaced = true;
        
        Debug.Log($"Placed {partName} at new position");
    }
    
    public void ResetPart()
    {
        isPlaced = false;
        ReturnToOriginalPosition();
    }
    
    // Public method to set part properties
    public void SetPartProperties(string name, PartType type, Sprite icon, Color color)
    {
        partName = name;
        partType = type;
        partIcon = icon;
        partColor = color;
        
        if (partImage != null)
        {
            partImage.sprite = partIcon;
            partImage.color = partColor;
        }
    }
}
