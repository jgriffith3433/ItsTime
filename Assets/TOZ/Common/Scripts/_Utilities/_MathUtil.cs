using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TOZ {

	public static class _MathUtil {

		public const double _PI = 3.1415926535897932385; // 180 degrees
		public const double _PI2 = _PI * 2.0; // 2pi which is 360 degrees.
		public const double _PIOver2 = _PI / 2.0;  // pi/2 which is 90 degrees.
		public const double _PIOver4 = _PI / 4.0;  // pi/4 which is 45 degrees. 
		public const double _DEG2RAD = _PI / 180.0;
		public const double _RAD2DEG = 1.0 / _DEG2RAD;
		public const double _EPSILON = double.Epsilon;
		public const double _SQRT2 = 1.4142135623730950488;
		public const double _SQRT3 = 1.7320508075688772935;
		public const double _E = 2.718281828459045;

		//Swap two values
		public static void _Swap<T>(ref T a, ref T b) { T t = a; a = b; b = t; }

		//Is value equal to 0
		public static bool _IsZero(float v) { return Mathf.Abs(v) <= Mathf.Epsilon; }
		public static bool _IsZero(double v) { return Math.Abs(v) <= _EPSILON; }

		//Is value equal to 1
		public static bool _IsOne(float v) { return _IsZero(v - 1.0f); }
		public static bool _IsOne(double v) { return _IsZero(v - 1.0); }

		//Is value equal to other value
		public static bool _AreEqual(float v1, float v2) {
			return (v1 >= v2 - Mathf.Epsilon && v1 <= v2 + Mathf.Epsilon) ? true : false;
		}
		public static bool _AreEqual(double v1, double v2) {
			return (v1 >= v2 - _EPSILON && v1 <= v2 + _EPSILON) ? true : false;
		}

		//Convert double to integer32 range
		public static double _DoubleToInt32Range(double v) {
			if(v >= 1073741824.0) {
				return (2.0 * System.Math.IEEERemainder(v, 1073741824.0)) - 1073741824.0;
			}
			else if(v <= -1073741824.0) {
				return (2.0 * System.Math.IEEERemainder(v, 1073741824.0)) + 1073741824.0;
			}
			else {
				return v;
			}
		}

		//Is value power of 2
		public static bool _IsPow2(int v) {
			return ((v != 0) && (v & (v - 1)) == 0);
		}

		//Wrap a value between min and max
		public static int _Wrap(int v, int min, int max) {
			int range = max - min + 1;
			if(v < min)
				v += range * ((min - v) / range + 1);
			return min + (v - min) % range;
		}

		public static int _Wrap(int v, int max) {
			return (((v % max) + max) % max);
		}

		//Is a value in range?
		public static bool _IsInRange(int v, int min, int max) {
			return min <= v && v <= max;
		}

		public static bool _IsInRange(float v, float min, float max) {
			return min <= v && v <= max;
		}

		public static bool _IsInRange(double v, float min, double max) {
			return min <= v && v <= max;
		}

		//Normalize a value to 0 +1 range
		public static float _NormalizeUnsigned(float v, float min, float max) {
			return (v - min) / (max - min);
		}

		public static double _NormalizeUnsigned(double v, double min, double max) {
			return (v - min) / (max - min);
		}

		//Normalize a value to -1 +1 range
		public static float _NormalizeSigned(float v, float min, float max) {
			float average = (min + max) / 2.0f;
			float range = (max - min) / 2.0f;
			return (v - average) / range;
		}

		public static double _NormalizeSigned(double v, double min, double max) {
			double average = (min + max) / 2.0;
			double range = (max - min) / 2.0;
			return (v - average) / range;
		}

		//Clamp a value on upper bound
		public static int _ClampUp(int v, int max) {
			return (v < max) ? v : max;
		}

		public static float _ClampUp(float v, float max) {
			return (v < max) ? v : max;
		}

		public static double _ClampUp(double v, double max) {
			return (v < max) ? v : max;
		}

		//Clamp a value on lower bound
		public static int _ClampDown(int v, int min) {
			return (v > min) ? v : min;
		}

		public static float _ClampDown(float v, float min) {
			return (v > min) ? v : min;
		}

		public static double _ClampDown(double v, double min) {
			return (v > min) ? v : min;
		}

		//Clamp a value on bounds
		public static int _Clamp(int v, int min, int max) {
			return (v > min) ? ((v < max) ? v : max) : min;
		}

		public static float _Clamp(float v, float min, float max) {
			return (v > min) ? ((v < max) ? v : max) : min;
		}

		public static double _Clamp(double v, double min, double max) {
			return (v > min) ? ((v < max) ? v : max) : min;
		}

		//Cubic S-Curve value
		public static float _SCurve3(float v) {
			return (v * v * (3.0f - 2.0f * v));
		}

		public static double _SCurve3(double v) {
			return (v * v * (3.0 - 2.0 * v));
		}

		//Quintic S-Curve value
		public static float _SCurve5(float v) {
			float s3 = v * v * v;
			float s4 = s3 * v;
			float s5 = s4 * v;
			return (6.0f * s5) - (15.0f * s4) + (10.0f * s3);
		}

		public static double _SCurve5(double v) {
			double s3 = v * v * v;
			double s4 = s3 * v;
			double s5 = s4 * v;
			return (6.0 * s5) - (15.0 * s4) + (10.0 * s3);
		}

		//Gauss function
		public static float _Gauss(float amp, float x, float y, float cx, float cy, float sx, float sy) {
			float dx = x - cx;
			float dy = y - cy;
			float xx = (dx * dx) / (2.0f * sx * sx);
			float yy = (dy * dy) / (2.0f * sy * sy);
			return amp * Mathf.Exp(-(xx + yy));
		}

		public static double _Gauss(double amp, double x, double y, double cx, double cy, double sx, double sy) {
			double dx = x - cx;
			double dy = y - cy;
			double xx = (dx * dx) / (2.0 * sx * sx);
			double yy = (dy * dy) / (2.0 * sy * sy);
			return amp * System.Math.Exp(-(xx + yy));
		}

		//Saturate value
		public static float _Saturate(float v) {
			return Mathf.Max(0.0f, Mathf.Min(1.0f, v));
		}

		public static double _Saturate(double v) {
			return System.Math.Max(0.0, System.Math.Min(1.0, v));
		}

		//Sign of value, different from Mathf as 0 returns 0
		public static int _Sign(float v) {
			return (v < 0.0f) ? -1 : (v > 0.0f) ? 1 : 0;
		}

		public static int _Sign(double v) {
			return (v < 0.0) ? -1 : (v > 0.0) ? 1 : 0;
		}

		//Fast floor
		public static int _FastFloor(float v) {
			return (v > 0.0f) ? ((int)v) : (((int)v) - 1);
		}

		public static int _FastFloor(double v) {
			return (v > 0) ? ((int)v) : (((int)v) - 1);
		}

		//Polar to cartesian
		//longitude = xAxis, latitude = yAxis
		public static Vector3 _PolarToCartesian(float lat, float lon) {
			Vector3 direction = Vector3.forward;
			Quaternion rotation = Quaternion.Euler(-lon, -lat, 0.0f);
			return rotation * direction;
		}

		//Cartesian to polar
		//x = longitude, y = latitude
		public static Vector2 _CartesianToPolar(Vector3 pos) {
			float length = new Vector2(pos.x, pos.z).magnitude;
			Vector2 result = Vector3.zero;
			result.y = Mathf.Atan2(-pos.x, pos.z);
			result.x = Mathf.Atan2(pos.y, length);
			result *= Mathf.Rad2Deg;
			return result;
		}

		//Latitude and Longitude to XYZ on a unit sphere
		//longitude = xAxis, latitude = yAxis
		public static void _LatLonToXYZ(float lat, float lon, ref float x, ref float y, ref float z) {
			float r = Mathf.Cos(Mathf.Deg2Rad * lon);
			x = r * -Mathf.Sin(Mathf.Deg2Rad * lat);
			y = Mathf.Sin(Mathf.Deg2Rad * lon);
			z = r * Mathf.Cos(Mathf.Deg2Rad * lat);
		}

		// (For TOZ Noise, forward is negative)
		//longitude = xAxis, latitude = yAxis
		public static void _LatLonToXYZ(double lat, double lon, ref double x, ref double y, ref double z) {
			double r = Math.Cos(_DEG2RAD * lon);
			x = r * Math.Sin(_DEG2RAD * lat);
			y = Math.Sin(_DEG2RAD * lon);
			z = r * -Math.Cos(_DEG2RAD * lat);
		}
	}

}