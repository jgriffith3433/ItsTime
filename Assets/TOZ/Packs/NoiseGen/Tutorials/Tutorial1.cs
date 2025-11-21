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

	public class Tutorial1 : MonoBehaviour {
		Texture2D tex;

		// Use this for initialization
		void Start () {
			//Create the module
			IModule perlin = new Perlin();

			//Build the noisemap with the module
			NoiseMapBuilderPlane builder = new NoiseMapBuilderPlane(256, 256);
			builder.Build(perlin);

			//Create the renderer and setup the source noise map
			NoiseMapRenderer renderer = new NoiseMapRenderer();
			renderer.SourceNoiseMap = builder.Map;
			
			//Create the gradient for the renderer to use
			GradientColor gradient = new GradientColor();
			gradient.BuildGrayscaleGradient();
			renderer.SourceGradientColor = gradient;
			
			//Render and get the texture
			renderer.RenderImage();
			tex = renderer.GetTexture2D();
		}

		void OnGUI () {
			if(tex != null)
				GUI.DrawTexture(new Rect(0, 0, tex.width, tex.height), tex, ScaleMode.ScaleToFit, true);
		}
	}

}