using System.Collections.Generic;
using Terra.Terrain;
using UnityEngine;

namespace UnityEditor.Terra {
	/// <summary>
	/// Handles displaying various GUI elements for the Terra
	/// Settings custom inspector
	/// </summary>
	public class TerraGUI {
		private TerraSettings Settings;
		private TerrainSettingsEditor SettingsEditor;
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
						new AddTextureWindow(ref splat);
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
			}
		}

		/// <summary>
		/// Displays GUI elements for the "Noise" tab
		/// </summary>
		/// <param name="manager">GraphManager instance for opening / closing graph</param>
		public void Noise(GraphManager manager) {
			EditorGUILayout.Space();

			if (Settings.SelectedFile != "") {
				if (manager.GraphFileCanBeRead(Settings.SelectedFile))
					if (manager.HasValidEndNode()) manager.OptionGraphOpenSuccess();
					else manager.MessageNoEndNode();
				else
					manager.OptionGraphOpenError();
			} else {
				manager.OptionIncorrectFileSelection();
			}
		}

		/// <summary>
		/// Cached texture used by <code>GetWhiteTexture</code> method
		/// </summary>
		private static Texture2D WhiteTex;

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