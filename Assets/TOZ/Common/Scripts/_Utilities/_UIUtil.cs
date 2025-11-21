using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TOZ {

	public static class _UIUtil {
		public static void _SelectNull() {
			EventSystem.current.SetSelectedGameObject(null);
		}

		public static void _SelectFirstActiveElement(GameObject go) {
			GameObject selection = null;
			Selectable[] selectables = go.GetComponentsInChildren<Selectable>(true);
			foreach(Selectable s in selectables) {
				if(s.IsActive() && s.IsInteractable()) {
					selection = s.gameObject;
					break;
				}
			}
			EventSystem.current.SetSelectedGameObject(selection);
		}
#if UNITY_EDITOR
		public static void _UI_LabelInfo(string prefix, string label) {
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel(prefix);
			EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
			EditorGUILayout.EndHorizontal();
		}

		public static void _UI_ColorInfo(string prefix, Color col) {
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel(prefix);
			EditorGUIUtility.labelWidth = 200;
			EditorGUILayout.ColorField(new GUIContent(col.ToString()), col, false, false, false);
			EditorGUIUtility.labelWidth = 0;
			EditorGUILayout.EndHorizontal();
		}

		//Draw a centered label with Handles
		public static void _Hndl_LabelCenter(Vector3 wPos, string str) {
			SceneView view = SceneView.currentDrawingSceneView;
			if(!view)
				return;

			Camera cam = view.camera;
			Vector3 scrPos = cam.WorldToScreenPoint(wPos);
			if(scrPos.y < 0 || scrPos.y > cam.pixelHeight || scrPos.x < 0 || scrPos.x > cam.pixelWidth || scrPos.z < 0)
				return;

			GUIContent content = new GUIContent(str);
			GUIStyle style = new GUIStyle(EditorGUIUtility.GetBuiltinSkin(EditorSkin.Scene).label) {
				richText = true,
			};

			Vector2 rectSize = style.CalcSize(content);
			Rect rect = new Rect(Vector2.zero, rectSize);
			rect.center = new Vector2(scrPos.x / EditorGUIUtility.pixelsPerPoint, (cam.pixelHeight - scrPos.y) / EditorGUIUtility.pixelsPerPoint);

			Handles.BeginGUI();
			EditorGUI.DrawRect(rect, Color.red * 0.5f);
			EditorGUI.LabelField(rect, str, style);
			Handles.EndGUI();
		}

		//Draw a Dot with Handles
		public static void _Hndl_Dot(Vector3 pos, Quaternion rot, float size) {
			Color cachedCol = Handles.color;
			Handles.color = Color.white;
			float handleSize = HandleUtility.GetHandleSize(pos) * size;
			Handles.DotHandleCap(0, pos, rot, handleSize, EventType.Repaint);
			Handles.color = cachedCol;
		}

		//Draw a Sphere with Handles
		public static void _Hndl_Sphere(Vector3 pos, float size, Color col) {
			Color cachedCol = Handles.color;
			Handles.color = col;
			float handleSize = HandleUtility.GetHandleSize(pos) * size;
			Handles.SphereHandleCap(0, pos, Quaternion.identity, handleSize, EventType.Repaint);
			Handles.color = cachedCol;
		}

		//Draw a Line with Handles
		public static void _Hndl_Line(Vector3 start, Vector3 end, float size, Color col) {
			Color cachedCol = Handles.color;
			Handles.color = col;
			Handles.DrawLine(start, end, size);
			Handles.color = cachedCol;
		}

		//Draw an Arc with Handles
		public static void _Hndl_Arc(Vector3 pos, Vector3 normal, Vector3 dir, float angle, float radius, Color col) {
			Color cachedCol = Handles.color;
			Handles.color = col;
			Handles.DrawSolidArc(pos, normal, dir, angle, radius);
			Handles.color = cachedCol;
		}

		//Draw a Wire Arc with Handles
		public static void _Hndl_WireArc(Vector3 pos, Vector3 normal, Vector3 dir, float angle, float radius, Color col) {
			Color cachedCol = Handles.color;
			Handles.color = col;
			Handles.DrawWireArc(pos, normal, dir, angle, radius);
			Handles.color = cachedCol;
		}

		//Draw a Wire Arc with Handles
		public static void _Hndl_WireDisc(Vector3 pos, Vector3 normal, float size, Color col) {
			Color cachedCol = Handles.color;
			Handles.color = col;
			float handleSize = HandleUtility.GetHandleSize(pos) * size;
			Handles.DrawWireDisc(pos, normal, handleSize);
			Handles.color = cachedCol;
		}

		public static void _Hndl_Rectangle(Vector3 pos, Vector2 scale, Vector3 t, Vector3 b, Vector3 n, Color col) {
			Color cachedCol = Handles.color;
			Handles.color = col;
			Vector3 xSize = (b * scale.x);
			Vector3 ySize = (n * scale.y);
			Vector3 p1 = pos + (-xSize + -ySize);
			Vector3 p2 = pos + (-xSize + ySize);
			Vector3 p3 = pos + (xSize + ySize);
			Vector3 p4 = pos + (xSize + -ySize);
			Handles.DrawLine(p1, p2);
			Handles.DrawLine(p3, p4);
			Handles.DrawLine(p2, p3);
			Handles.DrawLine(p4, p1);
			Handles.color = cachedCol;
		}

		//Draw a position handle
		public static Vector3 _Hndl_Position(Vector3 pos, Quaternion rot) {
			Color cachedCol = Handles.color;

			float handleSize = HandleUtility.GetHandleSize(pos) * 1.0f;
			Handles.color = Color.red;
			pos = Handles.Slider(pos, rot * Vector3.right, handleSize, new Handles.CapFunction(Handles.ArrowHandleCap), 1f);
			Handles.color = Color.green;
			pos = Handles.Slider(pos, rot * Vector3.up, handleSize, new Handles.CapFunction(Handles.ArrowHandleCap), 1f);
			Handles.color = Color.blue;
			pos = Handles.Slider(pos, rot * Vector3.forward, handleSize, new Handles.CapFunction(Handles.ArrowHandleCap), 1f);

			Handles.color = cachedCol;
			return pos;
		}

		//Draw a scale XY handle
		public static Vector3 _Hndl_Scale2D(Vector3 pos, Quaternion rot, Vector2 scale) {
			Color cachedCol = Handles.color;

			float handleSize = HandleUtility.GetHandleSize(pos) * 0.75f;
			Handles.color = Color.red;
			scale.x = Handles.ScaleSlider(scale.x, pos, rot * Vector3.right, Quaternion.identity, handleSize, 1f);
			Handles.color = Color.green;
			scale.y = Handles.ScaleSlider(scale.y, pos, rot * Vector3.up, Quaternion.identity, handleSize, 1f);

			Handles.color = cachedCol;
			return scale;
		}

		//Draw a scale XYZ handle
		public static Vector3 _Hndl_Scale3D(Vector3 pos, Quaternion rot, Vector3 scale) {
			Color cachedCol = Handles.color;

			float handleSize = HandleUtility.GetHandleSize(pos) * 0.75f;
			Handles.color = Color.red;
			scale.x = Handles.ScaleSlider(scale.x, pos, Vector3.right, rot, handleSize, 1f);
			Handles.color = Color.green;
			scale.y = Handles.ScaleSlider(scale.y, pos, Vector3.up, rot, handleSize, 1f);
			Handles.color = Color.blue;
			scale.z = Handles.ScaleSlider(scale.z, pos, Vector3.forward, rot, handleSize, 1f);

			Handles.color = cachedCol;
			return scale;
		}
#endif

		public static string _Label(string str, string color = "black") {
			return "<color="+color+"><b>"+str+"</b></color>";
		}

		public static string _Text(string str, string color = "white") {
			return "<color="+color+">"+str+"</color>";
		}
	}

}