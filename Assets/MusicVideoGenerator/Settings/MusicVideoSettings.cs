using UnityEngine;

namespace MusicVideoGenerator
{
    /// <summary>
    /// Settings for music video generation.
    /// Simple and focused - roles handle most behavior configuration.
    /// </summary>
    [CreateAssetMenu(fileName = "MusicVideoSettings", menuName = "Music Video/Settings")]
    public class MusicVideoSettings : ScriptableObject
    {
        [Header("📊 Music Analysis Data")]
        [Tooltip("Aubio beats file")]
        public TextAsset beatsFile;
        
        [Tooltip("Aubio onsets file")]
        public TextAsset onsetsFile;
        
        [Tooltip("Aubio notes file")]
        public TextAsset notesFile;
        
        [Tooltip("Aubio pitch file")]
        public TextAsset pitchFile;
        
        [Tooltip("Aubio silence file")]
        public TextAsset silenceFile;
        
        [Header("🎲 Generation")]
        [Tooltip("Random seed - same seed = same result")]
        public int randomSeed = 12345;
        
        [Range(0.5f, 2f)]
        [Tooltip("Overall story intensity multiplier")]
        public float storyIntensity = 1f;
        
        [Header("🎵 Music Analysis")]
        [Range(1f, 10f)]
        [Tooltip("Time window for analyzing music sections")]
        public float musicAnalysisWindowSize = 4f;
        
        [Range(0.1f, 2f)]
        [Tooltip("Intensity threshold for section classification")]
        public float intensityThreshold = 0.5f;
        
        [Range(0.1f, 2f)]
        [Tooltip("Energy threshold for section classification")]
        public float energyThreshold = 0.5f;
        
        [Header("📏 Scaling")]
        [Tooltip("Enable scale animations")]
        public bool enableScaleAnimation = true;
        
        [Range(0.1f, 10f)]
        [Tooltip("Global scale transition speed")]
        public float scaleSpeed = 2f;
        
        [Tooltip("Pulse on beats")]
        public bool enableBeatPulse = true;
        
        [Range(0f, 2f)]
        [Tooltip("Beat pulse intensity")]
        public float beatPulseIntensity = 0.5f;
        
        [Header("📍 Movement Boundaries")]
        [Tooltip("Constrain movement to defined area")]
        public bool constrainMovement = true;
        
        [Tooltip("Maximum movement area (world space)")]
        public Vector3 movementBounds = new Vector3(10f, 10f, 10f);
        
        [Tooltip("Reduce movement intensity globally")]
        [Range(0f, 2f)]
        public float movementIntensityMultiplier = 1f;
        
        [Header("🎬 Stage Dimensions")]
        [Tooltip("Total performance space bounds for camera and lighting placement")]
        public Vector3 stageBounds = new Vector3(10f, 10f, 10f);
        
        [Tooltip("Stage center position in world space")]
        public Vector3 stageCenter = Vector3.zero;
        
        [Header("🎨 Color Effects")]
        [Tooltip("Flash on onsets")]
        public bool enableOnsetFlashes = false;
        
        [Range(0f, 1f)]
        [Tooltip("Onset flash intensity")]
        public float flashIntensity = 0.3f;
        
        [Header("⚙️ Performance")]
        [Range(0.05f, 0.5f)]
        [Tooltip("Keyframe interval - lower = smoother but larger files")]
        public float keyframeInterval = 0.1f;
        
        [Tooltip("Optimize curves to reduce file size")]
        public bool optimizeCurves = true;
        
        [Range(0.001f, 0.1f)]
        [Tooltip("Curve optimization tolerance")]
        public float curveOptimizationTolerance = 0.01f;
        
        [Header("🐛 Debug")]
        [Tooltip("Show debug information")]
        public bool showDebugInfo = false;
        
        /// <summary>
        /// Check if any music analysis data is assigned.
        /// </summary>
        public bool HasAnyMusicData()
        {
            return beatsFile != null || onsetsFile != null || notesFile != null ||
                   pitchFile != null || silenceFile != null;
        }
        
        private void OnValidate()
        {
            storyIntensity = Mathf.Max(0.1f, storyIntensity);
            musicAnalysisWindowSize = Mathf.Clamp(musicAnalysisWindowSize, 1f, 10f);
            intensityThreshold = Mathf.Max(0.1f, intensityThreshold);
            energyThreshold = Mathf.Max(0.1f, energyThreshold);
            scaleSpeed = Mathf.Max(0.1f, scaleSpeed);
            keyframeInterval = Mathf.Clamp(keyframeInterval, 0.05f, 0.5f);
            curveOptimizationTolerance = Mathf.Clamp(curveOptimizationTolerance, 0.001f, 0.1f);
        }
    }
}