using UnityEngine;

namespace MusicVideoGenerator
{
    /// <summary>
    /// Identifies the procedural shape type and stores metadata for shape-aware animation.
    /// Attached to parent objects of compound shapes.
    /// </summary>
    public class ShapeIdentity : MonoBehaviour
    {
        [Header("Shape Information")]
        [Tooltip("Type of procedural shape")]
        public ShapeType shapeType;
        
        [Tooltip("Original scale used during creation")]
        public Vector3 originalScale = Vector3.one;
        
        [Tooltip("Theme color used")]
        public Color primaryColor = Color.white;
        
        [Tooltip("Theme this shape belongs to")]
        public string themeName = "";
        
        [Header("Shape-Specific Data")]
        [Tooltip("Number of elements in this shape (satellites, petals, segments, etc.)")]
        public int elementCount = 0;
        
        [Tooltip("Radius or size parameter")]
        public float shapeRadius = 1f;
        
        [Tooltip("Height or vertical extent")]
        public float shapeHeight = 1f;
        
        [Tooltip("Rotation axis for spinning shapes")]
        public Vector3 rotationAxis = Vector3.up;
        
        [Tooltip("Natural animation speed multiplier")]
        public float animationSpeed = 1f;
        
        /// <summary>
        /// Get all child renderers that make up this shape
        /// </summary>
        public Renderer[] GetShapeElements()
        {
            return GetComponentsInChildren<Renderer>();
        }
        
        /// <summary>
        /// Get element at specific index (for orbital systems, petals, etc.)
        /// </summary>
        public Transform GetElement(int index)
        {
            if (index < 0 || index >= transform.childCount) return null;
            return transform.GetChild(index);
        }
    }
}
