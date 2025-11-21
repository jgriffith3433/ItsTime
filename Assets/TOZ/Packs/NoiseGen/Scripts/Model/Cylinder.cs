using System;
using TOZ;
using TOZ.Noise;

namespace TOZ.Noise.Model {

	public sealed class Cylinder {
		//Properties
		public IModule Module { get; set; }

		//Constructor
		public Cylinder(IModule module) {
			Module = module;
		}

		//Methods
		public double GetValue(double angle, double height) {
			double x = 0, y = 0, z = 0;
			x = Math.Sin(angle * _MathUtil._DEG2RAD);
			y = height;
			z = -Math.Cos(angle * _MathUtil._DEG2RAD);
			return Module.GetValue(x, y, z);
		}
	}

}