using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TOZ {

	public static class _DirUtil {
		//2D Direction
		public static Vector2 _Get2D(Vector2 v1, Vector2 v2) { return (v2 - v1); }

		//2D Direction
		public static Vector2 _Get2DNormalized(Vector2 v1, Vector2 v2) { return _Get2D(v1, v2).normalized; }

		//3D Direction
		public static Vector3 _Get3D(Vector3 v1, Vector3 v2) { return (v2 - v1); }

		//3D Direction
		public static Vector3 _Get3DNormalized(Vector3 v1, Vector3 v2) { return _Get3D(v1, v2).normalized; }

		//2D Direction (Y Up)
		public static Vector2 _GetXZ(Vector3 v1, Vector2 v2) {
			return _Get2D(_VecUtil._GetXZ(v1), v2);
		}

		public static Vector2 _GetXZNormalized(Vector3 v1, Vector2 v2) {
			return _Get2DNormalized(_VecUtil._GetXZ(v1), v2);
		}

		public static Vector2 GetXZ(Vector2 v1, Vector3 v2) {
			return _Get2D(v1, _VecUtil._GetXZ(v2));
		}

		public static Vector2 GetXZNormalized(Vector2 v1, Vector3 v2) {
			return _Get2DNormalized(v1, _VecUtil._GetXZ(v2));
		}

		public static Vector2 _GetXZ(Vector3 v1, Vector3 v2) {
			return _Get2D(_VecUtil._GetXZ(v1), _VecUtil._GetXZ(v2));
		}

		public static Vector2 _GetXZNormalized(Vector3 v1, Vector3 v2) {
			return _Get2DNormalized(_VecUtil._GetXZ(v1), _VecUtil._GetXZ(v2));
		}

		public static Vector3 _GetPlanarXZ(Vector3 v1, Vector3 v2) {
			Vector2 dir = _GetXZ(v1, v2);
			return new Vector3(dir.x, 0f, dir.y);
		}

		public static Vector3 _GetPlanarXZNormalized(Vector3 v1, Vector3 v2) {
			Vector2 dir = _GetXZ(v1, v2);
			return new Vector3(dir.x, 0f, dir.y).normalized;
		}

		//2D Direction (X Up)
		public static Vector2 _GetYZ(Vector3 v1, Vector2 v2) {
			return _Get2D(_VecUtil._GetYZ(v1), v2);
		}

		public static Vector2 _GetYZNormalized(Vector3 v1, Vector2 v2) {
			return _Get2DNormalized(_VecUtil._GetYZ(v1), v2);
		}

		public static Vector2 _GetYZ(Vector2 v1, Vector3 v2) {
			return _Get2D(v1, _VecUtil._GetYZ(v2));
		}

		public static Vector2 _GetYZNormalized(Vector2 v1, Vector3 v2) {
			return _Get2DNormalized(v1, _VecUtil._GetYZ(v2));
		}

		public static Vector2 _GetYZ(Vector3 v1, Vector3 v2) {
			return _Get2D(_VecUtil._GetYZ(v1), _VecUtil._GetYZ(v2));
		}

		public static Vector2 GetYZNormalized(Vector3 v1, Vector3 v2) {
			return _Get2DNormalized(_VecUtil._GetYZ(v1), _VecUtil._GetYZ(v2));
		}

		public static Vector3 _GetPlanarYZ(Vector3 v1, Vector3 v2) {
			Vector2 dir = _GetYZ(v1, v2);
			return new Vector3(0f, dir.x, dir.y);
		}

		public static Vector3 _GetPlanarYZNormalized(Vector3 v1, Vector3 v2) {
			Vector2 dir = _GetYZ(v1, v2);
			return new Vector3(0f, dir.x, dir.y).normalized;
		}

		//2D Direction (Z Up)
		public static Vector2 _GetXY(Vector3 v1, Vector2 v2) {
			return _Get2D(_VecUtil._GetXY(v1), v2);
		}

		public static Vector2 _GetXYNormalized(Vector3 v1, Vector2 v2) {
			return _Get2DNormalized(_VecUtil._GetXY(v1), v2);
		}

		public static Vector2 _GetXY(Vector2 v1, Vector3 v2) {
			return _Get2D(v1, _VecUtil._GetXY(v2));
		}

		public static Vector2 _GetXYNormalized(Vector2 v1, Vector3 v2) {
			return _Get2DNormalized(v1, _VecUtil._GetXY(v2));
		}

		public static Vector2 _GetXY(Vector3 v1, Vector3 v2) {
			return _Get2D(_VecUtil._GetXY(v1), _VecUtil._GetXY(v2));
		}

		public static Vector2 _GetXYNormalized(Vector3 v1, Vector3 v2) {
			return _Get2DNormalized(_VecUtil._GetXY(v1), _VecUtil._GetXY(v2));
		}

		public static Vector3 _GetPlanarXY(Vector3 v1, Vector3 v2) {
			Vector2 dir = _GetXY(v1, v2);
			return new Vector3(dir.x, dir.y, 0f);
		}

		public static Vector3 _GetPlanarXYNormalized(Vector3 v1, Vector3 v2) {
			Vector2 dir = _GetXY(v1, v2);
			return new Vector3(dir.x, dir.y, 0f).normalized;
		}

		//Are the two directions looking at same directions
		public static bool _IsSame(Vector2 v1, Vector2 v2) {
			return Vector2.Dot(v1, v2) > Mathf.Epsilon;
		}

		//Are the two directions looking at same directions
		public static bool _IsSame(Vector3 v1, Vector3 v2) {
			return Vector3.Dot(v1, v2) > Mathf.Epsilon;
		}

		//Are the two directions opposite (first vector is backface)
		public static bool _IsOpposite(Vector2 v1, Vector2 v2) {
			return Vector2.Dot(v1, v2) < Mathf.Epsilon;
		}

		//Are the two directions opposite (first vector is backface)
		public static bool _IsOpposite(Vector3 v1, Vector3 v2) {
			return Vector3.Dot(v1, v2) < Mathf.Epsilon;
		}

		//Are the two directions perpendicular(first vector is the normal of second)?
		public static bool _IsPerpendicular(Vector2 v1, Vector2 v2) {
			return _MathUtil._IsZero(Vector2.Dot(v1, v2));
		}

		//Are the two directions perpendicular(first vector is the normal of second)?
		public static bool _IsPerpendicular(Vector3 v1, Vector3 v2) {
			return _MathUtil._IsZero(Vector3.Dot(v1, v2));
		}

		//Get point on direction at a distance
		public static Vector3 _GetPoint(Vector3 origin, Vector3 direction, float distance) {
			Ray ray = new Ray(origin, direction);
			return ray.GetPoint(distance);
		}
	}

}