using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using TOZ;

namespace TOZEditor {

	public static class _CustomMenuItems {
		[MenuItem("Assets/Create/TOZ/SO_SplineData")]
		public static void Create_SO_SplineData() {
			_AssetUtil._Create<SO_SplineData>();
		}

		[MenuItem("Assets/Create/TOZ/SO_Vector3")]
		public static void Create_SO_Vector3() {
			_AssetUtil._Create<SO_Vector3>();
		}

		[MenuItem("Assets/Create/TOZ/SO_Vector4")]
		public static void Create_SO_Vector4() {
			_AssetUtil._Create<SO_Vector4>();
		}
	}

}