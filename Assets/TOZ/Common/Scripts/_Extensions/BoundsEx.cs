using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TOZ {

	public static class BoundsEx {
		public static bool Contains(this Bounds self, Bounds other) {
			return self.Contains(other.min) && self.Contains(other.max);
		}
	}

}