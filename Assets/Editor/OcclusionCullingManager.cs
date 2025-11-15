using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;

public class OcclusionCullingManager : EditorWindow
{
    private Vector2 scrollPosition;
    private Scene currentScene;
    
    // Occlusion settings
    private float smallestOccluder = 5f;
    private float smallestHole = 0.25f;
    private float backfaceThreshold = 100f;
    
    // Analysis
    private bool occlusionDataExists = false;
    private int staticObjectCount = 0;
    private int occluderStaticCount = 0;
    private int occludeeStaticCount = 0;
    private int totalRenderers = 0;
    private long occlusionDataSize = 0;
    
    // Categories
    private bool showSceneAnalysis = true;
    private bool showOcclusionSettings = true;
    private bool showObjectSettings = true;
    private bool showBakeSettings = true;
    private bool showVisualization = true;
    
    // Object lists
    private List<GameObject> largeStaticObjects = new List<GameObject>();
    private List<GameObject> nonStaticObjects = new List<GameObject>();
    private List<GameObject> occluderCandidates = new List<GameObject>();
    
    [MenuItem("Tools/Occlusion Culling Manager")]
    public static void ShowWindow()
    {
        var window = GetWindow<OcclusionCullingManager>("Occlusion Culling");
        window.minSize = new Vector2(800, 600);
    }
    
    private void OnEnable()
    {
        currentScene = SceneManager.GetActiveScene();
        AnalyzeScene();
    }
    
    private void OnGUI()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        DrawHeader();
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(5);
        
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        DrawSceneAnalysis();
        EditorGUILayout.Space(5);
        
        DrawOcclusionSettings();
        EditorGUILayout.Space(5);
        
        DrawObjectSettings();
        EditorGUILayout.Space(5);
        
        DrawBakeSettings();
        EditorGUILayout.Space(5);
        
        DrawVisualization();
        
        EditorGUILayout.EndScrollView();
    }
    
    private void DrawHeader()
    {
        EditorGUILayout.LabelField("Occlusion Culling Manager", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        
        EditorGUILayout.LabelField($"Scene: {currentScene.name}", GUILayout.Width(250));
        
        if (occlusionDataExists)
        {
            GUI.backgroundColor = Color.green;
            EditorGUILayout.LabelField("✓ Occlusion Data Present", EditorStyles.boldLabel, GUILayout.Width(200));
            GUI.backgroundColor = Color.white;
        }
        else
        {
            GUI.backgroundColor = Color.yellow;
            EditorGUILayout.LabelField("⚠ No Occlusion Data", EditorStyles.boldLabel, GUILayout.Width(200));
            GUI.backgroundColor = Color.white;
        }
        
        GUILayout.FlexibleSpace();
        
        if (GUILayout.Button("Refresh Analysis", GUILayout.Width(150)))
        {
            AnalyzeScene();
            Repaint();
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.HelpBox(
            "Occlusion Culling prevents rendering objects that are hidden behind other objects, significantly improving performance. " +
            "Mark large static objects as 'Occluder Static' and all visible objects as 'Occludee Static', then bake occlusion data.",
            MessageType.Info);
    }
    
    private void DrawSceneAnalysis()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        showSceneAnalysis = EditorGUILayout.Foldout(showSceneAnalysis, "Scene Analysis", true, EditorStyles.foldoutHeader);
        
        if (showSceneAnalysis)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Total Renderers:", GUILayout.Width(180));
            EditorGUILayout.LabelField(totalRenderers.ToString(), EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Static Objects:", GUILayout.Width(180));
            EditorGUILayout.LabelField(staticObjectCount.ToString(), EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Occluder Static:", GUILayout.Width(180));
            GUI.color = occluderStaticCount > 0 ? Color.green : Color.red;
            EditorGUILayout.LabelField(occluderStaticCount.ToString(), EditorStyles.boldLabel);
            GUI.color = Color.white;
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Occludee Static:", GUILayout.Width(180));
            GUI.color = occludeeStaticCount > 0 ? Color.green : Color.red;
            EditorGUILayout.LabelField(occludeeStaticCount.ToString(), EditorStyles.boldLabel);
            GUI.color = Color.white;
            EditorGUILayout.EndHorizontal();
            
            if (occlusionDataExists)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Occlusion Data Size:", GUILayout.Width(180));
                EditorGUILayout.LabelField(FormatBytes(occlusionDataSize), EditorStyles.boldLabel);
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.Space(10);
            
            // Recommendations
            EditorGUILayout.LabelField("Recommendations:", EditorStyles.boldLabel);
            
            if (staticObjectCount == 0)
            {
                EditorGUILayout.HelpBox("⚠ No static objects found! Mark non-moving objects as static for occlusion culling to work.", MessageType.Warning);
            }
            else if (occluderStaticCount == 0)
            {
                EditorGUILayout.HelpBox("⚠ No occluder static objects! Mark large objects (walls, buildings, terrain) as Occluder Static.", MessageType.Warning);
            }
            else if (occludeeStaticCount == 0)
            {
                EditorGUILayout.HelpBox("⚠ No occludee static objects! Mark objects that should be culled as Occludee Static.", MessageType.Warning);
            }
            else if (!occlusionDataExists)
            {
                EditorGUILayout.HelpBox("✓ Scene is ready for occlusion baking! Click 'Bake Occlusion Culling' below.", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("✓ Occlusion culling is set up and baked! Test in Game view to see the benefits.", MessageType.Info);
            }
            
            // Statistics
            float staticPercentage = totalRenderers > 0 ? (staticObjectCount / (float)totalRenderers) * 100f : 0f;
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField($"Static Coverage: {staticPercentage:F1}%");
            Rect rect = GUILayoutUtility.GetRect(18, 18, GUILayout.ExpandWidth(true));
            EditorGUI.ProgressBar(rect, staticPercentage / 100f, $"{staticPercentage:F1}%");
            
            if (staticPercentage < 50f)
            {
                EditorGUILayout.HelpBox("Low static coverage. Consider marking more non-moving objects as static.", MessageType.Warning);
            }
        }
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawOcclusionSettings()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        showOcclusionSettings = EditorGUILayout.Foldout(showOcclusionSettings, "Occlusion Culling Settings", true, EditorStyles.foldoutHeader);
        
        if (showOcclusionSettings)
        {
            EditorGUILayout.HelpBox(
                "These settings control how occlusion data is generated. Adjust based on your scene scale and performance needs.",
                MessageType.Info);
            
            EditorGUILayout.Space(5);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Smallest Occluder:", 
                "Minimum size (in meters) of objects that will be used as occluders. Smaller values = more accurate but larger data."), 
                GUILayout.Width(150));
            smallestOccluder = EditorGUILayout.Slider(smallestOccluder, 0.5f, 10f);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Smallest Hole:", 
                "Minimum size (in meters) of gaps through which objects can be seen. Smaller values = more accurate."), 
                GUILayout.Width(150));
            smallestHole = EditorGUILayout.Slider(smallestHole, 0.1f, 1f);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Backface Threshold:", 
                "Percentage of backfaces required to treat an object as an occluder. Higher = more selective."), 
                GUILayout.Width(150));
            backfaceThreshold = EditorGUILayout.Slider(backfaceThreshold, 5f, 100f);
            EditorGUILayout.LabelField($"{backfaceThreshold:F0}%", GUILayout.Width(50));
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(10);
            
            EditorGUILayout.LabelField("Recommended Settings:", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Indoor Scene (High Detail)"))
            {
                smallestOccluder = 2f;
                smallestHole = 0.15f;
                backfaceThreshold = 60f;
            }
            if (GUILayout.Button("Outdoor Scene (Medium Detail)"))
            {
                smallestOccluder = 5f;
                smallestHole = 0.25f;
                backfaceThreshold = 70f;
            }
            if (GUILayout.Button("Large Open Scene (Low Detail)"))
            {
                smallestOccluder = 10f;
                smallestHole = 0.5f;
                backfaceThreshold = 80f;
            }
            EditorGUILayout.EndHorizontal();
        }
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawObjectSettings()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        showObjectSettings = EditorGUILayout.Foldout(showObjectSettings, "Object Configuration", true, EditorStyles.foldoutHeader);
        
        if (showObjectSettings)
        {
            EditorGUILayout.HelpBox(
                "Configure which objects should be occluders (block view) and occludees (can be hidden).",
                MessageType.Info);
            
            EditorGUILayout.Space(5);
            
            // Quick setup buttons
            EditorGUILayout.LabelField("Quick Setup:", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            
            GUI.backgroundColor = Color.cyan;
            if (GUILayout.Button("Auto-Configure Scene", GUILayout.Height(35)))
            {
                if (EditorUtility.DisplayDialog("Auto-Configure Occlusion",
                    "This will automatically configure occlusion culling:\n\n" +
                    "• Mark all non-moving objects as static\n" +
                    "• Mark large objects (>5m bounds) as Occluder Static\n" +
                    "• Mark all static renderers as Occludee Static\n\n" +
                    "Continue?", "Yes", "Cancel"))
                {
                    AutoConfigureScene();
                    AnalyzeScene();
                    EditorUtility.DisplayDialog("Success", "Scene auto-configured for occlusion culling!", "OK");
                }
            }
            GUI.backgroundColor = Color.white;
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(10);
            
            // Manual configuration
            EditorGUILayout.LabelField("Manual Configuration:", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Select All Non-Static Renderers"))
            {
                SelectNonStaticRenderers();
            }
            
            if (nonStaticObjects.Count > 0)
            {
                EditorGUILayout.HelpBox($"Found {nonStaticObjects.Count} non-static objects with renderers.", MessageType.Info);
                
                if (GUILayout.Button($"Mark {nonStaticObjects.Count} Objects as Static"))
                {
                    MarkObjectsAsStatic(nonStaticObjects);
                    AnalyzeScene();
                }
            }
            
            EditorGUILayout.Space(5);
            
            if (GUILayout.Button("Find Large Objects (Good Occluders)"))
            {
                FindLargeObjects();
            }
            
            if (occluderCandidates.Count > 0)
            {
                EditorGUILayout.HelpBox($"Found {occluderCandidates.Count} large objects that would make good occluders.", MessageType.Info);
                
                if (GUILayout.Button($"Mark {occluderCandidates.Count} Objects as Occluder Static"))
                {
                    MarkObjectsAsOccluderStatic(occluderCandidates);
                    AnalyzeScene();
                }
            }
            
            EditorGUILayout.Space(5);
            
            if (GUILayout.Button("Mark All Static Renderers as Occludee Static"))
            {
                MarkAllStaticAsOccludee();
                AnalyzeScene();
            }
            
            EditorGUILayout.Space(10);
            
            // Clear buttons
            EditorGUILayout.LabelField("Clear Settings:", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            
            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("Clear All Occluder Static"))
            {
                if (EditorUtility.DisplayDialog("Clear Occluder Static",
                    "Remove Occluder Static flag from all objects?", "Yes", "Cancel"))
                {
                    ClearOccluderStatic();
                    AnalyzeScene();
                }
            }
            
            if (GUILayout.Button("Clear All Occludee Static"))
            {
                if (EditorUtility.DisplayDialog("Clear Occludee Static",
                    "Remove Occludee Static flag from all objects?", "Yes", "Cancel"))
                {
                    ClearOccludeeStatic();
                    AnalyzeScene();
                }
            }
            GUI.backgroundColor = Color.white;
            
            EditorGUILayout.EndHorizontal();
        }
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawBakeSettings()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        showBakeSettings = EditorGUILayout.Foldout(showBakeSettings, "Bake Occlusion Data", true, EditorStyles.foldoutHeader);
        
        if (showBakeSettings)
        {
            EditorGUILayout.HelpBox(
                "After configuring objects and settings, bake the occlusion data. This process analyzes the scene and generates visibility data.",
                MessageType.Info);
            
            EditorGUILayout.Space(5);
            
            // Check if ready to bake
            bool readyToBake = occluderStaticCount > 0 && occludeeStaticCount > 0;
            
            if (!readyToBake)
            {
                GUI.backgroundColor = Color.yellow;
                EditorGUILayout.HelpBox("⚠ Scene not ready for baking. Configure occluders and occludees first.", MessageType.Warning);
                GUI.backgroundColor = Color.white;
            }
            else
            {
                GUI.backgroundColor = Color.green;
                EditorGUILayout.HelpBox("✓ Scene is ready for occlusion baking!", MessageType.Info);
                GUI.backgroundColor = Color.white;
            }
            
            EditorGUILayout.Space(5);
            
            EditorGUI.BeginDisabledGroup(!readyToBake);
            
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("Bake Occlusion Culling", GUILayout.Height(40)))
            {
                BakeOcclusionCulling();
            }
            GUI.backgroundColor = Color.white;
            
            EditorGUI.EndDisabledGroup();
            
            EditorGUILayout.Space(5);
            
            if (occlusionDataExists)
            {
                EditorGUILayout.BeginHorizontal();
                
                if (GUILayout.Button("Clear Occlusion Data", GUILayout.Height(30)))
                {
                    if (EditorUtility.DisplayDialog("Clear Occlusion Data",
                        "This will remove all baked occlusion data. You'll need to rebake. Continue?", "Yes", "Cancel"))
                    {
                        ClearOcclusionData();
                        AnalyzeScene();
                    }
                }
                
                if (GUILayout.Button("Open Occlusion Culling Window", GUILayout.Height(30)))
                {
                    EditorWindow.GetWindow(System.Type.GetType("UnityEditor.OcclusionCullingWindow,UnityEditor"));
                }
                
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.Space(10);
            
            EditorGUILayout.LabelField("Baking Tips:", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "• Baking can take time for large scenes\n" +
                "• Smaller 'Smallest Occluder' values = longer bake time but more accurate\n" +
                "• Smaller 'Smallest Hole' values = longer bake time but fewer visibility errors\n" +
                "• Test in Game view with Stats window to see culling in action\n" +
                "• Rebake when you move or add static objects",
                MessageType.Info);
        }
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawVisualization()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        showVisualization = EditorGUILayout.Foldout(showVisualization, "Visualization & Testing", true, EditorStyles.foldoutHeader);
        
        if (showVisualization)
        {
            EditorGUILayout.HelpBox(
                "Use Unity's built-in occlusion visualization to see how culling works in your scene.",
                MessageType.Info);
            
            EditorGUILayout.Space(5);
            
            if (GUILayout.Button("Open Occlusion Culling Visualization", GUILayout.Height(35)))
            {
                EditorWindow.GetWindow(System.Type.GetType("UnityEditor.OcclusionCullingWindow,UnityEditor"));
            }
            
            EditorGUILayout.Space(10);
            
            EditorGUILayout.LabelField("Testing in Game View:", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "1. Enter Play mode\n" +
                "2. Open Window → Analysis → Rendering Debugger\n" +
                "3. Or check 'Stats' in Game view to see rendered objects count\n" +
                "4. Move camera around - rendered count should decrease when objects are occluded\n" +
                "5. Lower numbers = better performance!",
                MessageType.Info);
            
            EditorGUILayout.Space(5);
            
            if (GUILayout.Button("Open Rendering Debugger", GUILayout.Height(30)))
            {
                EditorApplication.ExecuteMenuItem("Window/Analysis/Rendering Debugger");
            }
            
            EditorGUILayout.Space(10);
            
            EditorGUILayout.LabelField("Performance Impact:", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Expected Performance Gains:\n" +
                "• Indoor scenes: 30-60% reduction in rendered objects\n" +
                "• Dense outdoor scenes: 20-40% reduction\n" +
                "• Open scenes: 10-20% reduction\n\n" +
                "Best results in scenes with many objects and clear occlusion (walls, buildings, terrain).",
                MessageType.Info);
        }
        
        EditorGUILayout.EndVertical();
    }
    
    private void AnalyzeScene()
    {
        totalRenderers = 0;
        staticObjectCount = 0;
        occluderStaticCount = 0;
        occludeeStaticCount = 0;
        
        largeStaticObjects.Clear();
        nonStaticObjects.Clear();
        occluderCandidates.Clear();
        
        // Find all renderers in scene (exclude SkinnedMeshRenderers as they're animated)
        var allRenderers = FindObjectsOfType<Renderer>();
        
        foreach (var renderer in allRenderers)
        {
            // Skip SkinnedMeshRenderers (animated characters)
            if (renderer is SkinnedMeshRenderer)
                continue;
            
            totalRenderers++;
            
            GameObject go = renderer.gameObject;
            StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(go);
            
            // Check if static
            if ((flags & StaticEditorFlags.ContributeGI) != 0 || flags != 0)
            {
                staticObjectCount++;
            }
            else
            {
                nonStaticObjects.Add(go);
            }
            
            // Check occluder static
            if ((flags & StaticEditorFlags.OccluderStatic) != 0)
            {
                occluderStaticCount++;
            }
            
            // Check occludee static
            if ((flags & StaticEditorFlags.OccludeeStatic) != 0)
            {
                occludeeStaticCount++;
            }
            
            // Check if large (good occluder candidate)
            if (renderer.bounds.size.magnitude > 5f)
            {
                largeStaticObjects.Add(go);
            }
        }
        
        // Check if occlusion data exists
        CheckOcclusionData();
    }
    
    private void CheckOcclusionData()
    {
        // Check if occlusion data file exists
        string scenePath = currentScene.path;
        string occlusionPath = scenePath.Replace(".unity", "/OcclusionCullingData.asset");
        
        occlusionDataExists = System.IO.File.Exists(occlusionPath);
        
        if (occlusionDataExists)
        {
            System.IO.FileInfo fileInfo = new System.IO.FileInfo(occlusionPath);
            occlusionDataSize = fileInfo.Length;
        }
    }
    
    private void AutoConfigureScene()
    {
        var allRenderers = FindObjectsOfType<Renderer>();
        int configured = 0;
        int skipped = 0;
        
        foreach (var renderer in allRenderers)
        {
            GameObject go = renderer.gameObject;
            
            // Skip SkinnedMeshRenderers (animated characters) - they should never be static!
            if (renderer is SkinnedMeshRenderer)
            {
                skipped++;
                continue;
            }
            
            // Skip if already static
            StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(go);
            if (flags != 0) continue;
            
            // Mark as static (this includes all static flags)
            GameObjectUtility.SetStaticEditorFlags(go, StaticEditorFlags.ContributeGI);
            
            // If large, also mark as occluder static
            if (renderer.bounds.size.magnitude > 5f)
            {
                flags = GameObjectUtility.GetStaticEditorFlags(go);
                flags |= StaticEditorFlags.OccluderStatic;
                GameObjectUtility.SetStaticEditorFlags(go, flags);
            }
            
            // Mark as occludee static
            flags = GameObjectUtility.GetStaticEditorFlags(go);
            flags |= StaticEditorFlags.OccludeeStatic;
            GameObjectUtility.SetStaticEditorFlags(go, flags);
            
            configured++;
        }
        
        //Debug.Log($"Auto-configured {configured} objects for occlusion culling (skipped {skipped} SkinnedMeshRenderers)");
    }
    
    private void SelectNonStaticRenderers()
    {
        var allRenderers = FindObjectsOfType<Renderer>();
        nonStaticObjects.Clear();
        
        foreach (var renderer in allRenderers)
        {
            // Skip SkinnedMeshRenderers (animated characters)
            if (renderer is SkinnedMeshRenderer)
                continue;
            
            GameObject go = renderer.gameObject;
            StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(go);
            
            if (flags == 0)
            {
                nonStaticObjects.Add(go);
            }
        }
        
        if (nonStaticObjects.Count > 0)
        {
            Selection.objects = nonStaticObjects.ToArray();
        }
        
        //Debug.Log($"Found {nonStaticObjects.Count} non-static renderers (excluding SkinnedMeshRenderers)");
    }
    
    private void FindLargeObjects()
    {
        var allRenderers = FindObjectsOfType<Renderer>();
        occluderCandidates.Clear();
        
        foreach (var renderer in allRenderers)
        {
            // Skip SkinnedMeshRenderers (animated characters)
            if (renderer is SkinnedMeshRenderer)
                continue;
            
            if (renderer.bounds.size.magnitude > 5f)
            {
                occluderCandidates.Add(renderer.gameObject);
            }
        }
        
        if (occluderCandidates.Count > 0)
        {
            Selection.objects = occluderCandidates.ToArray();
        }
        
        //Debug.Log($"Found {occluderCandidates.Count} large objects (good occluder candidates, excluding SkinnedMeshRenderers)");
    }
    
    private void MarkObjectsAsStatic(List<GameObject> objects)
    {
        foreach (var go in objects)
        {
            if (go != null)
            {
                GameObjectUtility.SetStaticEditorFlags(go, StaticEditorFlags.ContributeGI);
            }
        }
        
        //Debug.Log($"Marked {objects.Count} objects as static");
    }
    
    private void MarkObjectsAsOccluderStatic(List<GameObject> objects)
    {
        foreach (var go in objects)
        {
            if (go != null)
            {
                StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(go);
                flags |= StaticEditorFlags.OccluderStatic;
                GameObjectUtility.SetStaticEditorFlags(go, flags);
            }
        }
        
        //Debug.Log($"Marked {objects.Count} objects as Occluder Static");
    }
    
    private void MarkAllStaticAsOccludee()
    {
        var allRenderers = FindObjectsOfType<Renderer>();
        int marked = 0;
        
        foreach (var renderer in allRenderers)
        {
            // Skip SkinnedMeshRenderers (animated characters)
            if (renderer is SkinnedMeshRenderer)
                continue;
            
            GameObject go = renderer.gameObject;
            StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(go);
            
            // If already has some static flag, add occludee
            if (flags != 0)
            {
                flags |= StaticEditorFlags.OccludeeStatic;
                GameObjectUtility.SetStaticEditorFlags(go, flags);
                marked++;
            }
        }
        
        //Debug.Log($"Marked {marked} static objects as Occludee Static (excluding SkinnedMeshRenderers)");
    }
    
    private void ClearOccluderStatic()
    {
        var allObjects = FindObjectsOfType<GameObject>();
        int cleared = 0;
        
        foreach (var go in allObjects)
        {
            StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(go);
            if ((flags & StaticEditorFlags.OccluderStatic) != 0)
            {
                flags &= ~StaticEditorFlags.OccluderStatic;
                GameObjectUtility.SetStaticEditorFlags(go, flags);
                cleared++;
            }
        }
        
        //Debug.Log($"Cleared Occluder Static from {cleared} objects");
    }
    
    private void ClearOccludeeStatic()
    {
        var allObjects = FindObjectsOfType<GameObject>();
        int cleared = 0;
        
        foreach (var go in allObjects)
        {
            StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(go);
            if ((flags & StaticEditorFlags.OccludeeStatic) != 0)
            {
                flags &= ~StaticEditorFlags.OccludeeStatic;
                GameObjectUtility.SetStaticEditorFlags(go, flags);
                cleared++;
            }
        }
        
        //Debug.Log($"Cleared Occludee Static from {cleared} objects");
    }
    
    private void BakeOcclusionCulling()
    {
        // Set occlusion culling settings
        StaticOcclusionCulling.smallestOccluder = smallestOccluder;
        StaticOcclusionCulling.smallestHole = smallestHole;
        StaticOcclusionCulling.backfaceThreshold = backfaceThreshold;
        
        //Debug.Log("Starting occlusion culling bake...");
        
        // Compute the occlusion culling data
        StaticOcclusionCulling.Compute();
        
        EditorUtility.DisplayDialog("Occlusion Baking Started",
            "Occlusion culling data is being generated. Check the Console for progress.\n\n" +
            "This may take several minutes for large scenes. You can continue working while it bakes.",
            "OK");
        
        // Refresh after a delay
        EditorApplication.delayCall += () =>
        {
            AnalyzeScene();
            Repaint();
        };
    }
    
    private void ClearOcclusionData()
    {
        StaticOcclusionCulling.Clear();
        //Debug.Log("Occlusion culling data cleared");
    }
    
    private string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        
        return $"{len:F2} {sizes[order]}";
    }
}
