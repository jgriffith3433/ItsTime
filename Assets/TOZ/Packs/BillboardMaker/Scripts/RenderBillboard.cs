using System;
//using System.IO;
using UnityEngine;

namespace TOZ {

	public class RenderBillboard : MonoBehaviour {
#region Variables
		//Components
		[SerializeField] private GameObject objectToRender;
		[SerializeField] private int imageSize = 512;
#endregion Variables

#region Methods
		public Texture2D Render() {
			if(objectToRender == null) {
				Debug.LogWarning("You must choose the object to render!");
				return null;
			}

			MeshRenderer[] meshRenderers = objectToRender.GetComponentsInChildren<MeshRenderer>(false);
			if(meshRenderers != null && meshRenderers.Length > 0) {
				Bounds bound = meshRenderers[0].bounds;
				for(int i = 0; i < meshRenderers.Length; i++) {
					bound.Encapsulate(meshRenderers[i].bounds);
				}

				GameObject go = new GameObject("RenderCamera", typeof(Camera));
				go.name = "RenderCamera" + go.GetInstanceID();
				go.hideFlags = HideFlags.HideAndDontSave;

				Camera cam = go.GetComponent<Camera>();
				cam.CopyFrom(Camera.main);
				cam.transform.rotation = Quaternion.identity;
				cam.transform.position = bound.center - (Vector3.forward * (bound.extents.z + 1));
				cam.clearFlags = CameraClearFlags.Depth;
				cam.backgroundColor = Color.clear;
				cam.orthographic = true;
				cam.orthographicSize = (bound.extents.x > bound.extents.y) ? bound.extents.x : bound.extents.y;
				cam.nearClipPlane = 1;
				cam.farClipPlane = 1 + bound.size.z;
				cam.depth = Camera.main.depth + 1;
				cam.useOcclusionCulling = false;
				cam.aspect = 1;

				RenderTexture renderTex = new RenderTexture(imageSize, imageSize, 16, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
				renderTex.hideFlags = HideFlags.HideAndDontSave;
				cam.targetTexture = renderTex;
				cam.Render();

				RenderTexture.active = renderTex;
				Texture2D texture = new Texture2D(imageSize, imageSize, TextureFormat.ARGB32, true);
				texture.ReadPixels(new Rect(0, 0, imageSize, imageSize), 0, 0, true);
				texture.Apply();

				RenderTexture.active = null;
				cam.targetTexture = null;
				DestroyImmediate(renderTex);
				DestroyImmediate(go);

				return texture;
			}

			return null;
		}
#endregion Methods
	}

}