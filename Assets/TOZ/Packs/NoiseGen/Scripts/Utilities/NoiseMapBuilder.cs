using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TOZ;
using TOZ.Noise;
using TOZ.Noise.Model;

namespace TOZ.Noise.Utilities {

	//NoiseMap classes
	public class NoiseMapBuilderBox {
		//Variables
		public NoiseMap Map;

		//Properties
		public double LowerXBound { get; set; }
		public double LowerYBound { get; set; }
		public double LowerZBound { get; set; }
		public double UpperXBound { get; set; }
		public double UpperYBound { get; set; }
		public double UpperZBound { get; set; }

		//Constructors//
		public NoiseMapBuilderBox() : this (16, 16, 16) {
		}

		public NoiseMapBuilderBox(int width, int height, int depth) {
			Map = new NoiseMap(width, height, depth);
			SetBounds(-1.0, -1.0, -1.0, 1.0, 1.0, 1.0);
		}

		//Methods//
		public void Build(IModule sourceModule) {
			Model.Box model = new Model.Box(sourceModule);
			
			double xExtent = UpperXBound - LowerXBound;
			double yExtent = UpperYBound - LowerYBound;
			double zExtent = UpperZBound - LowerZBound;
			double xDelta = xExtent / (Map.Width-1);
			double yDelta = yExtent / (Map.Height-1);
			double zDelta = zExtent / (Map.Depth-1);
			double xCurrent = LowerXBound;
			double yCurrent = LowerYBound;
			double zCurrent = LowerZBound;

			double finalValue;
			for (int z = 0; z < Map.Depth; z++) {
				yCurrent = LowerYBound;
				for (int y = 0; y < Map.Height; y++) {
					xCurrent = LowerXBound;
					for (int x = 0; x < Map.Width; x++) {
						finalValue = model.GetValue(xCurrent, yCurrent, zCurrent);
						Map.SetValue(x, y, z, finalValue);
						xCurrent += xDelta;
					}
					yCurrent += yDelta;
				}
				zCurrent += zDelta;
			}
		}

		public void SetBounds(double lx, double ly, double lz, double ux, double uy, double uz) {
			LowerXBound = lx; LowerYBound = ly; LowerZBound = lz;
			UpperXBound = ux; UpperYBound = uy; UpperZBound = uz;
		}
	}

	public class NoiseMapBuilderCylinder {
		//Variables
		public NoiseMap Map;

		//Properties
		public double LowerAngleBound { get; set; }
		public double LowerHeightBound { get; set; }
		public double UpperAngleBound { get; set; }
		public double UpperHeightBound { get; set; }

		//Constructors
		public NoiseMapBuilderCylinder() : this (64, 64) {
		}

		public NoiseMapBuilderCylinder(int width, int height) {
			Map = new NoiseMap(width, height, 1);
			SetBounds(-180.0, -1.0, 180.0, 1.0);
		}

		//Methods
		public void Build(IModule sourceModule) {
			Model.Cylinder model = new Model.Cylinder(sourceModule);
			
			double xExtent = UpperAngleBound - LowerAngleBound;
			double yExtent = UpperHeightBound - LowerHeightBound;
			double xDelta = xExtent / (Map.Width-1);
			double yDelta = yExtent / (Map.Height-1);
			double xCurrent = LowerAngleBound;
			double yCurrent = LowerHeightBound;

			double finalValue;
			for (int y = 0; y < Map.Height; y++) {
				xCurrent = LowerAngleBound;
				for (int x = 0; x < Map.Width; x++) {
					finalValue = model.GetValue(xCurrent, yCurrent);
					Map.SetValue(x, y, 0, finalValue);
					xCurrent += xDelta;
				}
				yCurrent += yDelta;
			}
		}

		public void SetBounds(double la, double lh, double ua, double uh) {
			LowerAngleBound = la;
			LowerHeightBound = lh;
			UpperAngleBound = ua;
			UpperHeightBound = uh;
		}
	}

	public class NoiseMapBuilderLine {
		//Variables
		public NoiseMap Map;

		//Properties
		public double LowerXBound { get; set; }
		public double UpperXBound { get; set; }

		//Constructors
		public NoiseMapBuilderLine() : this (128) {
		}

		public NoiseMapBuilderLine(int width) {
			Map = new NoiseMap(width, 1, 1);
			SetBounds(-1.0, 1.0);
		}

		//Methods
		public void Build(IModule sourceModule) {
			Model.Line model = new Model.Line(sourceModule);
			model.SetStartPoint(LowerXBound, 0.0, 0.0);
			model.SetEndPoint(UpperXBound, 0.0, 0.0);
			model.Attenuate = true;
			
			double extent = UpperXBound - LowerXBound;
			double delta = extent / (Map.Width-1);
			double current = LowerXBound;

			double finalValue;
			for(int x = 0; x < Map.Width; x++) {
				finalValue = model.GetValue(current);
				Map.SetValue(x, 0, 0, finalValue);
				current += delta;
			}
		}

		public void SetBounds(double l, double u) {
			LowerXBound = l; UpperXBound = u;
		}
	}

	public class NoiseMapBuilderPlane {
		//Variables
		public NoiseMap Map;

		//Properties
		public double LowerXBound { get; set; }
		public double LowerYBound { get; set; }
		public double UpperXBound { get; set; }
		public double UpperYBound { get; set; }
		public bool IsSeamless { get; set; }

		//Constructors
		public NoiseMapBuilderPlane() : this (64, 64) {
		}

		public NoiseMapBuilderPlane(int width, int height) {
			Map = new NoiseMap(width, height, 1);
			SetBounds(-1.0, -1.0, 1.0, 1.0);
			IsSeamless = false;
		}

		//Methods
		public void Build(IModule sourceModule) {
			Model.Plane model = new Model.Plane(sourceModule);
			
			double xExtent = UpperXBound - LowerXBound;
			double yExtent = UpperYBound - LowerYBound;
			double xDelta = xExtent / (Map.Width-1);
			double yDelta = yExtent / (Map.Height-1);
			double xCurrent = LowerXBound;
			double yCurrent = LowerYBound;

			double finalValue;
			for(int y = 0; y < Map.Height; y++) {
				xCurrent = LowerXBound;
				for(int x = 0; x < Map.Width; x++) {
					if (!IsSeamless) {
						finalValue = model.GetValue(xCurrent, yCurrent);
					}
					else {
						double swValue, seValue, nwValue, neValue;
						swValue = model.GetValue(xCurrent, yCurrent);
						seValue = model.GetValue(xCurrent + xExtent, yCurrent);
						nwValue = model.GetValue(xCurrent, yCurrent + yExtent);
						neValue = model.GetValue(xCurrent + xExtent, yCurrent + yExtent);
						double xBlend = 1.0 - ((xCurrent - LowerXBound) / xExtent);
						double yBlend = 1.0 - ((yCurrent - LowerYBound) / yExtent);
						double t0 = _InterpolationUtil._Linear(swValue, seValue, xBlend);
						double t1 = _InterpolationUtil._Linear(nwValue, neValue, xBlend);
						finalValue = _InterpolationUtil._Linear(t0, t1, yBlend);
					}
					Map.SetValue(x, y, 0, finalValue);
					xCurrent += xDelta;
				}
				yCurrent += yDelta;
			}
		}

		public void SetBounds(double lx, double ly, double ux, double uy) {
			LowerXBound = lx; UpperXBound = ux;
			LowerYBound = ly; UpperYBound = uy;
		}
	}

	public class NoiseMapBuilderSphere {
		//Variables
		public NoiseMap Map;

		//Properties
		public double BoundSouthLon { get; set; }
		public double BoundNorthLon { get; set; }
		public double BoundEastLat { get; set; }
		public double BoundWestLat { get; set; }

		//Constructors
		public NoiseMapBuilderSphere() : this (64, 64) {
		}

		public NoiseMapBuilderSphere(int width, int height) {
			Map = new NoiseMap(width, height, 1);
			SetBounds(-180.0, -90.0, 180.0, 90.0);
		}

		//Methods
		public void Build(IModule sourceModule) {
			Model.Sphere model = new Model.Sphere(sourceModule);

			double xExtent = BoundEastLat - BoundWestLat;
			double yExtent = BoundNorthLon - BoundSouthLon;
			double xDelta = xExtent / (Map.Width-1);
			double yDelta = yExtent / (Map.Height-1);
			double xCurrent = BoundWestLat;
			double yCurrent = BoundSouthLon;

			double finalValue;
			for(int y = 0; y < Map.Height; y++) {
				xCurrent = BoundWestLat;
				for(int x = 0; x < Map.Width; x++) {
					finalValue = model.GetValue(xCurrent, yCurrent);
					Map.SetValue(x, y, 0, finalValue);
					xCurrent += xDelta;
				}
				yCurrent += yDelta;
			}
		}

		public void SetBounds(double w, double s, double e, double n) {
			BoundWestLat = w;
			BoundSouthLon = s;
			BoundEastLat = e;
			BoundNorthLon = n;
		}
	}

	public class NoiseMapBuilderPlanet {
		//Variables
		public NoiseMap Map;

		//Properties
		public double LowerXBound { get; set; }
		public double LowerYBound { get; set; }
		public double LowerZBound { get; set; }
		public double UpperXBound { get; set; }
		public double UpperYBound { get; set; }
		public double UpperZBound { get; set; }

		//Constructors
		public NoiseMapBuilderPlanet() : this (129, 129) {
		}

		public NoiseMapBuilderPlanet(int width, int height) {
			Map = new NoiseMap(width, height, 6);
			SetBounds(-1.0, -1.0, -1.0, 1.0, 1.0, 1.0);
		}

		//Methods
		public void Build(IModule sourceModule) {
			Model.Box model = new Model.Box(sourceModule);

			//Its a box, boundaries should be same size
			double extent = UpperXBound - LowerXBound;
			double delta = extent / (Map.Width-1);
			double xCurrent = LowerXBound;
			double yCurrent = LowerYBound;
			double zCurrent = LowerZBound;
			double finalValue;

			//Back
			xCurrent = LowerXBound;
			yCurrent = LowerYBound;
			zCurrent = LowerZBound;
			for(int y = 0; y < Map.Height; y++) {
				xCurrent = LowerXBound;
				for(int x = 0; x < Map.Width; x++) {
					finalValue = model.GetValue(xCurrent, yCurrent, zCurrent);
					Map.SetValue(x, y, 0, finalValue);
					xCurrent += delta;
				}
				yCurrent += delta;
			}

			//Right
			xCurrent = LowerXBound;
			yCurrent = LowerYBound;
			zCurrent = LowerZBound;
			for(int y = 0; y < Map.Height; y++) {
				zCurrent = LowerZBound;
				for(int x = 0; x < Map.Width; x++) {
					finalValue = model.GetValue(xCurrent, yCurrent, zCurrent);
					Map.SetValue(x, y, 1, finalValue);
					zCurrent += delta;
				}
				yCurrent += delta;
			}

			//Top
			xCurrent = LowerXBound;
			yCurrent = LowerYBound;
			zCurrent = LowerZBound;
			for(int y = 0; y < Map.Height; y++) {
				xCurrent = LowerXBound;
				for(int x = 0; x < Map.Width; x++) {
					finalValue = model.GetValue(xCurrent, yCurrent, zCurrent);
					Map.SetValue(x, y, 2, finalValue);
					xCurrent += delta;
				}
				zCurrent += delta;
			}

			//Front
			xCurrent = LowerXBound;
			yCurrent = LowerYBound;
			zCurrent = LowerZBound;
			for(int y = 0; y < Map.Height; y++) {
				xCurrent = UpperXBound;
				for(int x = 0; x < Map.Width; x++) {
					finalValue = model.GetValue(xCurrent, yCurrent, zCurrent);
					Map.SetValue(x, y, 3, finalValue);
					xCurrent -= delta;
				}
				yCurrent += delta;
			}

			//Left
			xCurrent = LowerXBound;
			yCurrent = LowerYBound;
			zCurrent = LowerZBound;
			for(int y = 0; y < Map.Height; y++) {
				zCurrent = UpperZBound;
				for(int x = 0; x < Map.Width; x++) {
					finalValue = model.GetValue(xCurrent, yCurrent, zCurrent);
					Map.SetValue(x, y, 4, finalValue);
					zCurrent -= delta;
				}
				yCurrent += delta;
			}

			//Bottom
			xCurrent = LowerXBound;
			yCurrent = LowerYBound;
			zCurrent = LowerZBound;
			for(int y = 0; y < Map.Height; y++) {
				xCurrent = UpperXBound;
				for(int x = 0; x < Map.Width; x++) {
					finalValue = model.GetValue(xCurrent, yCurrent, zCurrent);
					Map.SetValue(x, y, 5, finalValue);
					xCurrent -= delta;
				}
				zCurrent += delta;
			}
		}

		public void SetBounds(double lx, double ly, double lz, double ux, double uy, double uz) {
			LowerXBound = lx; LowerYBound = ly; LowerZBound = lz;
			UpperXBound = ux; UpperYBound = uy; UpperZBound = uz;
		}
	}

}