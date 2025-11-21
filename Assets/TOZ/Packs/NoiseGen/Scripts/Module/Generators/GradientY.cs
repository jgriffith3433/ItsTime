using System;
using TOZ;
using TOZ.Noise;

namespace TOZ.Noise.Module {

	public class GradientY : IModule {
		//Properties
		public double Start { get; set; }
		public double End { get; set; }
		public double LowerBound { get; set; }
		public double UpperBound { get; set; }
		public bool Invert { get; set; }

		//Constructors
		public GradientY() {
			Start = -1.0;
			End = 1.0;
			LowerBound = -1.0;
			UpperBound = 1.0;
			Invert = false;
		}

		//Methods
		public double GetValue(double x, double y, double z) {
			if(Invert)
				return _InterpolationUtil._Grade(End, Start, LowerBound, UpperBound, y);
			else
				return _InterpolationUtil._Grade(Start, End, LowerBound, UpperBound, y);
			//Clamp is user defined
		}
	}

}