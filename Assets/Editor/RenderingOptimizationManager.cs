using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;
using System.Linq;
using URPShadowResolution = UnityEngine.Rendering.Universal.ShadowResolution;

public class RenderingOptimizationManager : EditorWindow
{
    private Vector2 scrollPosition;
    private bool urpDetected = false;
    private UniversalRenderPipelineAsset urpAsset;
    private List<UniversalRenderPipelineAsset> allURPAssets = new List<UniversalRenderPipelineAsset>();
    private int selectedURPAssetIndex = 0;
    
    // Categories
    private bool showURPSettings = true;
    private bool showQualitySettings = true;
    private bool showBuildSettings = true;
    private bool showTextureSettings = true;
    private bool showMobileOptimizations = true;
    
    // Optimization status
    private Dictionary<string, bool> optimizationStatus = new Dictionary<string, bool>();
    
    [MenuItem("Tools/Rendering Optimization Manager")]
    public static void ShowWindow()
    {
        var window = GetWindow<RenderingOptimizationManager>("Rendering Optimization");
        window.minSize = new Vector2(900, 700);
    }
    
    private void OnEnable()
    {
        DetectURPAssets();
        AnalyzeCurrentSettings();
    }
    
    private void OnGUI()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        DrawHeader();
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(5);
        
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        if (urpDetected)
        {
            DrawURPAssetSelector();
            EditorGUILayout.Space(5);
        }
        
        DrawOptimizationScore();
        EditorGUILayout.Space(10);
        
        if (urpDetected)
        {
            DrawURPSettings();
            EditorGUILayout.Space(5);
        }
        
        DrawQualitySettings();
        EditorGUILayout.Space(5);
        
        DrawBuildSettings();
        EditorGUILayout.Space(5);
        
        DrawTextureSettings();
        EditorGUILayout.Space(5);
        
        DrawMobileOptimizations();
        EditorGUILayout.Space(5);
        
        DrawQuickActions();
        
        EditorGUILayout.EndScrollView();
    }
    
    private void DrawHeader()
    {
        EditorGUILayout.LabelField("Rendering Optimization Manager", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        
        if (urpDetected)
        {
            GUI.backgroundColor = Color.green;
            EditorGUILayout.LabelField("✓ URP Detected", EditorStyles.boldLabel);
            GUI.backgroundColor = Color.white;
        }
        else
        {
            GUI.backgroundColor = Color.yellow;
            EditorGUILayout.LabelField("⚠ Built-in Pipeline Detected", EditorStyles.boldLabel);
            GUI.backgroundColor = Color.white;
        }
        
        GUILayout.FlexibleSpace();
        
        if (GUILayout.Button("Refresh Analysis", GUILayout.Width(150)))
        {
            DetectURPAssets();
            AnalyzeCurrentSettings();
            Repaint();
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.HelpBox(
            "This tool analyzes your project's rendering settings and provides one-click optimizations for mobile performance. " +
            "Green items are optimized, yellow items need attention, red items are performance issues.",
            MessageType.Info);
    }
    
    private void DetectURPAssets()
    {
        allURPAssets.Clear();
        
        // Find all URP assets in the project
        string[] guids = AssetDatabase.FindAssets("t:UniversalRenderPipelineAsset");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(path);
            if (asset != null)
            {
                allURPAssets.Add(asset);
            }
        }
        
        // Get current pipeline asset
        if (GraphicsSettings.currentRenderPipeline is UniversalRenderPipelineAsset currentAsset)
        {
            urpAsset = currentAsset;
            urpDetected = true;
            selectedURPAssetIndex = allURPAssets.IndexOf(urpAsset);
            if (selectedURPAssetIndex < 0) selectedURPAssetIndex = 0;
        }
        else
        {
            urpDetected = false;
            if (allURPAssets.Count > 0)
            {
                urpAsset = allURPAssets[0];
            }
        }
    }
    
    private void DrawURPAssetSelector()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("URP Asset Selection", EditorStyles.boldLabel);
        
        if (allURPAssets.Count > 0)
        {
            string[] assetNames = allURPAssets.Select(a => a.name).ToArray();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Active URP Asset:", GUILayout.Width(120));
            
            int newIndex = EditorGUILayout.Popup(selectedURPAssetIndex, assetNames);
            if (newIndex != selectedURPAssetIndex)
            {
                selectedURPAssetIndex = newIndex;
                urpAsset = allURPAssets[selectedURPAssetIndex];
            }
            
            if (GUILayout.Button("Select in Project", GUILayout.Width(120)))
            {
                Selection.activeObject = urpAsset;
                EditorGUIUtility.PingObject(urpAsset);
            }
            
            EditorGUILayout.EndHorizontal();
            
            // Show if this is the active pipeline
            if (GraphicsSettings.currentRenderPipeline == urpAsset)
            {
                GUI.backgroundColor = Color.green;
                EditorGUILayout.HelpBox("✓ This is the currently active render pipeline", MessageType.Info);
                GUI.backgroundColor = Color.white;
            }
            else
            {
                GUI.backgroundColor = Color.yellow;
                EditorGUILayout.HelpBox("This URP asset is not currently active in Graphics Settings", MessageType.Warning);
                if (GUILayout.Button("Set as Active Pipeline"))
                {
                    GraphicsSettings.defaultRenderPipeline = urpAsset;
                    EditorUtility.DisplayDialog("Success", $"Set {urpAsset.name} as active render pipeline", "OK");
                }
                GUI.backgroundColor = Color.white;
            }
        }
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawOptimizationScore()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Optimization Score", EditorStyles.boldLabel);
        
        int totalChecks = optimizationStatus.Count;
        int optimizedCount = optimizationStatus.Count(kvp => kvp.Value);
        float score = totalChecks > 0 ? (optimizedCount / (float)totalChecks) * 100f : 0f;
        
        Color scoreColor = Color.red;
        if (score >= 80f) scoreColor = Color.green;
        else if (score >= 50f) scoreColor = Color.yellow;
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"Optimized: {optimizedCount}/{totalChecks}", GUILayout.Width(150));
        
        Rect rect = GUILayoutUtility.GetRect(18, 18, GUILayout.ExpandWidth(true));
        EditorGUI.ProgressBar(rect, score / 100f, $"{score:F1}%");
        
        GUI.color = scoreColor;
        GUILayout.Label("●", GUILayout.Width(20));
        GUI.color = Color.white;
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawURPSettings()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        showURPSettings = EditorGUILayout.Foldout(showURPSettings, "URP Pipeline Settings", true, EditorStyles.foldoutHeader);
        
        if (showURPSettings && urpAsset != null)
        {
            SerializedObject urpSO = new SerializedObject(urpAsset);
            
            // MSAA
            DrawOptimizationToggle(
                "Disable MSAA (Mobile)",
                "MSAA is expensive on mobile. Use FXAA or no AA instead.",
                urpAsset.msaaSampleCount == 1,
                () => {
                    urpSO.FindProperty("m_MSAA").intValue = 1;
                    urpSO.ApplyModifiedProperties();
                    EditorUtility.SetDirty(urpAsset);
                });
            
            // HDR
            DrawOptimizationToggle(
                "Disable HDR (Mobile)",
                "HDR doubles bandwidth usage. Disable for mobile.",
                !urpAsset.supportsHDR,
                () => {
                    urpSO.FindProperty("m_SupportsHDR").boolValue = false;
                    urpSO.ApplyModifiedProperties();
                    EditorUtility.SetDirty(urpAsset);
                });
            
            // Main Light Shadows
            DrawOptimizationToggle(
                "Enable Main Light Shadows",
                "Main light shadows are important for visual quality.",
                urpAsset.supportsMainLightShadows,
                () => {
                    urpSO.FindProperty("m_MainLightShadowsSupported").boolValue = true;
                    urpSO.ApplyModifiedProperties();
                    EditorUtility.SetDirty(urpAsset);
                });
            
            // Additional Lights
            DrawOptimizationToggle(
                "Limit Additional Lights (Mobile)",
                "Reduce per-pixel lights for better performance.",
                urpAsset.maxAdditionalLightsCount <= 1,
                () => {
                    urpSO.FindProperty("m_AdditionalLightsRenderingMode").intValue = 1; // Per Pixel
                    urpSO.FindProperty("m_AdditionalLightsPerObjectLimit").intValue = 1;
                    urpSO.ApplyModifiedProperties();
                    EditorUtility.SetDirty(urpAsset);
                });
            
            // Shadow Resolution
            DrawOptimizationToggle(
                "Optimize Shadow Resolution (Mobile)",
                "Use 1024 or lower for mobile shadow maps.",
                (int)urpAsset.mainLightShadowmapResolution <= 1024,
                () => {
                    urpSO.FindProperty("m_MainLightShadowmapResolution").intValue = 1024;
                    urpSO.FindProperty("m_AdditionalLightsShadowmapResolution").intValue = 512;
                    urpSO.ApplyModifiedProperties();
                    EditorUtility.SetDirty(urpAsset);
                });
            
            // Shadow Distance
            DrawOptimizationToggle(
                "Optimize Shadow Distance (Mobile)",
                "Reduce shadow distance to 30m or less for mobile.",
                urpAsset.shadowDistance <= 30f,
                () => {
                    urpSO.FindProperty("m_ShadowDistance").floatValue = 30f;
                    urpSO.ApplyModifiedProperties();
                    EditorUtility.SetDirty(urpAsset);
                });
            
            // Shadow Cascades
            DrawOptimizationToggle(
                "Use Single Shadow Cascade (Mobile)",
                "Multiple cascades increase draw calls.",
                urpAsset.shadowCascadeCount == 1,
                () => {
                    urpSO.FindProperty("m_ShadowCascadeCount").intValue = 1;
                    urpSO.ApplyModifiedProperties();
                    EditorUtility.SetDirty(urpAsset);
                });
            
            // Soft Shadows
            DrawOptimizationToggle(
                "Disable Soft Shadows (Mobile)",
                "Soft shadows add extra texture samples.",
                !urpAsset.supportsSoftShadows,
                () => {
                    urpSO.FindProperty("m_SoftShadowsSupported").boolValue = false;
                    urpSO.ApplyModifiedProperties();
                    EditorUtility.SetDirty(urpAsset);
                });
            
            // Depth Texture
            DrawOptimizationToggle(
                "Disable Depth Texture (If Not Needed)",
                "Depth texture costs bandwidth. Only enable if using effects that need it.",
                urpAsset.supportsCameraDepthTexture == false,
                () => {
                    urpSO.FindProperty("m_RequireDepthTexture").boolValue = false;
                    urpSO.ApplyModifiedProperties();
                    EditorUtility.SetDirty(urpAsset);
                });
            
            // Opaque Texture
            DrawOptimizationToggle(
                "Disable Opaque Texture (If Not Needed)",
                "Opaque texture costs bandwidth. Only enable if using refraction/distortion.",
                urpAsset.supportsCameraOpaqueTexture == false,
                () => {
                    urpSO.FindProperty("m_RequireOpaqueTexture").boolValue = false;
                    urpSO.ApplyModifiedProperties();
                    EditorUtility.SetDirty(urpAsset);
                });
            
            EditorGUILayout.Space(5);
            
            // Quick apply all mobile optimizations
            GUI.backgroundColor = new Color(1f, 0.5f, 0f);
            if (GUILayout.Button("Apply All Mobile URP Optimizations", GUILayout.Height(35)))
            {
                if (EditorUtility.DisplayDialog("Apply Mobile Optimizations",
                    "This will apply all mobile-friendly URP settings:\n\n" +
                    "• Disable MSAA\n" +
                    "• Disable HDR\n" +
                    "• Limit additional lights to 1\n" +
                    "• Set shadow resolution to 1024\n" +
                    "• Set shadow distance to 30m\n" +
                    "• Use single shadow cascade\n" +
                    "• Disable soft shadows\n" +
                    "• Optimize render textures\n\n" +
                    "Continue?", "Yes", "Cancel"))
                {
                    ApplyMobileURPOptimizations(urpAsset);
                    EditorUtility.DisplayDialog("Success", "Mobile URP optimizations applied!", "OK");
                    AnalyzeCurrentSettings();
                }
            }
            GUI.backgroundColor = Color.white;
        }
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawQualitySettings()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        showQualitySettings = EditorGUILayout.Foldout(showQualitySettings, "Quality Settings", true, EditorStyles.foldoutHeader);
        
        if (showQualitySettings)
        {
            int currentQuality = QualitySettings.GetQualityLevel();
            string[] qualityNames = QualitySettings.names;
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Current Quality Level:", GUILayout.Width(150));
            int newQuality = EditorGUILayout.Popup(currentQuality, qualityNames);
            if (newQuality != currentQuality)
            {
                QualitySettings.SetQualityLevel(newQuality);
                AnalyzeCurrentSettings();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
            
            // VSync
            DrawOptimizationToggle(
                "Disable VSync (Mobile)",
                "VSync can cause performance issues on mobile.",
                QualitySettings.vSyncCount == 0,
                () => {
                    QualitySettings.vSyncCount = 0;
                });
            
            // Pixel Light Count
            DrawOptimizationToggle(
                "Limit Pixel Lights (Mobile)",
                "More than 1 pixel light impacts performance.",
                QualitySettings.pixelLightCount <= 1,
                () => {
                    QualitySettings.pixelLightCount = 1;
                });
            
            // Texture Quality
            DrawOptimizationToggle(
                "Full Texture Quality",
                "Ensure textures are not being downsampled.",
                QualitySettings.globalTextureMipmapLimit == 0,
                () => {
                    QualitySettings.globalTextureMipmapLimit = 0;
                });
            
            // Anisotropic Filtering
            DrawOptimizationToggle(
                "Optimize Anisotropic Filtering (Mobile)",
                "Use Per Texture setting for better control.",
                QualitySettings.anisotropicFiltering == AnisotropicFiltering.Enable,
                () => {
                    QualitySettings.anisotropicFiltering = AnisotropicFiltering.Enable;
                });
            
            // Anti Aliasing
            DrawOptimizationToggle(
                "Disable Anti-Aliasing in Quality (Use URP)",
                "Handle AA in URP settings instead.",
                QualitySettings.antiAliasing == 0,
                () => {
                    QualitySettings.antiAliasing = 0;
                });
            
            // Soft Particles
            DrawOptimizationToggle(
                "Disable Soft Particles (Mobile)",
                "Soft particles require depth texture.",
                !QualitySettings.softParticles,
                () => {
                    QualitySettings.softParticles = false;
                });
            
            // Realtime Reflection Probes
            DrawOptimizationToggle(
                "Disable Realtime Reflection Probes (Mobile)",
                "Use baked reflection probes instead.",
                !QualitySettings.realtimeReflectionProbes,
                () => {
                    QualitySettings.realtimeReflectionProbes = false;
                });
            
            // LOD Bias
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("LOD Bias:", GUILayout.Width(150));
            float newLODBias = EditorGUILayout.Slider(QualitySettings.lodBias, 0.3f, 2f);
            if (!Mathf.Approximately(newLODBias, QualitySettings.lodBias))
            {
                QualitySettings.lodBias = newLODBias;
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.HelpBox("Lower LOD bias = earlier LOD switches = better performance. Recommended: 0.7-1.0 for mobile.", MessageType.Info);
        }
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawBuildSettings()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        showBuildSettings = EditorGUILayout.Foldout(showBuildSettings, "Build & Platform Settings", true, EditorStyles.foldoutHeader);
        
        if (showBuildSettings)
        {
            // Color Space
            DrawOptimizationToggle(
                "Use Linear Color Space",
                "Linear color space gives better visual quality.",
                PlayerSettings.colorSpace == ColorSpace.Linear,
                () => {
                    PlayerSettings.colorSpace = ColorSpace.Linear;
                });
            
            // Graphics API
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Graphics APIs:", GUILayout.Width(150));
            var apis = PlayerSettings.GetGraphicsAPIs(BuildTarget.Android);
            string apiString = string.Join(", ", apis);
            EditorGUILayout.LabelField(apiString);
            EditorGUILayout.EndHorizontal();
            
            DrawOptimizationToggle(
                "Auto Graphics API (Recommended)",
                "Let Unity choose the best graphics API for the platform.",
                PlayerSettings.GetUseDefaultGraphicsAPIs(BuildTarget.Android),
                () => {
                    PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, true);
                    PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.iOS, true);
                });
            
            // GPU Skinning
            DrawOptimizationToggle(
                "Enable GPU Skinning",
                "Offload skinned mesh calculations to GPU.",
                PlayerSettings.gpuSkinning,
                () => {
                    PlayerSettings.gpuSkinning = true;
                });
            
            // Multithreaded Rendering
            DrawOptimizationToggle(
                "Enable Multithreaded Rendering",
                "Significantly improves performance on multi-core devices.",
                PlayerSettings.GetMobileMTRendering(BuildTargetGroup.Android),
                () => {
                    PlayerSettings.SetMobileMTRendering(BuildTargetGroup.Android, true);
                    PlayerSettings.SetMobileMTRendering(BuildTargetGroup.iOS, true);
                });
            
            // Static Batching
            DrawOptimizationToggle(
                "Enable Static Batching",
                "Reduces draw calls for static objects.",
                PlayerSettings.GetStaticBatchingForPlatform(BuildTarget.Android),
                () => {
                    // Note: Unity doesn't provide a simple API to set batching per platform
                    // Static batching is typically controlled via Player Settings UI
                    EditorUtility.DisplayDialog("Static Batching", 
                        "Please enable Static Batching in:\nEdit → Project Settings → Player → Other Settings → Static Batching", 
                        "OK");
                });
            
            // Dynamic Batching
            bool dynamicBatching = PlayerSettings.GetDynamicBatchingForPlatform(BuildTarget.Android);
            EditorGUILayout.BeginHorizontal();
            GUI.backgroundColor = dynamicBatching ? Color.yellow : Color.green;
            EditorGUILayout.LabelField(dynamicBatching ? "⚠" : "✓", GUILayout.Width(20));
            GUI.backgroundColor = Color.white;
            
            EditorGUILayout.LabelField("Disable Dynamic Batching (Use SRP Batcher)", GUILayout.Width(300));
            if (GUILayout.Button(dynamicBatching ? "Check Settings" : "Disabled", GUILayout.Width(120)))
            {
                EditorUtility.DisplayDialog("Dynamic Batching",
                    "Please disable Dynamic Batching in:\nEdit → Project Settings → Player → Other Settings → Dynamic Batching\n\n" +
                    "URP's SRP Batcher is more efficient than Dynamic Batching.",
                    "OK");
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.HelpBox("Dynamic batching is less efficient than SRP Batcher in URP.", MessageType.Info);
            
            // Accelerometer Frequency
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Accelerometer Frequency:", GUILayout.Width(180));
            int newAccelFreq = EditorGUILayout.IntSlider(PlayerSettings.accelerometerFrequency, 0, 60);
            if (newAccelFreq != PlayerSettings.accelerometerFrequency)
            {
                PlayerSettings.accelerometerFrequency = newAccelFreq;
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.HelpBox("Lower frequency = better battery life. Use 0 if not needed.", MessageType.Info);
        }
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawTextureSettings()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        showTextureSettings = EditorGUILayout.Foldout(showTextureSettings, "Texture & Compression Settings", true, EditorStyles.foldoutHeader);
        
        if (showTextureSettings)
        {
            EditorGUILayout.HelpBox(
                "To optimize textures, select textures in your project and apply these settings:\n\n" +
                "• Max Size: 2048 or lower for mobile\n" +
                "• Compression: ASTC for Android, or PVRTC for iOS\n" +
                "• Generate Mip Maps: Enabled (reduces aliasing)\n" +
                "• Use Crunch Compression: For non-critical textures",
                MessageType.Info);
            
            EditorGUILayout.Space(5);
            
            if (GUILayout.Button("Select All Textures in Project", GUILayout.Height(30)))
            {
                SelectAllTextures();
            }
            
            EditorGUILayout.Space(5);
            
            // Find large textures
            if (GUILayout.Button("Find Large Textures (>2048)", GUILayout.Height(30)))
            {
                FindLargeTextures();
            }
            
            // Find uncompressed textures
            if (GUILayout.Button("Find Uncompressed Textures", GUILayout.Height(30)))
            {
                FindUncompressedTextures();
            }
            
            EditorGUILayout.Space(5);
            
            GUI.backgroundColor = new Color(1f, 0.5f, 0f);
            if (GUILayout.Button("Compress All Textures for Mobile", GUILayout.Height(35)))
            {
                if (EditorUtility.DisplayDialog("Compress Textures",
                    "This will set compression format for all textures to ASTC (Android) and adjust max size. This may take a while. Continue?",
                    "Yes", "Cancel"))
                {
                    CompressAllTexturesForMobile();
                }
            }
            GUI.backgroundColor = Color.white;
        }
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawMobileOptimizations()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        showMobileOptimizations = EditorGUILayout.Foldout(showMobileOptimizations, "Mobile-Specific Optimizations", true, EditorStyles.foldoutHeader);
        
        if (showMobileOptimizations)
        {
            EditorGUILayout.HelpBox(
                "Mobile optimization checklist:\n\n" +
                "✓ Use texture atlases to reduce draw calls\n" +
                "✓ Use object pooling for frequently spawned objects\n" +
                "✓ Disable unnecessary cameras\n" +
                "✓ Use occlusion culling\n" +
                "✓ Bake lighting when possible\n" +
                "✓ Use LOD groups (use Scene Performance Manager)\n" +
                "✓ Optimize UI by reducing transparency\n" +
                "✓ Profile regularly with Unity Profiler",
                MessageType.Info);
            
            EditorGUILayout.Space(5);
            
            // Android specific
            EditorGUILayout.LabelField("Android Optimizations", EditorStyles.boldLabel);
            
            DrawOptimizationToggle(
                "Optimize Mesh Data",
                "Remove unused vertex attributes.",
                PlayerSettings.stripUnusedMeshComponents,
                () => {
                    PlayerSettings.stripUnusedMeshComponents = true;
                });
            
            // IL2CPP
            bool useIL2CPP = PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android) == ScriptingImplementation.IL2CPP;
            DrawOptimizationToggle(
                "Use IL2CPP (Recommended)",
                "IL2CPP provides better performance than Mono.",
                useIL2CPP,
                () => {
                    PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
                });
            
            // Managed Stripping Level
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Managed Stripping Level:", GUILayout.Width(180));
            var strippingLevel = PlayerSettings.GetManagedStrippingLevel(BuildTargetGroup.Android);
            var newStrippingLevel = (ManagedStrippingLevel)EditorGUILayout.EnumPopup(strippingLevel);
            if (newStrippingLevel != strippingLevel)
            {
                PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.Android, newStrippingLevel);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.HelpBox("Higher stripping = smaller build size. Use 'Medium' or 'High' for mobile.", MessageType.Info);
        }
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawQuickActions()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("Apply ALL Mobile Optimizations", GUILayout.Height(40)))
        {
            if (EditorUtility.DisplayDialog("Apply All Optimizations",
                "This will apply ALL recommended mobile optimizations including:\n\n" +
                "• All URP mobile settings\n" +
                "• Quality settings\n" +
                "• Build settings\n" +
                "• Platform settings\n\n" +
                "This is recommended for mobile projects. Continue?",
                "Yes, Apply All", "Cancel"))
            {
                ApplyAllMobileOptimizations();
                EditorUtility.DisplayDialog("Success", 
                    "All mobile optimizations have been applied!\n\n" +
                    "Remember to:\n" +
                    "• Test on target devices\n" +
                    "• Use Scene Performance Manager for LODs\n" +
                    "• Profile with Unity Profiler", "OK");
                AnalyzeCurrentSettings();
            }
        }
        
        GUI.backgroundColor = Color.cyan;
        if (GUILayout.Button("Open Scene Performance Manager", GUILayout.Height(40)))
        {
            ScenePerformanceManager.ShowWindow();
        }
        
        GUI.backgroundColor = Color.white;
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawOptimizationToggle(string label, string tooltip, bool isOptimized, System.Action onOptimize)
    {
        EditorGUILayout.BeginHorizontal();
        
        GUI.backgroundColor = isOptimized ? Color.green : Color.red;
        EditorGUILayout.LabelField(isOptimized ? "✓" : "✗", GUILayout.Width(20));
        GUI.backgroundColor = Color.white;
        
        EditorGUILayout.LabelField(new GUIContent(label, tooltip), GUILayout.Width(300));
        
        if (!isOptimized)
        {
            if (GUILayout.Button("Optimize", GUILayout.Width(100)))
            {
                onOptimize?.Invoke();
                AnalyzeCurrentSettings();
                Repaint();
            }
        }
        else
        {
            GUI.enabled = false;
            GUILayout.Button("Optimized", GUILayout.Width(100));
            GUI.enabled = true;
        }
        
        EditorGUILayout.EndHorizontal();
        
        // Store optimization status
        if (!optimizationStatus.ContainsKey(label))
        {
            optimizationStatus[label] = isOptimized;
        }
        else
        {
            optimizationStatus[label] = isOptimized;
        }
    }
    
    private void AnalyzeCurrentSettings()
    {
        optimizationStatus.Clear();
        
        if (urpAsset != null)
        {
            optimizationStatus["MSAA"] = urpAsset.msaaSampleCount == 1;
            optimizationStatus["HDR"] = !urpAsset.supportsHDR;
            optimizationStatus["Additional Lights"] = urpAsset.maxAdditionalLightsCount <= 1;
            optimizationStatus["Shadow Resolution"] = (int)urpAsset.mainLightShadowmapResolution <= 1024;
            optimizationStatus["Shadow Distance"] = urpAsset.shadowDistance <= 30f;
            optimizationStatus["Shadow Cascades"] = urpAsset.shadowCascadeCount == 1;
            optimizationStatus["Soft Shadows"] = !urpAsset.supportsSoftShadows;
        }
        
        optimizationStatus["VSync"] = QualitySettings.vSyncCount == 0;
        optimizationStatus["Pixel Lights"] = QualitySettings.pixelLightCount <= 1;
        optimizationStatus["GPU Skinning"] = PlayerSettings.gpuSkinning;
        optimizationStatus["Multithreaded Rendering"] = PlayerSettings.GetMobileMTRendering(BuildTargetGroup.Android);
    }
    
    private void ApplyMobileURPOptimizations(UniversalRenderPipelineAsset asset)
    {
        SerializedObject so = new SerializedObject(asset);
        
        so.FindProperty("m_MSAA").intValue = 1;
        so.FindProperty("m_SupportsHDR").boolValue = false;
        so.FindProperty("m_AdditionalLightsRenderingMode").intValue = 1;
        so.FindProperty("m_AdditionalLightsPerObjectLimit").intValue = 1;
        so.FindProperty("m_MainLightShadowmapResolution").intValue = (int)URPShadowResolution._1024;
        so.FindProperty("m_AdditionalLightsShadowmapResolution").intValue = (int)URPShadowResolution._512;
        so.FindProperty("m_ShadowDistance").floatValue = 30f;
        so.FindProperty("m_ShadowCascadeCount").intValue = 1;
        so.FindProperty("m_SoftShadowsSupported").boolValue = false;
        
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(asset);
    }
    
    private void ApplyAllMobileOptimizations()
    {
        // URP Settings
        if (urpAsset != null)
        {
            ApplyMobileURPOptimizations(urpAsset);
        }
        
        // Quality Settings
        QualitySettings.vSyncCount = 0;
        QualitySettings.pixelLightCount = 1;
        QualitySettings.antiAliasing = 0;
        QualitySettings.softParticles = false;
        QualitySettings.realtimeReflectionProbes = false;
        QualitySettings.lodBias = 1.0f;
        
        // Build Settings
        PlayerSettings.colorSpace = ColorSpace.Linear;
        PlayerSettings.gpuSkinning = true;
        PlayerSettings.SetMobileMTRendering(BuildTargetGroup.Android, true);
        PlayerSettings.SetMobileMTRendering(BuildTargetGroup.iOS, true);
        PlayerSettings.stripUnusedMeshComponents = true;
        
        // Scripting
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.Android, ManagedStrippingLevel.Medium);
        
        // Note: Batching settings are in Player Settings UI, not easily accessible via script
        
        AssetDatabase.SaveAssets();
    }
    
    private void SelectAllTextures()
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets" });
        Object[] textures = new Object[guids.Length];
        
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            textures[i] = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
        
        Selection.objects = textures;
        EditorUtility.DisplayDialog("Textures Selected", $"Selected {textures.Length} textures in the project.", "OK");
    }
    
    private void FindLargeTextures()
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets" });
        List<Object> largeTextures = new List<Object>();
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            
            if (importer != null && importer.maxTextureSize > 2048)
            {
                largeTextures.Add(AssetDatabase.LoadAssetAtPath<Texture2D>(path));
            }
        }
        
        if (largeTextures.Count > 0)
        {
            Selection.objects = largeTextures.ToArray();
            EditorUtility.DisplayDialog("Large Textures Found", 
                $"Found {largeTextures.Count} textures larger than 2048. They have been selected.", "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("No Large Textures", "No textures larger than 2048 found.", "OK");
        }
    }
    
    private void FindUncompressedTextures()
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets" });
        List<Object> uncompressed = new List<Object>();
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            
            if (importer != null)
            {
                var platformSettings = importer.GetPlatformTextureSettings("Android");
                if (platformSettings.format == TextureImporterFormat.RGBA32 || 
                    platformSettings.format == TextureImporterFormat.RGB24)
                {
                    uncompressed.Add(AssetDatabase.LoadAssetAtPath<Texture2D>(path));
                }
            }
        }
        
        if (uncompressed.Count > 0)
        {
            Selection.objects = uncompressed.ToArray();
            EditorUtility.DisplayDialog("Uncompressed Textures Found",
                $"Found {uncompressed.Count} uncompressed textures. They have been selected.", "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("All Textures Compressed", "All textures are using compression.", "OK");
        }
    }
    
    private void CompressAllTexturesForMobile()
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets" });
        int processed = 0;
        
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            
            EditorUtility.DisplayProgressBar("Compressing Textures",
                $"Processing {path} ({i + 1}/{guids.Length})",
                (float)i / guids.Length);
            
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            
            if (importer != null)
            {
                // Android settings
                var androidSettings = importer.GetPlatformTextureSettings("Android");
                androidSettings.overridden = true;
                androidSettings.format = TextureImporterFormat.ASTC_6x6;
                androidSettings.maxTextureSize = 2048;
                importer.SetPlatformTextureSettings(androidSettings);
                
                // iOS settings
                var iosSettings = importer.GetPlatformTextureSettings("iPhone");
                iosSettings.overridden = true;
                iosSettings.format = TextureImporterFormat.ASTC_6x6;
                iosSettings.maxTextureSize = 2048;
                importer.SetPlatformTextureSettings(iosSettings);
                
                importer.SaveAndReimport();
                processed++;
            }
        }
        
        EditorUtility.ClearProgressBar();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        EditorUtility.DisplayDialog("Compression Complete",
            $"Compressed {processed} textures for mobile.\n\nTextures are now using ASTC 6x6 compression with max size 2048.", "OK");
    }
}