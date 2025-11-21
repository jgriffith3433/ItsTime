using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TOZ;

namespace TOZ.Celestial {

	public abstract class PlanetoidBase : CelestialBase {
#region Properties

#endregion Properties

#region Variables
		//Variables
		[Header("LOD Settings")]
		[SerializeField] internal PlanetQualityType Quality = PlanetQualityType.Normal;

		internal Vector3 EyePosOnSurface;
		internal float CullAngle;
		internal float[] LODDistances;
		internal float[] LODHeights;

		internal int TotalQuads;
		internal int TotalVertices;

		[Header("DataMap Settings")]
		[SerializeField] private Texture2D dataMap = null;
		internal Texture2D DataMap;
		internal Vector4[,] Data;
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
			EyePosOnSurface = this.PosW;
			CullAngle = _LOD._Angle;

			LODDistances = new float[] {
				this.SideSize, //Max LOD 1
				this.SideSize / 2.0f, //Max LOD 2
				this.SideSize / 4.0f, //Max LOD 3
				this.SideSize / 8.0f, //Max LOD 4
				this.SideSize / 16.0f, //Max LOD 5
				this.SideSize / 32.0f, //Max LOD 6
				//this.SquareLength / 64.0f //Max LOD 7 // NOT USED
			};

			LODHeights = new float[] {
				//this.Radius + GroundThickness * 12.0f,
				this.Diameter * 2.0f, 						//Max LOD 1
				this.Diameter, 								//Max LOD 2
				this.NavigationRadius,						//Max LOD 3
				this.Radius + GroundThickness * 3.0f, //Max LOD 4
				this.Radius + GroundThickness * 1.5f, //Max LOD 5
				this.Radius + GroundThickness * 0.6f, //Max LOD 6
				//this.Radius + GroundThickness * 0.4f  //Max LOD 7 // NOT USED
			};

			switch(Quality) {
				case PlanetQualityType.Low: 
					TotalQuads = _LOD._Quads;
				break;
				case PlanetQualityType.Medium:
					TotalQuads = _LOD._Quads * 2;
				break;
				case PlanetQualityType.Normal:
					TotalQuads = _LOD._Quads * 4;
				break;
				case PlanetQualityType.Good:
					TotalQuads = _LOD._Quads * 8;
				break;
				case PlanetQualityType.High:
					TotalQuads = _LOD._Quads * 16;
				break;
				case PlanetQualityType.Ultra:
					TotalQuads = _LOD._Quads * 32;
				break;
				default:
					TotalQuads = _LOD._Quads;
				break;
			}
			TotalVertices = TotalQuads + 1;
		}

		private void DoEnable() {
			CreateDataMap();
			
			Debug.Log(this +
							", Angle: " + CullAngle +
							", Total Divisions: " + _LOD._Divisions +
							", Total Quads: " + TotalQuads +
							", Data Width: " + ((TotalQuads * 4) + 1) +
							", Data Height: " + ((TotalQuads * 2) + 1) +
							", Quad Width: " + ((SideSize / TotalQuads) * 2.0f)
			);
		}

		private void DoDisable() {
			DeleteDataMap();
		}

		private void OnDrawGizmosSelected() {
			if(LODDistances != null && LODDistances.Length > 0) {
				Vector3 pos = this.Trs.TransformPoint(this.DirToEyeNormalized * this.Radius);
				/*
				Gizmos.color = Color.yellow;
				for(int i = 0; i < LODDistances.Length; i++) {
					Gizmos.DrawWireSphere(pos, LODDistances[i]);
				}
				*/
				float coneRadius = Mathf.Min(this.Radius, _GeomUtil._CalculateConeRadius(this.Radius, CullAngle)) * this.Scale.x;
				//Debug.Log(coneRadius);
				float angleToHorizon = PlanetUtil.CalculateCullingAngle(this.CoreDistToEye, this.Radius);
				//Debug.Log(angleToHorizon);
				float distToHorizon = PlanetUtil.CalculateDistanceToHorizon(this.CoreDistToEye, this.Radius);
				//Debug.Log(distToHorizon);
#if UNITY_EDITOR
				for(int i = 0; i < LODDistances.Length; i++) {
					UnityEditor.Handles.DrawWireDisc(pos, this.DirToEyeNormalized, LODDistances[i] * this.Scale.x, 2f);
					_UIUtil._Hndl_LabelCenter(pos + Vector3.up * LODDistances[i], _UIUtil._Label("LOD: ", "yellow") + _UIUtil._Text(i.ToString(), "yellow"));
				}

				//float coneRadius = GeomUtil.CalculateConeRadius(this.Radius, CullAngle);
				UnityEditor.Handles.color = Color.cyan;
				UnityEditor.Handles.DrawWireDisc(pos, this.DirToEyeNormalized, coneRadius, 2f);
				
				Quaternion quat = Quaternion.LookRotation(this.DirToEyeNormalized);
				Vector3 up = (quat * Vector3.up) * coneRadius;
				Vector3 right = (quat * Vector3.right) * coneRadius;
				Vector3 crossL = (quat * new Vector3(-1, 1, 0).normalized) * coneRadius;
				Vector3 crossR = (quat * new Vector3(1, 1, 0).normalized) * coneRadius;
				
				UnityEditor.Handles.DrawLine(this.PosW, pos + up);
				UnityEditor.Handles.DrawLine(this.PosW, pos - up);
				UnityEditor.Handles.DrawLine(this.PosW, pos + right);
				UnityEditor.Handles.DrawLine(this.PosW, pos - right);
				UnityEditor.Handles.DrawLine(this.PosW, pos + crossL);
				UnityEditor.Handles.DrawLine(this.PosW, pos - crossL);
				UnityEditor.Handles.DrawLine(this.PosW, pos + crossR);
				UnityEditor.Handles.DrawLine(this.PosW, pos - crossR);
#endif
			}
		}
#endregion Methods

#region Helper Methods
		protected void UpdatePlanetoid() {
			EyePosOnSurface = this.Trs.TransformPoint(this.DirToEyeNormalized * this.Radius);
			CullAngle = PlanetUtil.CalculateCullingAngle(Mathf.Max(this.CoreDistToEye, this.CloudsRadius), this.FloorRadius);
		}

		private void CreateDataMap() {
			DataMap = (dataMap != null) ? dataMap : Star._DataMap;
/*
			Color[,] cols = ArrayUtil.MonoToDuo(DataMap.GetPixels(0), DataMap.width, DataMap.height);
			Data = new Vector4[DataMap.width, DataMap.height];
			//Debug.Log(cols.GetLength(0)-1);
			for(int y=0; y<DataMap.height; y++) {
				for(int x=0; x<DataMap.width; x++) {
					Data[x,y] = (Vector4)cols[x,y];
				}
			}
			cols = null;
*/
		}

		private void DeleteDataMap() {
			DataMap = null;
			//Data = null;
		}
#endregion Helper Methods

#region Object Methods
		//Overrides//
		public override string ToString() {
			return "Planetoid Index: {0}, Name: {1}".FormatWith(this.Index, this.Name);
		}
#endregion Object Methods
	}

}