using System;
using TOZ;
using TOZ.Noise;

namespace TOZ.Noise.Module {

	public class Voronoi : IModule {
		//Properties
		public double Frequency { get; set; }
		public double Displacement { get; set; }
		public bool EnableDistance { get; set; }
		public int Seed { get; set; }
		public bool Clamped { get; set; }

		//Constructor
		public Voronoi() {
			Frequency = 1.0;
			Displacement = 1.0;
			EnableDistance = true;
			Seed = 0;
			Clamped = true;
		}

		//Methods
		public double GetValue(double x, double y, double z) {
			double minDist = 2147483647.0;
			double xCandidate = 0.0;
			double yCandidate = 0.0;
			double zCandidate = 0.0;

			//This method could be more efficient by caching.
			x *= Frequency;
			y *= Frequency;
			z *= Frequency;

			int xInt = (x > 0.0 ? (int)x : (int)x - 1);
			int yInt = (x > 0.0 ? (int)y : (int)y - 1);
			int zInt = (x > 0.0 ? (int)z : (int)z - 1);

			//Inside each unit cube, theres a seed point at random position.
			for(int zCur = zInt - 2; zCur <= zInt + 2; zCur++) {
				for(int yCur = yInt - 2; yCur <= yInt + 2; yCur++) {
					for(int xCur = xInt - 2; xCur <= xInt + 2; xCur++) {
						//Calculate the position and distance to the seed point.
						double xPos = xCur + _GENERATOR.DNoiseVal(xCur, yCur, zCur, Seed);
						double yPos = yCur + _GENERATOR.DNoiseVal(xCur, yCur, zCur, Seed+1);
						double zPos = zCur + _GENERATOR.DNoiseVal(xCur, yCur, zCur, Seed+2);
						double xDist = xPos - x;
						double yDist = yPos - y;
						double zDist = zPos - z;
						double dist = xDist * xDist + yDist * yDist + zDist * zDist;
						
						if(dist < minDist) {
							//This seed point is closer to any others found so far.
							minDist = dist;
							xCandidate = xPos;
							yCandidate = yPos;
							zCandidate = zPos;
						}
					}
				}
			}

			double v;
			if(EnableDistance) {
				//Determine the distance to nearest seed point
				double xDist = xCandidate - x;
				double yDist = yCandidate - y;
				double zDist = zCandidate - z;
				v = Math.Sqrt(xDist * xDist + yDist * yDist + zDist * zDist) * _MathUtil._SQRT3 - 1.0;
			}
			else {
				v = 0.0;
			}
			//Return calculated distance with displacement applied
			v += (Displacement * _GENERATOR.DNoiseVal(
														(int)(Math.Floor(xCandidate)),
														(int)(Math.Floor(yCandidate)),
														(int)(Math.Floor(zCandidate)))
			);

			if(Clamped)
				v = _MathUtil._Clamp(v, -1.0, 1.0);
			return v;
		}
	}

}