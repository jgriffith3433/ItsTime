using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TOZ {

	public static class _DistUtil {
		//2D Distance
		public static float _Get2D(Vector2 v1, Vector2 v2) {
			return (v1 - v2).magnitude;
		}

		public static float _Get2DSqr(Vector2 v1, Vector2 v2) {
			return (v1 - v2).sqrMagnitude;
		}

		//2D Distance (Y Up)
		public static float _GetXZ(Vector3 v1, Vector2 v2) {
			return _Get2D(_VecUtil._GetXZ(v1), v2);
		}

		public static float _GetXZ(Vector2 v1, Vector3 v2) {
			return _Get2D(v1, _VecUtil._GetXZ(v2));
		}

		public static float _GetXZ(Vector3 v1, Vector3 v2) {
			return _Get2D(_VecUtil._GetXZ(v1), _VecUtil._GetXZ(v2));
		}

		//2D Distance (X Up)
		public static float _GetYZ(Vector3 v1, Vector2 v2) {
			return _Get2D(_VecUtil._GetYZ(v1), v2);
		}

		public static float _GetYZ(Vector2 v1, Vector3 v2) {
			return _Get2D(v1, _VecUtil._GetYZ(v2));
		}

		public static float _GetYZ(Vector3 v1, Vector3 v2) {
			return _Get2D(_VecUtil._GetYZ(v1), _VecUtil._GetYZ(v2));
		}

		//2D Distance (Z Up)
		public static float _GetXY(Vector3 v1, Vector2 v2) {
			return _Get2D(_VecUtil._GetXY(v1), v2);
		}

		public static float _GetXY(Vector2 v1, Vector3 v2) {
			return _Get2D(v1, _VecUtil._GetXY(v2));
		}

		public static float _GetXY(Vector3 v1, Vector3 v2) {
			return _Get2D(_VecUtil._GetXY(v1), _VecUtil._GetXY(v2));
		}

		//3D Distance
		public static float _Get3D(Vector3 v1, Vector3 v2) {
			return (v1 - v2).magnitude;
		}

		public static float _Get3DSqr(Vector3 v1, Vector3 v2) {
			return (v1 - v2).sqrMagnitude;
		}

		//Spherical Distance (lat lon distance)
		public static float _GetSpherical(Vector2 v1, Vector2 v2, float radius) {
			float lat1 = v1.y * Mathf.Deg2Rad;
			float lon1 = v1.x * Mathf.Deg2Rad;
			float lat2 = v2.y * Mathf.Deg2Rad;
			float lon2 = v2.x * Mathf.Deg2Rad;
			float a = Mathf.Sin(lat1) * Mathf.Sin(lat2) + Mathf.Cos(lat1) * Mathf.Cos(lat2) * Mathf.Cos(lon1 - lon2);
			return Mathf.Acos(a) * radius;
		}
	}

}