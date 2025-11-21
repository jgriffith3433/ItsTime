 /**********************************NOTES********************************************
USAGE:
latitude = AngleUtil.Clamp(latitude, -90f, 90f);
longitude = AngleUtil.Normalize(longitude);
transform.position = MathUtil.PolarToCartesian(latitude, longitude) * 5f;
Debug.Log(MathUtil.CartesianToPolar(transform.position));
**************************************************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TOZ {

	public static class _AngleUtil {
		public const float _Deg = 0.002777777777777778f;
		public const float _Min = 0.000046296296296296f;
		public const float _Sec = 0.000000771604938272f;
		public const float _Rad = 0.159154943091895336f;
		public const float _Grad = 0.0025f;

		public const float _Rad2Grad = 200.0f / Mathf.PI;
		public const float _Grad2Rad = Mathf.PI / 200.0f;

/*
		public static float Delta(float a1, float a2) {
			Mathf.DeltaAngle(a1, a2);
		}
*/
		//Angle wrapped to be between 0 and 360 degrees
		public static float _Wrap(float v) {
			return (((v % 360f) + 360f) % 360f);
		}

		//Angle normalized to be between -180 and +180 degrees
		public static float _Normalize(float v) {
			return _Wrap(v + 180f) - 180f;
		}

		//Clamp an angle between min and max (angle normalized -180 to 180)
		public static float _Clamp(float v, float min, float max) {
			return _MathUtil._Clamp(_Normalize(v), min, max);
		}

		//Lerp an angle between a and b (shortest distance using deltaAngle)
		public static float _Lerp(float a, float b, float t) {
			return _Wrap(Mathf.LerpAngle(a, b, t));
		}

		//returns dot as angle in degrees
		public static float _DotToAngle(Vector3 v1, Vector3 v2) {
			return Mathf.Acos(Vector3.Dot(v1, v2)) * Mathf.Rad2Deg;
		}

		//is a right angle (i.e. 90° or pi/2)
		public static bool _IsRight(float v) {
			return _MathUtil._AreEqual(v, 90.0f);
		}

		//is an acute angle (i.e. less than 90° but greater than 0°)
		public static bool _IsAcute(float v) {
			return (v > 0.0f && v < 90.0f);
		}

		//is an obtuse angle (i.e. greater than 90° but less than 180°)
		public static bool _IsObtuse(float v) {
			return (v > 90.0f && v < 180.0f);
		}

		//is a reflex angle (i.e. greater than 180° but less than 360°)
		public static bool _IsReflex(float v) {
			return (v > 180.0f && v < 360.0f);
		}
	}

}