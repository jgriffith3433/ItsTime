using System;
using TOZ;
using TOZ.Noise;

namespace TOZ.Noise.Model {

	public sealed class Box {
		//Properties
		public IModule Module { get; set; }

		//Constructor
		public Box(IModule module) {
			Module = module;
		}

		//Methods
		public double GetValue(double x, double y, double z) {
			return Module.GetValue(x, y, z);
		}
	}

}