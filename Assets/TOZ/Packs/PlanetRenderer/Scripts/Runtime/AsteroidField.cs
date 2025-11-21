using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TOZ;

namespace TOZ.Celestial {

	public sealed class AsteroidField : GameObjectBase, IComparable<AsteroidField> {
#region Static List
		//Variables
		internal static List<AsteroidField> _Data;

		//Constructor
		static AsteroidField() {
			_Clear();
		}

		//List Methods
		internal static void _Clear() {
			if(_Data != null) {
				_Data.Clear();
			}
			else {
				_Data = new List<AsteroidField>();
			}
		}

		internal static void _Add(AsteroidField i) {
			if(_Data != null && !_Contains(i)) {
				_Data.Add(i);
				_Sort();
			}
		}

		internal static void _Insert(int index, AsteroidField i) {
			if(_Data != null && !_Contains(i)) {
				_Data.Insert(index, i);
				_ArrangeIDs();
			}
		}

		internal static void _Remove(AsteroidField i) {
			if(_Data != null && _Contains(i)) {
				_Data.Remove(i);
				_ArrangeIDs();
			}
		}

		internal static void _RemoveAt(int index) {
			if(_Data != null) {
				_Data.RemoveAt(index);
				_ArrangeIDs();
			}
		}

		internal static int _Count() {
			return _Data.Count;
		}

		internal static bool _Contains(AsteroidField i) {
			return _Data.Contains(i);
		}

		internal static void _Reverse() {
			if(_Data != null) {
				_Data.Reverse();
				_ArrangeIDs();
			}
		}

		internal static void _Sort() {
			if(_Data != null) {
				_Data.Sort();
				_ArrangeIDs();
			}
		}

		internal static AsteroidField[] _Array() {
			return _Data.ToArray();
		}

		private static void _ArrangeIDs() {
			for(int i = 0; i < _Count(); i++) {
				_Data[i].Id = i;
			}
		}
#endregion Static List

#region Variables
		//Variables
		internal int Id;

		[Header("Info Settings")]
		public string Name = "An Asteroid Field";

		[Header("Creation Settings")]
		[SerializeField] private SO_SplineData path = null;
		[SerializeField] private int amount = 200;
		[SerializeField] private float scatterScale = 500f;
		[SerializeField] private float minScale = 1f;
		[SerializeField] private float maxScale = 10f;
		[SerializeField] private GameObject[] asteroidPrefabs = null;
		internal GameObject Root = null;
#endregion Variables

#region Mono Methods
		private void Awake() {
			DoAwake();
		}

		private void OnEnable() {
			DoEnable();
		}

		private void OnDisable() {
			DoDisable();
		}
#endregion Mono Methods

#region Methods
		private void DoAwake() {
			Root = this.Trs.GetChild(0).gameObject;

			Spline spline = new Spline(path.Type, path.Segments);
			spline.Initialize(path.Data, true);

			//We want same positions on every regeneration
			//Local randomization is done below
			//Although asteroids regenerate once
			_RandUtil._SetSeed(Id);
			for(int i = 0; i < amount; i++) {
				Vector3 rnd = _RandUtil._GetInsideUnitSphere() * scatterScale;
				rnd.y *= 0.2f;
				Vector3 pos = spline.GetNodeAtTime((float)i / (float)amount).P + rnd;
				Quaternion rot = UnityEngine.Random.rotation;
				int index = UnityEngine.Random.Range(0, asteroidPrefabs.Length);
				GameObject go = Instantiate(asteroidPrefabs[index], pos, rot, this.Root.transform);
				float scale = UnityEngine.Random.Range(minScale, maxScale);
				go.transform.localScale *= scale;
			}
			spline = null;
		}

		private void DoEnable() {
			Id = _Count();
			_Add(this);
		}

		private void DoDisable() {
			this.Id = -1;
			_Remove(this);
		}
#endregion Methods

#region Object Methods
		//Comparers//
		public int CompareTo(AsteroidField other) {
			if(other == null)
				return 1;
			return this.Id.CompareTo(other.Id);
		}

		public int CompareTo(object other) {
			if(other is AsteroidField)
				return CompareTo((AsteroidField)other);
			else {
				Debug.LogError("Compared object is not typeof AsteroidField class.");
				throw new ArgumentException("Compared object is not typeof AsteroidField class.");
			}
		}

		//Overrides//
		public static bool operator ==(AsteroidField a, AsteroidField b) {
			if(System.Object.ReferenceEquals(a, b))
				return true;
			if(((object)a == null) || ((object)b == null))
				return false;
			return (a.Id == b.Id);
		}

		public static bool operator !=(AsteroidField a, AsteroidField b) {
			return !(a == b);
		}

		public override bool Equals(System.Object obj) {
			if(obj == null)
				return false;
			AsteroidField other = obj as AsteroidField;
			if((System.Object)other == null)
				return false;
			return (this == other);
		}

		public bool Equals(AsteroidField other) {
			if((object)other == null)
				return false;
			return (this == other);
		}

		public override string ToString() {
			return "AsteroidField ID: {0}, Name: {1}".FormatWith(this.Id, this.Name);
		}

		public override int GetHashCode() {
			return _HashUtil._Get(this.Id);
		}
#endregion Object Methods
	}

}