using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TOZ {

	[Serializable]
	public sealed class SplineNode {
#region Variables
		//Variables
		public Vector3 P;
		public Vector2 S;
		[NonSerialized] public Vector3 T;
		[NonSerialized] public Vector3 B;
		[NonSerialized] public Vector3 N;
#endregion Variables

#region Constructors
		//Constructors
		public SplineNode() : this(Vector3.zero) {
		}

		public SplineNode(Vector3 p) : this(p, Vector2.one) {
		}

		public SplineNode(Vector3 p, Vector2 s) : this(
			p, s, Vector3.forward, Vector3.right, Vector3.up) {
		}

		public SplineNode(Vector3 p, Vector2 s, Vector3 t, Vector3 b, Vector3 n) {
			this.P = p;
			this.S = s;
			this.T = t;
			this.B = b;
			this.N = n;
		}
#endregion Constructors

#region Object Methods
		//Overrides
		public static bool operator ==(SplineNode a, SplineNode b) {
			if(System.Object.ReferenceEquals(a, b)) {
				return true;
			}
			if(((object)a == null) || ((object)b == null)) {
				return false;
			}
			return (a.P == b.P);
		}

		public static bool operator !=(SplineNode a, SplineNode b) {
			return !(a == b);
		}

		public override bool Equals(System.Object obj) {
			if(obj == null) {
				return false;
			}
			SplineNode other = obj as SplineNode;
			if((System.Object)other == null) {
				return false;
			}
			return (this == other);
		}

		public bool Equals(SplineNode other) {
			if((object)other == null) {
				return false;
			}
			return (this == other);
		}

		public override string ToString() {
			return "SplineNode P: {0}".FormatWith(this.P.ToString());
		}

		public override int GetHashCode() {
			return _HashUtil._Get(this.P);
		}
#endregion Object Methods
	}

}