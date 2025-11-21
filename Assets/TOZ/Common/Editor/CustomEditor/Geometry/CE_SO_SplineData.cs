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

	[CustomEditor(typeof(SO_SplineData))]
	public sealed class CE_SO_SplineData : Editor {
#region Variables
		//Variables
		private SO_SplineData obj;
		private Spline spline;

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
		private float gui_Time;
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
			obj = (SO_SplineData)target;
			spline = new Spline(obj.Type, obj.Segments);

			InitializeGUIVariables();

			SceneView.duringSceneGui += OnSceneGUI;
		}

		private void DoDisable() {
			SceneView.duringSceneGui -= OnSceneGUI;

			spline = null;
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
					obj.Add(new SplineNode(new Vector3(sin, 0f, cos)));
				}
			}
			EditorGUILayout.EndHorizontal();
#endregion Create Circle
			EditorGUILayout.Space();
#region Top Buttons
			EditorGUILayout.BeginHorizontal("box");
			EditorGUILayout.Space();
			if(GUILayout.Button("ADD NEW", GUILayout.Width(80))) {
				//obj.Add(default); // default is literal, no need for default(Vector3)
				obj.Add(new SplineNode());
			}
			if(GUILayout.Button("CLEAR", GUILayout.Width(80))) {
				obj.Clear();
			}
			if(GUILayout.Button("NORMALIZE", GUILayout.Width(80))) {
				Vector3 center = _GeomUtil._CalculateCenteroid(obj.Positions);
				float length = 1f;
				for(int i = 0; i < obj.Count(); i++) {
					float d = Mathf.Abs((obj[i].P - center).magnitude) * 2f;
					if(d > length) {
						length = d;
					}
				}
				for(int i = 0; i < obj.Count(); i++) {
					obj[i].P = (obj[i].P - center) / length;
				}
			}
			if(GUILayout.Button("CENTER", GUILayout.Width(80))) {
				Vector3 center = _GeomUtil._CalculateCenteroid(obj.Positions);
				for(int i = 0; i < obj.Count(); i++) {
					obj[i].P = (obj[i].P - center);
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
				EditorGUILayout.BeginVertical("box", GUILayout.Height((EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 3));
				//EditorGUILayout.LabelField("Node ID: " + i);
				EditorGUILayout.LabelField("Index: " + i);
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.BeginVertical();
				obj[i].P = EditorGUILayout.Vector3Field("Position:", obj[i].P);
				obj[i].S = EditorGUILayout.Vector2Field("Scale:", obj[i].S);
				EditorGUILayout.EndVertical();

				if(GUILayout.Button("INS", GUILayout.Width(36), GUILayout.ExpandHeight(true))) {
					//obj.Insert(i, default); // default is literal, no need for default(Vector3)
					obj.Insert(i, new SplineNode());
					break;
				}
				if(GUILayout.Button("DEL", GUILayout.Width(36), GUILayout.ExpandHeight(true))) {
					obj.RemoveAt(i);
					break;
				}
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.EndVertical();
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
						obj[i].P = _VecUtil._SetX(obj[i].P, gui_Columns);
					}
				}
				if(GUILayout.Button("P.Y's", GUILayout.Width(40))) {
					for(int i = 0; i < obj.Count(); i++) {
						obj[i].P = _VecUtil._SetY(obj[i].P, gui_Columns);
					}
				}
				if(GUILayout.Button("P.Z's", GUILayout.Width(40))) {
					for(int i = 0; i < obj.Count(); i++) {
						obj[i].P = _VecUtil._SetZ(obj[i].P, gui_Columns);
					}
				}
				if(GUILayout.Button("S.X's", GUILayout.Width(40))) {
					for(int i = 0; i < obj.Count(); i++) {
						obj[i].S = new Vector2(gui_Columns, obj[i].S.y);
					}
				}
				if(GUILayout.Button("S.Y's", GUILayout.Width(40))) {
					for(int i = 0; i < obj.Count(); i++) {
						obj[i].S = new Vector2(obj[i].S.x, gui_Columns);
					}
				}
				EditorGUILayout.EndHorizontal();
			}
#endregion Edit The List
			EditorGUILayout.Space();
#region Spline Type
			EditorGUILayout.BeginVertical("box");
			obj.Type = (SplineType)EditorGUILayout.EnumPopup("Spline Type: ", obj.Type);
			obj.Segments = EditorGUILayout.IntSlider("Spline Segments:", obj.Segments, 1, 32);
			gui_Color = EditorGUILayout.ColorField("Spline Color", gui_Color);
			gui_CompareFunction = (CompareFunction)EditorGUILayout.EnumPopup("Line comparison: ", gui_CompareFunction);
			EditorGUILayout.EndVertical();
#endregion Spline Type
			if(GUI.changed) {
				gui_TglEditInSceneName = (gui_EditInScene) ? "STOP EDIT" : "SCENE EDIT";
				EditorUtility.SetDirty(obj);
				spline.Clear();
				if(obj.Count() > 3) {
					spline.Type = obj.Type;
					spline.Segments = obj.Segments;
					spline.Initialize(obj.Data, true);
				}
				SceneView.RepaintAll();
			}

			EditorGUILayout.Space();
#region Spline Info
			EditorGUILayout.BeginVertical("box");
			EditorGUILayout.LabelField("Total Nodes: " + spline.Count().ToString());
			EditorGUILayout.LabelField("Total Length: " + spline.GetLength().ToString());
			gui_Time = EditorGUILayout.Slider("Time", gui_Time, 0f, 1f);
			EditorGUILayout.EndVertical();
#endregion Spline Info
		}

		private void DoSceneGUI(SceneView sceneView) {
			//Initialize spline
			if(obj.Count() > 3) {
				spline.Initialize(obj.Data, true);
			}

			//Draw Nodes
			if(obj.Count() > 0) {
				Handles.zTest = gui_CompareFunction;
				for(int i = 0; i < obj.Count(); i++) {
					_UIUtil._Hndl_Sphere(obj[i].P, 0.25f, gui_Color);
					_UIUtil._Hndl_LabelCenter(obj[i].P - Vector3.up * 0.5f, "V: ".Label() + "{0}".FormatWith(i).Text("yellow"));
				}
				Handles.zTest = CompareFunction.Always;
			}

			//Draw spline
			if(spline != null && spline.Count() > 3) {
				Handles.color = gui_Color;
				Handles.zTest = gui_CompareFunction;
				for(int n = 0; n < spline.Count()-1; n++) {
					Handles.DrawLine(spline[n].P, spline[n+1].P);
				}

				for(int n = 0; n < spline.Count(); n++) {
					_UIUtil._Hndl_Dot(spline[n].P, Quaternion.identity, 0.05f);
					_UIUtil._Hndl_Rectangle(spline[n].P, spline[n].S, spline[n].T, spline[n].B, spline[n].N, gui_Color);
				}
				Handles.zTest = CompareFunction.Always;

				Vector3 pos = spline.GetPosAtTime(gui_Time);
				SplineNode node = spline.GetNodeAtTime(gui_Time);
				_UIUtil._Hndl_LabelCenter(pos + Vector3.up * 2f, spline.IndexOf(node).ToString());
				//_UIUtil._Hndl_Sphere(pos, 2.0f, Color.green);
				//Handles.color = Color.green;
				_UIUtil._Hndl_WireDisc(pos, Vector3.up, 1.0f, Color.yellow);
				_UIUtil._Hndl_Sphere(pos, 0.125f, Color.yellow);
				//Handles.DrawWireDisc(pos, Vector3.up, 2.0f);
				//Handles.DrawWireCube(pos, Vector3.one * 2.0f);
			}

			if(!gui_EditInScene)
				return;

			//Move with handles
			if(obj.Count() > 3) {
				Quaternion rot = Quaternion.identity;
				for(int i = 0; i < obj.Count(); i++) {
					if(spline.Type.Is(SplineType.Open)) {
						if(i < 1 || i > obj.Count()-2)
							rot = Quaternion.identity;
						else
							rot = Quaternion.LookRotation(spline[(i-1) * spline.Segments].T, spline[(i-1)* spline.Segments].N);
					}
					else {
						rot = Quaternion.LookRotation(spline[i * spline.Segments].T, spline[i * spline.Segments].N);
					}

					obj[i].P = _UIUtil._Hndl_Position(obj[i].P, rot);
					obj[i].S = _UIUtil._Hndl_Scale2D(obj[i].P, rot, obj[i].S);
				}
			}

			int controlID = GUIUtility.GetControlID(sceneView.GetHashCode(), FocusType.Passive);
			Event current = Event.current;
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
					obj.Add(new SplineNode(pos));

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
			gui_Time = 0f;
		}
#endregion Helper Methods
	}

}
