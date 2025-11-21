using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TOZ;

namespace TOZ.Celestial {

	public static class PlanetUtil {

		public const double _LY = 9460730472580800; //in meters (950000000)
		public const double _AU = 149597871; //in meters (150000)

		//Get polar coordinates of arbitrary pos
		//x = longitude, y = latitude
		public static Vector2 GetPolarCoords(Planet planet, Vector3 pos) {
			Vector3 dir = _DirUtil._Get3DNormalized(planet.PosW, pos);
			return _MathUtil._CartesianToPolar(dir);
		}

		//coord length is equal to peak radius
		//longitude = xAxis, latitude = yAxis
		public static Vector3 GetCartesianCoords(Planet planet, float lat, float lon) {
			return planet.PosW + _MathUtil._PolarToCartesian(lat, lon) * planet.PeakRadius;
		}

		public static Vector2 DirectionToSphericalUV(Vector3 dir) {
			Vector2 uv = Vector2.zero;
			uv.x = -Mathf.Atan2(dir.x, dir.z) / (2f * Mathf.PI) + 0.5f;
			uv.y = 1.0f + (Mathf.Asin(dir.y) / Mathf.PI - 0.5f);
			return uv;
		}

		public static Vector3 SphericalUVToDirection(Vector2 uv) {
			Vector2 latlon = SphericalUVToLatLon(uv);
			return _MathUtil._PolarToCartesian(latlon.x, latlon.y);
		}

		public static Vector3 SphericalUVToDirection(float u, float v) {
			Vector2 latlon = SphericalUVToLatLon(new Vector2(u, v));
			return _MathUtil._PolarToCartesian(latlon.y, latlon.x);
		}

		//x = longitude, y = latitude
		public static Vector2 SphericalUVToLatLon(Vector2 v) {
			Vector2 ll = Vector2.zero;
			ll.y = _AngleUtil._Normalize((v.x * 2.0f - 1.0f) * 180f);
			ll.x = _AngleUtil._Normalize((v.y * 2.0f - 1.0f) * 90f);
			return ll;
		}

		public static Vector2 LatLonToSphericalUV(float lat, float lon) {
			Vector3 dir = _MathUtil._PolarToCartesian(lat, lon);
			return DirectionToSphericalUV(dir);
		}

		public static Vector3 CubeToSphere1(Vector3 v) {
			float x2 = v.x * v.x;
			float y2 = v.y * v.y;
			float z2 = v.z * v.z;
			float x2Half = x2 * 0.5f;
			float y2Half = y2 * 0.5f;
			float z2Half = z2 * 0.5f;
			v.x = v.x * Mathf.Sqrt(1f - y2Half - z2Half + (y2 * z2) * (1f / 3f));
			v.y = v.y * Mathf.Sqrt(1f - z2Half - x2Half + (z2 * x2) * (1f / 3f));
			v.z = v.z * Mathf.Sqrt(1f - x2Half - y2Half + (x2 * y2) * (1f / 3f));
			return v;
		}

		public static Vector3 CubeToSphere0(Vector3 v) {
			float x2 = v.x * v.x;
			float y2 = v.y * v.y;
			float z2 = v.z * v.z;
			float x2Half = x2 * 0.5f;
			float y2Half = y2 * 0.5f;
			float z2Half = z2 * 0.5f;
			v.x = v.x * Mathf.Sqrt(1f - y2Half - z2Half + (y2 * z2) / 3f);
			v.y = v.y * Mathf.Sqrt(1f - z2Half - x2Half + (z2 * x2) / 3f);
			v.z = v.z * Mathf.Sqrt(1f - x2Half - y2Half + (x2 * y2) / 3f);
			return v;
		}

		public static Vector3 CubeToSphere2(Vector3 v) {
			return v.normalized;
		}

		//Height is eye distance to planet core
		public static float CalculateDistanceToHorizon(float height, float radius) {
			float ce2 = height * height;
			float cr2 = radius * radius;
			return Mathf.Sqrt(ce2 - cr2);
		}

		//Height is eye distance to planet core
		public static float CalculateArcDistanceToHorizon(float height, float radius) {
			return radius * Mathf.Acos(radius / height);
		}

		//Calculate culling angle, Height is eye distance to planet core
		public static float CalculateCullingAngle(float height, float radius) {
			return Mathf.Acos(radius / height) * Mathf.Rad2Deg;
		}
	}

}