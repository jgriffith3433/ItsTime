using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TOZ {

	public static class ColorEx {
		//Get grayscale value of Color32
		public static float Grayscale(this Color32 col) {
			return ((Color)col).grayscale;
		}

		//Minimum component
		public static float MinComponent(this Color col) {
			return Mathf.Min(Mathf.Min(col.r, col.g), col.b);
		}

		//Invert the colors rgb
		public static void Invert(this Color col) {
			col = new Color(1.0f - col.r, 1.0f - col.g, 1.0f - col.b, col.a);
		}
	}

}