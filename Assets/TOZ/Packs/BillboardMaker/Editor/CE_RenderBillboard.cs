using System;
//using System.IO;
using UnityEngine;
using UnityEditor;
using TOZ;

namespace TOZEditor {

	[CustomEditor(typeof(RenderBillboard))]
	public class CE_RenderBillboard : Editor {
		//Variables
		private RenderBillboard obj;

		private string str_helpRender =
		"To Render:\n" +
		"*First, drop your object that has a MeshRenderer component in the scene, position doesnt matter.\n" +
		"*Then select it as Object To Render above.\n" +
		"*It will be rendered from the backside (-z) position.\n" +
		"*Adjust your lighting in the scene, preferably (-z) position.\n" +
		"*Finally click on RENDER. The texture will be saved in the root of your project folder";

		//Mono Methods
		void OnEnable() {
			obj = (RenderBillboard)target;
		}

		public override void OnInspectorGUI() {
			DrawDefaultInspector();
			EditorGUILayout.HelpBox(str_helpRender, MessageType.Info, true);
			if(GUILayout.Button("RENDER")) {
				Texture2D tex = obj.Render();
				tex.Save();
			}
		}
	}

}
