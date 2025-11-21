using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TOZ {

	public static class TexEx {
		//Invert colors
		public static void Invert(this Texture2D tex) {
			Color[] col = tex.GetPixels();
			for(int i = 0; i < col.Length; i++) {
				col[i] = _ColUtil.Invert(col[i]);
			}
			tex.SetPixels(col);
			tex.Apply();
		}

		//Fill with color
		public static void Fill(this Texture2D tex, Color color) {
			int size = tex.width * tex.height;
			Color[] col = new Color[size];
			for(int i = 0; i < size; i++) {
				col[i] = color;
			}
			tex.SetPixels(col);
			tex.Apply();
		}

		//Fill with mapped floats
		public static void Fill(this Texture2D tex, float[] map, float alpha = 1.0f) {
			int size = tex.width * tex.height;
			Color[] col = new Color[size];
			for(int i = 0; i < size; i++) {
				col[i] = new Color(map[i], map[i], map[i], alpha);
			}
			tex.SetPixels(col);
			tex.Apply();
		}

		//Resize texture2d with bilinear interpolation
		public static Texture2D Resample(this Texture2D tex, int width, int height, TextureFormat format, bool mipmaps) {
			Texture2D result = new Texture2D(width, height, format, mipmaps);
			int size = width * height;
			Color[] col = new Color[size];
			float x = (1f / tex.width) * ((float)tex.width / width);
			float y = (1f / tex.height) * ((float)tex.height / height);
			for(int i = 0; i < size; i++) {
				col[i] = tex.GetPixelBilinear(x * ((float)i % width), y * (Mathf.Floor((float)i / width)));
			}
			result.SetPixels(col);
			result.Apply();
			return result;
		}

		//Deforms a mesh with the given texture
		public static void DeformMesh(this Texture2D tex, Mesh mesh, float scale) {
			if(!tex.isReadable) {
				Debug.LogWarning("Texture is NOT read/write enabled!");
				return;
			}

			TextureWrapMode wrapMode = tex.wrapMode;
			tex.wrapMode = TextureWrapMode.Clamp;
			FilterMode filterMode = tex.filterMode;
			tex.filterMode = FilterMode.Point;

			Vector3[] vertices = mesh.vertices;
			Vector3[] normals = mesh.normals;
			Vector2[] uvs = mesh.uv;
			for(int v = 0; v < uvs.Length; v++) {
				float diff = tex.GetPixelBilinear(uvs[v].x, uvs[v].y).grayscale * 2.0f - 1.0f;
				vertices[v] += normals[v] * diff * scale;
			}
			mesh.vertices = vertices;
			mesh.RecalculateBounds();
			mesh.RecalculateNormals();
			_MeshUtil._SmoothNormals(mesh);
			mesh.RecalculateTangents();

			tex.wrapMode = wrapMode;
			tex.filterMode = filterMode;
		}

		//Deforms a terrain with the given texture
		public static void DeformTerrain(this Texture2D tex, Terrain terrain) {
			if(!tex.isReadable) {
				Debug.LogWarning("Texture is NOT read/write enabled!");
				return;
			}

			TextureWrapMode wrapMode = tex.wrapMode;
			tex.wrapMode = TextureWrapMode.Clamp;
			FilterMode filterMode = tex.filterMode;
			tex.filterMode = FilterMode.Point;
			//tex.Apply();

			TerrainData terData = terrain.terrainData;
			float[,] heights = new float[terData.heightmapResolution, terData.heightmapResolution];

			for(int y=0; y < terData.heightmapResolution; y++) {
				for(int x=0; x < terData.heightmapResolution; x++) {
					//heights[x,y] = tex.GetPixelBilinear((float)y / (terData.heightmapResolution+1), (float)x / (terData.heightmapResolution+1)).grayscale;
					heights[x,y] = tex.GetPixelBilinear((float)y / (terData.heightmapResolution-1), (float)x / (terData.heightmapResolution-1)).grayscale;
				}
			}
			terData.SetHeights(0, 0, heights);

			tex.wrapMode = wrapMode;
			tex.filterMode = filterMode;
			//tex.Apply();

			float terrainHalfHeight = terData.size.y * 0.5f;
			float fraction = terrainHalfHeight / 256; //Half height / 256 color palette
			float terrainPosition = terrainHalfHeight + fraction;
			Debug.Log("Terrain position Y should be: " + -terrainPosition);
		}

		//Deforms a terrain with the given texture
		public static void DeformTerrainAdditive(this Texture2D tex, Terrain terrain) {
			if(!tex.isReadable) {
				Debug.LogWarning("Texture is NOT read/write enabled!");
				return;
			}

			TextureWrapMode wrapMode = tex.wrapMode;
			tex.wrapMode = TextureWrapMode.Clamp;
			FilterMode filterMode = tex.filterMode;
			tex.filterMode = FilterMode.Point;
			//tex.Apply();

			TerrainData terData = terrain.terrainData;
			float[,] heights = terData.GetHeights(0, 0, terData.heightmapResolution, terData.heightmapResolution);

			for(int y=0; y < terData.heightmapResolution; y++) {
				for(int x=0; x < terData.heightmapResolution; x++) {
					//heights[x,y] = tex.GetPixelBilinear((float)y / (terData.heightmapResolution+1), (float)x / (terData.heightmapResolution+1)).grayscale;
					heights[x,y] += tex.GetPixelBilinear((float)y / (terData.heightmapResolution-1), (float)x / (terData.heightmapResolution-1)).grayscale;
				}
			}
			terData.SetHeights(0, 0, heights);

			tex.wrapMode = wrapMode;
			tex.filterMode = filterMode;
			//tex.Apply();

			float terrainHalfHeight = terData.size.y * 0.5f;
			float fraction = terrainHalfHeight / 256; //Half height / 256 color palette
			float terrainPosition = terrainHalfHeight + fraction;
			Debug.Log("Terrain position Y should be: " + -terrainPosition);
		}

#if UNITY_EDITOR
		//Save texture2d to disc
		public static void Save(this Texture2D tex, string name = "New Texture2D") {
			string file = EditorUtility.SaveFilePanelInProject("Save Texture2D", name, "png", "Please enter a file name");
			//string file = AssetDatabase.GenerateUniqueAssetPath("Assets/New Texture.png");
			if(file.Length != 0) {
				byte[] bytes = tex.EncodeToPNG();
				File.WriteAllBytes(file, bytes);
				AssetDatabase.Refresh();
				Debug.Log("Texture2D is saved at location:" + file);
			}
		}

		//Save texture3d to disc
		public static void Save(this Texture3D tex, string name = "New Texture3D") {
			string file = EditorUtility.SaveFilePanelInProject("Save Texture3D", name, "asset", "Please enter a file name");
			//string file = AssetDatabase.GenerateUniqueAssetPath("Assets/New Texture3D.asset");
			if(file.Length != 0) {
				AssetDatabase.CreateAsset(tex, file);
				AssetDatabase.Refresh();
				Debug.Log("Texture3D is saved at location:" + file);
			}
		}
#endif
	}

}