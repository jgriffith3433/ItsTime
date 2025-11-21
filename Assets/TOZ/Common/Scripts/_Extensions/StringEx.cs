using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TOZ {

	public static class StringEx {
		//Format a string
		public static string FormatWith(this string str, params object[] args) {
			return String.Format(str, args);
		}

		/// Get string value after [first] a.
		public static string Before(this string str, string a) {
			int posA = str.IndexOf(a);
			return (posA == -1) ? string.Empty : str.Substring(0, posA);
		}

		//Get string value between [first] a and [last] b.
		public static string Between(this string str, string a, string b) {
			int posA = str.IndexOf(a);
			int posB = str.LastIndexOf(b);
			if(posA == -1) {
				return string.Empty;
			}
			if(posB == -1) {
				return string.Empty;
			}
			int length = posA + a.Length;
			if(length >= posB) {
				return string.Empty;
			}
			return str.Substring(length, posB - length);
		}

		/// Get string value after [last] a.
		public static string After(this string str, string a) {
			int posA = str.LastIndexOf(a);
			if(posA == -1) {
				return string.Empty;
			}
			int length = posA + a.Length;
			if(length >= str.Length) {
				return string.Empty;
			}
			return str.Substring(length);
		}

		public static string Label(this string str, string color = "black") {
			return "<color="+color+"><b>"+str+"</b></color>";
		}

		public static string Text(this string str, string color = "white") {
			return "<color="+color+">"+str+"</color>";
		}
	}

}