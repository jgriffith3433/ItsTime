using System;
using TOZ;

namespace TOZ.Noise {

	public interface IModule {
		double GetValue(double x, double y, double z);
	}

}