using System;
using TOZ;
using TOZ.Noise;

namespace TOZ.Noise.Module {

	public class BarsV : IModule {
		//Properties
		public double LowerBound { get; set; }
		public double UpperBound { get; set; }
		public double Frequency { get; set; }

		//Constructors
		public BarsV() {
			LowerBound = -1.0;
			UpperBound = 1.0;
			Frequency = 1.0;
		}

		//Methods
		public double GetValue(double x, double y, double z) {
			double rate = _InterpolationUtil._Grade(-1.0, 1.0, LowerBound, UpperBound, x);
			return Math.Cos(rate * Frequency * _MathUtil._PI);
			//Already clamped
		}
	}

}