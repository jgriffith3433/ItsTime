using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TOZ {

	public static class EnumEx {
		//Mask equals to enumFlag?
		public static bool Is<T>(this Enum mask, T enumFlag) {
			return ((int)(object)mask) == ((int)(object)enumFlag);
		}

		//Mask is not equals to enumFlag?
		public static bool IsNot<T>(this Enum mask, T enumFlag) {
			return !(((int)(object)mask) == ((int)(object)enumFlag));
		}

		//Mask has enumFlag?
		public static bool Has<T>(this Enum mask, T enumFlag) {
			return (((int)(object)mask) & ((int)(object)enumFlag)) == ((int)(object)enumFlag);
		}

		//Mask has not enumFlag?
		public static bool HasNot<T>(this Enum mask, T enumFlag) {
			return !((((int)(object)mask) & ((int)(object)enumFlag)) == ((int)(object)enumFlag));
		}

		//Add an enumFlag to mask
		public static T Add<T>(this Enum mask, T enumFlag) {
			return (T)(object)(((int)(object)mask) | ((int)(object)enumFlag));
		}

		//Remove an enumFlag from mask
		public static T Remove<T>(this Enum mask, T enumFlag) {
			return (T)(object)(((int)(object)mask) & ~((int)(object)enumFlag));
		}

		//Toggle an enumFlag in mask
		public static T Toggle<T>(this Enum mask, T enumFlag) {
			return (T)(object)(((int)(object)mask) ^ ((int)(object)enumFlag));
		}

		//Mask is not empty?
		public static bool IsNotEmpty<T>(this Enum mask, T enumFlag) {
			return (((int)(object)mask) & ((int)(object)enumFlag)) != 0;
		}

		//EnumFlag Count
		public static int Count<T>(this Enum mask) {
			return (Enum.GetNames(typeof(T)).Length);
		}
	}

}