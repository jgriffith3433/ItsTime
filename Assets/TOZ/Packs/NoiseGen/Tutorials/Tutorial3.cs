using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TOZ;
using TOZ.Noise;
using TOZ.Noise.Utilities;
using TOZ.Noise.Model;
using TOZ.Noise.Module;

namespace TOZ.Noise.Tutorials {

	public class Tutorial3 : MonoBehaviour {
		float heightScale = 1.0f;
		float yScale = 0.5f;

		void Update() {
			IModule perlin = new Perlin();
			double yPos = (double)(Time.time * yScale);
			float height = heightScale * (float)perlin.GetValue(0.0, yPos, 0.0);
			Vector3 pos = transform.position;
			pos.y = height;
			transform.position = pos;
		}
	}

}