using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TOZ {

	public static class RectEx {
		//Rect contains other rect?
		public static bool Contains(this Rect self, Rect other) {
			return self.Contains(other.min, true) && self.Contains(other.max, true);
		}

		//Enlarge rect
		public static void Enlarge(this Rect self, float xSize, float ySize) {
			self.x -= xSize;
			self.y -= ySize;
			self.width += (xSize * 2f);
			self.height += (ySize * 2f);
		}

		//Position rect on screen
		public static void Position(this Rect self, float x, float y) {
			self.center = new Vector2(x, Screen.height - y);
		}
	}

}