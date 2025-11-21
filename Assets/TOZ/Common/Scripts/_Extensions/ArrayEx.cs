using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TOZ {

	public static class ArrayEx {
		//Array has item?
		public static bool HasItem<T>(this T[] array, T item) {
			return Array.Exists(array, element => element.Equals(item));
		}

		//Array has item?
		public static bool HasItem<T>(this T[] array, Predicate<T> match) {
			return Array.Exists(array, match);
		}

		//Apply action for each element
		public static void ForEach<T>(this T[] array, Action<T> action) {
			Array.ForEach(array, action);
		}
	}

}