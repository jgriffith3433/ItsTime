using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TOZ;

namespace TOZ.Celestial {

	public enum CelestialType {
		None, Sun, Planet
	}

	public enum PlanetQualityType {
		Low, Medium, Normal, Good, High, Ultra
	}

	public enum PlanetSurfaceType {
		None, Ground, Water
	}

	public enum PlanetUvType {
		Planar, Spherical
	}

	public enum PlanetSideType {
		Back, Right, Top, Front, Left, Bottom
	}

	public enum PlanetShaderType {
		Gradient, Bumped, Splat, Albedo
	}

	public enum PlayerType {
		None, Human, Hovercar, Spacecraft
	}

}