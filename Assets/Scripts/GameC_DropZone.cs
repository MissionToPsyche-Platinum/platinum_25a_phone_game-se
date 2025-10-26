using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class DropZone : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Drop Zone Settings")]
    [SerializeField] private bool acceptAllParts = true;
    [SerializeField] private List<SpaceCraftPart.PartType> acceptedPartTypes = new List<SpaceCraftPart.PartType>();
    [SerializeField] private int maxParts = 5;
    [SerializeField] private Color highlightColor = Color.green;
    [SerializeField] private Color normalColor = Color.white;
    
    [Header("Visual Feedback")]
    [SerializeField] private bool showVisualFeedback = true;
    [SerializeField] private Image dropZoneImage;
    
    private List<SpaceCraftPart> placedParts = new List<SpaceCraftPart>();
    private Color originalColor;
    
    private void Awake()
    {
        // Get the image component for visual feedback
        if (dropZoneImage == null)
        {
            dropZoneImage = GetComponent<Image>();
        }
        
        if (dropZoneImage != null)
        {
            originalColor = dropZoneImage.color;
        }
    }
    
    public bool CanAcceptPart(SpaceCraftPart part)
    {
        if (part == null) return false;
        
        // Check if already placed
        if (part.IsPlaced)
        {
            Debug.Log($"Part {part.PartName} is already placed");
            return false;
        }
        
        // Check capacity
        if (placedParts.Count >= maxParts)
        {
            Debug.Log($"Drop zone is full (max {maxParts} parts)");
            return false;
        }
        
        // Check if zone accepts all parts or specific types
        if (!acceptAllParts && !acceptedPartTypes.Contains(part.Type))
        {
            Debug.Log($"Drop zone doesn't accept part type: {part.Type}");
            return false;
        }
        
        return true;
    }
    
    public void OnPartDropped(SpaceCraftPart part)
    {
        if (!CanAcceptPart(part)) return;
        
        // Calculate position for the part (simple grid layout)
        Vector3 dropPosition = CalculateDropPosition();
        
        // Place the part
        part.PlacePart(transform, dropPosition);
        placedParts.Add(part);
        
        // Update visual feedback
        UpdateVisualFeedback();
        
        // Notify completion
        OnPartPlaced(part);
        
        Debug.Log($"Part {part.PartName} dropped successfully");
    }
    
    private Vector3 CalculateDropPosition()
    {
        // Simple grid-based positioning
        int partsPerRow = 3;
        float spacing = 120f;
        
        int index = placedParts.Count;
        int row = index / partsPerRow;
        int col = index % partsPerRow;
        
        float x = (col - partsPerRow / 2f) * spacing;
        float y = -row * spacing;
        
        return new Vector3(x, y, 0);
    }
    
    private void UpdateVisualFeedback()
    {
        if (!showVisualFeedback || dropZoneImage == null) return;
        
        // Change color based on fill level
        float fillRatio = (float)placedParts.Count / maxParts;
        Color currentColor;
        
        if (fillRatio >= 1f)
        {
            currentColor = Color.red; // Full
        }
        else if (fillRatio >= 0.7f)
        {
            currentColor = Color.yellow; // Almost full
        }
        else
        {
            currentColor = Color.green; // Available space
        }
        
        dropZoneImage.color = currentColor;
    }
    
    protected virtual void OnPartPlaced(SpaceCraftPart part)
    {
        // Override this method in derived classes for custom behavior
        Debug.Log($"Part {part.PartName} of type {part.Type} was placed in drop zone");
    }
    
    public void OnDrop(PointerEventData eventData)
    {
        // This is called when something is dropped on this GameObject
        // The actual logic is handled in SpaceCraftPart.OnEndDrag
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!showVisualFeedback || dropZoneImage == null) return;
        
        // Check if we're dragging a valid part
        if (DragDropManager.Instance != null && DragDropManager.Instance.IsDragging)
        {
            SpaceCraftPart draggedPart = DragDropManager.Instance.CurrentlyDraggedPart;
            if (draggedPart != null && CanAcceptPart(draggedPart))
            {
                dropZoneImage.color = highlightColor;
            }
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!showVisualFeedback || dropZoneImage == null) return;
        
        // Restore normal color
        UpdateVisualFeedback();
    }
    
    public void ClearAllParts()
    {
        foreach (var part in placedParts)
        {
            if (part != null)
            {
                part.ResetPart();
            }
        }
        placedParts.Clear();
        UpdateVisualFeedback();
    }
    
    public int GetPlacedPartsCount()
    {
        return placedParts.Count;
    }
    
    public List<SpaceCraftPart> GetPlacedParts()
    {
        return new List<SpaceCraftPart>(placedParts);
    }
}
