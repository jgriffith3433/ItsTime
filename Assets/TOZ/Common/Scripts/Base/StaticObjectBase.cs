using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TOZ {

	[ExecuteInEditMode]
	public abstract class StaticObjectBase : GameObjectBase {
#region Properties
		public Bounds Bounds { get { return bounds; } }
		public float Volume { get { return volume; } }
		public Bounds IdentityBounds { get { return identityBounds; } }
		public float IdentityVolume { get { return identityVolume; } }
		protected virtual string strInfo {
			get {
				return	"Identity Volume: ".Label() + "{0:0.00# m\xB3}".FormatWith(IdentityVolume).Text()
							+ "\n" +
							"Volume: ".Label() + "{0:0.00# m\xB3}".FormatWith(Volume).Text();
			}
		}
#endregion Properties

#region Variables
		//Components
		[NonSerialized] public Collider[] Colls;

		//Variables
		private Bounds bounds;
		private float volume;
		private Bounds identityBounds;
		private float identityVolume;
#endregion Variables

#region Mono Methods
#if UNITY_EDITOR
		protected virtual void OnValidate() {
			if(Colls == null)
				Awake();
			else
				UpdateComponentsAndVariables();
		}

		protected virtual void OnTransformChildrenChanged() {
			Awake();
		}

		protected virtual void OnDrawGizmosSelected() {
			DoDrawGizmos();
		}
#endif

		protected virtual void Awake() {
			DoAwake();
		}

		protected virtual void OnEnable() {
			DoEnable();
		}

		protected virtual void OnDisable() {
			DoDisable();
		}
#endregion Mono Methods

#region Methods
		private void DoAwake() {
			Colls = this.GetComponentsInChildren<Collider>(true);

			UpdateComponentsAndVariables();
		}

		private void DoEnable() {
			#if UNITY_EDITOR
			if(!Application.isPlaying) {
				EditorApplication.update += UpdateComponentsAndVariables;
			}
			#endif
		}

		private void DoDisable() {
			#if UNITY_EDITOR
			if(!Application.isPlaying) {
				EditorApplication.update -= UpdateComponentsAndVariables;
			}
			#endif
		}

		#if UNITY_EDITOR
		private void DoDrawGizmos() {
			if(Bounds == null)
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

			_UIUtil._Hndl_LabelCenter(PosW, strInfo);
		}
		#endif
#endregion Methods

#region Helper Methods
		private void UpdateComponentsAndVariables() {
			CalculateVolumeAndBounds();
			CalculateIdentityVolumeAndBounds();
		}

		private void CalculateVolumeAndBounds() {
			bounds = new Bounds(PosW, Vector3.zero);
			volume = 0f;
			foreach(Collider c in this.Colls) {
				if(!c.enabled || c.isTrigger || !c.gameObject.activeSelf)
					continue;

				bounds.Encapsulate(c.bounds);
				//Calculate individual solid collider volumes without empty spaces (all boxes though)
				volume += c.bounds.size.x * c.bounds.size.y * c.bounds.size.z;
			}
		}

		private void CalculateIdentityVolumeAndBounds() {
			Quaternion cachedRotation = RotW;
			RotW = Quaternion.identity;
			Physics.SyncTransforms();

			identityBounds = new Bounds(PosW, Vector3.zero);
			identityVolume = 0f;
			foreach(Collider c in this.Colls) {
				if(!c.enabled || c.isTrigger || !c.gameObject.activeSelf)
					continue;

				identityBounds.Encapsulate(c.bounds);
				//Calculate individual solid collider volumes without empty spaces (all boxes though)
				identityVolume += c.bounds.size.x * c.bounds.size.y * c.bounds.size.z;
			}

			RotW = cachedRotation;
			Physics.SyncTransforms();
		}
#endregion Helper Methods
	}

}