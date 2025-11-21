using System;
using UnityEngine;
using TOZ;
using TOZ.Noise;

namespace TOZ.Noise.Module {

	public class Test : IModule {
		//Properties//
		public double Start { get; set; }
		public double End { get; set; }
		public double LowerXBound { get; set; }
		public double UpperXBound { get; set; }
		public double LowerYBound { get; set; }
		public double UpperYBound { get; set; }
		public bool Invert { get; set; }

		//Constructors//
		public Test() : this(-1.0, 1.0, -1.0, 1.0) { }

		public Test(double lowerXBound, double upperXBound, double lowerYBound, double upperYBound) {
			Start = -1.0;
			End = 1.0;
			LowerXBound = lowerXBound;
			UpperXBound = upperXBound;
			LowerYBound = lowerYBound;
			UpperYBound = upperYBound;
			Invert = false;
		}

		//Methods//
		public double GetValue(double x, double y, double z) {
			double distX = (UpperXBound - LowerXBound);
			double rateX = 1.0 / distX * (x - LowerXBound);
			//Debug.Log(x + " " + y + " " + z + "    " + rateX);
			double ix = _InterpolationUtil._Linear(Start, End, rateX);
			//Debug.Log(x + "       " + ix);
			double distY = (UpperYBound - LowerYBound);
			double rateY = 1.0 / distY * (y - LowerYBound);
			double iy = _InterpolationUtil._Linear(Start, End, rateY);

			double val = 0.0;
			double centerX = (UpperXBound + LowerXBound) * 0.5;
			double centerY = (UpperYBound + LowerYBound) * 0.5;
			if(Invert)
				return _InterpolationUtil._Grade(LowerXBound, UpperXBound, End, Start, x);
			else {
				//val = ix;
				val = _InterpolationUtil._Grade(Start, End, centerX, UpperXBound, x);
				val *= _InterpolationUtil._Grade(End, Start, LowerXBound, centerX, x);
				//val *= 2.0;
				val *= _InterpolationUtil._Grade(Start, End, centerY, UpperYBound, y);
				val *= _InterpolationUtil._Grade(End, Start, LowerYBound, centerY, y);
			}
			return ix;
			//Already clamped
		}
	}

}