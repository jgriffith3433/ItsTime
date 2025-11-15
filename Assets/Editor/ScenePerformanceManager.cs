using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class ScenePerformanceManager : EditorWindow
{
    private Vector2 scrollPosition;
    private List<MeshInfo> meshInfoList = new List<MeshInfo>();
    private bool showOnlyWithoutLOD = false;
    private bool showOnlyWithLOD = false;
    private int minPolyCount = 0;
    private int maxPolyCount = int.MaxValue;
    private string searchFilter = "";
    private SortMode sortMode = SortMode.PolyCountDesc;
    
    // LOD Settings
    private float lod0Distance = 100f;
    private float lod1Distance = 700f;
    private float lod2Distance = 1000f;
    private float cullDistance = 1000f;
    private float lod1Quality = 0.6f;
    private float lod2Quality = 0.3f;
    private bool preserveBorders = true;
    private bool preserveSeams = true;
    

    // Smart LOD Settings
    private int minPolyCountForLOD = 50;  // Don't add LOD to objects below this poly count
    private bool skipLowPolyObjects = true;
    private bool autoAdjustQualityByPolyCount = true;
    
    // Statistics
    private int totalMeshes = 0;
    private int totalPolygons = 0;
    private int meshesWithLOD = 0;
    private int meshesWithoutLOD = 0;
    
    private enum SortMode
    {
        PolyCountDesc,
        PolyCountAsc,
        Name,
        LODStatus
    }
    
    private class MeshInfo
    {
        public GameObject gameObject;
        public MeshFilter meshFilter;
        public MeshRenderer meshRenderer;
        public int triangleCount;
        public int vertexCount;
        public LODGroup lodGroup;
        public bool hasLOD;
        public string path;
        public bool isSelected;
    }
    
    [MenuItem("Tools/Scene Performance Manager")]
    public static void ShowWindow()
    {
        var window = GetWindow<ScenePerformanceManager>("Scene Performance");
        window.minSize = new Vector2(800, 600);
    }
    
    private void OnEnable()
    {
        RefreshMeshList();
    }
    
    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        DrawHeader();
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(5);
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        DrawStatistics();
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(5);
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        DrawFilters();
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(5);
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        DrawLODSettings();
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(5);
        
        DrawMeshList();
        
        EditorGUILayout.EndScrollView();
    }
    
    private void DrawHeader()
    {
        EditorGUILayout.LabelField("Scene Performance Manager", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Refresh Mesh List", GUILayout.Height(30)))
        {
            RefreshMeshList();
        }
        
        GUI.backgroundColor = Color.cyan;
        if (GUILayout.Button("Performance Report", GUILayout.Height(30)))
        {
            ShowPerformanceReport();
        }
        GUI.backgroundColor = Color.white;
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("Add LOD to Selected", GUILayout.Height(30)))
        {
            AddLODToSelected();
        }
        
        GUI.backgroundColor = Color.red;
        if (GUILayout.Button("Remove LOD from Selected", GUILayout.Height(30)))
        {
            RemoveLODFromSelected();
        }
        
        GUI.backgroundColor = new Color(1f, 0.3f, 0.3f);
        if (GUILayout.Button("Clear ALL LODs", GUILayout.Height(30)))
        {
            ClearAllLODs();
        }
        
        GUI.backgroundColor = Color.white;
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Select All"))
        {
            SelectAll(true);
        }
        
        if (GUILayout.Button("Deselect All"))
        {
            SelectAll(false);
        }
        
        if (GUILayout.Button("Select High Poly (>1000)"))
        {
            SelectByPolyCount(1000, int.MaxValue);
        }
        
        if (GUILayout.Button("Select Very High (>5000)"))
        {
            SelectByPolyCount(5000, int.MaxValue);
        }
        
        if (GUILayout.Button("Select Without LOD"))
        {
            SelectByLODStatus(false);
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Select Medium (500-1000)"))
        {
            SelectByPolyCount(500, 1000);
        }
        
        if (GUILayout.Button("Select Low (<500)"))
        {
            SelectByPolyCount(0, 500);
        }
        
        if (GUILayout.Button("Select Needs LOD (>500, no LOD)"))
        {
            SelectNeedsLOD();
        }
        
        EditorGUILayout.EndHorizontal();
    }
    
    private void DrawStatistics()
    {
        EditorGUILayout.LabelField("Statistics", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"Total Meshes: {totalMeshes}", GUILayout.Width(200));
        EditorGUILayout.LabelField($"Total Polygons: {totalPolygons:N0}", GUILayout.Width(200));
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"With LOD: {meshesWithLOD}", GUILayout.Width(200));
        EditorGUILayout.LabelField($"Without LOD: {meshesWithoutLOD}", GUILayout.Width(200));
        EditorGUILayout.EndHorizontal();
        
        float lodPercentage = totalMeshes > 0 ? (meshesWithLOD / (float)totalMeshes) * 100f : 0f;
        EditorGUILayout.LabelField($"LOD Coverage: {lodPercentage:F1}%");
        
        Rect rect = GUILayoutUtility.GetRect(18, 18, GUILayout.ExpandWidth(true));
        EditorGUI.ProgressBar(rect, lodPercentage / 100f, $"{lodPercentage:F1}%");
    }
    
    private void DrawFilters()
    {
        EditorGUILayout.LabelField("Filters", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Search:", GUILayout.Width(60));
        searchFilter = EditorGUILayout.TextField(searchFilter);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        showOnlyWithoutLOD = EditorGUILayout.ToggleLeft("Show Only Without LOD", showOnlyWithoutLOD, GUILayout.Width(200));
        showOnlyWithLOD = EditorGUILayout.ToggleLeft("Show Only With LOD", showOnlyWithLOD, GUILayout.Width(200));
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Poly Count Range:", GUILayout.Width(120));
        minPolyCount = EditorGUILayout.IntField(minPolyCount, GUILayout.Width(80));
        EditorGUILayout.LabelField("to", GUILayout.Width(20));
        maxPolyCount = EditorGUILayout.IntField(maxPolyCount, GUILayout.Width(80));
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Sort By:", GUILayout.Width(60));
        sortMode = (SortMode)EditorGUILayout.EnumPopup(sortMode);
        EditorGUILayout.EndHorizontal();
    }
    
    private void DrawLODSettings()
    {
        EditorGUILayout.LabelField("LOD Settings", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("LOD0 Distance:", GUILayout.Width(120));
        lod0Distance = EditorGUILayout.Slider(lod0Distance, 1f, 1000f);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("LOD1 Distance:", GUILayout.Width(120));
        lod1Distance = EditorGUILayout.Slider(lod1Distance, 1f, 1000f);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("LOD2 Distance:", GUILayout.Width(120));
        lod2Distance = EditorGUILayout.Slider(lod2Distance, 1f, 1000f);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Cull Distance:", GUILayout.Width(120));
        cullDistance = EditorGUILayout.Slider(cullDistance, 1f, 1000f);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        
        EditorGUILayout.BeginHorizontal();
        GUI.backgroundColor = Color.cyan;
        if (GUILayout.Button("Update All LOD Distances", GUILayout.Height(25)))
        {
            UpdateAllLODDistances();
        }
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        
        EditorGUILayout.LabelField("Auto-Generate LOD Settings", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("LOD1 Quality:", GUILayout.Width(120));
        lod1Quality = EditorGUILayout.Slider(lod1Quality, 0.1f, 1f);
        EditorGUILayout.LabelField($"{(lod1Quality * 100):F0}%", GUILayout.Width(50));
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("LOD2 Quality:", GUILayout.Width(120));
        lod2Quality = EditorGUILayout.Slider(lod2Quality, 0.05f, 1f);
        EditorGUILayout.LabelField($"{(lod2Quality * 100):F0}%", GUILayout.Width(50));
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        
        // Smart LOD settings
        GUI.backgroundColor = new Color(0.5f, 1f, 0.5f);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUI.backgroundColor = Color.white;
        
        EditorGUILayout.LabelField("Smart LOD Protection", EditorStyles.boldLabel);
        
        skipLowPolyObjects = EditorGUILayout.ToggleLeft("Skip Low-Poly Objects (Recommended)", skipLowPolyObjects);
        
        EditorGUI.BeginDisabledGroup(!skipLowPolyObjects);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Min Poly Count for LOD:", GUILayout.Width(150));
        minPolyCountForLOD = EditorGUILayout.IntSlider(minPolyCountForLOD, 50, 5000);
        EditorGUILayout.EndHorizontal();
        EditorGUI.EndDisabledGroup();
        
        EditorGUILayout.HelpBox(
            $"Objects with fewer than {minPolyCountForLOD} triangles will NOT get LODs added. " +
            "This prevents destroying already-optimized low-poly objects.", MessageType.Info);
        
        EditorGUILayout.Space(3);
        
        autoAdjustQualityByPolyCount = EditorGUILayout.ToggleLeft("Auto-Adjust Quality by Poly Count (Recommended)", autoAdjustQualityByPolyCount);
        
        if (autoAdjustQualityByPolyCount)
        {
            EditorGUILayout.HelpBox(
                "High-poly objects (>5000 tris) get more aggressive reduction.\n" +
                "Medium-poly objects (1000-5000 tris) use standard settings.\n" +
                "This prevents over-simplifying medium-detail objects.", MessageType.Info);
        }
        
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(5);
        
        preserveBorders = EditorGUILayout.ToggleLeft("Preserve Mesh Borders", preserveBorders);
        preserveSeams = EditorGUILayout.ToggleLeft("Preserve UV Seams", preserveSeams);
        
        EditorGUILayout.Space(5);
        
        // Preset buttons
        EditorGUILayout.LabelField("Quality Presets:", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Aggressive (Mobile)"))
        {
            lod1Quality = 0.4f;
            lod2Quality = 0.15f;
            minPolyCountForLOD = 500;
            skipLowPolyObjects = true;
            autoAdjustQualityByPolyCount = true;
        }
        
        if (GUILayout.Button("Balanced"))
        {
            lod1Quality = 0.6f;
            lod2Quality = 0.3f;
            minPolyCountForLOD = 500;
            skipLowPolyObjects = true;
            autoAdjustQualityByPolyCount = true;
        }
        
        if (GUILayout.Button("Conservative"))
        {
            lod1Quality = 0.75f;
            lod2Quality = 0.5f;
            minPolyCountForLOD = 1000;
            skipLowPolyObjects = true;
            autoAdjustQualityByPolyCount = false;
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        
        EditorGUILayout.BeginHorizontal();
        GUI.backgroundColor = new Color(1f, 0.7f, 0f);
        if (GUILayout.Button("Auto-Generate LODs for Selected", GUILayout.Height(30)))
        {
            AutoGenerateLODsForSelected();
        }
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.HelpBox(
            "Smart LOD protection enabled: Low-poly objects will be skipped automatically.\n" +
            "Use 'Select High Poly' buttons to target objects that need LODs most.", MessageType.Info);
    }
    
    private void DrawMeshList()
    {
        var filteredList = GetFilteredMeshList();
        
        EditorGUILayout.LabelField($"Meshes ({filteredList.Count})", EditorStyles.boldLabel);
        
        // Header
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.Label("", GUILayout.Width(20)); // Checkbox
        GUILayout.Label("Object Name", EditorStyles.miniLabel, GUILayout.Width(250));
        GUILayout.Label("Triangles", EditorStyles.miniLabel, GUILayout.Width(80));
        GUILayout.Label("Vertices", EditorStyles.miniLabel, GUILayout.Width(80));
        GUILayout.Label("LOD", EditorStyles.miniLabel, GUILayout.Width(50));
        GUILayout.Label("Actions", EditorStyles.miniLabel, GUILayout.ExpandWidth(true));
        EditorGUILayout.EndHorizontal();
        
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        foreach (var meshInfo in filteredList)
        {
            if (meshInfo.gameObject == null) continue;
            
            EditorGUILayout.BeginHorizontal();
            
            // Checkbox
            meshInfo.isSelected = EditorGUILayout.Toggle(meshInfo.isSelected, GUILayout.Width(20));
            
            // Object name (clickable)
            if (GUILayout.Button(meshInfo.gameObject.name, EditorStyles.label, GUILayout.Width(250)))
            {
                Selection.activeGameObject = meshInfo.gameObject;
                EditorGUIUtility.PingObject(meshInfo.gameObject);
            }
            
            // Triangle count with color coding
            Color originalColor = GUI.color;
            if (meshInfo.triangleCount > 1000)
                GUI.color = Color.red;
            else if (meshInfo.triangleCount > 500)
                GUI.color = Color.yellow;
            
            EditorGUILayout.LabelField(meshInfo.triangleCount.ToString("N0"), GUILayout.Width(80));
            GUI.color = originalColor;
            
            // Vertex count
            EditorGUILayout.LabelField(meshInfo.vertexCount.ToString("N0"), GUILayout.Width(80));
            
            // LOD status
            if (meshInfo.hasLOD)
            {
                GUI.color = Color.green;
                EditorGUILayout.LabelField("✓", GUILayout.Width(50));
            }
            else
            {
                GUI.color = Color.gray;
                EditorGUILayout.LabelField("✗", GUILayout.Width(50));
            }
            GUI.color = originalColor;
            
            // Actions
            EditorGUILayout.BeginHorizontal();
            
            if (!meshInfo.hasLOD)
            {
                if (GUILayout.Button("Add LOD", GUILayout.Width(80)))
                {
                    AddLODToMesh(meshInfo);
                }
            }
            else
            {
                if (GUILayout.Button("Remove LOD", GUILayout.Width(80)))
                {
                    RemoveLODFromMesh(meshInfo);
                }
                
                if (GUILayout.Button("Edit", GUILayout.Width(50)))
                {
                    Selection.activeGameObject = meshInfo.gameObject;
                }
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndHorizontal();
        }
        
        EditorGUILayout.EndScrollView();
    }
    
    private void RefreshMeshList()
    {
        meshInfoList.Clear();
        
        // Find all MeshFilters in the scene (excluding SkinnedMeshRenderers)
        var allMeshFilters = FindObjectsOfType<MeshFilter>();
        
        foreach (var meshFilter in allMeshFilters)
        {
            // Skip if parent has SkinnedMeshRenderer
            if (meshFilter.GetComponent<SkinnedMeshRenderer>() != null)
                continue;
            
            // Skip LOD child objects (LOD1, LOD2, etc.)
            if (meshFilter.gameObject.name == "LOD1" || meshFilter.gameObject.name == "LOD2")
                continue;
            
            var mesh = meshFilter.sharedMesh;
            if (mesh == null) continue;
            
            var meshInfo = new MeshInfo
            {
                gameObject = meshFilter.gameObject,
                meshFilter = meshFilter,
                meshRenderer = meshFilter.GetComponent<MeshRenderer>(),
                triangleCount = mesh.triangles.Length / 3,
                vertexCount = mesh.vertexCount,
                lodGroup = meshFilter.GetComponentInParent<LODGroup>(),
                path = GetGameObjectPath(meshFilter.gameObject)
            };
            
            meshInfo.hasLOD = meshInfo.lodGroup != null;
            meshInfoList.Add(meshInfo);
        }
        
        UpdateStatistics();
        Repaint();
    }
    
    private void UpdateStatistics()
    {
        totalMeshes = meshInfoList.Count;
        totalPolygons = meshInfoList.Sum(m => m.triangleCount);
        meshesWithLOD = meshInfoList.Count(m => m.hasLOD);
        meshesWithoutLOD = meshInfoList.Count(m => !m.hasLOD);
    }
    
    private List<MeshInfo> GetFilteredMeshList()
    {
        var filtered = meshInfoList.Where(m => m.gameObject != null).ToList();
        
        // Apply filters
        if (!string.IsNullOrEmpty(searchFilter))
        {
            filtered = filtered.Where(m => m.gameObject.name.ToLower().Contains(searchFilter.ToLower())).ToList();
        }
        
        if (showOnlyWithoutLOD)
        {
            filtered = filtered.Where(m => !m.hasLOD).ToList();
        }
        else if (showOnlyWithLOD)
        {
            filtered = filtered.Where(m => m.hasLOD).ToList();
        }
        
        filtered = filtered.Where(m => m.triangleCount >= minPolyCount && m.triangleCount <= maxPolyCount).ToList();
        
        // Apply sorting
        switch (sortMode)
        {
            case SortMode.PolyCountDesc:
                filtered = filtered.OrderByDescending(m => m.triangleCount).ToList();
                break;
            case SortMode.PolyCountAsc:
                filtered = filtered.OrderBy(m => m.triangleCount).ToList();
                break;
            case SortMode.Name:
                filtered = filtered.OrderBy(m => m.gameObject.name).ToList();
                break;
            case SortMode.LODStatus:
                filtered = filtered.OrderByDescending(m => m.hasLOD).ToList();
                break;
        }
        
        return filtered;
    }
    
    private void AddLODToMesh(MeshInfo meshInfo)
    {
        if (meshInfo.hasLOD) return;
        
        Undo.RegisterCompleteObjectUndo(meshInfo.gameObject, "Add LOD Group");
        
        var lodGroup = meshInfo.gameObject.AddComponent<LODGroup>();
        var lods = new LOD[3];
        
        // Screen relative heights must be in descending order (closer = higher value)
        float lod0Height = 1.0f - (lod0Distance / cullDistance);
        float lod1Height = 1.0f - (lod1Distance / cullDistance);
        float lod2Height = 1.0f - (lod2Distance / cullDistance);
        
        // Ensure descending order
        lod0Height = Mathf.Clamp01(lod0Height);
        lod1Height = Mathf.Clamp(lod1Height, 0.01f, lod0Height - 0.01f);
        lod2Height = Mathf.Clamp(lod2Height, 0.0f, lod1Height - 0.01f);
        
        // LOD 0 - Original mesh
        lods[0] = new LOD(lod0Height, new Renderer[] { meshInfo.meshRenderer });
        
        // LOD 1 - Same mesh for now (will be replaced if auto-generate is used)
        lods[1] = new LOD(lod1Height, new Renderer[] { meshInfo.meshRenderer });
        
        // LOD 2 - Same mesh for now (will be replaced if auto-generate is used)
        lods[2] = new LOD(lod2Height, new Renderer[] { meshInfo.meshRenderer });
        
        lodGroup.SetLODs(lods);
        lodGroup.RecalculateBounds();
        
        // Add DynamicLOD component with enableDynamic set to true
        var dynamicLOD = meshInfo.gameObject.AddComponent<DynamicLOD>();
        dynamicLOD.enableDynamic = true;
        
        meshInfo.lodGroup = lodGroup;
        meshInfo.hasLOD = true;
        
        EditorUtility.SetDirty(meshInfo.gameObject);
        UpdateStatistics();
    }
    
    private void RemoveLODFromMesh(MeshInfo meshInfo)
    {
        if (!meshInfo.hasLOD || meshInfo.lodGroup == null) return;
        
        GameObject parentObject = meshInfo.gameObject;
        
        // Find and destroy LOD child objects (LOD1, LOD2, etc.)
        Transform parentTransform = meshInfo.lodGroup.transform;
        List<GameObject> lodChildren = new List<GameObject>();
        
        foreach (Transform child in parentTransform)
        {
            if (child.name.StartsWith("LOD") && (child.name == "LOD1" || child.name == "LOD2"))
            {
                lodChildren.Add(child.gameObject);
            }
        }
        
        // Destroy LOD children first
        foreach (var lodChild in lodChildren)
        {
            Undo.DestroyObjectImmediate(lodChild);
        }
        
        // Then destroy the LODGroup component
        Undo.DestroyObjectImmediate(meshInfo.lodGroup);
        
        meshInfo.lodGroup = null;
        meshInfo.hasLOD = false;
        
        // Only set dirty if the parent object still exists
        if (parentObject != null)
        {
            EditorUtility.SetDirty(parentObject);
        }
        
        UpdateStatistics();
    }
    
    private void AddLODToSelected()
    {
        var selected = meshInfoList.Where(m => m.isSelected && !m.hasLOD).ToList();
        
        if (selected.Count == 0)
        {
            EditorUtility.DisplayDialog("No Selection", "Please select meshes without LOD to add LOD groups.", "OK");
            return;
        }
        
        if (EditorUtility.DisplayDialog("Add LOD Groups", 
            $"Add LOD groups to {selected.Count} selected mesh(es)?", "Yes", "Cancel"))
        {
            foreach (var meshInfo in selected)
            {
                AddLODToMesh(meshInfo);
            }
            
            RefreshMeshList();
            EditorUtility.DisplayDialog("Success", $"Added LOD groups to {selected.Count} mesh(es).", "OK");
        }
    }
    
    private void RemoveLODFromSelected()
    {
        var selected = meshInfoList.Where(m => m.isSelected && m.hasLOD).ToList();
        
        if (selected.Count == 0)
        {
            EditorUtility.DisplayDialog("No Selection", "Please select meshes with LOD to remove LOD groups.", "OK");
            return;
        }
        
        if (EditorUtility.DisplayDialog("Remove LOD Groups", 
            $"Remove LOD groups from {selected.Count} selected mesh(es)?", "Yes", "Cancel"))
        {
            foreach (var meshInfo in selected)
            {
                RemoveLODFromMesh(meshInfo);
            }
            
            RefreshMeshList();
            EditorUtility.DisplayDialog("Success", $"Removed LOD groups from {selected.Count} mesh(es).", "OK");
        }
    }
    
    private void ClearAllLODs()
    {
        var withLOD = meshInfoList.Where(m => m.hasLOD && m.gameObject != null).ToList();
        
        if (withLOD.Count == 0)
        {
            EditorUtility.DisplayDialog("No LODs Found", "No meshes with LOD groups found in the scene.", "OK");
            return;
        }
        
        if (EditorUtility.DisplayDialog("Clear ALL LOD Groups", 
            $"This will remove LOD groups and delete all LOD child objects from {withLOD.Count} mesh(es).\n\n" +
            "This action can be undone with Ctrl+Z.\n\n" +
            "Continue?", "Yes, Clear All", "Cancel"))
        {
            int removed = 0;
            
            foreach (var meshInfo in withLOD)
            {
                if (meshInfo.gameObject != null && meshInfo.hasLOD)
                {
                    RemoveLODFromMesh(meshInfo);
                    removed++;
                }
            }
            
            RefreshMeshList();
            EditorUtility.DisplayDialog("Success", 
                $"Cleared LOD groups from {removed} mesh(es).\n\nAll LOD child objects have been deleted.", "OK");
        }
    }
    
    private void SelectAll(bool select)
    {
        var filtered = GetFilteredMeshList();
        foreach (var meshInfo in filtered)
        {
            meshInfo.isSelected = select;
        }
        Repaint();
    }
    
    private void SelectByPolyCount(int min, int max)
    {
        foreach (var meshInfo in meshInfoList)
        {
            meshInfo.isSelected = meshInfo.triangleCount >= min && meshInfo.triangleCount < max;
        }
        Repaint();
    }
    
    private void SelectByLODStatus(bool hasLOD)
    {
        foreach (var meshInfo in meshInfoList)
        {
            meshInfo.isSelected = meshInfo.hasLOD == hasLOD;
        }
        Repaint();
    }
    
    private void SelectNeedsLOD()
    {
        foreach (var meshInfo in meshInfoList)
        {
            meshInfo.isSelected = !meshInfo.hasLOD && meshInfo.triangleCount >= minPolyCountForLOD;
        }
        Repaint();
    }
    
    private string GetGameObjectPath(GameObject obj)
    {
        string path = obj.name;
        Transform parent = obj.transform.parent;
        
        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }
        
        return path;
    }
    
    private void UpdateAllLODDistances()
    {
        var lodGroups = FindObjectsOfType<LODGroup>();
        
        if (lodGroups.Length == 0)
        {
            EditorUtility.DisplayDialog("No LOD Groups", "No LOD groups found in the scene.", "OK");
            return;
        }
        
        if (EditorUtility.DisplayDialog("Update All LOD Distances", 
            $"Update LOD distances for {lodGroups.Length} LOD group(s) in the scene?", "Yes", "Cancel"))
        {
            int updated = 0;
            
            foreach (var lodGroup in lodGroups)
            {
                Undo.RegisterCompleteObjectUndo(lodGroup, "Update LOD Distances");
                
                LOD[] lods = lodGroup.GetLODs();
                
                // Screen relative heights must be in descending order (closer = higher value)
                float lod0Height = 1.0f - (lod0Distance / cullDistance);
                float lod1Height = 1.0f - (lod1Distance / cullDistance);
                float lod2Height = 1.0f - (lod2Distance / cullDistance);
                
                // Ensure descending order
                lod0Height = Mathf.Clamp01(lod0Height);
                lod1Height = Mathf.Clamp(lod1Height, 0.01f, lod0Height - 0.01f);
                lod2Height = Mathf.Clamp(lod2Height, 0.0f, lod1Height - 0.01f);
                
                if (lods.Length > 0)
                    lods[0].screenRelativeTransitionHeight = lod0Height;
                if (lods.Length > 1)
                    lods[1].screenRelativeTransitionHeight = lod1Height;
                if (lods.Length > 2)
                    lods[2].screenRelativeTransitionHeight = lod2Height;
                
                lodGroup.SetLODs(lods);
                lodGroup.RecalculateBounds();
                EditorUtility.SetDirty(lodGroup);
                updated++;
            }
            
            EditorUtility.DisplayDialog("Success", $"Updated {updated} LOD group(s) with new distances.", "OK");
        }
    }
    
    private void AutoGenerateLODsForSelected()
    {
        var selected = meshInfoList.Where(m => m.isSelected).ToList();
        
        if (selected.Count == 0)
        {
            EditorUtility.DisplayDialog("No Selection", "Please select meshes to auto-generate LODs.", "OK");
            return;
        }
        
        // Count low-poly objects for informational message
        int lowPolyCount = 0;
        if (skipLowPolyObjects)
        {
            lowPolyCount = selected.Count(m => m.triangleCount < minPolyCountForLOD);
        }
        
        string lowPolyMessage = lowPolyCount > 0 ? $"\n\n{lowPolyCount} low-poly objects will use original mesh for all LOD levels." : "";
        
        if (EditorUtility.DisplayDialog("Auto-Generate LODs", 
            $"Generate LOD meshes for {selected.Count} selected mesh(es)?\n\n" +
            $"LOD1: {lod1Quality * 100:F0}% quality\n" +
            $"LOD2: {lod2Quality * 100:F0}% quality\n" +
            $"Min Poly Count: {minPolyCountForLOD}{lowPolyMessage}\n\n" +
            "This will create new child GameObjects with meshes.", "Generate", "Cancel"))
        {
            int successCount = 0;
            int failCount = 0;
            
            for (int i = 0; i < selected.Count; i++)
            {
                EditorUtility.DisplayProgressBar("Generating LODs", 
                    $"Processing {selected[i].gameObject.name} ({i + 1}/{selected.Count})", 
                    (float)i / selected.Count);
                
                if (GenerateLODForMesh(selected[i]))
                    successCount++;
                else
                    failCount++;
            }
            
            EditorUtility.ClearProgressBar();
            RefreshMeshList();
            
            string message = $"Successfully generated LODs for {successCount} mesh(es).";
            if (failCount > 0)
                message += $"\n{failCount} mesh(es) failed (may already have LOD children).";
            if (lowPolyCount > 0)
                message += $"\n{lowPolyCount} low-poly mesh(es) used original mesh for all LOD levels.";
            
            EditorUtility.DisplayDialog("LOD Generation Complete", message, "OK");
        }
    }
    
    private bool GenerateLODForMesh(MeshInfo meshInfo)
    {
        if (meshInfo.meshFilter == null || meshInfo.meshFilter.sharedMesh == null)
            return false;
        
        Mesh originalMesh = meshInfo.meshFilter.sharedMesh;
        GameObject parentObj = meshInfo.gameObject;
        
        // Check if LOD children already exist
        Transform existingLOD1 = parentObj.transform.Find("LOD1");
        Transform existingLOD2 = parentObj.transform.Find("LOD2");
        
        if (existingLOD1 != null || existingLOD2 != null)
        {
            //Debug.LogWarning($"LOD children already exist for {parentObj.name}. Skipping.");
            return false;
        }
        
        // Create or get LODGroup
        LODGroup lodGroup = parentObj.GetComponent<LODGroup>();
        if (lodGroup == null)
        {
            lodGroup = parentObj.AddComponent<LODGroup>();
        }
        
        // Create or get DynamicLOD component
        DynamicLOD dynamicLOD = parentObj.GetComponent<DynamicLOD>();
        if (dynamicLOD == null)
        {
            dynamicLOD = parentObj.AddComponent<DynamicLOD>();
            dynamicLOD.enableDynamic = true;
        }
        
        Undo.RegisterCompleteObjectUndo(parentObj, "Generate LODs");
        
        // Check if this is a low-poly object that should just copy the mesh
        bool isLowPoly = skipLowPolyObjects && meshInfo.triangleCount < minPolyCountForLOD;
        
        Mesh lod1Mesh;
        Mesh lod2Mesh;
        
        if (isLowPoly)
        {
            // For low-poly objects, just use copies of the original mesh
            lod1Mesh = Object.Instantiate(originalMesh);
            lod1Mesh.name = originalMesh.name + "_LOD1_Copy";
            lod2Mesh = Object.Instantiate(originalMesh);
            lod2Mesh.name = originalMesh.name + "_LOD2_Copy";
        }
        else
        {
            // Auto-adjust quality based on poly count if enabled
            float adjustedLod1Quality = lod1Quality;
            float adjustedLod2Quality = lod2Quality;
            
            if (autoAdjustQualityByPolyCount)
            {
                int triangleCount = meshInfo.triangleCount;
                
                if (triangleCount > 5000)
                {
                    // Very high poly - more aggressive reduction
                    adjustedLod1Quality = Mathf.Max(0.3f, lod1Quality * 0.7f);
                    adjustedLod2Quality = Mathf.Max(0.1f, lod2Quality * 0.6f);
                }
                else if (triangleCount > 2000)
                {
                    // High poly - moderately aggressive
                    adjustedLod1Quality = Mathf.Max(0.4f, lod1Quality * 0.85f);
                    adjustedLod2Quality = Mathf.Max(0.15f, lod2Quality * 0.8f);
                }
                // Else: use standard quality settings
            }
            
            // Generate simplified meshes for normal poly count objects
            lod1Mesh = GenerateSimplifiedMesh(originalMesh, adjustedLod1Quality);
            lod2Mesh = GenerateSimplifiedMesh(originalMesh, adjustedLod2Quality);
        }
        
        GameObject lod1Obj = CreateLODChild(parentObj, "LOD1", lod1Mesh, meshInfo.meshRenderer.sharedMaterials);
        GameObject lod2Obj = CreateLODChild(parentObj, "LOD2", lod2Mesh, meshInfo.meshRenderer.sharedMaterials);
        
        // Setup LOD levels
        // Screen relative heights must be in descending order (closer = higher value)
        // Convert distances to screen relative heights (inverse relationship)
        float lod0Height = 1.0f - (lod0Distance / cullDistance);
        float lod1Height = 1.0f - (lod1Distance / cullDistance);
        float lod2Height = 1.0f - (lod2Distance / cullDistance);
        
        // Ensure descending order
        lod0Height = Mathf.Clamp01(lod0Height);
        lod1Height = Mathf.Clamp(lod1Height, 0.01f, lod0Height - 0.01f);
        lod2Height = Mathf.Clamp(lod2Height, 0.0f, lod1Height - 0.01f);
        
        LOD[] lods = new LOD[3];
        lods[0] = new LOD(lod0Height, new Renderer[] { meshInfo.meshRenderer });
        lods[1] = new LOD(lod1Height, new Renderer[] { lod1Obj.GetComponent<MeshRenderer>() });
        lods[2] = new LOD(lod2Height, new Renderer[] { lod2Obj.GetComponent<MeshRenderer>() });
        
        lodGroup.SetLODs(lods);
        lodGroup.RecalculateBounds();
        
        EditorUtility.SetDirty(parentObj);
        
        return true;
    }
    
    private GameObject CreateLODChild(GameObject parent, string name, Mesh mesh, Material[] materials)
    {
        GameObject lodObj = new GameObject(name);
        lodObj.transform.SetParent(parent.transform, false);
        lodObj.transform.localPosition = Vector3.zero;
        lodObj.transform.localRotation = Quaternion.identity;
        lodObj.transform.localScale = Vector3.one;
        
        MeshFilter mf = lodObj.AddComponent<MeshFilter>();
        mf.sharedMesh = mesh;
        
        MeshRenderer mr = lodObj.AddComponent<MeshRenderer>();
        mr.sharedMaterials = materials;
        
        Undo.RegisterCreatedObjectUndo(lodObj, "Create LOD Child");
        
        return lodObj;
    }
    
    private Mesh GenerateSimplifiedMesh(Mesh originalMesh, float quality)
    {
        if (quality >= 0.95f)
        {
            // If quality is very high, just return a copy
            Mesh copy = Object.Instantiate(originalMesh);
            copy.name = originalMesh.name + "_LOD";
            return copy;
        }
        
        // Create a copy of the original mesh
        Mesh simplifiedMesh = Object.Instantiate(originalMesh);
        simplifiedMesh.name = originalMesh.name + $"_LOD_Q{(quality * 100):F0}";
        
        // Get original mesh data
        Vector3[] originalVertices = originalMesh.vertices;
        Vector3[] originalNormals = originalMesh.normals;
        Vector2[] originalUV = originalMesh.uv;
        Vector2[] originalUV2 = originalMesh.uv2;
        Vector2[] originalUV3 = originalMesh.uv3;
        Vector2[] originalUV4 = originalMesh.uv4;
        Color[] originalColors = originalMesh.colors;
        BoneWeight[] originalBoneWeights = originalMesh.boneWeights;
        Vector4[] originalTangents = originalMesh.tangents;
        int[] originalTriangles = originalMesh.triangles;
        
        // Build vertex uniqueness map based on position AND UV
        Dictionary<string, int> vertexMap = new Dictionary<string, int>();
        List<Vector3> newVertices = new List<Vector3>();
        List<Vector3> newNormals = new List<Vector3>();
        List<Vector2> newUV = new List<Vector2>();
        List<Vector2> newUV2 = new List<Vector2>();
        List<Vector2> newUV3 = new List<Vector2>();
        List<Vector2> newUV4 = new List<Vector2>();
        List<Color> newColors = new List<Color>();
        List<BoneWeight> newBoneWeights = new List<BoneWeight>();
        List<Vector4> newTangents = new List<Vector4>();
        List<int> vertexRemapping = new List<int>();
        
        // More conservative thresholds to reduce holes
        float positionThreshold = 0.0001f; // Fixed threshold, don't make it quality-dependent
        float uvThreshold = 0.001f; // Very tight to preserve UVs
        
        // First pass: build unique vertex list considering both position and UV
        for (int i = 0; i < originalVertices.Length; i++)
        {
            Vector3 pos = originalVertices[i];
            Vector2 uv = (originalUV != null && originalUV.Length > i) ? originalUV[i] : Vector2.zero;
            
            // Create a key based on both position and UV
            string key = $"{Mathf.Round(pos.x / positionThreshold)},{Mathf.Round(pos.y / positionThreshold)},{Mathf.Round(pos.z / positionThreshold)}" +
                         $"_{Mathf.Round(uv.x / uvThreshold)},{Mathf.Round(uv.y / uvThreshold)}";
            
            int newIndex;
            if (vertexMap.ContainsKey(key))
            {
                newIndex = vertexMap[key];
            }
            else
            {
                newIndex = newVertices.Count;
                vertexMap[key] = newIndex;
                
                newVertices.Add(pos);
                
                if (originalNormals != null && originalNormals.Length > i)
                    newNormals.Add(originalNormals[i]);
                    
                if (originalUV != null && originalUV.Length > i)
                    newUV.Add(originalUV[i]);
                    
                if (originalUV2 != null && originalUV2.Length > i)
                    newUV2.Add(originalUV2[i]);
                    
                if (originalUV3 != null && originalUV3.Length > i)
                    newUV3.Add(originalUV3[i]);
                    
                if (originalUV4 != null && originalUV4.Length > i)
                    newUV4.Add(originalUV4[i]);
                
                if (originalColors != null && originalColors.Length > i)
                    newColors.Add(originalColors[i]);
                    
                if (originalBoneWeights != null && originalBoneWeights.Length > i)
                    newBoneWeights.Add(originalBoneWeights[i]);
                    
                if (originalTangents != null && originalTangents.Length > i)
                    newTangents.Add(originalTangents[i]);
            }
            
            vertexRemapping.Add(newIndex);
        }
        
        // Second pass: Keep ALL triangles that aren't degenerate (no skipping)
        // This prevents holes - we only simplify by vertex merging, not triangle removal
        List<int> newTriangles = new List<int>();
        
        for (int i = 0; i < originalTriangles.Length; i += 3)
        {
            int idx0 = vertexRemapping[originalTriangles[i]];
            int idx1 = vertexRemapping[originalTriangles[i + 1]];
            int idx2 = vertexRemapping[originalTriangles[i + 2]];
            
            // Only skip degenerate triangles (where vertices merged to same point)
            if (idx0 != idx1 && idx1 != idx2 && idx0 != idx2)
            {
                newTriangles.Add(idx0);
                newTriangles.Add(idx1);
                newTriangles.Add(idx2);
            }
        }
        
        // Build the simplified mesh
        Mesh result = new Mesh();
        result.name = simplifiedMesh.name;
        result.vertices = newVertices.ToArray();
        result.triangles = newTriangles.ToArray();
        
        if (newNormals.Count == newVertices.Count)
            result.normals = newNormals.ToArray();
        else
            result.RecalculateNormals();
        
        if (newUV.Count == newVertices.Count)
            result.uv = newUV.ToArray();
            
        if (newUV2.Count == newVertices.Count)
            result.uv2 = newUV2.ToArray();
            
        if (newUV3.Count == newVertices.Count)
            result.uv3 = newUV3.ToArray();
            
        if (newUV4.Count == newVertices.Count)
            result.uv4 = newUV4.ToArray();
        
        if (newColors.Count == newVertices.Count)
            result.colors = newColors.ToArray();
            
        if (originalBoneWeights != null && newBoneWeights.Count == newVertices.Count)
            result.boneWeights = newBoneWeights.ToArray();
            
        if (newTangents.Count == newVertices.Count)
            result.tangents = newTangents.ToArray();
        else
            result.RecalculateTangents();
        
        result.RecalculateBounds();
        
        // Optimize the mesh without changing topology
        UnityEditor.MeshUtility.Optimize(result);
        
        return result;
    }

    private void ShowPerformanceReport()
    {
        RefreshMeshList();
        var veryHighPoly = meshInfoList.Where(m => m.triangleCount > 5000).ToList();
        var highPoly = meshInfoList.Where(m => m.triangleCount >= 2000 && m.triangleCount <= 5000).ToList();
        var mediumPoly = meshInfoList.Where(m => m.triangleCount >= 500 && m.triangleCount < 2000).ToList();
        var lowPoly = meshInfoList.Where(m => m.triangleCount < 500).ToList();
        var needsLOD = meshInfoList.Where(m => !m.hasLOD && m.triangleCount >= minPolyCountForLOD).ToList();
        int totalTrianglesWithoutLOD = needsLOD.Sum(m => m.triangleCount);
        int potentialSavings = (int)(totalTrianglesWithoutLOD * (1.0f - lod1Quality));
        string report = "=== SCENE PERFORMANCE REPORT ===\n\nMESH BREAKDOWN:\n" +
            $"Very High Poly (>5000 tris): {veryHighPoly.Count} meshes ({veryHighPoly.Sum(m => m.triangleCount):N0} total tris)\n" +
            $"High Poly (2000-5000 tris): {highPoly.Count} meshes ({highPoly.Sum(m => m.triangleCount):N0} total tris)\n" +
            $"Medium Poly (500-2000 tris): {mediumPoly.Count} meshes ({mediumPoly.Sum(m => m.triangleCount):N0} total tris)\n" +
            $"Low Poly (<500 tris): {lowPoly.Count} meshes ({lowPoly.Sum(m => m.triangleCount):N0} total tris)\n\nLOD COVERAGE:\n" +
            $"With LOD: {meshesWithLOD} meshes\nWithout LOD: {meshesWithoutLOD} meshes\nLOD Coverage: {(totalMeshes > 0 ? (meshesWithLOD / (float)totalMeshes) * 100f : 0f):F1}%\n\n" +
            $"OPTIMIZATION OPPORTUNITIES:\nMeshes needing LOD (>{minPolyCountForLOD} tris): {needsLOD.Count}\n" +
            $"Total triangles in meshes needing LOD: {totalTrianglesWithoutLOD:N0}\nPotential triangle reduction: {potentialSavings:N0} tris\n";
        if (veryHighPoly.Count > 0) { report += "\n TOP PERFORMANCE CONCERNS:\n"; var topOffenders = veryHighPoly.OrderByDescending(m => m.triangleCount).Take(5).ToList(); foreach (var mesh in topOffenders) { report += $"   {mesh.gameObject.name}: {mesh.triangleCount:N0} tris [{(mesh.hasLOD ? " Has LOD" : " NO LOD")}]\n"; } }
        report += "\nRECOMMENDATIONS:\n";
        if (needsLOD.Count > 0) report += $"1. Add LODs to {needsLOD.Count} high-poly meshes\n   Action: Click 'Select Needs LOD' then 'Auto-Generate LODs'\n\n";
        if (veryHighPoly.Any(m => !m.hasLOD)) report += $"2. CRITICAL: {veryHighPoly.Count(m => !m.hasLOD)} very high-poly meshes have no LODs\n\n";
        report += "\nTIPS FOR MOBILE:\n Use 'Aggressive (Mobile)' preset\n Focus on meshes with >1000 triangles\n Use Rendering + Occlusion Culling Managers\n";
        EditorUtility.DisplayDialog("Performance Report", report, "OK");
        //Debug.Log(report);
    }
}
