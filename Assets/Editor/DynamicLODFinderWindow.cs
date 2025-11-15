using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class DynamicLODFinderWindow : EditorWindow
{
    [SerializeField]
    private GameObject targetObject;
    
    [SerializeField]
    private GameObject[] foundDynamicLODs = new GameObject[0];
    
    private SerializedObject serializedObject;
    private SerializedProperty targetObjectProperty;
    private SerializedProperty foundDynamicLODsProperty;
    
    [MenuItem("Tools/DynamicLOD Finder")]
    public static void ShowWindow()
    {
        GetWindow<DynamicLODFinderWindow>("DynamicLOD Finder");
    }
    
    private void OnEnable()
    {
        serializedObject = new SerializedObject(this);
        targetObjectProperty = serializedObject.FindProperty("targetObject");
        foundDynamicLODsProperty = serializedObject.FindProperty("foundDynamicLODs");
    }
    
    private void OnGUI()
    {
        serializedObject.Update();
        
        EditorGUILayout.LabelField("DynamicLOD Finder", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(targetObjectProperty, new GUIContent("Target GameObject"));
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            if (targetObject != null)
            {
                FindDynamicLODs();
            }
            else
            {
                foundDynamicLODs = new GameObject[0];
            }
        }
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Search for DynamicLOD Components"))
        {
            if (targetObject != null)
            {
                FindDynamicLODs();
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Please assign a target GameObject first.", "OK");
            }
        }
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField($"Found {foundDynamicLODs.Length} DynamicLOD Components:", EditorStyles.boldLabel);
        
        EditorGUILayout.PropertyField(foundDynamicLODsProperty, new GUIContent("DynamicLOD Components"), true);
        
        serializedObject.ApplyModifiedProperties();
    }
    
    private void FindDynamicLODs()
    {
        if (targetObject == null)
            return;
        
        List<GameObject> dynamicLODs = new List<GameObject>();
        
        // Get DynamicLOD on the target object itself
        DynamicLOD dynamicLOD = targetObject.GetComponent<DynamicLOD>();
        if (dynamicLOD != null)
        {
            dynamicLODs.Add(dynamicLOD.gameObject);
        }
        
        // Get all DynamicLOD components in children
        DynamicLOD[] childDynamicLODs = targetObject.GetComponentsInChildren<DynamicLOD>();
        foreach (DynamicLOD childDynamicLOD in childDynamicLODs)
        {
            if (!dynamicLODs.Contains(childDynamicLOD.gameObject))
            {
                dynamicLODs.Add(childDynamicLOD.gameObject);
            }
        }
        
        foundDynamicLODs = dynamicLODs.ToArray();
        
        serializedObject.Update();
        serializedObject.ApplyModifiedProperties();
        
        Debug.Log($"Found {foundDynamicLODs.Length} DynamicLOD component(s) in '{targetObject.name}' and its children.");
    }
}
