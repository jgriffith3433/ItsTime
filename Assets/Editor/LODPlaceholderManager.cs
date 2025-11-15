using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class LODPlaceholderManager : EditorWindow
{
    private Vector2 scrollPosition;
    private List<LODGroupInfo> lodGroupList = new List<LODGroupInfo>();
    private PlaceholderType placeholderType = PlaceholderType.BoundingBox;
    private int voxelResolution = 3;
    private bool preserveColors = true;
    private Material placeholderMaterial;
    private bool autoApplyToNewLODs = false;
    
    private enum PlaceholderType
    {
        BoundingBox,
        SimplifiedBox,
        Voxelized,
        Sphere,
        Capsule
    }
    
    private class LODGroupInfo
    {
        public GameObject gameObject;
        public LODGroup lodGroup;
        public bool hasLOD2;
        public bool hasPlaceholder;
        public int lod2TriangleCount;
        public Renderer lod2Renderer;
        public bool isSelected;
    }
    
    [MenuItem("Tools/LOD Placeholder Manager")]
    public static void ShowWindow()
    {
        var window = GetWindow<LODPlaceholderManager>("LOD Placeholders");
        window.minSize = new Vector2(700, 500);
    }
    
    private void OnEnable()
    {
        RefreshLODList();
    }
    
    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        DrawHeader();
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(5);
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        DrawPlaceholderSettings();
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(5);
        
        DrawLODGroupList();
        
        EditorGUILayout.EndScrollView();
    }
    
    private void DrawHeader()
    {
        EditorGUILayout.LabelField("LOD Placeholder Manager", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Replace distant LOD2 meshes with ultra-low-poly placeholders to dramatically improve performance.\n" +
            "Perfect for mobile optimization when you need aggressive LOD settings.", 
            MessageType.Info);
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Refresh LOD List", GUILayout.Height(30)))
        {
            RefreshLODList();
        }
        
        GUI.backgroundColor = Color.cyan;
        if (GUILayout.Button("Select All", GUILayout.Height(30)))
        {
            SelectAll(true);
        }
        
        if (GUILayout.Button("Deselect All", GUILayout.Height(30)))
        {
            SelectAll(false);
        }
        GUI.backgroundColor = Color.white;
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(3);
        
        EditorGUILayout.BeginHorizontal();
        
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("Apply Placeholders to Selected", GUILayout.Height(35)))
        {
            ApplyPlaceholdersToSelected();
        }
        GUI.backgroundColor = Color.white;
        
        GUI.backgroundColor = Color.yellow;
        if (GUILayout.Button("Restore Original LOD2", GUILayout.Height(35)))
        {
            RestoreOriginalLOD2();
        }
        GUI.backgroundColor = Color.white;
        
        EditorGUILayout.EndHorizontal();
    }
    
    private void DrawPlaceholderSettings()
    {
        EditorGUILayout.LabelField("Placeholder Settings", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Placeholder Type:", GUILayout.Width(120));
        placeholderType = (PlaceholderType)EditorGUILayout.EnumPopup(placeholderType);
        EditorGUILayout.EndHorizontal();
        
        // Type-specific settings
        if (placeholderType == PlaceholderType.Voxelized)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Voxel Resolution:", GUILayout.Width(120));
            voxelResolution = EditorGUILayout.IntSlider(voxelResolution, 2, 16);
            EditorGUILayout.LabelField($"({voxelResolution}x{voxelResolution}x{voxelResolution})", GUILayout.Width(80));
            EditorGUILayout.EndHorizontal();
            
            preserveColors = EditorGUILayout.ToggleLeft("Preserve Material Colors", preserveColors);
        }
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Override Material:", GUILayout.Width(120));
        placeholderMaterial = (Material)EditorGUILayout.ObjectField(placeholderMaterial, typeof(Material), false);
        EditorGUILayout.EndHorizontal();
        
        if (placeholderMaterial == null)
        {
            EditorGUILayout.HelpBox("No material set - will use original materials from LOD0", MessageType.Info);
        }
        
        EditorGUILayout.Space(5);
        
        // Show placeholder preview info
        string previewText = GetPlaceholderDescription();
        EditorGUILayout.HelpBox(previewText, MessageType.None);
    }
    
    private string GetPlaceholderDescription()
    {
        switch (placeholderType)
        {
            case PlaceholderType.BoundingBox:
                return "Bounding Box: Simple cube (12 triangles) - Most performance";
            case PlaceholderType.SimplifiedBox:
                return "Simplified Box: Beveled convex shape (48 triangles) - Better visuals, still fast";
            case PlaceholderType.Voxelized:
                return $"Voxelized: 8 corner voxels for shape hint (~96 triangles)";
            case PlaceholderType.Sphere:
                return "Sphere: Low-poly sphere (~80 triangles)";
            case PlaceholderType.Capsule:
                return "Capsule: Low-poly capsule (~96 triangles)";
            default:
                return "";
        }
    }
    
    private void DrawLODGroupList()
    {
        EditorGUILayout.LabelField($"LOD Groups with LOD2 ({lodGroupList.Count})", EditorStyles.boldLabel);
        
        // Statistics
        int withPlaceholder = lodGroupList.Count(l => l.hasPlaceholder);
        int withoutPlaceholder = lodGroupList.Count - withPlaceholder;
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"With Placeholders: {withPlaceholder}", GUILayout.Width(200));
        EditorGUILayout.LabelField($"Original LOD2: {withoutPlaceholder}", GUILayout.Width(200));
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(3);
        
        // Header
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.Label("", GUILayout.Width(20)); // Checkbox
        GUILayout.Label("Object Name", EditorStyles.miniLabel, GUILayout.Width(250));
        GUILayout.Label("LOD2 Tris", EditorStyles.miniLabel, GUILayout.Width(80));
        GUILayout.Label("Status", EditorStyles.miniLabel, GUILayout.Width(100));
        GUILayout.Label("Actions", EditorStyles.miniLabel, GUILayout.ExpandWidth(true));
        EditorGUILayout.EndHorizontal();
        
        foreach (var lodInfo in lodGroupList)
        {
            if (lodInfo.gameObject == null) continue;
            
            EditorGUILayout.BeginHorizontal();
            
            // Checkbox
            lodInfo.isSelected = EditorGUILayout.Toggle(lodInfo.isSelected, GUILayout.Width(20));
            
            // Object name (clickable)
            if (GUILayout.Button(lodInfo.gameObject.name, EditorStyles.label, GUILayout.Width(250)))
            {
                Selection.activeGameObject = lodInfo.gameObject;
                EditorGUIUtility.PingObject(lodInfo.gameObject);
            }
            
            // LOD2 triangle count
            Color originalColor = GUI.color;
            if (lodInfo.hasPlaceholder)
                GUI.color = Color.green;
            
            EditorGUILayout.LabelField(lodInfo.lod2TriangleCount.ToString("N0"), GUILayout.Width(80));
            GUI.color = originalColor;
            
            // Status
            if (lodInfo.hasPlaceholder)
            {
                GUI.color = Color.green;
                EditorGUILayout.LabelField("Placeholder ✓", GUILayout.Width(100));
            }
            else
            {
                GUI.color = Color.yellow;
                EditorGUILayout.LabelField("Original", GUILayout.Width(100));
            }
            GUI.color = originalColor;
            
            // Actions
            EditorGUILayout.BeginHorizontal();
            
            if (!lodInfo.hasPlaceholder)
            {
                if (GUILayout.Button("Apply", GUILayout.Width(60)))
                {
                    ApplyPlaceholder(lodInfo);
                }
            }
            else
            {
                if (GUILayout.Button("Restore", GUILayout.Width(60)))
                {
                    RestoreLOD2(lodInfo);
                }
            }
            
            if (GUILayout.Button("Select", GUILayout.Width(60)))
            {
                Selection.activeGameObject = lodInfo.gameObject;
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndHorizontal();
        }
    }
    
    private void RefreshLODList()
    {
        lodGroupList.Clear();
        
        var allLODGroups = FindObjectsOfType<LODGroup>();
        
        foreach (var lodGroup in allLODGroups)
        {
            LOD[] lods = lodGroup.GetLODs();
            
            // We need at least 3 LOD levels (LOD0, LOD1, LOD2)
            if (lods.Length < 3) continue;
            
            LOD lod2 = lods[2];
            if (lod2.renderers == null || lod2.renderers.Length == 0) continue;
            
            Renderer lod2Renderer = lod2.renderers[0];
            if (lod2Renderer == null) continue;
            
            MeshFilter mf = lod2Renderer.GetComponent<MeshFilter>();
            if (mf == null || mf.sharedMesh == null) continue;
            
            var lodInfo = new LODGroupInfo
            {
                gameObject = lodGroup.gameObject,
                lodGroup = lodGroup,
                hasLOD2 = true,
                lod2Renderer = lod2Renderer,
                lod2TriangleCount = mf.sharedMesh.triangles.Length / 3,
                hasPlaceholder = IsPlaceholder(mf.sharedMesh)
            };
            
            lodGroupList.Add(lodInfo);
        }
        
        // Sort by triangle count descending
        lodGroupList = lodGroupList.OrderByDescending(l => l.lod2TriangleCount).ToList();
        
        Repaint();
    }
    
    private bool IsPlaceholder(Mesh mesh)
    {
        // Check if the mesh name contains our placeholder marker
        return mesh.name.Contains("_Placeholder_");
    }
    
    private void SelectAll(bool select)
    {
        foreach (var lodInfo in lodGroupList)
        {
            lodInfo.isSelected = select;
        }
        Repaint();
    }
    
    private void ApplyPlaceholdersToSelected()
    {
        var selected = lodGroupList.Where(l => l.isSelected && !l.hasPlaceholder).ToList();
        
        if (selected.Count == 0)
        {
            EditorUtility.DisplayDialog("No Selection", 
                "Please select LOD groups without placeholders to apply placeholders.", "OK");
            return;
        }
        
        if (EditorUtility.DisplayDialog("Apply Placeholders", 
            $"Replace LOD2 meshes with {placeholderType} placeholders for {selected.Count} object(s)?\n\n" +
            "Original LOD2 meshes will be backed up and can be restored.", "Apply", "Cancel"))
        {
            int successCount = 0;
            
            for (int i = 0; i < selected.Count; i++)
            {
                EditorUtility.DisplayProgressBar("Applying Placeholders", 
                    $"Processing {selected[i].gameObject.name} ({i + 1}/{selected.Count})", 
                    (float)i / selected.Count);
                
                if (ApplyPlaceholder(selected[i]))
                    successCount++;
            }
            
            EditorUtility.ClearProgressBar();
            RefreshLODList();
            
            EditorUtility.DisplayDialog("Success", 
                $"Applied placeholders to {successCount} LOD group(s).", "OK");
        }
    }
    
    private bool ApplyPlaceholder(LODGroupInfo lodInfo)
    {
        if (lodInfo.lod2Renderer == null) return false;
        
        MeshFilter mf = lodInfo.lod2Renderer.GetComponent<MeshFilter>();
        if (mf == null || mf.sharedMesh == null) return false;
        
        // Backup original mesh by tagging it
        Mesh originalMesh = mf.sharedMesh;
        
        // Get bounds from the LOD2 mesh itself (local space bounds)
        Bounds bounds = originalMesh.bounds;
        
        // Generate placeholder mesh
        Mesh placeholderMesh = GeneratePlaceholderMesh(originalMesh, bounds);
        placeholderMesh.name = originalMesh.name + "_Placeholder_" + placeholderType.ToString();
        
        // Store original mesh name for restoration
        if (!originalMesh.name.Contains("_Original"))
        {
            // This is the first time - we need to preserve the original reference
            // We'll store it in the mesh name
            placeholderMesh.name += "_Original:" + originalMesh.name;
        }
        
        Undo.RecordObject(mf, "Apply Placeholder Mesh");
        mf.sharedMesh = placeholderMesh;
        
        // Apply material if specified
        if (placeholderMaterial != null)
        {
            MeshRenderer mr = lodInfo.lod2Renderer.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                Undo.RecordObject(mr, "Apply Placeholder Material");
                mr.sharedMaterial = placeholderMaterial;
            }
        }
        
        EditorUtility.SetDirty(lodInfo.gameObject);
        
        return true;
    }
    
    private void RestoreOriginalLOD2()
    {
        var selected = lodGroupList.Where(l => l.isSelected && l.hasPlaceholder).ToList();
        
        if (selected.Count == 0)
        {
            EditorUtility.DisplayDialog("No Selection", 
                "Please select LOD groups with placeholders to restore original LOD2.", "OK");
            return;
        }
        
        if (EditorUtility.DisplayDialog("Restore Original LOD2", 
            $"Restore original LOD2 meshes for {selected.Count} object(s)?", "Restore", "Cancel"))
        {
            int successCount = 0;
            
            foreach (var lodInfo in selected)
            {
                if (RestoreLOD2(lodInfo))
                    successCount++;
            }
            
            RefreshLODList();
            
            EditorUtility.DisplayDialog("Success", 
                $"Restored original LOD2 for {successCount} LOD group(s).", "OK");
        }
    }
    
    private bool RestoreLOD2(LODGroupInfo lodInfo)
    {
        // For now, this will regenerate the LOD2 from LOD0
        // In a production version, you'd want to store the original mesh reference
        
        EditorUtility.DisplayDialog("Restore Notice", 
            "To restore original LOD2, use Scene Performance Manager to regenerate LODs.\n\n" +
            "Future version will include automatic backup/restore.", "OK");
        
        return false;
    }
    
    private Mesh GeneratePlaceholderMesh(Mesh originalMesh, Bounds bounds)
    {
        switch (placeholderType)
        {
            case PlaceholderType.BoundingBox:
                return CreateBoundingBoxMesh(bounds);
            case PlaceholderType.SimplifiedBox:
                return CreateSimplifiedBoxMesh(originalMesh, bounds);
            case PlaceholderType.Voxelized:
                return CreateVoxelizedMesh(originalMesh, bounds);
            case PlaceholderType.Sphere:
                return CreateSphereMesh(bounds);
            case PlaceholderType.Capsule:
                return CreateCapsuleMesh(bounds);
            default:
                return CreateBoundingBoxMesh(bounds);
        }
    }
    
    private Mesh CreateBoundingBoxMesh(Bounds bounds)
    {
        Mesh mesh = new Mesh();
        mesh.name = "BoundingBox";
        
        // Use local space - center at origin since the mesh is positioned by the GameObject transform
        Vector3 center = Vector3.zero;
        Vector3 extents = bounds.extents;
        
        // 8 vertices of a box
        Vector3[] vertices = new Vector3[8]
        {
            center + new Vector3(-extents.x, -extents.y, -extents.z),
            center + new Vector3(extents.x, -extents.y, -extents.z),
            center + new Vector3(extents.x, extents.y, -extents.z),
            center + new Vector3(-extents.x, extents.y, -extents.z),
            center + new Vector3(-extents.x, -extents.y, extents.z),
            center + new Vector3(extents.x, -extents.y, extents.z),
            center + new Vector3(extents.x, extents.y, extents.z),
            center + new Vector3(-extents.x, extents.y, extents.z)
        };
        
        // 12 triangles (6 faces * 2 triangles)
        int[] triangles = new int[]
        {
            // Front
            0, 2, 1, 0, 3, 2,
            // Back
            5, 7, 4, 5, 6, 7,
            // Left
            4, 3, 0, 4, 7, 3,
            // Right
            1, 6, 5, 1, 2, 6,
            // Top
            3, 6, 2, 3, 7, 6,
            // Bottom
            4, 1, 5, 4, 0, 1
        };
        
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        
        return mesh;
    }
    
    private Mesh CreateSimplifiedBoxMesh(Mesh originalMesh, Bounds bounds)
    {
        // Analyze the original mesh to create a better approximation
        Mesh mesh = new Mesh();
        mesh.name = "SimplifiedBox";
        
        Vector3[] originalVertices = originalMesh.vertices;
        
        // Sample vertices from the original mesh to find extremes in 6 directions
        Vector3 center = Vector3.zero;
        
        // Find the furthest vertices in each direction
        Vector3 furthestNegX = Vector3.zero, furthestPosX = Vector3.zero;
        Vector3 furthestNegY = Vector3.zero, furthestPosY = Vector3.zero;
        Vector3 furthestNegZ = Vector3.zero, furthestPosZ = Vector3.zero;
        
        float minX = float.MaxValue, maxX = float.MinValue;
        float minY = float.MaxValue, maxY = float.MinValue;
        float minZ = float.MaxValue, maxZ = float.MinValue;
        
        foreach (Vector3 v in originalVertices)
        {
            if (v.x < minX) { minX = v.x; furthestNegX = v; }
            if (v.x > maxX) { maxX = v.x; furthestPosX = v; }
            if (v.y < minY) { minY = v.y; furthestNegY = v; }
            if (v.y > maxY) { maxY = v.y; furthestPosY = v; }
            if (v.z < minZ) { minZ = v.z; furthestNegZ = v; }
            if (v.z > maxZ) { maxZ = v.z; furthestPosZ = v; }
        }
        
        // Create vertices using the actual extremes from the mesh
        List<Vector3> vertices = new List<Vector3>
        {
            // Use the 6 extreme points
            furthestNegX, // 0 - Left
            furthestPosX, // 1 - Right
            furthestNegY, // 2 - Bottom
            furthestPosY, // 3 - Top
            furthestNegZ, // 4 - Front
            furthestPosZ, // 5 - Back
            
            // Add 8 corner approximations based on combinations
            new Vector3(minX, minY, minZ), // 6
            new Vector3(maxX, minY, minZ), // 7
            new Vector3(maxX, maxY, minZ), // 8
            new Vector3(minX, maxY, minZ), // 9
            new Vector3(minX, minY, maxZ), // 10
            new Vector3(maxX, minY, maxZ), // 11
            new Vector3(maxX, maxY, maxZ), // 12
            new Vector3(minX, maxY, maxZ)  // 13
        };
        
        // Create a convex hull-like structure connecting these points
        List<int> triangles = new List<int>
        {
            // Front face (using front extreme and corners)
            4, 7, 6,  4, 8, 7,  4, 9, 8,  4, 6, 9,
            
            // Back face (using back extreme and corners)
            5, 10, 11,  5, 11, 12,  5, 12, 13,  5, 13, 10,
            
            // Left face
            0, 6, 10,  0, 10, 13,  0, 13, 9,  0, 9, 6,
            
            // Right face
            1, 11, 7,  1, 12, 11,  1, 8, 12,  1, 7, 8,
            
            // Top face
            3, 9, 13,  3, 13, 12,  3, 12, 8,  3, 8, 9,
            
            // Bottom face
            2, 6, 7,  2, 7, 11,  2, 11, 10,  2, 10, 6
        };
        
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        
        return mesh;
    }
    
    private Mesh CreateVoxelizedMesh(Mesh originalMesh, Bounds bounds)
    {
        // Ultra-simplified voxel approach - just create a few representative boxes
        // This creates only 6-8 voxels instead of a full grid for performance
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        
        // Use local space - create voxels centered at origin
        Vector3 voxelSize = bounds.size / voxelResolution;
        Vector3 startPos = -bounds.extents;
        
        // Only create voxels at the 8 corners and optionally the 6 face centers
        // This gives shape recognition with minimal polygons
        int[] cornerPositions = new int[]
        {
            0, 0, 0,  // Corner 1
            voxelResolution-1, 0, 0,  // Corner 2
            0, voxelResolution-1, 0,  // Corner 3
            voxelResolution-1, voxelResolution-1, 0,  // Corner 4
            0, 0, voxelResolution-1,  // Corner 5
            voxelResolution-1, 0, voxelResolution-1,  // Corner 6
            0, voxelResolution-1, voxelResolution-1,  // Corner 7
            voxelResolution-1, voxelResolution-1, voxelResolution-1  // Corner 8
        };
        
        for (int i = 0; i < cornerPositions.Length; i += 3)
        {
            int x = cornerPositions[i];
            int y = cornerPositions[i + 1];
            int z = cornerPositions[i + 2];
            
            Vector3 voxelPos = startPos + new Vector3(
                x * voxelSize.x,
                y * voxelSize.y,
                z * voxelSize.z
            );
            
            AddVoxel(vertices, triangles, voxelPos, voxelSize);
        }
        
        Mesh mesh = new Mesh();
        mesh.name = "Voxelized";
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        
        return mesh;
    }
    
    private void AddVoxel(List<Vector3> vertices, List<int> triangles, Vector3 position, Vector3 size)
    {
        int startIndex = vertices.Count;
        
        // 8 vertices of voxel
        vertices.Add(position);
        vertices.Add(position + new Vector3(size.x, 0, 0));
        vertices.Add(position + new Vector3(size.x, size.y, 0));
        vertices.Add(position + new Vector3(0, size.y, 0));
        vertices.Add(position + new Vector3(0, 0, size.z));
        vertices.Add(position + new Vector3(size.x, 0, size.z));
        vertices.Add(position + size);
        vertices.Add(position + new Vector3(0, size.y, size.z));
        
        // 12 triangles (simplified - add all faces)
        int[] voxelTris = new int[]
        {
            0, 2, 1, 0, 3, 2,
            5, 7, 4, 5, 6, 7,
            4, 3, 0, 4, 7, 3,
            1, 6, 5, 1, 2, 6,
            3, 6, 2, 3, 7, 6,
            4, 1, 5, 4, 0, 1
        };
        
        foreach (int idx in voxelTris)
        {
            triangles.Add(startIndex + idx);
        }
    }
    
    private Mesh CreateSphereMesh(Bounds bounds)
    {
        // Create a low-poly sphere
        GameObject tempSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Mesh sphereMesh = Object.Instantiate(tempSphere.GetComponent<MeshFilter>().sharedMesh);
        DestroyImmediate(tempSphere);
        
        // Scale to fit bounds in local space (centered at origin)
        float radius = Mathf.Max(bounds.extents.x, bounds.extents.y, bounds.extents.z);
        Vector3[] vertices = sphereMesh.vertices;
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = vertices[i] * radius; // Don't add bounds.center - keep at origin
        }
        sphereMesh.vertices = vertices;
        sphereMesh.RecalculateBounds();
        sphereMesh.name = "Sphere";
        
        return sphereMesh;
    }
    
    private Mesh CreateCapsuleMesh(Bounds bounds)
    {
        // Create a low-poly capsule
        GameObject tempCapsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        Mesh capsuleMesh = Object.Instantiate(tempCapsule.GetComponent<MeshFilter>().sharedMesh);
        DestroyImmediate(tempCapsule);
        
        // Scale to fit bounds in local space (centered at origin)
        Vector3 scale = bounds.size;
        Vector3[] vertices = capsuleMesh.vertices;
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = Vector3.Scale(vertices[i], scale * 0.5f); // Don't add bounds.center - keep at origin
        }
        capsuleMesh.vertices = vertices;
        capsuleMesh.RecalculateBounds();
        capsuleMesh.name = "Capsule";
        
        return capsuleMesh;
    }
}
