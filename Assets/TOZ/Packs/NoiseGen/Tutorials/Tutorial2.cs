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

	public class Tutorial2 : MonoBehaviour {
		Texture2D tex;

		// Use this for initialization
		void Start () {
			IModule mountainTerrain = new RidgedMulti();
			IModule baseFlatTerrain = new Billow();
			((Billow)baseFlatTerrain).Frequency = 2.0;
			IModule flatTerrain = new ScaleBias(baseFlatTerrain, 0.125, -0.75);
			IModule terrainType = new Perlin();
			((Perlin)terrainType).Frequency = 0.5;
			((Perlin)terrainType).Persistence = 0.25;
			IModule finalTerrain = new Select(flatTerrain, mountainTerrain, terrainType);
			((Select)finalTerrain).SetBounds(0.0, 1.0);
			((Select)finalTerrain).SetEdgeFallOff(0.125);
			
			NoiseMapBuilderPlane builder = new NoiseMapBuilderPlane(512, 512);
			builder.SetBounds(6.0, 10.0, 1.0, 5.0);
			builder.Build(finalTerrain);
			NoiseMapRenderer renderer = new NoiseMapRenderer();
			renderer.SourceNoiseMap = builder.Map;
			
			GradientColor gradient = new GradientColor();
			gradient.Add(-1.0000, new Color32(32, 160, 0, 255));
			gradient.Add(-0.2500, new Color32(224, 224, 0, 255));
			gradient.Add(0.2500, new Color32(128, 128, 128, 255));
			gradient.Add(1.0000, new Color32(255, 255, 255, 255));
			renderer.SourceGradientColor = gradient;
			renderer.IsLightEnabled = true;
			renderer.LightContrast = 3.0;
			renderer.LightBrightness = 2.0;
			renderer.RenderImage();

			tex = renderer.GetTexture2D();
		}

		void OnGUI () {
			if(tex != null)
				GUI.DrawTexture(new Rect(0, 0, tex.width, tex.height), tex, ScaleMode.ScaleToFit, true);
		}
	}

}