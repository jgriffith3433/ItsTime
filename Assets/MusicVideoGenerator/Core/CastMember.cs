using UnityEngine;

namespace MusicVideoGenerator
{
    /// <summary>
    /// Represents a single cast member with their assigned role and GameObject.
    /// This is the simplified version - just drag, drop, and assign a role!
    /// </summary>
    [System.Serializable]
    public class CastMember
    {
        [Tooltip("The GameObject to animate")]
        public GameObject gameObject;
        
        [Tooltip("The role this cast member plays in the story")]
        public CastRole role = CastRole.BackgroundSupport;
        
        [Header("Optional Overrides")]
        [Tooltip("Override the default color palette for this specific member")]
        public bool useCustomColors = false;
        
        [Tooltip("Custom primary color (only used if useCustomColors is true)")]
        public Color customPrimaryColor = Color.white;
        
        [Tooltip("Custom secondary color (only used if useCustomColors is true)")]
        public Color customSecondaryColor = Color.cyan;
        
        [Space(10)]
        [Range(0f, 10f)]
        [Tooltip("Delay before this member starts animating (seconds)")]
        public float animationDelay = 0f;
        
        // Cached behavior config
        private RoleBehaviorConfig cachedConfig;
        
        /// <summary>
        /// Get the behavior configuration for this cast member's role.
        /// </summary>
        public RoleBehaviorConfig GetBehaviorConfig()
        {
            if (cachedConfig == null || cachedConfig.role != role)
            {
                cachedConfig = RoleBehaviorConfig.CreateDefault(role);
                
                // Apply custom colors if specified
                if (useCustomColors)
                {
                    cachedConfig.primaryColor = customPrimaryColor;
                    cachedConfig.secondaryColor = customSecondaryColor;
                }
            }
            
            return cachedConfig;
        }
        
        /// <summary>
        /// Check if this cast member is valid and ready for animation.
        /// </summary>
        public bool IsValid()
        {
            return gameObject != null;
        }
        
        /// <summary>
        /// Get the local transform data for this cast member.
        /// </summary>
        public TransformData GetInitialTransform()
        {
            if (gameObject == null)
            {
                return new TransformData();
            }
            
            return new TransformData
            {
                position = gameObject.transform.localPosition,
                rotation = gameObject.transform.localRotation,
                scale = gameObject.transform.localScale
            };
        }
        
        /// <summary>
        /// Check if this is a main cast member (lead/co-star/featured).
        /// </summary>
        public bool IsMainCast()
        {
            return role == CastRole.LeadStar || 
                   role == CastRole.CoStar || 
                   role == CastRole.FeaturedSupport;
        }
        
        /// <summary>
        /// Check if this is a supporting cast member (background/ambient).
        /// </summary>
        public bool IsSupportingCast()
        {
            return role == CastRole.BackgroundSupport || 
                   role == CastRole.Ambient;
        }
        
        /// <summary>
        /// Get a descriptive name for this cast member.
        /// </summary>
        public string GetDisplayName()
        {
            string objName = gameObject != null ? gameObject.name : "NULL";
            return $"{objName} ({role})";
        }
    }
    
    /// <summary>
    /// Stores transform data for animation.
    /// </summary>
    public struct TransformData
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
    }
}