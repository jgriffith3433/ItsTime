using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TOZ {

	public sealed class SO_Vector4 : ScriptableObjectBase<Vector4> {
#region Indexer and Converters
		//Converter
		public Vector3[] Positions {
			get {
				return Data.ConvertAll(new System.Converter<Vector4, Vector3>(
					delegate(Vector4 p) { return (Vector3)p; }
				)).ToArray();
			}
		}

		public float[] Alphas {
			get {
				return Data.ConvertAll(new System.Converter<Vector4, float>(
					delegate(Vector4 p) { return p.w; }
				)).ToArray();
			}
		}
#endregion Indexer and Converters
	}

}