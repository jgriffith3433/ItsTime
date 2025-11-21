using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TOZ {

	public static class _RandUtil {
		//Private variables
		private static string[] _firstNames = {
			"Sergio", "Daniel", "Carolina", "David", "Reina", "Saul",
			"Bernard", "Danny", "Dimas", "Yuri", "Ivan", "Laura"
		};

		private static string[] _lastNames = {
			"Tapia", "Gutierrez", "Rueda", "Galviz", "Yuli", "Rivera",
			"Mamami", "Saucedo", "Dominguez", "Escobar", "Martin", "Crespo"
		};

		public static void _SetSeed(int i) {
			UnityEngine.Random.InitState(i);
		}

		//Get a random float (0f to 1f)
		public static float _Get() {
			return UnityEngine.Random.value;
		}

		//Get a random signed float (-1 to +1)
		public static float _GetSigned() {
			return _Get() * 2.0f - 1.0f;
		}

		//Get a random signed int (-1 or 0 or +1)
		public static int _GetSignedInt() {
			return Mathf.RoundToInt(_GetSigned());
		}

		//Get a random signed int (-1 or +1)
		public static int _Get1Or1() {
			return _Get() < 0.5f ? -1 : 1;
		}

		//Get a random int between min and max
		public static int _GetRange(int min, int max) {
			return UnityEngine.Random.Range(min, max);
		}

		//Get a random float between min and max
		public static float _GetRange(float min, float max) {
			return UnityEngine.Random.Range(min, max);
		}

		//Random name from above strings
		public static string _GetName() {
			int a = _GetRange(0, _firstNames.Length);
			int b = _GetRange(0, _lastNames.Length);
			return "{0} {1}".FormatWith(_firstNames[a], _lastNames[b]);
		}

		//A Random Vector3 inside a triangle
		public static Vector3 _GetInsideTriangle(Vector3 p1, Vector3 p2, Vector3 p3) {
			float a = _Get();
			float b = _Get();
			if ((a + b) > 1f) {
				a = 1f - a;
				b = a - b;
			}
			float c = a - a - b;
			return a * p1 + b * p2 + c * p3;
		}

		public static Vector2 _GetInsideRectangle(float x1, float y1, float x2, float y2) {
			float x = _GetRange(x1, x2);
			float y = _GetRange(y1, y2);
			return new Vector2(x, y);
		}

		public static Vector3 _GetInsideBox(float x1, float y1, float z1, float x2, float y2, float z2) {
			float x = _GetRange(x1, x2);
			float y = _GetRange(y1, y2);
			float z = _GetRange(z1, z2);
			return new Vector3(x, y, z);
		}

		public static Vector2 _GetInsideUnitCircle() {
			return UnityEngine.Random.insideUnitCircle;
		}

		public static Vector2 _GetOnUnitCircle() {
			return UnityEngine.Random.insideUnitCircle.normalized;
		}

		public static Vector3 _GetInsideUnitSphere() {
			return UnityEngine.Random.insideUnitSphere;
		}

		public static Vector3 _GetOnUnitSphere() {
			return UnityEngine.Random.onUnitSphere;
		}

		public static Vector3 _GetOnUnitHemiSphere() {
			Vector3 v = _GetOnUnitSphere();
			v.y = Mathf.Abs(v.y);
			return v;
		}

		public static Color _GetColor() {
			return new Color(_Get(), _Get(), _Get(), 1.0f);
		}

		//returns random point around the position
		public static Vector2 _GetPosition(Vector2 v, float scale) {
			return v + (_GetOnUnitCircle() * scale);
		}

		//returns random point around the position
		public static Vector3 _GetPosition(Vector3 v, float scale) {
			return v + (_GetOnUnitSphere() * scale);
		}

		//Returns a texture with random vectors
		public static Texture2D _GetTexture(int width, int height, int skipWidth = 1, int skipHeight = 1) {
			Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
			tex.filterMode = FilterMode.Point;
			tex.wrapMode = TextureWrapMode.Clamp;

			Color[] cols = new Color[tex.width * tex.height];
			for(int y = 0; y < tex.height; y++) {
				for(int x = 0; x < tex.width; x++) {
					int i = y * tex.width + x;
					if((x % skipWidth == 0) && (y % skipHeight == 0)) {
						Vector3 v = _GetOnUnitSphere();
						v.x = v.x * 0.5f + 0.5f;
						v.y = v.y * 0.5f + 0.5f;
						v.z = v.z * 0.5f + 0.5f;
						cols[i] = new Color(v.x, v.y, v.z, 1.0f);
					}
					else {
						cols[i] = new Color(0.5f, 0.5f, 0.5f, 1.0f);
					}
				}
			}
			tex.SetPixels(cols);
			tex.Apply();
			return tex;
		}
	}

}