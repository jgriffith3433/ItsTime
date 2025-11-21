using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TOZ;

namespace TOZ.Celestial {

	public class HoverCar : PlayerBase {
#region Variables
		//Variables
		internal Transform Cockpit;
		internal Transform Coll;
		internal bool IsActive;

		internal bool CanControl;

		[Header("Motion Variables")]
		[SerializeField] private float maxTorqueSpeed = 0.3f;
		private float thrustTorque;
		private Vector3 accelTorque;
		private Vector3 forceTorque;

		private float turbo;

		[SerializeField] private float neutralHeight = 2.0f;
		[SerializeField] private float jumpHeight = 2.0f;
		[NonSerialized] public float ThrustAntiGravity;
		[NonSerialized] public Vector3 AccelAntiGravity;
		[NonSerialized] public Vector3 ForceAntiGravity;

		[SerializeField] private float maxHorizontalSpeed = 1f;
		private float thrustHorizontalMovement;
		private Vector3 accelHorizontalMovement;

		private float thrustLateralMovement;
		private Vector3 accelLateralMovement;

		private Vector3 accelMovement;
		private Vector3 forceMovement;
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
			this.Type = PlayerType.Hovercar;

			Cockpit = this.Trs.GetChild(0);
			Coll = this.Trs.GetChild(1);
		}

		private void DoEnable() {
			IsActive = true;

			CanControl = true;
			Cursor.visible = !CanControl;
			Cursor.lockState = Cursor.visible ? CursorLockMode.None : CursorLockMode.Locked;
		}

		private void DoDisable() {
			IsActive = false;

			CanControl = false;
			Cursor.visible = !CanControl;
			Cursor.lockState = Cursor.visible ? CursorLockMode.None : CursorLockMode.Locked;
		}

		private void DoUpdate() {
			if(!IsActive)
				return;

			if(Input.GetKeyDown(KeyCode.Space)) {
				CanControl = !CanControl;
				Cursor.visible = !CanControl;
				Cursor.lockState = Cursor.visible ? CursorLockMode.None : CursorLockMode.Locked;
			}

			if(_EYES._InGravityZone) {
				//Vector3 forward = this.Trs.forward - (-this.DirectionToClosestCelestialNormalized) * Vector3.Dot(this.Trs.forward, (-this.DirectionToClosestCelestialNormalized));
				Vector3 forward = Vector3.ProjectOnPlane(this.Trs.forward, -this.Gravity);
				this.gravityRotation = Quaternion.LookRotation(forward, -this.Gravity);
				this.rotations = Quaternion.Slerp(this.RotW, this.gravityRotation, Time.deltaTime);
			}
			else {
				this.gravityRotation = this.RotW;
				this.rotations = this.gravityRotation;
			}

			if(!CanControl) {
				AccelAntiGravity = Vector3.zero;
				accelTorque = Vector3.zero;
				forceMovement = Vector3.zero;
			}
			else {
				//Torque
				thrustTorque = maxTorqueSpeed * Input.GetAxis("Mouse X");
				//thrustTorque = maxTorqueSpeed * ((Input.GetKey(KeyCode.E) ? 1f : 0f) - (Input.GetKey(KeyCode.Q) ? 1f : 0f));
				accelTorque = this.Trs.up * thrustTorque;
				forceTorque = accelTorque;

				turbo = (_EYES._InGravityZone ? 1f : 10f) * (Input.GetKey(KeyCode.LeftShift) ? 10f : 1f);
				//Horizontal
				thrustHorizontalMovement = maxHorizontalSpeed * Input.GetAxis("Vertical");
				accelHorizontalMovement = this.Trs.forward * thrustHorizontalMovement;
				//Lateral
				thrustLateralMovement = maxHorizontalSpeed * Input.GetAxis("Horizontal");
				accelLateralMovement = this.Trs.right * thrustLateralMovement;
				//Total
				accelMovement = accelHorizontalMovement + accelLateralMovement;
				forceMovement = turbo * accelMovement;

				float InputVertical = (Input.GetKey(KeyCode.R) ? 1f : 0f) - (Input.GetKey(KeyCode.F) ? 1f : 0f);
				float desiredHeight = neutralHeight + _MathUtil._ClampDown(InputVertical * jumpHeight, 0.25f-neutralHeight);
				ThrustAntiGravity = _InterpolationUtil._Grade(2f, 0f, 0f, desiredHeight * 2f, groundHeight);

				Vector3 vertical = this.Trs.TransformDirection(Vector3.up);
				AccelAntiGravity = vertical * GravityMagnitude * ThrustAntiGravity;
				ForceAntiGravity = this.Rgd.mass * AccelAntiGravity;
			}
		}

		private void DoFixedUpdate() {
			if(!IsActive)
				return;

			if(!isGrounded) {
				this.Rgd.AddForce(this.Gravity, ForceMode.Acceleration);
			}

			this.Rgd.MoveRotation(this.rotations);
			this.Rgd.AddForce(AccelAntiGravity, ForceMode.Acceleration);
			//Torque
			CalculateBankRotation();
			this.Rgd.AddTorque(accelTorque, ForceMode.VelocityChange);
			this.Rgd.AddForce(forceMovement * this.Rgd.mass, ForceMode.Force);
		}
/*
		void OnGUI() {
			GUILayout.Label("Use Mouse+WASD to move, R to jump");
			GUILayout.Label("IsGrounded: " + this.isGrounded.ToString());
			GUILayout.Label("Ground Steepness: " + this.groundSteepness.ToString());
			GUILayout.Label("Ground Height: " + this.groundHeight.ToString());

			GUILayout.Label("Motion: " + motionDirection.ToString());
			GUILayout.Label("Speed: " + speed.ToString());
			GUILayout.Label("Velocity: " + this.Rgd.velocity.magnitude.ToString());
			GUILayout.Label("isRunning: " + isRunning.ToString());
			GUILayout.Label("isJumping: " + isJumping.ToString());
			GUILayout.Label("isFalling: " + isFalling.ToString());
		}*/

		public void CalculateBankRotation() {
			Vector3 forward = this.Trs.forward - groundNormal * Vector3.Dot(this.Trs.forward, groundNormal);
			//Quaternion groundRotation = Quaternion.FromToRotation(Vector3.up, groundNormal);
			Quaternion groundRotation = Quaternion.LookRotation(forward, groundNormal);

			float angle = Quaternion.Angle(this.Rgd.rotation, groundRotation);
			float totalTime = angle / maxTorqueSpeed * Time.deltaTime;
			float rate = Mathf.Min(1f, Time.deltaTime / totalTime);
			Quaternion banking = Quaternion.Slerp(this.Rgd.rotation, groundRotation, rate);
			this.Rgd.MoveRotation(banking);
		}
#endregion Methods
	}

}