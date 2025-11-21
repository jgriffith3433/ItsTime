using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TOZ;

namespace TOZ.Celestial {

	internal static class _LOD {
#region Variables
		//Constants
		internal const int _Max = 6;
		internal const int _Segments = 2;
		internal const int _Subdivisions = 2;
		internal const float _Angle = 65f;

		//Variables
		internal static int _Divisions;
		internal static int _Quads;

		private static int _simultaneousDivisions;
		private static List<PlanetTreeNode> _waitingQueue;
		private static List<PlanetTreeNode> _creationQueue;

		//Constructor
		static _LOD() {
			_Initialize();
		}
#endregion Variables

#region Methods
		internal static void _Setup(int divisions) {
			_Initialize();

			_Divisions = (int)Mathf.Pow(_Subdivisions, _Max+1);
			_Quads = _Divisions * _Segments;

			_simultaneousDivisions = divisions;
		}

		internal static void _Initialize() {
			_Divisions = 64;
			_Quads = _Divisions * _Segments;

			_simultaneousDivisions = 1;

			if(_waitingQueue != null) {
				_waitingQueue.Clear();
			}
			else {
				_waitingQueue = new List<PlanetTreeNode>();
			}

			if(_creationQueue != null) {
				_creationQueue.Clear();
			}
			else {
				_creationQueue = new List<PlanetTreeNode>();
			}
		}

		internal static void _Update() {
			_waitingQueue.RemoveAll(item => item == null);
			_creationQueue.RemoveAll(item => item == null);

			for(int i = 0; i < _simultaneousDivisions; i++) {
				if(_waitingQueue.Count > 0 && _creationQueue.Count < _simultaneousDivisions) {
					_creationQueue.Add(_waitingQueue[0]);
					_waitingQueue.RemoveAt(0);
					_creationQueue[0].CreateChildren();
					_creationQueue[0].IsQueued = false;
					_creationQueue.RemoveAt(0);
				}
				else
					break;
			}
		}

		internal static void _Enqueue(PlanetTreeNode node) {
			if(!_waitingQueue.Contains(node) && !_creationQueue.Contains(node)) {
				_waitingQueue.Add(node);
				_waitingQueue.Sort();
				node.IsQueued = true;
			}
		}

		internal static void _Dequeue(PlanetTreeNode node) {
			if(_waitingQueue.Contains(node) && !_creationQueue.Contains(node)) {
				_waitingQueue.Remove(node);
				node.IsQueued = false;
			}
		}
#endregion Methods
	}

}