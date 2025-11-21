using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TOZ {

#region Enums
	[Serializable]
	public enum SplineType {
		Open, Closed, Straight
	}
#endregion Enums

	[Serializable]
	public sealed class Spline {
#region Variables
		//Variables
		public SplineType Type;
		public int Segments;
		public List<SplineNode> Nodes;
#endregion Variables

#region Constructors
		//Constructors
		public Spline() : this(SplineType.Straight, 2) { 
		}

		public Spline(SplineType type, int segments) {
			this.Type = type;
			this.Segments = segments;
			Clear();
		}
#endregion Constructors

#region Indexer and Converters
		//Indexer
		public SplineNode this[int i] {
			get { return Nodes[i]; }
			set { Nodes[i] = value; }
		}

		//Converters
		public SplineNode[] Array() {
			return Nodes.ToArray();
		}

		public Vector3[] Positions {
			get {
				return Nodes.ConvertAll(new System.Converter<SplineNode, Vector3>(
					delegate(SplineNode node) { return node.P; }
				)).ToArray();
			}
		}

		public Vector2[] Scales {
			get {
				return Nodes.ConvertAll(new System.Converter<SplineNode, Vector2>(
					delegate(SplineNode node) { return node.S; }
				)).ToArray();
			}
		}

		public Vector3[] Tangents {
			get {
				return Nodes.ConvertAll(new System.Converter<SplineNode, Vector3>(
					delegate(SplineNode node) { return node.T; }
				)).ToArray();
			}
		}

		public Vector3[] Binormals {
			get {
				return Nodes.ConvertAll(new System.Converter<SplineNode, Vector3>(
					delegate(SplineNode node) { return node.B; }
				)).ToArray();
			}
		}

		public Vector3[] Normals {
			get {
				return Nodes.ConvertAll(new System.Converter<SplineNode, Vector3>(
					delegate(SplineNode node) { return node.N; }
				)).ToArray();
			}
		}
#endregion Indexer and Converters

#region List Methods
		public void Clear() {
			if(Nodes != null) {
				Nodes.Clear();
			}
			else {
				Nodes = new List<SplineNode>();
			}
		}

		public int Count() {
			return Nodes.Count;
		}

		public void Add() {
			Add(default(SplineNode));
		}

		public void Add(SplineNode i) {
			if(Nodes != null) {
				Nodes.Add(i);
			}
		}

		public void Insert(int index) {
			Insert(index, default(SplineNode));
		}

		public void Insert(int index, SplineNode i) {
			if(Nodes != null) {
				Nodes.Insert(index, i);
			}
		}

		public void Remove(SplineNode i) {
			if(Nodes != null && !Contains(i)) {
				Nodes.Remove(i);
			}
		}

		public void RemoveAt(int index) {
			if(Nodes != null && index < Count()) {
				Nodes.RemoveAt(index);
			}
		}

		public int IndexOf(SplineNode i) {
			return Nodes.IndexOf(i);
		}

		public bool Contains(SplineNode i) {
			return Nodes.Contains(i);
		}

		public void Reverse() {
			if(Nodes != null) {
				Nodes.Reverse();
			}
		}
#endregion List Methods

#region Methods
		//Methods
		public void Initialize(List<Vector3> list, bool calculateTBN) {
			Initialize(list.ToArray(), calculateTBN);
		}

		public void Initialize(Vector3[] list, bool calculateTBN) {
			SplineNode[] nodes = new SplineNode[list.Length];
			for(int i = 0; i < nodes.Length; i++) {
				nodes[i] = new SplineNode(list[i]);
			}
			Initialize(nodes, calculateTBN);
		}

		public void Initialize(List<SplineNode> nodes, bool calculateTBN) {
			Initialize(nodes.ToArray(), calculateTBN);
		}

		public void Initialize(SplineNode[] nodes, bool calculateTBN) {
			if(nodes.Length < 4) {
				Debug.LogWarning("You need 4 or more nodes to create a spline.");
				return;
			}

			Clear();
			CalculateNodes(nodes);
			if(calculateTBN) {
				CalculateTBN(nodes);
			}
		}

		public float GetLength() {
			float d = 0f;
			for(int i = 0; i < Count()-1; i++) {
				d += _DistUtil._Get3D(Nodes[i].P, Nodes[i+1].P);
			}
			return d;
		}

		public SplineNode GetNodeAtTime(float time) {
			time = Mathf.Clamp(time, 0f, 1f);
			int size = Count();
			float rate = 1.0f / (float)(size-1);
			int i = Mathf.RoundToInt(time / rate);

			return Nodes[((i+0) + size) % size];
		}

		public Vector3 GetPosAtTime(float time) {
			time = Mathf.Clamp(time, 0f, 1f);
			int size = Count();
			float rate = 1.0f / (float)(size-1);
			int i = (int)(time / rate);

			Vector3 p1 = Nodes[((i-1) + size) % size].P;
			if(Type.IsNot(SplineType.Closed) && i == 0) {
				p1 = Nodes[0].P + _DirUtil._Get3D(Nodes[1].P, Nodes[0].P);
			}
			Vector3 p2 = Nodes[((i+0) + size) % size].P;
			Vector3 p3 = Nodes[((i+1) + size) % size].P;
			Vector3 p4 = Nodes[((i+2) + size) % size].P;
			if(Type.IsNot(SplineType.Closed) && i == size-2) {
				p4 = Nodes[size-1].P + _DirUtil._Get3D(Nodes[size-2].P, Nodes[size-1].P);
			}
			float localTime = (time - ((float)i * rate)) / rate;

			return _InterpolationUtil._CatmullRom(p1, p2, p3, p4, localTime);
		}

		private void CalculateNodes(SplineNode[] nodes) {
			int size = nodes.Length;
			Vector3 p1 = Vector3.zero, p2 = Vector3.zero, p3 = Vector3.zero, p4 = Vector3.zero;
			
			int sp = (Type.Is(SplineType.Closed)) ? 0 : (Type.Is(SplineType.Open)) ? 1 : 0;
			int ep = (Type.Is(SplineType.Closed)) ? size : (Type.Is(SplineType.Open)) ? size-2 : size-1;
			int p;

			for(p = sp; p < ep; p++) {
				p1 = nodes[((p-1) + size) % size].P;
				if(Type.Is(SplineType.Straight) && p == 0) {
					p1 = nodes[0].P + _DirUtil._Get3D(nodes[1].P, nodes[0].P);
				}
				p2 = nodes[((p+0) + size) % size].P;
				p3 = nodes[((p+1) + size) % size].P;
				p4 = nodes[((p+2) + size) % size].P;
				if(Type.Is(SplineType.Straight) && p == size-2) {
					p4 = nodes[size-1].P + _DirUtil._Get3D(nodes[size-2].P, nodes[size-1].P);
				}
				//Main nodes
				Add(new SplineNode(p2, nodes[((p+0) + size) % size].S));
				//Segment nodes
				for(int s = 1; s < Segments; s++) {
					float t = (float)s / Segments;
					Add(new SplineNode(_InterpolationUtil._CatmullRom(p1, p2, p3, p4, t), Vector2.Lerp(nodes[((p+0) + size) % size].S, nodes[((p+1) + size) % size].S, t)));
				}
			}

			//Final node
			Nodes.Add(new SplineNode(p3, nodes[((p+0) + size) % size].S));
		}

		private void CalculateTBN(SplineNode[] nodes) {
			int size = Count();
			for(int n = 1; n < size-1; n++) {
				Nodes[n].T = (_DirUtil._Get3D(Nodes[n-1].P, Nodes[n].P) + _DirUtil._Get3D(Nodes[n].P, Nodes[n+1].P)).normalized;
				Nodes[n].B = -Vector3.Cross(Nodes[n].T, Vector3.up).normalized;
				Nodes[n].N = -Vector3.Cross(Nodes[n].B, Nodes[n].T).normalized;
			}

			if(Type.Is(SplineType.Closed)) {
				//First Vertex tangent
				Nodes[0].T = (_DirUtil._Get3D(Nodes[size-2].P, Nodes[0].P) + _DirUtil._Get3D(Nodes[0].P, Nodes[1].P)).normalized;
				Nodes[0].B = -Vector3.Cross(Nodes[0].T, Vector3.up).normalized;
				Nodes[0].N = -Vector3.Cross(Nodes[0].B, Nodes[0].T).normalized;
				//Last Vertex tangent (Same as first vertex, for smoothness)
				Nodes[size-1].T = Nodes[0].T;
				Nodes[size-1].B = Nodes[0].B;
				Nodes[size-1].N = Nodes[0].N;
			}
			else if(Type.Is(SplineType.Straight)) {
				//First Vertex tangent
				Nodes[0].T = _DirUtil._Get3DNormalized(Nodes[0].P, Nodes[1].P);
				Nodes[0].B = -Vector3.Cross(Nodes[0].T, Vector3.up).normalized;
				Nodes[0].N = -Vector3.Cross(Nodes[0].B, Nodes[0].T).normalized;
				//Last Vertex tangent
				Nodes[size-1].T = _DirUtil._Get3DNormalized(Nodes[size-2].P, Nodes[size-1].P);
				Nodes[size-1].B = -Vector3.Cross(Nodes[size-1].T, Vector3.up).normalized;
				Nodes[size-1].N = -Vector3.Cross(Nodes[size-1].B, Nodes[size-1].T).normalized;
			}
			else {
				//First Vertex tangent
				Nodes[0].T = (_DirUtil._Get3D(nodes[0].P, Nodes[0].P) + _DirUtil._Get3D(Nodes[0].P, Nodes[1].P)).normalized;
				Nodes[0].B = -Vector3.Cross(Nodes[0].T, Vector3.up).normalized;
				Nodes[0].N = -Vector3.Cross(Nodes[0].B, Nodes[0].T).normalized;
				//Last Vertex tangent
				Nodes[size-1].T = (_DirUtil._Get3D(Nodes[size-2].P, Nodes[size-1].P) + _DirUtil._Get3D(Nodes[size-1].P, nodes[nodes.Length-1].P)).normalized;
				Nodes[size-1].B = -Vector3.Cross(Nodes[size-1].T, Vector3.up).normalized;
				Nodes[size-1].N = -Vector3.Cross(Nodes[size-1].B, Nodes[size-1].T).normalized;
			}
		}
#endregion Methods

#region Object Methods
		//Overrides
		public override string ToString() {
			return "Spline Count: {0}".FormatWith(this.Count());
		}
#endregion Object Methods
	}

}