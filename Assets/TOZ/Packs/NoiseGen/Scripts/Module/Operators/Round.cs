using System;
using TOZ;
using TOZ.Noise;

namespace TOZ.Noise.Module {

	public class Round : IModule {
		//Properties
		public IModule Module0 { get; set; }

		//Construct
		public Round() { }

		public Round(IModule mod0) {
			Module0 = mod0;
		}

		//Methods
		public double GetValue(double x, double y, double z) {
			double v = Math.Round(Module0.GetValue(x, y, z));
			return v;
		}
	}

}