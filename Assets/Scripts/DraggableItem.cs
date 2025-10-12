using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Item Properties")]
    public Color itemColor;
    
    private Vector3 startPosition;
    private Transform startParent;
    private Canvas canvas;
    private Image image;
    private bool isBeingDragged = false;
    
    void Start()
    {
        canvas = GetComponentInParent<Canvas>();
        image = GetComponent<Image>();
        
        // Safety check
        if (canvas == null)
        {
            Debug.LogError($"Canvas not found for {gameObject.name}!");
            enabled = false;
        }
    }
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isBeingDragged) return;
        
        Debug.Log($"Starting drag for {gameObject.name} with color {itemColor}");
        Debug.Log($"EventSystem current: {UnityEngine.EventSystems.EventSystem.current != null}");
        Debug.Log($"Canvas: {(canvas != null ? canvas.name : "null")}");
        isBeingDragged = true;
        startPosition = transform.position;
        startParent = transform.parent;
        
        // Visual feedback - make semi-transparent
        if (image != null)
        {
            Color c = image.color;
            c.a = 0.6f;
            image.color = c;
        }
        
        // Move to canvas top level so it appears above everything
        if (canvas != null)
        {
            transform.SetParent(canvas.transform, true);
        }
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        if (!isBeingDragged || canvas == null) return;
        
        // Follow mouse position
        Vector2 position;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform, 
            eventData.position, 
            eventData.pressEventCamera, 
            out position))
        {
            transform.position = canvas.transform.TransformPoint(position);
        }
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isBeingDragged) return;
        
        Debug.Log($"Ending drag for {gameObject.name}");
        isBeingDragged = false;
        
        // Restore visual
        if (image != null)
        {
            Color c = image.color;
            c.a = 1f;
            image.color = c;
        }
        
        // Use multiple methods to find drop target
        DropZone dropZone = null;
        
        // Method 1: Check raycast result from eventData
        GameObject droppedOn = eventData.pointerCurrentRaycast.gameObject;
        Debug.Log($"EventData raycast hit: {(droppedOn != null ? droppedOn.name : "null")}");
        
        // Method 1a: Manual raycast as backup
        if (droppedOn == null)
        {
            GraphicRaycaster raycaster = canvas.GetComponent<GraphicRaycaster>();
            if (raycaster != null)
            {
                var results = new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();
                raycaster.Raycast(eventData, results);
                Debug.Log($"Manual raycast found {results.Count} results");
                
                foreach (var result in results)
                {
                    Debug.Log($"Manual raycast hit: {result.gameObject.name}");
                    if (result.gameObject.GetComponent<DropZone>() != null)
                    {
                        droppedOn = result.gameObject;
                        break;
                    }
                }
            }
        }
        
        if (droppedOn != null)
        {
            dropZone = droppedOn.GetComponent<DropZone>();
        }
        
        // Method 2: If raycast failed, check all drop zones by distance
        if (dropZone == null)
        {
            Debug.Log("Raycast failed, checking by distance...");
            DropZone[] allDropZones = FindObjectsOfType<DropZone>();
            float minDistance = float.MaxValue;
            DropZone closestZone = null;
            
            foreach (DropZone zone in allDropZones)
            {
                float distance = Vector3.Distance(transform.position, zone.transform.position);
                Debug.Log($"Distance to {zone.name}: {distance}");
                
                // If within drop range (100 pixels)
                if (distance < 100f && distance < minDistance)
                {
                    minDistance = distance;
                    closestZone = zone;
                }
            }
            
            if (closestZone != null)
            {
                Debug.Log($"Found closest zone: {closestZone.name}");
                dropZone = closestZone;
            }
        }
        
        // Check if we can drop
        if (dropZone != null)
        {
            Debug.Log($"Found drop zone: {dropZone.name}");
            Debug.Log($"Zone accepted color: {dropZone.acceptedColor}");
            Debug.Log($"Item color: {itemColor}");
            Debug.Log($"Can accept: {dropZone.CanAccept(itemColor)}");
            
            if (dropZone.CanAccept(itemColor))
            {
                Debug.Log("Successfully dropped item!");
                // Manually trigger OnDrop on the DropZone to ensure proper handling
                dropZone.OnDrop(eventData);
                
                // Successfully dropped on matching zone
                transform.SetParent(dropZone.transform, false);
                transform.localPosition = Vector3.zero;
                dropZone.SetItem(this);
                return;
            }
            else
            {
                Debug.Log("Color mismatch - cannot drop here");
            }
        }
        
        Debug.Log("Returning item to original position");
        // Return to original position
        if (startParent != null)
        {
            transform.SetParent(startParent, false);
            transform.position = startPosition;
        }
    }
    
    void OnDestroy()
    {
        // Cleanup
        canvas = null;
        image = null;
        startParent = null;
    }
}
