using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TOZ;

namespace TOZ.Celestial {

	internal static class _SS {
#region Variables
		//Constants
		internal const float _Scale = 0.05f;
		internal const float _EarthGravity = 0.981f;

		//Variables
		internal static Star _Star;
		internal static float _LightPower; //For Scattering (25 < Range < 75)

		//Constructor
		static _SS() {
			_Initialize();
		}
#endregion Variables

#region Methods
		internal static void _Setup(Star star) {
			_Initialize();

			_Star = star;
			_LightPower = 50.0f;

			_EYES._Setup();
			_LOD._Setup(4);
			_NAV._Setup();
		}

		internal static void _Initialize() {
			_NAV._Initialize();
			_LOD._Initialize();
			_EYES._Initialize();

			_LightPower = 50.0f;
			_Star = null;
		}

		internal static void _Update() {
			_EYES._Update();
			_LOD._Update();
			if(_Star.AreCelestialsDynamic) {
				_NAV._Update();
			}
		}
#endregion Methods
	}

}