using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using TOZ;

namespace TOZ.Celestial {

	public abstract class CelestialBase : GameObjectBase, IIndexed, IReticleObject, IComparable<CelestialBase> {
#region Static List
		//Variables
		public static DataContainer<CelestialBase> _List;

		//Constructor
		static CelestialBase() {
			_List = new DataContainer<CelestialBase>();
		}
#endregion Static List

#region Properties
		//IIndexed Implementations
		public int ID { get { return GetInstanceID(); } }
		public int Index { get { return _List.IndexOf(this); } }

		//IReticleObject Implementations
		public Vector3 ScrPos { get { return _VecUtil._ClampToScreen(_EYES._MainCam.WorldToScreenPoint(PosW)); } }
		public float ScrSize { get { return _GeomUtil._GetAngularSize(Trs, _EYES._MainCam, Diameter); } }

		internal float SDotC { get { return Vector3.Dot(DirToSunNormalized, DirToEyeNormalized); } }

		public float Diameter { get { return Radius * 2.0f; } }
		public float SideSize { get { return Radius / (float)_MathUtil._SQRT2; } }
		public float Circumference { get { return Diameter * Mathf.PI; } }
		public float GroundThickness { get { return Radius * 0.025f; } }
		public float GroundThicknessHalf { get { return GroundThickness * 0.5f; } }
		public float CloudsThickness { get { return GroundThickness * 1.5f; } }
		public float AtmosphereThickness { get { return GroundThickness * 3.0f; } }
		
		public float FloorRadius { get { return Radius - GroundThicknessHalf; } }
		public float PeakRadius { get { return Radius + GroundThicknessHalf; } }
		public float CloudsRadius { get { return Radius + CloudsThickness; } }
		public float AtmosphereRadius { get { return Radius + AtmosphereThickness; } }
		public float OrbitRadius { get { return Radius + (AtmosphereThickness * 1.5f); } }
		public float NavigationRadius { get { return Radius + (AtmosphereThickness * 4.0f); } }
#endregion Properties

#region Variables
		//Variables
		[Header("Info Settings")]
		[NonSerialized] public CelestialType Type = CelestialType.None;
		public string Name = "A Celestial Object";
		public bool IsMoon = false;
		public CelestialBase Parent = null;

		[Header("Creation Settings")]
		[Range(0f, 10f)] public float GravityScale = 1.0f;
		public float Radius = 256f;

		[NonSerialized] public Vector3 DirToEye;
		[NonSerialized] public Vector3 DirToEyeNormalized;
		[NonSerialized] public float CoreDistToEye;
		[NonSerialized] public float FloorDistToEye;
		[NonSerialized] public float SeaDistToEye;
		[NonSerialized] public float PeakDistToEye;
		[NonSerialized] public float CloudsDistToEye;
		[NonSerialized] public float AtmosphereDistToEye;
		[NonSerialized] public float OrbitDistToEye;
		[NonSerialized] public float NavigationDistToEye;

		[NonSerialized] public Vector3 DirToSun;
		[NonSerialized] public Vector3 DirToSunNormalized;
		[NonSerialized] public float CoreDistToSun;
#endregion Variables

#region Mono Methods
		protected virtual void OnEnable() {
			DoEnable();
		}

		protected virtual void OnDisable() {
			DoDisable();
		}
#endregion Mono Methods

#region Methods
		private void DoEnable() {
			_List.Add(this);

			//Waypoints
			CreateSphericalWaypoints();
		}

		private void DoDisable() {
			_List.Remove(this);

			//Waypoints
			_NAV._Update();
		}
#endregion Methods

#region Helper Methods
		internal void UpdateCelestial() {
			DirToEye = _DirUtil._Get3D(this.PosW, _EYES._Head.position);
			DirToEyeNormalized = DirToEye.normalized;
			CoreDistToEye = DirToEye.magnitude;
			FloorDistToEye = CoreDistToEye - FloorRadius;
			SeaDistToEye = CoreDistToEye - Radius;
			PeakDistToEye = CoreDistToEye - PeakRadius;
			CloudsDistToEye = CoreDistToEye - CloudsRadius;
			AtmosphereDistToEye = CoreDistToEye - AtmosphereRadius;
			OrbitDistToEye = CoreDistToEye - OrbitRadius;
			NavigationDistToEye = CoreDistToEye - NavigationRadius;

			if(_SS._Star != null) {
				switch(Type) {
					case CelestialType.Sun:
						DirToSun = Vector3.zero;
						DirToSunNormalized = Vector3.zero;
						CoreDistToSun = 0f;
					break;
					case CelestialType.Planet:
						DirToSun = _DirUtil._Get3D(this.PosW, _SS._Star.PosW);
						DirToSunNormalized = DirToSun.normalized;
						CoreDistToSun = DirToSun.magnitude;
					break;
					default:
						DirToSun = Vector3.zero;
						DirToSunNormalized = Vector3.zero;
						CoreDistToSun = 0f;
					break;
				}
			}
		}

		internal void CreateSphericalWaypoints() {
			for(int i = 0; i < _NAV._SphericalData.Length; i++) {
				_NAV._Add(this.PosW + (_NAV._SphericalData[i] * NavigationRadius));
			}
		}
#endregion Helper Methods

#region Object Methods
		//Comparers
		public int CompareTo(CelestialBase other) {
			if(other == null)
				return 1;
			return this.CoreDistToSun.CompareTo(other.CoreDistToSun);
		}

		public int CompareTo(object other) {
			Assert.IsTrue(other is CelestialBase, "Compared object is not typeof CelestialBase class.");
			return CompareTo((CelestialBase)other);
		}

		//Overrides
		public static bool operator ==(CelestialBase a, CelestialBase b) {
			if(System.Object.ReferenceEquals(a, b))
				return true;
			if(((object)a == null) || ((object)b == null))
				return false;
			return (a.ID == b.ID);
		}

		public static bool operator !=(CelestialBase a, CelestialBase b) {
			return !(a == b);
		}

		public override bool Equals(System.Object obj) {
			if(obj == null)
				return false;
			CelestialBase other = obj as CelestialBase;
			if((System.Object)other == null)
				return false;
			return (this == other);
		}

		public bool Equals(CelestialBase other) {
			if((object)other == null)
				return false;
			return (this == other);
		}

		public override string ToString() {
			return "CelestialBase Index: {0}, Name: {1}".FormatWith(this.Index, this.Name);
		}

		public override int GetHashCode() {
			return _HashUtil._Get(this.ID);
		}
#endregion Object Methods
	}

}