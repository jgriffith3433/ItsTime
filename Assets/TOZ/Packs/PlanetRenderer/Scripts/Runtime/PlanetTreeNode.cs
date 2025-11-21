using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using TOZ;

namespace TOZ.Celestial {

	public sealed class PlanetTreeNode : GameObjectBase, IComparable<PlanetTreeNode> {
#region Variables
		//Components
		private Mesh mesh;
		private MeshFilter meshFilter;
		private MeshRenderer meshRenderer;
		private MeshCollider meshCollider;

		//Variables
		private Vector3[] vertices;
		private int[] triangles;
		private Vector2[] uvs;
		private Vector2[] uvs4;
		private Vector3[] normals;
		private Vector4[] tangents;
		private Color[] colors;

		private PlanetTree tree;
		private PlanetTreeNode parent;
		private List<PlanetTreeNode> children;
		private int lod;
		private int idx, idy;
		private Vector3 llPlot, ulPlot, urPlot, lrPlot;
		private Vector2 llUv, ulUv, urUv, lrUv, mUv;
		private Vector3 llCorner, ulCorner, urCorner, lrCorner, mCorner;
		private Vector2 llLatLon, ulLatLon, urLatLon, lrLatLon, mLatLon;
		//internal int DataLX, DataLY, DataUX, DataUY;

		internal int Segments;
		internal bool IsQueued;
		private bool isGenerated;
		private bool hasChildren;
		private bool hasNature;
		private GameObject goNatureRoot;
		private bool hasWildLife;
		private GameObject goWildLifeRoot;

		private bool isInHorizon;
		private bool isInFrustum;
		private float priority;
		private float highestPoint;
#endregion Variables

#region Mono Methods
		private void OnDisable() {
			DoDisable();
		}

		private void OnDrawGizmosSelected() {
			if(this != null) {
				/*
				Vector3 dir1 = MathUtil.PolarToCartesian(llLatLon.y, llLatLon.x);
				Vector3 dir2 = MathUtil.PolarToCartesian(ulLatLon.y, ulLatLon.x);
				Vector3 dir3 = MathUtil.PolarToCartesian(urLatLon.y, urLatLon.x);
				Vector3 dir4 = MathUtil.PolarToCartesian(lrLatLon.y, lrLatLon.x);
				float dist = tree.Planet.CloudsRadius;
				Vector3 pos1 = tree.Planet.PosW + (dir1 * dist);
				Vector3 pos2 = tree.Planet.PosW + (dir2 * dist);
				Vector3 pos3 = tree.Planet.PosW + (dir3 * dist);
				Vector3 pos4 = tree.Planet.PosW + (dir4 * dist);

				Gizmos.color = Color.white;
				Gizmos.DrawWireSphere(pos1, 25f);
				Gizmos.color = Color.red;
				Gizmos.DrawWireSphere(pos2, 25f);
				Gizmos.color = Color.green;
				Gizmos.DrawWireSphere(pos3, 25f);
				Gizmos.color = Color.blue;
				Gizmos.DrawWireSphere(pos4, 25f);
				
				float x1 = AngleUtil.Wrap(llLatLon.x);
				float y1 = AngleUtil.Wrap(llLatLon.y);
				float x2 = AngleUtil.Wrap(urLatLon.x);
				float y2 = AngleUtil.Wrap(urLatLon.y);
				//Debug.Log("1: " + llUv + "                " + "2: " + urUv);

				for(int i = 0; i < mesh.vertices.Length; i++) {
					Gizmos.color = Color.green;
					Gizmos.DrawRay(this.PosW + mesh.vertices[i], mesh.normals[i]);
					Gizmos.color = Color.red;
					Gizmos.DrawRay(this.PosW + mesh.vertices[i], mesh.tangents[i]);
					//Gizmos.DrawRay(this.PosW + mesh.vertices[i], new Vector3(mesh.tangents[i].x, mesh.tangents[i].y, mesh.tangents[i].z));
				}
				*/
			}
		}
#endregion Mono Methods

#region Methods
		private void DoDisable() {
			IsQueued = false;
			isGenerated = false;
			hasChildren = false;
			hasNature = false;
			goNatureRoot = null;
			hasWildLife = false;
			goWildLifeRoot = null;
			isInHorizon = false;

			DeleteChildren();
			children = null;
		}
#endregion Methods

#region Helper Methods
		internal void Initialize(PlanetTree t, PlanetTreeNode p, int l, int x, int y, Vector3 ll, Vector3 ul, Vector3 ur, Vector3 lr) {
			tree = t;
			parent = p;
			lod = l;
			idx = x;
			idy = y;

			//Plot corners, latitudes and longitudes
			Vector3 dir;
			llPlot = ll;
			dir = PlanetUtil.CubeToSphere0(llPlot);
			llUv = PlanetUtil.DirectionToSphericalUV(dir);
			llCorner = dir * tree.Planet.Radius;
			llLatLon = _MathUtil._CartesianToPolar(llCorner);

			//DataLX = (int)Mathf.RoundToInt(InterpolationUtil.Linear(0, Tree.Planet.Data.GetLength(0)-1, LLUv.x));
			//DataLY = (int)Mathf.RoundToInt(InterpolationUtil.Linear(0, Tree.Planet.Data.GetLength(1)-1, LLUv.y));
			ulPlot = ul;
			dir = PlanetUtil.CubeToSphere0(ulPlot);
			ulUv = PlanetUtil.DirectionToSphericalUV(dir);
			ulCorner = dir * tree.Planet.Radius;
			ulLatLon = _MathUtil._CartesianToPolar(ulCorner);

			urPlot = ur;
			dir = PlanetUtil.CubeToSphere0(urPlot);
			urUv = PlanetUtil.DirectionToSphericalUV(dir);
			urCorner = dir * tree.Planet.Radius;
			urLatLon = _MathUtil._CartesianToPolar(urCorner);

			//DataUX = (int)Mathf.RoundToInt(InterpolationUtil.Linear(0, Tree.Planet.Data.GetLength(0)-1, URUv.x));
			//DataUY = (int)Mathf.RoundToInt(InterpolationUtil.Linear(0, Tree.Planet.Data.GetLength(1)-1, URUv.y));
			lrPlot = lr;
			dir = PlanetUtil.CubeToSphere0(lrPlot);
			lrUv = PlanetUtil.DirectionToSphericalUV(dir);
			lrCorner = dir * tree.Planet.Radius;
			lrLatLon = _MathUtil._CartesianToPolar(lrCorner);

			dir = PlanetUtil.CubeToSphere0((llPlot + urPlot) * 0.5f);
			mUv = PlanetUtil.DirectionToSphericalUV(dir);
			mCorner = dir * tree.Planet.Radius;
			mLatLon = _MathUtil._CartesianToPolar(mCorner);

			switch(tree.Side) {
				case PlanetSideType.Top:
				case PlanetSideType.Back:
					if(_MathUtil._IsZero(lrPlot.x) && _MathUtil._IsZero(urPlot.x) && _MathUtil._IsZero(lrUv.x) && _MathUtil._IsZero(urUv.x)) {
						lrUv.x = 1.0f;
						urUv.x = 1.0f;
				}
				break;
			}
					
			IsQueued = false;
			isGenerated = false;
			hasChildren = false;
			hasNature = false;
			goNatureRoot = null;
			hasWildLife = false;
			goWildLifeRoot = null;
			isInHorizon = false;
			highestPoint = -tree.Planet.GroundThicknessHalf;

			GenerateMeshData();
		}

		internal void CreateChildren() {
			if(hasChildren)
				return;

			children = new List<PlanetTreeNode>();

			Vector3 step = Vector3.zero;
			Vector3 ll = Vector3.zero;
			Vector3 ul = Vector3.zero;
			Vector3 ur = Vector3.zero;
			Vector3 lr = Vector3.zero;
			float x0, x1, y0, y1, z0, z1;
			for(int y = 0; y < _LOD._Subdivisions; y++) {
				for(int x = 0; x < _LOD._Subdivisions; x++) {

					switch(tree.Side) {
						case PlanetSideType.Back:
							step = (urPlot - llPlot) / _LOD._Subdivisions;
							x0 = llPlot.x + (step.x * x);
							x1 = llPlot.x + (step.x * (x+1));
							y0 = llPlot.y + (step.y * y);
							y1 = llPlot.y + (step.y * (y+1));
							ll = new Vector3(x0, y0, llPlot.z);
							ul = new Vector3(x0, y1, llPlot.z);
							ur = new Vector3(x1, y1, llPlot.z);
							lr = new Vector3(x1, y0, llPlot.z);
						break;
						case PlanetSideType.Right:
							step = (urPlot - llPlot) / _LOD._Subdivisions;
							z0 = llPlot.z + (step.z * x);
							z1 = llPlot.z + (step.z * (x+1));
							y0 = llPlot.y + (step.y * y);
							y1 = llPlot.y + (step.y * (y+1));
							ll = new Vector3(llPlot.x, y0, z0);
							ul = new Vector3(llPlot.x, y1, z0);
							ur = new Vector3(llPlot.x, y1, z1);
							lr = new Vector3(llPlot.x, y0, z1);
						break;
						case PlanetSideType.Top:
							step = (urPlot - llPlot) / _LOD._Subdivisions;
							x0 = llPlot.x + (step.x * x);
							x1 = llPlot.x + (step.x * (x+1));
							z0 = llPlot.z + (step.z * y);
							z1 = llPlot.z + (step.z * (y+1));
							ll = new Vector3(x0, llPlot.y, z0);
							ul = new Vector3(x0, llPlot.y, z1);
							ur = new Vector3(x1, llPlot.y, z1);
							lr = new Vector3(x1, llPlot.y, z0);
						break;
						case PlanetSideType.Front:
							step = (ulPlot - lrPlot) / _LOD._Subdivisions;
							x0 = llPlot.x - (step.x * x);
							x1 = llPlot.x - (step.x * (x+1));
							y0 = llPlot.y + (step.y * y);
							y1 = llPlot.y + (step.y * (y+1));
							ll = new Vector3(x0, y0, llPlot.z);
							ul = new Vector3(x0, y1, llPlot.z);
							ur = new Vector3(x1, y1, llPlot.z);
							lr = new Vector3(x1, y0, llPlot.z);
						break;
						case PlanetSideType.Left:
							step = (ulPlot - lrPlot) / _LOD._Subdivisions;
							z0 = llPlot.z - (step.z * x);
							z1 = llPlot.z - (step.z * (x+1));
							y0 = llPlot.y + (step.y * y);
							y1 = llPlot.y + (step.y * (y+1));
							ll = new Vector3(llPlot.x, y0, z0);
							ul = new Vector3(llPlot.x, y1, z0);
							ur = new Vector3(llPlot.x, y1, z1);
							lr = new Vector3(llPlot.x, y0, z1);
						break;
						case PlanetSideType.Bottom:
							step = (lrPlot - ulPlot) / _LOD._Subdivisions;
							x0 = llPlot.x + (step.x * x);
							x1 = llPlot.x + (step.x * (x+1));
							z0 = llPlot.z - (step.z * y);
							z1 = llPlot.z - (step.z * (y+1));
							ll = new Vector3(x0, llPlot.y, z0);
							ul = new Vector3(x0, llPlot.y, z1);
							ur = new Vector3(x1, llPlot.y, z1);
							lr = new Vector3(x1, llPlot.y, z0);
						break;
					}

					GameObject go = new GameObject("Node - {0} - {1} - LOD{2}".FormatWith(x, y, (lod+1)));
					go.layer = this.Go.layer;
					go.tag = this.Go.tag;
					go.transform.parent = this.Trs;
					go.transform.localPosition = Vector3.zero;
					go.transform.localRotation = Quaternion.identity;
					go.transform.localScale = Vector3.one;
					go.SetActive(false);
					PlanetTreeNode node = go.AddComponent<PlanetTreeNode>();
					go.SetActive(true);
					node.Initialize(tree, this, lod+1, x, y, ll, ul, ur, lr);
					children.Add(node);
				}
			}
			hasChildren = true;
		}

		internal void DeleteChildren() {
			if(children != null) {
				if(children.Count > 0) {
					for(int i = 0; i < children.Count; i++) {
						if(children[i] != null) {
							children[i].DeleteMeshes();
							children[i].DeleteObjects();
						}
					}
					children.Clear();
				}
			}
			hasChildren = false;

			CalculateCulling();
		}

		private void DeleteMeshes() {
			if(children != null) {
				if(children.Count > 0) {
					for(int i = 0; i < children.Count; i++) {
						if(children[i] != null) {
							children[i].DeleteMeshes();
						}
					}
				}
			}

			if(mesh != null)
				DestroyImmediate(mesh, false);

			isGenerated = false;
		}

		private void DeleteObjects() {
			if(children != null) {
				if(children.Count > 0) {
					for(int i = 0; i < children.Count; i++) {
						if(children[i] != null) {
							children[i].DeleteObjects();
						}
					}
				}
			}

			if(hasNature) {
				if(goNatureRoot != null) {
					for(int i = 0; i < goNatureRoot.transform.childCount; i++) {
						Transform cache = goNatureRoot.transform.GetChild(i);
						int count = cache.childCount;
						for(int j = 0; j < count; j++) {
							cache.GetChild(0).gameObject.SetActive(false);
							cache.GetChild(0).position = tree.Planet.PosW;
							cache.GetChild(0).localRotation = Quaternion.identity;
							cache.GetChild(0).localScale = Vector3.one;
							cache.GetChild(0).parent = tree.Planet.Parts[tree.Planet.Parts.Count-1].transform.GetChild(i);
						}
					}
				}
			}

			if(this.Go != null)
				DestroyImmediate(this.Go, false);

			hasNature = false;
			goNatureRoot = null;
			hasWildLife = false;
			goWildLifeRoot = null;
		}

		private void CreateNature() {
			if(tree.Surface.Is(PlanetSurfaceType.Ground) && tree.Planet.HasNature) {
				if(lod == _LOD._Max && tree.Planet.NaturePrefabs != null) {
					hasNature = true;
					GameObject go = new GameObject("Nature");
					go.layer = 26;
					go.tag = this.Go.tag;
					go.transform.parent = this.Trs;
					go.transform.localPosition = Vector3.zero;
					go.transform.localRotation = Quaternion.identity;
					go.transform.localScale = Vector3.one;
					goNatureRoot = go;

					_RandUtil._SetSeed(tree.Planet.ID);
					float height = -tree.Planet.GroundThickness;
					float steepness = 0f;
					int layerMask = 1 << 30;
					RaycastHit hit;

					for(int i = 0; i < tree.Planet.NaturePrefabs.Length; i++) {
						GameObject goCache = new GameObject("Cache"+i);
						goCache.layer = 2;
						goCache.tag = this.Go.tag;
						goCache.transform.parent = goNatureRoot.transform;
						goCache.transform.localPosition = Vector3.zero;
						goCache.transform.localRotation = Quaternion.identity;
						goCache.transform.localScale = Vector3.one;

						Transform vault = tree.Planet.Parts[tree.Planet.Parts.Count-1].transform.GetChild(i);
						for(int j = 0; j < tree.Planet.NaturePrefabs[i].MaxQuantityPerNode; j++) {
							/*
							float y1 = llLatLon.y + 90f;
							float y2 = urLatLon.y + 90f;
							float x1 = llLatLon.x + 180f;
							float x2 = urLatLon.x + 180f;
							float y = UnityEngine.Random.Range(y1, y2);
							float x = UnityEngine.Random.Range(x1, x2);
							float lat = y - 90f;
							float lon = x - 180f;
							Vector3 dir = MathUtil.PolarToCartesian(lat, lon);
							*/
							float xMin = Mathf.Min(Mathf.Min(llUv.x, ulUv.x), Mathf.Min(lrUv.x, urUv.x));
							float xMax = Mathf.Max(Mathf.Max(llUv.x, ulUv.x), Mathf.Max(lrUv.x, urUv.x));
							float yMin = Mathf.Min(Mathf.Min(llUv.y, ulUv.y), Mathf.Min(lrUv.y, urUv.y));
							float yMax = Mathf.Max(Mathf.Max(llUv.y, ulUv.y), Mathf.Max(lrUv.y, urUv.y));
							float u = UnityEngine.Random.Range(xMin, xMax);
							float v = UnityEngine.Random.Range(yMin, yMax);
							Vector3 dir = PlanetUtil.SphericalUVToDirection(u, v);
							float dist = tree.Planet.CloudsRadius;
							Vector3 pos = tree.Planet.PosW + (dir * dist);

							if(Physics.Raycast(pos, -dir, out hit, dist, layerMask, QueryTriggerInteraction.Ignore)) {
								height = _DistUtil._Get3D(hit.point, tree.Planet.PosW) - tree.Planet.Radius;
								steepness = 1.0f - _MathUtil._ClampDown(Vector3.Dot(hit.normal, dir), 0f);

								if(height >= tree.Planet.GroundThicknessHalf * tree.Planet.NaturePrefabs[i].MinHeight && height <= tree.Planet.GroundThicknessHalf * tree.Planet.NaturePrefabs[i].MaxHeight) {
									if(steepness >= tree.Planet.NaturePrefabs[i].MinSteepness && steepness <= tree.Planet.NaturePrefabs[i].MaxSteepness) {
										Quaternion rot = Quaternion.FromToRotation(Vector3.up, ((tree.Planet.NaturePrefabs[i].AlignToGroundNormals) ? hit.normal : dir));
										rot *= Quaternion.AngleAxis(UnityEngine.Random.Range(0f, 360f), Vector3.up);
										vault.GetChild(j).position = hit.point;
										vault.GetChild(j).localRotation = rot;
										vault.GetChild(j).localScale *= _SS._Scale;
										vault.GetChild(j).localScale *= UnityEngine.Random.Range(tree.Planet.NaturePrefabs[i].MinScale, tree.Planet.NaturePrefabs[i].MaxScale);
										vault.GetChild(j).gameObject.SetActive(true);
										vault.GetChild(j).parent = goCache.transform;
									}
								}
							}
						}
					}
				}
			}
		}
/*
		private void CreateNature() {
			if(Tree.Surface.Is(PlanetSurfaceType.Ground) && Tree.Planet.HasNature) {
				if(lod == _LOD._MaxLOD && Tree.Planet.NaturePrefabs != null) {
					hasNature = true;
					GameObject go = new GameObject("Nature");
					go.layer = 26;
					go.tag = this.Go.tag;
					go.transform.parent = this.Trs;
					go.transform.localPosition = Vector3.zero;
					go.transform.localRotation = Quaternion.identity;
					go.transform.localScale = Vector3.one;
					goNatureRoot = go;

					RandomUtil.SetSeed(Tree.Planet.ID);
					float height = -Tree.Planet.GroundThickness;
					float steepness = 0f;
					int layerMask = 1 << 30;
					RaycastHit hit;

					for(int i = 0; i < Tree.Planet.NaturePrefabs.Length; i++) {
						for(int j = 0; j < Tree.Planet.NaturePrefabs[i].MaxQuantityPerNode; j++) {
							float lat = UnityEngine.Random.Range(LLLatLon.y, URLatLon.y);
							float lon = UnityEngine.Random.Range(LLLatLon.x, URLatLon.x);
							Vector3 dir = MathUtil.PolarToCartesian(lat, lon);
							float dist = Tree.Planet.CloudsRadius;
							Vector3 pos = Tree.Planet.PosW + (dir * dist);

							if(Physics.Raycast(pos, -dir, out hit, dist, layerMask, QueryTriggerInteraction.Ignore)) {
								height = DistanceUtil.Get3D(hit.point, Tree.Planet.PosW) - Tree.Planet.Radius;
								steepness = 1.0f - MathUtil.ClampDown(Vector3.Dot(hit.normal, dir), 0f);

								if(height >= Tree.Planet.GroundThicknessHalf * Tree.Planet.NaturePrefabs[i].MinHeight && height <= Tree.Planet.GroundThicknessHalf * Tree.Planet.NaturePrefabs[i].MaxHeight) {
									if(steepness >= Tree.Planet.NaturePrefabs[i].MinSteepness && steepness <= Tree.Planet.NaturePrefabs[i].MaxSteepness) {
										Quaternion rot = Quaternion.FromToRotation(Vector3.up, ((Tree.Planet.NaturePrefabs[i].AlignToGroundNormals) ? hit.normal : dir));
										rot *= Quaternion.AngleAxis(UnityEngine.Random.Range(0f, 360f), Vector3.up);
										GameObject detail = Instantiate(Tree.Planet.NaturePrefabs[i].Prefab, hit.point, rot, go.transform);
										detail.transform.localScale *= _SOLARSYSTEM._Scale;
										detail.transform.localScale *= UnityEngine.Random.Range(Tree.Planet.NaturePrefabs[i].MinScale, Tree.Planet.NaturePrefabs[i].MaxScale);
									}
								}
							}
						}
					}
				}
			}
		}
*/
		private void CreateWildLife() {
			if(tree.Surface.Is(PlanetSurfaceType.Ground) && tree.Planet.HasWildLife) {
				if(lod == _LOD._Max && tree.Planet.WildLifePrefabs != null) {
					this.hasWildLife = true;
					GameObject go = new GameObject("WildLife");
					go.layer = 24;
					go.tag = this.Go.tag;
					go.transform.parent = this.Trs;
					go.transform.localPosition = Vector3.zero;
					go.transform.localRotation = Quaternion.identity;
					go.transform.localScale = Vector3.one;
					goWildLifeRoot = go;

					_RandUtil._SetSeed(tree.Planet.ID);
					float height = -tree.Planet.GroundThickness;
					float steepness = 0f;
					int layerMask = 1 << 30;
					RaycastHit hit;

					for(int i = 0; i < tree.Planet.WildLifePrefabs.Length; i++) {
						for(int j = 0; j < tree.Planet.WildLifePrefabs[i].MaxQuantityPerNode; j++) {
							float lat = UnityEngine.Random.Range(llLatLon.y, urLatLon.y);
							float lon = UnityEngine.Random.Range(llLatLon.x, urLatLon.x);
							Vector3 pos = tree.Planet.PosW + _MathUtil._PolarToCartesian(lat, lon) * tree.Planet.CloudsRadius;

							Vector3 dir = _DirUtil._Get3D(pos, tree.Planet.PosW);
							float dist = dir.magnitude;
							dir = dir.normalized;

							if(Physics.Raycast(pos, dir, out hit, dist, layerMask, QueryTriggerInteraction.Ignore)) {
								height = _DistUtil._Get3D(hit.point, tree.Planet.PosW) - tree.Planet.Radius;
								steepness = 1.0f - _MathUtil._ClampDown(Vector3.Dot(hit.normal, -dir), 0f);

								if(height >= tree.Planet.GroundThicknessHalf * tree.Planet.WildLifePrefabs[i].MinHeight && height <= tree.Planet.GroundThicknessHalf * tree.Planet.WildLifePrefabs[i].MaxHeight) {
									if(steepness >= tree.Planet.WildLifePrefabs[i].MinSteepness && steepness <= tree.Planet.WildLifePrefabs[i].MaxSteepness) {
										Quaternion rot = Quaternion.FromToRotation(Vector3.up, ((tree.Planet.WildLifePrefabs[i].AlignToGroundNormals) ? hit.normal : -dir));
										Vector3 spawnPos = hit.point + (-dir * 0.0f);
										GameObject detail = Instantiate(tree.Planet.WildLifePrefabs[i].Prefab, spawnPos, rot, go.transform);
										detail.layer = go.layer;
										detail.SetActive(false);
										detail.transform.localScale *= _SS._Scale;
										detail.transform.localScale *= UnityEngine.Random.Range(tree.Planet.WildLifePrefabs[i].MinScale, tree.Planet.WildLifePrefabs[i].MaxScale);
										detail.SetActive(true);
									}
								}
							}
						}
					}
				}
			}
		}
#endregion Helper Methods

#region MeshData Methods
		private void GenerateMeshData() {
			if(mesh == null) {
				mesh = new Mesh();
				mesh.name = this.Go.name;
			}

			if(meshFilter == null) {
				meshFilter = (MeshFilter)this.Go.AddComponent(typeof(MeshFilter));
				meshFilter.sharedMesh = mesh;
				
			}

			if(meshRenderer == null) {
				meshRenderer = (MeshRenderer)this.Go.AddComponent(typeof(MeshRenderer));
			}

			switch(tree.Surface) {
				case PlanetSurfaceType.Ground:
					meshRenderer.receiveShadows = true;
					meshRenderer.shadowCastingMode = ShadowCastingMode.TwoSided;
					meshRenderer.sharedMaterial = tree.Planet.GroundMaterial;
				break;
				case PlanetSurfaceType.Water:
					meshRenderer.receiveShadows = false;
					meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
					meshRenderer.sharedMaterial = tree.Planet.WaterMaterial;
				break;
			}

			switch(tree.Planet.Quality) {
				case PlanetQualityType.Low:
					Segments = _LOD._Segments;
				break;
				case PlanetQualityType.Medium:
					Segments = _LOD._Segments * 2;
				break;
				case PlanetQualityType.Normal:
					Segments = _LOD._Segments * 4;
				break;
				case PlanetQualityType.Good:
					Segments = _LOD._Segments * 8;
				break;
				case PlanetQualityType.High:
					Segments = _LOD._Segments * 16;
				break;
				case PlanetQualityType.Ultra:
					Segments = _LOD._Segments * 32;
				break;
				default:
					Segments = _LOD._Segments;
				break;
			}

			CalculateMeshData();
			ApplyMeshData();
/*
			//Calculate on another thread
			ThreadManager.RunOnThread(()=>{
				CalculateMeshData();
				ThreadManager.RunOnMainThread(ApplyMeshData);
			});
*/
		}

		private void CalculateMeshData() {
			int segmentsPlusOne = Segments + 1;
			int borders = 0;
			int vertCount = segmentsPlusOne * segmentsPlusOne;
			int triCount = (Segments) * (Segments) * 6;
			if(tree.Surface == PlanetSurfaceType.Ground) {
				if(lod >= 0) {
					borders = segmentsPlusOne * 4;
					vertCount += borders;
					triCount += (borders-4) * 6;
				}
			}

			vertices = new Vector3[vertCount];
			triangles = new int[triCount];
			uvs = new Vector2[vertCount];
			uvs4 = new Vector2[vertCount];
			normals = new Vector3[vertCount];
			tangents = new Vector4[vertCount];
			colors = new Color[vertCount];

			highestPoint = -tree.Planet.GroundThicknessHalf;

			int current = 0;
			Vector3 step = Vector3.zero;
			int index = 0;
			float xRatio = 0f, yRatio = 0f;
			Vector2 uv = Vector2.zero;
			for(int y = 0; y <= Segments; y++) {
				for(int x = 0; x <= Segments; x++) {
					current = y * segmentsPlusOne + x;

#region Calculate Vertices
					//Calculate Vertices
					switch(tree.Side) {
						case PlanetSideType.Back:
							step = (urPlot - llPlot) / (float)Segments;
							vertices[current] = new Vector3(llPlot.x + step.x * x, llPlot.y + step.y * y, llPlot.z);
						break;
						case PlanetSideType.Right:
							step = (urPlot - llPlot) / (float)Segments;
							vertices[current] = new Vector3(llPlot.x, llPlot.y + step.y * y, llPlot.z + step.z * x);
						break;
						case PlanetSideType.Top:
							step = (urPlot - llPlot) / (float)Segments;
							vertices[current] = new Vector3(llPlot.x + step.x * x, llPlot.y, llPlot.z + step.z * y);
						break;
						case PlanetSideType.Front:
							step = (ulPlot - lrPlot) / (float)Segments;
							vertices[current] = new Vector3(llPlot.x - step.x * x, llPlot.y + step.y * y, llPlot.z);
						break;
						case PlanetSideType.Left:
							step = (ulPlot - lrPlot) / (float)Segments;
							vertices[current] = new Vector3(llPlot.x, llPlot.y + step.y * y, llPlot.z - step.z * x);
						break;
						case PlanetSideType.Bottom:
							step = (ulPlot - lrPlot) / (float)Segments;
							vertices[current] = new Vector3(llPlot.x - step.x * x, llPlot.y, llPlot.z + step.z * y);
						break;
					}

					//Convert to spherical and enlarge
					vertices[current] = PlanetUtil.CubeToSphere0(vertices[current]) * tree.Planet.Radius;
					Vector3 currentDirection = vertices[current].normalized;
#endregion Calculate Vertices

#region Calculate Triangles
					//Calculate Triangles
					if(y < Segments && x < Segments) {
						triangles[index] = y * segmentsPlusOne + x;
						index++;
						triangles[index] = (y+1) * segmentsPlusOne + x;
						index++;
						triangles[index] = y * segmentsPlusOne + (x+1);
						index++;

						triangles[index] = y * segmentsPlusOne + (x+1);
						index++;
						triangles[index] = (y+1) * segmentsPlusOne + x;
						index++;
						triangles[index] = (y+1) * segmentsPlusOne + (x+1);
						index++;
					}
#endregion Calculate Triangles

#region Calculate UVs
					xRatio = (float)x / (float)Segments;
					yRatio = (float)y / (float)Segments;
					switch(tree.Side) {
						case PlanetSideType.Back:
							uv.x = _InterpolationUtil._Linear(llPlot.x, lrPlot.x, xRatio) * 0.5f + 0.5f;
							uv.y = _InterpolationUtil._Linear(llPlot.y, ulPlot.y, yRatio) * 0.5f + 0.5f;
						break;
						case PlanetSideType.Right:
							uv.x = _InterpolationUtil._Linear(llPlot.z, lrPlot.z, xRatio) * 0.5f + 0.5f;
							uv.y = _InterpolationUtil._Linear(llPlot.y, ulPlot.y, yRatio) * 0.5f + 0.5f;
						break;
						case PlanetSideType.Top:
							uv.x = _InterpolationUtil._Linear(llPlot.x, lrPlot.x, xRatio) * 0.5f + 0.5f;
							uv.y = _InterpolationUtil._Linear(llPlot.z, ulPlot.z, yRatio) * 0.5f + 0.5f;
						break;
						case PlanetSideType.Front:
							uv.x = 1f - (_InterpolationUtil._Linear(llPlot.x, lrPlot.x, xRatio) * 0.5f + 0.5f);
							uv.y =_InterpolationUtil._Linear(lrPlot.y, urPlot.y, yRatio) * 0.5f + 0.5f;
						break;
						case PlanetSideType.Left:
							uv.x = 1f - (_InterpolationUtil._Linear(llPlot.z, lrPlot.z, xRatio) * 0.5f + 0.5f);
							uv.y = _InterpolationUtil._Linear(lrPlot.y, urPlot.y, yRatio) * 0.5f + 0.5f;
						break;
						case PlanetSideType.Bottom:
							uv.x = 1f - (_InterpolationUtil._Linear(llPlot.x, lrPlot.x, xRatio) * 0.5f + 0.5f);
							uv.y = _InterpolationUtil._Linear(llPlot.z, ulPlot.z, yRatio) * 0.5f + 0.5f;
						break;
					}
					uvs[current] = uv;

					uv = Vector2.zero;
					uv = PlanetUtil.DirectionToSphericalUV(currentDirection);
					switch(tree.Side) {
						case PlanetSideType.Top:
						case PlanetSideType.Back:
							if(_MathUtil._IsZero(lrPlot.x) && _MathUtil._IsZero(urPlot.x) && _MathUtil._IsZero(uv.x))
								uv.x = 1.0f;
						break;
					}
					uvs4[current] = uv;
#endregion Calculate UVs

#region Get Data
					//Unfortunately, this is using Unity Api, so, no multi-threading for you
					Vector4 data = (Vector4)tree.Planet.DataMap.GetPixelBilinear(uvs4[current].x, uvs4[current].y);
					
					//int xMap = (int)Mathf.RoundToInt(InterpolationUtil.Linear(0, tree.Planet.Data.GetLength(0)-1, uvs4[current].x));
					//int yMap = (int)Mathf.RoundToInt(InterpolationUtil.Linear(0, tree.Planet.Data.GetLength(1)-1, uvs4[current].y));
					//Vector4 data = tree.Planet.Data[xMap, yMap];
#endregion Get Data

#region Calculate Heights
					//Calculate Heights
					float height = data.w * 2.0f - 1.0f;

					if((height * tree.Planet.GroundThicknessHalf) > highestPoint)
						highestPoint = (height * tree.Planet.GroundThicknessHalf);

					if(tree.Surface == PlanetSurfaceType.Ground) {
						if(lod >= 0) {
							vertices[current] += currentDirection * (height * tree.Planet.GroundThicknessHalf);
						}
					}

					if(tree.Surface == PlanetSurfaceType.Water) {
						highestPoint = 0f;
					}
#endregion Calculate Heights

#region Calculate Normals and Tangents
					Quaternion rot = Quaternion.FromToRotation(Vector3.forward, currentDirection);
					Quaternion rot2 = Quaternion.LookRotation(rot * Vector3.forward, Vector3.up);

					//Already normalized for water and others except ground
					normals[current] = rot * Vector3.forward;
					Vector3 tangent = rot2 * -Vector3.right;
					if(tree.Surface == PlanetSurfaceType.Ground) {
						Vector3 normal = new Vector3(-(data.x * 2.0f - 1.0f), (data.y * 2.0f - 1.0f), data.z * 0.1f);
						normal.z += (_LOD._Max-lod) * 0.2f; //Warp normals with the lod level for good lighting
						normal.Normalize();
						//Correct normals planetwide, funny tangents need another fix of rotation but is it really necessary?
						normals[current] += rot2 * normal;
						normals[current].Normalize();
						
						//Very unnecessary
						//tangent.x += -normal.x;
						//tangent.Normalize();
					}
					tangents[current] = new Vector4(tangent.x, tangent.y, tangent.z, 1.0f);

					//Incorrect normals except back, right and half of top and bottom
					//Good normals between tropics, slower
					//normals[current] = rot * normal;


					//Correct normals except top and bottom, but better approximation and faster
					//Good between tropics
					//normals[current] = (currentDirection + (rot * normal)).normalized;
/*
					switch(tree.Side) {
						case PlanetSideType.Back:
						case PlanetSideType.Right:
						case PlanetSideType.Front:
						case PlanetSideType.Left:
							binormal = Vector3.up;
						break;
						case PlanetSideType.Top:
						case PlanetSideType.Bottom:
							binormal = Vector3.forward;
						break;
					}
*/
					//tangent = -Vector3.Cross(binormal, normals[current]);
					//tangents[current] = new Vector4(tangent.x, tangent.y, tangent.z, 1.0f);
#endregion Calculate Normals and Tangents

#region Calculate Vertex Colors
					//Calculate Colors
					float heightVal = _MathUtil._Clamp(data.w, 0f, 1f);
					if(tree.Surface == PlanetSurfaceType.Water) {
						heightVal = _MathUtil._Clamp(data.w, 0f, 0.5f) * 2f;
					}

					float polarVal = _MathUtil._Clamp(Mathf.Abs(currentDirection.y), 0f, 1f);

					float steepVal = 0f;
					if(tree.Surface == PlanetSurfaceType.Ground) {
						steepVal = 1.0f - _MathUtil._ClampDown(Vector3.Dot(normals[current], currentDirection), 0f);
					}

					colors[current] = new Color(heightVal, polarVal, steepVal, 0);
#endregion Calculate Vertex Colors
				}
			}

#region Calculate Borders
			//Calculate Borders
			if(tree.Surface == PlanetSurfaceType.Ground) {
				if(lod >= 0) {
					float borderDepth = tree.Planet.GroundThickness * -2f;
					vertCount = segmentsPlusOne * segmentsPlusOne;
					for(int y = 0; y <= Segments; y++) {
						int x = 0;
						current = y * segmentsPlusOne + x;
						vertices[vertCount] = vertices[current] + vertices[current].normalized * borderDepth;
						// calculate triangle indexes
						if(y < Segments) {		
							triangles[index] = (y * segmentsPlusOne + x);
							index++;
							triangles[index] = vertCount;
							index++;
							triangles[index] = (y+1) * segmentsPlusOne + x;
							index++;
								
							triangles[index] = (y+1) * segmentsPlusOne + x;
							index++;
							triangles[index] = vertCount;
							index++;
							triangles[index] = vertCount + 1;
							index++;
						}
						uvs[vertCount] = uvs[current] + new Vector2(-0.1f, 0);
						normals[vertCount] = normals[current];
						tangents[vertCount] = tangents[current];
						colors[vertCount] = new Color(0, 0, 0, 0);
						vertCount++;
					}

					for(int x = 0; x <= Segments; x++) {
						int y = 0;
						current = y * segmentsPlusOne + x;
						vertices[vertCount] = vertices[current] + vertices[current].normalized * borderDepth;
						// calculate triangle indexes
						if(x < Segments) {		
							triangles[index] = (y * segmentsPlusOne + x);
							index++;
							triangles[index] = y * segmentsPlusOne + (x+1);
							index++;
							triangles[index] = vertCount;
							index++;
								
							triangles[index] = vertCount;
							index++;
							triangles[index] = y * segmentsPlusOne + (x+1);
							index++;
							triangles[index] = vertCount + 1;
							index++;
						}
						uvs[vertCount] = uvs[current] + new Vector2(0, -0.1f);
						normals[vertCount] = normals[current];
						tangents[vertCount] = tangents[current];
						colors[vertCount] = new Color(0, 0, 0, 0);
						vertCount++;
					}

					for(int y = 0; y <= Segments; y++) {
						int x = Segments;
						current = y * segmentsPlusOne + x;
						vertices[vertCount] = vertices[current] + vertices[current].normalized * borderDepth;
						// calculate triangle indexes
						if(y < Segments) {		
							triangles[index] = (y * segmentsPlusOne + x);
							index++;
							triangles[index] = (y+1) * segmentsPlusOne + x;
							index++;
							triangles[index] = vertCount;
							index++;
								
							triangles[index] = (y+1) * segmentsPlusOne + x;
							index++;
							triangles[index] = vertCount + 1;
							index++;
							triangles[index] = vertCount;
							index++;
						}
						uvs[vertCount] = uvs[current] + new Vector2(0.1f, 0);
						normals[vertCount] = normals[current];
						tangents[vertCount] = tangents[current];
						colors[vertCount] = new Color(0, 0, 0, 0);
						vertCount++;
					}

					for(int x = 0; x <= Segments; x++) {
						int y = Segments;
						current = y * segmentsPlusOne + x;
						vertices[vertCount] = vertices[current] + vertices[current].normalized * borderDepth;
						// calculate triangle indexes
						if(x < Segments) {		
							triangles[index] = (y * segmentsPlusOne + x);
							index++;
							triangles[index] = vertCount;
							index++;
							triangles[index] = y * segmentsPlusOne + (x+1);
							index++;
								
							triangles[index] = vertCount;
							index++;
							triangles[index] = vertCount + 1;
							index++;
							triangles[index] = y * segmentsPlusOne + (x+1);
							index++;
						}
						uvs[vertCount] = uvs[current] + new Vector2(0, 0.1f);
						normals[vertCount] = normals[current];
						tangents[vertCount] = tangents[current];
						colors[vertCount] = new Color(0, 0, 0, 0);
						vertCount++;
					}
				}
			}
#endregion Calculate Borders
		}

		private void ApplyMeshData() {
			if(this.Go != null) {
				if(this.Trs != null) {
					this.PosL = Vector3.zero;
					this.RotL = Quaternion.identity;
					this.Scale = Vector3.one;
				}

				if(mesh != null) {
					mesh.Clear();
					mesh.vertices = vertices;
					mesh.triangles = triangles;
					mesh.uv = uvs;
					mesh.uv4 = uvs4;
					mesh.normals = normals;
					mesh.tangents = tangents;
					mesh.colors = colors;

					vertices = null;
					triangles = null;
					uvs = null;
					uvs4 = null;
					normals = null;
					tangents = null;
					colors = null;
				}

				if(lod >= 0) {
					if(meshCollider != null && !meshCollider.enabled) {
						meshCollider.enabled = true;
						meshCollider.sharedMesh = mesh;
					}
					else {
						meshCollider = (MeshCollider)this.Go.AddComponent(typeof(MeshCollider));
						meshCollider.enabled = true;
						meshCollider.sharedMesh = mesh;
					}
				}

				if(parent != null) {
					if(parent.meshRenderer != null && parent.meshRenderer.enabled) {
						parent.meshRenderer.enabled = false;
					}

					if(parent.meshCollider != null && parent.meshCollider.enabled) {
						parent.meshCollider.enabled = false;
					}
				}

				isGenerated = true;

				CreateNature();
				CreateWildLife();
			}
		}
#endregion MeshData Methods

#region Update Methods
		internal void UpdateNode() {
			if(lod < _LOD._Max) {
				float closestCornerDistance = CalculateClosestCorner();

				if(!IsQueued) {
					if(isGenerated) {
						if(!hasChildren) {
							if(closestCornerDistance <= tree.Planet.LODDistances[lod] && tree.Planet.CoreDistToEye <= tree.Planet.LODHeights[lod] + highestPoint) {
								_LOD._Enqueue(this);
							}
							else {
								CalculateCulling();
							}
						}
						else {
							if(closestCornerDistance > tree.Planet.LODDistances[lod] || tree.Planet.CoreDistToEye > tree.Planet.LODHeights[lod] + highestPoint) {
								DeleteChildren();
							}
							else {
								for(int i = 0; i < children.Count; i++) {
									children[i].UpdateNode();
								}
							}
						}
					}
				}
				else {
					if(closestCornerDistance > tree.Planet.LODDistances[lod] || tree.Planet.CoreDistToEye > tree.Planet.LODHeights[lod] + highestPoint) {
						_LOD._Dequeue(this);
					}
				}
			}
		}

		private float CalculateClosestCorner() {
			Vector3 closestCorner = Vector3.positiveInfinity;
			float closestCornerDistance = Mathf.Infinity;
			//Vector3 eye = this.Trs.TransformPoint(tree.Planet.DirToEyeNormalized * tree.Planet.Radius);
			Vector3 pos = this.Trs.TransformPoint(llCorner);
			float distance = _DistUtil._Get3D(tree.Planet.EyePosOnSurface, pos);
			if(distance < closestCornerDistance) {
				closestCorner = pos;
				closestCornerDistance = distance;
			}

			pos = this.Trs.TransformPoint(ulCorner);
			distance = _DistUtil._Get3D(tree.Planet.EyePosOnSurface, pos);
			if(distance < closestCornerDistance) {
				closestCorner = pos;
				closestCornerDistance = distance;
			}

			pos = this.Trs.TransformPoint(lrCorner);
			distance = _DistUtil._Get3D(tree.Planet.EyePosOnSurface, pos);
			if(distance < closestCornerDistance) {
				closestCorner = pos;
				closestCornerDistance = distance;
			}

			pos = this.Trs.TransformPoint(urCorner);
			distance = _DistUtil._Get3D(tree.Planet.EyePosOnSurface, pos);
			if(distance < closestCornerDistance) {
				closestCorner = pos;
				closestCornerDistance = distance;
			}

			pos = this.Trs.TransformPoint(mCorner);
			distance = _DistUtil._Get3D(tree.Planet.EyePosOnSurface, pos);
			if(distance < closestCornerDistance) {
				closestCorner = pos;
				closestCornerDistance = distance;
			}

			priority = closestCornerDistance;
			float closestCornerAngle = Vector3.Angle(tree.Planet.DirToEyeNormalized, _DirUtil._Get3DNormalized(tree.Planet.PosW, closestCorner));
			isInHorizon = (closestCornerAngle <= tree.Planet.CullAngle);
			isInFrustum = GeometryUtility.TestPlanesAABB(_EYES._Frustum, meshRenderer.bounds);
			
			return closestCornerDistance;
		}
/*
		private void CalculateCulling() {
			EnableRenderers1();
			//bool shouldBeVisible = isInHorizon && isInFrustum;
			//EnableRenderers(shouldBeVisible, isInHorizon, shouldBeVisible);
		}
*/
		private void CalculateCulling() {
			bool isInVisibleHorizon = isInHorizon && isInFrustum;
			bool isInBackHorizon = isInHorizon && !isInFrustum;
			bool isInHorizonOrFrustum = isInHorizon || isInFrustum;
			bool isNotInClosestPlanet = _EYES._ClosestPlanet != tree.Planet;

			if(meshRenderer != null) {
				if(tree.Surface.Is(PlanetSurfaceType.Ground)) {
					meshRenderer.shadowCastingMode = ShadowCastingMode.TwoSided;
					meshRenderer.shadowCastingMode = isInBackHorizon ? ShadowCastingMode.ShadowsOnly : meshRenderer.shadowCastingMode;
					meshRenderer.shadowCastingMode = isNotInClosestPlanet ? ShadowCastingMode.Off : meshRenderer.shadowCastingMode;
				}
/*
				if(meshRenderer.enabled != isInHorizonOrFrustum) {
					meshRenderer.enabled = isInHorizonOrFrustum;
				}
*/
				if(meshRenderer.enabled != isInHorizon) {
					meshRenderer.enabled = isInHorizon;
				}
			}

			if(meshCollider != null && meshCollider.enabled != (isInHorizon && !isNotInClosestPlanet)) {
				meshCollider.enabled = (isInHorizon && !isNotInClosestPlanet);
			}

			if(hasNature) {
				if(goNatureRoot != null && goNatureRoot.activeInHierarchy != (isInVisibleHorizon && !isNotInClosestPlanet)) {
					goNatureRoot.SetActive((isInVisibleHorizon && !isNotInClosestPlanet));
				}
			}

			if(hasWildLife) {
				if(goWildLifeRoot != null && goWildLifeRoot.activeInHierarchy != (isInVisibleHorizon && !isNotInClosestPlanet)) {
					goWildLifeRoot.SetActive((isInVisibleHorizon && !isNotInClosestPlanet));
				}
			}
		}
/*
		private void EnableRenderers(bool rendererState, bool colliderState, bool natureState) {
			if(meshRenderer != null && meshRenderer.enabled != rendererState) {
				meshRenderer.enabled = rendererState;
			}

			if(meshCollider != null && meshCollider.enabled != colliderState) {
				meshCollider.enabled = colliderState;
			}

			if(hasNature) {
				if(goNatureRoot != null && goNatureRoot.activeInHierarchy != natureState) {
					goNatureRoot.SetActive(natureState);
				}
			}

			if(hasWildLife) {
				if(goWildLifeRoot != null && goWildLifeRoot.activeInHierarchy != natureState) {
					goWildLifeRoot.SetActive(natureState);
				}
			}
		}
*/
#endregion Update Methods

#region Object Methods
		//Comparers//
		public int CompareTo(PlanetTreeNode other) {
			if(other == null)
				return 1;
			return this.priority.CompareTo(other.priority);
		}

		public int CompareTo(object other) {
			if(other is PlanetTreeNode)
				return CompareTo((PlanetTreeNode)other);
			else {
				Debug.LogError("Compared object is not typeof PlanetTreeNode class.");
				throw new ArgumentException("Compared object is not typeof PlanetTreeNode class.");
			}
		}
#endregion Object Methods
	}
}