using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TOZ;

namespace TOZ.Noise.Utilities {

	//Defines a point used to build a color gradient.
	[Serializable]
	public class GradientPoint : IComparable<GradientPoint> {
		//Variables
		public double Pos;
		public Color Col;

		//Constructor
		public GradientPoint(double pos, Color col) {
			Pos = pos;
			Col = col;
		}

		//Comparer
		public int CompareTo(GradientPoint other) {
			return this.Pos.CompareTo(other.Pos);
		}
	}

	[Serializable]
	public class GradientColor {
		//Variables
		public List<GradientPoint> Points;

		//Constructors
		public GradientColor() {
			Points = new List<GradientPoint>();
		}

		//Methods
		//Add a gradient point to this gradient object (Preferably between -1.0 to +1.0 position range).
		//It doesnt check for duplicate entries
		public void Add(double pos, Color col) {
			Points.Add(new GradientPoint(pos, col));
			Points.Sort();
		}

		//Remove gradient point at index position
		public void Remove(int i) {
			Points.RemoveAt(i);
		}

		public void Clear() {
			Points.Clear();
		}

		//Returns the color at the specified position in color gradient.
		public Color GetColor(double pos) {
			//Find first element in gradient point that has gradient pos larger.
			int i;
			for(i = 0; i < Points.Count; i++) {
				if(pos < Points[i].Pos) {
					break;
				}
			}
			//Find two nearest gradient points to do linear interpolation
			int index0 = _MathUtil._Clamp(i-1, 0, Points.Count-1);
			int index1 = _MathUtil._Clamp(i, 0, Points.Count-1);
			//If some gradient points are missing
			if(index0 == index1) {
				return Points[index1].Col;
			}
			//Compute rate value for linear interpolation
			float rate = (float)((pos - Points[index0].Pos) / (Points[index1].Pos - Points[index0].Pos));
			//Perform linear interpolation with the rate value
			return Color.Lerp(Points[index0].Col, Points[index1].Col, rate);
		}

		public void BuildGrayscaleGradient() {
			Clear();
			Add(-1.0, new Color32(0, 0, 0, 255));
			Add(0.0, new Color32(128, 128, 128, 255));
			Add(1.0, new Color32(255, 255, 255, 255));
		}

		public void BuildWaterDepthGradient() {
			Clear();
			Add(-1.0, new Color32(255, 255, 255, 255));
			Add(0.0, new Color32(0, 0, 0, 255));
			Add(1.0, new Color32(0, 0, 0, 255));
		}

		public void BuildTerrainGradient() {
			Clear();
			Add(-1.00, new Color32(0, 0, 128, 255));
			Add(-0.20, new Color32(32, 64, 128, 255));
			Add(-0.04, new Color32(64, 96, 192, 255));
			Add(-0.02, new Color32(192, 192, 128, 255));
			Add( 0.05, new Color32(0, 192, 0, 255));
			Add( 0.25, new Color32(192, 192, 0, 255));
			Add( 0.50, new Color32(160, 96, 64, 255));
			Add( 0.75, new Color32(128, 255, 255, 255));
			Add( 1.00, new Color32(255, 255, 255, 255));
		}

		public void BuildPlanetGradient() {
			Clear();
			Add(-1.00, new Color32(0, 0, 128, 255));
			Add(-0.25, new Color32(0, 0, 255, 255));
			Add( 0.00, new Color32(0, 128, 255, 255));
			Add( 0.06, new Color32(240, 240, 64, 255));
			Add( 0.12, new Color32(32, 160, 0, 255));
			Add( 0.37, new Color32(224, 224, 0, 255));
			Add( 0.75, new Color32(128, 128, 128, 255));
			Add( 1.00, new Color32(255, 255, 255, 255));
		}

		public void BuildIslandGradient() {
			Clear();
			Add(-1.00, new Color32(0, 0, 128, 255));
			Add(-0.25, new Color32(0, 0, 255, 255));
			Add( 0.00, new Color32(0, 128, 255, 255));
			Add( 0.01, new Color32(200, 200, 150, 255));
			Add( 0.15, new Color32(32, 160, 0, 255));
			Add( 0.50, new Color32(225, 225, 0, 255));
			Add( 0.90, new Color32(128, 100, 75, 255));
			Add( 1.00, new Color32(255, 255, 255, 255));
		}

		public void CopyFromUnityGradient(Gradient gradient) {
			Clear();
			for(int i = 0; i < gradient.colorKeys.Length; i++) {
				Color col = gradient.colorKeys[i].color;
				col.a = gradient.alphaKeys[i].alpha;
				Add(gradient.colorKeys[i].time * 2f - 1f, col);
			}
		}

		public static Gradient CopyToUnityGradient(GradientColor gradientColor) {
			int count = gradientColor.Points.Count;
			GradientColorKey[] gck = new GradientColorKey[count];
			GradientAlphaKey[] gak = new GradientAlphaKey[count];
			for(int i = 0; i < count; i++) {
				gck[i].time = (float)(gradientColor.Points[i].Pos * 0.5 + 0.5);
				gck[i].color = gradientColor.Points[i].Col;
				gak[i].time = gck[i].time;
				gak[i].alpha = gradientColor.Points[i].Col.a;
			}
			Gradient gradient = new Gradient();
			gradient.SetKeys(gck, gak);
			return gradient;
		}
	}

}