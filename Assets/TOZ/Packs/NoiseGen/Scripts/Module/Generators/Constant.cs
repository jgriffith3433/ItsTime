using System;
using TOZ;
using TOZ.Noise;

namespace TOZ.Noise.Module {

	public class Constant : IModule {
		//Properties
		public double Value { get; set; }

		//Constructors
		public Constant() {
			Value = 0.0;
		}

		//Methods
		public double GetValue(double x, double y, double z) {
			double v = Value;
			return v;
		}
	}

}