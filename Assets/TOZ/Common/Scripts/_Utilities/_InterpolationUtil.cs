using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TOZ {

	public static class _InterpolationUtil {

		//Hermite
		public static double _Hermite(double t1, double v1, double v2, double t2, double s) {
			double s2 = s * s;
			double s3 = s2 * s;
			double p1 = ((2.0 * s3) - (3.0 * s2)) + 1.0;
			double p2 = (-2.0 * s3) + (3.0 * s2);
			double p3 = (s3 - (2.0 * s2)) + s;
			double p4 = s3 - s2;
			double res = (v1 * p1) + (v2 * p2) + (t1 * p3) + (t2 * p4);
			return res;
		}

		public static float _Hermite(float t1, float v1, float v2, float t2, float s) {
			float s2 = s * s;
			float s3 = s2 * s;
			float p1 = ((2f * s3) - (3f * s2)) + 1f;
			float p2 = (-2f * s3) + (3f * s2);
			float p3 = (s3 - (2f * s2)) + s;
			float p4 = s3 - s2;
			float res = (v1 * p1) + (v2 * p2) + (t1 * p3) + (t2 * p4);
			return res;
		}

		public static Vector2 _Hermite(Vector2 t1, Vector2 v1, Vector2 v2, Vector2 t2, float s) {
			float s2 = s * s;
			float s3 = s2 * s;
			float p1 = ((2f * s3) - (3f * s2)) + 1f;
			float p2 = (-2f * s3) + (3f * s2);
			float p3 = (s3 - (2f * s2)) + s;
			float p4 = s3 - s2;
			Vector2 res = (v1 * p1) + (v2 * p2) + (t1 * p3) + (t2 * p4);
			return res;
		}

		public static Vector3 _Hermite(Vector3 t1, Vector3 v1, Vector3 v2, Vector3 t2, float s) {
			float s2 = s * s;
			float s3 = s2 * s;
			float p1 = ((2f * s3) - (3f * s2)) + 1f;
			float p2 = (-2f * s3) + (3f * s2);
			float p3 = (s3 - (2f * s2)) + s;
			float p4 = s3 - s2;
			Vector3 res = (v1 * p1) + (v2 * p2) + (t1 * p3) + (t2 * p4);
			return res;
		}

		//Cardinal
		public static double _Cardinal(double v1, double v2, double v3, double v4, double s, double t) {
			double t1 = t * (v3 - v1);
			double t2 = t * (v4 - v2);
			return _Hermite(t1, v2, v3, t2, s);
		}

		public static float _Cardinal(float v1, float v2, float v3, float v4, float s, float t) {
			float t1 = t * (v3 - v1);
			float t2 = t * (v4 - v2);
			return _Hermite(t1, v2, v3, t2, s);
		}

		public static Vector2 _Cardinal(Vector2 v1, Vector2 v2, Vector2 v3, Vector2 v4, float s, float t) {
			Vector2 t1 = t * (v3 - v1);
			Vector2 t2 = t * (v4 - v2);
			return _Hermite(t1, v2, v3, t2, s);
		}

		public static Vector3 _Cardinal(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, float s, float t) {
			Vector3 t1 = t * (v3 - v1);
			Vector3 t2 = t * (v4 - v2);
			return _Hermite(t1, v2, v3, t2, s);
		}

		//CatmullRom
		public static double _CatmullRom(double v1, double v2, double v3, double v4, double s) {
			return _Cardinal(v1, v2, v3, v4, s, 0.5);
		}

		public static float _CatmullRom(float v1, float v2, float v3, float v4, float s) {
			return _Cardinal(v1, v2, v3, v4, s, 0.5f);
		}

		public static Vector2 _CatmullRom(Vector2 v1, Vector2 v2, Vector2 v3, Vector2 v4, float s) {
			return _Cardinal(v1, v2, v3, v4, s, 0.5f);
		}

		public static Vector3 _CatmullRom(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, float s) {
			return _Cardinal(v1, v2, v3, v4, s, 0.5f);
		}

		//Linear
		public static double _Linear(double v1, double v2, double s) {
			return (v1 * (1.0 - s) + (v2 * s));
		}

		public static float _Linear(float v1, float v2, float s) {
			return (v1 * (1.0f - s) + (v2 * s));
		}

		public static Vector2 _Linear(Vector2 v1, Vector2 v2, float s) {
			return (v1 * (1.0f - s) + (v2 * s));
		}

		public static Vector3 _Linear(Vector3 v1, Vector3 v2, float s) {
			return (v1 * (1.0f - s) + (v2 * s));
		}

		//Cosine
		public static double _Cosine(double v1, double v2, double s) {
			double s2 = (1.0 - System.Math.Cos(s * _MathUtil._PI)) / 2.0;
			return (v1 * (1.0 - s2) + v2 * s2);
		}

		public static float _Cosine(float v1, float v2, float s) {
			float s2 = (1.0f - Mathf.Cos(s * Mathf.PI)) / 2.0f;
			return (v1 * (1.0f - s2) + v2 * s2);
		}

		public static Vector2 _Cosine(Vector2 v1, Vector2 v2, float s) {
			float s2 = (1.0f - Mathf.Cos(s * Mathf.PI)) / 2.0f;
			return (v1 * (1.0f - s2) + v2 * s2);
		}

		public static Vector3 _Cosine(Vector3 v1, Vector3 v2, float s) {
			float s2 = (1.0f - Mathf.Cos(s * Mathf.PI)) / 2.0f;
			return (v1 * (1.0f - s2) + v2 * s2);
		}

		//Cubic
		public static double _Cubic(double v1, double v2, double v3, double v4, double s) {
			double s2 = s * s;
			double s3 = s2 * s;
			double p1 = v4 - v3 - v1 - v2;
			double p2 = v1 - v2 - p1;
			double p3 = v3 - v1;
			double p4 = v2;
			return p1 * s3 + p2 * s2 + p3 * s + p4;
		}

		public static float _Cubic(float v1, float v2, float v3, float v4, float s) {
			float s2 = s * s;
			float s3 = s2 * s;
			float p1 = v4 - v3 - v1 - v2;
			float p2 = v1 - v2 - p1;
			float p3 = v3 - v1;
			float p4 = v2;
			return p1 * s3 + p2 * s2 + p3 * s + p4;
		}

		public static Vector2 _Cubic(Vector2 v1, Vector2 v2, Vector2 v3, Vector2 v4, float s) {
			float s2 = s * s;
			float s3 = s2 * s;
			Vector2 p1 = v4 - v3 - v1 - v2;
			Vector2 p2 = v1 - v2 - p1;
			Vector2 p3 = v3 - v1;
			Vector2 p4 = v2;
			return p1 * s3 + p2 * s2 + p3 * s + p4;
		}

		public static Vector3 _Cubic(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, float s) {
			float s2 = s * s;
			float s3 = s2 * s;
			Vector3 p1 = v4 - v3 - v1 - v2;
			Vector3 p2 = v1 - v2 - p1;
			Vector3 p3 = v3 - v1;
			Vector3 p4 = v2;
			return p1 * s3 + p2 * s2 + p3 * s + p4;
		}

		//Smoothstep value
		public static double _Smoothstep(double a, double b, double t) {
			double v = _MathUtil._Saturate((t - a) / (b - a));
			return _MathUtil._SCurve3(v);
		}

		public static float _Smoothstep(float a, float b, float t) {
			float v = _MathUtil._Saturate((t - a) / (b - a));
			return _MathUtil._SCurve3(v);
		}

		//Grade value
		public static double _Grade(double a, double b, double min, double max, double t) {
			return _Linear(a, b, _Smoothstep(min, max, t));
		}

		public static float _Grade(float a, float b, float min, float max, float t) {
			return Mathf.Lerp(a, b, _Smoothstep(min, max, t));
		}

		public static Vector2 _Grade(Vector2 a, Vector2 b, float min, float max, float t) {
			return Vector2.Lerp(a, b, _Smoothstep(min, max, t));
		}

		public static Vector3 _Grade(Vector3 a, Vector3 b, float min, float max, float t) {
			return Vector3.Lerp(a, b, _Smoothstep(min, max, t));
		}

		public static Color _Grade(Color a, Color b, float min, float max, float t) {
			return Color.Lerp(a, b, _Smoothstep(min, max, t));
		}

		//Array Lerp
		public static double _Lerp(double[] array, float t) {
			t = Mathf.Clamp(t, 0f, 1f);
			if(t == 0f)
				return array[0];
			else if(t == 1f)
				return array[array.Length-1];
			else {
				float scaledTime = t * (float)(array.Length-1);
				double a = array[(int)scaledTime];
				double b = array[(int)(scaledTime + 1f)];
				float newT = scaledTime - Mathf.Floor(scaledTime);
				return _Linear(a, b, newT);
			}
		}

		public static float _Lerp(float[] array, float t) {
			t = Mathf.Clamp(t, 0f, 1f);
			if(t == 0f)
				return array[0];
			else if(t == 1f)
				return array[array.Length-1];
			else {
				float scaledTime = t * (float)(array.Length-1);
				float a = array[(int)scaledTime];
				float b = array[(int)(scaledTime + 1f)];
				float newT = scaledTime - Mathf.Floor(scaledTime);
				return _Linear(a, b, newT);
			}
		}

		public static Vector2 _Lerp(Vector2[] array, float t) {
			t = Mathf.Clamp(t, 0f, 1f);
			if(t == 0f)
				return array[0];
			else if(t == 1f)
				return array[array.Length-1];
			else {
				float scaledTime = t * (float)(array.Length-1);
				Vector2 a = array[(int)scaledTime];
				Vector2 b = array[(int)(scaledTime + 1f)];
				float newT = scaledTime - Mathf.Floor(scaledTime);
				return Vector2.Lerp(a, b, newT);
			}
		}

		public static Vector3 _Lerp(Vector3[] array, float t) {
			t = Mathf.Clamp(t, 0f, 1f);
			if(t == 0f)
				return array[0];
			else if(t == 1f)
				return array[array.Length-1];
			else {
				float scaledTime = t * (float)(array.Length-1);
				Vector3 a = array[(int)scaledTime];
				Vector3 b = array[(int)(scaledTime + 1f)];
				float newT = scaledTime - Mathf.Floor(scaledTime);
				return Vector3.Lerp(a, b, newT);
			}
		}

		public static Color _Lerp(Color[] array, float t) {
			if(t == 0f)
				return array[0];
			else if(t == 1f)
				return array[array.Length-1];
			else {
				float scaledTime = t * (float)(array.Length-1);
				Color a = array[(int)scaledTime];
				Color b = array[(int)(scaledTime + 1f)];
				float newT = scaledTime - Mathf.Floor(scaledTime);
				return Color.Lerp(a, b, newT);
			}
		}
	}

}