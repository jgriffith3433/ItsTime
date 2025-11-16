using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class LODGroupManagerWindow : EditorWindow
{
    [SerializeField]
    private GameObject parentGameObject;

    private Vector2 scrollPosition;
    private List<LODGroup> foundLODGroups = new List<LODGroup>();

    [MenuItem("Window/LOD Group Manager")]
    public static void ShowWindow()
    {
        GetWindow<LODGroupManagerWindow>("LOD Group Manager");
    }

    private void OnGUI()
    {
        GUILayout.Label("LOD Group Manager", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // Parent GameObject field
        parentGameObject = (GameObject)EditorGUILayout.ObjectField(
            "Parent GameObject",
            parentGameObject,
            typeof(GameObject),
            true
        );

        EditorGUILayout.Space();

        // Scan button
        if (GUILayout.Button("Scan for LODGroups Not in Parent", GUILayout.Height(30)))
        {
            ScanForLODGroups();
        }

        EditorGUILayout.Space();

        // Display found LODGroups
        if (foundLODGroups.Count > 0)
        {
            GUILayout.Label($"Found {foundLODGroups.Count} LODGroup(s) not in parent:", EditorStyles.boldLabel);
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));
            foreach (var lodGroup in foundLODGroups)
            {
                if (lodGroup != null)
                {
                    EditorGUILayout.ObjectField(lodGroup.name, lodGroup, typeof(LODGroup), true);
                }
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();

            if (GUILayout.Button("Process LODGroups", GUILayout.Height(30)))
            {
                ProcessLODGroups();
            }
        }
    }

    private void ScanForLODGroups()
    {
        foundLODGroups.Clear();

        if (parentGameObject == null)
        {
            EditorUtility.DisplayDialog("Error", "Please assign a Parent GameObject first.", "OK");
            return;
        }

        // Find all LODGroups in the scene
        LODGroup[] allLODGroups = FindObjectsOfType<LODGroup>();

        foreach (var lodGroup in allLODGroups)
        {
            // Check if this LODGroup is not a child of the parent GameObject
            if (!IsChildOf(lodGroup.transform, parentGameObject.transform))
            {
                foundLODGroups.Add(lodGroup);
            }
        }

        Debug.Log($"Found {foundLODGroups.Count} LODGroup(s) not in parent.");
    }

    private bool IsChildOf(Transform child, Transform parent)
    {
        Transform current = child;
        while (current != null)
        {
            if (current == parent)
                return true;
            current = current.parent;
        }
        return false;
    }

    private void ProcessLODGroups()
    {
        if (parentGameObject == null)
        {
            EditorUtility.DisplayDialog("Error", "Please assign a Parent GameObject first.", "OK");
            return;
        }

        if (foundLODGroups.Count == 0)
        {
            EditorUtility.DisplayDialog("Info", "No LODGroups to process.", "OK");
            return;
        }

        Undo.IncrementCurrentGroup();
        int undoGroup = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Process LODGroups");

        // Create a parent GameObject for all collider duplicates
        GameObject collidersParent = new GameObject("LOD_Colliders");
        Undo.RegisterCreatedObjectUndo(collidersParent, "Create Colliders Parent");

        int processedCount = 0;

        foreach (var lodGroup in foundLODGroups.ToList())
        {
            if (lodGroup == null)
                continue;

            GameObject original = lodGroup.gameObject;

            // Unpack prefab if needed (must use root)
            if (PrefabUtility.IsPartOfPrefabInstance(original))
            {
                GameObject prefabRoot = PrefabUtility.GetOutermostPrefabInstanceRoot(original);
                if (prefabRoot != null)
                {
                    PrefabUtility.UnpackPrefabInstance(prefabRoot, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                }
            }

            // Step 1: Create duplicate at the same world position
            GameObject duplicate = Instantiate(original);
            duplicate.name = original.name + "_Collider";
            duplicate.transform.position = original.transform.position;
            duplicate.transform.rotation = original.transform.rotation;
            duplicate.transform.localScale = original.transform.localScale;
            
            // Parent to the colliders parent object
            duplicate.transform.SetParent(collidersParent.transform);
            
            Undo.RegisterCreatedObjectUndo(duplicate, "Create Collider Duplicate");
            
            Debug.Log($"Created duplicate: {duplicate.name} at position {duplicate.transform.position}");

            // Step 2: Remove all components from duplicate except MeshCollider and Transform
            // Need to remove in reverse dependency order
            // Process all transforms in duplicate hierarchy
            foreach (Transform childTransform in duplicate.GetComponentsInChildren<Transform>(true))
            {
                Component[] components = childTransform.GetComponents<Component>();
                
                // First pass: collect components to remove (except Transform and MeshCollider)
                List<Component> componentsToRemove = new List<Component>();
                foreach (var component in components)
                {
                    if (component is Transform || component is MeshCollider)
                        continue;
                    componentsToRemove.Add(component);
                }
                
                // Remove non-LODGroup components first
                foreach (var component in componentsToRemove)
                {
                    if (!(component is LODGroup))
                    {
                        DestroyImmediate(component);
                    }
                }
                
                // Then remove LODGroup components
                foreach (var component in componentsToRemove)
                {
                    if (component is LODGroup)
                    {
                        DestroyImmediate(component);
                    }
                }
            }

            // Step 3: Child original to parent (without moving - maintain world position)
            Transform originalTransform = original.transform;
            Vector3 worldPos = originalTransform.position;
            Quaternion worldRot = originalTransform.rotation;
            Vector3 worldScale = originalTransform.lossyScale;
            
            originalTransform.SetParent(parentGameObject.transform);
            
            // Restore world position and rotation
            originalTransform.position = worldPos;
            originalTransform.rotation = worldRot;
            
            // Adjust local scale to maintain world scale
            Vector3 parentScale = parentGameObject.transform.lossyScale;
            originalTransform.localScale = new Vector3(
                worldScale.x / parentScale.x,
                worldScale.y / parentScale.y,
                worldScale.z / parentScale.z
            );
            
            Undo.RegisterCompleteObjectUndo(originalTransform, "Parent LODGroup");

            // Step 4: Remove MeshCollider from original and its children
            MeshCollider[] originalMeshColliders = original.GetComponentsInChildren<MeshCollider>(true);
            foreach (var meshCollider in originalMeshColliders)
            {
                DestroyImmediate(meshCollider);
            }

            processedCount++;
        }

        Undo.CollapseUndoOperations(undoGroup);

        foundLODGroups.Clear();
        Debug.Log($"Processed {processedCount} LODGroup(s).");
        EditorUtility.DisplayDialog("Success", $"Processed {processedCount} LODGroup(s).", "OK");
    }
}
