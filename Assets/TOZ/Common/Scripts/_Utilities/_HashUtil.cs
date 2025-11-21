/**********************************NOTES********************************************
USAGE:
public override int GetHashCode() {
	return Hash._Get(my int, my object, my float);
}
or
public override int GetHashCode() {
	return 0._Combine(my int)._CombineHash(my object)._CombineHash(my float);
}
**************************************************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TOZ {

	public static class _HashUtil {
		public static int _Get<T>(T arg) {
			unchecked {
				return arg.GetHashCode();
			}
		}

		public static int _Get<T1, T2>(T1 arg1, T2 arg2) {
			unchecked {
				return 31 * arg1.GetHashCode() + arg2.GetHashCode();
			}
		}

		public static int _Get<T1, T2, T3>(T1 arg1, T2 arg2, T3 arg3) {
			unchecked {
				int hash = arg1.GetHashCode();
				hash = 31 * hash + arg2.GetHashCode();
				return 31 * hash + arg3.GetHashCode();
			}
		}

		public static int _Get<T1, T2, T3, T4>(T1 arg1, T2 arg2, T3 arg3, T4 arg4) {
			unchecked {
				int hash = arg1.GetHashCode();
				hash = 31 * hash + arg2.GetHashCode();
				hash = 31 * hash + arg3.GetHashCode();
				return 31 * hash + arg4.GetHashCode();
			}
		}

		public static int _Get<T>(T[] list) {
			unchecked {
				int hash = 0;
				foreach(var item in list) {
					hash = 31 * hash + item.GetHashCode();
				}
				return hash;
			}
		}

		public static int _Get<T>(IEnumerable<T> list) {
			unchecked {
				int hash = 0;
				foreach(var item in list) {
					hash = 31 * hash + item.GetHashCode();
				}
				return hash;
			}
		}

		//Order doesnt matter, (1, 2, 3) and (3, 2, 1) has same hash code
		public static int _GetNonOrdered<T>(IEnumerable<T> list) {
			unchecked {
				int hash = 0;
				int count = 0;
				foreach(var item in list) {
					hash += item.GetHashCode();
					count++;
				}
				return 31 * hash + count.GetHashCode();
			}
		}

		//return 0.CombineHashCode(field1).CombineHashCode(field2).CombineHashCode(field3);
		public static int _Combine<T>(this int hash, T arg) {
			unchecked {
				return 31 * hash + arg.GetHashCode();
			}
		}
	}

}