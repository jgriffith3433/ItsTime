using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using TOZ;

namespace TOZ.Celestial {

	public abstract class PlayerBase : DynamicObjectBase {
#region Variables
		//Variables
		[Header("Info Settings")]
		[NonSerialized] public PlayerType Type = PlayerType.None;

		[Header("Motion Variables")]
		[SerializeField] protected LayerMask layerMask = 1 << 30;

		internal Vector3 DirectionToClosestCelestial;
		internal Vector3 DirectionToClosestCelestialNormalized;
		internal float DistanceToClosestCelestial;
		internal float GravityMagnitude;
		internal Vector3 Gravity;

		protected float groundHeight;
		protected Vector3 groundNormal;
		protected float groundSteepness;
		private float boundsRatio;
		protected bool isGrounded;

		protected Quaternion gravityRotation;
		protected Quaternion rotations;

		public bool OnSteepGround {
			get { return (groundSteepness >= 0.5f); }
		}
#endregion Variables

#region Mono Methods
		/*
#if UNITY_EDITOR
		protected override void OnDrawGizmosSelected() {
			base.OnDrawGizmosSelected();
			DoDrawGizmos();
		}
#endif
		*/
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

		protected virtual void Update() {
			DoUpdate();
		}
#endregion Mono Methods

#region Methods
		private void DoAwake() {

		}
/*
		private void DoDrawGizmos() {
			Gizmos.DrawLine(this.PosW, this.PosW + (-this.Trs.up * (this.IdentityBounds.extents.y + boundsRatio)));
		}
*/
		private void DoEnable() {
			//Magic numbers on Earth scaled
			this.Rgd.mass *= _SS._Scale;

			DirectionToClosestCelestial = Vector3.zero;
			DirectionToClosestCelestialNormalized = Vector3.zero;
			DistanceToClosestCelestial = Mathf.Infinity;
			GravityMagnitude = 0f;
			Gravity = Vector3.zero;

			groundHeight = Mathf.Infinity;
			groundNormal = Vector3.zero;
			groundSteepness = 0f;
			boundsRatio = this.IdentityBounds.extents.y * 0.1f;
			isGrounded = false;

			gravityRotation = this.RotW;
			rotations = gravityRotation;
		}

		private void DoDisable() {
			gravityRotation = this.RotW;
			rotations = gravityRotation;
		}

		private void DoUpdate() {
			if(_EYES._ClosestCelestial == null) {
				DirectionToClosestCelestial = Vector3.zero;
				DirectionToClosestCelestialNormalized = Vector3.zero;
				DistanceToClosestCelestial = Mathf.Infinity;
				GravityMagnitude = 0f;

				groundHeight = Mathf.Infinity;
				groundNormal = Vector3.zero;
				groundSteepness = 0f;
			}
			else {
				DirectionToClosestCelestial = _DirUtil._Get3D(this.PosW, _EYES._ClosestCelestial.PosW);
				DirectionToClosestCelestialNormalized = DirectionToClosestCelestial.normalized;
				DistanceToClosestCelestial = DirectionToClosestCelestial.magnitude;

				if(_EYES._InNavigationZone) {
					GravityMagnitude = _SS._EarthGravity * _EYES._ClosestCelestial.GravityScale;

					//Down ray only on the planet
					RaycastHit hit;
					if(Physics.Raycast(this.PosW, DirectionToClosestCelestialNormalized, out hit, DistanceToClosestCelestial, layerMask, QueryTriggerInteraction.Ignore)) {
						groundHeight = _DistUtil._Get3D(this.PosW, hit.point);
						groundNormal = hit.normal;
						groundSteepness = 1.0f - _MathUtil._ClampDown(Vector3.Dot(groundNormal, -DirectionToClosestCelestialNormalized), 0f);
					}
				}
				else {
					GravityMagnitude = 0f;

					groundHeight = Mathf.Infinity;
					groundNormal = Vector3.zero;
					groundSteepness = 0f;
				}
			}

			//Calculate final gravity
			Gravity = DirectionToClosestCelestialNormalized * GravityMagnitude;

			//Calculate isGrounded
			switch(Type) {
				case PlayerType.Human:
					isGrounded = (groundHeight <= boundsRatio);
				break;
				case PlayerType.Spacecraft:
				case PlayerType.	Hovercar:
					isGrounded = (groundHeight <= (this.IdentityBounds.extents.y + boundsRatio));
				break;
				default:
					isGrounded = (groundHeight <= (this.IdentityBounds.extents.y + boundsRatio));
				break;
			}
		}
#endregion Methods
	}

}