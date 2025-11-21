using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TOZ {

	public abstract class GameObjectBase : MonoBehaviour {
#region Properties
		public GameObject Go { get { return gameObject; } }
		public Transform Trs { get { return transform; } }
		public Vector3 PosL { get { return Trs.localPosition; } set { Trs.localPosition = value; } }
		public Vector3 PosW { get { return Trs.position; } set { Trs.position = value; } }
		public Quaternion RotL { get { return Trs.localRotation; } set { Trs.localRotation = value; } }
		public Quaternion RotW { get { return Trs.rotation; } set { Trs.rotation = value; } }
		public Vector3 Scale { get { return Trs.localScale; } set { Trs.localScale = value; } }
		public Vector3 Right { get { return Trs.right; } }
		public Vector3 Up { get { return Trs.up; } }
		public Vector3 Forward { get { return Trs.forward; } }
#endregion Properties
	}

}