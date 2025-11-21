using System;
using TOZ;
using TOZ.Noise;

namespace TOZ.Noise.Module {

	public sealed class Abs : IModule {
		//Properties
		public IModule Module0 { get; set; }

		//Constructors
		public Abs() { }

		public Abs(IModule mod0) {
			Module0 = mod0;
		}

		//Methods
		public double GetValue(double x, double y, double z) {
			double v = Math.Abs(Module0.GetValue(x, y, z));
			return v;
		}
	}

}