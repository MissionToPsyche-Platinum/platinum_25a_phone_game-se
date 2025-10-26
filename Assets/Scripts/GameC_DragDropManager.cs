using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class DragDropManager : MonoBehaviour
{
    [Header("Drag Drop Settings")]
    [SerializeField] private bool enableDragDrop = true;
    
    private static DragDropManager _instance;
    public static DragDropManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<DragDropManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("DragDropManager");
                    _instance = go.AddComponent<DragDropManager>();
                }
            }
            return _instance;
        }
    }
    
    private SpaceCraftPart currentlyDraggedPart;
    private List<SpaceCraftPart> allParts = new List<SpaceCraftPart>();
    private List<DropZone> allDropZones = new List<DropZone>();
    
    public SpaceCraftPart CurrentlyDraggedPart => currentlyDraggedPart;
    public bool IsDragging => currentlyDraggedPart != null;
    
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            EnsureEventSystem();
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    private void EnsureEventSystem()
    {
        // Check if there's already an EventSystem in the scene
        EventSystem existingEventSystem = FindFirstObjectByType<EventSystem>();
        if (existingEventSystem == null)
        {
            // Create EventSystem only if none exists
            GameObject eventSystemGO = new GameObject("EventSystem");
            eventSystemGO.AddComponent<EventSystem>();
            eventSystemGO.AddComponent<StandaloneInputModule>();
            Debug.Log("Created EventSystem for DragDropManager");
        }
        else
        {
            Debug.Log("EventSystem already exists in scene");
        }
    }
    
    private void Start()
    {
        // Find all parts and drop zones in the scene
        RefreshPartsAndZones();
    }
    
    public void RefreshPartsAndZones()
    {
        // Find all SpaceCraftParts
        allParts.Clear();
        SpaceCraftPart[] parts = FindObjectsByType<SpaceCraftPart>(FindObjectsSortMode.None);
        allParts.AddRange(parts);
        
        // Find all DropZones
        allDropZones.Clear();
        DropZone[] zones = FindObjectsByType<DropZone>(FindObjectsSortMode.None);
        allDropZones.AddRange(zones);
        
        Debug.Log($"Found {allParts.Count} parts and {allDropZones.Count} drop zones");
    }
    
    public void OnBeginDrag(SpaceCraftPart part)
    {
        if (!enableDragDrop) return;
        
        currentlyDraggedPart = part;
        
        // Notify all drop zones that dragging started
        foreach (var zone in allDropZones)
        {
            zone.OnPointerEnter(null); // Trigger highlight check
        }
        
        Debug.Log($"Started dragging {part.PartName}");
    }
    
    public void OnEndDrag(SpaceCraftPart part, bool wasPlaced)
    {
        if (!enableDragDrop) return;
        
        currentlyDraggedPart = null;
        
        // Notify all drop zones that dragging ended
        foreach (var zone in allDropZones)
        {
            zone.OnPointerExit(null); // Remove highlights
        }
        
        if (wasPlaced)
        {
            OnPartPlaced(part);
        }
        
        Debug.Log($"Ended dragging {part.PartName}, placed: {wasPlaced}");
    }
    
    private void OnPartPlaced(SpaceCraftPart part)
    {
        // Check if all parts are placed
        int placedPartsCount = 0;
        foreach (var p in allParts)
        {
            if (p.IsPlaced)
            {
                placedPartsCount++;
            }
        }
        
        Debug.Log($"Parts placed: {placedPartsCount}/{allParts.Count}");
        
        // Check for completion
        if (placedPartsCount == allParts.Count)
        {
            OnAllPartsPlaced();
        }
    }
    
    private void OnAllPartsPlaced()
    {
        Debug.Log("All parts have been placed!");
        
        // Trigger completion event
        OnSpacecraftCompleted?.Invoke();
    }
    
    // Events
    public System.Action OnSpacecraftCompleted;
    
    // Public methods for external control
    public void EnableDragDrop(bool enable)
    {
        enableDragDrop = enable;
        
        if (!enable && IsDragging)
        {
            // Return currently dragged part to original position
            currentlyDraggedPart?.ReturnToOriginalPosition();
            currentlyDraggedPart = null;
        }
    }
    
    public void ResetAllParts()
    {
        foreach (var part in allParts)
        {
            part.ResetPart();
        }
        
        foreach (var zone in allDropZones)
        {
            zone.ClearAllParts();
        }
        
        Debug.Log("Reset all parts to original positions");
    }
    
    public int GetPlacedPartsCount()
    {
        int count = 0;
        foreach (var part in allParts)
        {
            if (part.IsPlaced)
            {
                count++;
            }
        }
        return count;
    }
    
    public int GetTotalPartsCount()
    {
        return allParts.Count;
    }
    
    public List<SpaceCraftPart> GetUnplacedParts()
    {
        List<SpaceCraftPart> unplaced = new List<SpaceCraftPart>();
        foreach (var part in allParts)
        {
            if (!part.IsPlaced)
            {
                unplaced.Add(part);
            }
        }
        return unplaced;
    }
    
    public List<SpaceCraftPart> GetPlacedParts()
    {
        List<SpaceCraftPart> placed = new List<SpaceCraftPart>();
        foreach (var part in allParts)
        {
            if (part.IsPlaced)
            {
                placed.Add(part);
            }
        }
        return placed;
    }
}
