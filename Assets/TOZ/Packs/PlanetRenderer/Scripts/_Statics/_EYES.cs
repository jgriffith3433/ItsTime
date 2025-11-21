using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using TOZ;

namespace TOZ.Celestial {

	public static class _EYES {
#region Variables
		//Variables
		private static float[] _cullDistances;
		public static Camera _FrustumCam, _FarCam, _MainCam;
		public static Transform _Head;
		public static Plane[] _Frustum;

		public static bool _InNavigationZone;
		public static bool _InGravityZone;
		public static bool _InAtmosphere;
		public static bool _InClouds;
		public static bool _InOpenArea;
		public static bool _InClosedArea;
		public static bool _InWater;

		public static CelestialBase _ClosestCelestial;
		public static Planet _ClosestPlanet;

		//Constructor
		static _EYES() {
			_Initialize();
		}
#endregion Variables

#region Methods
		public static void _Initialize() {
			_cullDistances = new float[32];

			if(_MainCam != null) {
				_MainCam.layerCullSpherical = false;
				_MainCam.layerCullDistances = _cullDistances;
			}

			_FrustumCam = _FarCam = _MainCam = null;
			_Head = null;
			_Frustum = null;

			_InNavigationZone = false;
			_InGravityZone = false;
			_InAtmosphere = false;
			_InClouds = false;
			_InOpenArea = false;
			_InClosedArea = false;
			_InWater = false;

			_ClosestCelestial = null;
			_ClosestPlanet = null;
		}

		public static void _Setup() {
			_Initialize();

			_MainCam = Camera.main;
			_Head = _MainCam.transform.parent;
			_FrustumCam = _Head.GetChild(0).GetComponent<Camera>();
			_FarCam = _Head.GetChild(1).GetComponent<Camera>();

			_Update();
		}

		public static void _Update() {
			_Frustum = GeometryUtility.CalculateFrustumPlanes(_FrustumCam);

			//_InOpenArea = !_InClosedArea && !_InWater;
		}

		public static void _SetCullingDistance(float distance = 0f) {
			distance = _MathUtil._Clamp(distance, 0f, _MainCam.farClipPlane);

			_cullDistances[0] = distance; //Default
			_cullDistances[1] = distance; //Transparent FX
			_cullDistances[2] = distance; //Ignore Raycast
			//_cullDistances[3] = 0f; //Not Used
			//_cullDistances[4] = distance; //Water
			//_cullDistances[5] = 0f; //Not Used
			//_cullDistances[6] = 0f; //Not Used
			//_cullDistances[7] = 0f; //Not Used
			//_cullDistances[8] = 0f; //Not Used
			//_cullDistances[9] = 0f; //Not Used
			//_cullDistances[10] = distance; //LightSource
			//_cullDistances[11] = distance; //AudioSource
			//_cullDistances[12] = 0f; //Not Used
			//_cullDistances[13] = 0f; //Not Used
			//_cullDistances[14] = 0f; //Not Used
			//_cullDistances[15] = 0f; //Not Used
			//_cullDistances[16] = 0f; //Not Used
			//_cullDistances[17] = 0f; //Not Used
			_cullDistances[18] = distance; //Bullet
			_cullDistances[19] = distance; //Interactable
			_cullDistances[20] = distance; //Limbs
			_cullDistances[21] = distance; //Player
			_cullDistances[22] = distance; //Npc
			//_cullDistances[23] = 0f; //Not Used
			_cullDistances[24] = distance; //Wildlife
			_cullDistances[25] = distance; //Buildings
			_cullDistances[26] = distance; //Nature
			//_cullDistances[27] = distance; //Astro
			//_cullDistances[28] = distance; //Atmosphere
			//_cullDistances[29] = distance; //Clouds
			//_cullDistances[30] = distance; //Ground
			//_cullDistances[31] = 0f; //Not Used

			if(_MainCam != null) {
				_MainCam.layerCullDistances = _cullDistances;
			}
			//if(_FarCam != null) {
				//_FarCam.layerCullDistances = _cullDistances;
			//}
			//Dont need _FarCam here, because it doesnt render these layers anyhow
		}

		public static void _SetNearDistance(float distance = 0.3f) {
			distance = _MathUtil._ClampDown(distance, 0.0015f);
			if(_MainCam != null) {
				_MainCam.nearClipPlane = distance;
			}
			if(_FrustumCam != null) {
				_FrustumCam.nearClipPlane = distance;
			}
		}

		public static void _SetMiddleDistance(float distance = 150f) {
			if(_MainCam != null && _FarCam != null) {
				distance = _MathUtil._Clamp(distance, _MainCam.nearClipPlane+0.003f, _FarCam.farClipPlane-0.003f);
			}
			if(_MainCam != null) {
				_MainCam.farClipPlane = distance;
			}
			if(_FarCam != null) {
				_FarCam.nearClipPlane = distance;
			}
			//_SetCullingDistance(_MathUtil._Clamp(distance, 0f, _MainCam.farClipPlane));
		}

		public static void _SetFarDistance(float distance = 2000f) {
			distance = _MathUtil._ClampUp(distance, 1000000f);
			if(_FarCam != null) {
				_FarCam.farClipPlane = distance;
			}
			if(_FrustumCam != null) {
				_FrustumCam.farClipPlane = distance;
			}
		}

		public static List<Camera> _GetAllRenderingCameras() {
			List<Camera> allCams = new List<Camera>();
			allCams.Add(_MainCam);
			allCams.Add(_FarCam);
			allCams.Add(_FrustumCam);
			#if UNITY_EDITOR
			Camera[] sceneCams = SceneView.GetAllSceneCameras();
			for(int i = 0; i < sceneCams.Length; i++) {
				allCams.Add(sceneCams[i]);
			}
			#endif
			return allCams;
		}
#endregion Methods
	}

}