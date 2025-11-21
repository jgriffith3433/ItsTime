using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TOZ;

namespace TOZ.Celestial {

	public sealed class SpaceShip : PlayerBase {
#region Variables
		//Variables
		internal Transform Cockpit;
		internal Transform Coll;
		internal bool IsActive;

		[Header("Motion Variables")]
		private float maxTorqueSpeed = 0.01f;
		private Vector3 forceTorque;
		private Vector3 forceMovement;
		
		//private float cullingAngle;
#endregion Variables

#region Mono Methods
		protected override void Awake() {
			base.Awake();
			DoAwake();
		}

		protected override void OnEnable() {
			base.OnEnable();
			DoEnable();
		}

		protected override void OnDisable() {
			base.OnDisable();
			DoDisable();
		}

		protected override void Update() {
			base.Update();
			DoUpdate();
		}

		private void FixedUpdate() {
			DoFixedUpdate();
		}
#endregion Mono Methods

#region Methods
		private void DoAwake() {
			this.Type = PlayerType.Spacecraft;
			
			Cockpit = this.Trs.GetChild(0);
			Coll = this.Trs.GetChild(1);
		}

		private void DoEnable() {
			IsActive = true;
		}

		private void DoDisable() {
			IsActive = false;
		}

	private void DoUpdate() {
		if(!IsActive)
			return;

		// Use much slower rotation speed when not in gravity zone (in orbit)
		float rotationSpeed = _EYES._InGravityZone ? maxTorqueSpeed * 10f : maxTorqueSpeed * 0.5f;

		if(_EYES._InGravityZone) {
			this.gravityRotation = Quaternion.LookRotation(this.Trs.forward, -this.Gravity);
			this.rotations = Quaternion.Slerp(this.RotW, this.gravityRotation, Time.deltaTime * rotationSpeed);
		}
		else {
			this.gravityRotation = this.RotW;
			this.rotations = Quaternion.Slerp(this.rotations, this.gravityRotation, Time.deltaTime * rotationSpeed);
		}

		forceTorque = Vector3.zero;
		forceMovement = Vector3.zero;
	}

	private void DoFixedUpdate() {
		if(!IsActive)
			return;

		if(!isGrounded) {
			this.Rgd.AddForce(this.Gravity, ForceMode.Acceleration);
		}

		this.Rgd.MoveRotation(this.rotations);
		this.Rgd.AddTorque(forceTorque, ForceMode.VelocityChange);
		this.Rgd.AddForce(forceMovement * this.Rgd.mass, ForceMode.Force);
	}

	// void OnGUI() {
	// 		//GUILayout.Label("CullAngle: " + cullingAngle.ToString());
	// 		//GUILayout.Label("Closest Celestial: " + _EYES._ClosestCelestial.ToString());
	// 		GUILayout.Label("Ground Steepness: " + this.groundSteepness.ToString());
	// 		GUILayout.Label("Ground Height: " + this.groundHeight.ToString());
	// 		GUILayout.Label("IsGrounded: " + this.isGrounded.ToString());
	// 		GUILayout.Label("_InNavigationZone: " + _EYES._InNavigationZone);
	// 		GUILayout.Label("_InGravityZone: " + _EYES._InGravityZone);
	// 		GUILayout.Label("_InAtmosphere: " + _EYES._InAtmosphere);
	// 		GUILayout.Label("_InClouds: " + _EYES._InClouds);
	// 		GUILayout.Label("InWater: " + _EYES._InWater);
	// 	}

		// void OnTriggerEnter(Collider other) {
		// 	if(other.tag == "Player")
		// 		Debug.Log("Entered trigger");
		// }

		// void OnTriggerExit(Collider other) {
		// 	if(other.tag == "Player")
		// 		Debug.Log("Exited trigger");
		// }
#endregion Methods
	}

}
/*
			if(Input.GetKeyDown(KeyCode.H)) {
				hovercar.gameObject.SetActive(true);
				hovercar.localPosition = Vector3.forward * 2f;
				hovercar.parent = null;
				this.enabled = false;
				_EYES._Head.parent = hovercar.GetChild(0);
				_EYES._Head.localPosition = Vector3.zero;
				_EYES._Head.localRotation = Quaternion.identity;
				_EYES._Head.localScale = Vector3.zero;
			}
*/