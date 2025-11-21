using System;
using TOZ;
using TOZ.Noise;

namespace TOZ.Noise.Model {

	public sealed class Line {
		//Properties
		public IModule Module { get; set; }
		public bool Attenuate { get; set; }
		public double X0 { get; set; }
		public double Y0 { get; set; }
		public double Z0 { get; set; }
		public double X1 { get; set; }
		public double Y1 { get; set; }
		public double Z1 { get; set; }

		//Constructor
		public Line(IModule module) {
			Module = module;
			SetStartPoint(-1.0, 0.0, 0.0);
			SetEndPoint(1.0, 0.0, 0.0);
			Attenuate = true;
		}

		//Methods
		public double GetValue(double t) {
			double x = X0 + (X1 - X0) * t;
			double y = Y0 + (Y1 - Y0) * t;
			double z = Z0 + (Z1 - Z0) * t;
			double v = Module.GetValue(x, y, z);

			if(Attenuate) {
				return t * (1.0 - t) * 4.0 * v;
			}
			else {
				return v;
			}
		}

		public void SetStartPoint(double x, double y, double z) {
			X0 = x; Y0 = y; Z0 = z;
		}

		public void SetEndPoint(double x, double y, double z) {
			X1 = x; Y1 = y; Z1 = z;
		}
	}

}