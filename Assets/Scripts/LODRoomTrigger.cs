using UnityEngine;

/// <summary>
/// Disables dynamic LOD adjustments and forces LOD0 (highest quality) when player enters the trigger.
/// Re-enables dynamic LOD when player exits. Useful for rooms where you want full quality.
/// </summary>
[RequireComponent(typeof(Collider))]
public class LODRoomTrigger : MonoBehaviour
{
    [Header("LOD Components to Control")]
    [SerializeField] private DynamicLOD[] dynamicLODs;
    
    [Header("Settings")]
    [Tooltip("Tag to check for player")]
    [SerializeField] private string playerTag = "Player";
    
    [Tooltip("Force LOD0 (highest quality) when player is in trigger")]
    [SerializeField] private bool forceLOD0 = true;
    
    private bool playerInside = false;
    
    private void OnValidate()
    {
        // Ensure the collider is set to trigger
        Collider col = GetComponent<Collider>();
        if (col != null && !col.isTrigger)
        {
            col.isTrigger = true;
            Debug.LogWarning($"[LODRoomTrigger] Collider on '{gameObject.name}' was not set as trigger. Fixed automatically.");
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInside = true;
            DisableDynamicLODs();
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInside = false;
            EnableDynamicLODs();
        }
    }
    
    private void DisableDynamicLODs()
    {
        if (dynamicLODs == null || dynamicLODs.Length == 0)
        {
            Debug.LogWarning($"[LODRoomTrigger] No DynamicLOD components assigned to '{gameObject.name}'");
            return;
        }
        
        foreach (var dynamicLOD in dynamicLODs)
        {
            if (dynamicLOD == null) continue;
            
            // Disable dynamic adjustments
            dynamicLOD.enableDynamic = false;
            
            // Force LOD0 (highest quality) if enabled
            if (forceLOD0)
            {
                LODGroup lodGroup = dynamicLOD.LODGroup;
                if (lodGroup != null)
                {
                    LOD[] lods = lodGroup.GetLODs();
                    
                    // screenRelativeTransitionHeight is where the LOD transitions to the NEXT level
                    // To force LOD0 to always show, we need LOD0 transition to be very low
                    // and other LODs to transition immediately after
                    if (lods.Length > 0)
                    {
                        lods[0].screenRelativeTransitionHeight = 0.01f; // LOD0 transitions to LOD1 at 1%
                    }
                    if (lods.Length > 1)
                    {
                        lods[1].screenRelativeTransitionHeight = 0.005f; // LOD1 transitions to LOD2 at 0.5%
                    }
                    if (lods.Length > 2)
                    {
                        lods[2].screenRelativeTransitionHeight = 0.0f; // LOD2 culls at 0%
                    }
                    
                    lodGroup.SetLODs(lods);
                }
            }
        }
        
        Debug.Log($"[LODRoomTrigger] Player entered '{gameObject.name}' - Disabled dynamic LODs ({dynamicLODs.Length} components)");
    }
    
    private void EnableDynamicLODs()
    {
        if (dynamicLODs == null || dynamicLODs.Length == 0)
            return;
        
        foreach (var dynamicLOD in dynamicLODs)
        {
            if (dynamicLOD == null) continue;
            
            // Re-enable dynamic adjustments (DynamicLODManager will restore the LOD distances)
            dynamicLOD.enableDynamic = true;
        }
        
        Debug.Log($"[LODRoomTrigger] Player exited '{gameObject.name}' - Re-enabled dynamic LODs ({dynamicLODs.Length} components)");
    }
    
    // Helper method to manually populate the array with all DynamicLOD components in children
    [ContextMenu("Find All DynamicLOD Components in Children")]
    private void FindDynamicLODsInChildren()
    {
        dynamicLODs = GetComponentsInChildren<DynamicLOD>();
        Debug.Log($"[LODRoomTrigger] Found {dynamicLODs.Length} DynamicLOD components in children of '{gameObject.name}'");
        
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        #endif
    }
    
    // Helper method to find all DynamicLOD components inside the box collider bounds
    [ContextMenu("Find All DynamicLOD Components Inside Box Collider")]
    private void FindDynamicLODsInsideBoxCollider()
    {
        BoxCollider boxCollider = GetComponent<BoxCollider>();
        if (boxCollider == null)
        {
            Debug.LogWarning($"[LODRoomTrigger] No BoxCollider found on '{gameObject.name}'. This function requires a BoxCollider.");
            return;
        }
        
        // Calculate world-space bounds of the box collider
        Bounds bounds = new Bounds(
            transform.TransformPoint(boxCollider.center),
            Vector3.zero
        );
        
        // Transform the box collider's size to world space
        Vector3 worldSize = Vector3.Scale(boxCollider.size, transform.lossyScale);
        bounds.size = worldSize;
        
        // Find all DynamicLOD components in the scene
        DynamicLOD[] allDynamicLODs = FindObjectsOfType<DynamicLOD>();
        System.Collections.Generic.List<DynamicLOD> lodsInside = new System.Collections.Generic.List<DynamicLOD>();
        
        foreach (var dynamicLOD in allDynamicLODs)
        {
            if (dynamicLOD == null) continue;
            
            // Check if the object's position is inside the bounds
            Vector3 objectPosition = dynamicLOD.transform.position;
            
            // For more accurate checking, we can use the object's bounds if it has a renderer
            Renderer renderer = dynamicLOD.GetComponent<Renderer>();
            if (renderer != null)
            {
                // Check if renderer bounds intersect with trigger bounds
                if (bounds.Intersects(renderer.bounds))
                {
                    lodsInside.Add(dynamicLOD);
                }
            }
            else
            {
                // Fallback to point-in-bounds check for objects without renderers
                if (bounds.Contains(objectPosition))
                {
                    lodsInside.Add(dynamicLOD);
                }
            }
        }
        
        dynamicLODs = lodsInside.ToArray();
        Debug.Log($"[LODRoomTrigger] Found {dynamicLODs.Length} DynamicLOD components inside box collider bounds of '{gameObject.name}'");
        
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        #endif
    }
    
    private void OnDrawGizmos()
    {
        // Draw the trigger volume
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.color = playerInside ? new Color(0, 1, 0, 0.3f) : new Color(1, 1, 0, 0.3f);
            
            if (col is BoxCollider)
            {
                BoxCollider box = col as BoxCollider;
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawCube(box.center, box.size);
            }
            else if (col is SphereCollider)
            {
                SphereCollider sphere = col as SphereCollider;
                Gizmos.DrawSphere(transform.position + sphere.center, sphere.radius * transform.lossyScale.x);
            }
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        // Draw lines to all assigned DynamicLOD components
        if (dynamicLODs != null)
        {
            Gizmos.color = Color.cyan;
            foreach (var dynamicLOD in dynamicLODs)
            {
                if (dynamicLOD != null)
                {
                    Gizmos.DrawLine(transform.position, dynamicLOD.transform.position);
                }
            }
        }
    }
}
