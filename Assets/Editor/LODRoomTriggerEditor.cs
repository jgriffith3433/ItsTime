using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class LODRoomTriggerEditor : EditorWindow
{
    private Vector2 scrollPosition;
    private LODRoomTrigger[] allTriggers;
    private DynamicLOD[] allDynamicLODs;
    private int totalLODCount;
    private int coveredLODCount;
    private HashSet<DynamicLOD> coveredLODs;
    
    [MenuItem("Tools/LOD Room Trigger Manager")]
    public static void ShowWindow()
    {
        LODRoomTriggerEditor window = GetWindow<LODRoomTriggerEditor>("LOD Coverage Manager");
        window.minSize = new Vector2(400, 300);
        window.Show();
    }
    
    private void OnEnable()
    {
        RefreshData();
    }
    
    private void OnGUI()
    {
        EditorGUILayout.Space(10);
        
        // Header
        EditorGUILayout.LabelField("LOD Room Trigger Coverage Manager", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        
        // Refresh button
        if (GUILayout.Button("Refresh Data", GUILayout.Height(30)))
        {
            RefreshData();
        }
        
        EditorGUILayout.Space(10);
        
        // Coverage statistics
        DrawCoverageStats();
        
        EditorGUILayout.Space(10);
        
        // Update all button
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("Update All Triggers (Find LODs Inside Colliders)", GUILayout.Height(40)))
        {
            UpdateAllTriggers();
        }
        GUI.backgroundColor = Color.white;
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("LOD Room Triggers in Scene", EditorStyles.boldLabel);
        
        // Scrollable list of triggers
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        if (allTriggers != null && allTriggers.Length > 0)
        {
            foreach (var trigger in allTriggers)
            {
                if (trigger == null) continue;
                
                DrawTriggerItem(trigger);
            }
        }
        else
        {
            EditorGUILayout.HelpBox("No LODRoomTrigger components found in the scene.", MessageType.Info);
        }
        
        EditorGUILayout.EndScrollView();
        
        // Uncovered LODs section
        EditorGUILayout.Space(10);
        DrawUncoveredLODs();
    }
    
    private void RefreshData()
    {
        allTriggers = FindObjectsOfType<LODRoomTrigger>();
        allDynamicLODs = FindObjectsOfType<DynamicLOD>();
        totalLODCount = allDynamicLODs.Length;
        
        // Calculate covered LODs
        coveredLODs = new HashSet<DynamicLOD>();
        foreach (var trigger in allTriggers)
        {
            if (trigger.DynamicLODs != null)
            {
                foreach (var lod in trigger.DynamicLODs)
                {
                    if (lod != null)
                    {
                        coveredLODs.Add(lod);
                    }
                }
            }
        }
        
        coveredLODCount = coveredLODs.Count;
        
        Repaint();
    }
    
    private void DrawCoverageStats()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        EditorGUILayout.LabelField("Coverage Statistics", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Total DynamicLOD Components:", GUILayout.Width(200));
        EditorGUILayout.LabelField(totalLODCount.ToString(), EditorStyles.boldLabel);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Covered by Triggers:", GUILayout.Width(200));
        GUI.color = coveredLODCount > 0 ? Color.green : Color.red;
        EditorGUILayout.LabelField(coveredLODCount.ToString(), EditorStyles.boldLabel);
        GUI.color = Color.white;
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Uncovered:", GUILayout.Width(200));
        int uncovered = totalLODCount - coveredLODCount;
        GUI.color = uncovered == 0 ? Color.green : Color.yellow;
        EditorGUILayout.LabelField(uncovered.ToString(), EditorStyles.boldLabel);
        GUI.color = Color.white;
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Coverage Percentage:", GUILayout.Width(200));
        float percentage = totalLODCount > 0 ? (float)coveredLODCount / totalLODCount * 100f : 0f;
        GUI.color = percentage >= 100f ? Color.green : percentage >= 75f ? Color.yellow : Color.red;
        EditorGUILayout.LabelField($"{percentage:F1}%", EditorStyles.boldLabel);
        GUI.color = Color.white;
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Total Triggers:", GUILayout.Width(200));
        EditorGUILayout.LabelField(allTriggers != null ? allTriggers.Length.ToString() : "0", EditorStyles.boldLabel);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawTriggerItem(LODRoomTrigger trigger)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        EditorGUILayout.BeginHorizontal();
        
        // Trigger name and select button
        EditorGUILayout.LabelField(trigger.gameObject.name, EditorStyles.boldLabel, GUILayout.Width(200));
        
        int lodCount = trigger.DynamicLODs != null ? trigger.DynamicLODs.Length : 0;
        EditorGUILayout.LabelField($"LODs: {lodCount}", GUILayout.Width(80));
        
        if (GUILayout.Button("Select", GUILayout.Width(60)))
        {
            Selection.activeGameObject = trigger.gameObject;
            SceneView.FrameLastActiveSceneView();
        }
        
        if (GUILayout.Button("Update", GUILayout.Width(60)))
        {
            Undo.RecordObject(trigger, "Update LODRoomTrigger");
            trigger.FindDynamicLODsInsideBoxCollider();
            EditorUtility.SetDirty(trigger);
            RefreshData();
        }
        
        EditorGUILayout.EndHorizontal();
        
        // Show collider type
        Collider col = trigger.GetComponent<Collider>();
        if (col != null)
        {
            string colliderType = col.GetType().Name;
            EditorGUILayout.LabelField($"Collider: {colliderType}", EditorStyles.miniLabel);
        }
        
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(2);
    }
    
    private void DrawUncoveredLODs()
    {
        int uncoveredCount = totalLODCount - coveredLODCount;
        
        if (uncoveredCount > 0)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            bool foldout = EditorPrefs.GetBool("LODCoverageManager_ShowUncovered", false);
            foldout = EditorGUILayout.Foldout(foldout, $"Uncovered DynamicLODs ({uncoveredCount})", true, EditorStyles.foldoutHeader);
            EditorPrefs.SetBool("LODCoverageManager_ShowUncovered", foldout);
            
            if (foldout)
            {
                EditorGUILayout.Space(5);
                
                if (GUILayout.Button("Select All Uncovered LODs"))
                {
                    List<GameObject> uncoveredObjects = new List<GameObject>();
                    foreach (var lod in allDynamicLODs)
                    {
                        if (lod != null && !coveredLODs.Contains(lod))
                        {
                            uncoveredObjects.Add(lod.gameObject);
                        }
                    }
                    Selection.objects = uncoveredObjects.ToArray();
                }
                
                EditorGUILayout.Space(5);
                
                foreach (var lod in allDynamicLODs)
                {
                    if (lod != null && !coveredLODs.Contains(lod))
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(lod.gameObject.name, GUILayout.Width(250));
                        
                        if (GUILayout.Button("Select", GUILayout.Width(60)))
                        {
                            Selection.activeGameObject = lod.gameObject;
                            SceneView.FrameLastActiveSceneView();
                        }
                        
                        EditorGUILayout.EndHorizontal();
                    }
                }
            }
            
            EditorGUILayout.EndVertical();
        }
    }
    
    private void UpdateAllTriggers()
    {
        if (allTriggers == null || allTriggers.Length == 0)
        {
            EditorUtility.DisplayDialog("No Triggers Found", "No LODRoomTrigger components found in the scene.", "OK");
            return;
        }
        
        int updatedCount = 0;
        
        foreach (var trigger in allTriggers)
        {
            if (trigger != null)
            {
                Undo.RecordObject(trigger, "Update All LODRoomTriggers");
                trigger.FindDynamicLODsInsideBoxCollider();
                EditorUtility.SetDirty(trigger);
                updatedCount++;
            }
        }
        
        RefreshData();
        
        EditorUtility.DisplayDialog(
            "Update Complete", 
            $"Updated {updatedCount} LODRoomTrigger component(s).\n\n" +
            $"Total LODs: {totalLODCount}\n" +
            $"Covered: {coveredLODCount}\n" +
            $"Coverage: {(totalLODCount > 0 ? (float)coveredLODCount / totalLODCount * 100f : 0f):F1}%",
            "OK"
        );
    }
}