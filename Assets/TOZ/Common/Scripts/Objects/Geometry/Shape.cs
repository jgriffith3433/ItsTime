using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TOZ {

	[Serializable]
	public sealed class Shape {
#region Variables
		//Variables
		public Vector4[] P;
#endregion Variables

#region Properties
		//Properties
		public int Count { get { return P.Length; } }
		public bool IsClosed { get { return P[0] == P[Count-1]; } }
#endregion Properties

#region Constructors
		//Constructors
		public Shape() : this(null) {
		}

		public Shape(Vector4[] p) {
			P = p;
		}
#endregion Constructors

#region Indexer and Converters
		//Indexer
		public Vector4 this[int i] {
			get { return P[i]; } set { P[i] = value; }
		}

		//Converters
		public Vector3[] Positions {
			get {
				return Array.ConvertAll(P, new System.Converter<Vector4, Vector3>(
					delegate(Vector4 p) { return (Vector3)p; }
				));
			}
		}

		public float[] Alphas {
			get {
				return Array.ConvertAll(P, new System.Converter<Vector4, float>(
					delegate(Vector4 p) { return p.w; }
				));
			}
		}
#endregion Indexer and Converters

#region Methods
		//Methods
		public void Close() {
			if(!IsClosed) {
				Vector4[] tmp = new Vector4[this.Count+1];
				for(int i = 0; i < this.Count; i++) {
					tmp[i] = P[i];
				}
				tmp[tmp.Length-1] = P[0];
				P = tmp;
				tmp = null;
			}
		}

		public void Open() {
			if(IsClosed) {
				Vector4[] tmp = new Vector4[this.Count-1];
				for(int i = 0; i < this.Count-1; i++) {
					tmp[i] = P[i];
				}
				P = tmp;
				tmp = null;
			}
		}

		public float CalculateArea() {
			return _GeomUtil._CalculateArea(Positions);
		}

		public Vector3 CalculateCenteroid() {
			return _GeomUtil._CalculateCenteroid(Positions);
		}
#endregion Methods

#region Object Methods
		//Overrides
		public override string ToString() {
			return "Shape Count: {0}".FormatWith(this.Count);
		}
#endregion Object Methods
	}

}