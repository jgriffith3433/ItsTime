using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TOZ;

namespace TOZ.Celestial {

	public interface IReticleObject {
		Vector3 ScrPos { get; }
		float ScrSize { get; }
	}

}