using System;
using TOZ;
using TOZ.Noise;

namespace TOZ.Noise.Module {

	public sealed class BordersAverage : IModule {
		//Properties
		public IModule Module0 { get; set; }
		public double LowerBound { get; set; }
		public double UpperBound { get; set; }

		//Constructors
		public BordersAverage() { }

		public BordersAverage(IModule mod0) : this(mod0, -1.0, 1.0) {
		}

		public BordersAverage(IModule mod0, double lb, double ub) {
			Module0 = mod0;
			LowerBound = lb;
			UpperBound = ub;
		}

		//Methods
		public double GetValue(double x, double y, double z) {
			double v = Module0.GetValue(x, y, z);
			if(v >= LowerBound && v <= UpperBound) {
				v = (LowerBound + UpperBound) * 0.5;
			}
			return v;
		}
	}

}