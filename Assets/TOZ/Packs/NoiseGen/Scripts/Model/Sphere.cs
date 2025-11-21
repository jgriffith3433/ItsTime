using System;
using TOZ;
using TOZ.Noise;

namespace TOZ.Noise.Model {

	public sealed class Sphere {
		//Properties
		public IModule Module { get; set; }

		//Constructor
		public Sphere(IModule module) {
			Module = module;
		}

		//Methods
		public double GetValue(double lat, double lon) {
			double x = 0, y = 0, z = 0;
			_MathUtil._LatLonToXYZ(lat, lon, ref x, ref y, ref z);
			return Module.GetValue(x, y, z);
		}
	}

}