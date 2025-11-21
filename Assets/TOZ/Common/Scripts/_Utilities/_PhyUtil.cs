/**********************************NOTES********************************************
NOTES:
volume = collider.bounds.size.x * collider.bounds.size.y * collider.bounds.size.z;
volume = mass / density
density = mass / volume;
mass = density x volume
mass = Weight / Physics.gravity.magnitude;

force = mass x acceleration
mass = force / acceleration
acceleration = force / mass

speed = distance / time
velocity = distance / totalTimeToTravelDistance
totalTimeToTravelDistance = distance / velocity
distance = velocity * totalTimeToTravelDistance
acceleration = velocity * Time.deltaTime

USAGE:
**************************************************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TOZ {

	public static class _PhyUtil {
		//Calculate trajectory vector force to reach target
		//Use as ForceMode.VelocityChange
		public static Vector3 _GetVelocityToReachTarget(Vector3 origin, Vector3 target, float time, float offsV, float offsH) {
			Vector3 direction = _DirUtil._Get3D(origin, target);
			float height = direction.y + offsV;
			direction.y = 0f;
			float distance = direction.magnitude + offsH;
			//Calculate forces and apply
			float forceV = (height / time) + (0.5f * Physics.gravity.magnitude * time);
			float forceH = distance / time;
			Vector3 result = direction.normalized;
			result *= forceH;
			result.y = forceV;
			return result;
		}

		public static Vector3 _GetVelocityToReachTarget(Vector3 origin, Vector3 target, float time) {
			return _GetVelocityToReachTarget(origin, target, time, 0.0f, 0.0f);
		}

		public static float _GetFinalVelocity(float velocityChange, float drag) {
			return velocityChange * (1.0f / Mathf.Clamp01(drag * Time.fixedDeltaTime) - 1.0f);
		}

		public static float _GetFinalVelocityFromAcceleration(float acceleration, float drag) {
			return _GetFinalVelocity(acceleration * Time.fixedDeltaTime, drag);
		}

		public static float _GetFinalVelocityFromForce(float force, float drag, float mass) {
			return _GetFinalVelocityFromAcceleration((force / mass), drag);
		}

		public static float _GetDrag(float desiredVelocity, float velocityChange) {
			return velocityChange / ((desiredVelocity + velocityChange) * Time.fixedDeltaTime);
		}

		public static float _GetDragFromAcceleration(float desiredVelocity, float acceleration) {
			return _GetDrag(desiredVelocity, (acceleration * Time.fixedDeltaTime));
		}

		public static float _GetDragFromForce(float desiredVelocity, float force, float mass) {
			return _GetDragFromAcceleration(desiredVelocity, (force / mass));
		}

		public static float _GetRequiredVelocityChange(float desiredVelocity, float drag) {
			float dragMulti = Mathf.Clamp01(drag * Time.fixedDeltaTime);
			return desiredVelocity * dragMulti / (1.0f - dragMulti);
		}

		public static float _GetRequiredAcceleraton(float desiredVelocity, float drag) {
			return _GetRequiredVelocityChange(desiredVelocity, drag) / Time.fixedDeltaTime;
		}

		public static float _GetRequiredForce(float desiredVelocity, float drag, float mass) {
			return _GetRequiredAcceleraton(desiredVelocity, drag) * mass;
		}
	}

}