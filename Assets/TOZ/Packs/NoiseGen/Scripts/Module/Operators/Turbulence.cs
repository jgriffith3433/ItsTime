using System;
using TOZ;
using TOZ.Noise;

namespace TOZ.Noise.Module {

	public class Turbulence : IModule {
		//Variables
		private Perlin X;
		private Perlin Y;
		private Perlin Z;

		//Properties
		public IModule Module0 { get; set; }
		public double Power { get; set; }
		public double Frequency {
			get { return X.Frequency; }
			set { X.Frequency = value; Y.Frequency = value; Z.Frequency = value; }
		}
		public int Roughness {
			get { return X.Octaves; }
			set { X.Octaves = value; Y.Octaves = value; Z.Octaves = value; }
		}
		public int Seed {
			get { return X.Seed; }
			set { X.Seed = value; Y.Seed = value + 1; Z.Seed = value + 2; }
		}
		public bool Clamped { get; set; }

		//Constructor
		public Turbulence() {
			X = new Perlin();
			Y = new Perlin();
			Z = new Perlin();
		}

		public Turbulence(IModule mod0) {
			X = new Perlin();
			Y = new Perlin();
			Z = new Perlin();
			Module0 = mod0;
			Power = 1.0;
			Frequency = 1.0;
			Roughness = 6;
			Seed = 0;
			Clamped = true;
		}

		//Methods
		public double GetValue(double x, double y, double z) {
			//Get values from three Perlins
			double x0, y0, z0;
			double x1, y1, z1;
			double x2, y2, z2;
			x0 = x + (12414.0 / 65536.0);
			y0 = y + (65124.0 / 65536.0);
			z0 = z + (31337.0 / 65536.0);
			x1 = x + (26519.0 / 65536.0);
			y1 = y + (18128.0 / 65536.0);
			z1 = z + (60493.0 / 65536.0);
			x2 = x + (53820.0 / 65536.0);
			y2 = y + (11213.0 / 65536.0);
			z2 = z + (44845.0 / 65536.0);
			double xDistort = x + (X.GetValue(x0, y0, z0) * Power);
			double yDistort = y + (Y.GetValue(x1, y1, z1) * Power);
			double zDistort = z + (Z.GetValue(x2, y2, z2) * Power);
			//Retrieve the output value
			double v = Module0.GetValue(xDistort, yDistort, zDistort);

			if(Clamped)
				v = _MathUtil._Clamp(v, -1.0, 1.0);
			return v;
		}
	}

}