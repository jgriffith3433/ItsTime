using System;
using TOZ;
using TOZ.Noise;

namespace TOZ.Noise.Module {

	public class Invert : IModule {
		//Properties
		public IModule Module0 { get; set; }

		//Constructors
		public Invert() { }

		public Invert(IModule mod0) {
			Module0 = mod0;
		}

		//Methods
		public double GetValue(double x, double y, double z) {
			double v = -(Module0.GetValue(x, y, z));
			return v;
		}
	}

}