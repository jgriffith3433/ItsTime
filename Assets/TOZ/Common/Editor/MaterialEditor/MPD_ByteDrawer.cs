using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TOZEditor {

	//Material PropertyDrawer [MPD_Byte] makes sure that the value can only be an int between 0 and 255
	public class MPD_ByteDrawer : MaterialPropertyDrawer {
#region Mono Methods
		//Standart Mono Methods
		public override void OnGUI(Rect position, MaterialProperty prop, String label, MaterialEditor editor) {
			int value = (int)prop.floatValue;

			EditorGUI.BeginChangeCheck();
			EditorGUI.showMixedValue = prop.hasMixedValue;
			value = EditorGUI.IntField(position, label, Mathf.Clamp(value, 0, 255));
			EditorGUI.showMixedValue = false;
			if(EditorGUI.EndChangeCheck()) {
				prop.floatValue = (float)value;
			}
		}
#endregion Mono Methods
	}

}
