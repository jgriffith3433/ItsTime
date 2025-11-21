using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using TOZ;

namespace TOZEditor {

	public static class _AssetUtil {
		public static T _Create<T>() where T : ScriptableObject {
			T asset = ScriptableObject.CreateInstance<T>();
			
			string path = AssetDatabase.GetAssetPath(Selection.activeObject);
			if(path == string.Empty)
				path = "Assets";
			else if(Path.GetExtension(path) != string.Empty)
				path = path.Replace(Path.GetFileName(path), string.Empty);
			
			string name = AssetDatabase.GenerateUniqueAssetPath(path + "/New " + typeof(T).ToString() + "_" + ".asset");
			AssetDatabase.CreateAsset(asset, name);
			
			AssetDatabase.SaveAssets();
			EditorUtility.FocusProjectWindow();
			Selection.activeObject = asset;
			return asset;
		}

		public static void _SaveMesh(Mesh mesh) {
			bool isAsset = AssetDatabase.Contains(mesh);
			bool safeToSave = false;
			if(isAsset) {
				Debug.LogWarning("NOT ALLOWED TO OVERWRITE!! This is already a saved asset in: " + AssetDatabase.GetAssetPath(mesh));
				if(EditorUtility.DisplayDialog("SAVE AS NEW MESH?", "You can NOT overwrite an asset, would you like to save as new Mesh?", "Yes", "No")) {
					mesh = _MeshUtil._CreateCopy(mesh);
					safeToSave = true;
				}
				else {
					safeToSave = false;
				}
			}
			else {
				safeToSave = true;
			}

			if(safeToSave) {
				string file = EditorUtility.SaveFilePanelInProject("Save Mesh", "New Fancy Mesh", "mesh", "Please enter a file name");
				if(file.Length != 0) {
					if(EditorUtility.DisplayDialog("Optimize?", "Optimize mesh and reorder vertex/index buffers before saving?", "Yes", "No")) {
						MeshUtility.Optimize(mesh);
					}

					AssetDatabase.CreateAsset(mesh, file);
					AssetDatabase.SaveAssets();
					EditorUtility.FocusProjectWindow();
					Selection.activeObject = mesh;
				}
			}
		}
	}

}