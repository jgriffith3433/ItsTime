using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TOZ;

namespace TOZ.Celestial {

	public sealed class FarCamera : MonoBehaviour {
		private float cachedFogDistance;
		private ShadowQuality cachedShadows;

#region Mono Methods
		private void OnPreRender() {
			DoPreRender();
		}

		private void OnPostRender() {
			DoPostRender();
		}
#endregion Mono Methods

#region Methods
		private void DoPreRender() {
			cachedShadows = QualitySettings.shadows;
			QualitySettings.shadows = ShadowQuality.Disable;
			cachedFogDistance = RenderSettings.fogEndDistance;
			RenderSettings.fogEndDistance = 0f;
		}

		private void DoPostRender() {
			QualitySettings.shadows = cachedShadows;
			RenderSettings.fogEndDistance = cachedFogDistance;
		}
#endregion Methods
	}

}