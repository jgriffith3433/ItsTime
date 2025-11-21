using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using TOZ;

namespace TOZ.Celestial {

	public sealed class Star : StarBase {
#region Variables
		private Material matSkyBox;
		private Material matNebula;

		//Cached Variables
		private float cachedSkyExposure;
		private float cachedNebulaDensity;

		//Variables
		[Header("Planet Defaults")]
		internal static Texture2D _DataMap = null;
		[SerializeField] private Texture2D dataMap = null;

		//LOD Variables
		[Header("LOD and NAV Settings")]
		[Range(0f, 5f)]
		[SerializeField] private float updateInterval = 0f;
		private float timer = 0f;
		public bool AreCelestialsDynamic = false;

		//Delegates and Events
		internal delegate void SunDelegate();
		internal event SunDelegate OnPreUpdate, OnPostUpdate;
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

		private void Update() {
			DoUpdate();
		}
#endregion Mono Methods

#region Methods
		private void DoAwake() {
			_DataMap = dataMap;

			_SS._Setup(this);
		}

		private void DoEnable() {
			//Initialize
			matSkyBox = RenderSettings.skybox;
			GameObject nebula = GameObject.Find("Nebula");
			if(nebula != null) {
				matNebula = nebula.GetComponent<MeshRenderer>().sharedMaterial;
			}

			//Cache environment values
			cachedSkyExposure = matSkyBox.GetFloat("_Exposure");
			cachedNebulaDensity = 1f;
			if(matNebula != null) {
				cachedNebulaDensity = matNebula.GetFloat("_Density");
			}

			//Set environment values
			SetEnvironmentToInitial();
		}

		private void DoDisable() {
			//Reset environment values
			matSkyBox.SetFloat("_Exposure", cachedSkyExposure);
			if(matNebula != null) {
				matNebula.SetFloat("_Density", cachedNebulaDensity);
			}

			matSkyBox = null;
			matNebula = null;

			//Cleanup
			_SS._Initialize();
		}

		private void DoUpdate() {
			timer += Time.deltaTime;
			if(timer >= updateInterval) {
				timer -= timer;

				if(OnPreUpdate != null) {
					OnPreUpdate();
				}

				//Update _SOLARSYSTEM
				_SS._Update();

				//Update celestials and planets and get the closest one
				GetClosestCelestialToEye();

				//Adjust space environment
				SetEnvironmentToSpace();

				//Work on closest planet
				//_EYES._InWater = false;
				if(_EYES._ClosestPlanet != null) {
					if(_EYES._InNavigationZone) {
						if(_EYES._ClosestPlanet.HasAtmosphere) {
							//float sdotc = Mathf.Clamp(_EYES._ClosestPlanet.SDotC, 0f, 1f);
							float sdotc = _InterpolationUtil._Grade(0.0f, 1.0f, -0.1f, 0.1f, _EYES._ClosestPlanet.SDotC);
							SetEnvironmentToOrbital(sdotc);
						}
					}
					if(_EYES._InAtmosphere) {
						_EYES._SetCullingDistance(_EYES._ClosestPlanet.LODDistances[_LOD._Max-1]);
						//Adjust planet environment
						if(_EYES._ClosestPlanet.HasAtmosphere) {
							float sdotc = _InterpolationUtil._Grade(0.0f, 1.0f, -0.1f, 0.1f, _EYES._ClosestPlanet.SDotC);
							SetEnvironmentToAtmosphere(sdotc);
						}

						//Adjust underwater environment
						if(_EYES._InWater) {
							SetEnvironmentToUnderWater();
						}
					}

					if(_EYES._ClosestPlanet.HasAtmosphere) {
						for(int i = 0; i < AsteroidField._Count(); i++) {
							if(AsteroidField._Data[i].Root.activeInHierarchy != (!_EYES._InAtmosphere))
								AsteroidField._Data[i].Root.SetActive(!_EYES._InAtmosphere);
						}
					}
				}

				if(OnPostUpdate != null) {
					OnPostUpdate();
				}
			}
		}
#endregion Methods

#region Helper Methods
		protected override void SetEnvironmentToInitial() {
			base.SetEnvironmentToInitial();
			if(matSkyBox != null) {
				matSkyBox.SetFloat("_Exposure", 1.0f);
			}
			if(matNebula != null) {
				matNebula.SetFloat("_Density", cachedNebulaDensity);
			}
		}

		protected override void SetEnvironmentToSpace() {
			base.SetEnvironmentToSpace();
			if(matSkyBox != null) {
				matSkyBox.SetFloat("_Exposure", 1.0f);
			}
			if(matNebula != null) {
				matNebula.SetFloat("_Density", cachedNebulaDensity);
			}
		}

		protected override void SetEnvironmentToOrbital(float sdotc) {
			base.SetEnvironmentToOrbital(sdotc);
			if(matSkyBox != null) {
				matSkyBox.SetFloat("_Exposure", 1.0f);
			}
			if(matNebula != null) {
				matNebula.SetFloat("_Density", cachedNebulaDensity);
			}
		}

		protected override void SetEnvironmentToAtmosphere(float sdotc) {
			base.SetEnvironmentToAtmosphere(sdotc);
			if(matSkyBox != null) {
				matSkyBox.SetFloat("_Exposure", _InterpolationUtil._Grade(1.0f, 1.0f - sdotc, _EYES._ClosestPlanet.AtmosphereThickness, 0f, _EYES._ClosestPlanet.SeaDistToEye));
			}
			if(matNebula != null) {
				matNebula.SetFloat("_Density", _InterpolationUtil._Grade(cachedNebulaDensity, 0.0f, _EYES._ClosestPlanet.AtmosphereThickness, 0f, _EYES._ClosestPlanet.SeaDistToEye));
			}
		}

		protected override void SetEnvironmentToUnderWater() {
			base.SetEnvironmentToUnderWater();
			if(matSkyBox != null) {
				matSkyBox.SetFloat("_Exposure", 0f);
			}
			if(matNebula != null) {
				matNebula.SetFloat("_Density", 0.0f);
			}
		}
#endregion Helper Methods

#region Object Methods
		//Overrides//
		public override string ToString() {
			return "Star Index: {0}, Name: {1}".FormatWith(this.Index, this.Name);
		}
#endregion Object Methods
	}

}