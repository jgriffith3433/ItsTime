using UnityEngine;

namespace MusicVideoGenerator
{
    /// <summary>
    /// Defines the role types available for cast members.
    /// Each role has specific animation behaviors and importance in the story.
    /// </summary>
    public enum CastRole
    {
        /// <summary>Main character - center stage, dramatic movements</summary>
        LeadStar,
        
        /// <summary>Secondary main character - important, but not always center</summary>
        CoStar,
        
        /// <summary>Important supporting character - gets moments to shine</summary>
        FeaturedSupport,
        
        /// <summary>Background support - simple movements, less focus</summary>
        BackgroundSupport,
        
        /// <summary>Ambient element - subtle effects, atmosphere</summary>
        Ambient,
        
        /// <summary>Special spotlight moments - dramatic entrances/exits</summary>
        Spotlight
    }

    /// <summary>
    /// Configuration for how a specific role should behave during animation.
    /// </summary>
    [System.Serializable]
    public class RoleBehaviorConfig
    {
        public CastRole role;
        
        [Header("Movement")]
        public bool canMove = true;
        public float movementIntensity = 1f;
        public float movementRange = 10f;
        
        [Header("Rotation")]
        public bool canRotate = true;
        public float rotationSpeed = 180f;
        
        [Header("Scaling")]
        public bool canScale = true;
        public float scaleMin = 0.5f;
        public float scaleMax = 2f;
        
        [Header("Color")]
        public bool canChangeColor = true;
        public float colorTransitionSpeed = 1f;
        
        [Header("Story Importance")]
        [Range(0f, 1f)]
        [Tooltip("How often this role should be in focus (0 = never, 1 = always)")]
        public float focusProbability = 0.5f;
        
        [Range(0f, 2f)]
        [Tooltip("Energy multiplier for this role")]
        public float energyMultiplier = 1f;
        
        [Header("Color Palette")]
        public Color primaryColor = Color.white;
        public Color secondaryColor = Color.cyan;
        
        /// <summary>
        /// Creates default configuration for a given role.
        /// </summary>
        public static RoleBehaviorConfig CreateDefault(CastRole role)
        {
            var config = new RoleBehaviorConfig { role = role };
            
            switch (role)
            {
                case CastRole.LeadStar:
                    config.movementIntensity = 1.5f;
                    config.movementRange = 15f;
                    config.rotationSpeed = 360f;
                    config.scaleMin = 0.8f;
                    config.scaleMax = 3f;
                    config.focusProbability = 0.8f;
                    config.energyMultiplier = 1.8f;
                    config.primaryColor = new Color(1f, 0.8f, 0.2f); // Gold
                    config.secondaryColor = new Color(1f, 0.4f, 0.1f); // Orange
                    break;
                
                case CastRole.CoStar:
                    config.movementIntensity = 1.3f;
                    config.movementRange = 12f;
                    config.rotationSpeed = 270f;
                    config.scaleMin = 0.7f;
                    config.scaleMax = 2.5f;
                    config.focusProbability = 0.6f;
                    config.energyMultiplier = 1.5f;
                    config.primaryColor = new Color(0.2f, 0.8f, 1f); // Cyan
                    config.secondaryColor = new Color(0.5f, 0.5f, 1f); // Light blue
                    break;
                
                case CastRole.FeaturedSupport:
                    config.movementIntensity = 1f;
                    config.movementRange = 10f;
                    config.rotationSpeed = 180f;
                    config.scaleMin = 0.6f;
                    config.scaleMax = 2f;
                    config.focusProbability = 0.4f;
                    config.energyMultiplier = 1.2f;
                    config.primaryColor = new Color(0.2f, 1f, 0.5f); // Green
                    config.secondaryColor = new Color(0.5f, 1f, 0.8f); // Light green
                    break;
                
                case CastRole.BackgroundSupport:
                    config.movementIntensity = 0.5f;
                    config.movementRange = 5f;
                    config.canMove = true;
                    config.canRotate = true;
                    config.canScale = true;
                    config.rotationSpeed = 90f;
                    config.scaleMin = 0.8f;
                    config.scaleMax = 1.5f;
                    config.focusProbability = 0.2f;
                    config.energyMultiplier = 0.8f;
                    config.primaryColor = new Color(0.5f, 0.5f, 1f); // Lavender
                    config.secondaryColor = new Color(0.7f, 0.7f, 1f); // Light lavender
                    break;
                
                case CastRole.Ambient:
                    config.movementIntensity = 0.3f;
                    config.movementRange = 3f;
                    config.canMove = false;
                    config.canRotate = false;
                    config.canScale = true;
                    config.rotationSpeed = 45f;
                    config.scaleMin = 0.9f;
                    config.scaleMax = 1.2f;
                    config.focusProbability = 0.1f;
                    config.energyMultiplier = 0.6f;
                    config.primaryColor = new Color(0.3f, 0.3f, 0.5f); // Dark blue
                    config.secondaryColor = new Color(0.5f, 0.3f, 0.5f); // Purple
                    break;
                
                case CastRole.Spotlight:
                    config.movementIntensity = 2f;
                    config.movementRange = 20f;
                    config.rotationSpeed = 540f;
                    config.scaleMin = 0.5f;
                    config.scaleMax = 4f;
                    config.focusProbability = 0.9f;
                    config.energyMultiplier = 2f;
                    config.primaryColor = Color.white;
                    config.secondaryColor = new Color(1f, 1f, 0.5f); // Light yellow
                    break;
            }
            
            return config;
        }
    }
}