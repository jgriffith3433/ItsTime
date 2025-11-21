using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TOZ;

namespace TOZ.Celestial {

	public class Human : PlayerBase {
#region Variables
		//Variables
		internal Transform Cockpit;
		internal Transform Coll;
		internal bool IsActive;

		[Header("Motion Variables")]
		[SerializeField] private float mouseSensitivity = 10.0f;
		[SerializeField] private float maxHeadAngle = 80.0f;

		[SerializeField] private float walkSpeed = 1f;
		[SerializeField] private float runSpeed = 2f;
		[SerializeField] private float jumpHeight = 1f;
		[SerializeField] private int jumpDelay = 15;
		private Vector2 lookRotation;
		private float speed;
		private Vector3 motionDirection;
		private bool isRunning;
		private bool isJumping;
		private bool isFalling;
		private int jumpTimer;
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
			this.Type = PlayerType.Human;
			this.Rgd.centerOfMass = Vector3.zero;
			
			Cockpit = this.Trs.GetChild(0);
			Coll = this.Trs.GetChild(1);
		}

		private void DoEnable() {
			this.Rgd.linearDamping *= 0f;
			this.Rgd.angularDamping *= 0f;

			IsActive = true;

			lookRotation = Vector2.zero;
			speed = 0f;
			motionDirection = Vector3.zero;
			isRunning = false;
			isJumping = false;
			isFalling = false;
			jumpTimer = 0;
		}

		private void DoDisable() {
			IsActive = false;

			lookRotation = Vector2.zero;
			speed = 0f;
			motionDirection = Vector3.zero;
			isRunning = false;
			isJumping = false;
			isFalling = false;
			jumpTimer = 0;
		}

		private void DoUpdate() {
			if(!IsActive)
				return;

			if(_EYES._InGravityZone) {
				this.gravityRotation = Quaternion.FromToRotation(Vector3.up, -this.DirectionToClosestCelestialNormalized);
				this.rotations = this.gravityRotation;
			}
			else {
				this.gravityRotation = this.RotW;
				this.rotations = this.gravityRotation;
			}

			//Body rotation
			lookRotation.y += Input.GetAxis("Mouse X") * mouseSensitivity;
			lookRotation.y = _AngleUtil._Clamp(lookRotation.y, -180, 180);
			rotations *= Quaternion.AngleAxis(lookRotation.y, Vector3.up);
			//Camera rotation
			lookRotation.x -= Input.GetAxis("Mouse Y") * mouseSensitivity;
			lookRotation.x = _AngleUtil._Clamp(lookRotation.x, -maxHeadAngle, maxHeadAngle);
			_EYES._Head.localRotation = Quaternion.AngleAxis(lookRotation.x, Vector3.right);

			Vector3 inputDirection = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
			isRunning = Input.GetKey(KeyCode.LeftShift) & (!isJumping & !isFalling);
			speed = isRunning ? runSpeed : walkSpeed;
			bool inputJumping = Input.GetKeyDown(KeyCode.R);

			if(this.isGrounded) {
				motionDirection = this.Trs.TransformDirection(inputDirection);
				motionDirection = Vector3.ProjectOnPlane(motionDirection, this.groundNormal);
				motionDirection *= speed;

				//Handle slopes
				if(this.OnSteepGround) {
					Vector3 slideDirection = Vector3.Reflect(this.groundNormal, this.DirectionToClosestCelestialNormalized).normalized;
					motionDirection += slideDirection * speed;
				}

				//Handle Jumping
				if(isJumping) {
					isJumping = false;
				}

				if(inputJumping && !isJumping && jumpTimer <= 0) {
					isJumping = true;
					jumpTimer = jumpDelay;
					float jumpSpeed = Mathf.Sqrt(2.0f * jumpHeight * this.GravityMagnitude);
					motionDirection += -this.DirectionToClosestCelestialNormalized * jumpSpeed;
				}
				else {
					jumpTimer--;
					jumpTimer = _MathUtil._ClampDown(jumpTimer, 0);
				}

				//Handle Falling
				if(isFalling) {
					isFalling = false;
				}
			}
			else {
				if(Vector3.Dot(this.Rgd.linearVelocity, -this.DirectionToClosestCelestialNormalized) <= 0f && !isFalling) {
					isFalling = true;
					isJumping = false;
				}

				motionDirection += this.Gravity * Time.deltaTime;
				if(isFalling)
					motionDirection *= 1.0f - Time.deltaTime * 5.0f;
			}
		}

		private void DoFixedUpdate() {
			if(!IsActive)
				return;

			this.Rgd.MoveRotation(this.rotations);

			Vector3 velChange = motionDirection - this.Rgd.linearVelocity;
			this.Rgd.AddForce(velChange, ForceMode.VelocityChange);
		}

		void OnGUI() {
			GUILayout.Label("Use Mouse+WASD to move, R to jump");
			GUILayout.Label("IsGrounded: " + this.isGrounded.ToString());
			GUILayout.Label("Ground Steepness: " + this.groundSteepness.ToString());
			GUILayout.Label("Ground Height: " + this.groundHeight.ToString());

			GUILayout.Label("Motion: " + motionDirection.ToString());
			GUILayout.Label("Speed: " + speed.ToString());
			GUILayout.Label("Velocity: " + this.Rgd.linearVelocity.magnitude.ToString());
			GUILayout.Label("isRunning: " + isRunning.ToString());
			GUILayout.Label("isJumping: " + isJumping.ToString());
			GUILayout.Label("isFalling: " + isFalling.ToString());
		}
#endregion Methods
	}

}