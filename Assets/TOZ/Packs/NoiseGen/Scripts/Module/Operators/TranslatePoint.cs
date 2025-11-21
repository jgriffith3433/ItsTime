using System;
using TOZ;
using TOZ.Noise;

namespace TOZ.Noise.Module {

	public class TranslatePoint : IModule {
		//Properties
		public IModule Module0 { get; set; }
		public double X { get; set; }
		public double Y { get; set; }
		public double Z { get; set; }
		public bool Clamped { get; set; }

		//Constructors
		public TranslatePoint() { }

		public TranslatePoint(IModule mod0) : this(mod0, 0.0, 0.0, 0.0) {
		}

		public TranslatePoint(IModule mod0, double x, double y, double z) {
			Module0 = mod0;
			X = x;
			Y = y;
			Z = z;
			Clamped = true;
		}

		//Methods
		public double GetValue(double x, double y, double z) {
			double v = Module0.GetValue(x + X, y + Y, z + Z);
	
			if(Clamped)
				v = _MathUtil._Clamp(v, -1.0, 1.0);
			return v;
		}
	}

}