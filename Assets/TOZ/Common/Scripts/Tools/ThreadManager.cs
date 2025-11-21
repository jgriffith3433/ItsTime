using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace TOZ {

	public sealed class ThreadManager : MonoBehaviour {
#region Instance
		private static bool _initialized = false;
		private static ThreadManager _instance;
		public static ThreadManager _Instance {
			get {
				Initialize();
				return _instance;
			}
		}

		private static void Initialize() {
			if(!_initialized) {
				if(!Application.isPlaying)
					return;

				_initialized = true;
				GameObject go = new GameObject("ThreadManager");
				go.layer = 2;
				go.transform.localPosition = Vector3.zero;
				go.transform.localRotation = Quaternion.identity;
				go.transform.localScale = Vector3.one;
				_instance = go.AddComponent<ThreadManager>();
			}
		}
#endregion Instance

#region Variables
		public static int _ThreadsTotal = 8;
		private static int _threadCounter;
		private List<Action> actionsList = new List<Action>();
		private List<Action> currentActions = new List<Action>();
#endregion Variables

#region Mono Methods
		private void OnEnable() {
			DoEnable();
		}

		private void OnDisable() {
			DoDisable();
		}

		private void Update() {
			DoUpdate();
		}
#endregion Mono Methods

#region Methods
		private void DoEnable() {
			_initialized = true;
			_instance = this;
		}

		private void DoDisable() {
			if(_instance == this) {
				_instance = null;
			}
		}

		private void DoUpdate() {
			lock(actionsList) {
				currentActions.Clear();
				currentActions.AddRange(actionsList);
				actionsList.Clear();
			}

			for(int i = 0; i < currentActions.Count; i++) {
				currentActions[i]();
			}
		}
#endregion Methods

#region Helper Methods
		public static void RunOnMainThread(Action action) {
			lock(_Instance.actionsList) {
				_Instance.actionsList.Add(action);
			}
		}

		public static Thread RunOnThread(Action action) {
			Initialize();
			while(_threadCounter >= _ThreadsTotal) {
				Thread.Sleep(1);
			}
			Interlocked.Increment(ref _threadCounter);
			ThreadPool.QueueUserWorkItem(RunAction, action);

			return null;
		}

		private static void RunAction(object action) {
			try {
				((Action)action)();
			}
			catch {}
			finally {
				Interlocked.Decrement(ref _threadCounter);
			}
		}
#endregion Helper Methods
	}

}