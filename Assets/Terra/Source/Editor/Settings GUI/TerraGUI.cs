using System;
using System.Collections.Generic;
using Terra.Graph.Noise;
using Terra.ReorderableList;
using Terra.Terrain;
using Terra.Terrain.Util;
using System.Linq;
using UnityEngine;

namespace UnityEditor.Terra {
	/// <summary>
	/// Handles displaying various GUI elements for the Terra
	/// Settings custom inspector
	/// </summary>
	public class TerraGUI {
		private TerraSettings Settings;
		private Texture[] ToolbarImages;

		private ReorderableMaterialList matList;

		public TerraGUI(TerraSettings settings) {
			this.Settings = settings;
			matList = new ReorderableMaterialList(settings);
		}

		/// <summary>
		/// Displays a toggleable toolbar with icons from 
		/// the file system
		/// </summary>
		public void Toolbar() {
			EditorGUILayout.Space();

			//Set toolbar images
			if (ToolbarImages == null) {
				ToolbarImages = new Texture[] {
					(Texture)Resources.Load("terra_gui_general"),
					(Texture)Resources.Load("terra_gui_noise"),
					(Texture)Resources.Load("terra_gui_material"),
					(Texture)Resources.Load("terra_gui_object")
				};
			}

			Settings.EditorState.ToolbarSelection = (TerraSettings.ToolbarOptions)EditorGUIExtension.EnumToolbar(Settings.EditorState.ToolbarSelection, ToolbarImages);
		}

		/// <summary>
		/// Displays GUI elements for the "General" tab
		/// </summary>
		public void General() {
			//Tracked gameobject
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Tracked GameObject", EditorStyles.boldLabel);
			Settings.Generator.TrackedObject = (GameObject)EditorGUILayout.ObjectField(Settings.Generator.TrackedObject, typeof(GameObject), true);

			//Terrain settings
			string[] stringResOptions = { "32", "64", "128" };
			int[] resOptions = { 32, 64, 128 };

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Generation Settings", EditorStyles.boldLabel);
			Settings.EditorState.GenerateOnStart = EditorGUILayout.Toggle("Generate On Start", Settings.EditorState.GenerateOnStart);
			Settings.Generator.GenerationRadius = EditorGUILayout.IntField("Gen Radius", Settings.Generator.GenerationRadius);
			if (!Settings.EditorState.UseRandomSeed)
				TerraSettings.GenerationSeed = EditorGUILayout.IntField("Seed", TerraSettings.GenerationSeed);
			Settings.EditorState.UseRandomSeed = EditorGUILayout.Toggle("Use Random Seed", Settings.EditorState.UseRandomSeed);

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Mesh Settings", EditorStyles.boldLabel);
			Settings.Generator.MeshResolution = EditorGUILayout.IntPopup("Mesh Resolution", Settings.Generator.MeshResolution, stringResOptions, resOptions);
			Settings.Generator.Length = EditorGUILayout.IntField("Length", Settings.Generator.Length);
			Settings.Generator.Spread = EditorGUILayout.FloatField("Spread", Settings.Generator.Spread);
			Settings.Generator.Amplitude = EditorGUILayout.FloatField("Amplitude", Settings.Generator.Amplitude);

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Advanced", EditorStyles.boldLabel);
			if (!Settings.Generator.GenAllColliders)
				Settings.Generator.ColliderGenerationExtent = EditorGUILayout.FloatField("Collider Gen Extent", Settings.Generator.ColliderGenerationExtent);
			Settings.Generator.GenAllColliders = EditorGUILayout.Toggle("Gen All Colliders", Settings.Generator.GenAllColliders);

			EditorGUILayout.Space();
			EditorGUI.BeginChangeCheck();
			Settings.EditorState.DisplayPreview = EditorGUILayout.Toggle("Display Preview", Settings.EditorState.DisplayPreview);
			if (EditorGUI.EndChangeCheck()) {
				if (Settings.EditorState.DisplayPreview) {
					Settings.Preview.TriggerPreviewUpdate();
				} else {
					Settings.Preview.RemoveComponents();
				}
			}

			Settings.EditorState.UseMultithreading = EditorGUILayout.Toggle("Multithreaded", Settings.EditorState.UseMultithreading);
		}

		/// <summary>
		/// Displays GUI elements for the "Materials" tab
		/// </summary>
		public void Material() {
			EditorGUILayout.Space();

			//Use custom material instead
			Settings.EditorState.UseCustomMaterial = EditorGUILayout.Toggle("Custom Material", Settings.EditorState.UseCustomMaterial);

			if (Settings.EditorState.UseCustomMaterial) {
				Settings.CustomMaterial = (Material)EditorGUILayout.ObjectField("material", Settings.CustomMaterial, typeof(Material), false);
				return;
			}

			//Use textures
			if (Settings.SplatData != null) {
				//Display material list editor
				ReorderableListGUI.ListField(matList);

				//Advanced options
				Settings.EditorState.IsAdvancedFoldout = EditorGUILayout.Foldout(Settings.EditorState.IsAdvancedFoldout, "Advanced");
				if (Settings.EditorState.IsAdvancedFoldout) {
					EditorGUI.indentLevel = 1;
					EditorGUILayout.BeginHorizontal();
					Settings.Tessellation.UseTessellation = EditorGUILayout.Toggle("Tessellation", Settings.Tessellation.UseTessellation);
					if (GUILayout.Button("?", GUILayout.Width(25))) {
						const string msg = "Enabling tesselation can make your terrain mesh " +
											"appear to have a higher resolution without increasing " +
											"the resolution of the base mesh itself. Enabling this increases " +
											"the minimum required shader model to 4.6";
						EditorUtility.DisplayDialog("Help - Tessellation", msg, "Close");
					}
					EditorGUILayout.EndHorizontal();

					if (Settings.Tessellation.UseTessellation) {
						//TODO Use constant values
						Settings.Tessellation.TessellationAmount = EditorGUILayout.Slider("Amount", Settings.Tessellation.TessellationAmount, 1f, 32f);
						EditorGUILayout.MinMaxSlider("Distance", ref Settings.Tessellation.TessellationMinDistance,
													ref Settings.Tessellation.TessellationMaxDistance, 1f, 100f);

						//Display right aligned distance label
						GUIStyle style = GUI.skin.GetStyle("Label");
						style.alignment = TextAnchor.MiddleRight;

						string min = Settings.Tessellation.TessellationMinDistance.ToString("n1");
						string max = Settings.Tessellation.TessellationMaxDistance.ToString("n1");
						EditorGUILayout.LabelField("Min: " + min + " Max: " + max, style);
					}
					EditorGUI.indentLevel = 0;

				}

				//Use Grass
				Settings.Grass.PlaceGrass = EditorGUILayout.Toggle("Place Grass", Settings.Grass.PlaceGrass);
				if (Settings.Grass.PlaceGrass) {
					Settings.Grass.GrassStepLength = EditorGUILayout.Slider("Density", Settings.Grass.GrassStepLength, 1.5f, 30f);
					Settings.Grass.GrassVariation = EditorGUILayout.Slider("Variation", Settings.Grass.GrassVariation, 0f, 3f);
					Settings.Grass.GrassHeight = EditorGUILayout.Slider("Height", Settings.Grass.GrassHeight, 1f, 10f);
					Settings.Grass.BillboardDistance = EditorGUILayout.FloatField("Billboard Distance", Settings.Grass.BillboardDistance);
					Settings.Grass.ClipCutoff = EditorGUILayout.Slider("Clip Cutoff", Settings.Grass.ClipCutoff, 0.05f, 1f);

					Settings.Grass.GrassConstrainHeight = EditorGUILayout.Toggle("Constrain Height", Settings.Grass.GrassConstrainHeight);
					if (Settings.Grass.GrassConstrainHeight) {
						EditorGUI.indentLevel = 1;
						Settings.Grass.GrassMaxHeight = EditorGUILayout.FloatField("Max Height", Settings.Grass.GrassMaxHeight);
						Settings.Grass.GrassMinHeight = EditorGUILayout.FloatField("Min Height", Settings.Grass.GrassMinHeight);
						EditorGUI.indentLevel = 0;
					}

					Settings.Grass.GrassConstrainAngle = EditorGUILayout.Toggle("Constrain Angle", Settings.Grass.GrassConstrainAngle);
					if (Settings.Grass.GrassConstrainAngle) {
						EditorGUI.indentLevel = 1;
						EditorGUILayout.LabelField("Min Angle", Settings.Grass.GrassAngleMin.ToString("0") + " deg");
						EditorGUILayout.LabelField("Max Angle", Settings.Grass.GrassAngleMax.ToString("0") + " deg");
						EditorGUILayout.MinMaxSlider(ref Settings.Grass.GrassAngleMin, ref Settings.Grass.GrassAngleMax, 0f, 90f);
						EditorGUI.indentLevel = 0;
					}

					Settings.Grass.GrassTexture = (Texture2D)EditorGUILayout.ObjectField("Texture", Settings.Grass.GrassTexture, typeof(Texture2D), false);
				}

				EditorGUILayout.Space();
				if (Settings.EditorState.DisplayPreview && GUILayout.Button("Update Preview")) {
					Settings.Preview.TriggerMaterialsUpdate();
				}
			}

			if (GUILayout.Button("Add Material")) {
				if (Settings.SplatData == null)
					Settings.SplatData = new List<TerrainPaint.SplatData>();

				Settings.SplatData.Add(new TerrainPaint.SplatData());
			}
		}

		/// <summary>
		/// Displays GUI elements for the "Noise" tab
		/// </summary>
		public void Noise() {
			const int texMaxWidth = 188;
			const int texMinZoom = 20;
			const int texMaxZoom = 100;

			var mapTypes = new[] { Settings.HeightMapData, Settings.MoistureMapData, Settings.TemperatureMapData };
			bool updateTextures = mapTypes.Any(m => m.PreviewTexture == null);
			float texWidth = Settings.EditorState.InspectorWidth;
			
			texWidth = texWidth >= texMaxWidth ? texMaxWidth : texWidth;
			EditorGUIExtension.BeginBlockArea();

			for (int i = 0; i < mapTypes.Length; i++) {
				var md = mapTypes[i];
				bool updateThis = false; //Update THIS texture because of editor change?

				EditorGUI.BeginChangeCheck();

				var bold = new GUIStyle();
				bold.fontStyle = FontStyle.Bold;
				EditorGUILayout.LabelField(md.Name, bold);

				md.MapType = (TerraSettings.MapGeneratorType)EditorGUILayout.EnumPopup("Noise Type", md.MapType);
				md.TextureZoom = EditorGUILayout.Slider("Zoom", md.TextureZoom, texMinZoom, texMaxZoom);
				EditorGUILayout.Space();

				if (EditorGUI.EndChangeCheck()) updateThis = true;
				if (updateTextures || updateThis) md.UpdatePreviewTexture((int) texWidth, (int) (texWidth / 2));

				//Draw preview texture
				if (md.PreviewTexture != null) {
					var ctr = EditorGUILayout.GetControlRect(false, texWidth / 2);
					ctr.width = texWidth;
					ctr.x += 2;

					EditorGUI.DrawPreviewTexture(ctr, md.PreviewTexture);
				}

				if (i != mapTypes.Length - 1)
					EditorGUIExtension.AddSeperator();
			}

			EditorGUIExtension.EndBlockArea();
		}

		/// <summary>
		/// Displays GUI elements for the "Object Placement" tab
		/// </summary>
		public void ObjectPlacement() {
			//Display each type
			for (int i = 0; i < Settings.ObjectData.Count; i++) {
				EditorGUILayout.Space();

				//Surround each material w/ box
				GUIStyle boxStyle = new GUIStyle();
				boxStyle.padding = new RectOffset(3, 3, 3, 3);
				boxStyle.normal.background = GetWhiteTexture();
				EditorGUILayout.BeginVertical(boxStyle);

				ObjectPlacementData objectPlacementData = Settings.ObjectData[i];

				//Close button / name
				if (GUILayout.Button("X", GUILayout.Height(16), GUILayout.Width(18))) {
					Settings.ObjectData.RemoveAt(i);
					i--;
					continue;
				}

				//General
				objectPlacementData.Prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", objectPlacementData.Prefab, typeof(GameObject), false);
				objectPlacementData.AllowsIntersection = EditorGUILayout.Toggle("Can Intersect", objectPlacementData.AllowsIntersection);
				objectPlacementData.PlacementProbability = EditorGUILayout.IntSlider("Place Probability", objectPlacementData.PlacementProbability, 0, 100);
				objectPlacementData.Spread = EditorGUILayout.Slider("Object Spread", objectPlacementData.Spread, 5f, 50f);
				objectPlacementData.MaxObjects = EditorGUILayout.IntField("Max Objects", objectPlacementData.MaxObjects);
				if (objectPlacementData.MaxObjects < 1) objectPlacementData.MaxObjects = 1;

				//Height
				objectPlacementData.ConstrainHeight = EditorGUILayout.Toggle("Constrain Height", objectPlacementData.ConstrainHeight);
				if (objectPlacementData.ConstrainHeight) {
					EditorGUI.indentLevel = 1;

					objectPlacementData.MinHeight = EditorGUILayout.FloatField("Min Height", objectPlacementData.MinHeight);
					objectPlacementData.MaxHeight = EditorGUILayout.FloatField("Max Height", objectPlacementData.MaxHeight);

					FitMinMax(ref objectPlacementData.MinHeight, ref objectPlacementData.MaxHeight);

					EditorGUILayout.BeginHorizontal();
					objectPlacementData.HeightProbCurve = EditorGUILayout.CurveField("Probability", objectPlacementData.HeightProbCurve, Color.green, new Rect(0, 0, 1, 1));
					if (GUILayout.Button("?", GUILayout.Width(25))) {
						const string msg = "This is the height probability curve. The X axis represents the " +
											"min to max height and the Y axis represents the probability an " +
											"object will spawn. By default, the curve is set to a 100% probability " +
											"meaning all objects will spawn.";
						EditorUtility.DisplayDialog("Help - Height Probability", msg, "Close");
					}
					EditorGUILayout.EndHorizontal();

					EditorGUI.indentLevel = 0;
				}

				//Angle
				objectPlacementData.ConstrainAngle = EditorGUILayout.Toggle("Constrain Angle", objectPlacementData.ConstrainAngle);
				if (objectPlacementData.ConstrainAngle) {
					EditorGUI.indentLevel = 1;

					objectPlacementData.MinAngle = EditorGUILayout.FloatField("Min Angle", objectPlacementData.MinAngle);
					objectPlacementData.MaxAngle = EditorGUILayout.FloatField("Max Angle", objectPlacementData.MaxAngle);

					FitMinMax(ref objectPlacementData.MinAngle, ref objectPlacementData.MaxAngle);

					EditorGUILayout.BeginHorizontal();
					objectPlacementData.AngleProbCurve = EditorGUILayout.CurveField("Probability", objectPlacementData.AngleProbCurve, Color.green, new Rect(0, 0, 180, 1));
					if (GUILayout.Button("?", GUILayout.Width(25))) {
						const string msg = "This is the angle probability curve. The X axis represents " +
											"0 to 180 degrees and the Y axis represents the probability an " +
											"object will spawn. By default, the curve is set to a 100% probability " +
											"meaning all objects will spawn.";
						EditorUtility.DisplayDialog("Help - Angle Probability", msg, "Close");
					}
					EditorGUILayout.EndHorizontal();

					EditorGUI.indentLevel = 0;
				}

				//Translate
				EditorGUI.indentLevel = 1;
				objectPlacementData.ShowTranslateFoldout = EditorGUILayout.Foldout(objectPlacementData.ShowTranslateFoldout, "Translate");
				if (objectPlacementData.ShowTranslateFoldout) {
					objectPlacementData.TranslationAmount = EditorGUILayout.Vector3Field("Translate", objectPlacementData.TranslationAmount);

					EditorGUILayout.BeginHorizontal();
					objectPlacementData.IsRandomTranslate = EditorGUILayout.Toggle("Random", objectPlacementData.IsRandomTranslate);
					if (GUILayout.Button("?", GUILayout.Width(25))) {
						const string msg = "Optionally randomly translate the placed object. " +
											"Max and min extents for the random number generator can " +
											"be set.";
						EditorUtility.DisplayDialog("Help - Random Translate", msg, "Close");
					}
					EditorGUILayout.EndHorizontal();

					if (objectPlacementData.IsRandomTranslate) {
						EditorGUI.indentLevel = 2;

						objectPlacementData.RandomTranslateExtents.Min = EditorGUILayout.Vector3Field("Min", objectPlacementData.RandomTranslateExtents.Min);
						objectPlacementData.RandomTranslateExtents.Max = EditorGUILayout.Vector3Field("Max", objectPlacementData.RandomTranslateExtents.Max);

						FitMinMax(ref objectPlacementData.RandomTranslateExtents.Min, ref objectPlacementData.RandomTranslateExtents.Max);
						EditorGUI.indentLevel = 1;
					}
				}

				//Rotate
				objectPlacementData.ShowRotateFoldout = EditorGUILayout.Foldout(objectPlacementData.ShowRotateFoldout, "Rotate");
				if (objectPlacementData.ShowRotateFoldout) {
					objectPlacementData.RotationAmount = EditorGUILayout.Vector3Field("Rotation", objectPlacementData.RotationAmount);

					EditorGUILayout.BeginHorizontal();
					objectPlacementData.IsRandomRotation = EditorGUILayout.Toggle("Random", objectPlacementData.IsRandomRotation);
					if (GUILayout.Button("?", GUILayout.Width(25))) {
						const string msg = "Optionally randomly rotate the placed object. " +
											"Max and min extents for the random number generator can " +
											"be set.";
						EditorUtility.DisplayDialog("Help - Random Rotate", msg, "Close");
					}
					EditorGUILayout.EndHorizontal();

					if (objectPlacementData.IsRandomRotation) {
						EditorGUI.indentLevel = 2;

						objectPlacementData.RandomRotationExtents.Min = EditorGUILayout.Vector3Field("Min", objectPlacementData.RandomRotationExtents.Min);
						objectPlacementData.RandomRotationExtents.Max = EditorGUILayout.Vector3Field("Max", objectPlacementData.RandomRotationExtents.Max);

						FitMinMax(ref objectPlacementData.RandomRotationExtents.Min, ref objectPlacementData.RandomRotationExtents.Max);
						EditorGUI.indentLevel = 1;
					}
				}

				//Scale
				objectPlacementData.ShowScaleFoldout = EditorGUILayout.Foldout(objectPlacementData.ShowScaleFoldout, "Scale");
				if (objectPlacementData.ShowScaleFoldout) {
					objectPlacementData.ScaleAmount = EditorGUILayout.Vector3Field("Scale", objectPlacementData.ScaleAmount);

					EditorGUILayout.BeginHorizontal();
					objectPlacementData.IsRandomScale = EditorGUILayout.Toggle("Random", objectPlacementData.IsRandomScale);
					if (GUILayout.Button("?", GUILayout.Width(25))) {
						const string msg = "Optionally randomly scale the placed object. " +
											"Max and min extents for the random number generator can " +
											"be set.";
						EditorUtility.DisplayDialog("Help - Random Scale", msg, "Close");
					}
					EditorGUILayout.EndHorizontal();

					if (objectPlacementData.IsRandomScale) {
						objectPlacementData.IsUniformScale = EditorGUILayout.Toggle("Scale Uniformly", objectPlacementData.IsUniformScale);

						EditorGUI.indentLevel = 2;

						if (objectPlacementData.IsUniformScale) {
							objectPlacementData.UniformScaleMin = EditorGUILayout.FloatField("Min", objectPlacementData.UniformScaleMin);
							objectPlacementData.UniformScaleMax = EditorGUILayout.FloatField("Max", objectPlacementData.UniformScaleMax);
						} else {
							objectPlacementData.RandomScaleExtents.Min = EditorGUILayout.Vector3Field("Min", objectPlacementData.RandomScaleExtents.Min);
							objectPlacementData.RandomScaleExtents.Max = EditorGUILayout.Vector3Field("Max", objectPlacementData.RandomScaleExtents.Max);

							FitMinMax(ref objectPlacementData.RandomScaleExtents.Min, ref objectPlacementData.RandomScaleExtents.Max);
						}
						EditorGUI.indentLevel = 1;
					}
				}

				EditorGUI.indentLevel = 0;
				EditorGUILayout.EndVertical();
			}

			//Add new button
			EditorGUILayout.Space();
			if (GUILayout.Button("Add Object")) {
				if (Settings.ObjectData == null) {
					Settings.ObjectData = new List<ObjectPlacementData>();
				}

				Settings.ObjectData.Add(new ObjectPlacementData(TerraSettings.GenerationSeed));
			}

			//Update preview
			if (Settings.EditorState.DisplayPreview && GUILayout.Button("Update Preview")) {
				Settings.Preview.TriggerObjectPlacementUpdate();
			}
		}

		/// <summary>
		/// Fits the min and max values so that the min is never 
		/// greater than the max. 
		/// 
		/// If min > max
		///   min = max
		/// </summary>
		/// <param name="min">Minimum value</param>
		/// <param name="max">Maximum value</param>
		private static void FitMinMax(ref float min, ref float max) {
			min = min > max ? max : min;
		}

		/// <summary>
		/// Fits the min and max values so that the min Vector3's 
		/// components never exceed the max's.
		/// 
		/// If min > max
		///   min = max
		/// </summary>
		/// <param name="min">Minimum vector</param>
		/// <param name="max">Maximum vector</param>
		private static void FitMinMax(ref Vector3 min, ref Vector3 max) {
			if (min.x > max.x || min.y > max.y || min.z > max.z) {
				min = new Vector3(min.x > max.x ? max.x : min.x,
					min.y > max.y ? max.y : min.y,
					min.z > max.z ? max.z : min.z);
			}
		}

		/// <summary>
		/// Cached texture used by <code>GetWhiteTexture</code> method
		/// </summary>
		protected static Texture2D WhiteTex;

		/// <summary>
		/// Gets a cached white texture that can be used for GUI
		/// </summary>
		/// <returns>All white Texture instance</returns>
		private static Texture2D GetWhiteTexture() {
			if (WhiteTex == null) {
				WhiteTex = new Texture2D(1, 1);
				WhiteTex.SetPixel(0, 0, new Color(230f / 255f, 230f / 255f, 230f / 255f));
				WhiteTex.Apply();
			}

			return WhiteTex;
		}
	}

	public static class EditorGUILayoutExtensions {
		
	}
}