using System;
using TOZ;
using TOZ.Noise;

namespace TOZ.Noise.Module {

	public class RotatePoint : IModule {
		//Properties
		public IModule Module0 { get; set; }
		public double X { get; set; }
		public double Y { get; set; }
		public double Z { get; set; }
		public bool Clamped { get; set; }

		//Variables
		double m_x1Matrix, m_x2Matrix, m_x3Matrix;
		double m_y1Matrix, m_y2Matrix, m_y3Matrix;
		double m_z1Matrix, m_z2Matrix, m_z3Matrix;

		//Constructor
		public RotatePoint() { }

		public RotatePoint(IModule mod0) : this(mod0, 0.0, 0.0, 0.0) {
		}

		public RotatePoint(IModule mod0, double x, double y, double z) {
			Module0 = mod0;
			SetAngles(x, y, z);
			Clamped = true;
		}

		//Methods
		public void SetAngles(double x, double y, double z) {
			double xCos, yCos, zCos, xSin, ySin, zSin;
			xCos = Math.Cos(x * _MathUtil._DEG2RAD);
			yCos = Math.Cos(y * _MathUtil._DEG2RAD);
			zCos = Math.Cos(z * _MathUtil._DEG2RAD);
			xSin = Math.Sin(x * _MathUtil._DEG2RAD);
			ySin = Math.Sin(y * _MathUtil._DEG2RAD);
			zSin = Math.Sin(z * _MathUtil._DEG2RAD);

			m_x1Matrix = ySin * xSin * zSin + yCos * zCos;
			m_y1Matrix = xCos * zSin;
			m_z1Matrix = ySin * zCos - yCos * xSin * zSin;
			m_x2Matrix = ySin * xSin * zCos - yCos * zSin;
			m_y2Matrix = xCos * zCos;
			m_z2Matrix = -yCos * xSin * zCos - ySin * zSin;
			m_x3Matrix = -ySin * xCos;
			m_y3Matrix = xSin;
			m_z3Matrix = yCos * xCos;

			X = x; 
			Y = y;
			Z = z;
		}

		public double GetValue(double x, double y, double z) {
			double nx = (m_x1Matrix * x) + (m_y1Matrix * y) + (m_z1Matrix * z);
			double ny = (m_x2Matrix * x) + (m_y2Matrix * y) + (m_z2Matrix * z);
			double nz = (m_x3Matrix * x) + (m_y3Matrix * y) + (m_z3Matrix * z);
			double v = Module0.GetValue(nx, ny, nz);

			if(Clamped)
				v = _MathUtil._Clamp(v, -1.0, 1.0);
			return v;
		}
	}

}