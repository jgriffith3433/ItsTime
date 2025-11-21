using System;
using TOZ;
using TOZ.Noise;

namespace TOZ.Noise.Module {

	public class Select : IModule {
		//Variables
		private double edgeFalloff;

		//Properties
		public IModule Module0 { get; set; }
		public IModule Module1 { get; set; }
		public IModule ModuleA { get; set; }
		public double LowerBound { get; private set; }
		public double UpperBound { get; private set; }
		public double EdgeFallOff { 
			get { return edgeFalloff; }
			set {
				double boundSizeHalf = (UpperBound - LowerBound) / 2.0;
				edgeFalloff = (value > boundSizeHalf) ? boundSizeHalf : value;
			}
		}

		//Constructors
		public Select() { }

		public Select(IModule mod0, IModule mod1, IModule modA) : 
							this(mod0, mod1, modA, 0.0, -1.0, 1.0) {
		}

		public Select(IModule mod0, IModule mod1, IModule modA,
							double edge, double lb, double ub) {
			Module0 = mod0;
			Module1 = mod1;
			ModuleA = modA;
			EdgeFallOff = edge;
			LowerBound = lb;
			UpperBound = ub;
		}

		//Methods
		public double GetValue(double x, double y, double z) {
			double controlValue = ModuleA.GetValue(x, y, z);
			double alpha;
			if(EdgeFallOff > 0.0) {
				if(controlValue < (LowerBound - EdgeFallOff)) {
					return Module0.GetValue(x, y, z);
				}
				else if(controlValue < (LowerBound + EdgeFallOff)) {
					double lowerCurve = (LowerBound - EdgeFallOff);
					double upperCurve = (LowerBound + EdgeFallOff);
					alpha = _MathUtil._SCurve3((controlValue - lowerCurve) / (upperCurve - lowerCurve));
					return _InterpolationUtil._Linear(Module0.GetValue(x, y, z), Module1.GetValue(x, y, z), alpha);
				}
				else if(controlValue < (UpperBound - EdgeFallOff)) {
					return Module1.GetValue(x, y, z);
				}
				else if(controlValue < (UpperBound + EdgeFallOff)) {
					double lowerCurve = (UpperBound - EdgeFallOff);
					double upperCurve = (UpperBound + EdgeFallOff);
					alpha = _MathUtil._SCurve3((controlValue - lowerCurve) / (upperCurve - lowerCurve));
					return _InterpolationUtil._Linear(Module1.GetValue(x, y, z), Module0.GetValue(x, y, z), alpha);
				}
				else {
					return Module0.GetValue(x, y, z);
				}
			}
			else {
				if(controlValue < LowerBound || controlValue > UpperBound) {
					return Module0.GetValue(x, y, z);
				}
				else {
					return Module1.GetValue(x, y, z);
				}
			}
			//Already clamped
		}

		public void SetBounds(double lb, double ub) {
			LowerBound = lb;
			UpperBound = ub;
			//Make sure the edge falloff curves dont overlap.
			EdgeFallOff = edgeFalloff;
		}

		public void SetEdgeFallOff(double value) {
			double boundSizeHalf = (UpperBound - LowerBound) / 2.0;
			edgeFalloff = (value > boundSizeHalf) ? boundSizeHalf : value;
		}
	}

}