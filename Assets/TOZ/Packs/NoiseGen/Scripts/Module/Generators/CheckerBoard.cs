using System;
using TOZ;
using TOZ.Noise;

namespace TOZ.Noise.Module {

	public class CheckerBoard : IModule {
		//Constructors
		public CheckerBoard() {
		}

		//Methods
		public double GetValue(double x, double y, double z) {
			int ix = (int)(Math.Floor(_MathUtil._DoubleToInt32Range(x)));
			int iy = (int)(Math.Floor(_MathUtil._DoubleToInt32Range(y)));
			int iz = (int)(Math.Floor(_MathUtil._DoubleToInt32Range(z)));
			return (ix & 1 ^ iy & 1 ^ iz & 1) != 0 ? -1.0 : 1.0;
			//Already clamped
		}
	}

}