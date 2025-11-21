using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using TOZ;
using TOZ.Noise;
using TOZ.Celestial;

namespace TOZEditor {

	[CustomEditor(typeof(Planet))]
	[CanEditMultipleObjects]
	public class CE_Planet : Editor {
		private enum TabsType {
			Info, Creation, Atmosphere, Water, Ground, Nature
		}

		//Editor Variables
		private TabsType currentTab = TabsType.Info;
		private string[] str_Tabs = new string[6] { "Info", "Creation", "Atmosphere", "Water", "Ground", "Nature" };
		private string str_gradientError =
		"Gradient Error:\n" +
		"Your Gradient colorKeys count and alphaKeys count must be equal.";
		private string str_helpGradient = 
		"Gradient Shader, Gradient Help:\n\n" +
		"LOCATIONS:\n" +
		"0 is the bottom of the sea.\n" +
		"50 is the sea level.\n" +
		"100 is the top of the highest mountain.\n\n" +
		"COLORS:\n" +
		"Your planet will be textured depending on the colors you use here.";
		private string str_helpBumped = 
		"Bumped Shader, Gradient Help:\n\n" +
		"LOCATIONS:\n" +
		"0 is the bottom of the sea.\n" +
		"50 is the sea level.\n" +
		"100 is the top of the highest mountain.\n\n" +
		"COLORS:\n" +
		"Your planet will be textured depending on the colors you use here.\n" +
		"And a corresponding bumpMap will be auto generated and applied to your planet.\n" +
		"Use the below bumpMap value to define how strong bumpMap is.";
		private string str_helpSplat = 
		"Splat Shader, Gradient Help:\n\n" +
		"LOCATIONS:\n" +
		"0 is the bottom of the sea.\n" +
		"50 is the sea level.\n" +
		"100 is the top of the highest mountain.\n\n" +
		"COLORS:\n" +
		"Every color key must have an alpha key at the same location!!\n" +
		"RED color is the Splat0 Texture, ALPHA has to be 0.\n" +
		"GREEN color is the Splat1 Texture, ALPHA has to be 0.\n" +
		"BLUE color is the Splat2 Texture, ALPHA has to be 0.\n" +
		"BLACK color is the Splat3 Texture, ALPHA has to be 255.";
		private string str_unityBug =
		"Unity BUG:\n" +
		"SerializedProperty doesnt have a public gradientValue.\n\n" +
		"The Gradient Texture representation wont update if you revert the prefab, you must deselect and reselect the Planet Object to update gradient.";

		//Variables
		private Planet obj;

		private SerializedProperty myName;
		private SerializedProperty isMoon;
		private SerializedProperty parent;
		private SerializedProperty gravityScale;
		private SerializedProperty radius;

		private SerializedProperty quality;
		private SerializedProperty dataMap;

		private SerializedProperty hasGround;
		private SerializedProperty hasWater;
		private SerializedProperty hasClouds;
		private SerializedProperty hasAtmosphere;
		private SerializedProperty hasNature;
		private SerializedProperty hasWildLife;
		private SerializedProperty hasStation;

		private SerializedProperty planetShader;
		private SerializedProperty controlGradient;

		private SerializedProperty detail0Texture;
		private SerializedProperty detail0TextureScale;
		private SerializedProperty detail1Texture;
		private SerializedProperty detail1TextureScale;
		private SerializedProperty splat0Texture;
		private SerializedProperty splat0TextureScale;
		private SerializedProperty splat1Texture;
		private SerializedProperty splat1TextureScale;
		private SerializedProperty splat2Texture;
		private SerializedProperty splat2TextureScale;
		private SerializedProperty splat3Texture;
		private SerializedProperty splat3TextureScale;
		private SerializedProperty albedoTexture;

		private SerializedProperty waterReflectionTexture;
		private SerializedProperty waterBumpTexture;
		private SerializedProperty waterBumpTextureScale;
		private SerializedProperty wavePower;
		private SerializedProperty waveSpeed;
		private SerializedProperty fresnelPower;
		private SerializedProperty fresnelCutoff;
		private SerializedProperty shoreThickness;
		private SerializedProperty shallowThickness;
		private SerializedProperty waterShininess;

		private SerializedProperty cloudsTexture;

		//private SerializedProperty dayAmbient;
		private SerializedProperty zenithColor;
		private SerializedProperty averageDensity;
		private SerializedProperty skyDensity;
		private SerializedProperty sunSize;
		private SerializedProperty sunBlur;

		private SerializedProperty fogDistanceBelowWater;

		private SerializedProperty stationPrefab;
		private SerializedProperty isStationPositionedRandomly;
		private SerializedProperty stationPosition;

		private SerializedProperty naturePrefabs;
		private SerializedProperty wildLifePrefabs;

		//Mono Methods
		private void OnEnable() {
			obj = (Planet)target;

			myName = serializedObject.FindProperty("Name");
			isMoon = serializedObject.FindProperty("IsMoon");
			parent = serializedObject.FindProperty("Parent");
			gravityScale = serializedObject.FindProperty("GravityScale");
			radius = serializedObject.FindProperty("Radius");

			quality = serializedObject.FindProperty("Quality");
			dataMap = serializedObject.FindProperty("dataMap");

			hasGround = serializedObject.FindProperty("HasGround");
			hasWater = serializedObject.FindProperty("HasWater");
			hasClouds = serializedObject.FindProperty("HasClouds");
			hasAtmosphere = serializedObject.FindProperty("HasAtmosphere");
			hasNature = serializedObject.FindProperty("HasNature");
			hasWildLife = serializedObject.FindProperty("HasWildLife");
			hasStation = serializedObject.FindProperty("HasStation");

			planetShader = serializedObject.FindProperty("planetShader");
			controlGradient = serializedObject.FindProperty("ControlGradient");

			detail0Texture = serializedObject.FindProperty("detail0Texture");
			detail0TextureScale = serializedObject.FindProperty("detail0TextureScale");
			detail1Texture = serializedObject.FindProperty("detail1Texture");
			detail1TextureScale = serializedObject.FindProperty("detail1TextureScale");
			splat0Texture = serializedObject.FindProperty("splat0Texture");
			splat0TextureScale = serializedObject.FindProperty("splat0TextureScale");
			splat1Texture = serializedObject.FindProperty("splat1Texture");
			splat1TextureScale = serializedObject.FindProperty("splat1TextureScale");
			splat2Texture = serializedObject.FindProperty("splat2Texture");
			splat2TextureScale = serializedObject.FindProperty("splat2TextureScale");
			splat3Texture = serializedObject.FindProperty("splat3Texture");
			splat3TextureScale = serializedObject.FindProperty("splat3TextureScale");
			albedoTexture = serializedObject.FindProperty("albedoTexture");

			waterReflectionTexture = serializedObject.FindProperty("waterReflectionTexture");
			waterBumpTexture = serializedObject.FindProperty("waterBumpTexture");
			waterBumpTextureScale = serializedObject.FindProperty("waterBumpTextureScale");
			wavePower = serializedObject.FindProperty("wavePower");
			waveSpeed = serializedObject.FindProperty("waveSpeed");
			fresnelPower = serializedObject.FindProperty("fresnelPower");
			fresnelCutoff = serializedObject.FindProperty("fresnelCutoff");
			shoreThickness = serializedObject.FindProperty("shoreThickness");
			shallowThickness = serializedObject.FindProperty("shallowThickness");
			waterShininess = serializedObject.FindProperty("waterShininess");

			cloudsTexture = serializedObject.FindProperty("cloudsTexture");

			//dayAmbient = serializedObject.FindProperty("DayAmbient");
			zenithColor = serializedObject.FindProperty("zenithColor");
			averageDensity = serializedObject.FindProperty("averageDensity");
			skyDensity = serializedObject.FindProperty("skyDensity");
			sunSize = serializedObject.FindProperty("sunSize");
			sunBlur = serializedObject.FindProperty("sunBlur");

			fogDistanceBelowWater = serializedObject.FindProperty("fogDistanceBelowWater");

			stationPrefab = serializedObject.FindProperty("stationPrefab");
			isStationPositionedRandomly = serializedObject.FindProperty("isStationPositionedRandomly");
			stationPosition = serializedObject.FindProperty("stationPosition");

			naturePrefabs = serializedObject.FindProperty("NaturePrefabs");
			wildLifePrefabs = serializedObject.FindProperty("WildLifePrefabs");
		}

		public override void OnInspectorGUI() {
			serializedObject.Update();

			EditorGUILayout.Space();
			currentTab = (TabsType)GUILayout.Toolbar((int)currentTab, str_Tabs, EditorStyles.toolbarButton);
			switch(currentTab) {
				case TabsType.Info: {
					EditorGUILayout.Space();
					myName.stringValue = EditorGUILayout.TextField("Celestial Name: ", myName.stringValue);
					isMoon.boolValue = EditorGUILayout.Toggle("Is Moon: ", isMoon.boolValue);
					using(new EditorGUI.DisabledScope(!isMoon.boolValue)) {
						parent.objectReferenceValue = EditorGUILayout.ObjectField("Parent Celestial: ", parent.objectReferenceValue, typeof(CelestialBase), true);
					}
				}
				break;
				case TabsType.Creation: {
					EditorGUILayout.Space();
					gravityScale.floatValue = EditorGUILayout.Slider("Gravity Scale: ", gravityScale.floatValue, 0f, 10f);
					radius.floatValue = EditorGUILayout.FloatField("Planet Radius: ", radius.floatValue);
					EditorGUILayout.Space();
					quality.enumValueIndex = EditorGUILayout.Popup("Planet Quality:", quality.enumValueIndex, quality.enumDisplayNames);

					switch((PlanetQualityType)quality.enumValueIndex) {
						case PlanetQualityType.Low:
							EditorGUILayout.BeginHorizontal();
							EditorGUILayout.PrefixLabel("Quality Details: ");
							EditorGUILayout.LabelField("[1025, 513, 1]");
							EditorGUILayout.EndHorizontal();
						break;
						case PlanetQualityType.Medium:
							EditorGUILayout.BeginHorizontal();
							EditorGUILayout.PrefixLabel("Quality Details: ");
							EditorGUILayout.LabelField("[2049, 1025, 1]");
							EditorGUILayout.EndHorizontal();
						break;
						case PlanetQualityType.Normal:
							EditorGUILayout.BeginHorizontal();
							EditorGUILayout.PrefixLabel("Quality Details: ");
							EditorGUILayout.LabelField("[4097, 2049, 1]");
							EditorGUILayout.EndHorizontal();
						break;
						case PlanetQualityType.Good:
							EditorGUILayout.BeginHorizontal();
							EditorGUILayout.PrefixLabel("Quality Details: ");
							EditorGUILayout.LabelField("[8193, 4097, 1]");
							EditorGUILayout.EndHorizontal();
						break;
						case PlanetQualityType.High:
							EditorGUILayout.BeginHorizontal();
							EditorGUILayout.PrefixLabel("Quality Details: ");
							EditorGUILayout.LabelField("[16385, 8193, 1]" + " - " + "!!!!LOW FPS WARNING!!!!");
							EditorGUILayout.EndHorizontal();
						break;
						case PlanetQualityType.Ultra:
							EditorGUILayout.BeginHorizontal();
							EditorGUILayout.PrefixLabel("Quality Details: ");
							EditorGUILayout.LabelField("[32769, 16385, 1]" + " - " + "!!!!LOW FPS WARNING!!!!");
							EditorGUILayout.EndHorizontal();
						break;
					}
					EditorGUILayout.Space();
					dataMap.objectReferenceValue = EditorGUILayout.ObjectField("Data Texture: ", dataMap.objectReferenceValue, typeof(Texture2D), false);
					EditorGUILayout.Space();
					hasStation.boolValue = EditorGUILayout.Toggle("Planet Has Station: ", hasStation.boolValue);
					using(new EditorGUI.DisabledScope(!hasStation.boolValue)) {
						EditorGUILayout.Space();
						EditorGUILayout.PropertyField(stationPrefab, new GUIContent("Station Prefab"), true);
						isStationPositionedRandomly.boolValue = EditorGUILayout.Toggle("Positioned Randomly: ", isStationPositionedRandomly.boolValue);
						using(new EditorGUI.DisabledScope(isStationPositionedRandomly.boolValue)) {
							stationPosition.vector2Value = EditorGUILayout.Vector2Field("Lattitude & Longitude:", stationPosition.vector2Value);
						}
					}
				}
				break;
				case TabsType.Atmosphere: {
					EditorGUILayout.Space();
					EditorGUI.BeginChangeCheck();
					hasAtmosphere.boolValue = EditorGUILayout.Toggle("Planet Has Atmosphere: ", hasAtmosphere.boolValue);
					if(EditorGUI.EndChangeCheck()) {
						if(!hasAtmosphere.boolValue)
							hasClouds.boolValue = false;
					}
					EditorGUI.BeginChangeCheck();
					hasClouds.boolValue = EditorGUILayout.Toggle("Planet Has Clouds: ", hasClouds.boolValue);
					if(EditorGUI.EndChangeCheck()) {
						if(hasClouds.boolValue)
							hasAtmosphere.boolValue = true;
					}
					using(new EditorGUI.DisabledScope(!hasAtmosphere.boolValue)) {
						EditorGUILayout.Space();
						zenithColor.colorValue = EditorGUILayout.ColorField("Zenith Color: ", zenithColor.colorValue);
						//dayAmbient.colorValue = EditorGUILayout.ColorField("Day Ambient Color: ", dayAmbient.colorValue);
						EditorGUILayout.Space();
						averageDensity.floatValue = EditorGUILayout.Slider("Average Density: ", averageDensity.floatValue, 0.0f, 1.0f);
						skyDensity.floatValue = EditorGUILayout.Slider("Overall Density: ", skyDensity.floatValue, 0f, 0.2f);
						sunSize.floatValue = EditorGUILayout.Slider("Sun Size: ", sunSize.floatValue, 0f, 0.2f);
						sunBlur.floatValue = EditorGUILayout.Slider("Sun Blur: ", sunBlur.floatValue, -0.985f, -0.995f);
					}
					using(new EditorGUI.DisabledScope(!hasClouds.boolValue)) {
						EditorGUILayout.Space();
						cloudsTexture.objectReferenceValue = EditorGUILayout.ObjectField("Clouds Texture: ", cloudsTexture.objectReferenceValue, typeof(Texture2D), false);
					}
				}
				break;
				case TabsType.Water: {
					EditorGUILayout.Space();
					hasWater.boolValue = EditorGUILayout.Toggle("Planet Has Water: ", hasWater.boolValue);
					using(new EditorGUI.DisabledScope(!hasWater.boolValue)) {
						EditorGUILayout.Space();
						waterReflectionTexture.objectReferenceValue = EditorGUILayout.ObjectField("Reflection Texture: ", waterReflectionTexture.objectReferenceValue, typeof(Cubemap), false);
						waterBumpTexture.objectReferenceValue = EditorGUILayout.ObjectField("Waves Texture: ", waterBumpTexture.objectReferenceValue, typeof(Texture2D), false);
						waterBumpTextureScale.floatValue = EditorGUILayout.Slider("Waves Texture Scale: ", waterBumpTextureScale.floatValue, 0f, 16f);
						wavePower.floatValue = EditorGUILayout.Slider("Wave Power: ", wavePower.floatValue, 0f, 1f);
						waveSpeed.floatValue = EditorGUILayout.Slider("Wave Speed: ", waveSpeed.floatValue, 0f, 0.1f);
						EditorGUILayout.Space();
						
						fresnelPower.floatValue = EditorGUILayout.Slider("Fresnel Power: ", fresnelPower.floatValue, 0f, 10f);
						fresnelCutoff.floatValue = EditorGUILayout.Slider("Fresnel Cutoff: ", fresnelCutoff.floatValue, 0f, 10f);
						shoreThickness.floatValue = EditorGUILayout.Slider("Shore Thickness: ", shoreThickness.floatValue, 0f, 1f);
						EditorGUILayout.LabelField("Real size: " + shoreThickness.floatValue * (obj.Radius * 0.025f * 0.5f));
						shallowThickness.floatValue = EditorGUILayout.Slider("Shallow Thickness: ", shallowThickness.floatValue, 0.0001f, 1f);
						EditorGUILayout.LabelField("Real size: " + shallowThickness.floatValue * (obj.Radius * 0.025f * 0.5f));
						EditorGUILayout.Space();
						waterShininess.floatValue = EditorGUILayout.Slider("Shininess: ", waterShininess.floatValue, 0.1f, 120f);
						EditorGUILayout.Space();
						fogDistanceBelowWater.floatValue = EditorGUILayout.Slider("Fog Distance Below Water: ", fogDistanceBelowWater.floatValue, 0f, 25f);
					}
				}
				break;
				case TabsType.Ground: {
					EditorGUILayout.Space();
					EditorGUI.BeginChangeCheck();
					hasGround.boolValue = EditorGUILayout.Toggle("Planet Has Ground: ", hasGround.boolValue);
					if(EditorGUI.EndChangeCheck()) {
						if(!hasGround.boolValue)
							hasNature.boolValue = false;
					}
					using(new EditorGUI.DisabledScope(!hasGround.boolValue)) {
						EditorGUILayout.Space();
						planetShader.enumValueIndex = EditorGUILayout.Popup("Planet Shader:", planetShader.enumValueIndex, planetShader.enumDisplayNames);
						detail0Texture.objectReferenceValue = EditorGUILayout.ObjectField("Detail0 Texture: ", detail0Texture.objectReferenceValue, typeof(Texture2D), false);
						detail0TextureScale.floatValue = EditorGUILayout.Slider("Detail0 Texture Scale: ", detail0TextureScale.floatValue, 0f, 16f);
						switch((PlanetShaderType)planetShader.enumValueIndex) {
							case PlanetShaderType.Gradient:
							case PlanetShaderType.Bumped:
							case PlanetShaderType.Albedo: {
								EditorGUILayout.Space();
								detail1Texture.objectReferenceValue = EditorGUILayout.ObjectField("Detail1 Texture: ", detail1Texture.objectReferenceValue, typeof(Texture2D), false);
								detail1TextureScale.floatValue = EditorGUILayout.Slider("Detail1 Texture Scale: ", detail1TextureScale.floatValue, 0f, 16f);
							}
							break;
						}
						switch((PlanetShaderType)planetShader.enumValueIndex) {
							case PlanetShaderType.Gradient:
							case PlanetShaderType.Bumped:
							case PlanetShaderType.Splat: {
								EditorGUILayout.Space();
								EditorGUILayout.PropertyField(controlGradient, new GUIContent("Gradient"), true);
								EditorGUILayout.Space();
								if(obj.ControlGradient.colorKeys.Length != obj.ControlGradient.alphaKeys.Length) {
									EditorGUILayout.HelpBox(str_gradientError, MessageType.Error, true);
									EditorGUILayout.Space();
								}
								EditorGUILayout.HelpBox(str_unityBug, MessageType.Warning, true);
							}
							break;
						}
						EditorGUILayout.Space();
						switch((PlanetShaderType)planetShader.enumValueIndex) {
							case PlanetShaderType.Gradient: {
								EditorGUILayout.HelpBox(str_helpGradient, MessageType.Info, true);
								EditorGUILayout.Space();
							}
							break;
							case PlanetShaderType.Bumped: {
								EditorGUILayout.HelpBox(str_helpBumped, MessageType.Info, true);
								EditorGUILayout.Space();
							}
							break;
							case PlanetShaderType.Splat: {
								EditorGUILayout.HelpBox(str_helpSplat, MessageType.Info, true);
								EditorGUILayout.Space();
								splat0Texture.objectReferenceValue = EditorGUILayout.ObjectField("Splat0 Texture: ", splat0Texture.objectReferenceValue, typeof(Texture2D), false);
								splat0TextureScale.floatValue = EditorGUILayout.Slider("Splat0 Texture Scale: ", splat0TextureScale.floatValue, 0f, 16f);
								EditorGUILayout.Space();
								splat1Texture.objectReferenceValue = EditorGUILayout.ObjectField("Splat1 Texture: ", splat1Texture.objectReferenceValue, typeof(Texture2D), false);
								splat1TextureScale.floatValue = EditorGUILayout.Slider("Splat1 Texture Scale: ", splat1TextureScale.floatValue, 0f, 16f);
								EditorGUILayout.Space();
								splat2Texture.objectReferenceValue = EditorGUILayout.ObjectField("Splat2 Texture: ", splat2Texture.objectReferenceValue, typeof(Texture2D), false);
								splat2TextureScale.floatValue = EditorGUILayout.Slider("Splat2 Texture Scale: ", splat2TextureScale.floatValue, 0f, 16f);
								EditorGUILayout.Space();
								splat3Texture.objectReferenceValue = EditorGUILayout.ObjectField("Splat3 Texture: ", splat3Texture.objectReferenceValue, typeof(Texture2D), false);
								splat3TextureScale.floatValue = EditorGUILayout.Slider("Splat3 Texture Scale: ", splat3TextureScale.floatValue, 0f, 16f);
							}
							break;
							case PlanetShaderType.Albedo: {
								albedoTexture.objectReferenceValue = EditorGUILayout.ObjectField("Albedo Texture: ", albedoTexture.objectReferenceValue, typeof(Texture2D), false);
							}
							break;
						}
					}
				}
				break;
				case TabsType.Nature: {
					EditorGUILayout.Space();
					EditorGUI.BeginChangeCheck();
					hasNature.boolValue = EditorGUILayout.Toggle("Planet Has Nature: ", hasNature.boolValue);
					if(EditorGUI.EndChangeCheck()) {
						if(hasNature.boolValue)
							hasGround.boolValue = true;
					}
					using(new EditorGUI.DisabledScope(!hasNature.boolValue)) {
						EditorGUILayout.PropertyField(naturePrefabs, new GUIContent("Nature Prefabs"), true);
					}

					EditorGUILayout.Space();
					EditorGUI.BeginChangeCheck();
					hasWildLife.boolValue = EditorGUILayout.Toggle("Planet Has WildLife: ", hasWildLife.boolValue);
					if(EditorGUI.EndChangeCheck()) {
						if(hasWildLife.boolValue)
							hasGround.boolValue = true;
					}
					using(new EditorGUI.DisabledScope(!hasWildLife.boolValue)) {
						EditorGUILayout.PropertyField(wildLifePrefabs, new GUIContent("WildLife Prefabs"), true);
					}
				}
				break;
			}

			serializedObject.ApplyModifiedProperties();
		}

		//Helper Methods
		[DrawGizmo(GizmoType.NonSelected | GizmoType.Active | GizmoType.InSelectionHierarchy)]
		private static void DrawGizmo(Planet cs, GizmoType gizmoType) {
			if(!Application.isPlaying) {
				if(cs.HasGround) {
					Gizmos.color = new Color(0, 1, 0, 0.5f);
					Gizmos.DrawSphere(cs.transform.position, cs.Radius);
				}
				if(cs.HasAtmosphere) {
					float atmosphereRadius = cs.Radius + (cs.Radius * 0.025f) * 3f;
					Gizmos.color = new Color(0, 0, 1, 0.25f);
					Gizmos.DrawSphere(cs.transform.position, atmosphereRadius);
				}
				if(cs.HasStation) {
					float lat = cs.StationPosition.x;
					float lon = cs.StationPosition.y;
					Vector3 dir = _MathUtil._PolarToCartesian(lat, lon);
					if(cs.IsStationPositionedRandomly) {
						_RandUtil._SetSeed(cs.ID);
						lat = UnityEngine.Random.Range(-180f, 180f);
						lon = UnityEngine.Random.Range(-90f, 90f);
						dir = _MathUtil._PolarToCartesian(lat, lon);
					}
					Vector3 pos1 = cs.transform.position + (dir * cs.Radius);
					Vector3 pos2 = cs.transform.position + (dir * cs.Radius * 1.5f);
					Gizmos.color = Color.yellow;
					Quaternion quat = Quaternion.LookRotation(dir);
					_UIUtil._Hndl_WireArc(pos1, quat * Vector3.right, dir, 360f, cs.Radius * 0.1f, Color.yellow);
					Gizmos.DrawLine(pos1, pos2);
					_UIUtil._Hndl_LabelCenter(pos2, "There is a Station here!");
				}
			}
			
			_UIUtil._Hndl_WireArc(cs.transform.position, Vector3.right, Vector3.up, 180f, cs.Radius, Color.yellow);
			_UIUtil._Hndl_WireArc(cs.transform.position, Vector3.up, Vector3.forward, 360f, cs.Radius, Color.yellow);
			_UIUtil._Hndl_LabelCenter(cs.transform.position + Vector3.forward * cs.Radius, "Lat=0, Lon=0");
			_UIUtil._Hndl_LabelCenter(cs.transform.position + Vector3.up * cs.Radius, "Lon=90");
			_UIUtil._Hndl_LabelCenter(cs.transform.position + Vector3.down * cs.Radius, "Lon=-90");
			_UIUtil._Hndl_LabelCenter(cs.transform.position + Vector3.left * cs.Radius, "Lat=90");
			_UIUtil._Hndl_LabelCenter(cs.transform.position + Vector3.right * cs.Radius, "Lat=-90");
			
			if(cs.Parent == null) {
				_UIUtil._Hndl_WireArc(cs.transform.position, Vector3.up, Vector3.forward, 360f, cs.Diameter * 2f, Color.green);
				_UIUtil._Hndl_WireArc(cs.transform.position, Vector3.up, Vector3.forward, 360f, cs.Diameter * 3f, Color.green);
			}
			else {
				_UIUtil._Hndl_Line(cs.transform.position, cs.Parent.transform.position, 1f, Color.green);
			}
		}
	}

}
