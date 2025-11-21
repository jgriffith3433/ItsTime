using System;
using TOZ;
using TOZ.Noise;

namespace TOZ.Noise.Module {

	public class Log : IModule {
		//Properties
		public IModule Module0 { get; set; }
		public double Value { get; set; }
		public bool Clamped { get; set; }

		//Constructors
		public Log() { }

		public Log(IModule mod0) : this(mod0, 1.0) {
		}

		public Log(IModule mod0, double v) {
			Module0 = mod0;
			Value = v;
			Clamped = true;
		}

		//Methods
		public double GetValue(double x, double y, double z) {
			double v = Math.Log(Module0.GetValue(x, y, z), Value);

			if(Clamped)
				v = _MathUtil._Clamp(v, -1.0, 1.0);
			return v;
		}
	}

}