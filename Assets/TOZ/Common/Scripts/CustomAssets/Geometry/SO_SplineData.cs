using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TOZ {

	public sealed class SO_SplineData : ScriptableObjectBase<SplineNode> {
#region Variables
		//Variables
		public SplineType Type;
		public int Segments;
#endregion Variables

#region Mono Methods
		//Mono Methods
		public override void Awake() {
			base.Awake();

			Type = SplineType.Straight;
			Segments = 2;
		}
#endregion Mono Methods

#region Indexer and Converters
		//Converter
		public Vector3[] Positions {
			get {
				return Data.ConvertAll(new System.Converter<SplineNode, Vector3>(
					delegate(SplineNode node) { return node.P; } 
				)).ToArray();
			}
		}

		public Vector2[] Scales {
			get {
				return Data.ConvertAll(new System.Converter<SplineNode, Vector2>(
					delegate(SplineNode node) { return node.S; }
				)).ToArray();
			}
		}

		public Vector3[] Tangents {
			get {
				return Data.ConvertAll(new System.Converter<SplineNode, Vector3>(
					delegate(SplineNode node) { return node.T; }
				)).ToArray();
			}
		}

		public Vector3[] Binormals {
			get {
				return Data.ConvertAll(new System.Converter<SplineNode, Vector3>(
					delegate(SplineNode node) { return node.B; }
				)).ToArray();
			}
		}

		public Vector3[] Normals {
			get {
				return Data.ConvertAll(new System.Converter<SplineNode, Vector3>(
					delegate(SplineNode node) { return node.N; }
				)).ToArray();
			}
		}
#endregion Indexer and Converters
	}

}