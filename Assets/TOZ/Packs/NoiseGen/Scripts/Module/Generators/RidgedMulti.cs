using System;
using TOZ;
using TOZ.Noise;

namespace TOZ.Noise.Module {

	public sealed class RidgedMulti : IModule {
		//Properties//
		public double Frequency { get; set; }
		public double Lacunarity { get; set; }
		public NoiseQuality Quality { get; set; }
		public int Octaves { get; set; }
		public int Seed { get; set; }
		public double Offset { get; set; }
		public double Gain { get; set; }
		public bool Clamped { get; set; }

		//Constructor//
		public RidgedMulti() {
			//Defaults
			Frequency = 1.0;
			Lacunarity = 2.0;
			Quality = NoiseQuality.Normal;
			Octaves = 6;
			Seed = 0;
			Offset = 1.0;
			Gain = 2.0;
			Clamped = true;
		}

		//Methods//
		public double GetValue(double x, double y, double z) {
			double[] spectralWeights = CalculateSpectralWeights();
			
			x *= Frequency;
			y *= Frequency;
			z *= Frequency;

			double v = 0.0;
			double signal = 0.0;
			double weight = 1.0;

			double nx, ny, nz;
			int seed;
			for(int curOctave = 0; curOctave < Octaves; curOctave++) {
				nx = _MathUtil._DoubleToInt32Range(x);
				ny = _MathUtil._DoubleToInt32Range(y);
				nz = _MathUtil._DoubleToInt32Range(z);
				//Get the coherent-noise value from input value and add it to final result
				seed = (Seed + curOctave) & 0x7fffffff;
				signal = _GENERATOR.GradientNoise(nx, ny, nz, Quality, seed);

				//Make the ridges
				signal = Math.Abs(signal);
				signal = Offset - signal;
				//signal = Offset + signal;
				//Square signal to increase sharpness of ridges.
				signal *= signal;
				//The weighting from previous octave is applied to the signal.
				signal *= weight;
				//Weight successive contributions by previous signal.
				weight = _MathUtil._Clamp(signal * Gain, 0.0, 1.0);

				//Add the signal to output value.
				v += (signal * spectralWeights[curOctave]);
				//Next Octave
				x *= Lacunarity;
				y *= Lacunarity;
				z *= Lacunarity;
			}
			//v = (v * 1.25) - 1.0;
			v -= 1.0;

			if(Clamped)
				v = _MathUtil._Clamp(v, -1.0, 1.0);
			return v;
		}

		private double[] CalculateSpectralWeights() {
			double[] weights = new double[Octaves];
			//This exponent parameter should be user defined
			double h = 1.0;
			double frequency = 1.0;
			for(int i = 0; i < weights.Length; i++) {
				weights[i] = Math.Pow(frequency, -h);
				frequency *= Lacunarity;
			}
			return weights;
		}
	}

}