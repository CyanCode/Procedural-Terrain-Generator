﻿using Assets.Terra.Nodes;
using System.Collections.Generic;
using Terra.Terrain;
using UNEB;
using UnityEngine;

namespace UnityEditor.Terra {
	/// <summary>
	/// Handles displaying various GUI elements for the Terra
	/// Settings custom inspector
	/// </summary>
	public class TerraGUI {
		private TerraSettings Settings;
		private Texture[] ToolbarImages;

		public TerraGUI(TerraSettings settings) {
			this.Settings = settings;
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
			
			Settings.ToolbarSelection = (TerraSettings.ToolbarOptions)EditorGUIExtension.EnumToolbar(Settings.ToolbarSelection, ToolbarImages);
		}

		/// <summary>
		/// Displays GUI elements for the "General" tab
		/// </summary>
		public void General() {
			//Tracked gameobject
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Tracked GameObject", EditorStyles.boldLabel);
			Settings.TrackedObject = (GameObject)EditorGUILayout.ObjectField(Settings.TrackedObject, typeof(GameObject), true);

			//Terrain settings
			string[] stringResOptions = { "32", "64", "128" };
			int[] resOptions = { 32, 64, 128 };

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Generation Settings", EditorStyles.boldLabel);
			Settings.GenerateOnStart = EditorGUILayout.Toggle("Generate On Start", Settings.GenerateOnStart);
			Settings.GenerationRadius = EditorGUILayout.IntField("Gen Radius", Settings.GenerationRadius);
			if (!Settings.UseRandomSeed)
				TerraSettings.GenerationSeed = EditorGUILayout.IntField("Seed", TerraSettings.GenerationSeed);
			Settings.UseRandomSeed = EditorGUILayout.Toggle("Use Random Seed", Settings.UseRandomSeed);

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Mesh Settings", EditorStyles.boldLabel);
			Settings.MeshResolution = EditorGUILayout.IntPopup("Mesh Resolution", Settings.MeshResolution, stringResOptions, resOptions);
			Settings.Length = EditorGUILayout.IntField("Length", Settings.Length);
			Settings.Spread = EditorGUILayout.FloatField("Spread", Settings.Spread);
			Settings.Amplitude = EditorGUILayout.FloatField("Amplitude", Settings.Amplitude);

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Advanced", EditorStyles.boldLabel);
			if (!Settings.GenAllColliders)
				Settings.ColliderGenerationExtent = EditorGUILayout.FloatField("Collider Gen Extent", Settings.ColliderGenerationExtent);
			Settings.GenAllColliders = EditorGUILayout.Toggle("Gen All Colliders", Settings.GenAllColliders);

			EditorGUILayout.Space();
			EditorGUI.BeginChangeCheck();
			Settings.DisplayPreview = EditorGUILayout.Toggle("Display Preview", Settings.DisplayPreview);
			if (EditorGUI.EndChangeCheck()) {
				if (Settings.DisplayPreview) {
					Settings.Preview.TriggerPreviewUpdate();
				} else {
					Settings.Preview.RemoveComponents();
				}
			}

			Settings.UseMultithreading = EditorGUILayout.Toggle("Multithreaded", Settings.UseMultithreading);
		}

		/// <summary>
		/// Displays GUI elements for the "Materials" tab
		/// </summary>
		public void Material() {
			EditorGUILayout.Space();

			//Use custom material instead
			Settings.UseCustomMaterial = EditorGUILayout.Toggle("Custom Material", Settings.UseCustomMaterial);

			if (Settings.UseCustomMaterial) {
				Settings.CustomMaterial = (Material)EditorGUILayout.ObjectField("material", Settings.CustomMaterial, typeof(Material), false);
				return;
			}

			//Use textures
			if (Settings.SplatSettings != null) {
				for (int i = 0; i < Settings.SplatSettings.Count; i++) {
					TerrainPaint.SplatSetting splat = Settings.SplatSettings[i];

					//Surround each material w/ box
					GUIStyle boxStyle = new GUIStyle();
					boxStyle.padding = new RectOffset(3, 3, 3, 3);
					boxStyle.normal.background = GetWhiteTexture();
					EditorGUILayout.BeginVertical(boxStyle);

					//Close button / name
					EditorGUILayout.BeginHorizontal();
					if (GUILayout.Button("X", GUILayout.Height(16), GUILayout.Width(18))) {
						Settings.SplatSettings.RemoveAt(i);
						i--;
						continue;
					}
					//EditorGUILayout.LabelField((splat.PlacementType == PlacementType.Angle ? "Angled" : "Elevation") + " Material " + (i + 1));
					EditorGUILayout.LabelField("Elevation Material " + (i + 1));
					EditorGUILayout.EndHorizontal();

					//Material settings
					EditorGUILayout.Space();

					if (splat.Diffuse == null) {
						EditorGUILayout.HelpBox("This splat material does not have a selected diffuse texture.", MessageType.Warning);
					} else {
						EditorGUILayout.BeginHorizontal();
						EditorGUILayout.BeginVertical();

						splat.Diffuse = (Texture2D)EditorGUILayout.ObjectField(splat.Diffuse,
							typeof(Texture2D), false, GUILayout.Width(80), GUILayout.Height(80));
						EditorGUILayout.LabelField("Diffuse", GUILayout.Width(60));
						EditorGUILayout.EndVertical();

						if (splat.Normal != null) {
							EditorGUILayout.BeginVertical();
							splat.Normal = (Texture2D)EditorGUILayout.ObjectField(splat.Normal,
								typeof(Texture2D), false, GUILayout.Width(80), GUILayout.Height(80));
							EditorGUILayout.LabelField("Normal", GUILayout.Width(60));
							EditorGUILayout.EndVertical();
						}

						EditorGUILayout.EndHorizontal();
					}
					if (GUILayout.Button("Edit Material")) {
						AddTextureWindow.Init(ref splat);
					}

					EditorGUILayout.Space();

					//Blend factor
					splat.Blend = EditorGUILayout.FloatField("Blend Amount", splat.Blend);

					//GUI for different types
					splat.PlacementType = (TerrainPaint.PlacementType)EditorGUILayout.EnumPopup("Placement Type", splat.PlacementType);
					switch (splat.PlacementType) {
						case TerrainPaint.PlacementType.Angle:
							EditorGUILayout.LabelField("Min Angle", splat.AngleMin.ToString("0") + " deg");
							EditorGUILayout.LabelField("Max Angle", splat.AngleMax.ToString("0") + " deg");
							EditorGUILayout.MinMaxSlider(ref splat.AngleMin, ref splat.AngleMax, 0f, 90f);
							break;
						case TerrainPaint.PlacementType.ElevationRange:
							if (!splat.IsMaxHeight)
								splat.MaxRange = EditorGUILayout.FloatField("Max Height", splat.MaxRange);
							if (!splat.IsMinHeight)
								splat.MinRange = EditorGUILayout.FloatField("Min Height", splat.MinRange);

							//Checkboxes for infinity & -infinity heights
							EditorGUI.BeginChangeCheck();
							if (splat.IsMaxHeight || !Settings.IsMaxHeightSelected) splat.IsMaxHeight = EditorGUILayout.Toggle("Is Highest Material", splat.IsMaxHeight);
							if (EditorGUI.EndChangeCheck())
								Settings.IsMaxHeightSelected = splat.IsMaxHeight;

							EditorGUI.BeginChangeCheck();
							if (splat.IsMinHeight || !Settings.IsMinHeightSelected) splat.IsMinHeight = EditorGUILayout.Toggle("Is Lowest Material", splat.IsMinHeight);
							if (EditorGUI.EndChangeCheck())
								Settings.IsMinHeightSelected = splat.IsMinHeight;

							//if (splat.MinRange > splat.MaxRange) splat.MinRange = splat.MaxRange;
							break;
					}

					EditorGUILayout.EndVertical();
					EditorGUILayout.Separator();
				}

				if (GUILayout.Button("Add Material")) {
					if (Settings.SplatSettings == null)
						Settings.SplatSettings = new List<TerrainPaint.SplatSetting>();

					Settings.SplatSettings.Add(new TerrainPaint.SplatSetting());
				}
				
				if (Settings.DisplayPreview && GUILayout.Button("Update Preview")) {
					Settings.Preview.TriggerMaterialsUpdate();
				}
			}
		}

		/// <summary>
		/// Displays GUI elements for the "Noise" tab
		/// </summary>
		public void Noise() {
			GraphManager manager = Settings.Manager;
			manager.Update();

			EditorGUILayout.Space();	

			//Check if graph is loaded and assign generator
			//from end node
			if (manager.Graph == null) {
				const string msg = "A node graph asset file must be attached to 'Graph' before " +
					"terrain can be generated.";
				EditorGUILayout.HelpBox(msg, MessageType.Warning);
			}
			
			//Display message if the generator failed to load
			//OR display message about a successful load
			if (manager.Graph != null && Settings.Manager.GetEndGenerator() == null) {
				const string msg = "The attached node graph either does not have a supplied End node " +
					"or the End node is missing its input.";
				EditorGUILayout.HelpBox(msg, MessageType.Warning);
			} else if (Settings.Manager.GetEndGenerator() != null) {
				const string msg = "Hooray! The attached node graph is ready for use.";
				EditorGUILayout.HelpBox(msg, MessageType.Info);
			}

			EditorGUILayout.Space();
			manager.Graph = (NodeGraph) EditorGUILayout.ObjectField("Graph", manager.Graph, typeof(NodeGraph), false);
			EditorGUILayout.Space();

			Settings.Spread = EditorGUILayout.FloatField("Spread", Settings.Spread);
			Settings.Amplitude = EditorGUILayout.FloatField("Amplitude", Settings.Amplitude);

			EditorGUILayout.Space();
			if (Application.isEditor && Settings.DisplayPreview) {
				if (GUILayout.Button("Update Preview")) {
					Settings.Preview.TriggerPreviewUpdate();
				}
			}
		}

		/// <summary>
		/// Displays GUI elements for the "Object Placement" tab
		/// </summary>
		public void ObjectPlacement() {
			//Display each type
			for (int i = 0; i < Settings.ObjectPlacementSettings.Count; i++) {
				EditorGUILayout.Space();

				//Surround each material w/ box
				GUIStyle boxStyle = new GUIStyle();
				boxStyle.padding = new RectOffset(3, 3, 3, 3);
				boxStyle.normal.background = GetWhiteTexture();
				EditorGUILayout.BeginVertical(boxStyle);

				ObjectPlacementType type = Settings.ObjectPlacementSettings[i];

				//Close button / name
				if (GUILayout.Button("X", GUILayout.Height(16), GUILayout.Width(18))) {
					Settings.ObjectPlacementSettings.RemoveAt(i);
					i--;
					continue;
				}

				//General
				type.Prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", type.Prefab, typeof(GameObject), false);
				type.AllowsIntersection = EditorGUILayout.Toggle("Can Intersect", type.AllowsIntersection);
				type.PlacementProbability = EditorGUILayout.IntSlider("Place Probability", type.PlacementProbability, 0, 100);
				type.Spread = EditorGUILayout.Slider("Object Spread", type.Spread, 5f, 50f);
				type.MaxObjects = EditorGUILayout.IntField("Max Objects", type.MaxObjects);
				if (type.MaxObjects < 1) type.MaxObjects = 1;

				//Height
				type.ConstrainHeight = EditorGUILayout.Toggle("Constrain Height", type.ConstrainHeight);
				if (type.ConstrainHeight) {
					EditorGUI.indentLevel = 1;
					
					type.MinHeight = EditorGUILayout.DelayedFloatField("Min Height", type.MinHeight);
					type.MaxHeight = EditorGUILayout.DelayedFloatField("Max Height", type.MaxHeight);

					FitMinMax(ref type.MinHeight, ref type.MaxHeight);

					EditorGUILayout.BeginHorizontal();
					type.HeightProbCurve = EditorGUILayout.CurveField("Probability", type.HeightProbCurve, Color.green, new Rect(0, 0, 1, 1));
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
				type.ConstrainAngle = EditorGUILayout.Toggle("Constrain Angle", type.ConstrainAngle);
				if (type.ConstrainAngle) {
					EditorGUI.indentLevel = 1;

					type.MinAngle = EditorGUILayout.DelayedFloatField("Min Angle", type.MinAngle);
					type.MaxAngle = EditorGUILayout.DelayedFloatField("Max Angle", type.MaxAngle);
					
					FitMinMax(ref type.MinAngle, ref type.MaxAngle);

					EditorGUILayout.BeginHorizontal();
					type.AngleProbCurve = EditorGUILayout.CurveField("Probability", type.AngleProbCurve, Color.green, new Rect(0, 0, 180, 1));
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
				Settings.ShowTranslateFoldout = EditorGUILayout.Foldout(Settings.ShowTranslateFoldout, "Translate");
				if (Settings.ShowTranslateFoldout) {
					type.TranslationAmount = EditorGUILayout.Vector3Field("Translate", type.TranslationAmount);

					EditorGUILayout.BeginHorizontal();
					type.IsRandomTranslate = EditorGUILayout.Toggle("Random", type.IsRandomTranslate);
					if (GUILayout.Button("?", GUILayout.Width(25))) {
						const string msg = "Optionally randomly translate the placed object. " +
											"Max and min extents for the random number generator can " +
											"be set.";
						EditorUtility.DisplayDialog("Help - Random Translate", msg, "Close");
					}
					EditorGUILayout.EndHorizontal();

					if (type.IsRandomTranslate) {
						EditorGUI.indentLevel = 2;

					 	type.RandomTranslateExtents.Min = EditorGUILayout.Vector3Field("Min", type.RandomTranslateExtents.Min);
						type.RandomTranslateExtents.Max = EditorGUILayout.Vector3Field("Max", type.RandomTranslateExtents.Max);

						FitMinMax(ref type.RandomTranslateExtents.Min, ref type.RandomTranslateExtents.Max);
						EditorGUI.indentLevel = 1;
					}
				}

				//Rotate
				Settings.ShowRotateFoldout = EditorGUILayout.Foldout(Settings.ShowRotateFoldout, "Rotate");
				if (Settings.ShowRotateFoldout) {
					type.RotationAmount = EditorGUILayout.Vector3Field("Rotation", type.RotationAmount);

					EditorGUILayout.BeginHorizontal();
					type.IsRandomRotation = EditorGUILayout.Toggle("Random", type.IsRandomRotation);
					if (GUILayout.Button("?", GUILayout.Width(25))) {
						const string msg = "Optionally randomly rotate the placed object. " +
											"Max and min extents for the random number generator can " +
											"be set.";
						EditorUtility.DisplayDialog("Help - Random Rotate", msg, "Close");
					}
					EditorGUILayout.EndHorizontal();

					if (type.IsRandomRotation) {
						EditorGUI.indentLevel = 2;

						type.RandomRotationExtents.Min = EditorGUILayout.Vector3Field("Min", type.RandomRotationExtents.Min);
						type.RandomRotationExtents.Max = EditorGUILayout.Vector3Field("Max", type.RandomRotationExtents.Max);

						FitMinMax(ref type.RandomRotationExtents.Min, ref type.RandomRotationExtents.Max);
						EditorGUI.indentLevel = 1;
					}
				}

				//Scale
				Settings.ShowScaleFoldout = EditorGUILayout.Foldout(Settings.ShowScaleFoldout, "Scale");
				if (Settings.ShowScaleFoldout) {
					type.ScaleAmount = EditorGUILayout.Vector3Field("Scale", type.ScaleAmount);

					EditorGUILayout.BeginHorizontal();
					type.IsRandomScale = EditorGUILayout.Toggle("Random", type.IsRandomScale);
					if (GUILayout.Button("?", GUILayout.Width(25))) {
						const string msg = "Optionally randomly scale the placed object. " +
											"Max and min extents for the random number generator can " +
											"be set.";
						EditorUtility.DisplayDialog("Help - Random Scale", msg, "Close");
					}
					EditorGUILayout.EndHorizontal();

					if (type.IsRandomScale) {
						type.IsUniformScale = EditorGUILayout.Toggle("Scale Uniformly", type.IsUniformScale);

						EditorGUI.indentLevel = 2;

						if (type.IsUniformScale) {
							type.UniformScaleMin = EditorGUILayout.FloatField("Min", type.UniformScaleMin);
							type.UniformScaleMax = EditorGUILayout.FloatField("Max", type.UniformScaleMax);
						} else {
							type.RandomScaleExtents.Min = EditorGUILayout.Vector3Field("Min", type.RandomScaleExtents.Min);
							type.RandomScaleExtents.Max = EditorGUILayout.Vector3Field("Max", type.RandomScaleExtents.Max);

							FitMinMax(ref type.RandomScaleExtents.Min, ref type.RandomScaleExtents.Max);
						}
						EditorGUI.indentLevel = 1;
					}
				}

				EditorGUILayout.EndVertical();
			}

			//Add new button
			EditorGUILayout.Space();
			if (GUILayout.Button("Add Object")) {
				if (Settings.ObjectPlacementSettings == null) {
					Settings.ObjectPlacementSettings = new List<ObjectPlacementType>();
				}

				Settings.ObjectPlacementSettings.Add(new ObjectPlacementType(TerraSettings.GenerationSeed));
			}

			//Update preview
			if (Settings.DisplayPreview && GUILayout.Button("Update Preview")) {
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
		public static void FitMinMax(ref float min, ref float max) {
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
		public static void FitMinMax(ref Vector3 min, ref Vector3 max) {
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
}