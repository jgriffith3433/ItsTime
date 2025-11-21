using System;
using TOZ;
using TOZ.Noise;

namespace TOZ.Noise.Module {

	public class Clamp : IModule {
		//Properties
		public IModule Module0 { get; set; }
		public double LowerBound { get; set; }
		public double UpperBound { get; set; }

		//Constructors
		public Clamp() { }

		public Clamp(IModule mod0) : this(mod0, -1.0, 1.0) {
		}

		public Clamp(IModule mod0, double lb, double ub) {
			Module0 = mod0;
			LowerBound = lb;
			UpperBound = ub;
		}

		//Methods
		public double GetValue(double x, double y, double z) {
			return _MathUtil._Clamp(Module0.GetValue(x, y, z), LowerBound, UpperBound);
			//Already clamped
		}
	}

}