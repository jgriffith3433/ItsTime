using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DynamicLODManager))]
public class DynamicLODManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DynamicLODManager manager = (DynamicLODManager)target;
        
        DrawDefaultInspector();
        
        EditorGUILayout.Space(10);
        
        EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);
        
        GUI.backgroundColor = Color.cyan;
        if (GUILayout.Button("Scan Scene for DynamicLOD Components", GUILayout.Height(35)))
        {
            manager.ScanSceneForLODGroups();
        }
        GUI.backgroundColor = Color.white;
        
        EditorGUILayout.Space(5);
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Reset to Default", GUILayout.Height(30)))
        {
            manager.ResetAllLODGroups();
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(10);
        
        EditorGUILayout.HelpBox(
            "1. Use Scene Performance Manager to add LODGroups (automatically adds DynamicLOD components)\n" +
            "2. Click 'Scan Scene for DynamicLOD Components' to find all DynamicLOD components with enableDynamic=true\n" +
            "3. Set your Target FPS (e.g., 30 for mobile)\n" +
            "4. Enter Play Mode - the system will automatically adjust LOD distances to maintain FPS\n\n" +
            "The system works in both directions:\n" +
            "• If FPS drops below target → Decreases quality (more aggressive LODs)\n" +
            "• If FPS is above target → Increases quality (less aggressive LODs)\n\n" +
            "Only LODGroups with DynamicLOD components and enableDynamic=true will be adjusted.",
            MessageType.Info);
    }
}

