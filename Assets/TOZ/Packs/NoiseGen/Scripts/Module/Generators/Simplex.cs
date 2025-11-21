using System;
using TOZ;
using TOZ.Noise;

namespace TOZ.Noise.Module {

	public sealed class Simplex : IModule {
		//Properties
		public double Frequency { get; set; }
		public double Lacunarity { get; set; }
		public int Octaves { get; set; }
		public double Persistence { get; set; }
		public int Seed { get; set; }
		public bool Clamped { get; set; }

		//Constructors
		public Simplex() {
			Frequency = 1.0;
			Lacunarity = 2.0;
			Octaves = 6;
			Persistence = 0.5;
			Seed = 0;
			Clamped = true;
		}

		//Methods
		public double GetValue(double x, double y, double z) {
			x *= Frequency;
			y *= Frequency;
			z *= Frequency;

			double v = 0.0;
			double signal = 0.0;
			double curPersistence = 1.0;

			double nx, ny, nz;
			int seed;
			for(int curOctave = 0; curOctave < Octaves; curOctave++) {
				nx = _MathUtil._DoubleToInt32Range(x);
				ny = _MathUtil._DoubleToInt32Range(y);
				nz = _MathUtil._DoubleToInt32Range(z);
				//Get the coherent-noise value from input value and add it to final result
				seed = (Seed + curOctave) & 0x7fffffff;
				signal = _GENERATOR.GradientNoise(nx, ny, nz, seed);
				v += signal * curPersistence;
				
				x *= Lacunarity;
				y *= Lacunarity;
				z *= Lacunarity;
				curPersistence *= Persistence;
			}

			if(Clamped)
				v = _MathUtil._Clamp(v, -1.0, 1.0);
			return v;
		}
	}

}