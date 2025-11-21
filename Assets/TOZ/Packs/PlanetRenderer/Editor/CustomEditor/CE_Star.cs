using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using TOZ;
using TOZ.Celestial;

namespace TOZEditor {

	[CustomEditor(typeof(Star))]
	[CanEditMultipleObjects]
	public sealed class CE_Star : Editor {
#region Variables
		private enum TabsType {
			Info, Creation, Space, LOD_NAV
		}

		//Editor Variables
		private TabsType currentTab = TabsType.Info;
		private string[] str_Tabs = new string[4] { "Info", "Creation", "Space", "LOD & NAV" };

		//Variables
		private SerializedProperty myName;
		private SerializedProperty isMoon;
		private SerializedProperty parent;
		private SerializedProperty gravityScale;
		private SerializedProperty radius;

		private SerializedProperty flareAsset;
		private SerializedProperty ambientColorInSpace;
		private SerializedProperty lightIntensityInSpace;
		private SerializedProperty clipDistanceInSpace;

		private SerializedProperty dataMap;
		private SerializedProperty updateInterval;
		private SerializedProperty areCelestialsDynamic;
#endregion Variables

#region Mono Methods
		private void OnEnable() {
			DoEnable();
		}

		public override void OnInspectorGUI() {
			DoInspectorGUI();
		}
#endregion Mono Methods

#region Methods
		//Mono Methods
		private void DoEnable() {
			myName = serializedObject.FindProperty("Name");
			isMoon = serializedObject.FindProperty("IsMoon");
			parent = serializedObject.FindProperty("Parent");
			gravityScale = serializedObject.FindProperty("GravityScale");
			radius = serializedObject.FindProperty("Radius");

			flareAsset = serializedObject.FindProperty("flareAsset");
			ambientColorInSpace = serializedObject.FindProperty("ambientColorInSpace");
			lightIntensityInSpace = serializedObject.FindProperty("lightIntensityInSpace");
			clipDistanceInSpace = serializedObject.FindProperty("clipDistanceInSpace");

			dataMap = serializedObject.FindProperty("dataMap");
			updateInterval = serializedObject.FindProperty("updateInterval");
			areCelestialsDynamic = serializedObject.FindProperty("AreCelestialsDynamic");
		}

		private void DoInspectorGUI() {
			serializedObject.Update();

			EditorGUILayout.Space();
			currentTab = (TabsType)GUILayout.Toolbar((int)currentTab, str_Tabs, EditorStyles.toolbarButton);
			switch(currentTab) {
				case TabsType.Info: {
					EditorGUILayout.Space();
					myName.stringValue = EditorGUILayout.TextField("Celestial Name: ", myName.stringValue);
					isMoon.boolValue = EditorGUILayout.Toggle("Is Moon: ", isMoon.boolValue);
					using(new EditorGUI.DisabledScope(!isMoon.boolValue)) {
						parent.objectReferenceValue = EditorGUILayout.ObjectField("Parent Celestial: ", parent.objectReferenceValue, typeof(CelestialBase), true);
					}
				}
				break;
				case TabsType.Creation: {
					EditorGUILayout.Space();
					gravityScale.floatValue = EditorGUILayout.Slider("Gravity Scale: ", gravityScale.floatValue, 0f, 10f);
					radius.floatValue = EditorGUILayout.FloatField("Sun Radius: ", radius.floatValue);
					EditorGUILayout.Space();
					EditorGUILayout.PropertyField(flareAsset, new GUIContent("Flare"), true);
					EditorGUILayout.Space();
					dataMap.objectReferenceValue = EditorGUILayout.ObjectField("Data Texture: ", dataMap.objectReferenceValue, typeof(Texture2D), false);
				}
				break;
				case TabsType.Space: {
					EditorGUILayout.Space();
					ambientColorInSpace.colorValue = EditorGUILayout.ColorField("Ambient Color: ", ambientColorInSpace.colorValue);
					lightIntensityInSpace.floatValue = EditorGUILayout.Slider("Sun Intensity: ", lightIntensityInSpace.floatValue, 0.5f, 1.5f);
					clipDistanceInSpace.floatValue = EditorGUILayout.Slider("Clip Distance: ", clipDistanceInSpace.floatValue, 0f, 65535f);
				}
				break;
				case TabsType.LOD_NAV: {
					EditorGUILayout.Space();
					updateInterval.floatValue = EditorGUILayout.Slider("LOD Update Interval: ", updateInterval.floatValue, 0f, 5f);
					EditorGUILayout.Space();
					areCelestialsDynamic.boolValue = EditorGUILayout.Toggle("Are Celestials Dynamic: ", areCelestialsDynamic.boolValue);
				}
				break;
			}

			serializedObject.ApplyModifiedProperties();
		}
#endregion Methods

#region Helper Methods
		//Helper Methods
		[DrawGizmo(GizmoType.NonSelected | GizmoType.Active | GizmoType.InSelectionHierarchy)]
		private static void DrawGizmo(Star cs, GizmoType gizmoType) {
			Gizmos.color = new Color(1, 1, 0, 0.5f);
			Gizmos.DrawSphere(cs.transform.position, cs.Radius);
			float atmosphereRadius = cs.Radius + (cs.Radius * 0.025f) * 3f;
			Gizmos.color = new Color(0, 0, 1, 0.25f);
			Gizmos.DrawSphere(cs.transform.position, atmosphereRadius);

			if(_NAV._Wps != null) {
				Gizmos.color = new Color(1, 0, 0, 1f);
				for(int i = 0; i < _NAV._Wps.Count; i++) {
					Gizmos.DrawWireCube(_NAV._Wps[i], Vector3.one * 150f);
				}
			}
		}
#endregion Helper Methods
	}

}
