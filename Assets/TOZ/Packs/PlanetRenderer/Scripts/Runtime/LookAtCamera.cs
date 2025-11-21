using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TOZ;

namespace TOZ.Celestial {

	[ExecuteInEditMode]
	public sealed class LookAtCamera : GameObjectBase {
#region Mono Methods
		private void OnEnable() {
			DoEnable();
		}

		private void OnDisable() {
			DoDisable();
		}
#endregion Mono Methods

#region Methods
		private void DoEnable() {
			Camera.onPreCull += DoPreCull;
		}

		private void DoDisable() {
			Camera.onPreCull -= DoPreCull;
		}
#endregion Methods

#region Event Methods
		private void DoPreCull(Camera cam) {
			Trs.LookAt(cam.transform.position, Trs.parent.up);
		}
#endregion Event Methods
	}

}