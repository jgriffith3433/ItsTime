using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using TOZ;
using TOZ.Noise;
using TOZ.Celestial;

namespace TOZEditor {

	[CustomEditor(typeof(AsteroidField))]
	[CanEditMultipleObjects]
	public class CE_AsteroidField : Editor {
		private enum TabsType {
			Info, Creation
		}

		//Editor Variables
		private TabsType currentTab = TabsType.Info;
		private string[] str_Tabs = new string[2] { "Info", "Creation" };

		//Variables
		private SerializedProperty myname;

		private SerializedProperty path;
		private SerializedProperty amount;
		private SerializedProperty scatterScale;
		private SerializedProperty minScale;
		private SerializedProperty maxScale;
		private SerializedProperty asteroidPrefabs;

		//Mono Methods
		private void OnEnable() {
			myname = serializedObject.FindProperty("Name");

			path = serializedObject.FindProperty("path");
			amount = serializedObject.FindProperty("amount");
			scatterScale = serializedObject.FindProperty("scatterScale");
			minScale = serializedObject.FindProperty("minScale");
			maxScale = serializedObject.FindProperty("maxScale");
			asteroidPrefabs = serializedObject.FindProperty("asteroidPrefabs");
		}

		public override void OnInspectorGUI() {
			serializedObject.Update();

			EditorGUILayout.Space();
			currentTab = (TabsType)GUILayout.Toolbar((int)currentTab, str_Tabs, EditorStyles.toolbarButton);
			switch(currentTab) {
				case TabsType.Info: {
					EditorGUILayout.Space();
					myname.stringValue = EditorGUILayout.TextField("Celestial Name: ", myname.stringValue);
				}
				break;
				case TabsType.Creation: {
					path.objectReferenceValue = EditorGUILayout.ObjectField("Spline Path: ", path.objectReferenceValue, typeof(SO_SplineData), false);
					amount.intValue = EditorGUILayout.IntField("Asteroid Amount: ", amount.intValue);
					scatterScale.floatValue = EditorGUILayout.FloatField("Scatter Scale: ", scatterScale.floatValue);
					minScale.floatValue = EditorGUILayout.FloatField("Min Scale: ", minScale.floatValue);
					maxScale.floatValue = EditorGUILayout.FloatField("Max Scale: ", maxScale.floatValue);
					EditorGUILayout.PropertyField(asteroidPrefabs, new GUIContent("Asteroid Prefabs"), true);
				}
				break;
			}

			serializedObject.ApplyModifiedProperties();
		}
	}

}
