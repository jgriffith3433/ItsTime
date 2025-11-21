using System;
using System.Collections.Generic;
using TOZ;
using TOZ.Noise;

namespace TOZ.Noise.Module {

	public class Terrace : IModule {
		//Variables
		public List<double> Points;

		//Properties
		public IModule Module0 { get; set; }
		public double Thickness { get; set; }
		public bool Invert { get; set; }

		//Constructor
		public Terrace() {
			Points = new List<double>();
		}

		public Terrace(IModule mod0) {
			Points = new List<double>();
			Module0 = mod0;
			Thickness = 2.0;
			Invert = false;
		}

		//Methods
		public double GetValue(double x, double y, double z) {
			double v = Module0.GetValue(x, y, z);
			//Find the first element which has an input value larger than output value
			int i;
			for(i = 0; i < Points.Count; i++) {
				if(v < Points[i]) {
					break;
				}
			}
			//Find two nearest control points to do quadratic interpolation
			int index0 = _MathUtil._Clamp(i-1, 0, Points.Count-1);
			int index1 = _MathUtil._Clamp(i, 0, Points.Count-1);
			//If some points are missing, get nearest points value and exit now
			if (index0 == index1) {
				return Points[index1];
			}
			//Compute alpha value for cubic interpolation
			double value0 = Points[index0];
			double value1 = Points[index1];
			double alpha = (v - value0) / (value1 - value0);
			if(Invert) {
				alpha = 1.0 - alpha;
				_MathUtil._Swap(ref value0, ref value1);
			}
			//Squaring alpha produces terrace effect.
			//alpha *= alpha;
			alpha = Math.Pow(alpha, Thickness);
			//Now perform linear interpolation with the alpha
			return _InterpolationUtil._Linear(value0, value1, alpha);
			//Already clamped
		}
	}

}