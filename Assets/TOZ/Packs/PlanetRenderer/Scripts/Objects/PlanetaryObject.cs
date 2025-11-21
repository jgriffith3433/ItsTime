using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TOZ.Celestial {

	[Serializable]
	public sealed class PlanetaryObject {
#region Variables
		//Variables//
		public GameObject Prefab = null;
		[Range(0, 5000)] public int MaxQuantityPerNode = 10;
		[Range(-1f, 1f)] public float MinHeight = 0f;
		[Range(-1f, 1f)] public float MaxHeight = 0.15f;
		[Range(0f, 1f)] public float MinSteepness = 0f;
		[Range(0f, 1f)] public float MaxSteepness = 0.15f;
		[Range(0f, 100f)] public float MinScale = 1f;
		[Range(0f, 100f)] public float MaxScale = 1f;
		public bool AlignToGroundNormals = false;
		[NonSerialized] public GameObject[] GOs = null;
#endregion Variables
	}

}