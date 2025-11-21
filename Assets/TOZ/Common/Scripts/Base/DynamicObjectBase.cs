using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TOZ {

	[ExecuteInEditMode]
	[RequireComponent(typeof(Rigidbody))]
	public abstract class DynamicObjectBase : StaticObjectBase {
#region Properties
		public Vector3 Velocity { get { return Rgd.linearVelocity; } }
		public float Speed { get { return Rgd.linearVelocity.magnitude; } }
		public float Density { get { return density; } }
		public Vector3 TensorW { get { return tensorW; } }
		public Vector3 TensorL { get { return tensorL; } }
		protected override string strInfo {
			get {
				return	base.strInfo
							+ "\n" +
							"Weight: ".Label("grey") + "{0:0.00# kgs}".FormatWith(Weight).Text()
							+ "\n" +
							"Density: ".Label("grey") + "{0:0.00# kgs/m\xB3}".FormatWith(Density).Text()
							+ "\n" +
							"Mass: ".Label("grey") + "{0:0.00# kgs}".FormatWith(Rgd.mass).Text()
							+ "\n" +
							"Velocity: ".Label("grey") + "{0:0.00# m/s}".FormatWith(Velocity).Text()
							+ "\n" +
							"Speed: ".Label("grey") + "{0:0.00# m/s}".FormatWith(Speed).Text();
			}
		}
#endregion Properties

#region Variables
		//Components
		[NonSerialized] public Rigidbody Rgd;

		//Variables
		[Header("Physics Variables")]
		public float Weight = 1.0f;

		private float density;
		private Vector3 tensorW;
		private Vector3 tensorL;
#endregion Variables

#region Mono Methods
#if UNITY_EDITOR
		protected override void OnValidate() {
			base.OnValidate();
			if(Rgd == null)
				Awake();
			else
				UpdateComponentsAndVariables();
		}

		protected override void OnTransformChildrenChanged() {
			base.OnTransformChildrenChanged();
			Awake();
		}

		protected override void OnDrawGizmosSelected() {
			//base.OnDrawGizmosSelected();
			DoDrawGizmos();
		}
#endif

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
#endregion Mono Methods

#region Methods
		private void DoAwake() {
			Rgd = this.GetComponent<Rigidbody>();

			UpdateComponentsAndVariables();
		}

		private void DoEnable() {
			#if UNITY_EDITOR
			if(!Application.isPlaying) {
				EditorApplication.update += UpdateComponentsAndVariables;
			}
			#endif
			//Magic numbers on Earth
			Rgd.linearDamping = 0.9803921f;
			Rgd.angularDamping = 3.54862f;
		}

		private void DoDisable() {
			#if UNITY_EDITOR
			if(!Application.isPlaying) {
				EditorApplication.update -= UpdateComponentsAndVariables;
			}
			#endif
			//Magic numbers reset
			Rgd.linearDamping = 0.0f;
			Rgd.angularDamping = 0.05f;
		}

#if UNITY_EDITOR
		private void DoDrawGizmos() {
			if(Rgd == null)
				return;

			Color cachedColor = Gizmos.color;
			Gizmos.color = Color.blue;
			Matrix4x4 cachedMatrix = Gizmos.matrix;
			Gizmos.matrix = Trs.localToWorldMatrix;
			Gizmos.DrawWireCube(IdentityBounds.center - PosW, _VecUtil._Divide(IdentityBounds.size, Trs.localScale));
			Gizmos.matrix = cachedMatrix;
			Gizmos.color = Color.red;
			Gizmos.DrawWireCube(Bounds.center, Bounds.size);
			Gizmos.color = cachedColor;

			_UIUtil._Hndl_LabelCenter(Rgd.worldCenterOfMass, strInfo);

			_UIUtil._Hndl_Sphere(Rgd.worldCenterOfMass, _VecUtil._Min(this.Bounds.size) * 0.2f, Color.magenta);
			_UIUtil._Hndl_Sphere(Rgd.worldCenterOfMass + Rgd.inertiaTensor, _VecUtil._Min(this.Bounds.size) * 0.1f, Color.black);
			_UIUtil._Hndl_Line(Rgd.worldCenterOfMass, Rgd.worldCenterOfMass + Rgd.inertiaTensor, 2f, Color.black);
		}
#endif
#endregion Methods

#region Helper Methods
		private void UpdateComponentsAndVariables() {
			CalculateRigidbodyProperties();
		}

		private void CalculateRigidbodyProperties() {
			Rgd.mass = Weight / Physics.gravity.magnitude;
			Rgd.ResetCenterOfMass();
			Rgd.ResetInertiaTensor();
			
			density = Rgd.mass / this.IdentityVolume;
			tensorW = Rgd.inertiaTensorRotation * Rgd.inertiaTensor;
			tensorL = Rgd.rotation * tensorW;
		}
#endregion Helper Methods
	}

}