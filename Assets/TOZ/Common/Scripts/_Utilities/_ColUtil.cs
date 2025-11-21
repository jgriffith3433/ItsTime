using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TOZ {

	public static class _ColUtil {
		//Invert the colors rgb
		public static Color Invert(Color col) {
			return new Color(1.0f - col.r, 1.0f - col.g, 1.0f - col.b, col.a);
		}
	}

}