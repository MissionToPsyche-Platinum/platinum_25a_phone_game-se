using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DropZone : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Drop Zone Properties")]
    public Color acceptedColor;
    
    private Image image;
    private bool hasItem = false;
    private DraggableItem currentItem;
    
    void Start()
    {
        image = GetComponent<Image>();
        
        // Set initial outline color
        if (image != null)
        {
            Color outlineColor = acceptedColor;
            outlineColor.a = 0.3f;
            image.color = outlineColor;
        }
    }
    
    public bool CanAccept(Color itemColor)
    {
        if (hasItem) return false;
        
        // Check if colors match (with some tolerance for float precision)
        return Mathf.Abs(itemColor.r - acceptedColor.r) < 0.1f &&
               Mathf.Abs(itemColor.g - acceptedColor.g) < 0.1f &&
               Mathf.Abs(itemColor.b - acceptedColor.b) < 0.1f;
    }
    
    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log($"OnDrop called on {gameObject.name}");
        GameObject droppedObject = eventData.pointerDrag;
        
        if (droppedObject != null && !hasItem)
        {
            Debug.Log($"Dropped object: {droppedObject.name}");
            DraggableItem dragComponent = droppedObject.GetComponent<DraggableItem>();
            if (dragComponent != null && CanAccept(dragComponent.itemColor))
            {
                Debug.Log($"Accepting item in {gameObject.name}");
                // Accept the item
                hasItem = true;
                currentItem = dragComponent;
                
                // Hide the outline since we have the item
                if (image != null)
                {
                    Color c = image.color;
                    c.a = 0.1f;
                    image.color = c;
                }
            }
            else
            {
                Debug.Log($"Cannot accept item - hasItem: {hasItem}, CanAccept: {(dragComponent != null ? CanAccept(dragComponent.itemColor) : false)}");
            }
        }
        else
        {
            Debug.Log($"Cannot drop - droppedObject: {(droppedObject != null ? "exists" : "null")}, hasItem: {hasItem}");
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Visual feedback when hovering over drop zone during drag
        if (eventData.pointerDrag != null && !hasItem)
        {
            DraggableItem dragComponent = eventData.pointerDrag.GetComponent<DraggableItem>();
            if (dragComponent != null && CanAccept(dragComponent.itemColor))
            {
                // Highlight the zone
                if (image != null)
                {
                    Color c = acceptedColor;
                    c.a = 0.6f;
                    image.color = c;
                }
            }
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        // Remove highlight when leaving drop zone
        if (!hasItem && image != null)
        {
            Color c = acceptedColor;
            c.a = 0.3f;
            image.color = c;
        }
    }
    
    public void SetItem(DraggableItem item)
    {
        if (item != null)
        {
            hasItem = true;
            currentItem = item;
            
            // Hide the outline
            if (image != null)
            {
                Color c = image.color;
                c.a = 0.1f;
                image.color = c;
            }
        }
    }
    
    public void RemoveItem()
    {
        if (hasItem)
        {
            hasItem = false;
            currentItem = null;
            
            // Restore outline
            if (image != null)
            {
                Color c = acceptedColor;
                c.a = 0.3f;
                image.color = c;
            }
        }
    }
    
    void OnDestroy()
    {
        // Cleanup
        image = null;
        currentItem = null;
    }
}
