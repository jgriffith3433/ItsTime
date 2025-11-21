using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TOZ {

	public static class _QuatUtil {
		//Reverse the Quaternion
		public static Quaternion _Negate(Quaternion q) { return new Quaternion(-q.x, -q.y, -q.z, -q.w); }

		//Add 2 quaternions
		public static Quaternion _Add(Quaternion q1, Quaternion q2) {
			Quaternion result = Quaternion.identity;
			result.x = q1.x + q2.x;
			result.y = q1.y + q2.y;
			result.z = q1.z + q2.z;
			result.w = q1.w + q2.w;
			return result;
		}

		//Subtract 2 quaternions
		public static Quaternion _Subtract(Quaternion q1, Quaternion q2) {
			Quaternion result = Quaternion.identity;
			result.x = q1.x - q2.x;
			result.y = q1.y - q2.y;
			result.z = q1.z - q2.z;
			result.w = q1.w - q2.w;
			return result;
		}

		//Multiply by value
		public static Quaternion _Multiply(Quaternion q, float v) {
			Quaternion result = Quaternion.identity;
			result.x = q.x * v;
			result.y = q.y * v;
			result.z = q.z * v;
			result.w = q.w * v;
			return result;
		}

		//Multiply 2 quaternions
		public static Quaternion _Multiply(Quaternion q1, Quaternion q2) {
			Quaternion result = Quaternion.identity;
			result.x = (q2.x * q1.w + q1.x * q2.w + q2.y * q1.z) - (q2.z * q1.y);
			result.y = (q2.y * q1.w + q1.y * q2.w + q2.z * q1.x) - (q2.x * q1.z);
			result.z = (q2.z * q1.w + q1.z * q2.w + q2.x * q1.y) - (q2.y * q1.x);
			result.w = (q2.w * q1.w) - (q2.x * q1.x + q2.y * q1.y + q2.z * q1.z);
			return result;
		}

		//Angle of the quaternion in radians
		public static float _GetAngle(Quaternion q) {
			float length = (q.x * q.x) + (q.y * q.y) + (q.z * q.z);
			if(_MathUtil._IsZero(length))
				return 0.0f;

			return 2.0f * Mathf.Acos(_MathUtil._Clamp(q.w, -1.0f, 1.0f));
		}

		//Axis of the quaternion
		public static Vector3 _GetAxis(Quaternion q) {
			float length = (q.x * q.x) + (q.y * q.y) + (q.z * q.z);
			if(_MathUtil._IsZero(length))
				return Vector3.right;

			float inv = 1.0f / Mathf.Sqrt(length);
			return new Vector3(q.x * inv, q.y * inv, q.z * inv);
		}

		//Get forward vector of quaternion
		public static Vector3 _GetForwardVector(Quaternion q) {
			return q * Vector3.forward;
		}

		//Get up vector of quaternion
		public static Vector3 _GetUpVector(Quaternion q) {
			return q * Vector3.up;
		}

		//Get right vector of quaternion
		public static Vector3 _GetRightVector(Quaternion q) {
			return q * Vector3.right;
		}

		//Get quaternion from matrix4x4
		public static Quaternion _GetFromMatrix4x4(Matrix4x4 mtx) {
			return Quaternion.LookRotation(mtx.GetColumn(2), mtx.GetColumn(1));
		}

		//Get a looking at rotation quaternion
		public static Quaternion _RotateToDirection(Vector3 direction) {
			return Quaternion.LookRotation(direction);
		}

		public static Quaternion _RotateToDirection(Vector3 v1, Vector3 v2) {
			return _RotateToDirection(_DirUtil._Get3DNormalized(v1, v2));
		}

		//Align to custom forward and up vectors
		public static Quaternion _RotateToDirection(Vector3 forward, Vector3 up, Vector3 forwardAxis, Vector3 upAxis) {
			Quaternion quatA = Quaternion.LookRotation(forward, up);
			Quaternion quatB = Quaternion.LookRotation(forwardAxis, upAxis);
			return quatA * Quaternion.Inverse(quatB);
		}

		/// Creates a quaternion given a yaw, pitch, and roll value.
		public static Quaternion _PitchYawRoll(float pitch, float yaw, float roll) {
			float halfPitch = pitch * 0.5f;
			float halfYaw = yaw * 0.5f;
			float halfRoll = roll * 0.5f;

			float sinPitch = Mathf.Sin(halfPitch);
			float cosPitch = Mathf.Cos(halfPitch);
			float sinYaw = Mathf.Sin(halfYaw);
			float cosYaw = Mathf.Cos(halfYaw);
			float sinRoll = Mathf.Sin(halfRoll);
			float cosRoll = Mathf.Cos(halfRoll);

			Quaternion result = Quaternion.identity;
			result.x = (cosYaw * sinPitch * cosRoll) + (sinYaw * cosPitch * sinRoll);
			result.y = (sinYaw * cosPitch * cosRoll) - (cosYaw * sinPitch * sinRoll);
			result.z = (cosYaw * cosPitch * sinRoll) - (sinYaw * sinPitch * cosRoll);
			result.w = (cosYaw * cosPitch * cosRoll) + (sinYaw * sinPitch * sinRoll);
			return result;
		}

		//Get a local rotation on local X axis
		public static Quaternion _RotateLocalX(Quaternion original, float angle) {
			return original * Quaternion.AngleAxis(_AngleUtil._Wrap(angle), Vector3.right);
		}

		//Get a local rotation on local Y axis
		public static Quaternion _RotateLocalY(Quaternion original, float angle) {
			return original * Quaternion.AngleAxis(_AngleUtil._Wrap(angle), Vector3.up);
		}

		//Get a local rotation on local Z axis
		public static Quaternion _RotateLocalZ(Quaternion original, float angle) {
			return original * Quaternion.AngleAxis(_AngleUtil._Wrap(angle), Vector3.forward);
		}
	}

}