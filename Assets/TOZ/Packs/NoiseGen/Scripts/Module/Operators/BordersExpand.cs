using System;
using TOZ;
using TOZ.Noise;

namespace TOZ.Noise.Module {

	public sealed class BordersExpand : IModule {
		//Properties
		public IModule Module0 { get; set; }
		public double LowerBound { get; set; }
		public double Center { get; set; }
		public double UpperBound { get; set; }

		//Constructors
		public BordersExpand() { }

		public BordersExpand(IModule mod0) : this(mod0, -1.0, 0.0, 1.0) {
		}

		public BordersExpand(IModule mod0, double lb, double c, double ub) {
			Module0 = mod0;
			LowerBound = lb;
			Center = c;
			UpperBound = ub;
		}

		//Methods
		public double GetValue(double x, double y, double z) {
			double v = Module0.GetValue(x, y, z);
			if(v <= Center) {
				v = LowerBound;
			}
			else {
				v = UpperBound;
			}
			return v;
		}
	}

}