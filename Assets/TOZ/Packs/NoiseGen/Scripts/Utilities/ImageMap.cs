using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TOZ;

namespace TOZ.Noise.Utilities {

	public class ImageMap {
		//Variables
		public Color[,,] Data;
		public int Width;
		public int Height;
		public int Depth;
		public Color Border;

		//Constructors
		public ImageMap() : this(0, 0, 0, Color.clear) {
		}

		public ImageMap(int width, int height) : this(width, height, 1, Color.clear) {
		}

		public ImageMap(int width, int height, int depth) : this(width, height, depth, Color.clear) {
		}

		public ImageMap(int width, int height, int depth, Color border) {
			SetSize(width, height, depth);
			Border = border;
		}

		public ImageMap(Texture2D tex, Color border) {
			SetSize(tex.width, tex.height, 1);
			Border = border;

			Color[] col = tex.GetPixels();
			int i = 0;
			for(int y = 0; y < tex.height; y++) {
				for(int x = 0; x < tex.width; x++) {
					Data[x, y, 0] = col[i];
					i++;
				}
			}
		}

		public ImageMap(Texture3D tex, Color border) {
			SetSize(tex.width, tex.height, tex.depth);
			Border = border;

			Color[] col = tex.GetPixels();
			int i = 0;
			for(int z = 0; z < tex.depth; z++) {
				for(int y = 0; y < tex.height; y++) {
					for(int x = 0; x < tex.width; x++) {
						Data[x, y, z] = col[i];
						i++;
					}
				}
			}
		}

		//Methods
		public Color GetValue(int x, int y, int z) {
			if(Data != null) {
				if(x >= 0 && x < Width && y >= 0 && y < Height && z >= 0 && z < Depth) {
					return Data[x, y, z];
				}
			}
			return Border;
		}

		public void SetValue(int x, int y, int z, Color v) {
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
			Data = new Color[Width, Height, Depth];
		}
	}

}