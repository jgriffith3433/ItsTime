using System;
using TOZ;
using TOZ.Noise;

namespace TOZ.Noise.Module {

	public class Dots : IModule {
		//Properties
		public double Frequency { get; set; }

		//Constructors
		public Dots() {
			Frequency = 1.0;
		}

		//Methods
		public double GetValue(double x, double y, double z) {
			x *= Frequency;
			y *= Frequency;
			return Math.Cos(x * _MathUtil._PI) * Math.Sin(y * _MathUtil._PI);
			//Already clamped
		}
	}

}