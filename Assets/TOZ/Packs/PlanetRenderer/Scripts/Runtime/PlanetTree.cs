using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TOZ;

namespace TOZ.Celestial {

	public sealed class PlanetTree : GameObjectBase {
#region Variables
		//Variables
		internal Planet Planet;
		internal PlanetSurfaceType Surface;
		internal PlanetSideType Side;
		internal List<PlanetTreeNode> Nodes;
		//internal Vector2 LLLatLon, ULLatLon, URLatLon, LRLatLon;
#endregion Variables

#region Mono Methods
		private void OnDisable() {
			DoDisable();
		}
#endregion Mono Methods

#region Methods
		private void DoDisable() {
			DeleteNodes();
		}
#endregion Methods

#region Helper Methods
		internal void Initialize(Planet planet, PlanetSurfaceType surface, PlanetSideType side) {
			Planet = planet;
			Surface = surface;
			Side = side;

			CreateNodes();
		}

		private void CreateNodes() {
			DeleteNodes();

			Nodes = new List<PlanetTreeNode>();

			float step = 2f / _LOD._Subdivisions;
			float offs = _LOD._Subdivisions / 2f * step;
			Vector3 ll = Vector3.zero;
			Vector3 ul = Vector3.zero;
			Vector3 ur = Vector3.zero;
			Vector3 lr = Vector3.zero;

			for(int y = 0; y < _LOD._Subdivisions; y++) {
				float y0 = (y * step) - offs;
				float y1 = ((y+1) * step) - offs;
				for(int x = 0; x < _LOD._Subdivisions; x++) {
					float x0 = (x * step) - offs;
					float x1 = ((x+1) * step) - offs;

					switch(Side) {
						case PlanetSideType.Back:
							ll = new Vector3(x0, y0, -offs);
							ul = new Vector3(x0, y1, -offs);
							ur = new Vector3(x1, y1, -offs);
							lr = new Vector3(x1, y0, -offs);
						break;
						case PlanetSideType.Right:
							ll = new Vector3(offs, y0, x0);
							ul = new Vector3(offs, y1, x0);
							ur = new Vector3(offs, y1, x1);
							lr = new Vector3(offs, y0, x1);
						break;
						case PlanetSideType.Top:
							ll = new Vector3(x0, offs, y0);
							ul = new Vector3(x0, offs, y1);
							ur = new Vector3(x1, offs, y1);
							lr = new Vector3(x1, offs, y0);
						break;
						case PlanetSideType.Front:
							ll = new Vector3(-x0, y0, offs);
							ul = new Vector3(-x0, y1, offs);
							ur = new Vector3(-x1, y1, offs);
							lr = new Vector3(-x1, y0, offs);
						break;
						case PlanetSideType.Left:
							ll = new Vector3(-offs, y0, -x0);
							ul = new Vector3(-offs, y1, -x0);
							ur = new Vector3(-offs, y1, -x1);
							lr = new Vector3(-offs, y0, -x1);
						break;
						case PlanetSideType.Bottom:
							ll = new Vector3(-x0, -offs , y0);
							ul = new Vector3(-x0, -offs , y1);
							ur = new Vector3(-x1, -offs , y1);
							lr = new Vector3(-x1, -offs , y0);
						break;
					}

					GameObject go = new GameObject("Node - {0} - {1} - LOD{2}".FormatWith(x, y, 0));
					go.layer = this.Go.layer;
					go.tag = this.Go.tag;
					go.transform.parent = this.Trs;
					go.transform.localPosition = Vector3.zero;
					go.transform.localRotation = Quaternion.identity;
					go.transform.localScale = Vector3.one;
					go.SetActive(false);
					PlanetTreeNode node = go.AddComponent<PlanetTreeNode>();
					go.SetActive(true);
					node.Initialize(this, null, 0, x, y, ll, ul, ur, lr);
					Nodes.Add(node);
				}
			}
/*
			LLLatLon = Nodes[0].LLLatLon;
			ULLatLon = Nodes[(Nodes.Count-1) - (_LOD._Subdivisions-1)].ULLatLon;
			URLatLon = Nodes[Nodes.Count-1].URLatLon;
			LRLatLon = Nodes[_LOD._Subdivisions-1].LRLatLon;*/
		}

		internal void DeleteNodes() {
			if(Nodes != null) {
				if(Nodes.Count > 0) {
					for(int i = 0; i < Nodes.Count; i++) {
						if(Nodes[i] != null) {
							Nodes[i].DeleteChildren();
						}
					}
					Nodes.Clear();
				}
				Nodes = null;
			}
		}
#endregion Helper Methods

#region Update Methods
		internal void UpdateTree() {
			if(Nodes != null) {
				if(Nodes.Count > 0) {
					for(int i = 0; i < Nodes.Count; i++) {
						if(Nodes[i] != null) {
							Nodes[i].UpdateNode();
						}
					}
				}
			}
		}
#endregion Update Methods
	}

}