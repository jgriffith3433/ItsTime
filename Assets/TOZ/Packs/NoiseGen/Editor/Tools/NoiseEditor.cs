using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using TOZ;
using TOZ.Noise;
using TOZ.Noise.Model;
using TOZ.Noise.Module;
using TOZ.Noise.Utilities;

namespace TOZEditor {
#region Enums
	internal enum NoiseModuleType {
		Ball, BarsH, BarsV, BarsW, Billow, CheckerBoard, Constant, Cylinders,
		Dots, GradientX, GradientY, GradientZ, Perlin, RidgedMulti,
		Simplex, Spheres, Textured, Voronoi,
		Abs, Acosine, Add, Blend, BordersAverage, BordersExpand,
		Clamp, ColorBurn, ColorDodge, Curve, Displace, Divide,
		Exclusion, Extract, Invert, LinearBurn, Log, Min, Max, Multiply,
		Power, PowerOf, RotatePoint, Round,
		ScaleBias, ScalePoint, ScreenOverlay, Select, Subtract,
		Terrace, TranslatePoint, Turbulence, Test
	}

	internal enum NoiseGradientType {
		Custom, Grayscale, WaterDepth, Terrain, Planet, Island
	}

	internal enum NoiseMapType {
		Box, Cylinder, Plane, Sphere
	}
#endregion Enums

#region Data Class
	internal class NoiseModules {
		internal int id = -1;
		internal IModule module = null;
		internal System.Enum type = NoiseModuleType.Perlin;
		internal int mod0 = 0;
		internal int mod1 = 0;
		internal int mod2 = 0;
		internal int mod3 = 0;

		internal double frequency = 1.0;
		internal double lacunarity = 2.0;
		internal System.Enum quality = NoiseQuality.Normal;
		internal int octaves = 6;
		internal double persistence = 0.5;
		internal int roughness = 3;
		internal double power = 1.0;
		internal int seed = 0;
		internal double offset = 1.0;
		internal double gain = 2.0;
		internal double lowerBound = -1.0;
		internal double upperBound = 1.0;
		internal double tempVal1 = 0.0;
		internal double tempVal2 = 0.0;
		internal double tempVal3 = 0.0;
		internal bool tempBool1 = false;
		internal Texture2D tempTex1 = Texture2D.blackTexture;
		internal AnimationCurve tempCurve = new AnimationCurve(
														new Keyframe(-1.0f, -1.0f),
														new Keyframe(-0.5f, -0.5f),
														new Keyframe(0.0f, 0.0f),
														new Keyframe(0.5f, 0.5f),
														new Keyframe(1.0f, 1.0f)
		);
		internal bool clamped = true;
	}
#endregion Data Class

	public class NoiseEditor : EditorWindow {
#region Editor Window
		//Variables
		private static NoiseEditor window;
		private const int _x = 40;
		private const int _y = 40;
		private const int _col1 = 180;
		private const int _col2 = 504;
		private const int _row1 = 275;
		private const int _row2 = 275;
		private const int _winW = _col1 + _col2;
		private const int _winH = _row1 + _row2;

		[MenuItem("Window/TOZ/Tools/Noise Editor", false, 33)]
		private static void CreateWindow() {
			//Init window
			window = (NoiseEditor)GetWindow(typeof(NoiseEditor), true);
			window.position = new Rect(_x, _y, _winW, _winH);
			window.minSize = new Vector2(_winW, _winH);
			window.maxSize = new Vector2(_winW, _winH);
			window.titleContent = new GUIContent("Noise Editor");
			window.Show();
		}

		private static TextureEditor windowTextureEditor;

		private void ShowTextureWindow(string title, bool isNormalTexture, bool isPlanetTexture) {
			int width = (int)Mathf.Min(Mathf.Max(renderedTexture.width, 256), 512);
			int height = (int)Mathf.Min(Mathf.Max(renderedTexture.height, 256), 512);

			TextureEditor.Texture = renderedTexture;
			TextureEditor.Width = width;
			TextureEditor.Height = height;
			TextureEditor.IsNormalTexture = isNormalTexture;
			TextureEditor.IsPlanetTexture = isPlanetTexture;

			int x = _x + _winW + 10, y = _y + 20, w = width + 245, h = height + 8;
			windowTextureEditor = (TextureEditor)GetWindow(typeof(TextureEditor), true);
			windowTextureEditor.position = new Rect(x, y, w, h);
			windowTextureEditor.minSize = new Vector2(502, 280);
			windowTextureEditor.maxSize = new Vector2(758, 520);
			windowTextureEditor.titleContent = new GUIContent("Texture Editor");
			windowTextureEditor.ShowPopup();
		}
#endregion Editor Window

#region Variables
		//Variables
		private List<NoiseModules> modules;
		private List<string> modulesNames;
		private string[] modulesNamesArray;
		private int selectedModule;
		
		private bool togModules = true;
		private Vector2 scrollModules = Vector2.zero;
		private System.Enum popModules = NoiseModuleType.Perlin;

		private bool togGradient = true;
		private Vector2 scrollGradient = Vector2.zero;
		private System.Enum popGradient = NoiseGradientType.Grayscale;
		private GradientColor gradient;
		private double gradientPos = 0.0;
		private Color32 gradientColor = Color.white;

		private bool togModuleOptions = true;
		private Vector2 scrollModuleOptions = Vector2.zero;

		private bool togRenderOptions = true;
		private Vector2 scrollRenderOptions = Vector2.zero;
		private System.Enum popMaps = NoiseMapType.Plane;
		private int textureWidth = 256;
		private int textureHeight = 256;
		private int textureDepth = 16;
		private bool wrapEnabled = false;
		private bool seamlessEnabled = false;
		private double bumpHeight = 5.0;
		private double lowerXBound = -1.0;
		private double lowerYBound = -1.0;
		private double lowerZBound = -1.0;
		private double upperXBound = 1.0;
		private double upperYBound = 1.0;
		private double upperZBound = 1.0;
		private bool lightEnabled = false;
		private double lightAzimuth = 0.0;
		private double lightElevation = 45.0;
		private double lightBrightness = 2.0;
		private double lightContrast = 3.0;
		private Color32 lightColor = Color.white;
		private Texture2D renderedTexture;
#endregion Variables

#region Mono Methods
		private void OnEnable() {
			DoEnable();
		}

		private void OnDisable() {
			DoDisable();
		}

		private void OnGUI() {
			DoGUI();
		}
#endregion Mono Methods

#region Methods
		private void DoEnable() {
			modules = new List<NoiseModules>();
			modulesNames = new List<string>();
			//selectedModule = 0;
			AddModule();

			gradient = new GradientColor();
			gradient.BuildGrayscaleGradient();
		}

		private void DoDisable() {
			modules = null;
			modulesNames = null;
			modulesNamesArray = null;

			//DO NOT REMOVE
			//AdsView.CreateWindow();
		}

		private void DoGUI() {
#region Window
			//Whole window start
			EditorGUILayout.BeginHorizontal(GUILayout.Width(_winW), GUILayout.Height(_winH-5));

#region First Column
			//First Column start
			EditorGUILayout.BeginVertical(GUILayout.Width(_col1), GUILayout.Height(_winH));
#region Modules
			//Modules start
			EditorGUILayout.BeginVertical("box", GUILayout.MaxHeight(_row1));

			togModules = EditorGUILayout.Foldout(togModules, "Modules(.Type)");
			if(togModules) {
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.BeginHorizontal();
				if(GUILayout.Button("Add")) {
					AddModule();
				}
				if(GUILayout.Button("Del")) {
					RemoveModule();
				}
				if(GUILayout.Button("Clear")) {
					ClearModules();
				}
				EditorGUILayout.EndHorizontal();

				scrollModules = EditorGUILayout.BeginScrollView(scrollModules);
				selectedModule = GUILayout.SelectionGrid(selectedModule, modulesNamesArray, 1);
				EditorGUILayout.EndScrollView();
		
				if(EditorGUI.EndChangeCheck()) {
					EditorGUI.FocusTextInControl(string.Empty);
					popModules = modules[selectedModule].type;
				}
			}

			//Modules end
			EditorGUILayout.EndVertical();
#endregion Modules

#region Gradient
			//Gradients start
			EditorGUILayout.BeginVertical("box", GUILayout.MaxHeight(_row2));

			togGradient = EditorGUILayout.Foldout(togGradient, "Gradient");
			if(togGradient) {
				EditorGUI.BeginChangeCheck();
				popGradient = (NoiseGradientType)EditorGUILayout.EnumPopup(popGradient);
				if(GUILayout.Button("Clear")) {
					gradient.Clear();
					if(popGradient.IsNot(NoiseGradientType.Custom))
						popGradient = NoiseGradientType.Custom;
				}
				if(EditorGUI.EndChangeCheck()) {
					EditorGUI.FocusTextInControl(string.Empty);
					switch((NoiseGradientType)popGradient) {
						case NoiseGradientType.Custom: {
							gradientPos = 0;
							gradientColor = Color.white;
						}
						break;
						case NoiseGradientType.Grayscale: {
							gradient.BuildGrayscaleGradient();
						}
						break;
						case NoiseGradientType.WaterDepth: {
							gradient.BuildWaterDepthGradient();
						}
						break;
						case NoiseGradientType.Terrain: {
							gradient.BuildTerrainGradient();
						}
						break;
						case NoiseGradientType.Planet: {
							gradient.BuildPlanetGradient();
						}
						break;
						case NoiseGradientType.Island: {
							gradient.BuildIslandGradient();
						}
						break;
						default: {
							Debug.LogError("Error on Gradient type selection.");
						}
						break;
					}
				}

				if((NoiseGradientType)popGradient == NoiseGradientType.Custom) {
					EditorGUILayout.BeginHorizontal();
					gradientPos = EditorGUILayout.DoubleField(gradientPos, GUILayout.MaxWidth(40));
					gradientColor = EditorGUILayout.ColorField(gradientColor);
					
					if(GUILayout.Button("Add", GUILayout.MaxWidth(40))) {
						gradient.Add(gradientPos, gradientColor);
						EditorGUI.FocusTextInControl(string.Empty);
					}
					EditorGUILayout.EndHorizontal();
				}
				if(GUILayout.Button("Sort")) {
					gradient.Points.Sort();
					EditorGUI.FocusTextInControl(string.Empty);
				}

				scrollGradient = EditorGUILayout.BeginScrollView(scrollGradient);
				EditorGUI.BeginChangeCheck();
				for(int i = 0; i < gradient.Points.Count; i++) {
					EditorGUILayout.BeginHorizontal();
					if(popGradient.Is(NoiseGradientType.Custom))
						gradient.Points[i].Pos = EditorGUILayout.DoubleField(gradient.Points[i].Pos, GUILayout.MaxWidth(40));
					else
						EditorGUILayout.LabelField(gradient.Points[i].Pos.ToString(), GUILayout.MaxWidth(40));
					gradient.Points[i].Col = EditorGUILayout.ColorField(gradient.Points[i].Col);
					if(GUILayout.Button("Del", GUILayout.MaxWidth(40))) {
						gradient.Remove(i);
					}
					EditorGUILayout.EndHorizontal();
				}
				if(EditorGUI.EndChangeCheck()) {
					if(popGradient.IsNot(NoiseGradientType.Custom))
						popGradient = NoiseGradientType.Custom;
				}
				EditorGUILayout.EndScrollView();
			}

			//Gradients end
			EditorGUILayout.EndVertical();
#endregion Gradient
			//First Column end
			EditorGUILayout.EndVertical();
#endregion First Column

#region Second Column
			//Second Column start
			EditorGUILayout.BeginVertical(GUILayout.Width(_col2), GUILayout.Height(_winH));
#region Module Options
			//Module Options start
			EditorGUILayout.BeginVertical("box", GUILayout.MaxHeight(_row1));

			togModuleOptions = EditorGUILayout.Foldout(togModuleOptions, "Module ID(" + modules[selectedModule].id.ToString() + ") Options");
			if(togModuleOptions) {
				if(modules.Count > 0) {
					scrollModuleOptions = EditorGUILayout.BeginScrollView(scrollModuleOptions);

					EditorGUI.BeginChangeCheck();
					popModules = (NoiseModuleType)EditorGUILayout.EnumPopup("Module Type: ", popModules);

					switch((NoiseModuleType)popModules) {
						case NoiseModuleType.Ball : {
							modules[selectedModule].frequency = EditorGUILayout.DoubleField("Frequency: ", modules[selectedModule].frequency);
							modules[selectedModule].tempBool1 = EditorGUILayout.Toggle("Invert: ", modules[selectedModule].tempBool1);
						}
						break;
						case NoiseModuleType.BarsH: {
							modules[selectedModule].frequency = EditorGUILayout.DoubleField("Frequency: ", modules[selectedModule].frequency);
						}
						break;
						case NoiseModuleType.BarsV: {
							modules[selectedModule].frequency = EditorGUILayout.DoubleField("Frequency: ", modules[selectedModule].frequency);
						}
						break;
						case NoiseModuleType.BarsW: {
							modules[selectedModule].frequency = EditorGUILayout.DoubleField("Frequency: ", modules[selectedModule].frequency);
						}
						break;
						case NoiseModuleType.Billow: {
							modules[selectedModule].frequency = EditorGUILayout.DoubleField("Frequency: ", modules[selectedModule].frequency);
							modules[selectedModule].lacunarity = EditorGUILayout.DoubleField("Lacunarity: ", modules[selectedModule].lacunarity);
							modules[selectedModule].quality = EditorGUILayout.EnumPopup("Quality: ", modules[selectedModule].quality);
							modules[selectedModule].octaves = EditorGUILayout.IntField("Octaves: ", modules[selectedModule].octaves);
							modules[selectedModule].persistence = EditorGUILayout.DoubleField("Persistence: ", modules[selectedModule].persistence);
							modules[selectedModule].seed = EditorGUILayout.IntField("Seed: ", modules[selectedModule].seed);
							modules[selectedModule].clamped = EditorGUILayout.Toggle("Return clamped value: ", modules[selectedModule].clamped);
						}
						break;
						case NoiseModuleType.CheckerBoard: {
						}
						break;
						case NoiseModuleType.Constant: {
							modules[selectedModule].tempVal1 = EditorGUILayout.DoubleField("Constant Value: ", modules[selectedModule].tempVal1);
						}
						break;
						case NoiseModuleType.Cylinders: {
							modules[selectedModule].frequency = EditorGUILayout.DoubleField("Frequency: ", modules[selectedModule].frequency);
						}
						break;
						case NoiseModuleType.Dots: {
							modules[selectedModule].frequency = EditorGUILayout.DoubleField("Frequency: ", modules[selectedModule].frequency);
						}
						break;
						case NoiseModuleType.GradientX: {
							modules[selectedModule].tempVal1 = EditorGUILayout.DoubleField("Start: ", modules[selectedModule].tempVal1);
							modules[selectedModule].tempVal2 = EditorGUILayout.DoubleField("End: ", modules[selectedModule].tempVal2);	
							modules[selectedModule].tempBool1 = EditorGUILayout.Toggle("Invert: ", modules[selectedModule].tempBool1);
						}
						break;
						case NoiseModuleType.GradientY: {
							modules[selectedModule].tempVal1 = EditorGUILayout.DoubleField("Start: ", modules[selectedModule].tempVal1);
							modules[selectedModule].tempVal2 = EditorGUILayout.DoubleField("End: ", modules[selectedModule].tempVal2);
							modules[selectedModule].tempBool1 = EditorGUILayout.Toggle("Invert: ", modules[selectedModule].tempBool1);
						}
						break;
						case NoiseModuleType.GradientZ: {
							modules[selectedModule].tempVal1 = EditorGUILayout.DoubleField("Start: ", modules[selectedModule].tempVal1);
							modules[selectedModule].tempVal2 = EditorGUILayout.DoubleField("End: ", modules[selectedModule].tempVal2);
							modules[selectedModule].tempBool1 = EditorGUILayout.Toggle("Invert: ", modules[selectedModule].tempBool1);
						}
						break;
						case NoiseModuleType.Perlin: {
							modules[selectedModule].frequency = EditorGUILayout.DoubleField("Frequency: ", modules[selectedModule].frequency);
							modules[selectedModule].lacunarity = EditorGUILayout.DoubleField("Lacunarity: ", modules[selectedModule].lacunarity);
							modules[selectedModule].quality = EditorGUILayout.EnumPopup("NoiseQuality: ", modules[selectedModule].quality);
							modules[selectedModule].octaves = EditorGUILayout.IntField("OctaveCount: ", modules[selectedModule].octaves);
							modules[selectedModule].persistence = EditorGUILayout.DoubleField("Persistence: ", modules[selectedModule].persistence);
							modules[selectedModule].seed = EditorGUILayout.IntField("Seed: ", modules[selectedModule].seed);
							modules[selectedModule].clamped = EditorGUILayout.Toggle("Return clamped value: ", modules[selectedModule].clamped);
						}
						break;
						case NoiseModuleType.RidgedMulti: {
							modules[selectedModule].frequency = EditorGUILayout.DoubleField("Frequency: ", modules[selectedModule].frequency);
							modules[selectedModule].lacunarity = EditorGUILayout.DoubleField("Lacunarity: ", modules[selectedModule].lacunarity);
							modules[selectedModule].quality = EditorGUILayout.EnumPopup("NoiseQuality: ", modules[selectedModule].quality);
							modules[selectedModule].octaves = EditorGUILayout.IntField("OctaveCount: ", modules[selectedModule].octaves);
							modules[selectedModule].seed = EditorGUILayout.IntField("Seed: ", modules[selectedModule].seed);
							modules[selectedModule].offset = EditorGUILayout.DoubleField("Offset: ", modules[selectedModule].offset);
							modules[selectedModule].gain = EditorGUILayout.DoubleField("Gain: ", modules[selectedModule].gain);
							modules[selectedModule].clamped = EditorGUILayout.Toggle("Return clamped value: ", modules[selectedModule].clamped);
						}
						break;
						case NoiseModuleType.Simplex: {
							modules[selectedModule].frequency = EditorGUILayout.DoubleField("Frequency: ", modules[selectedModule].frequency);
							modules[selectedModule].lacunarity = EditorGUILayout.DoubleField("Lacunarity: ", modules[selectedModule].lacunarity);
							modules[selectedModule].octaves = EditorGUILayout.IntField("OctaveCount: ", modules[selectedModule].octaves);
							modules[selectedModule].persistence = EditorGUILayout.DoubleField("Persistence: ", modules[selectedModule].persistence);
							modules[selectedModule].seed = EditorGUILayout.IntField("Seed: ", modules[selectedModule].seed);
							modules[selectedModule].clamped = EditorGUILayout.Toggle("Return clamped value: ", modules[selectedModule].clamped);
						}
						break;
						case NoiseModuleType.Spheres: {
							modules[selectedModule].frequency = EditorGUILayout.DoubleField("Frequency: ", modules[selectedModule].frequency);
						}
						break;
						case NoiseModuleType.Textured: {
							modules[selectedModule].tempTex1 = (Texture2D)EditorGUILayout.ObjectField("Texture: ", modules[selectedModule].tempTex1, typeof(Texture2D), false);
						}
						break;
						case NoiseModuleType.Voronoi: {
							modules[selectedModule].frequency = EditorGUILayout.DoubleField("Frequency: ", modules[selectedModule].frequency);
							modules[selectedModule].tempVal1 = EditorGUILayout.DoubleField("Displacement: ", modules[selectedModule].tempVal1);
							modules[selectedModule].tempBool1 = EditorGUILayout.Toggle("Enable Distance: ", modules[selectedModule].tempBool1);
							modules[selectedModule].seed = EditorGUILayout.IntField("Seed: ", modules[selectedModule].seed);
							modules[selectedModule].clamped = EditorGUILayout.Toggle("Return clamped value: ", modules[selectedModule].clamped);
						}
						break;
						case NoiseModuleType.Abs: {
							modules[selectedModule].mod0 = EditorGUILayout.Popup("Source Module0: ", modules[selectedModule].mod0, modulesNamesArray);
						}
						break;
						case NoiseModuleType.Acosine: {
							modules[selectedModule].mod0 = EditorGUILayout.Popup("Source Module0: ", modules[selectedModule].mod0, modulesNamesArray);
							modules[selectedModule].tempBool1 = EditorGUILayout.Toggle("Invert: ", modules[selectedModule].tempBool1);
						}
						break;
						case NoiseModuleType.Add: {
							modules[selectedModule].mod0 = EditorGUILayout.Popup("Source Module0: ", modules[selectedModule].mod0, modulesNamesArray);
							modules[selectedModule].mod1 = EditorGUILayout.Popup("Source Module1: ", modules[selectedModule].mod1, modulesNamesArray);
							modules[selectedModule].clamped = EditorGUILayout.Toggle("Return clamped value: ", modules[selectedModule].clamped);
						}
						break;
						case NoiseModuleType.Blend: {
							modules[selectedModule].mod0 = EditorGUILayout.Popup("Source Module0: ", modules[selectedModule].mod0, modulesNamesArray);
							modules[selectedModule].mod1 = EditorGUILayout.Popup("Source Module1: ", modules[selectedModule].mod1, modulesNamesArray);
							modules[selectedModule].mod2 = EditorGUILayout.Popup("Control Module: ", modules[selectedModule].mod2, modulesNamesArray);
						}
						break;
						case NoiseModuleType.BordersAverage: {
							modules[selectedModule].mod0 = EditorGUILayout.Popup("Source Module0: ", modules[selectedModule].mod0, modulesNamesArray);
							modules[selectedModule].lowerBound = EditorGUILayout.DoubleField("Lower Bound: ", modules[selectedModule].lowerBound);
							modules[selectedModule].upperBound = EditorGUILayout.DoubleField("Upper Bound: ", modules[selectedModule].upperBound);	
						}
						break;
						case NoiseModuleType.BordersExpand: {
							modules[selectedModule].mod0 = EditorGUILayout.Popup("Source Module0: ", modules[selectedModule].mod0, modulesNamesArray);
							modules[selectedModule].lowerBound = EditorGUILayout.DoubleField("Lower Bound: ", modules[selectedModule].lowerBound);
							modules[selectedModule].tempVal1 = EditorGUILayout.DoubleField("Border: ", modules[selectedModule].tempVal1);
							modules[selectedModule].upperBound = EditorGUILayout.DoubleField("Upper Bound: ", modules[selectedModule].upperBound);	
						}
						break;
						case NoiseModuleType.Clamp: {
							modules[selectedModule].mod0 = EditorGUILayout.Popup("Source Module0: ", modules[selectedModule].mod0, modulesNamesArray);
							modules[selectedModule].lowerBound = EditorGUILayout.DoubleField("Lower Bound: ", modules[selectedModule].lowerBound);
							modules[selectedModule].upperBound = EditorGUILayout.DoubleField("Upper Bound: ", modules[selectedModule].upperBound);	
						}
						break;
						case NoiseModuleType.ColorBurn: {
							modules[selectedModule].mod0 = EditorGUILayout.Popup("Source Module0: ", modules[selectedModule].mod0, modulesNamesArray);
							modules[selectedModule].mod1 = EditorGUILayout.Popup("Source Module1: ", modules[selectedModule].mod1, modulesNamesArray);
						}
						break;
						case NoiseModuleType.ColorDodge: {
							modules[selectedModule].mod0 = EditorGUILayout.Popup("Source Module0: ", modules[selectedModule].mod0, modulesNamesArray);
							modules[selectedModule].mod1 = EditorGUILayout.Popup("Source Module1: ", modules[selectedModule].mod1, modulesNamesArray);
						}
						break;
						case NoiseModuleType.Curve: {
							modules[selectedModule].mod0 = EditorGUILayout.Popup("Source Module0: ", modules[selectedModule].mod0, modulesNamesArray);
							modules[selectedModule].tempCurve = EditorGUILayout.CurveField("Curve: ", modules[selectedModule].tempCurve);
						}
						break;
						case NoiseModuleType.Displace: {
							modules[selectedModule].mod0 = EditorGUILayout.Popup("Source Module0: ", modules[selectedModule].mod0, modulesNamesArray);
							modules[selectedModule].mod1 = EditorGUILayout.Popup("XDisplace Module: ", modules[selectedModule].mod1, modulesNamesArray);
							modules[selectedModule].mod2 = EditorGUILayout.Popup("YDisplace Module: ", modules[selectedModule].mod2, modulesNamesArray);
							modules[selectedModule].mod3 = EditorGUILayout.Popup("ZDisplace Module: ", modules[selectedModule].mod3, modulesNamesArray);
							modules[selectedModule].clamped = EditorGUILayout.Toggle("Return clamped value: ", modules[selectedModule].clamped);
						}
						break;
						case NoiseModuleType.Divide: {
							modules[selectedModule].mod0 = EditorGUILayout.Popup("Source Module0: ", modules[selectedModule].mod0, modulesNamesArray);
							modules[selectedModule].mod1 = EditorGUILayout.Popup("Source Module1: ", modules[selectedModule].mod1, modulesNamesArray);
							modules[selectedModule].clamped = EditorGUILayout.Toggle("Return clamped value: ", modules[selectedModule].clamped);
						}
						break;
						case NoiseModuleType.Exclusion: {
							modules[selectedModule].mod0 = EditorGUILayout.Popup("Source Module0: ", modules[selectedModule].mod0, modulesNamesArray);
							modules[selectedModule].mod1 = EditorGUILayout.Popup("Source Module1: ", modules[selectedModule].mod1, modulesNamesArray);
						}
						break;
						case NoiseModuleType.Extract: {
							modules[selectedModule].mod0 = EditorGUILayout.Popup("Source Module0: ", modules[selectedModule].mod0, modulesNamesArray);
							modules[selectedModule].lowerBound = EditorGUILayout.DoubleField("Lower Bound: ", modules[selectedModule].lowerBound);
							modules[selectedModule].upperBound = EditorGUILayout.DoubleField("Upper Bound: ", modules[selectedModule].upperBound);	
						}
						break;
						case NoiseModuleType.Invert: {
							modules[selectedModule].mod0 = EditorGUILayout.Popup("Source Module0: ", modules[selectedModule].mod0, modulesNamesArray);
						}
						break;
						case NoiseModuleType.LinearBurn: {
							modules[selectedModule].mod0 = EditorGUILayout.Popup("Source Module0: ", modules[selectedModule].mod0, modulesNamesArray);
							modules[selectedModule].mod1 = EditorGUILayout.Popup("Source Module1: ", modules[selectedModule].mod1, modulesNamesArray);
						}
						break;
						case NoiseModuleType.Log: {
							modules[selectedModule].mod0 = EditorGUILayout.Popup("Source Module0: ", modules[selectedModule].mod0, modulesNamesArray);
							modules[selectedModule].tempVal1 = EditorGUILayout.DoubleField("Value: ", modules[selectedModule].tempVal1);
							modules[selectedModule].clamped = EditorGUILayout.Toggle("Return clamped value: ", modules[selectedModule].clamped);
						}
						break;
						case NoiseModuleType.Max: {
							modules[selectedModule].mod0 = EditorGUILayout.Popup("Source Module0: ", modules[selectedModule].mod0, modulesNamesArray);
							modules[selectedModule].mod1 = EditorGUILayout.Popup("Source Module1: ", modules[selectedModule].mod1, modulesNamesArray);
						}
						break;
						case NoiseModuleType.Min: {
							modules[selectedModule].mod0 = EditorGUILayout.Popup("Source Module0: ", modules[selectedModule].mod0, modulesNamesArray);
							modules[selectedModule].mod1 = EditorGUILayout.Popup("Source Module1: ", modules[selectedModule].mod1, modulesNamesArray);
						}
						break;
						case NoiseModuleType.Multiply: {
							modules[selectedModule].mod0 = EditorGUILayout.Popup("Source Module0: ", modules[selectedModule].mod0, modulesNamesArray);
							modules[selectedModule].mod1 = EditorGUILayout.Popup("Source Module1: ", modules[selectedModule].mod1, modulesNamesArray);
							modules[selectedModule].clamped = EditorGUILayout.Toggle("Return clamped value: ", modules[selectedModule].clamped);
						}
						break;
						case NoiseModuleType.Power: {
							modules[selectedModule].mod0 = EditorGUILayout.Popup("Source Module0: ", modules[selectedModule].mod0, modulesNamesArray);
							modules[selectedModule].mod1 = EditorGUILayout.Popup("Source Module1: ", modules[selectedModule].mod1, modulesNamesArray);
							modules[selectedModule].clamped = EditorGUILayout.Toggle("Return clamped value: ", modules[selectedModule].clamped);
						}
						break;
						case NoiseModuleType.PowerOf: {
							modules[selectedModule].mod0 = EditorGUILayout.Popup("Source Module0: ", modules[selectedModule].mod0, modulesNamesArray);
							modules[selectedModule].frequency = EditorGUILayout.DoubleField("Frequency: ", modules[selectedModule].frequency);
							modules[selectedModule].clamped = EditorGUILayout.Toggle("Return clamped value: ", modules[selectedModule].clamped);
						}
						break;
						case NoiseModuleType.RotatePoint: {
							modules[selectedModule].mod0 = EditorGUILayout.Popup("Source Module0: ", modules[selectedModule].mod0, modulesNamesArray);
							modules[selectedModule].tempVal1 = EditorGUILayout.DoubleField("X Angle: ", modules[selectedModule].tempVal1);
							modules[selectedModule].tempVal2 = EditorGUILayout.DoubleField("Y Angle: ", modules[selectedModule].tempVal2);
							modules[selectedModule].tempVal3 = EditorGUILayout.DoubleField("Z Angle: ", modules[selectedModule].tempVal3);
							modules[selectedModule].clamped = EditorGUILayout.Toggle("Return clamped value: ", modules[selectedModule].clamped);
						}
						break;
						case NoiseModuleType.Round: {
							modules[selectedModule].mod0 = EditorGUILayout.Popup("Source Module0: ", modules[selectedModule].mod0, modulesNamesArray);
						}
						break;
						case NoiseModuleType.ScaleBias: {
							modules[selectedModule].mod0 = EditorGUILayout.Popup("Source Module0: ", modules[selectedModule].mod0, modulesNamesArray);
							modules[selectedModule].tempVal1 = EditorGUILayout.DoubleField("Scale: ", modules[selectedModule].tempVal1);
							modules[selectedModule].tempVal2 = EditorGUILayout.DoubleField("Bias: ", modules[selectedModule].tempVal2);
							modules[selectedModule].clamped = EditorGUILayout.Toggle("Return clamped value: ", modules[selectedModule].clamped);
						}
						break;
						case NoiseModuleType.ScalePoint: {
							modules[selectedModule].mod0 = EditorGUILayout.Popup("Source Module0: ", modules[selectedModule].mod0, modulesNamesArray);
							modules[selectedModule].tempVal1 = EditorGUILayout.DoubleField("X Scale: ", modules[selectedModule].tempVal1);
							modules[selectedModule].tempVal2 = EditorGUILayout.DoubleField("Y Scale: ", modules[selectedModule].tempVal2);
							modules[selectedModule].tempVal3 = EditorGUILayout.DoubleField("Z Scale: ", modules[selectedModule].tempVal3);
							modules[selectedModule].clamped = EditorGUILayout.Toggle("Return clamped value: ", modules[selectedModule].clamped);
						}
						break;
						case NoiseModuleType.ScreenOverlay: {
							modules[selectedModule].mod0 = EditorGUILayout.Popup("Source Module0: ", modules[selectedModule].mod0, modulesNamesArray);
							modules[selectedModule].mod1 = EditorGUILayout.Popup("Source Module1: ", modules[selectedModule].mod1, modulesNamesArray);
						}
						break;
						case NoiseModuleType.Select: {
							modules[selectedModule].mod0 = EditorGUILayout.Popup("Source Module0: ", modules[selectedModule].mod0, modulesNamesArray);
							modules[selectedModule].mod1 = EditorGUILayout.Popup("Source Module1: ", modules[selectedModule].mod1, modulesNamesArray);
							modules[selectedModule].mod2 = EditorGUILayout.Popup("Control Module: ", modules[selectedModule].mod2, modulesNamesArray);
							modules[selectedModule].lowerBound = EditorGUILayout.DoubleField("Lower Bound: ", modules[selectedModule].lowerBound);
							modules[selectedModule].upperBound = EditorGUILayout.DoubleField("Upper Bound: ", modules[selectedModule].upperBound);	
							modules[selectedModule].tempVal1 = EditorGUILayout.DoubleField("Edge Falloff: ", modules[selectedModule].tempVal1);
						}
						break;
						case NoiseModuleType.Subtract: {
							modules[selectedModule].mod0 = EditorGUILayout.Popup("Source Module0: ", modules[selectedModule].mod0, modulesNamesArray);
							modules[selectedModule].mod1 = EditorGUILayout.Popup("Source Module1: ", modules[selectedModule].mod1, modulesNamesArray);
							modules[selectedModule].clamped = EditorGUILayout.Toggle("Return clamped value: ", modules[selectedModule].clamped);
						}
						break;
						case NoiseModuleType.Terrace: {
							modules[selectedModule].mod0 = EditorGUILayout.Popup("Source Module0: ", modules[selectedModule].mod0, modulesNamesArray);
							modules[selectedModule].tempCurve = EditorGUILayout.CurveField("Curve: ", modules[selectedModule].tempCurve);
							modules[selectedModule].tempVal1 = EditorGUILayout.DoubleField("Thickness: ", modules[selectedModule].tempVal1);
							modules[selectedModule].tempBool1 = EditorGUILayout.Toggle("Invert Terraces: ", modules[selectedModule].tempBool1);
						}
						break;
						case NoiseModuleType.TranslatePoint: {
							modules[selectedModule].mod0 = EditorGUILayout.Popup("Source Module0: ", modules[selectedModule].mod0, modulesNamesArray);
							modules[selectedModule].tempVal1 = EditorGUILayout.DoubleField("X Translation: ", modules[selectedModule].tempVal1);
							modules[selectedModule].tempVal2 = EditorGUILayout.DoubleField("Y Translation: ", modules[selectedModule].tempVal2);
							modules[selectedModule].tempVal3 = EditorGUILayout.DoubleField("Z Translation: ", modules[selectedModule].tempVal3);
							modules[selectedModule].clamped = EditorGUILayout.Toggle("Return clamped value: ", modules[selectedModule].clamped);
						}
						break;
						case NoiseModuleType.Turbulence: {
							modules[selectedModule].mod0 = EditorGUILayout.Popup("Source Module0: ", modules[selectedModule].mod0, modulesNamesArray);
							modules[selectedModule].power = EditorGUILayout.DoubleField("Power: ", modules[selectedModule].power);
							modules[selectedModule].frequency = EditorGUILayout.DoubleField("Frequency: ", modules[selectedModule].frequency);
							modules[selectedModule].roughness = EditorGUILayout.IntField("Roughness: ", modules[selectedModule].roughness);
							modules[selectedModule].seed = EditorGUILayout.IntField("Seed: ", modules[selectedModule].seed);
							modules[selectedModule].clamped = EditorGUILayout.Toggle("Return clamped value: ", modules[selectedModule].clamped);
						}
						break;
						case NoiseModuleType.Test: {
							modules[selectedModule].tempVal1 = EditorGUILayout.DoubleField("Start: ", modules[selectedModule].tempVal1);
							modules[selectedModule].tempVal2 = EditorGUILayout.DoubleField("End: ", modules[selectedModule].tempVal2);	
							modules[selectedModule].tempBool1 = EditorGUILayout.Toggle("Invert: ", modules[selectedModule].tempBool1);
							modules[selectedModule].clamped = EditorGUILayout.Toggle("Return clamped value: ", modules[selectedModule].clamped);
						}
						break;
					}

					if(EditorGUI.EndChangeCheck()) {
						modules[selectedModule].type = popModules;
						string name = modules[selectedModule].type.ToString();
						modulesNames[selectedModule] = ("{0:d} - " + name).FormatWith(modules[selectedModule].id);
						modulesNamesArray = modulesNames.ToArray();
					}

					switch((NoiseModuleType)popModules) {
						case NoiseModuleType.Ball: {
							modules[selectedModule].module = new Ball();
							((Ball)modules[selectedModule].module).Frequency = modules[selectedModule].frequency;
							((Ball)modules[selectedModule].module).Invert = modules[selectedModule].tempBool1;
						}
						break;
						case NoiseModuleType.BarsH: {
							modules[selectedModule].module = new BarsH();
							((BarsH)modules[selectedModule].module).LowerBound = lowerYBound;
							((BarsH)modules[selectedModule].module).UpperBound = upperYBound;
							((BarsH)modules[selectedModule].module).Frequency = modules[selectedModule].frequency;
						}
						break;
						case NoiseModuleType.BarsV: {
							modules[selectedModule].module = new BarsV();
							((BarsV)modules[selectedModule].module).LowerBound = lowerXBound;
							((BarsV)modules[selectedModule].module).UpperBound = upperXBound;
							((BarsV)modules[selectedModule].module).Frequency = modules[selectedModule].frequency;
						}
						break;
						case NoiseModuleType.BarsW: {
							modules[selectedModule].module = new BarsV();
							((BarsV)modules[selectedModule].module).LowerBound = lowerXBound;
							((BarsV)modules[selectedModule].module).UpperBound = upperXBound;
							((BarsV)modules[selectedModule].module).Frequency = modules[selectedModule].frequency;
						}
						break;
						case NoiseModuleType.Billow: {
							modules[selectedModule].module = new Billow();
							((Billow)modules[selectedModule].module).Frequency = modules[selectedModule].frequency;
							((Billow)modules[selectedModule].module).Lacunarity = modules[selectedModule].lacunarity;
							((Billow)modules[selectedModule].module).Quality = (NoiseQuality)modules[selectedModule].quality;
							((Billow)modules[selectedModule].module).Octaves = modules[selectedModule].octaves;
							((Billow)modules[selectedModule].module).Persistence = modules[selectedModule].persistence;
							((Billow)modules[selectedModule].module).Seed = modules[selectedModule].seed;
							((Billow)modules[selectedModule].module).Clamped = modules[selectedModule].clamped;
						}
						break;
						case NoiseModuleType.CheckerBoard: {
							modules[selectedModule].module = new CheckerBoard();
						}
						break;
						case NoiseModuleType.Constant: {
							modules[selectedModule].module = new Constant();
							((Constant)modules[selectedModule].module).Value = modules[selectedModule].tempVal1;
						}
						break;
						case NoiseModuleType.Cylinders: {
							modules[selectedModule].module = new Cylinders();
							((Cylinders)modules[selectedModule].module).Frequency = modules[selectedModule].frequency;
						}
						break;
						case NoiseModuleType.Dots: {
							modules[selectedModule].module = new Dots();
							((Dots)modules[selectedModule].module).Frequency = modules[selectedModule].frequency;
						}
						break;
						case NoiseModuleType.GradientX: {
							modules[selectedModule].module = new GradientX();
							((GradientX)modules[selectedModule].module).Start = modules[selectedModule].tempVal1;
							((GradientX)modules[selectedModule].module).End = modules[selectedModule].tempVal2;
							((GradientX)modules[selectedModule].module).LowerBound = lowerXBound;
							((GradientX)modules[selectedModule].module).UpperBound = upperXBound;
							((GradientX)modules[selectedModule].module).Invert = modules[selectedModule].tempBool1;
						}
						break;
						case NoiseModuleType.GradientY: {
							modules[selectedModule].module = new GradientY();
							((GradientY)modules[selectedModule].module).Start = modules[selectedModule].tempVal1;
							((GradientY)modules[selectedModule].module).End = modules[selectedModule].tempVal2;
							((GradientY)modules[selectedModule].module).LowerBound = lowerYBound;
							((GradientY)modules[selectedModule].module).UpperBound = upperYBound;
							((GradientY)modules[selectedModule].module).Invert = modules[selectedModule].tempBool1;
						}
						break;
						case NoiseModuleType.GradientZ: {
							modules[selectedModule].module = new GradientZ();
							((GradientZ)modules[selectedModule].module).Start = modules[selectedModule].tempVal1;
							((GradientZ)modules[selectedModule].module).End = modules[selectedModule].tempVal2;
							((GradientZ)modules[selectedModule].module).LowerBound = lowerYBound;
							((GradientZ)modules[selectedModule].module).UpperBound = upperYBound;
							((GradientZ)modules[selectedModule].module).Invert = modules[selectedModule].tempBool1;
						}
						break;
						case NoiseModuleType.Perlin: {
							modules[selectedModule].module = new Perlin();
							((Perlin)modules[selectedModule].module).Frequency = modules[selectedModule].frequency;
							((Perlin)modules[selectedModule].module).Lacunarity = modules[selectedModule].lacunarity;
							((Perlin)modules[selectedModule].module).Quality = (NoiseQuality)modules[selectedModule].quality;
							((Perlin)modules[selectedModule].module).Octaves = modules[selectedModule].octaves;
							((Perlin)modules[selectedModule].module).Persistence = modules[selectedModule].persistence;
							((Perlin)modules[selectedModule].module).Seed = modules[selectedModule].seed;
							((Perlin)modules[selectedModule].module).Clamped = modules[selectedModule].clamped;
						}
						break;
						case NoiseModuleType.RidgedMulti: {
							modules[selectedModule].module = new RidgedMulti();
							((RidgedMulti)modules[selectedModule].module).Frequency = modules[selectedModule].frequency;
							((RidgedMulti)modules[selectedModule].module).Lacunarity = modules[selectedModule].lacunarity;
							((RidgedMulti)modules[selectedModule].module).Quality = (NoiseQuality)modules[selectedModule].quality;
							((RidgedMulti)modules[selectedModule].module).Octaves = modules[selectedModule].octaves;
							((RidgedMulti)modules[selectedModule].module).Seed = modules[selectedModule].seed;
							((RidgedMulti)modules[selectedModule].module).Offset = modules[selectedModule].offset;
							((RidgedMulti)modules[selectedModule].module).Gain = modules[selectedModule].gain;
							((RidgedMulti)modules[selectedModule].module).Clamped = modules[selectedModule].clamped;
						}
						break;
						case NoiseModuleType.Simplex: {
							modules[selectedModule].module = new Simplex();
							((Simplex)modules[selectedModule].module).Frequency = modules[selectedModule].frequency;
							((Simplex)modules[selectedModule].module).Lacunarity = modules[selectedModule].lacunarity;
							((Simplex)modules[selectedModule].module).Octaves = modules[selectedModule].octaves;
							((Simplex)modules[selectedModule].module).Persistence = modules[selectedModule].persistence;
							((Simplex)modules[selectedModule].module).Seed = modules[selectedModule].seed;
							((Simplex)modules[selectedModule].module).Clamped = modules[selectedModule].clamped;
						}
						break;
						case NoiseModuleType.Spheres: {
							modules[selectedModule].module = new Spheres();
							((Spheres)modules[selectedModule].module).Frequency = modules[selectedModule].frequency;
						}
						break;
						case NoiseModuleType.Textured: {
							modules[selectedModule].module = new Textured();
							((Textured)modules[selectedModule].module).LowerXBound = lowerXBound;
							((Textured)modules[selectedModule].module).UpperXBound = upperXBound;
							((Textured)modules[selectedModule].module).LowerYBound = lowerYBound;
							((Textured)modules[selectedModule].module).UpperYBound = upperYBound;
							((Textured)modules[selectedModule].module).Tex = modules[selectedModule].tempTex1;
						}
						break;
						case NoiseModuleType.Voronoi: {
							modules[selectedModule].module = new Voronoi();
							((Voronoi)modules[selectedModule].module).Frequency = modules[selectedModule].frequency;
							((Voronoi)modules[selectedModule].module).Displacement = modules[selectedModule].tempVal1;
							((Voronoi)modules[selectedModule].module).EnableDistance = modules[selectedModule].tempBool1;
							((Voronoi)modules[selectedModule].module).Seed = modules[selectedModule].seed;
							((Voronoi)modules[selectedModule].module).Clamped = modules[selectedModule].clamped;
						}
						break;
						case NoiseModuleType.Abs: {
							modules[selectedModule].module = new Abs();
							((Abs)modules[selectedModule].module).Module0 = modules[modules[selectedModule].mod0].module;
						}
						break;
						case NoiseModuleType.Acosine: {
							modules[selectedModule].module = new Acosine();
							((Acosine)modules[selectedModule].module).Module0 = modules[modules[selectedModule].mod0].module;
							((Acosine)modules[selectedModule].module).Invert = modules[selectedModule].tempBool1;
						}
						break;
						case NoiseModuleType.Add: {
							modules[selectedModule].module = new Add();
							((Add)modules[selectedModule].module).Module0 = modules[modules[selectedModule].mod0].module;
							((Add)modules[selectedModule].module).Module1 = modules[modules[selectedModule].mod1].module;
							((Add)modules[selectedModule].module).Clamped = modules[selectedModule].clamped;
						}
						break;
						case NoiseModuleType.Blend: {
							modules[selectedModule].module = new Blend();
							((Blend)modules[selectedModule].module).Module0 = modules[modules[selectedModule].mod0].module;
							((Blend)modules[selectedModule].module).Module1 = modules[modules[selectedModule].mod1].module;
							((Blend)modules[selectedModule].module).ModuleA = modules[modules[selectedModule].mod2].module;
						}
						break;
						case NoiseModuleType.BordersAverage: {
							modules[selectedModule].module = new BordersAverage();
							((BordersAverage)modules[selectedModule].module).Module0 = modules[modules[selectedModule].mod0].module;
							((BordersAverage)modules[selectedModule].module).LowerBound = modules[selectedModule].lowerBound;
							((BordersAverage)modules[selectedModule].module).UpperBound = modules[selectedModule].upperBound;
						}
						break;
						case NoiseModuleType.BordersExpand: {
							modules[selectedModule].module = new BordersExpand();
							((BordersExpand)modules[selectedModule].module).Module0 = modules[modules[selectedModule].mod0].module;
							((BordersExpand)modules[selectedModule].module).LowerBound = modules[selectedModule].lowerBound;
							((BordersExpand)modules[selectedModule].module).Center = modules[selectedModule].tempVal1;
							((BordersExpand)modules[selectedModule].module).UpperBound = modules[selectedModule].upperBound;
						}
						break;
						case NoiseModuleType.Clamp: {
							modules[selectedModule].module = new Clamp();
							((Clamp)modules[selectedModule].module).Module0 = modules[modules[selectedModule].mod0].module;
							((Clamp)modules[selectedModule].module).LowerBound = modules[selectedModule].lowerBound;
							((Clamp)modules[selectedModule].module).UpperBound = modules[selectedModule].upperBound;
						}
						break;
						case NoiseModuleType.ColorBurn: {
							modules[selectedModule].module = new ColorBurn();
							((ColorBurn)modules[selectedModule].module).Module0 = modules[modules[selectedModule].mod0].module;
							((ColorBurn)modules[selectedModule].module).Module1 = modules[modules[selectedModule].mod1].module;
						}
						break;
						case NoiseModuleType.ColorDodge: {
							modules[selectedModule].module = new ColorDodge();
							((ColorDodge)modules[selectedModule].module).Module0 = modules[modules[selectedModule].mod0].module;
							((ColorDodge)modules[selectedModule].module).Module1 = modules[modules[selectedModule].mod1].module;
						}
						break;
						case NoiseModuleType.Curve: {
							modules[selectedModule].module = new Curve();
							((Curve)modules[selectedModule].module).Module0 = modules[modules[selectedModule].mod0].module;
							List<ControlPoint> points = new List<ControlPoint>();
							for(int i = 0; i < modules[selectedModule].tempCurve.keys.Length; i++) {
								points.Add(new ControlPoint(modules[selectedModule].tempCurve.keys[i].time, modules[selectedModule].tempCurve.keys[i].value));
							}
							((Curve)modules[selectedModule].module).Points = points;
						}
						break;
						case NoiseModuleType.Displace: {
							modules[selectedModule].module = new Displace();
							((Displace)modules[selectedModule].module).Module0 = modules[modules[selectedModule].mod0].module;
							((Displace)modules[selectedModule].module).X = modules[modules[selectedModule].mod1].module;
							((Displace)modules[selectedModule].module).Y = modules[modules[selectedModule].mod2].module;
							((Displace)modules[selectedModule].module).Z = modules[modules[selectedModule].mod3].module;
							((Displace)modules[selectedModule].module).Clamped = modules[selectedModule].clamped;
						}
						break;
						case NoiseModuleType.Divide: {
							modules[selectedModule].module = new Divide();
							((Divide)modules[selectedModule].module).Module0 = modules[modules[selectedModule].mod0].module;
							((Divide)modules[selectedModule].module).Module1 = modules[modules[selectedModule].mod1].module;
							((Divide)modules[selectedModule].module).Clamped = modules[selectedModule].clamped;
						}
						break;
						case NoiseModuleType.Exclusion: {
							modules[selectedModule].module = new Exclusion();
							((Exclusion)modules[selectedModule].module).Module0 = modules[modules[selectedModule].mod0].module;
							((Exclusion)modules[selectedModule].module).Module1 = modules[modules[selectedModule].mod1].module;
						}
						break;
						case NoiseModuleType.Extract: {
							modules[selectedModule].module = new Extract();
							((Extract)modules[selectedModule].module).Module0 = modules[modules[selectedModule].mod0].module;
							((Extract)modules[selectedModule].module).LowerBound = modules[selectedModule].lowerBound;
							((Extract)modules[selectedModule].module).UpperBound = modules[selectedModule].upperBound;
						}
						break;
						case NoiseModuleType.Invert: {
							modules[selectedModule].module = new Invert();
							((Invert)modules[selectedModule].module).Module0 = modules[modules[selectedModule].mod0].module;
						}
						break;
						case NoiseModuleType.LinearBurn: {
							modules[selectedModule].module = new LinearBurn();
							((LinearBurn)modules[selectedModule].module).Module0 = modules[modules[selectedModule].mod0].module;
							((LinearBurn)modules[selectedModule].module).Module1 = modules[modules[selectedModule].mod1].module;
						}
						break;
						case NoiseModuleType.Log: {
							modules[selectedModule].module = new Log();
							((Log)modules[selectedModule].module).Module0 = modules[modules[selectedModule].mod0].module;
							((Log)modules[selectedModule].module).Value = modules[selectedModule].tempVal1;
							((Log)modules[selectedModule].module).Clamped = modules[selectedModule].clamped;
						}
						break;
						case NoiseModuleType.Max: {
							modules[selectedModule].module = new Max();
							((Max)modules[selectedModule].module).Module0 = modules[modules[selectedModule].mod0].module;
							((Max)modules[selectedModule].module).Module1 = modules[modules[selectedModule].mod1].module;
						}
						break;
						case NoiseModuleType.Min: {
							modules[selectedModule].module = new Min();
							((Min)modules[selectedModule].module).Module0 = modules[modules[selectedModule].mod0].module;
							((Min)modules[selectedModule].module).Module1 = modules[modules[selectedModule].mod1].module;
						}
						break;
						case NoiseModuleType.Multiply: {
							modules[selectedModule].module = new Multiply();
							((Multiply)modules[selectedModule].module).Module0 = modules[modules[selectedModule].mod0].module;
							((Multiply)modules[selectedModule].module).Module1 = modules[modules[selectedModule].mod1].module;
							((Multiply)modules[selectedModule].module).Clamped = modules[selectedModule].clamped;
						}
						break;
						case NoiseModuleType.Power: {
							modules[selectedModule].module = new Power();
							((Power)modules[selectedModule].module).Module0 = modules[modules[selectedModule].mod0].module;
							((Power)modules[selectedModule].module).Module1 = modules[modules[selectedModule].mod1].module;
							((Power)modules[selectedModule].module).Clamped = modules[selectedModule].clamped;
						}
						break;
						case NoiseModuleType.PowerOf: {
							modules[selectedModule].module = new PowerOf();
							((PowerOf)modules[selectedModule].module).Module0 = modules[modules[selectedModule].mod0].module;
							((PowerOf)modules[selectedModule].module).Value = modules[selectedModule].frequency;
							((PowerOf)modules[selectedModule].module).Clamped = modules[selectedModule].clamped;
						}
						break;
						case NoiseModuleType.RotatePoint: {
							modules[selectedModule].module = new RotatePoint();
							((RotatePoint)modules[selectedModule].module).Module0 = modules[modules[selectedModule].mod0].module;
							((RotatePoint)modules[selectedModule].module).SetAngles(modules[selectedModule].tempVal1, modules[selectedModule].tempVal2, modules[selectedModule].tempVal3);
							((RotatePoint)modules[selectedModule].module).Clamped = modules[selectedModule].clamped;
						}
						break;
						case NoiseModuleType.Round: {
							modules[selectedModule].module = new Round();
							((Round)modules[selectedModule].module).Module0 = modules[modules[selectedModule].mod0].module;
						}
						break;
						case NoiseModuleType.ScaleBias: {
							modules[selectedModule].module = new ScaleBias();
							((ScaleBias)modules[selectedModule].module).Module0 = modules[modules[selectedModule].mod0].module;
							((ScaleBias)modules[selectedModule].module).Scale = modules[selectedModule].tempVal1;
							((ScaleBias)modules[selectedModule].module).Bias = modules[selectedModule].tempVal2;
							((ScaleBias)modules[selectedModule].module).Clamped = modules[selectedModule].clamped;
						}
						break;
						case NoiseModuleType.ScalePoint: {
							modules[selectedModule].module = new ScalePoint();
							((ScalePoint)modules[selectedModule].module).Module0 = modules[modules[selectedModule].mod0].module;
							((ScalePoint)modules[selectedModule].module).X = modules[selectedModule].tempVal1;
							((ScalePoint)modules[selectedModule].module).Y = modules[selectedModule].tempVal2;
							((ScalePoint)modules[selectedModule].module).Z = modules[selectedModule].tempVal3;
							((ScalePoint)modules[selectedModule].module).Clamped = modules[selectedModule].clamped;
						}
						break;
						case NoiseModuleType.ScreenOverlay: {
							modules[selectedModule].module = new ScreenOverlay();
							((ScreenOverlay)modules[selectedModule].module).Module0 = modules[modules[selectedModule].mod0].module;
							((ScreenOverlay)modules[selectedModule].module).Module1 = modules[modules[selectedModule].mod1].module;
						}
						break;
						case NoiseModuleType.Select: {
							modules[selectedModule].module = new Select();
							((Select)modules[selectedModule].module).Module0 = modules[modules[selectedModule].mod0].module;
							((Select)modules[selectedModule].module).Module1 = modules[modules[selectedModule].mod1].module;
							((Select)modules[selectedModule].module).ModuleA = modules[modules[selectedModule].mod2].module;
							((Select)modules[selectedModule].module).SetBounds(modules[selectedModule].lowerBound, modules[selectedModule].upperBound);
							((Select)modules[selectedModule].module).SetEdgeFallOff(modules[selectedModule].tempVal1);
						}
						break;
						case NoiseModuleType.Subtract: {
							modules[selectedModule].module = new Subtract();
							((Subtract)modules[selectedModule].module).Module0 = modules[modules[selectedModule].mod0].module;
							((Subtract)modules[selectedModule].module).Module1 = modules[modules[selectedModule].mod1].module;
							((Subtract)modules[selectedModule].module).Clamped = modules[selectedModule].clamped;
						}
						break;
						case NoiseModuleType.Terrace: {
							modules[selectedModule].module = new Terrace();
							((Terrace)modules[selectedModule].module).Module0 = modules[modules[selectedModule].mod0].module;
							((Terrace)modules[selectedModule].module).Invert = modules[selectedModule].tempBool1;
							List<double> points = new List<double>();
							for(int i = 0; i < modules[selectedModule].tempCurve.keys.Length; i++) {
								points.Add(modules[selectedModule].tempCurve.keys[i].value);
							}
							((Terrace)modules[selectedModule].module).Points = points;
							((Terrace)modules[selectedModule].module).Thickness = modules[selectedModule].tempVal1;
						}
						break;
						case NoiseModuleType.TranslatePoint: {
							modules[selectedModule].module = new TranslatePoint();
							((TranslatePoint)modules[selectedModule].module).Module0 = modules[modules[selectedModule].mod0].module;
							((TranslatePoint)modules[selectedModule].module).X = modules[selectedModule].tempVal1;
							((TranslatePoint)modules[selectedModule].module).Y = modules[selectedModule].tempVal2;
							((TranslatePoint)modules[selectedModule].module).Z = modules[selectedModule].tempVal3;
							((TranslatePoint)modules[selectedModule].module).Clamped = modules[selectedModule].clamped;
						}
						break;
						case NoiseModuleType.Turbulence: {
							modules[selectedModule].module = new Turbulence();
							((Turbulence)modules[selectedModule].module).Module0 = modules[modules[selectedModule].mod0].module;
							((Turbulence)modules[selectedModule].module).Power = modules[selectedModule].power;
							((Turbulence)modules[selectedModule].module).Frequency = modules[selectedModule].frequency;
							((Turbulence)modules[selectedModule].module).Roughness = modules[selectedModule].roughness;
							((Turbulence)modules[selectedModule].module).Seed = modules[selectedModule].seed;
							((Turbulence)modules[selectedModule].module).Clamped = modules[selectedModule].clamped;
						}
						break;
						case NoiseModuleType.Test: {
							modules[selectedModule].module = new Test();
							((Test)modules[selectedModule].module).Start = modules[selectedModule].tempVal1;
							((Test)modules[selectedModule].module).End = modules[selectedModule].tempVal2;
							((Test)modules[selectedModule].module).LowerXBound = lowerXBound;
							((Test)modules[selectedModule].module).UpperXBound = upperXBound;
							((Test)modules[selectedModule].module).LowerYBound = lowerYBound;
							((Test)modules[selectedModule].module).UpperYBound = upperYBound;
							((Test)modules[selectedModule].module).Invert = modules[selectedModule].tempBool1;
							//((Test1)modules[selectedModule].module).Frequency = modules[selectedModule].frequency;
							//((Box)modules[selectedModule].module).LowerXBound = lowerXBound;
							//((Box)modules[selectedModule].module).UpperXBound = upperXBound;
							//((Box)modules[selectedModule].module).LowerYBound = lowerYBound;
							//((Box)modules[selectedModule].module).UpperYBound = upperYBound;
							//((Test)modules[selectedModule].module).Module0 = modules[modules[selectedModule].mod0].module;
							//((Test)modules[selectedModule].module).Module1 = modules[modules[selectedModule].mod1].module;
							//((Test)modules[selectedModule].module).Clamped = modules[selectedModule].clamped;
							//((Test)modules[selectedModule].module).Module0 = modules[modules[selectedModule].mod0].module;
							//((Test)modules[selectedModule].module).Module1 = modules[modules[selectedModule].mod1].module;
							//((Test)modules[selectedModule].module).Clamped = modules[selectedModule].clamped;
						}
						break;
					}

					EditorGUILayout.EndScrollView();
				}
			}

			//Module Options end
			EditorGUILayout.EndVertical();
#endregion Module Options

#region Render Options
			//Render Options start
			EditorGUILayout.BeginVertical("box", GUILayout.MaxHeight(_row2));

			togRenderOptions = EditorGUILayout.Foldout(togRenderOptions, "Render Options");
			if(togRenderOptions) {
				scrollRenderOptions = EditorGUILayout.BeginScrollView(scrollRenderOptions);
				EditorGUI.BeginChangeCheck();
				popMaps = (NoiseMapType)EditorGUILayout.EnumPopup("Mapping Projection: ", popMaps);
				if(EditorGUI.EndChangeCheck()) {
					seamlessEnabled = false;
					bumpHeight = 5;
					wrapEnabled = false;
					lightEnabled = false;
					lightColor = Color.white;
					lightElevation = 45.0;
					lightAzimuth = 45.0;
					lightBrightness = 2.0;
					lightContrast = 3.0;

					if(popMaps.Is(NoiseMapType.Plane)) {
						lowerXBound = -1.0; lowerYBound = -1.0; lowerZBound = 0.0;
						upperXBound = 1.0; upperYBound = 1.0; upperZBound = 0.0;
						textureWidth = 256; textureHeight = 256; textureDepth = 1;
					}
					if(popMaps.Is(NoiseMapType.Sphere)) {
						lowerXBound = -180.0; lowerYBound = -90.0; lowerZBound = 0.0;
						upperXBound = 180.0; upperYBound = 90.0; upperZBound = 0.0;
						textureWidth = 512; textureHeight = 256; textureDepth = 1;
					}
					if(popMaps.Is(NoiseMapType.Cylinder)) {
						lowerXBound = -180.0; lowerYBound = -1.0; lowerZBound = 0.0;
						upperXBound = 180.0; upperYBound = 1.0; upperZBound = 0.0;
						textureWidth = 512; textureHeight = 256; textureDepth = 1;
					}
					if(popMaps.Is(NoiseMapType.Box)) {
						lowerXBound = -1.0; lowerYBound = -1.0; lowerZBound = -1.0;
						upperXBound = 1.0; upperYBound = 1.0; upperZBound = 1.0;
						textureWidth = 16; textureHeight = 16; textureDepth = 16;
					}
				}

				if(popMaps.Is(NoiseMapType.Plane)) {
					seamlessEnabled = EditorGUILayout.Toggle("Seamless Texture: ", seamlessEnabled);
					wrapEnabled = EditorGUILayout.Toggle("Wrap Texture: ", wrapEnabled);
				}

				textureWidth = EditorGUILayout.IntField("Texture Width: ", textureWidth);
				textureHeight = EditorGUILayout.IntField("Texture Height: ", textureHeight);
				if(popMaps.Is(NoiseMapType.Box)) {
					textureDepth = EditorGUILayout.IntField("Texture Depth: ", textureDepth);
				}

				EditorGUILayout.BeginHorizontal();
				lowerXBound = EditorGUILayout.DoubleField("LowerX Bound: ", lowerXBound);
				upperXBound = EditorGUILayout.DoubleField("UpperX Bound: ", upperXBound);
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.BeginHorizontal();
				lowerYBound = EditorGUILayout.DoubleField("LowerY Bound: ", lowerYBound);
				upperYBound = EditorGUILayout.DoubleField("UpperY Bound: ", upperYBound);
				EditorGUILayout.EndHorizontal();
				if(popMaps.Is(NoiseMapType.Box)) {
					EditorGUILayout.BeginHorizontal();
					lowerZBound = EditorGUILayout.DoubleField("LowerZ Bound: ", lowerZBound);
					upperZBound = EditorGUILayout.DoubleField("UpperZ Bound: ", upperZBound);
					EditorGUILayout.EndHorizontal();
				}
				if(popMaps.IsNot(NoiseMapType.Box)) {
					bumpHeight = EditorGUILayout.DoubleField("Height Scale: ", bumpHeight);
				}
				if(popMaps.Is(NoiseMapType.Plane)) {
					lightEnabled = EditorGUILayout.Toggle("Lighting Enabled: ", lightEnabled);
				}
				if(lightEnabled) {
					EditorGUILayout.BeginHorizontal();
					lightColor = EditorGUILayout.ColorField("Sun Color: ", lightColor);
					lightBrightness = EditorGUILayout.DoubleField("Brightness: ", lightBrightness);
					EditorGUILayout.EndHorizontal();
					lightContrast = EditorGUILayout.DoubleField("Contrast: ", lightContrast);
					EditorGUILayout.BeginHorizontal();
					lightElevation = EditorGUILayout.DoubleField("Elevation: ", lightElevation);
					lightAzimuth = EditorGUILayout.DoubleField("Azimuth: ", lightAzimuth);
					EditorGUILayout.EndHorizontal();
				}
				EditorGUILayout.EndScrollView();

				EditorGUILayout.Space();

				EditorGUILayout.BeginHorizontal();
				//EditorGUILayout.LabelField("Render:", GUILayout.Width(50));
				GUILayout.FlexibleSpace();
				if(popMaps.IsNot(NoiseMapType.Box)) {
					if(GUILayout.Button("Texture2D", GUILayout.Width(75))) {
						if(modules != null && modules.Count > 0) {
							renderedTexture = RenderTexture2D(modules[selectedModule].module);
							ShowTextureWindow("Rendered Texture", false, false);
						}
					}
					if(GUILayout.Button("NormalMap", GUILayout.Width(75))) {
						if(modules != null && modules.Count > 0) {
							renderedTexture = RenderNormal(modules[selectedModule].module);
							ShowTextureWindow("Rendered Normals Texture", true, false);
						}
					}
					if(GUILayout.Button("FlowMap", GUILayout.Width(75))) {
						if(modules != null && modules.Count > 0) {
							renderedTexture = RenderFlow(modules[selectedModule].module);
							ShowTextureWindow("Rendered Flow Texture", true, false);
						}
					}
					if(GUILayout.Button("SlopeMap", GUILayout.Width(75))) {
						if(modules != null && modules.Count > 0) {
							renderedTexture = RenderSlope(modules[selectedModule].module);
							ShowTextureWindow("Rendered Slope Texture", true, false);
						}
					}
					if(GUILayout.Button("PlanetMap", GUILayout.Width(75))) {
						if(modules != null && modules.Count > 0) {
							renderedTexture = RenderPlanet(modules[selectedModule].module);
							ShowTextureWindow("Rendered Texture", true, true);
							/*if(EditorUtility.DisplayDialog("BUY from AssetStore",
										"A Planet Texture can only be used with TOZ Planet Renderer which is a seperate paid product that renders LOD planets in a solar system scale.\nWhich includes an enchanced version of NoiseGen.",
										"ASSETSTORE", "CANCEL")
							)
								Application.OpenURL("https://assetstore.unity.com/packages/templates/systems/toz-planet-renderer-122275?aid=1101l4NkK");
							*/
						}
					}
				}
				else {
					if(GUILayout.Button("Texture3D", GUILayout.Width(75))) {
						if(modules != null && modules.Count > 0) {
							Texture3D tex = RenderTexture3D(modules[selectedModule].module);
							tex.Save();
						}
					}
				}

				if(GUILayout.Button("NoiseMap", GUILayout.Width(75))) {
					if(modules != null && modules.Count > 0) {
						SaveNoiseMap(modules[selectedModule].module);
					}
				}
				if(GUILayout.Button("?", GUILayout.Width(22))) {
					Application.OpenURL("https://tozlab.com/documentation/noisegen-procedural-texture-generator");
				}
				EditorGUILayout.EndHorizontal();
			}

			//Render Options end
			EditorGUILayout.EndVertical();
#endregion Render Options

			//Second Column end
			EditorGUILayout.EndVertical();
#endregion Second Column
			//Whole window end
			EditorGUILayout.EndHorizontal();
#endregion Window
		}
#endregion Methods

#region Helper Methods
		private void AddModule() {
			modules.Add(new NoiseModules());
			int i = modules.Count-1;
			modules[i].id = i;
			modules[i].module = new Perlin();
			modules[i].type = NoiseModuleType.Perlin;
			string name = modules[i].type.ToString();
			modulesNames.Add(("{0:d} - " + name).FormatWith(modules[i].id));
			modulesNamesArray = modulesNames.ToArray();
			selectedModule = modules[i].id;
		}

		private void RemoveModule() {
			if(selectedModule > 0 && selectedModule < modules.Count) {
				modules.RemoveAt(selectedModule);
				modulesNames.RemoveAt(selectedModule);
				for(int i = selectedModule; i < modules.Count; i++) {
					modules[i].id -=1;
					modulesNames[i] = modulesNames[i].Replace(modulesNames[i].Before(" -"), modules[i].id.ToString());
				}
				selectedModule -= 1;
				modulesNamesArray = modulesNames.ToArray();
			}
		}

		private void ClearModules() {
			modules.Clear();
			modulesNames.Clear();
			modulesNamesArray = null;
			AddModule();
		}

		private Texture2D RenderTexture2D(IModule module) {
			NoiseMapRenderer renderer = new NoiseMapRenderer();
			SetBuilderAndRendererOptions(ref renderer, module);

			renderer.RenderImage();
			return renderer.GetTexture2D();
		}

		private Texture2D RenderNormal(IModule module) {
			NoiseMapRenderer renderer = new NoiseMapRenderer();
			SetBuilderAndRendererOptions(ref renderer, module);

			renderer.RenderNormal();
			return renderer.GetTexture2D();
		}

		private Texture2D RenderFlow(IModule module) {
			NoiseMapRenderer renderer = new NoiseMapRenderer();
			SetBuilderAndRendererOptions(ref renderer, module);

			renderer.RenderFlowMap();
			return renderer.GetTexture2D();
		}

		private Texture2D RenderSlope(IModule module) {
			NoiseMapRenderer renderer = new NoiseMapRenderer();
			SetBuilderAndRendererOptions(ref renderer, module);

			renderer.RenderSlope();
			return renderer.GetTexture2D();
		}

		private Texture2D RenderPlanet(IModule module) {
			NoiseMapRenderer renderer = new NoiseMapRenderer();
			SetBuilderAndRendererOptions(ref renderer, module);

			renderer.RenderPlanet();
			return renderer.GetTexture2D();
		}

		private Texture3D RenderTexture3D(IModule module) {
			NoiseMapRenderer renderer = new NoiseMapRenderer();
			SetBuilderAndRendererOptions(ref renderer, module);

			renderer.RenderImage();
			return renderer.GetTexture3D();
		}

		private void SetBuilderAndRendererOptions(ref NoiseMapRenderer renderer, IModule module) {
			renderer.SourceGradientColor = gradient;
			renderer.BumpHeight = bumpHeight;
			renderer.IsWrapEnabled = wrapEnabled;
			renderer.IsLightEnabled = lightEnabled;
			renderer.LightColor = lightColor;
			renderer.LightElevation = lightElevation;
			renderer.LightAzimuth = lightAzimuth;
			renderer.LightBrightness = lightBrightness;
			renderer.LightContrast = lightContrast;

			switch((NoiseMapType)popMaps) {
				case NoiseMapType.Plane: {
					NoiseMapBuilderPlane builder = new NoiseMapBuilderPlane(textureWidth, textureHeight);
					builder.SetBounds(lowerXBound, lowerYBound, upperXBound, upperYBound);
					builder.IsSeamless = seamlessEnabled;
					builder.Build(module);
					renderer.SourceNoiseMap = builder.Map;
				}
				break;
				case NoiseMapType.Sphere: {
					NoiseMapBuilderSphere builder = new NoiseMapBuilderSphere(textureWidth, textureHeight);
					builder.SetBounds(lowerXBound, lowerYBound, upperXBound, upperYBound);
					builder.Build(module);
					renderer.SourceNoiseMap = builder.Map;
				}
				break;
				case NoiseMapType.Cylinder: {
					NoiseMapBuilderCylinder builder = new NoiseMapBuilderCylinder(textureWidth, textureHeight);
					builder.SetBounds(lowerXBound, lowerYBound, upperXBound, upperYBound);
					builder.Build(module);
					renderer.SourceNoiseMap = builder.Map;
				}
				break;
				case NoiseMapType.Box: {
					NoiseMapBuilderBox builder = new NoiseMapBuilderBox(textureWidth, textureHeight, textureDepth);
					builder.SetBounds(lowerXBound, lowerYBound, lowerZBound, upperXBound, upperYBound, upperZBound);
					builder.Build(module);
					renderer.SourceNoiseMap = builder.Map;
				}
				break;
			}
		}

		private void SaveNoiseMap(IModule module) {
			SO_NoiseMap asset = ScriptableObject.CreateInstance<SO_NoiseMap>();

			switch((NoiseMapType)popMaps) {
				case NoiseMapType.Plane: {
					NoiseMapBuilderPlane builder = new NoiseMapBuilderPlane(textureWidth, textureHeight);
					builder.SetBounds(lowerXBound, lowerYBound, upperXBound, upperYBound);
					builder.IsSeamless = seamlessEnabled;
					builder.Build(module);
					asset.Data = _ArrayUtil._TrioToMono(builder.Map.Data);
					asset.Width = builder.Map.Width;
					asset.Height = builder.Map.Height;
					asset.Depth = builder.Map.Depth;
				}
				break;
				case NoiseMapType.Sphere: {
					NoiseMapBuilderSphere builder = new NoiseMapBuilderSphere(textureWidth, textureHeight);
					builder.SetBounds(lowerXBound, lowerYBound, upperXBound, upperYBound);
					builder.Build(module);
					asset.Data = _ArrayUtil._TrioToMono(builder.Map.Data);
					asset.Width = builder.Map.Width;
					asset.Height = builder.Map.Height;
					asset.Depth = builder.Map.Depth;
				}
				break;
				case NoiseMapType.Cylinder: {
					NoiseMapBuilderCylinder builder = new NoiseMapBuilderCylinder(textureWidth, textureHeight);
					builder.SetBounds(lowerXBound, lowerYBound, upperXBound, upperYBound);
					builder.Build(module);
					asset.Data = _ArrayUtil._TrioToMono(builder.Map.Data);
					asset.Width = builder.Map.Width;
					asset.Height = builder.Map.Height;
					asset.Depth = builder.Map.Depth;
				}
				break;
				case NoiseMapType.Box: {
					NoiseMapBuilderBox builder = new NoiseMapBuilderBox(textureWidth, textureHeight, textureDepth);
					builder.SetBounds(lowerXBound, lowerYBound, lowerZBound, upperXBound, upperYBound, upperZBound);
					builder.Build(module);
					asset.Data = _ArrayUtil._TrioToMono(builder.Map.Data);
					asset.Width = builder.Map.Width;
					asset.Height = builder.Map.Height;
					asset.Depth = builder.Map.Depth;
				}
				break;
				default:
				break;
			}

			string file = EditorUtility.SaveFilePanelInProject("Save NoiseMap", "New NoiseMap", "asset", "Please enter a file name");
			//string file = AssetDatabase.GenerateUniqueAssetPath("Assets/New NoiseMap.asset");
			if(file.Length != 0) {
				AssetDatabase.CreateAsset(asset, file);
				AssetDatabase.Refresh();
				Debug.Log("NoiseMap is saved at location:" + file);
			}
		}
#endregion Helper Methods
	}

}