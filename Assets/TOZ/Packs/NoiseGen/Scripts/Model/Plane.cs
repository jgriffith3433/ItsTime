using System;
using TOZ;
using TOZ.Noise;

namespace TOZ.Noise.Model {

	public sealed class Plane {
		//Properties
		public IModule Module { get; set; }

		//Constructor
		public Plane(IModule module) {
			Module = module;
		}

		//Methods
		public double GetValue(double x, double y) {
			return Module.GetValue(x, y, 0);
		}
	}

}