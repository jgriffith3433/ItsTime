using System;
using TOZ;
using TOZ.Noise;

namespace TOZ.Noise.Module {

	public class ColorDodge : IModule {
		//Properties
		public IModule Module0 { get; set; }
		public IModule Module1 { get; set; }

		//Constructors
		public ColorDodge() { }

		public ColorDodge(IModule mod0, IModule mod1) {
			Module0 = mod0;
			Module1 = mod1;
		}

		//Methods
		public double GetValue(double x, double y, double z) {
			double v1 = Module0.GetValue(x, y, z) * 0.5 + 0.5;
			double v2 = Module1.GetValue(x, y, z) * 0.5 + 0.5;
			double v = (v1 / (1.0 - v2)) * 2.0 - 1.0;
			return v;
		}
	}

}