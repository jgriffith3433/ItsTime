using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TOZ {

	public abstract class ScriptableObjectBase<T> : ScriptableObject {
#region Variables
		//Variables
		public List<T> Data;
#endregion Variables

#region Mono Methods
		//Mono Methods
		public virtual void Awake() {
			if(Data == null) {
				Data = new List<T>();
			}
		}
#endregion Mono Methods

#region Indexer and Converters
		//Indexer
		public T this[int i] {
			get { return Data[i]; }
			set { Data[i] = value; }
		}

		//Converter
		public T[] Array() {
			return Data.ToArray();
		}
#endregion Indexer and Converters

#region List Methods
		public void Clear() {
			Data.Clear();
		}

		public int Count() {
			return Data.Count;
		}

		public void Add() {
			Add(default(T));
		}

		public void Add(T i) {
			if(Data != null) {
				Data.Add(i);
			}
		}

		public void Insert(int index) {
			Insert(index, default(T));
		}

		public void Insert(int index, T i) {
			if(Data != null) {
				Data.Insert(index, i);
			}
		}

		public void Remove(T i) {
			if(Data != null && Contains(i)) {
				Data.Remove(i);
			}
		}

		public void RemoveAt(int index) {
			if(Data != null && index < Count()) {
				Data.RemoveAt(index);
			}
		}

		public int IndexOf(T i) {
			return Data.IndexOf(i);
		}

		public bool Contains(T i) {
			return Data.Contains(i);
		}

		public void Reverse() {
			if(Data != null) {
				Data.Reverse();
			}
		}
#endregion List Methods
	}

}