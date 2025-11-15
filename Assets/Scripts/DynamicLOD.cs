using UnityEngine;

/// <summary>
/// Component that marks a GameObject with an LODGroup for dynamic LOD management.
/// Add this component to enable runtime LOD adjustments based on performance.
/// </summary>
[RequireComponent(typeof(LODGroup))]
public class DynamicLOD : MonoBehaviour
{
    [Header("Dynamic LOD Settings")]
    [Tooltip("Enable dynamic LOD adjustments at runtime")]
    public bool enableDynamic = true;
    
    [Header("Component References")]
    [Tooltip("Reference to the LODGroup component")]
    [SerializeField] private LODGroup lodGroup;
    
    /// <summary>
    /// Gets the LODGroup component reference
    /// </summary>
    public LODGroup LODGroup
    {
        get
        {
            if (lodGroup == null)
            {
                lodGroup = GetComponent<LODGroup>();
            }
            return lodGroup;
        }
    }
    
    private void Awake()
    {
        // Ensure we have a reference to the LODGroup
        if (lodGroup == null)
        {
            lodGroup = GetComponent<LODGroup>();
        }
    }
    
    private void OnValidate()
    {
        // Automatically get the LODGroup component in the editor
        if (lodGroup == null)
        {
            lodGroup = GetComponent<LODGroup>();
        }
    }
}
