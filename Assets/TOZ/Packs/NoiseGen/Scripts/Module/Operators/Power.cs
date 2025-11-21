using System;
using TOZ;
using TOZ.Noise;

namespace TOZ.Noise.Module {

	public class Power : IModule {
		//Properties
		public IModule Module0 { get; set; }
		public IModule Module1 { get; set; }
		public bool Clamped { get; set; }

		//Constructors
		public Power() { }

		public Power(IModule mod0, IModule mod1) {
			Module0 = mod0;
			Module1 = mod1;
			Clamped = true;
		}

		//Methods
		public double GetValue(double x, double y, double z) {
			double v = Math.Pow(Module0.GetValue(x, y, z), Module1.GetValue(x, y, z));

			if(Clamped)
				v = _MathUtil._Clamp(v, -1.0, 1.0);
			return v;
		}
	}

}