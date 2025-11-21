using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TOZ;
using TOZ.Noise;

namespace TOZ.Noise.Utilities {

	public class NoiseMapRenderer {
		//Variables
		public NoiseMap SourceNoiseMap;
		public ImageMap DestinationImageMap;
		public GradientColor SourceGradientColor;

		//Properties
		public bool IsWrapEnabled { get; set; }
		public bool IsLightEnabled { get; set; }
		public Color LightColor { get; set; }
		public double LightElevation { get; set; }
		public double LightAzimuth { get; set; }
		public double LightBrightness { get; set; }
		public double LightContrast { get; set; }
		public double BumpHeight { get; set; }

		//Constructors
		public NoiseMapRenderer() {
			SourceNoiseMap = null;
			DestinationImageMap = null;
			SourceGradientColor = new GradientColor();
			SourceGradientColor.BuildGrayscaleGradient();

			IsWrapEnabled = false;
			IsLightEnabled = false;
			LightColor = Color.white;
			LightElevation = 45.0;
			LightAzimuth = 45.0;
			LightBrightness = 2.0;
			LightContrast = 3.0;
			BumpHeight = 1.0;
		}

		//Methods
		public void RenderImage() {
			int width = SourceNoiseMap.Width;
			int height = SourceNoiseMap.Height;
			int depth = SourceNoiseMap.Depth;
			DestinationImageMap = new ImageMap(width, height, depth);

			int ofsL, ofsR, ofsD, ofsU;
			Color destColor;
			double lightIntensity = 1.0;
			for(int z=0; z < depth; z++) {
				for(int y=0; y < height; y++) {
					for(int x=0; x < width; x++) {
						if(IsLightEnabled) {
							ofsL = -1; ofsD = -1; ofsR = 1; ofsU = 1;
							if(IsWrapEnabled) {
								if(x == 0) ofsL = (width-1);
								if(y == 0) ofsD = (height-1);
								if(x == width-1) ofsR = -(width-1);
								if(y == height-1) ofsU = -(height-1);
							}
							else {
								if(x == 0) ofsL = 0;
								if(y == 0) ofsD = 0;
								if(x == width-1) ofsR = 0;
								if(y == height-1) ofsU = 0;
							}

							double nl = SourceNoiseMap.GetValue(x + ofsL, y, z);
							double nd = SourceNoiseMap.GetValue(x, y + ofsD, z);
							double nr = SourceNoiseMap.GetValue(x + ofsR, y, z);
							double nu = SourceNoiseMap.GetValue(x, y + ofsU, z);
							lightIntensity = GetLightIntensity(nl, nd, nr, nu);
							lightIntensity *= LightBrightness;
						}

						destColor = SourceGradientColor.GetColor(SourceNoiseMap.GetValue(x, y, z));
						DestinationImageMap.SetValue(x, y, z, GetImageColor(destColor, lightIntensity));
					}
				}
			}
		}

		public void RenderNormal() {
			int width = SourceNoiseMap.Width;
			int height = SourceNoiseMap.Height;
			int depth = 1;
			DestinationImageMap = new ImageMap(width, height, depth);

			int ofsL, ofsR, ofsD, ofsU;
			for(int z=0; z < depth; z++) {
				for(int y=0; y < height; y++) {
					for(int x=0; x < width; x++) {
						ofsL = -1; ofsD = -1; ofsR = 1; ofsU = 1;
						if(IsWrapEnabled) {
							if(x == 0) ofsL = (width-1);
							if(y == 0) ofsD = (height-1);
							if(x == width-1) ofsR = -(width-1);
							if(y == height-1) ofsU = -(height-1);
						}
						else {
							if(x == 0) ofsL = 0;
							if(y == 0) ofsD = 0;
							if(x == width-1) ofsR = 0;
							if(y == height-1) ofsU = 0;
						}

						double nc = SourceNoiseMap.GetValue(x, y, 0);
						double nl = SourceNoiseMap.GetValue(x + ofsL, y, 0);
						double nd = SourceNoiseMap.GetValue(x, y + ofsD, 0);
						double nr = SourceNoiseMap.GetValue(x + ofsR, y, 0);
						double nu = SourceNoiseMap.GetValue(x, y + ofsU, 0);
						
						DestinationImageMap.SetValue(x, y, z, GetNormalColor(nc, nl, nd, nr, nu, BumpHeight));
					}
				}
			}
		}

		public void RenderFlow() {
			int width = SourceNoiseMap.Width;
			int height = SourceNoiseMap.Height;
			int depth = 1;
			DestinationImageMap = new ImageMap(width, height, depth);

			int ofsL, ofsR, ofsD, ofsU;
			for(int z=0; z < depth; z++) {
				for(int y=0; y < height; y++) {
					for(int x=0; x < width; x++) {
						ofsL = -1; ofsD = -1; ofsR = 1; ofsU = 1;
						if(IsWrapEnabled) {
							if(x == 0) ofsL = (width-1);
							if(y == 0) ofsD = (height-1);
							if(x == width-1) ofsR = -(width-1);
							if(y == height-1) ofsU = -(height-1);
						}
						else {
							if(x == 0) ofsL = 0;
							if(y == 0) ofsD = 0;
							if(x == width-1) ofsR = 0;
							if(y == height-1) ofsU = 0;
						}

						double nc = SourceNoiseMap.GetValue(x, y, 0);
						double nl = SourceNoiseMap.GetValue(x + ofsL, y, 0);
						double nd = SourceNoiseMap.GetValue(x, y + ofsD, 0);
						double nr = SourceNoiseMap.GetValue(x + ofsR, y, 0);
						double nu = SourceNoiseMap.GetValue(x, y + ofsU, 0);
						DestinationImageMap.SetValue(x, y, z, GetFlowColor(nc, nl, nd, nr, nu, BumpHeight));
					}
				}
			}
		}

		public void RenderSlope() {
			int width = SourceNoiseMap.Width;
			int height = SourceNoiseMap.Height;
			int depth = 1;
			DestinationImageMap = new ImageMap(width, height, depth);

			int ofsL, ofsR, ofsD, ofsU;
			for(int z=0; z < depth; z++) {
				for(int y=0; y < height; y++) {
					for(int x=0; x < width; x++) {
						ofsL = -1; ofsD = -1; ofsR = 1; ofsU = 1;
						if(IsWrapEnabled) {
							if(x == 0) ofsL = (width-1);
							if(y == 0) ofsD = (height-1);
							if(x == width-1) ofsR = -(width-1);
							if(y == height-1) ofsU = -(height-1);
						}
						else {
							if(x == 0) ofsL = 0;
							if(y == 0) ofsD = 0;
							if(x == width-1) ofsR = 0;
							if(y == height-1) ofsU = 0;
						}

						double nc = SourceNoiseMap.GetValue(x, y, 0);
						double nl = SourceNoiseMap.GetValue(x + ofsL, y, 0);
						double nd = SourceNoiseMap.GetValue(x, y + ofsD, 0);
						double nr = SourceNoiseMap.GetValue(x + ofsR, y, 0);
						double nu = SourceNoiseMap.GetValue(x, y + ofsU, 0);
						DestinationImageMap.SetValue(x, y, z, GetSlopeColor(nc, nl, nd, nr, nu, BumpHeight));
					}
				}
			}
		}

		public void RenderPlanet() {
			int width = SourceNoiseMap.Width;
			int height = SourceNoiseMap.Height;
			int depth = 1;
			DestinationImageMap = new ImageMap(width, height, depth);

			int ofsL, ofsR, ofsD, ofsU;
			for(int z=0; z < depth; z++) {
				for(int y=0; y < height; y++) {
					for(int x=0; x < width; x++) {
						ofsL = -1; ofsD = -1; ofsR = 1; ofsU = 1;
						if(IsWrapEnabled) {
							if(x == 0) ofsL = (width-1);
							if(y == 0) ofsD = (height-1);
							if(x == width-1) ofsR = -(width-1);
							if(y == height-1) ofsU = -(height-1);
						}
						else {
							if(x == 0) ofsL = 0;
							if(y == 0) ofsD = 0;
							if(x == width-1) ofsR = 0;
							if(y == height-1) ofsU = 0;
						}

						double nc = SourceNoiseMap.GetValue(x, y, 0);
						double nl = SourceNoiseMap.GetValue(x + ofsL, y, 0);
						double nd = SourceNoiseMap.GetValue(x, y + ofsD, 0);
						double nr = SourceNoiseMap.GetValue(x + ofsR, y, 0);
						double nu = SourceNoiseMap.GetValue(x, y + ofsU, 0);
						DestinationImageMap.SetValue(x, y, z, GetPlanetColor(nc, nl, nd, nr, nu, BumpHeight));
					}
				}
			}
		}

		private Color GetImageColor(Color sourceColor, double lightIntensity) {
			Color result = sourceColor;

			if(IsLightEnabled) {
				Color light = (float)lightIntensity * LightColor;
				result *= light;
			}

			result.r = _MathUtil._Clamp(result.r, 0.0f, 1.0f);
			result.g = _MathUtil._Clamp(result.g, 0.0f, 1.0f);
			result.b = _MathUtil._Clamp(result.b, 0.0f, 1.0f);
			result.a = _MathUtil._Clamp(result.a, 0.0f, 1.0f);
			return new Color(result.r, result.g, result.b, result.a);
		}

		private Color GetNormalColor(double nc, double nl, double nd, double nr, double nu, double height) {
			Vector4 vec = GetVector(nc, nl, nd, nr, nu, height);
			byte x = (byte)((int)Math.Floor((vec.x + 1.0) * 127.5) & 0xff);
			byte y = (byte)((int)Math.Floor((vec.y + 1.0) * 127.5) & 0xff);
			byte z = (byte)((int)Math.Floor((vec.z + 1.0) * 127.5) & 0xff);
			return new Color32(x, y, z, 255);
		}

		private Color GetFlowColor(double nc, double nl, double nd, double nr, double nu, double height) {
			Vector4 vec = GetVector(nc, nl, nd, nr, nu, height);
			byte x = (byte)((int)Math.Floor((-vec.x + 1.0) * 127.5) & 0xff);
			byte y = (byte)((int)Math.Floor((-vec.y + 1.0) * 127.5) & 0xff);
			return new Color32(x, y, 0, 255);
		}

		private Color GetSlopeColor(double nc, double nl, double nd, double nr, double nu, double height) {
			Vector4 vec = GetVector(nc, nl, nd, nr, nu, height);
			float p = vec.x * vec.x + vec.y * vec.y;
			float g = Mathf.Sqrt(p);
			float v = Mathf.Atan(g) * Mathf.Rad2Deg / 90f;
			byte c = (byte)((int)Math.Floor((v + 1.0) * 127.5) & 0xff);
			return new Color32(c, c, c, 255);
		}

		private Color GetPlanetColor(double nc, double nl, double nd, double nr, double nu, double height) {
			Vector4 vec = GetVector(nc, nl, nd, nr, nu, height);
			byte x = (byte)((int)Math.Floor((vec.x + 1.0) * 127.5) & 0xff);
			byte y = (byte)((int)Math.Floor((vec.y + 1.0) * 127.5) & 0xff);
			byte z = (byte)((int)Math.Floor((vec.z + 1.0) * 127.5) & 0xff);
			byte w = (byte)((int)Math.Floor((vec.w + 1.0) * 127.5) & 0xff);
			return new Color32(x, y, z, w);
		}

		private double GetLightIntensity(double nl, double nd, double nr, double nu) {
			double cosAzimuth = Math.Cos(LightAzimuth * _MathUtil._DEG2RAD);
			double sinAzimuth = Math.Sin(LightAzimuth * _MathUtil._DEG2RAD);
			double cosElev = Math.Cos(LightElevation * _MathUtil._DEG2RAD);
			double sinElev = Math.Sin(LightElevation * _MathUtil._DEG2RAD);

			double io = 1.0 * _MathUtil._SQRT2 * sinElev / 2.0;
			double ix = (1.0 - io) * LightContrast * _MathUtil._SQRT2 * cosElev * cosAzimuth;
			double iy = (1.0 - io) * LightContrast * _MathUtil._SQRT2 * cosElev * sinAzimuth;
			double intensity = (ix * (nl - nr) + iy * (nd - nu) + io);

			return _MathUtil._ClampDown(intensity, 0.0);
		}

		private Vector4 GetVector(double c, double l, double d, double r, double u, double height) {
			l *= height;
			d *= height;
			r *= height;
			u *= height;
			double lr = (l - r);
			double du = (d - u);
			double dist = Math.Sqrt((lr * lr) + (du * du) + 1.0);
			double x = lr / dist;
			double y = du / dist;
			double z = 1.0 / dist;
			return new Vector4((float)x, (float)y, (float)z, (float)c);
		}

		public Texture2D GetTexture2D() {
			Texture2D tex = new Texture2D(DestinationImageMap.Width, DestinationImageMap.Height, TextureFormat.RGBA32, true);
			tex.wrapMode = TextureWrapMode.Clamp;
			tex.filterMode = FilterMode.Point;
			Color[] cols = new Color[DestinationImageMap.Width * DestinationImageMap.Height];
			int i = 0;
			for(int y=0; y < tex.height; y++) {
				for(int x=0; x < tex.width; x++) {
					cols[i] = DestinationImageMap.GetValue(x, y, 0);
					i++;
				}
			}
			tex.SetPixels(cols);
			tex.Apply();
			return tex;
		}

		public Texture3D GetTexture3D() {
			Texture3D tex = new Texture3D(DestinationImageMap.Width, DestinationImageMap.Height, DestinationImageMap.Depth, TextureFormat.RGBA32, true);
			tex.wrapMode = TextureWrapMode.Clamp;
			tex.filterMode = FilterMode.Point;
			Color[] cols = new Color[DestinationImageMap.Width * DestinationImageMap.Height * DestinationImageMap.Depth];
			int i = 0;
			for(int z=0; z < tex.depth; z++) {
				for(int y=0; y < tex.height; y++) {
					for(int x=0; x < tex.width; x++) {
						cols[i] = DestinationImageMap.GetValue(x, y, z);
						i++;
					}
				}
			}
			tex.SetPixels(cols);
			tex.Apply();
			return tex;
		}

		
		
		
		
		
		
		public void RenderFlowMap() {
			int width = SourceNoiseMap.Width;
			int height = SourceNoiseMap.Height;
			int depth = 1;
			DestinationImageMap = new ImageMap(width, height, depth);

			//Calculate velocities of flow
			float[,] heightMap = new float[width, height];
			float[,] waterMap = new float[width, height];
			float[,] velocityMap = new float[width, height];
			float[,,] flowOut = new float[width, height, 4];
			Vector2[,] dirMap = new Vector2[width, height];
			float amount = 0.001f;
			float time = 0.002f;
			int _L = 0;
			int _R = 1;
			int _B = 2;
			int _T = 3;
			int iterations = 5;

			//Fill Heightmap
			int ofsL, ofsR, ofsD, ofsU;
			for(int z=0; z < depth; z++) {
				for(int y=0; y < height; y++) {
					for(int x=0; x < width; x++) {
						ofsL = -1; ofsD = -1; ofsR = 1; ofsU = 1;
						if(IsWrapEnabled) {
							if(x == 0) ofsL = (width-1);
							if(y == 0) ofsD = (height-1);
							if(x == width-1) ofsR = -(width-1);
							if(y == height-1) ofsU = -(height-1);
						}
						else {
							if(x == 0) ofsL = 0;
							if(y == 0) ofsD = 0;
							if(x == width-1) ofsR = 0;
							if(y == height-1) ofsU = 0;
						}

						double nc = SourceNoiseMap.GetValue(x, y, 0);
						byte w = (byte)((int)Math.Floor((nc + 1.0) * 127.5) & 0xff);
						heightMap[x,y] = (float)w / 255f;
					}
				}
			}

			//Fill Watermap
			for(int y = 0; y < height; y++) {
				for(int x = 0; x < width; x++) {
					waterMap[x,y] = amount;
				}
			}

			//Calculate everything
			for(int i = 0; i < iterations; i++) {
				//Compute flowOut
				for(int y = 0; y < height; y++) {
					for(int x = 0; x < width; x++) {
						ofsL = (x == 0) ? 0 : x-1;
						ofsR = (x == width-1) ? width-1 : x+1;
						ofsD = (y == 0) ? 0 : y-1;
						ofsU = (y == height-1) ? height-1 : y+1;

						float wC = waterMap[x, y];
						float wL = waterMap[ofsL, y];
						float wR = waterMap[ofsR, y];
						float wD = waterMap[x, ofsD];
						float wU = waterMap[x, ofsU];

						float lC = heightMap[x, y];
						float lL = heightMap[ofsL, y];
						float lR = heightMap[ofsR, y];
						float lD = heightMap[x, ofsD];
						float lU = heightMap[x, ofsU];

						float diff0 = (wC + lC) - (wL + lL);
						float diff1 = (wC + lC) - (wR + lR);
						float diff2 = (wC + lC) - (wD + lD);
						float diff3 = (wC + lC) - (wU + lU);

						//out flow is previous flow plus flow for this time step.
						float flow0 = Mathf.Max(0, flowOut[x,y,_L] + diff0);
						float flow1 = Mathf.Max(0, flowOut[x,y,_R] + diff1);
						float flow2 = Mathf.Max(0, flowOut[x,y,_B] + diff2);
						float flow3 = Mathf.Max(0, flowOut[x,y,_T] + diff3);

						float sum = flow0 + flow1 + flow2 + flow3;
						if(sum > 0.0f) {
							//If the sum of the flowOut flux exceeds the amount in the cell
							//flow value will be scaled down by a factor K to avoid negative update.
							float k = Mathf.Clamp(wC / (sum * time), 0f, 1f);
							flowOut[x,y,_L] = flow0 * k;
							flowOut[x,y,_R] = flow1 * k;
							flowOut[x,y,_B] = flow2 * k;
							flowOut[x,y,_T] = flow3 * k;
						}
						else {
							flowOut[x,y,_L] = 0.0f;
							flowOut[x,y,_R] = 0.0f;
							flowOut[x,y,_B] = 0.0f;
							flowOut[x,y,_T] = 0.0f;
						}
					}
				}

				//Update Watermap
				for(int y = 0; y < height; y++) {
					for(int x = 0; x < width; x++) {
						float fOut = flowOut[x,y,_L] + flowOut[x,y,_R] + flowOut[x,y,_B] + flowOut[x,y,_T];
						float fIn = 0.0f;

						//Flow in is inflow from neighour cells. Note for the cell on the left you need 
						//thats cells flow to the right (ie it flows into this cell)
						fIn += (x == width-1) ? 0.0f : flowOut[x+1, y, _L];
						fIn += (x == 0) ? 0.0f : flowOut[x-1, y, _R];
						fIn += (y == height-1) ? 0.0f : flowOut[x, y+1, _B];
						fIn += (y == 0) ? 0.0f : flowOut[x, y-1, _T];

						float v = waterMap[x,y] + (fIn - fOut) * time;
						if(v < 0.0f)
							v = 0.0f;

						//Result is net volume change over time
						waterMap[x,y] = v;
					}
				}
			}

			//Calculate Velocitymap
			for(int y = 0; y < height; y++) {
				for(int x = 0; x < width; x++) {
					float l = (x == 0) ? 0.0f : flowOut[x-1, y, _R] - flowOut[x, y, _L];
					float r = (x == width-1) ? 0.0f : flowOut[x, y, _R] - flowOut[x+1, y, _L];
					float b = (y == 0) ? 0.0f : flowOut[x, y, _B] - flowOut[x, y-1, _T];
					float t = (y == height-1) ? 0.0f : flowOut[x, y+1, _B] - flowOut[x, y, _T];

					float vx = (l + r) * 0.5f;
					float vy = (b + t) * 0.5f;

					dirMap[x,y] = new Vector2(-vx, vy).normalized;
					velocityMap[x,y] = Mathf.Sqrt(vx * vx + vy * vy);
				}
			}

			//Normalize Velocitymap
			float min = float.PositiveInfinity;
			float max = float.NegativeInfinity;
			for(int y = 0; y < height; y++) {
				for(int x = 0; x < width; x++) {
					float v = velocityMap[x,y];
					if(v < min)
						min = v;
					if(v > max)
						max = v;
				}
			}

			float size = max - min;
			for(int y = 0; y < height; y++) {
				for(int x = 0; x < width; x++) {
					float v = velocityMap[x,y];
					v = (size < Mathf.Epsilon) ? 0f : (v - min) / size;
					velocityMap[x,y] = v;
				}
			}

			for(int z=0; z < depth; z++) {
				for(int y=0; y < height; y++) {
					for(int x=0; x < width; x++) {
						//float v = velocityMap[x,y];
						//DestinationImageMap.SetValue(x, y, z, new Color(v, v, v, 1.0f));
						byte xc = (byte)((int)Math.Floor((dirMap[x,y].x + 1.0) * 127.5) & 0xff);
						byte yc = (byte)((int)Math.Floor((dirMap[x,y].y + 1.0) * 127.5) & 0xff);
						byte wc = (byte)((int)Math.Floor(((dirMap[x,y].magnitude * 2f - 1f) + 1.0) * 127.5) & 0xff);
						DestinationImageMap.SetValue(x, y, z, new Color32(xc, yc, 0, wc));
					}
				}
			}
		}
	}

}