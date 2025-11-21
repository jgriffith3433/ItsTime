using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TOZ {

	public sealed class DataContainer<T> where T : class, IIndexed, IComparable<T> {
#region Variables
		//Variables
		public List<T> Data;
#endregion Variables

#region Constructors
		//Constructors
		public DataContainer() {
			Clear();
		}
#endregion Constructors

#region Indexer and Converters
		//Indexer
		public T this[int i] {
			get { return Data[i]; }
			set { Data[i] = value; }
		}

		public T[] Array() {
			return Data.ToArray();
		}
#endregion Indexer and Converters

#region List Methods
		//List Methods
		public void Clear() {
			if(Data != null) {
				Data.Clear();
			}
			else {
				Data = new List<T>();
			}
		}

		public int Count() {
			return Data.Count;
		}

		public void Add(T i) {
			if(Data != null && !Contains(i)) {
				Data.Add(i);
				Sort();
			}
		}

		public void Insert(int index, T i) {
			if(Data != null && !Contains(i)) {
				Data.Insert(index, i);
				Sort();
			}
		}

		public void Remove(T i) {
			if(Data != null && Contains(i)) {
				Data.Remove(i);
			}
		}

		public void RemoveAt(int index) {
			if(Data != null) {
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

		public void Sort() {
			if(Data != null) {
				Data.Sort();
			}
		}
#endregion List Methods
	}

}