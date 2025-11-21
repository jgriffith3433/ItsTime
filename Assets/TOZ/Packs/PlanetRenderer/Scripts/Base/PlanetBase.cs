using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TOZ;

namespace TOZ.Celestial {

	public abstract class PlanetBase : PlanetoidBase {
#region Properties
		public Color DayAmbient { get { return zenithColor; } }
#endregion Properties

#region Variables
		//Variables
		[Header("Feature Settings")]
		public bool HasGround = true;
		public bool HasWater = false;
		public bool HasClouds = false;
		public bool HasAtmosphere = false;
		public bool HasNature = false;
		public bool HasWildLife = false;
		public bool HasStation = false;

		[Header("Shading Settings")]
		[SerializeField] protected PlanetShaderType planetShader = PlanetShaderType.Gradient;
		public Gradient ControlGradient;
		internal Texture2D ControlTexture;

		[Header("Ground Settings")]
		[SerializeField] private Texture2D detail0Texture = null;
		[Range(0f, 16f)]
		[SerializeField] private float detail0TextureScale = 1f;
		[SerializeField] private Texture2D detail1Texture = null;
		[Range(0f, 16f)]
		[SerializeField] private float detail1TextureScale = 1f;
		[SerializeField] private Texture2D splat0Texture = null;
		[Range(0f, 16f)]
		[SerializeField] private float splat0TextureScale = 1f;
		[SerializeField] private Texture2D splat1Texture = null;
		[Range(0f, 16f)]
		[SerializeField] private float splat1TextureScale = 1f;
		[SerializeField] private Texture2D splat2Texture = null;
		[Range(0f, 16f)]
		[SerializeField] private float splat2TextureScale = 1f;
		[SerializeField] private Texture2D splat3Texture = null;
		[Range(0f, 16f)]
		[SerializeField] private float splat3TextureScale = 1f;
		[SerializeField] private Texture2D albedoTexture = null;
		private Shader shdGroundFromSpace;
		private Shader shdGroundFromPlanet;
		internal Material GroundMaterial;

		[Header("Water Settings")]
		[SerializeField] private Cubemap waterReflectionTexture = null;
		[SerializeField] private Texture2D waterBumpTexture = null;
		[Range(0f, 16f)]
		[SerializeField] private float waterBumpTextureScale = 2f;
		[Range(0f, 1f)]
		[SerializeField] private float wavePower = 0.5f;
		[Range(0f, 0.1f)]
		[SerializeField] private float waveSpeed = 0.05f;
		[Range(0f, 10f)]
		[SerializeField] private float fresnelPower = 2.5f;
		[Range(0f, 10f)]
		[SerializeField] private float fresnelCutoff = 5.0f;
		[Range(0f, 1f)]
		[SerializeField] private float shoreThickness = 0.01f;
		[Range(0.0001f, 1f)]
		[SerializeField] private float shallowThickness = 0.5f;
		[Range(0.1f, 120f)]
		[SerializeField] private float waterShininess = 50f;
		private Shader shdWaterFromSpace;
		private Shader shdWaterFromPlanet;
		private Shader shdWaterFromBelow;
		internal Material WaterMaterial;

		[Header("Clouds Settings")]
		[SerializeField] private Texture2D cloudsTexture = null;
		private Shader shdCloudsFromSpace;
		private Shader shdCloudsFromPlanet;
		internal Material CloudsMaterial;

		[Header("Atmosphere Settings")]
		//[SerializeField] internal Color DayAmbient = new Color32(128, 128, 128, 255);
		[SerializeField] private Color zenithColor = new Color32(121, 145, 166, 255);
		[Range(0.0f, 1.0f)]
		[SerializeField] private float averageDensity = 0.160f;
		[Range(0.0f, 0.2f)]
		[SerializeField] private float skyDensity = 0.005f;
		[Range(0.0f, 0.2f)]
		[SerializeField] private float sunSize = 0.002f;
		[Range(-0.985f, -0.995f)]
		[SerializeField] private float sunBlur = -0.990f;
		private Shader shdAtmosphereFromSpace;
		private Shader shdAtmosphereFromPlanet;
		internal Material AtmosphereMaterial;

		[Header("Environment Settings")]
		[Range(0f, 25f)]
		[SerializeField] private float fogDistanceBelowWater = 2.5f;
		internal Color FogColorAboveWater;
		internal float FogDensityAboveWater;
		internal Color FogColorBelowWater;
		internal float FogDensityBelowWater;
		private Vector3 invWaveLength;
		private Color waterColor;
		private Color shallowColor;
#endregion Variables

#region Mono Methods
		protected override void Awake() {
			base.Awake();
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
			this.Type = CelestialType.Planet;

			GetEnvironmentVariables();

			switch(planetShader) {
				case PlanetShaderType.Gradient:
					shdGroundFromSpace = Shader.Find("TOZ/Celestial/DetailFromSpace");
					shdGroundFromPlanet = Shader.Find("TOZ/Celestial/DetailFromPlanet");
				break;
				case PlanetShaderType.Bumped:
					shdGroundFromSpace = Shader.Find("TOZ/Celestial/BumpedFromSpace");
					shdGroundFromPlanet = Shader.Find("TOZ/Celestial/BumpedFromPlanet");
				break;
				case PlanetShaderType.Splat:
					shdGroundFromSpace = Shader.Find("TOZ/Celestial/SplatFromSpace");
					shdGroundFromPlanet = Shader.Find("TOZ/Celestial/SplatFromPlanet");
				break;
				case PlanetShaderType.Albedo:
					shdGroundFromSpace = Shader.Find("TOZ/Celestial/AlbedoFromSpace");
					shdGroundFromPlanet = Shader.Find("TOZ/Celestial/AlbedoFromPlanet");
				break;
			}

			shdWaterFromSpace = Shader.Find("TOZ/Celestial/WaterFromSpace");
			shdWaterFromPlanet = Shader.Find("TOZ/Celestial/WaterFromPlanet");
			shdWaterFromBelow = Shader.Find("TOZ/Celestial/WaterFromBelow");

			shdCloudsFromSpace = Shader.Find("TOZ/Celestial/CloudsFromSpace");
			shdCloudsFromPlanet = Shader.Find("TOZ/Celestial/CloudsFromPlanet");

			shdAtmosphereFromSpace = Shader.Find("TOZ/Celestial/AtmosphereFromSpace");
			shdAtmosphereFromPlanet = Shader.Find("TOZ/Celestial/AtmosphereFromPlanet");
		}

		private void DoEnable() {
			CreateTextures();
		}

		private void DoDisable() {
			DeleteTextures();
		}
#endregion Methods

#region Helper Methods
		internal virtual void UpdatePlanet() {
			base.UpdatePlanetoid();

			//Update Grounds Material
			if(HasGround) {
				if(GroundMaterial != null) {
					GroundMaterial.shader = (this.CoreDistToEye < this.NavigationRadius) ? shdGroundFromPlanet : shdGroundFromSpace;
					UpdateGroundMaterial();
				}
			}

			//Update Waters Material
			if(HasWater) {
				if(WaterMaterial != null) {
					if(this.CoreDistToEye < this.NavigationRadius) {
						WaterMaterial.shader = shdWaterFromPlanet;
						if(this.CoreDistToEye < this.Radius) {
							WaterMaterial.shader = shdWaterFromBelow;
						}
					}
					else {
						WaterMaterial.shader = shdWaterFromSpace;
					}
					UpdateWaterMaterial();
				}
			}

			//Update Clouds Material
			if(HasClouds) {
				if(CloudsMaterial != null) {
					CloudsMaterial.shader = (this.CoreDistToEye < this.CloudsRadius) ? shdCloudsFromPlanet : shdCloudsFromSpace;
					UpdateCloudsMaterial();
				}
			}

			//Update Atmosphere Material
			if(HasAtmosphere) {
				if(AtmosphereMaterial != null) {
					AtmosphereMaterial.shader = (this.CoreDistToEye < this.AtmosphereRadius) ? shdAtmosphereFromPlanet : shdAtmosphereFromSpace;
					UpdateAtmosphereMaterial();
				}
			}

			/*CelestialBase target = this;
			if(this.Parent != null) {
				target = this.Parent;
			}*/
			if(this.Parent == null) {
				float distToScaleStart = this.Diameter * 2f;
				float distToScaleEnd = this.Diameter * 3f;
				float distToScale = distToScaleEnd - distToScaleStart;

				float dist = _MathUtil._Clamp(this.CoreDistToEye, distToScaleStart, distToScaleEnd);
				float rate = (dist - distToScaleStart) / distToScale;
				//Debug.Log(dist + " hehe " + " heh " + distToScale + " he " + rate);
				this.Scale = Vector3.Lerp(Vector3.one, Vector3.one * 0.1f, rate);
			}
			else {
				this.Scale = this.Parent.Scale;
			}
		}

		protected void CreateTextures() {
			//Control texture
			if(planetShader.Is(PlanetShaderType.Albedo)) {
				ControlTexture = albedoTexture;
			}
			else {
				ControlTexture = _TexUtil._GetTextureFromGradient(128, 4, ControlGradient);
				ControlTexture.wrapMode = TextureWrapMode.Clamp;
				ControlTexture.hideFlags = HideFlags.HideAndDontSave;
			}
		}

		protected void DeleteTextures() {
			if(ControlTexture != null && planetShader.IsNot(PlanetShaderType.Albedo))
				DestroyImmediate(ControlTexture, false);
		}

		protected void CreateGroundMaterial() {
			GroundMaterial = new Material(shdGroundFromPlanet);
			GroundMaterial.hideFlags = HideFlags.HideAndDontSave;
			GroundMaterial.enableInstancing = true;

			Vector2 planetaryUVScale = Vector2.one * TotalQuads;

			GroundMaterial.SetTexture("_ControlTex", ControlTexture);
			GroundMaterial.SetTexture("_Detail0Tex", detail0Texture);
			GroundMaterial.SetTextureScale("_Detail0Tex", planetaryUVScale * detail0TextureScale);
			switch(planetShader) {
				case PlanetShaderType.Gradient:
					GroundMaterial.SetTexture("_Detail1Tex", detail1Texture);
					GroundMaterial.SetTextureScale("_Detail1Tex", planetaryUVScale * detail1TextureScale);
				break;
				case PlanetShaderType.Bumped:
					GroundMaterial.SetTexture("_Detail1Tex", detail1Texture);
					GroundMaterial.SetTextureScale("_Detail1Tex", planetaryUVScale * detail1TextureScale);
					GroundMaterial.SetTexture("_BumpMap", this.DataMap);
				break;
				case PlanetShaderType.Splat:
					GroundMaterial.SetTexture("_Splat0", splat0Texture);
					GroundMaterial.SetTextureScale("_Splat0", planetaryUVScale * splat0TextureScale);
					GroundMaterial.SetTexture("_Splat1", splat1Texture);
					GroundMaterial.SetTextureScale("_Splat1", planetaryUVScale * splat1TextureScale);
					GroundMaterial.SetTexture("_Splat2", splat2Texture);
					GroundMaterial.SetTextureScale("_Splat2", planetaryUVScale * splat2TextureScale);
					GroundMaterial.SetTexture("_Splat3", splat3Texture);
					GroundMaterial.SetTextureScale("_Splat3", planetaryUVScale * splat3TextureScale);
				break;
				case PlanetShaderType.Albedo:
					GroundMaterial.SetTexture("_Detail1Tex", detail1Texture);
					GroundMaterial.SetTextureScale("_Detail1Tex", planetaryUVScale * detail1TextureScale);
				break;
			}
			UpdateGroundMaterial();
		}

		protected void UpdateGroundMaterial() {
			GroundMaterial.SetVector("_SunDirection", this.DirToSunNormalized);
		}

		protected void DeleteGroundMaterial() {
			if(GroundMaterial != null)
				DestroyImmediate(GroundMaterial, false);
		}

		protected void CreateWaterMaterial() {
			WaterMaterial = new Material(shdWaterFromPlanet);
			WaterMaterial.hideFlags = HideFlags.HideAndDontSave;
			//WaterMaterial.enableInstancing = true;

			Vector2 planetaryUVScale = Vector2.one * TotalQuads;

			WaterMaterial.SetColor("_Color", waterColor);
			WaterMaterial.SetColor("_ShallowColor", shallowColor);
			WaterMaterial.SetTexture("_WaveTex", waterBumpTexture);
			WaterMaterial.SetTextureScale("_WaveTex", planetaryUVScale * waterBumpTextureScale);
			WaterMaterial.SetTexture("_CubeTex", waterReflectionTexture);
			switch(planetShader) {
				case PlanetShaderType.Gradient:
				case PlanetShaderType.Bumped:
				case PlanetShaderType.Albedo:
					WaterMaterial.SetTexture("_RefractionTex", detail0Texture);
					WaterMaterial.SetTextureScale("_RefractionTex", planetaryUVScale * detail0TextureScale);
				break;
				case PlanetShaderType.Splat:
					WaterMaterial.SetTexture("_RefractionTex", splat0Texture);
					WaterMaterial.SetTextureScale("_RefractionTex", planetaryUVScale * splat0TextureScale);
				break;
			}
			UpdateWaterMaterial();
		}

		protected void UpdateWaterMaterial() {
			WaterMaterial.SetVector("_SunDirection", DirToSunNormalized);
			WaterMaterial.SetFloat("_WavePower", wavePower);
			WaterMaterial.SetFloat("_WaveSpeed", waveSpeed);
			WaterMaterial.SetFloat("_FresnelPower", fresnelPower);
			WaterMaterial.SetFloat("_FresnelCutOff", fresnelCutoff);
			WaterMaterial.SetFloat("_Shore", shoreThickness);
			WaterMaterial.SetFloat("_Shallow", shallowThickness);
			WaterMaterial.SetFloat("_Shininess", waterShininess);
		}

		protected void DeleteWaterMaterial() {
			if(WaterMaterial != null)
				DestroyImmediate(WaterMaterial, false);
		}

		protected void CreateCloudsMaterial() {
			CloudsMaterial = new Material(shdCloudsFromPlanet);
			CloudsMaterial.hideFlags = HideFlags.HideAndDontSave;
			//CloudsMaterial.enableInstancing = true;

			CloudsMaterial.SetTexture("_MainTex", cloudsTexture);
			UpdateCloudsMaterial();
		}

		protected void UpdateCloudsMaterial() {
			CloudsMaterial.SetVector("_SunDirection", this.DirToSunNormalized);
		}

		protected void DeleteCloudsMaterial() {
			if(CloudsMaterial != null)
				DestroyImmediate(CloudsMaterial, false);
		}

		protected void CreateAtmosphereMaterial() {
			AtmosphereMaterial = new Material(shdAtmosphereFromPlanet);
			AtmosphereMaterial.hideFlags = HideFlags.HideAndDontSave;
			//AtmosphereMaterial.enableInstancing = true;
			UpdateAtmosphereMaterial();
		}

		protected void UpdateAtmosphereMaterial() {
			AtmosphereMaterial.SetVector("_SunDirection", this.DirToSunNormalized);
			AtmosphereMaterial.SetVector("_CamPos", this.DirToEye);
			AtmosphereMaterial.SetFloat("_CamHeight", this.CoreDistToEye);
			AtmosphereMaterial.SetFloat("_InnerRadius", this.FloorRadius * this.Scale.x);
			AtmosphereMaterial.SetFloat("_OuterRadius", this.AtmosphereRadius * this.Scale.x);
			AtmosphereMaterial.SetFloat("_AverageDensity", averageDensity);
			AtmosphereMaterial.SetFloat("_SunPower", _SS._LightPower);
			AtmosphereMaterial.SetVector("_FlareDirection", -_SS._Star.DirToEyeNormalized);
			AtmosphereMaterial.SetVector("_InvWaveLength", invWaveLength);
			AtmosphereMaterial.SetFloat("_Kr", skyDensity);
			AtmosphereMaterial.SetFloat("_Km", sunSize);
			AtmosphereMaterial.SetFloat("_G", sunBlur);
		}

		protected void DeleteAtmosphereMaterial() {
			if(AtmosphereMaterial != null)
				DestroyImmediate(AtmosphereMaterial, false);
		}

		private void GetEnvironmentVariables() {
			float H, S, V;
			Color.RGBToHSV((Color)zenithColor, out H, out S, out V);
			H = Mathf.Repeat(H + 0.5f, 1.0f);
			Color horizonColor = Color.HSVToRGB(H, S, V);

			invWaveLength.x = 1.0f / Mathf.Pow(horizonColor.r, 1.6f);
			invWaveLength.y = 1.0f / Mathf.Pow(horizonColor.g, 1.6f);
			invWaveLength.z = 1.0f / Mathf.Pow(horizonColor.b, 1.6f);
			//FogColorAboveWater = Color.Lerp(zenithColor, horizonColor, _SS._Sun.Blend);
			FogColorAboveWater = new Vector4(invWaveLength.x, invWaveLength.y, invWaveLength.z, 1.0f).normalized;

			invWaveLength.x = 1.0f / Mathf.Pow(horizonColor.r, 4.0f);
			invWaveLength.y = 1.0f / Mathf.Pow(horizonColor.g, 4.0f);
			invWaveLength.z = 1.0f / Mathf.Pow(horizonColor.b, 4.0f);
			waterColor = new Vector4(invWaveLength.x, invWaveLength.y, invWaveLength.z, 1.0f).normalized;
			shallowColor = new Vector4(invWaveLength.x, invWaveLength.y * 2.0f, invWaveLength.z, 1.0f).normalized;
			FogColorBelowWater = new Color(waterColor.r * 0.5f, waterColor.g * 0.5f, waterColor.b * 0.5f, 1.0f);
			
			FogDensityAboveWater = _UnityUtil._GetFogDensityForDistance(LODDistances[_LOD._Max-1]);
			FogDensityBelowWater = _UnityUtil._GetFogDensityForDistance(fogDistanceBelowWater);
		}
#endregion Helper Methods

#region Object Methods
		//Overrides//
		public override string ToString() {
			return "PlanetBase Index: {0}, Name: {1}".FormatWith(this.Index, this.Name);
		}
#endregion Object Methods
	}

}