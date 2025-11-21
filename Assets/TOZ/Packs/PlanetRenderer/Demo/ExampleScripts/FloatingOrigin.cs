using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TOZ;

namespace TOZ.Celestial {

	public sealed class FloatingOrigin : MonoBehaviour {
#region Variables
		[SerializeField] private float distanceFromOrigin = 500f;
		private GameObject nebula;
		private GameObject asteroids;
#endregion Variables

#region Mono Methods
		private void LateUpdate() {
			DoLateUpdate();
		}
#endregion Mono Methods

#region Methods
		private void Start() {
			_SS._Star.AreCelestialsDynamic = true;
			nebula = GameObject.Find("Nebula");
			asteroids = GameObject.Find("AsteroidField");
		}

		private void DoLateUpdate() {
			/*
			This is an example script only, you must write your own.
			But the basic idea is, to move everything with an offset when the camera
			position is larger than a certain distance.
			*/
			if(transform.root.position.magnitude > distanceFromOrigin) {
				foreach(CelestialBase cBase in CelestialBase._List.Data) {
					cBase.PosW -= transform.root.position;
				}
				//Forexample, the nav way points should be recreated here every time
				//But then, the pathfinding will have to be offset as well
				if(nebula != null) {
					nebula.transform.position -= transform.root.position;
				}
				if(asteroids != null) {
					asteroids.transform.position -= transform.root.position;
				}
				//Finally move the camera
				transform.root.position -= transform.root.position;
			}
		}
#endregion Methods
	}

}