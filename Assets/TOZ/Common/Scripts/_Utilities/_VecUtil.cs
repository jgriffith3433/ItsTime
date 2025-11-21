using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TOZ {

	public static class _VecUtil {
		//Set (X)
		public static Vector3 _SetX(Vector3 v, float x) { return new Vector3(x, v.y, v.z); }

		//Set (Y)
		public static Vector3 _SetY(Vector3 v, float y) { return new Vector3(v.x, y, v.z); }

		//Set (Z)
		public static Vector3 _SetZ(Vector3 v, float z) { return new Vector3(v.x, v.y, z); }

		//returns xy components of vec2 as zy and set (X)
		public static Vector3 _SetZY(Vector2 v, float x) { return new Vector3(x, v.y, v.x); }

		//returns xy components of vec2 as yz and set (X)
		public static Vector3 _SetYZ(Vector2 v, float x) { return new Vector3(x, v.x, v.y); }

		//returns xy components of vec2 as xz and set (Y)
		public static Vector3 _SetXZ(Vector2 v, float y) { return new Vector3(v.x, y, v.y); }

		//returns xy components of vec2 as zx and set (Y)
		public static Vector3 _SetZX(Vector2 v, float y) { return new Vector3(v.y, y, v.x); }

		//returns xy components of vec2 as xy and set (Z)
		public static Vector3 _SetXY(Vector2 v, float z) { return new Vector3(v.x, v.y, z); }

		//returns xy components of vec2 as yx and set (Z)
		public static Vector3 _SetYX(Vector2 v, float z) { return new Vector3(v.y, v.x, z); }

		//returns zy components as vec2
		public static Vector2 _GetZY(Vector3 v) { return new Vector2(v.z, v.y); }

		//returns yz components as vec2
		public static Vector2 _GetYZ(Vector3 v) { return new Vector2(v.y, v.z); }

		//returns xz components as vec2
		public static Vector2 _GetXZ(Vector3 v) { return new Vector2(v.x, v.z); }

		//returns zx components as vec2
		public static Vector2 _GetZX(Vector3 v) { return new Vector2(v.z, v.x); }

		//returns xy components as vec2
		public static Vector2 _GetXY(Vector3 v) { return new Vector2(v.x, v.y); }

		//returns yx components as vec2
		public static Vector2 _GetYX(Vector3 v) { return new Vector2(v.y, v.x); }

		//Reverse the direction of the 2D vector
		public static Vector2 _Negate(Vector2 v) { return new Vector2(-v.x, -v.y); }

		//Reverse the direction of the 3D vector
		public static Vector3 _Negate(Vector3 v) { return new Vector3(-v.x, -v.y, -v.z); }

		//Round the 3d vector elements to nearest int
		public static Vector2 _Round(Vector2 v) { return new Vector2(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y)); }

		//Round the 3d vector elements to nearest int
		public static Vector3 _Round(Vector3 v) { return new Vector3(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y), Mathf.RoundToInt(v.z)); }

		//Clamp vector values between upper and lower bounds
		public static Vector2 _Clamp(Vector2 v, float min, float max) {
			float x = (v.x > min) ? ((v.x < max) ? v.x : max) : min;
			float y = (v.y > min) ? ((v.y < max) ? v.y : max) : min;
			return new Vector2(x, y);
		}

		//Clamp vector values between upper and lower bounds
		public static Vector2 _Clamp(Vector2 v, float xMin, float xMax, float yMin, float yMax) {
			float x = (v.x > xMin) ? ((v.x < xMax) ? v.x : xMax) : xMin;
			float y = (v.y > yMin) ? ((v.y < yMax) ? v.y : yMax) : yMin;
			return new Vector2(x, y);
		}

		//Clamp vector values between upper and lower bounds
		public static Vector3 _Clamp(Vector3 v, float min, float max) {
			float x = (v.x > min) ? ((v.x < max) ? v.x : max) : min;
			float y = (v.y > min) ? ((v.y < max) ? v.y : max) : min;
			float z = (v.z > min) ? ((v.z < max) ? v.z : max) : min;
			return new Vector3(x, y, z);
		}

		//Clamp vector values between upper and lower bounds
		public static Vector3 _Clamp(Vector3 v, float xMin, float xMax, float yMin, float yMax, float zMin, float zMax) {
			float x = (v.x > xMin) ? ((v.x < xMax) ? v.x : xMax) : xMin;
			float y = (v.y > yMin) ? ((v.y < yMax) ? v.y : yMax) : yMin;
			float z = (v.z > zMin) ? ((v.z < zMax) ? v.z : zMax) : zMin;
			return new Vector3(x, y, z);
		}

		//Clamp vector values between upper and lower bounds
		public static Vector3 _ClampToScreen(Vector3 v) {
			Vector2 center = new Vector2(Screen.width / 2.0f, Screen.height / 2.0f);
			float x = v.x;
			float y = v.y;
			float z = v.z;
			
			if(z >= 0f) {
				x = ((x > 0) ? ((x < Screen.width) ? x : Screen.width) : 0);
				y = ((y > 0) ? ((y < Screen.height) ? y : Screen.height) : 0);
			}
			else {
				x = (x >= center.x) ? 0 : Screen.width;
				y = (y >= center.y) ? 0 : Screen.height;
			}
			return new Vector3(x, y, z);
		}

		//Rotate Vector 2D with given angle
		public static Vector2 _Rotate(Vector2 v, float a) {
			float rad = a * Mathf.Deg2Rad;
			float sin = Mathf.Sin(rad);
			float cos = Mathf.Cos(rad);
			float x = (v.x * cos) - (v.y * sin);
			float y = (v.x * sin) + (v.y * cos);
			return new Vector2(x, y);
		}

		//Rotate Vector 3D on X axis (Pitch) (Gimbal Lock)
		public static Vector3 _RotateX(Vector3 v, float a) {
			float rad = a * Mathf.Deg2Rad;
			float sin = Mathf.Sin(rad);
			float cos = Mathf.Cos(rad);
			float x = v.x;
			float y = (v.y * cos) - (v.z * sin);
			float z = (v.z * cos) + (v.y * sin);
			return new Vector3(x, y, z);
		}

		//Rotate Vector 3D on Y axis (Yaw) (Gimbal Lock)
		public static Vector3 _RotateY(Vector3 v, float a) {
			float rad = a * Mathf.Deg2Rad;
			float sin = Mathf.Sin(rad);
			float cos = Mathf.Cos(rad);
			float x = (v.x * cos) + (v.z * sin);
			float y = v.y;
			float z = (v.z * cos) - (v.x * sin);
			return new Vector3(x, y, z);
		}

		//Rotate Vector 3D on Z axis (Roll) (Gimbal Lock)
		public static Vector3 _RotateZ(Vector3 v, float a) {
			float rad = a * Mathf.Deg2Rad;
			float sin = Mathf.Sin(rad);
			float cos = Mathf.Cos(rad);
			float x = (v.x * cos) - (v.y * sin);
			float y = (v.y * cos) + (v.x * sin);
			float z = v.z;
			return new Vector3(x, y, z);
		}

		//Rotate Vector with given quaternion (No Gimbal Lock)
		public static Vector3 _Rotate(Vector3 v, Quaternion q) {
			return q * v;
		}

		//Rotate Vector with given angle on given axis (No Gimbal Lock)
		public static Vector3 _Rotate(Vector3 v, float angle, Vector3 axis) {
			return Quaternion.AngleAxis(angle, axis) * v;
		}

		//returns perp product of the given 2 vec2s
		public static float _Perp(Vector2 v1, Vector2 v2) {
			return (v1.x * v2.y) - (v2.x * v1.y);
		}

		//returns mixed product of the given 3 vec3s
		public static float _Mixed(Vector3 v1, Vector3 v2, Vector3 v3) {
			return Vector3.Dot(Vector3.Cross(v1, v2), v3);
		}

		//Get the magnitude on given axis
		public static float _GetMagnitudeOnAxis(Vector3 v, Vector3 axis) {
			float length = v.magnitude;
			if(length <= 0f)
				return 0f;

			float dot = Vector3.Dot(axis, v / length);
			return dot * length;
		}

		//returns multiplication of 2 vectors
		public static Vector2 _Mul(Vector2 v1, Vector2 v2) {
			return new Vector2(v1.x * v2.x, v1.y * v2.y);
		}

		//returns multiplication of 2 vectors
		public static Vector3 _Mul(Vector3 v1, Vector3 v2) {
			return new Vector3(v1.x * v2.x, v1.y * v2.y, v1.z * v2.z);
		}

		//returns division of 2 vectors
		public static Vector2 _Divide(Vector2 v1, Vector2 v2) {
			return new Vector2(v1.x / v2.x, v1.y / v2.y);
		}

		//returns division of 2 vectors
		public static Vector3 _Divide(Vector3 v1, Vector3 v2) {
			return new Vector3(v1.x / v2.x, v1.y / v2.y, v1.z / v2.z);
		}

		//returns Reciprocal
		public static Vector3 _Reciprocal(Vector3 v) {
			return new Vector3(1.0f / v.x, 1.0f / v.y, 1.0f / v.z);
		}

		//Get max value
		public static float _Max(Vector2 v) {
			return Mathf.Max(v.x, v.y);
		}

		public static float _Max(Vector3 v) {
			return Mathf.Max(v.x, Mathf.Max(v.y, v.z));
		}

		//Get min value
		public static float _Min(Vector2 v) {
			return Mathf.Min(v.x, v.y);
		}

		public static float _Min(Vector3 v) {
			return Mathf.Min(v.x, Mathf.Min(v.y, v.z));
		}
	}

}