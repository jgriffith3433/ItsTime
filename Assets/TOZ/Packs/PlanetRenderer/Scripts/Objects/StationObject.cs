using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TOZ.Celestial {

	/*Not used yet, this is for future update if/when we need plural stations per planet*/
	[Serializable]
	public sealed class StationObject {
#region Variables
		//Variables//
		public GameObject Prefab = null;
		public Vector3 Position = Vector3.zero;
#endregion Variables
	}

}