using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using TOZ;
using TOZ.Noise;

namespace TOZEditor {

	[CustomEditor(typeof(SO_NoiseMap))]
	public class CE_SO_NoiseMap : Editor {
		//Variables
		SO_NoiseMap obj;

		//Mono Methods
		private void OnEnable() {
			obj = (SO_NoiseMap)target;
		}

		public override void OnInspectorGUI() {
			DoInspectorGUI();
		}

		//GUI Methods
		public void DoInspectorGUI() {
			string dataField = (obj.Data != null) ? 
				"Data Array is initialized as mono, convert to trio by using ArrayUtil" : " Data Array is null";
			EditorGUILayout.LabelField(dataField);
			EditorGUILayout.LabelField("Data as Mono: " + "[" + obj.Data.Length.ToString() + "]");
			EditorGUILayout.LabelField("Data as Trio: " + "[" + obj.Width.ToString() + "," + obj.Height.ToString() + "," + obj.Depth.ToString() + "]");
		}
	}

}
