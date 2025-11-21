using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TOZ {

	[ExecuteInEditMode, RequireComponent(typeof(Camera))]
	public sealed class CameraDepthAndNormalsTexture : MonoBehaviour {
#region Variables
		//Components
		private Camera cam;

		//Variables
		[SerializeField] private DepthTextureMode depthMode;
#endregion Variables

#region Mono Methods
#if UNITY_EDITOR
		private void OnValidate() {
			if(cam == null)
				Awake();
			else
				UpdateComponentsAndVariables();
		}
#endif
		private void Awake() {
			cam = GetComponent<Camera>();
			UpdateComponentsAndVariables();
		}
#endregion Mono Methods
		
#region Helper Methods
		private void UpdateComponentsAndVariables() {
			cam.depthTextureMode = depthMode;
		}
#endregion Helper Methods
	}

}