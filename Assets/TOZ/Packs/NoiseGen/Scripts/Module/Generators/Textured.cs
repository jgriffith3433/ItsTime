using System;
using UnityEngine;
using TOZ;
using TOZ.Noise;

namespace TOZ.Noise.Module {

	public class Textured : IModule {
		//Properties
		public double LowerXBound { get; set; }
		public double LowerYBound { get; set; }
		public double UpperXBound { get; set; }
		public double UpperYBound { get; set; }
		public Texture2D Tex { get; set; }

		//Constructors
		public Textured() : this(-1.0, -1.0, 1.0, 1.0, Texture2D.blackTexture) {
		}

		public Textured(double lxb, double lyb, double uxb, double uyb, Texture2D tex) {
			LowerXBound = lxb;
			LowerYBound = lyb;
			UpperXBound = uxb;
			UpperYBound = uyb;
			Tex = tex;
		}

		//Methods//
		public double GetValue(double x, double y, double z) {
			double distX = (UpperXBound - LowerXBound);
			double rateX = 1.0 / distX * (x - LowerXBound);
			double ix = _InterpolationUtil._Linear(0.0, 1.0, rateX);
			double distY = (UpperYBound - LowerYBound);
			double rateY = 1.0 / distY * (y - LowerYBound);
			double iy = _InterpolationUtil._Linear(0.0, 1.0, rateY);
			return Tex.GetPixelBilinear((float)ix, (float)iy).grayscale * 2.0f - 1.0f;
			//Already clamped
		}
	}

}