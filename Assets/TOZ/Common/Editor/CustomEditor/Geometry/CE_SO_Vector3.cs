using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif
using TOZ;

namespace TOZEditor {

	[CustomEditor(typeof(SO_Vector3))]
	public sealed class CE_SO_Vector3 : Editor {
#region Variables
		//Variables
		private SO_Vector3 obj;

		//GUI Variables
		private float gui_CircleRadius;
		private int gui_CircleSegments;
		private string gui_TglEditInSceneName;
		private bool gui_EditInScene;
		private bool gui_TglRoundToInt;
		private Vector3 gui_Offset;
		private Vector2 gui_ScrollPos;
		private float gui_Columns;
		private Color gui_Color;
		private CompareFunction gui_CompareFunction;
#endregion Variables

#region Mono Methods
		private void OnEnable() {
			DoEnable();
		}

		private void OnDisable() {
			DoDisable();
		}

		public override void OnInspectorGUI() {
			DoInspectorGUI();
		}

		private void OnSceneGUI(SceneView sceneView) {
			DoSceneGUI(sceneView);
		}
#endregion Mono Methods

#region Methods
		private void DoEnable() {
			obj = (SO_Vector3)target;
			InitializeGUIVariables();

			SceneView.duringSceneGui += OnSceneGUI;
		}

		private void DoDisable() {
			SceneView.duringSceneGui -= OnSceneGUI;
		}

		private void DoInspectorGUI() {
#region Create Circle
			EditorGUILayout.BeginHorizontal("box");
			EditorGUILayout.BeginVertical();
			gui_CircleRadius = EditorGUILayout.FloatField("Circle Radius: ", gui_CircleRadius);
			gui_CircleSegments = _MathUtil._Clamp(EditorGUILayout.IntField("Circle Segments: ", gui_CircleSegments), 3, 360);
			EditorGUILayout.EndVertical();
			if(GUILayout.Button("CREATE CIRCLE", GUILayout.ExpandHeight(true))) {
				obj.Clear();
				float c = 360.0f / gui_CircleSegments;
				for(float i = 0; i <= 359f; i += c) {
					float sin = Mathf.Sin((i+180f) * Mathf.Deg2Rad) * gui_CircleRadius;
					float cos = -Mathf.Cos((i+180f) * Mathf.Deg2Rad) * gui_CircleRadius;
					obj.Add(new Vector3(sin, cos, 0f));
				}
			}
			EditorGUILayout.EndHorizontal();
#endregion Create Circle
			EditorGUILayout.Space();
#region Top Buttons
			EditorGUILayout.BeginHorizontal("box");
			EditorGUILayout.Space();
			if(GUILayout.Button("ADD NEW", GUILayout.Width(80))) {
				obj.Add(default); // default is literal, no need for default(Vector3)
			}
			if(GUILayout.Button("CLEAR", GUILayout.Width(80))) {
				obj.Clear();
			}
			if(GUILayout.Button("NORMALIZE", GUILayout.Width(80))) {
				Vector3 center = _GeomUtil._CalculateCenteroid(obj.Array());
				float length = 1f;
				for(int i = 0; i < obj.Count(); i++) {
					float d = Mathf.Abs((obj[i] - center).magnitude) * 2f;
					if(d > length) {
						length = d;
					}
				}
				for(int i = 0; i < obj.Count(); i++) {
					obj[i] = (obj[i] - center) / length;
				}
			}
			if(GUILayout.Button("CENTER", GUILayout.Width(80))) {
				Vector3 center = _GeomUtil._CalculateCenteroid(obj.Array());
				for(int i = 0; i < obj.Count(); i++) {
					obj[i] = (obj[i] - center);
				}
			}
			if(GUILayout.Button(gui_TglEditInSceneName, GUILayout.Width(80))) {
				gui_EditInScene = !gui_EditInScene;
				SceneView.RepaintAll();
			}
			EditorGUILayout.Space();
			EditorGUILayout.EndHorizontal();
#endregion Top Buttons

#region Edit In Scene
			if(gui_EditInScene) {
				EditorGUILayout.Space();
				EditorGUILayout.BeginVertical("box");
				gui_TglRoundToInt = EditorGUILayout.Toggle("Round Float Numbers?", gui_TglRoundToInt);
				gui_Offset = EditorGUILayout.Vector3Field("Raycast Offset:", gui_Offset);
				EditorGUILayout.EndVertical();
			}
#endregion Edit In Scene
			EditorGUILayout.Space();
#region The List
			//The list
			EditorGUILayout.BeginVertical();
			gui_ScrollPos = EditorGUILayout.BeginScrollView(gui_ScrollPos, GUILayout.Height(320f));
			EditorGUIUtility.labelWidth = 70f;
			for(int i = 0; i < obj.Count(); i++) {
				EditorGUILayout.BeginHorizontal("box");
				obj[i] = EditorGUILayout.Vector3Field("Index: " + i, obj[i]);

				if(GUILayout.Button("INS", GUILayout.Width(36))) {
					obj.Insert(i, default); // default is literal, no need for default(Vector3)
					break;
				}
				if(GUILayout.Button("DEL", GUILayout.Width(36))) {
					obj.RemoveAt(i);
					break;
				}
				EditorGUILayout.EndHorizontal();
			}
			EditorGUIUtility.labelWidth = 0f;
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndScrollView();
#endregion The List
			EditorGUILayout.Space();
#region Edit The List
			if(obj.Count() > 0) {
				EditorGUILayout.BeginHorizontal("box");
				gui_Columns = EditorGUILayout.FloatField("SET COLUMNS: ", gui_Columns);
				if(GUILayout.Button("P.X's", GUILayout.Width(40))) {
					for(int i = 0; i < obj.Count(); i++) {
						obj[i] = _VecUtil._SetX(obj[i], gui_Columns);
					}
				}
				if(GUILayout.Button("P.Y's", GUILayout.Width(40))) {
					for(int i = 0; i < obj.Count(); i++) {
						obj[i] = _VecUtil._SetY(obj[i], gui_Columns);
					}
				}
				if(GUILayout.Button("P.Z's", GUILayout.Width(40))) {
					for(int i = 0; i < obj.Count(); i++) {
						obj[i] = _VecUtil._SetZ(obj[i], gui_Columns);
					}
				}
				EditorGUILayout.EndHorizontal();
			}
#endregion Edit The List
			EditorGUILayout.Space();
#region Drawing Type
			EditorGUILayout.BeginVertical("box");
			gui_Color = EditorGUILayout.ColorField("Line Color", gui_Color);
			gui_CompareFunction = (CompareFunction)EditorGUILayout.EnumPopup("Line comparison: ", gui_CompareFunction);
			EditorGUILayout.EndVertical();
#endregion Drawing Type
			if(GUI.changed) {
				gui_TglEditInSceneName = (gui_EditInScene) ? "STOP EDIT" : "SCENE EDIT";
				EditorUtility.SetDirty(obj);
				SceneView.RepaintAll();
			}
		}

		private void DoSceneGUI(SceneView sceneView) {
			Event current = Event.current;

			//Draw spheres
			if(obj.Count() > 0) {
				Handles.zTest = gui_CompareFunction;
				for(int i = 0; i < obj.Count(); i++) {
					_UIUtil._Hndl_Sphere(obj[i], 0.25f, gui_Color);
					_UIUtil._Hndl_LabelCenter(obj[i] - Vector3.up * 0.5f, "V: ".Label() + "{0}".FormatWith(i).Text("yellow"));
				}
				Handles.zTest = CompareFunction.Always;
			}

			if(!gui_EditInScene)
				return;

			//Move with handles
			if(obj.Count() > 0) {
				for(int i = 0; i < obj.Count(); i++) {
					obj[i] = _UIUtil._Hndl_Position(obj[i], Quaternion.identity);
				}
			}

			int controlID = GUIUtility.GetControlID(sceneView.GetHashCode(), FocusType.Passive);
			switch(current.GetTypeForControl(controlID)) {
				case EventType.Layout: {
					HandleUtility.AddDefaultControl(controlID);
				}
				break;
				case EventType.MouseDown: {
					if(current.alt || current.control)
						return;
					if(current.button != 0)
						return;
					if(current.type == EventType.MouseDown)
						GUIUtility.hotControl = controlID;
				}
				break;
				case EventType.MouseUp: {
					if(current.alt || current.control)
						return;
					if(current.button != 0)
						return;
					if(GUIUtility.hotControl != controlID)
						return;

					Ray r = HandleUtility.GUIPointToWorldRay(current.mousePosition);
					Vector3 pos = r.origin + (r.direction * sceneView.cameraDistance);
					RaycastHit hit;
					if(Physics.Raycast(r, out hit, float.MaxValue)) {
						if(hit.transform) {
							pos = hit.point + gui_Offset;
						}
					}
					if(gui_TglRoundToInt) {
						pos = _VecUtil._Round(pos);
					}
					obj.Add(pos);

					EditorUtility.SetDirty(obj);
					current.Use();
					GUIUtility.hotControl = 0;
				}
				break;
				case EventType.Repaint: {
					HandleUtility.Repaint();
				}
				break;
			}
		}
#endregion Methods

#region Helper Methods
		void InitializeGUIVariables() {
			gui_CircleRadius = 0.5f;
			gui_CircleSegments = 5;
			gui_TglEditInSceneName = "SCENE EDIT";
			gui_EditInScene = false;
			gui_TglRoundToInt = false;
			gui_Offset = Vector3.zero;
			gui_ScrollPos = Vector2.zero;
			gui_Columns = 0f;
			gui_Color = Color.cyan;
			gui_CompareFunction = CompareFunction.Always;
		}
#endregion Helper Methods
	}

}
