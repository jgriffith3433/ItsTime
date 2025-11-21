using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TOZ;

namespace TOZ.Celestial {

	public abstract class StarBase : CelestialBase {
#region Properties
		public Color Color {
			get { return compLight.color; }
			private set { compLight.color = compFlare.color = value; }
		}
		public float Intensity {
			get { return compLight.intensity; }
			private set { compLight.intensity = compFlare.brightness = value; }
		}
		public Color FinalColor {
			get {
				Vector3 c = new Vector3(Color.r, Color.g, Color.b) * Intensity;
				return new Color(c.x, c.y, c.z, Color.a);
			}
		}
		public float ShadowStrength {
			get { return compLight.shadowStrength; }
			private set { compLight.shadowStrength = Mathf.Clamp(value, 0f, 1f); }
		}
#endregion Properties

#region Variables
		//Components
		private Transform trsGround;
		private Transform trsLight;
		private Light compLight;
		private Transform trsFlare;
		private LensFlare compFlare;

		//Cached Variables
		protected Color cachedAmbientColor;
		protected Color cachedSunColor;
		protected float cachedSunIntensity;
		protected float cachedShadowDistance;
		protected Color cachedFogColor;
		protected float cachedFogDistance;
		protected float cachedCamNearClipPlane;
		protected float cachedCamFarClipPlane;

		//Variables
		[Header("Ground Settings")]
		[SerializeField] private Flare flareAsset = null;

		[Header("Environment Settings")]
		[SerializeField] private Color ambientColorInSpace = new Color32(0, 0, 0, 255);
		[Range(0.5f, 1.5f)]
		[SerializeField] private float lightIntensityInSpace = 1.0f; // Sun Power
		[Range(0f, 65535f)]
		[SerializeField] private float clipDistanceInSpace = 1000f;
#endregion Variables

#region Mono Methods
		protected virtual void Awake() {
			DoAwake();
		}

		protected override void OnEnable() {
			base.OnEnable();
			DoEnable();
		}

		protected override void OnDisable() {
			base.OnDisable();
			DoDisable();
		}
#endregion Mono Methods

#region Methods
		private void DoAwake() {
			this.Type = CelestialType.Sun;

			trsGround = this.Trs.GetChild(0);
			trsLight = this.Trs.GetChild(1);
			compLight = trsLight.gameObject.GetComponent<Light>();
			trsFlare = this.Trs.GetChild(2);
			compFlare = trsFlare.gameObject.GetComponent<LensFlare>();
		}

		private void DoEnable() {
			//Adjust sun properties
			trsGround.localScale = Vector3.one * (this.Radius * 2f);
			trsGround.GetComponent<MeshRenderer>().sharedMaterial.SetColor("_Color", compLight.color);
			compFlare.flare = flareAsset;

			//Cache environment values
			CacheEnvironment();
			
			Debug.Log(this);
		}

		private void DoDisable() {
			//Reset sun properties
			trsGround.localScale = Vector3.one;
			trsGround.GetComponent<MeshRenderer>().sharedMaterial.SetColor("_Color", cachedSunColor);
			compFlare.flare = null;

			//Decache environment values
			DecacheEnvironment();
		}
#endregion Methods

#region Helper Methods
		private void CacheEnvironment() {
			cachedAmbientColor = RenderSettings.ambientLight;
			cachedSunColor = compLight.color;
			cachedSunIntensity = compLight.intensity;
			CalculateBrightness(cachedSunIntensity);
			cachedShadowDistance = QualitySettings.shadowDistance;
			cachedFogColor = RenderSettings.fogColor;
			cachedFogDistance = RenderSettings.fogEndDistance;
			cachedCamNearClipPlane = _EYES._FrustumCam.nearClipPlane;
			cachedCamFarClipPlane = _EYES._FrustumCam.farClipPlane;
		}

		private void DecacheEnvironment() {
			RenderSettings.ambientLight = cachedAmbientColor;
			compLight.color = cachedSunColor;
			CalculateBrightness(cachedSunIntensity);
			QualitySettings.shadowDistance = cachedShadowDistance;
			RenderSettings.fogColor = cachedFogColor;
			RenderSettings.fogEndDistance = cachedFogDistance;
			_EYES._SetNearDistance(cachedCamNearClipPlane);
			_EYES._SetMiddleDistance(cachedCamFarClipPlane);
		}

		private void CalculateBrightness(float intensity = 1.0f) {
			compLight.intensity = intensity;
			_SS._LightPower = compLight.intensity * 50f; // 25 < Range < 75
			compFlare.brightness = (_SS._LightPower + 25f) / 100f; // 0.5 < Range < 1.0
			Shader.SetGlobalColor("_StarColor", compLight.color);
		}

		protected void GetClosestCelestialToEye() {
			_EYES._ClosestCelestial = null;
			float closestCelestialDistance = Mathf.Infinity;
			for(int i = 0; i < CelestialBase._List.Count(); i++) {
				CelestialBase._List[i].UpdateCelestial();

				if(CelestialBase._List[i].Type.Is(CelestialType.Planet)) {
					Planet planet = CelestialBase._List[i] as Planet;
					planet.UpdatePlanet();
				}

				if(CelestialBase._List[i].NavigationDistToEye < closestCelestialDistance) {
					closestCelestialDistance = CelestialBase._List[i].NavigationDistToEye;
					_EYES._ClosestCelestial = CelestialBase._List[i];
				}
			}

			_EYES._ClosestPlanet = null;
			_EYES._InNavigationZone = false;
			_EYES._InGravityZone = false;
			_EYES._InAtmosphere = false;
			_EYES._InClouds = false;
			_EYES._InWater = false;
			if(_EYES._ClosestCelestial != null) {
				if(_EYES._ClosestCelestial.Type.Is(CelestialType.Planet)) {
					_EYES._ClosestPlanet = _EYES._ClosestCelestial as Planet;
				}

				if(_EYES._ClosestCelestial.NavigationDistToEye <= 0f) {
					_EYES._InNavigationZone = true;
					if(_EYES._ClosestCelestial.OrbitDistToEye <= 0f) {
						_EYES._InGravityZone = true;
						if(_EYES._ClosestCelestial.AtmosphereDistToEye <= 0f) {
							_EYES._InAtmosphere = true;
							if(_EYES._ClosestCelestial.CloudsDistToEye <= 0f) {
								_EYES._InClouds = true;
								if(_EYES._InClouds) {
									if(_EYES._ClosestPlanet != null && _EYES._ClosestPlanet.HasWater) {
										int layerMask = 1 << 4;
										if(!Physics.Raycast(_EYES._Head.position, -_EYES._ClosestPlanet.DirToEyeNormalized, _EYES._ClosestPlanet.CoreDistToEye, layerMask, QueryTriggerInteraction.Ignore)) {
											_EYES._InWater = true;
										}
									}
								}
							}
						}
					}
				}
			}

			//Adjust directional light to fake as a spot light
			if(_EYES._InNavigationZone && _EYES._ClosestCelestial.Type.IsNot(CelestialType.Sun)) {
				trsLight.LookAt(_EYES._ClosestCelestial.PosW);
			}
			else {
				trsLight.LookAt(_EYES._Head.position);
			}
		}

		protected virtual void SetEnvironmentToInitial() {
			RenderSettings.ambientLight = ambientColorInSpace;
			Color = cachedSunColor;
			CalculateBrightness(lightIntensityInSpace);
			RenderSettings.fogColor = ambientColorInSpace;

			float dist = clipDistanceInSpace;
			_EYES._SetNearDistance(0.3f);
			_EYES._SetMiddleDistance(dist);
			RenderSettings.fogEndDistance = dist;
			QualitySettings.shadowDistance = dist;
		}

		protected virtual void SetEnvironmentToSpace() {
			RenderSettings.ambientLight = ambientColorInSpace;
			//Color = cachedSunColor;
			CalculateBrightness(lightIntensityInSpace);
			RenderSettings.fogColor = ambientColorInSpace;

			float dist = Mathf.Min(_EYES._ClosestCelestial.CoreDistToEye, clipDistanceInSpace);
			_EYES._SetNearDistance(0.3f);
			_EYES._SetMiddleDistance(dist);
			RenderSettings.fogEndDistance = dist;
			QualitySettings.shadowDistance = dist * 0.5f;
			_EYES._SetCullingDistance(clipDistanceInSpace);
		}

		protected virtual void SetEnvironmentToOrbital(float sdotc) {
			RenderSettings.ambientLight = Color.Lerp(ambientColorInSpace, Color.Lerp(ambientColorInSpace, _EYES._ClosestPlanet.DayAmbient, sdotc), _InterpolationUtil._Grade(0f, 1f, (_EYES._ClosestPlanet.AtmosphereThickness * 4.0f), 0f, _EYES._ClosestPlanet.AtmosphereDistToEye));
			//Color = cachedSunColor + Color.Lerp(ambientColorInSpace, Color.Lerp(ambientColorInSpace, _EYES._ClosestPlanet.FogColorAboveWater, sdotc), _InterpolationUtil._Grade(0f, 1f, (_EYES._ClosestPlanet.AtmosphereThickness * 4.0f), 0f, _EYES._ClosestPlanet.AtmosphereDistToEye));
			CalculateBrightness(lightIntensityInSpace * _InterpolationUtil._Grade(1.0f, 0.65f, (_EYES._ClosestPlanet.AtmosphereThickness * 4.0f), 0f, _EYES._ClosestPlanet.AtmosphereDistToEye));
			RenderSettings.fogColor = Color.Lerp(ambientColorInSpace, Color.Lerp(ambientColorInSpace, _EYES._ClosestPlanet.FogColorAboveWater, sdotc), _InterpolationUtil._Grade(0f, 1f, (_EYES._ClosestPlanet.AtmosphereThickness * 4.0f), 0f, _EYES._ClosestPlanet.AtmosphereDistToEye));

			float dist = _EYES._ClosestCelestial.CoreDistToEye;
			_EYES._SetNearDistance(0.3f);
			_EYES._SetMiddleDistance(dist);
			float distF = _InterpolationUtil._Grade(_EYES._ClosestCelestial.CoreDistToEye, _EYES._ClosestPlanet.LODDistances[0], (_EYES._ClosestPlanet.AtmosphereThickness * 4.0f), 0f, _EYES._ClosestPlanet.AtmosphereDistToEye);
			RenderSettings.fogEndDistance = distF * 0.8f;
			QualitySettings.shadowDistance = dist;
		}

		protected virtual void SetEnvironmentToAtmosphere(float sdotc) {
			RenderSettings.ambientLight = Color.Lerp(ambientColorInSpace, _EYES._ClosestPlanet.DayAmbient, sdotc);
			//Color = cachedSunColor + _EYES._ClosestPlanet.FogColorAboveWater;
			CalculateBrightness(lightIntensityInSpace * 0.65f);
			RenderSettings.fogColor = Color.Lerp(ambientColorInSpace, _EYES._ClosestPlanet.FogColorAboveWater, sdotc);

			float distN = _InterpolationUtil._Grade(0.3f, 0.02f, _EYES._ClosestPlanet.CloudsThickness, 0f, _EYES._ClosestPlanet.SeaDistToEye);
			_EYES._SetNearDistance(distN);
			float distF = _InterpolationUtil._Grade(_EYES._ClosestPlanet.LODDistances[0], _EYES._ClosestPlanet.LODDistances[_LOD._Max-1] * 0.5f, _EYES._ClosestPlanet.AtmosphereThickness, 0f, _EYES._ClosestPlanet.SeaDistToEye);
			_EYES._SetMiddleDistance(distF);

			RenderSettings.fogEndDistance = distF * 0.8f;
			QualitySettings.shadowDistance = distF;
		}

		protected virtual void SetEnvironmentToUnderWater() {
			RenderSettings.ambientLight = ambientColorInSpace;
			//Color = _EYES._ClosestPlanet.FogColorBelowWater;
			CalculateBrightness(lightIntensityInSpace * 0.35f);
			RenderSettings.fogColor = Color.Lerp(ambientColorInSpace, _EYES._ClosestPlanet.FogColorBelowWater, _InterpolationUtil._Grade(0f, 1f, 0f, _EYES._ClosestPlanet.GroundThicknessHalf, _EYES._ClosestPlanet.FloorDistToEye));

			float dist = _EYES._ClosestPlanet.LODDistances[_LOD._Max-1];
			_EYES._SetNearDistance(0.02f);
			_EYES._SetMiddleDistance(dist);
			RenderSettings.fogEndDistance = dist * 0.05f;
			QualitySettings.shadowDistance = dist * 0.05f;
		}
#endregion Helper Methods

#region Object Methods
		//Overrides//
		public override string ToString() {
			return "StarBase Index: {0}, Name: {1}".FormatWith(this.Index, this.Name);
		}
#endregion Object Methods
	}

}