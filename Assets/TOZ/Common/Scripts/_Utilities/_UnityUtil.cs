using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TOZ {

	public static class _UnityUtil {
		public static float _GetFogClippingDistance() {
			float distance = Camera.main.farClipPlane;

			if(RenderSettings.fog == true) {
				if(RenderSettings.fogMode.Is(FogMode.Linear)) {
					distance = RenderSettings.fogEndDistance;
				}

				if(RenderSettings.fogMode.Is(FogMode.Exponential)) {
					//Mathf.Log(1f / 0.0019f) = 2.721246399f;
					distance = (2.721246399f / RenderSettings.fogDensity);
				}

				if(RenderSettings.fogMode.Is(FogMode.ExponentialSquared)) {
					//Mathf.Sqrt(Mathf.Log(1f / 0.0019f)) = 1.6496200772f;
					distance = (1.6496200772f / RenderSettings.fogDensity);
				}
			}

			return distance;
		}

		public static float _GetFogDensityForDistance(float distance, float maxDistance = 4096f) {
			float density = 0.0f;
			float tolerance = 0.0f;

			if(RenderSettings.fog == true) {
				if(RenderSettings.fogMode.Is(FogMode.Linear)) {
					density = _MathUtil._Saturate((distance - RenderSettings.fogStartDistance) / (RenderSettings.fogEndDistance - RenderSettings.fogStartDistance));
				}

				if(RenderSettings.fogMode.Is(FogMode.Exponential)) {
					//Mathf.Log(1f / 0.0019f) = 2.721246399f;
					float maxDiv = 2.721246399f;
					float minDiv = 3.3f - maxDiv; //0.578753601f
					tolerance = maxDiv / (maxDistance - (maxDistance * minDiv));
					
					density = maxDiv / (distance - (distance * minDiv));
					if(density <= tolerance) density = 0f;
					if(density > 1.0f) density = 1f;
					density = Mathf.Clamp(density, 0f, 1f);
					
					//density = 2.721246399f / distance;
				}

				if(RenderSettings.fogMode.Is(FogMode.ExponentialSquared)) {
					//Mathf.Sqrt(Mathf.Log(1f / 0.0019f)) = 1.6496200772f;
					float maxDiv = 1.6496200772f;
					float minDiv = 2.0f - maxDiv; //0.3503799228f
					tolerance = maxDiv / (maxDistance - (maxDistance * minDiv));
					
					density = maxDiv / (distance - (distance * minDiv));
					if(density <= tolerance) density = 0f;
					if(density > 1.0f) density = 1f;
					density = Mathf.Clamp(density, 0f, 1f);
				}
			}

			return density;
		}
	}

}