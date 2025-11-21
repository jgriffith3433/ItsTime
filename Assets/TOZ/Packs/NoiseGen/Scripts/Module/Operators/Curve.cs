using System;
using System.Collections.Generic;
using TOZ;
using TOZ.Noise;

namespace TOZ.Noise.Module {

	public struct ControlPoint {
		public double In;
		public double Out;

		public ControlPoint(double i, double o) {
			In = i;
			Out = o;
		}
	}

	public class Curve : IModule {
		//Variables
		public List<ControlPoint> Points;

		//Properties
		public IModule Module0 { get; set; }

		//Constructor
		public Curve() {
			Points = new List<ControlPoint>();
		}

		public Curve(IModule mod0) {
			Points = new List<ControlPoint>();
			Module0 = mod0;
		}

		//Methods
		public double GetValue(double x, double y, double z) {
			double v = Module0.GetValue(x, y, z);

			//Find the first element which has an input value larger than output value
			int i;
			for(i = 0; i < Points.Count; i++) {
				if(v < Points[i].In) {
					break;
				}
			}
			//Find four nearest control points to do cubic interpolation
			int index0 = _MathUtil._Clamp(i-2, 0, Points.Count-1);
			int index1 = _MathUtil._Clamp(i-1, 0, Points.Count-1);
			int index2 = _MathUtil._Clamp(i, 0, Points.Count-1);
			int index3 = _MathUtil._Clamp(i+1, 0, Points.Count-1);
			//If some points are missing, get nearest points value and exit now
			if(index1 == index2) {
				return Points[index1].Out;
			}
			//Compute rate value for cubic interpolation
			double rate = (v - Points[index1].In) / (Points[index2].In - Points[index1].In);
			//Now perform cubic interpolation with the rate
			return _InterpolationUtil._Cubic(
											Points[index0].Out,
											Points[index1].Out,
											Points[index2].Out,
											Points[index3].Out,
											rate
			);
			//Already clamped
		}
	}

}