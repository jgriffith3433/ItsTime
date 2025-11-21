using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TOZ;

namespace TOZ.Noise.Utilities {

	public class NoiseMap {
		//Variables
		public double[,,] Data;
		public int Width;
		public int Height;
		public int Depth;
		public double Border;

		//Constructors
		public NoiseMap() : this(0, 0, 0, 0.0) {
		}

		public NoiseMap(int width, int height) : this(width, height, 1, 0.0) {
		}

		public NoiseMap(int width, int height, int depth) : this(width, height, depth, 0.0) {
		}

		public NoiseMap(int width, int height, int depth, double border) {
			SetSize(width, height, depth);
			Border = border;
		}

		public NoiseMap(Texture2D tex, double border) {
			SetSize(tex.width, tex.height, 1);
			Border = border;

			Color[] col = tex.GetPixels();
			int i = 0;
			for(int y = 0; y < tex.height; y++) {
				for(int x = 0; x < tex.width; x++) {
					Data[x, y, 0] = col[i].grayscale * 2.0 - 1.0;
					i++;
				}
			}
		}

		public NoiseMap(Texture3D tex, double border) {
			SetSize(tex.width, tex.height, tex.depth);
			Border = border;

			Color[] col = tex.GetPixels();
			int i = 0;
			for(int z = 0; z < tex.depth; z++) {
				for(int y = 0; y < tex.height; y++) {
					for(int x = 0; x < tex.width; x++) {
						Data[x, y, z] = col[i].grayscale * 2.0 - 1.0;
						i++;
					}
				}
			}
		}

		//Methods
		public double GetValue(int x, int y, int z) {
			if(Data != null) {
				if(x >= 0 && x < Width && y >= 0 && y < Height && z >= 0 && z < Depth) {
					return Data[x, y, z];
				}
			}
			return Border;
		}

		public void SetValue(int x, int y, int z, double v) {
			if(Data != null) {
				if(x >= 0 && x < Width && y >= 0 && y < Height && z >= 0 && z < Depth) {
					Data[x, y, z] = v;
				}
			}
		}

		public void SetSize(int width, int height, int depth) {
			Width = width;
			Height = height;
			Depth = depth;
			Data = new double[Width, Height, Depth];
		}
	}

}