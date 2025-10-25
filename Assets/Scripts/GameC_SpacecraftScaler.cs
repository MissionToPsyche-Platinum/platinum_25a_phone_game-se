using UnityEngine;

public class GameC_SpacecraftScaler : MonoBehaviour
{
    [Header("Scaling Settings")]
    [SerializeField] private float fixedWidth = 800f;
    [SerializeField] private float fixedHeight = 400f;
    
    [Header("Mobile Detection")]
    [SerializeField] private bool forceMobileMode = false;
    [SerializeField] private float mobileBreakpoint = 768f; // Width threshold for mobile detection
    
    [Header("References")]
    [SerializeField] private GameObject spacecraftModel;
    [SerializeField] private RectTransform parentPanel; // Should be the SpaceCraftPanel RectTransform, NOT the Canvas
    
    private void Start()
    {
        ScaleSpacecraft();
    }
    
    private void ScaleSpacecraft()
    {
        if (spacecraftModel == null)
        {
            Debug.LogError("Spacecraft model not assigned!");
            return;
        }
        
        if (parentPanel == null)
        {
            Debug.LogError("Parent panel not assigned!");
            return;
        }
        
        // Debug: Log the SpaceCraftPanel dimensions
        Debug.Log($"SpaceCraftPanel dimensions: Width={parentPanel.rect.width}, Height={parentPanel.rect.height}");
        Debug.Log($"SpaceCraftPanel anchored position: {parentPanel.anchoredPosition}");
        Debug.Log($"SpaceCraftPanel size delta: {parentPanel.sizeDelta}");
        Debug.Log($"Fixed dimensions: Width={fixedWidth}, Height={fixedHeight}");
        
        // Determine if we're on mobile or PC
        bool isMobile = IsMobileDevice();
        
        if (isMobile)
        {
            ScaleForMobile();
        }
        else
        {
            ScaleForPC();
        }
    }
    
    private bool IsMobileDevice()
    {
        if (forceMobileMode)
            return true;
            
        // Check screen width for mobile detection
        float screenWidth = Screen.width;
        return screenWidth <= mobileBreakpoint;
    }
    
    private void ScaleForPC()
    {
        // Scale to fixed width (800 pixels)
        float targetWidth = fixedWidth;
        
        // Get current spacecraft bounds (in local space)
        Bounds spacecraftBounds = GetSpacecraftLocalBounds();
        float currentWidth = spacecraftBounds.size.x;
        
        Debug.Log($"PC Scaling Debug: Target width {targetWidth}, Current spacecraft width {currentWidth}");
        
        if (currentWidth <= 0)
        {
            Debug.LogError("Could not determine spacecraft width!");
            return;
        }
        
        // Calculate scale factor
        float scaleFactor = targetWidth / currentWidth;
        
        // Clamp scale factor to reasonable values (prevent extreme scaling)
        scaleFactor = Mathf.Clamp(scaleFactor, 0.1f, 100f);
        
        // Apply uniform scaling
        spacecraftModel.transform.localScale = Vector3.one * scaleFactor;
        
        // Center the spacecraft in the parent panel
        CenterSpacecraft();
        
        Debug.Log($"PC Scaling: Target width {targetWidth}, Scale factor {scaleFactor}");
    }
    
    private void ScaleForMobile()
    {
        // Scale to fixed height (400 pixels)
        float targetHeight = fixedHeight;
        
        // Get current spacecraft bounds (in local space)
        Bounds spacecraftBounds = GetSpacecraftLocalBounds();
        float currentHeight = spacecraftBounds.size.y;
        
        Debug.Log($"Mobile Scaling Debug: Target height {targetHeight}, Current spacecraft height {currentHeight}");
        
        if (currentHeight <= 0)
        {
            Debug.LogError("Could not determine spacecraft height!");
            return;
        }
        
        // Calculate scale factor
        float scaleFactor = targetHeight / currentHeight;
        
        // Clamp scale factor to reasonable values (prevent extreme scaling)
        scaleFactor = Mathf.Clamp(scaleFactor, 0.1f, 100f);
        
        // Apply uniform scaling
        spacecraftModel.transform.localScale = Vector3.one * scaleFactor;
        
        // Center the spacecraft in the parent panel
        CenterSpacecraft();
        
        Debug.Log($"Mobile Scaling: Target height {targetHeight}, Scale factor {scaleFactor}");
    }
    
    private Bounds GetSpacecraftLocalBounds()
    {
        // Get the mesh filter to calculate local bounds
        MeshFilter meshFilter = spacecraftModel.GetComponent<MeshFilter>();
        if (meshFilter != null && meshFilter.mesh != null)
        {
            Bounds bounds = meshFilter.mesh.bounds;
            Debug.Log($"Spacecraft bounds from MeshFilter: {bounds.size}");
            return bounds;
        }
        
        // Try to get bounds from child mesh filters
        MeshFilter[] childMeshFilters = spacecraftModel.GetComponentsInChildren<MeshFilter>();
        if (childMeshFilters.Length > 0)
        {
            Bounds combinedBounds = childMeshFilters[0].mesh.bounds;
            for (int i = 1; i < childMeshFilters.Length; i++)
            {
                if (childMeshFilters[i].mesh != null)
                {
                    combinedBounds.Encapsulate(childMeshFilters[i].mesh.bounds);
                }
            }
            Debug.Log($"Spacecraft bounds from child MeshFilters: {combinedBounds.size}");
            return combinedBounds;
        }
        
        Debug.LogError("No mesh found on spacecraft model! Using default bounds.");
        return new Bounds(Vector3.zero, Vector3.one); // Return default bounds
    }
    
    private void CenterSpacecraft()
    {
        // Center the spacecraft in the parent panel (both horizontally and vertically)
        spacecraftModel.transform.localPosition = Vector3.zero;
        
        // If the spacecraft has an offset center, adjust for it
        Bounds localBounds = GetSpacecraftLocalBounds();
        Vector3 centerOffset = -localBounds.center;
        spacecraftModel.transform.localPosition = centerOffset;
    }
    
    // Public method to manually trigger rescaling (useful for testing)
    [ContextMenu("Rescale Spacecraft")]
    public void RescaleSpacecraft()
    {
        ScaleSpacecraft();
    }
    
    // Method to update scaling when screen orientation changes
    private void OnRectTransformDimensionsChange()
    {
        // Only rescale if the component is active
        if (gameObject.activeInHierarchy)
        {
            ScaleSpacecraft();
        }
    }
}
