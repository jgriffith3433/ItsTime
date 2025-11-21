using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TOZ;
using TOZ.Noise;
using TOZ.Noise.Module;
using TOZ.Noise.Utilities;

namespace TOZ.Celestial {

	public sealed class Planet : PlanetBase {
#region Properties
		public Vector2 StationPosition { get { return stationPosition; } }
		public bool IsStationPositionedRandomly { get { return isStationPositionedRandomly; } }
#endregion Properties

#region Variables
		//Variables
		internal List<GameObject> Parts;
		internal List<PlanetTree> GroundTrees;
		internal List<PlanetTree> WaterTrees;

		[Header("Station Settings")]
		[SerializeField] private GameObject stationPrefab = null;
		[SerializeField] private bool isStationPositionedRandomly = true;
		[SerializeField] private Vector2 stationPosition = Vector2.zero;
		[NonSerialized] public GameObject Station = null;

		[Header("Nature Settings")]
		public PlanetaryObject[] NaturePrefabs = null;
		public PlanetaryObject[] WildLifePrefabs = null;
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

		}

		private void DoEnable() {
			CreateParts();
		}

		private void DoDisable() {
			DeleteParts();
		}

		private void OnDrawGizmos() {
			/*
			if(this != null) {
				Vector3 p = PlanetUtil.GetCartesianCoords(this, 45f, -45f);
				Vector3 dir = DirectionUtil.Get3DNormalized(this.PosW, p);
				Vector2 uv = PlanetUtil.DirectionToSphericalUV(dir);
				Gizmos.color = Color.white;
				Gizmos.DrawSphere(p, 50f);
				Debug.Log(
					PlanetUtil.GetPolarCoords(this, p) + " - " +
					dir + " - " +
					uv + " - " +
					PlanetUtil.SphericalUVToLatLon(uv) + " - " +
					PlanetUtil.LatLonToSphericalUV(45f, -45f)
				);
			}
			*/
		}

#endregion Methods

#region Helper Methods
		internal override void UpdatePlanet() {
			base.UpdatePlanet();
/*
			Vector3 d = MathUtil.PolarToCartesian(-90.0f, -10.0f);
			Vector2 dd = MathUtil.CartesianToPolar(d);
			Vector2 uv = PlanetUtil.PosToSphericalUV(d);
			Vector2 ll = PlanetUtil.UVToLatLon(uv);
			Vector2 uv2 = PlanetUtil.LatLonToUV(ll.x, ll.y);

			Debug.Log(d + " " + dd);
			Debug.Log(uv + " " + ll + " " + uv2);
*/
			
			
			//Update Projector Shadows
			Parts[0].transform.rotation = Quaternion.LookRotation(-this.DirToSunNormalized, Vector3.up);

			//Update Ground Trees
			if(HasGround) {
				if(GroundTrees != null && GroundTrees.Count > 0) {
					for(int side = 0; side < GroundTrees.Count; side++) {
						if(GroundTrees[side] != null) {
							GroundTrees[side].UpdateTree();
						}
					}
				}
			}

			//Update Water Trees
			if(HasWater) {
				if(WaterTrees != null && WaterTrees.Count > 0) {
					for(int side = 0; side < WaterTrees.Count; side++) {
						if(WaterTrees[side] != null) {
							WaterTrees[side].UpdateTree();
						}
					}
				}
			}
		}

		private void CreateParts() {
			DeleteParts();
			Parts = new List<GameObject>();

			GameObject go;
			//ShadowProjector
			go = this.Trs.GetChild(0).gameObject;
			Projector proj = go.GetComponent<Projector>();
			proj.aspectRatio = 1f;
			proj.nearClipPlane = this.AtmosphereRadius;
			proj.farClipPlane = this.AtmosphereRadius * 10f;
			proj.fieldOfView = 60f;
			proj.orthographic = true;
			proj.orthographicSize = this.Radius;
			go.SetActive(true);
			Parts.Add(go);
			proj = null;
			go = null;

			//Atmosphere
			go = this.Trs.GetChild(1).gameObject;
			if(HasAtmosphere) {
				go.transform.localScale = Vector3.one * (this.AtmosphereRadius * 2f);
				go.SetActive(true);
				Parts.Add(go);
				CreateAtmosphereMaterial();
				go.GetComponent<MeshRenderer>().sharedMaterial = AtmosphereMaterial;
			}
			else {
				go.SetActive(false);
			}
			go = null;

			//Clouds
			go = this.Trs.GetChild(2).gameObject;
			if(HasClouds) {
				go.transform.localScale = Vector3.one * (this.CloudsRadius * 2f);
				go.SetActive(true);
				Parts.Add(go);
				CreateCloudsMaterial();
				go.GetComponent<MeshRenderer>().sharedMaterial = CloudsMaterial;
			}
			else {
				go.SetActive(false);
			}
			go = null;
/*
			//Windzone
			go = this.Trs.GetChild(3).gameObject;
			if(HasNature) {
				WindZone wz = go.GetComponent<WindZone>();
				wz.radius = this.CloudsRadius;
				go.SetActive(true);
				Parts.Add(go);
			}
			else {
				go.SetActive(false);
			}
			go = null;
*/
			//Water
			if(HasWater) {
				go = new GameObject("Water");
				go.layer = 4;
				go.tag = this.Go.tag;
				go.transform.parent = this.Trs;
				go.transform.localPosition = Vector3.zero;
				go.transform.localRotation = Quaternion.identity;
				go.transform.localScale = Vector3.one;
				Parts.Add(go);
				CreateWaterTrees(Parts.Count-1);
				go = null;
			}

			//Ground
			if(HasGround) {
				go = new GameObject("Ground");
				go.layer = 30;
				go.tag = this.Go.tag;
				go.transform.parent = this.Trs;
				go.transform.localPosition = Vector3.zero;
				go.transform.localRotation = Quaternion.identity;
				go.transform.localScale = Vector3.one;
				Parts.Add(go);
				CreateGroundTrees(Parts.Count-1);
				go = null;
			}

			//Station
			if(HasStation) {
				go = new GameObject("Stations");
				go.layer = 25;
				go.tag = this.Go.tag;
				go.transform.parent = this.Trs;
				go.transform.localPosition = Vector3.zero;
				go.transform.localRotation = Quaternion.identity;
				go.transform.localScale = Vector3.one;
				Parts.Add(go);
				CreateStation(Parts.Count-1);
				go = null;
			}

			//Cache
			if(HasNature) {
				go = new GameObject("Cache");
				go.layer = 2;
				go.tag = this.Go.tag;
				go.transform.parent = this.Trs;
				go.transform.localPosition = Vector3.zero;
				go.transform.localRotation = Quaternion.identity;
				go.transform.localScale = Vector3.one;
				Parts.Add(go);
				CreateCache(Parts.Count-1);
				go = null;
			}
		}

		private void DeleteParts() {
			if(Parts != null) {
				if(Parts.Count > 0) {
					for(int i = 0; i < Parts.Count; i++) {
						if(Parts[i] != null) {
							if(Parts[i].name == "PlanetShadow") {
								Parts[i].SetActive(false);
								Parts[i] = null;
								continue;
							}
							if(Parts[i].name == "Atmosphere") {
								Parts[i].transform.localScale = Vector3.one;
								Parts[i].SetActive(false);
								DeleteAtmosphereMaterial();
								Parts[i] = null;
								continue;
							}
							if(Parts[i].name == "Clouds") {
								Parts[i].transform.localScale = Vector3.one;
								Parts[i].SetActive(false);
								DeleteCloudsMaterial();
								Parts[i] = null;
								continue;
							}
							/*
							if(Parts[i].name == "WindZone") {
								Parts[i].SetActive(false);
								Parts[i] = null;
								continue;
							}
							*/
							if(Parts[i].name == "Water") {
								DeleteWaterTrees();
								DestroyImmediate(Parts[i]);
								Parts[i] = null;
								continue;
							}
							if(Parts[i].name == "Ground") {
								DeleteGroundTrees();
								DestroyImmediate(Parts[i]);
								Parts[i] = null;
								continue;
							}
							if(Parts[i].name == "Stations") {
								DeleteStation();
								DestroyImmediate(Parts[i]);
								Parts[i] = null;
								continue;
							}
							if(Parts[i].name == "Cache") {
								DeleteCache();
								DestroyImmediate(Parts[i]);
								Parts[i] = null;
								continue;
							}
						}
					}
					Parts.Clear();
				}
				Parts = null;
			}
		}

		private void CreateWaterTrees(int partIndex) {
			DeleteWaterTrees();
			WaterTrees = new List<PlanetTree>();
			CreateWaterMaterial();

			for(int side = 0; side < 6; side++) {
				GameObject go = new GameObject("Tree - " + ((PlanetSideType)side).ToString());
				go.layer = Parts[partIndex].layer;
				go.tag = Parts[partIndex].tag;
				go.transform.parent = Parts[partIndex].transform;
				go.transform.localPosition = Vector3.zero;
				go.transform.localRotation = Quaternion.identity;
				go.transform.localScale = Vector3.one;
				go.SetActive(false);
				PlanetTree tree = go.AddComponent<PlanetTree>();
				go.SetActive(true);
				tree.Initialize(this, PlanetSurfaceType.Water, (PlanetSideType)side);
				WaterTrees.Add(tree);
			}
		}

		private void DeleteWaterTrees() {
			if(WaterTrees != null) {
				if(WaterTrees.Count > 0) {
					for(int i = 0; i < WaterTrees.Count; i++) {
						WaterTrees[i].DeleteNodes();
						DestroyImmediate(WaterTrees[i].Go);
						WaterTrees[i] = null;
					}
					WaterTrees.Clear();
				}
				WaterTrees = null;
			}
			DeleteWaterMaterial();
		}

		private void CreateGroundTrees(int partIndex) {
			DeleteGroundTrees();
			GroundTrees = new List<PlanetTree>();
			CreateGroundMaterial();

			for(int side = 0; side < 6; side++) {
				GameObject go = new GameObject("Tree - " + ((PlanetSideType)side).ToString());
				go.layer = Parts[partIndex].layer;
				go.tag = Parts[partIndex].tag;
				go.transform.parent = Parts[partIndex].transform;
				go.transform.localPosition = Vector3.zero;
				go.transform.localRotation = Quaternion.identity;
				go.transform.localScale = Vector3.one;
				go.SetActive(false);
				PlanetTree tree = go.AddComponent<PlanetTree>();
				go.SetActive(true);
				tree.Initialize(this, PlanetSurfaceType.Ground, (PlanetSideType)side);
				GroundTrees.Add(tree);
			}
		}

		private void DeleteGroundTrees() {
			if(GroundTrees != null) {
				if(GroundTrees.Count > 0) {
					for(int i = 0; i < GroundTrees.Count; i++) {
						GroundTrees[i].DeleteNodes();
						DestroyImmediate(GroundTrees[i].Go);
						GroundTrees[i] = null;
					}
					GroundTrees.Clear();
				}
				GroundTrees = null;
			}
			DeleteGroundMaterial();
		}

		//There is the issue of random position might not be precisely on a vertex,
		//So, station might be floating or sinking into the terrain at middle points.
		//Solution for low qualities is to use exact vertex positions
		//High qualities wont be so obvious to matter.
		private void CreateStation(int partIndex) {
			if(stationPrefab != null) {
				Station = Instantiate(stationPrefab, Vector3.zero, Quaternion.identity);
				Station.layer = Parts[partIndex].layer;
				Station.tag = Parts[partIndex].tag;
				Station.transform.parent = Parts[partIndex].transform;
				Station.transform.localPosition = Vector3.zero;
				Station.transform.localRotation = Quaternion.identity;
				Station.transform.localScale *= _SS._Scale;

				float lat = stationPosition.x;
				float lon = stationPosition.y;
				_RandUtil._SetSeed(this.ID);
				if(isStationPositionedRandomly) {
					lat = UnityEngine.Random.Range(-180f, 180f);
					lon = UnityEngine.Random.Range(-90f, 90f);
				}
				Vector3 dir = _MathUtil._PolarToCartesian(lat, lon);
				Vector2 uv = PlanetUtil.DirectionToSphericalUV(dir);

				Vector4 data = (Vector4)this.DataMap.GetPixelBilinear(uv.x, uv.y);
				float height = data.w * 2.0f - 1.0f;
				if(this.HasWater) {
					height = Mathf.Max(0f, height);
				}

				Vector3 pos = this.PosW + (dir * this.Radius);
				pos += dir * (height * this.GroundThicknessHalf);
				Quaternion rot = Quaternion.FromToRotation(Vector3.up, dir);
				rot *= Quaternion.AngleAxis(UnityEngine.Random.Range(0f, 360f), Vector3.up);

				Station.transform.position = pos;
				Station.transform.rotation = rot;
				Transform wps = Station.transform.Find("Waypoints");
				if(wps != null) {
					foreach(Transform wp in wps) {
						_NAV._Wps.Add(wp.position);
					}
				}
			}
		}

		private void DeleteStation() {
			if(Station != null) {
				DestroyImmediate(Station);
				_NAV._Update();
			}
		}

		private void CreateCache(int partIndex) {
			for(int i = 0; i < NaturePrefabs.Length; i++) {
				GameObject go = new GameObject("Cache"+i);
				go.layer = 2;
				go.tag = this.Go.tag;
				go.transform.parent = Parts[partIndex].transform;
				go.transform.localPosition = Vector3.zero;
				go.transform.localRotation = Quaternion.identity;
				go.transform.localScale = Vector3.one;

				NaturePrefabs[i].GOs = new GameObject[NaturePrefabs[i].MaxQuantityPerNode * 48];
				for(int j = 0; j < NaturePrefabs[i].GOs.Length; j++) {
					NaturePrefabs[i].GOs[j] = Instantiate(NaturePrefabs[i].Prefab, this.PosW, Quaternion.identity, go.transform);
					NaturePrefabs[i].GOs[j].transform.localScale = Vector3.one;
					NaturePrefabs[i].GOs[j].SetActive(false);
				}
			}
		}

		private void DeleteCache() {
			if(NaturePrefabs != null) {
				for(int i = 0; i < NaturePrefabs.Length; i++) {
					if(NaturePrefabs[i].GOs != null) {
						for(int j = 0; j < NaturePrefabs[i].GOs.Length; j++) {
							DestroyImmediate(NaturePrefabs[i].GOs[j]);
						}
					}
				}
			}
		}

		private Vector3 RayIntersectingPlane(Ray ray, Vector3 planeOrigin, Vector3 planeNormal) {
			Vector3 diff = ray.origin - planeOrigin;
			float dot1 = Vector3.Dot(diff, planeNormal);
			float dot2 = Vector3.Dot(ray.direction, planeNormal);
			float dot3 = dot1 / dot2;
			return ray.origin - ray.direction * dot3;
		}
#endregion Helper Methods
	}

}