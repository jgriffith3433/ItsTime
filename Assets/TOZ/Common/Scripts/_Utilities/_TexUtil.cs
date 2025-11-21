using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TOZ {

	public static class _TexUtil {
		//Get the gradient as a Texture2D
		public static Texture2D _GetTextureFromGradient(int width, int height, Gradient gradient) {
			Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
			tex.filterMode = FilterMode.Bilinear;
			tex.wrapMode = TextureWrapMode.Clamp;
			float rate = 1.0f / (tex.width - 1);
			Color[] cols = new Color[tex.width * tex.height];
			int i = 0;
			for(int y = 0; y < tex.height; y++) {
				for(int x = 0; x < tex.width; x++) {
					float t = x * rate;
					cols[i] = gradient.Evaluate(t);
					i++;
				}
			}
			tex.SetPixels(cols);
			tex.Apply();
			return tex;
		}

		//Extract specific side of a cubemap
		public static Texture2D _ExtractCubemapFace(Cubemap cubeMap, CubemapFace face) {
			Color[] cols = cubeMap.GetPixels(face);
			Texture2D tex = new Texture2D(cubeMap.width, cubeMap.height, TextureFormat.ARGB32, true);
			tex.SetPixels(cols);
			tex.Apply();
			return tex;
		}

		//Extract terrains heightmap
		public static void ExtractHeightmap(Terrain terrain, float alpha = 1.0f) {
			TerrainData terData = terrain.terrainData;
			float[,] map = terData.GetHeights(0, 0, terData.heightmapResolution, terData.heightmapResolution);
			Texture2D tex = new Texture2D(terData.heightmapResolution, terData.heightmapResolution, TextureFormat.ARGB32, false);
			for(int y=0; y < tex.height; y++) {
				for(int x=0; x < tex.width; x++) {
					tex.SetPixel(x, y, new Color(map[y, x], map[y, x], map[y, x], alpha));
				}
			}
			tex.Apply();
			#if UNITY_EDITOR
			tex.Save();
			#endif
		}

		//Extract terrains splatmap
		public static void ExtractSplatmaps(Terrain terrain) {
			TerrainData terData = terrain.terrainData;
			for(int i = 0; i < terData.alphamapTextureCount; i++) {
				Texture2D tex = terData.alphamapTextures[i];
				#if UNITY_EDITOR
				tex.Save();
				#endif
			}
		}

		//Extract terrains holesmap
		public static void ExtractHolesmap(Terrain terrain) {
			TerrainData terData = terrain.terrainData;
			bool[,] map = terData.GetHoles(0, 0, terData.holesResolution, terData.holesResolution);
			Texture2D tex = new Texture2D(terData.holesResolution, terData.holesResolution, TextureFormat.ARGB32, false);
			for(int y=0; y < tex.height; y++) {
				for(int x=0; x < tex.width; x++) {
					float i = map[y, x] ? 1f : 0f;
					tex.SetPixel(x, y, new Color(i, i, i, 1.0f));
				}
			}
			tex.Apply();
			#if UNITY_EDITOR
			tex.Save();
			#endif
		}

		//Extract terrains basemap
		public static void ExtractBasemap(Terrain terrain) {
			TerrainData terData = terrain.terrainData;
			if(terData != null) {
				Bounds bound = terData.bounds;
				
				GameObject go = new GameObject("RenderCamera", typeof(Camera));
				go.name = "RenderCamera" + go.GetInstanceID();
				go.hideFlags = HideFlags.HideAndDontSave;

				Camera cam = go.GetComponent<Camera>();
				cam.CopyFrom(Camera.main);
				cam.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
				cam.transform.position = terrain.transform.position + (bound.center + (Vector3.up * (bound.extents.y + 1)));
				cam.clearFlags = CameraClearFlags.Depth;
				cam.backgroundColor = Color.clear;
				cam.orthographic = true;
				cam.orthographicSize = (bound.extents.x > bound.extents.y) ? bound.extents.x : bound.extents.y;
				cam.nearClipPlane = 1;
				cam.farClipPlane = 2 + bound.size.y;
				cam.depth = Camera.main.depth + 1;
				cam.useOcclusionCulling = false;
				cam.aspect = 1;

				Material mat = new Material(Shader.Find("TOZ/Terrain/Splat/Basemap"));
				mat.hideFlags = HideFlags.HideAndDontSave;
				Material cachedMaterial = terrain.materialTemplate;
				terrain.materialTemplate = mat;
				RenderTexture renderTex = new RenderTexture(terData.baseMapResolution, terData.baseMapResolution, 16, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
				renderTex.hideFlags = HideFlags.HideAndDontSave;
				cam.targetTexture = renderTex;
				cam.Render();
				terrain.materialTemplate = cachedMaterial;
				UnityEngine.Object.DestroyImmediate(mat);

				RenderTexture.active = renderTex;
				Texture2D texture = new Texture2D(terData.baseMapResolution, terData.baseMapResolution, TextureFormat.ARGB32, true);
				texture.ReadPixels(new Rect(0, 0, terData.baseMapResolution, terData.baseMapResolution), 0, 0, true);
				texture.Apply();

				RenderTexture.active = null;
				cam.targetTexture = null;
				UnityEngine.Object.DestroyImmediate(renderTex);
				UnityEngine.Object.DestroyImmediate(go);

				#if UNITY_EDITOR
				texture.Save();
				#endif
			}
		}

		//Return a copy of the texture2d
		public static Texture2D CopyTexture2D(Texture2D tex) {
			Texture2D texCopy = new Texture2D(tex.width, tex.height, TextureFormat.ARGB32, true);
			texCopy.SetPixels(tex.GetPixels());
			texCopy.Apply();
			return texCopy;
		}
	}

}