using System;
using TOZ;
using TOZ.Noise;

namespace TOZ.Noise.Module {

	public class Displace : IModule {
		//Properties
		public IModule Module0 { get; set; }
		public IModule X { get; set; }
		public IModule Y { get; set; }
		public IModule Z { get; set; }
		public bool Clamped { get; set; }

		//Constructors
		public Displace() { }

		public Displace(IModule mod0, IModule x, IModule y, IModule z) {
			Module0 = mod0;
			X = x;
			Y = y;
			Z = z;
			Clamped = true;
		}

		//Methods
		public double GetValue(double x, double y, double z) {
			double v1 = x + X.GetValue(x, y, z);
			double v2 = y + Y.GetValue(x, y, z);
			double v3 = z + Z.GetValue(x, y, z);
			double v = Module0.GetValue(v1, v2, v3);

			if(Clamped)
				v = _MathUtil._Clamp(v, -1.0, 1.0);
			return v;
		}
	}

}