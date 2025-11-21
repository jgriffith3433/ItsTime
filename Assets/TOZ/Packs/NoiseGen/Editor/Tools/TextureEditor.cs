using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using TOZ;

namespace TOZEditor {

	public class TextureEditor : EditorWindow {
		//Variables
		public static Texture2D Texture;
		public static int Width, Height;
		public static bool IsNormalTexture;
		public static bool IsPlanetTexture;

		private float deformMultiplier = 1f;
		private float sedimentCapacityFactor = 4f;
		private int brushRadius = 2;

		//Mono Methods
		void OnGUI() {
			//First Column start
			EditorGUILayout.BeginVertical("box", GUILayout.Width(230), GUILayout.ExpandHeight(true));

			if(GUILayout.Button("SAVE TEXTURE")) {
				Texture.Save();
			}

			if(!IsNormalTexture) {
				GUILayout.FlexibleSpace();
				EditorGUILayout.LabelField("EROSION", EditorStyles.boldLabel);
				sedimentCapacityFactor = EditorGUILayout.FloatField("Sediment Capacity: ", sedimentCapacityFactor);
				brushRadius = EditorGUILayout.IntField("Brush Radius: ", brushRadius);

				if(GUILayout.Button("ERODE")) {
					float[,] map = _MapUtil._GetGrayscaleMapFromTexture(Texture);
					_MapUtil._Erode(map, sedimentCapacityFactor, brushRadius);
					float[] map2 = _ArrayUtil._DuoToMono(map);
					Texture.Fill(map2);
				}

				GUILayout.FlexibleSpace();
				EditorGUILayout.LabelField("DEFORMATIONS", EditorStyles.boldLabel);
				EditorGUILayout.LabelField("!!!NO UNDO FOR DEFORMATIONS!!!");
				EditorGUILayout.Space();
				if(GUILayout.Button("DEFORM TERRAIN")) {
					if(Selection.activeGameObject != null) {
						Terrain terrain = Selection.activeGameObject.GetComponent<Terrain>();
						if(terrain != null) {
							//Undo.IncrementCurrentGroup();
							Undo.RegisterCompleteObjectUndo(terrain.terrainData, "Deform Terrain");
							//Undo.RegisterCompleteObjectUndo(terrain.terrainData.alphamapTextures, "Modify Terrain");
							Texture.DeformTerrain(terrain);
						}
						else {
							Debug.LogWarning("Selected object is not a terrain.");
						}
					}
					else {
						Debug.LogWarning("You must select an active terrain from the Hierarchy.");
					}
				}
				if(GUILayout.Button("ADD TO TERRAIN")) {
					if(Selection.activeGameObject != null) {
						Terrain terrain = Selection.activeGameObject.GetComponent<Terrain>();
						if(terrain != null) {
							//Undo.IncrementCurrentGroup();
							Undo.RegisterCompleteObjectUndo(terrain.terrainData, "Deform Terrain Additive");
							//Undo.RegisterCompleteObjectUndo(terrain.terrainData.alphamapTextures, "Modify Terrain");
							Texture.DeformTerrainAdditive(terrain);
						}
						else {
							Debug.LogWarning("Selected object is not a terrain.");
						}
					}
					else {
						Debug.LogWarning("You must select an active terrain from the Hierarchy.");
					}
				}
				EditorGUILayout.Space();
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Multiplier:", GUILayout.MaxWidth(60));
				deformMultiplier = EditorGUILayout.FloatField(deformMultiplier, GUILayout.MaxWidth(40));
				if(GUILayout.Button("DEFORM MESH")) {
					if(Selection.activeGameObject != null) {
						Mesh mesh = Selection.activeGameObject.GetComponent<MeshFilter>().sharedMesh;
						if(mesh != null) {
							Texture.DeformMesh(mesh, deformMultiplier);
						}
						else {
							Debug.LogWarning("Selected object doesnt have a mesh.");
						}
					}
					else {
						Debug.LogWarning("You must select an active object from the Hierarchy.");
					}
				}
				EditorGUILayout.EndHorizontal();
			}

			if(IsPlanetTexture) {
				GUILayout.FlexibleSpace();
				EditorGUILayout.HelpBox("A Planet Texture can only be used with TOZ Planet Renderer which is a seperate paid product that renders LOD planets in a solar system scale.", MessageType.Warning);
				if(GUILayout.Button("GO TO ASSETSTORE")) {
					Application.OpenURL("https://assetstore.unity.com/packages/templates/systems/toz-planet-renderer-122275?aid=1101l4NkK");
				}
			}

			GUILayout.FlexibleSpace();
			EditorGUILayout.LabelField("Preview Size:", Width + " x " + Height);
			EditorGUILayout.LabelField("Actual Size:", Texture.width + " x " + Texture.height);
			//First Column end
			EditorGUILayout.EndVertical();

			EditorGUI.DrawPreviewTexture(new Rect(240, 6, Width, Height), Texture, null);
		}
	}

}