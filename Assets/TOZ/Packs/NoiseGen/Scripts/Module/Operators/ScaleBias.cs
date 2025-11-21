using System;
using TOZ;
using TOZ.Noise;

namespace TOZ.Noise.Module {

	public class ScaleBias : IModule {
		//Properties
		public IModule Module0 { get; set; }
		public double Scale { get; set; }
		public double Bias { get; set; }
		public bool Clamped { get; set; }

		//Construct
		public ScaleBias() { }

		public ScaleBias(IModule mod0) : this(mod0, 1.0, 0.0) {
		}

		public ScaleBias(IModule mod0, double scale, double bias) {
			Module0 = mod0;
			Scale = scale;
			Bias = bias;
			Clamped = true;
		}

		//Methods
		public double GetValue(double x, double y, double z) {
			double v = Module0.GetValue(x, y, z) * Scale + Bias;

			if(Clamped)
				v = _MathUtil._Clamp(v, -1.0, 1.0);
			return v;
		}
	}

}