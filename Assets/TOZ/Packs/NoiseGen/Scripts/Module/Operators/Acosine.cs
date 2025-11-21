using System;
using TOZ;
using TOZ.Noise;

namespace TOZ.Noise.Module {

	public sealed class Acosine : IModule {
		//Properties
		public IModule Module0 { get; set; }
		public bool Invert { get; set; }

		//Constructors
		public Acosine() { }

		public Acosine(IModule mod0) {
			Module0 = mod0;
			Invert = false;
		}

		//Methods
		public double GetValue(double x, double y, double z) {
			double v = Math.Acos(-Module0.GetValue(x, y, z)) / _MathUtil._PI;

			if(Invert) {
				return v - 1.0;
			}
			else {
				return v;
			}
		}
	}

}