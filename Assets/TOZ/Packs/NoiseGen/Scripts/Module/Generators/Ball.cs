using System;
using TOZ;
using TOZ.Noise;

namespace TOZ.Noise.Module {

	public class Ball : IModule {
		//Properties
		public double Frequency { get; set; }
		public bool Invert { get; set; }

		//Constructors
		public Ball() {
			Frequency = 1.0;
			Invert = false;
		}

		//Methods
		public double GetValue(double x, double y, double z) {
			x *= Frequency;
			y *= Frequency;
			z *= Frequency;
			double distFromCenter = _MathUtil._Clamp(Math.Sqrt(x * x + y * y + z * z), 0.0, 1.0);
			distFromCenter = (1.0 - distFromCenter) * 2.0 - 1.0;
			
			if(Invert) {
				return -distFromCenter;
			}
			else {
				return distFromCenter;
			}
			//Already clamped
		}
	}

}