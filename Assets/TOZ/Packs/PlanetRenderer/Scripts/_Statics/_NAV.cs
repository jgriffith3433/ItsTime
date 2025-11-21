using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TOZ;

namespace TOZ.Celestial {

	public static class _NAV {
#region Variables
		//Variables
		internal static Vector3[] _SphericalData;

		public static List<Vector3> _Wps;

		//Constructor
		static _NAV() {
			_Initialize();
		}
#endregion Variables

#region Methods
		internal static void _Setup() {
			_Initialize();

			_GetSphericalData(5, 8);
		}

		internal static void _Initialize() {
			if(_Wps != null) {
				_Wps.Clear();
			}
			else {
				_Wps = new List<Vector3>();
			}
		}

		internal static void _Add(Vector3 wp) {
			_Wps.Add(wp);
		}

		internal static void _Remove(Vector3 wp) {
			_Wps.Remove(wp);
		}

		internal static void _Update() {
			_Wps.Clear();
			for(int i = 0; i < CelestialBase._List.Count(); i++) {
				CelestialBase._List[i].CreateSphericalWaypoints();
			}
		}

		private static void _GetSphericalData(int latTotal, int lonTotal) {
			float latAngle = 180f / (latTotal-1);
			float lonAngle = 360f / lonTotal;
			int navTotal = (latTotal * lonTotal) - 2;

			_SphericalData = new Vector3[navTotal];

			int i = 0;
			float ang, sinY, x, y, z;
			for(int lat = 0; lat < latTotal; lat++) {
				ang = (lat * latAngle) * Mathf.Deg2Rad;
				y = Mathf.Cos(ang);
				sinY = Mathf.Sin(ang);
				for(int lon = 0; lon < lonTotal; lon++) {
					ang = (lon * lonAngle) * Mathf.Deg2Rad;
					x = Mathf.Sin(ang) * sinY;
					z = Mathf.Cos(ang) * sinY;

					_SphericalData[i] = new Vector3(x, y, z);
					i++;

					if(lat == 0 || lat == (latTotal-1))
						break;
				}
			}
		}
#endregion Methods
	}

}