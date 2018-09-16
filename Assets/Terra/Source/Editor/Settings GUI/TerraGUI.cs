using System.Collections.Generic;
using Terra.ReorderableList;
using System.Linq;
using Assets.Terra.Source;
using Terra.Data;
using Terra.Graph.Noise;
using Terra.Terrain;
using UnityEngine;

namespace UnityEditor.Terra {
	/// <summary>
	/// Handles displaying various GUI elements for the Terra
	/// Settings custom inspector
	/// </summary>
	public class TerraGUI {
		/// <summary>
		/// Reorderable lists that are contained within a biome's detail data
		/// </summary>
		private struct DetailSubList {
			public ReorderableMaterialList MaterialsList;
			public ReorderableObjectList ObjectsList;

			public DetailSubList(TerraSettings settings, DetailData details) {
				ObjectsList = new ReorderableObjectList(settings, details);
				MaterialsList = new ReorderableMaterialList(settings, details);
			}

			public static bool operator ==(DetailSubList x, DetailSubList y) {
				return x.Equals(y);
			}

			public static bool operator !=(DetailSubList x, DetailSubList y) {
				return !x.Equals(y);
			}

			public override bool Equals(object obj) {
				return obj is DetailSubList && Equals((DetailSubList)obj);
			}

			public bool Equals(DetailSubList other) {
				return Equals(MaterialsList, other.MaterialsList) && Equals(ObjectsList, other.ObjectsList);
			}

			public override int GetHashCode() {
				unchecked {
					return ((MaterialsList != null ? MaterialsList.GetHashCode() : 0) * 397) ^ (ObjectsList != null ? ObjectsList.GetHashCode() : 0);
				}
			}
		}
	
		private TerraSettings _settings;
		private Texture[] _toolbarImages;

		private ReorderableBiomeList _biomeList;

		/// <summary>
		/// Each biome has an associated <see cref="ReorderableMaterialList"/> and 
		/// <see cref="ReorderableObjectList"/>. This structure keeps track of those 
		/// associations. 
		/// </summary>
		private List<KeyValuePair<BiomeData, DetailSubList>> _biomeDetails;


		public TerraGUI(TerraSettings settings) {
			this._settings = settings;

			_biomeList = new ReorderableBiomeList(settings);
			_biomeDetails = new List<KeyValuePair<BiomeData, DetailSubList>>();
		}

		/// <summary>
		/// Displays a toggleable toolbar with icons from 
		/// the file system
		/// </summary>
		public void Toolbar() {
			EditorGUILayout.Space();

			//Set toolbar images
			if (_toolbarImages == null) {
				_toolbarImages = new Texture[] {
					(Texture)Resources.Load("terra_gui_wrench"),
					(Texture)Resources.Load("terra_gui_map"),
					(Texture)Resources.Load("terra_gui_biome"),
					(Texture)Resources.Load("terra_gui_detail")
				};
			}

			_settings.EditorState.ToolbarSelection = (ToolbarOptions)EditorGUIExtension.EnumToolbar(_settings.EditorState.ToolbarSelection, _toolbarImages);
		}

		/// <summary>
		/// Displays GUI elements for the "General" tab
		/// </summary>
		public void General() {
			//Tracked gameobject
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Tracked GameObject", EditorStyles.boldLabel);
			_settings.Generator.TrackedObject = (GameObject)EditorGUILayout.ObjectField(_settings.Generator.TrackedObject, typeof(GameObject), true);

			//Terrain settings
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Generation Settings", EditorStyles.boldLabel);
			_settings.Generator.GenerateOnStart = EditorGUILayout.Toggle("Generate On Start", _settings.Generator.GenerateOnStart);
			_settings.Generator.GenerationRadius = EditorGUILayout.IntField("Gen Radius", _settings.Generator.GenerationRadius);
			if (!_settings.Generator.UseRandomSeed)
				TerraSettings.GenerationSeed = EditorGUILayout.IntField("Seed", TerraSettings.GenerationSeed);
			_settings.Generator.UseRandomSeed = EditorGUILayout.Toggle("Use Random Seed", _settings.Generator.UseRandomSeed);

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Mesh Settings", EditorStyles.boldLabel);

			//Show LOD info
			_settings.EditorState.IsLodFoldout = EditorGUILayout.Foldout(_settings.EditorState.IsLodFoldout, "Level of Detail");
			if (_settings.EditorState.IsLodFoldout) {
				EditorGUI.indentLevel++;

				EditorGUILayout.BeginHorizontal(new GUIStyle { margin = new RectOffset(12, 0, 0, 0)});
				var lod = _settings.Generator.Lod;
				lod.UseHighLodLevel = GUILayout.Toggle(lod.UseHighLodLevel, "High");
				lod.UseMediumLodLevel = GUILayout.Toggle(lod.UseMediumLodLevel, "Med");
				lod.UseLowLodLevel = GUILayout.Toggle(lod.UseLowLodLevel, "Low");
				EditorGUILayout.EndHorizontal();

				if (lod.UseHighLodLevel)
					lod.High = General_LodFoldout("High", lod.High);
				if (lod.UseMediumLodLevel)
					lod.Medium = General_LodFoldout("Medium", lod.Medium);
				if (lod.UseLowLodLevel)
					lod.Low = General_LodFoldout("Low", lod.Low);

				EditorGUI.indentLevel--;
			}

			_settings.Generator.Length = EditorGUILayout.IntField("Length", _settings.Generator.Length);
			_settings.Generator.Spread = EditorGUILayout.FloatField("Spread", _settings.Generator.Spread);
			_settings.Generator.Amplitude = EditorGUILayout.FloatField("Amplitude", _settings.Generator.Amplitude);

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Advanced", EditorStyles.boldLabel);
			if (!_settings.Generator.GenAllColliders)
				_settings.Generator.ColliderGenerationExtent = EditorGUILayout.FloatField("Collider Gen Extent", _settings.Generator.ColliderGenerationExtent);
			_settings.Generator.GenAllColliders = EditorGUILayout.Toggle("Gen All Colliders", _settings.Generator.GenAllColliders);
			_settings.Generator.UseMultithreading = EditorGUILayout.Toggle("Multithreaded", _settings.Generator.UseMultithreading);
		}

		/// <summary>
		/// Displays GUI elements for the "Maps" tab
		/// </summary>
		public void Maps() {
			const string mapDescription = "Create height, temperature, and moisture maps from various noise functions; or create your own custom map generator.";
			Header("Maps", mapDescription);

			const int texMaxWidth = 188;
			const int texMinZoom = 20;
			const int texMaxZoom = 100;

			var mapTypes = new[] { _settings.HeightMapData, _settings.MoistureMapData, _settings.TemperatureMapData };
			bool updateTextures = mapTypes.Any(m => m.PreviewTexture == null);

			float texWidth = _settings.EditorState.InspectorWidth;
			texWidth = texWidth >= texMaxWidth ? texMaxWidth : texWidth;

			//GUIHelper.Separator(EditorGUILayout.GetControlRect(false, 1));

			for (int i = 0; i < mapTypes.Length; i++) {
				EditorGUILayout.Space();

				var md = mapTypes[i];
				bool updateThis = false; //Update THIS texture because of editor change?

				EditorGUI.BeginChangeCheck();

				var bold = new GUIStyle();
				bold.fontStyle = FontStyle.Bold;
				EditorGUILayout.LabelField(md.Name, bold);

				EditorGUI.indentLevel++; //Indent following controls
				md.MapType = (MapGeneratorType)EditorGUILayout.EnumPopup("Noise Type", md.MapType);
				if (md.MapType == MapGeneratorType.Custom) {
					md.Graph = (NoiseGraph)EditorGUILayout.ObjectField("Noise Graph", md.Graph, typeof(NoiseGraph), false);
					//TODO Add "Create new" button under graph
				}
				
				md.TextureZoom = EditorGUILayout.Slider("Zoom", md.TextureZoom, texMinZoom, texMaxZoom);
				EditorGUILayout.Space();

				//Update texture if editor changed
				if (EditorGUI.EndChangeCheck()) updateThis = true;
				if (updateTextures || updateThis) {
					texWidth -= 20;
					md.UpdatePreviewTexture((int)texWidth, (int)(texWidth / 2));
				}

				//Draw preview texture
				if (md.PreviewTexture != null) {

					//Draw texture
					var ctr = EditorGUILayout.GetControlRect(false, texWidth / 2);
					ctr.width = texWidth;
					ctr.x += 17;

					EditorGUI.DrawPreviewTexture(ctr, md.PreviewTexture);

					//Draw color fields
					EditorGUILayout.BeginHorizontal();

					ctr = EditorGUILayout.GetControlRect(false, 16);
					ctr.width = (texWidth / 2) + 8;
					ctr.x += 2;

					md.RampColor1 = EditorGUI.ColorField(ctr, md.RampColor1);
					ctr.x += ctr.width - 2;
					md.RampColor2 = EditorGUI.ColorField(ctr, md.RampColor2);

					EditorGUILayout.EndHorizontal();
				}

				EditorGUILayout.Space();
				EditorGUI.indentLevel--;
				//if (i != mapTypes.Length - 1) 
				//	GUIHelper.Separator(EditorGUILayout.GetControlRect(false, 1));
			}
		}

		/// <summary>
		/// Displays GUI elements for the "Biomes" tab
		/// </summary>
		public void Biomes() {
			const string description = "Create biomes by building a list of constraints that define when a biome should appear.";
			Header("Biomes", description);

			//Blend between biomes amount
			_settings.Generator.BiomeBlendAmount = 
				EditorGUILayout.Slider("Biome Blend", _settings.Generator.BiomeBlendAmount, 0f, 30f);

			//Display material list editor
			ReorderableListGUI.ListField(_biomeList);

			//Calculate texture preview size
			const float texWidth = 128;
			float inspectorWidth = _settings.EditorState.InspectorWidth;

			//Previewing
			EditorGUILayout.Space();
			EditorGUI.indentLevel++;
			_settings.EditorState.ShowBiomePreview = EditorGUILayout.Foldout(_settings.EditorState.ShowBiomePreview, "Show Preview");
			if (_settings.EditorState.ShowBiomePreview) {
				if (_settings.EditorState.BiomePreview != null) {
					var ctr = EditorGUILayout.GetControlRect(false, texWidth);
					ctr.width = texWidth;
					ctr.x += 17;

					EditorGUI.DrawPreviewTexture(ctr, _settings.EditorState.BiomePreview);
				}

				//Zoom slider
				var margin = new GUIStyle { margin = new RectOffset(25, 0, 0, 0) };
				GUILayout.BeginHorizontal(margin);
				
				GUILayout.Label("Zoom", GUILayout.ExpandWidth(false));
				float labelWidth = GUI.skin.label.CalcSize(new GUIContent("Zoom")).x;
				_settings.EditorState.BiomePreviewZoom =
					GUILayout.HorizontalSlider(_settings.EditorState.BiomePreviewZoom, 10f, 75f, GUILayout.MaxWidth(texWidth - labelWidth));

				GUILayout.EndHorizontal();

				//Update preview button
				var rect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight);
				rect.x += 17;
				rect.y += 2;
				rect.width = texWidth;
				if (GUI.Button(rect, "Update Preview")) {
					float startSpread = _settings.Generator.Spread;
					_settings.Generator.Spread = _settings.EditorState.BiomePreviewZoom;

					TileMesh tm = new TileMesh(null, new LodData.LodLevel(0, 64, 64, 64));
					tm.CreateHeightmap(new GridPosition());

					WeightedBiomeMap map = new WeightedBiomeMap(tm, 64);
					map.CreateMap();
					_settings.EditorState.BiomePreview = map.GetPreviewTexture();

					_settings.Generator.Spread = startSpread;
				}
			}

			//Whittaker diagram
			_settings.EditorState.ShowWhittakerInfo = EditorGUILayout.Foldout(_settings.EditorState.ShowWhittakerInfo, "Whittaker Diagram");
			if (_settings.EditorState.ShowWhittakerInfo) {
				const string text = "Terra's biomes are based off of Whittaker's biome classification system. " +
									"You can read more below.";
				var linkStyle = new GUIStyle(GUI.skin.label) {
					normal = {textColor = Color.blue},
					padding = {left = 32}
				};

				EditorGUI.indentLevel++;
								
				EditorGUILayout.LabelField(text);

				if (GUILayout.Button("Whittaker Diagram", linkStyle)) {
					Application.OpenURL("https://en.wikipedia.org/wiki/File:Climate_influence_on_terrestrial_biome.svg");
				}
				if (GUILayout.Button("Biome Wikipedia", linkStyle)) {
					Application.OpenURL("https://en.wikipedia.org/wiki/Biome");
				}
				
				EditorGUI.indentLevel--;
			}

			EditorGUILayout.Space();
			EditorGUI.indentLevel--;
		}

		public void Details() {
			const string description = "Apply details to your biomes. These include textures, objects, and grass.";
			Header("Details", description);

			EditorGUIExtension.BeginBlockArea();

			for (var i = 0; i < _settings.BiomesData.Count; i++) {
				var biome = _settings.BiomesData[i];
				var detail = biome.Details;

				EditorGUILayout.BeginHorizontal();

				EditorGUILayout.LabelField(biome.Name, EditorGUIExtension.TerraStyle.TextTitle);

				//Show color preview
				var bg = new Texture2D(1, 1);
				bg.SetPixel(1, 1, biome.Color);
				bg.Apply();

				var bgStyle = new GUIStyle();
				bgStyle.normal.background = bg;
				bgStyle.padding.bottom = (int) EditorGUIUtility.singleLineHeight;

				EditorGUILayout.BeginVertical(bgStyle);
				EditorGUILayout.EndVertical();

				EditorGUILayout.EndHorizontal();

				EditorGUILayout.Space();
				EditorGUI.indentLevel++;

				detail.ShowMaterialFoldout = EditorGUILayout.Foldout(detail.ShowMaterialFoldout, "Textures");
				if (detail.ShowMaterialFoldout) {
					EditorGUI.indentLevel--;
					Detail_Texture(biome);
					EditorGUI.indentLevel++;
				}
				
				detail.ShowObjectFoldout = EditorGUILayout.Foldout(detail.ShowObjectFoldout, "Objects");
				if (detail.ShowObjectFoldout) {
					EditorGUI.indentLevel--;
					Detail_Object(biome);
					EditorGUI.indentLevel++;
				}
				
				detail.ShowGrassFoldout = EditorGUILayout.Foldout(detail.ShowGrassFoldout, "Grass");
				EditorGUI.indentLevel--;

				if (i < _settings.BiomesData.Count - 1)
					EditorGUIExtension.AddBlockAreaSeperator();
			}
			
			EditorGUIExtension.EndBlockArea();
			EditorGUILayout.Space();

			//Shader Settings
			const string desc = "Scale up textures in the distance to hide texture tiling.";
			Header("Distance Texture Scaling", desc);

			ShaderData sd = _settings.ShaderData;
			sd.FarScaleMultiplier = EditorGUILayout.FloatField("Far Tex Scale", sd.FarScaleMultiplier);
			sd.TransitionStart = EditorGUILayout.FloatField("Blend Start", sd.TransitionStart);
			sd.TransitionFalloff = EditorGUILayout.FloatField("Blend Falloff", sd.TransitionFalloff);
		}

		/// <summary>
		/// Display GUI for updating the preview shown in the editor
		/// </summary>
		public void PreviewUpdate() {
			EditorGUILayout.Space();
			EditorGUILayout.Separator();
			EditorGUILayout.Space();

			if (GUILayout.Button("Generate")) {
				if (_settings.Generator.GenerationRadius > 4) {
					int amt = TilePool.GetTilePositionsFromRadius(
						_settings.Generator.GenerationRadius, new GridPosition(0, 0), _settings.Generator.Length)
						.Count;
					string msg = "You are about to generate " + amt + " Tiles synchronously which " +
									   "may take a while. Are you sure you want to continue?";
					if (EditorUtility.DisplayDialog("Warning", msg, "Continue")) {
						_settings.GenerateEditor();
					}
				} else {
					_settings.GenerateEditor();
				}
			}
			if (GUILayout.Button("Clear Tiles")) {
				if (_settings.Generator != null && _settings.Generator.Pool != null) {
					_settings.Generator.Pool.RemoveAll();
				}
			}
		}

		/// <summary>
		/// Displays information useful to debugging Terra
		/// </summary>
		public void Debug() {
			// ReSharper disable once ConditionIsAlwaysTrueOrFalse
#pragma warning disable 162 //Unreachable Code
			if (TerraSettings.TerraDebug.WRITE_SPLAT_TEXTURES) {
				TerraSettings.TerraDebug.MAX_TEXTURE_WRITE_COUNT =
					EditorGUILayout.IntField("Tex Count", TerraSettings.TerraDebug.MAX_TEXTURE_WRITE_COUNT);
			}
#pragma warning restore 162
		}

		/// <summary>
		/// Displays a GUI for the passed <see cref="LodData.LodLevel"/> 
		/// and modifies its values.
		/// </summary>
		/// <param name="name">String name to display on the foldout</param>
		/// <param name="level">LodLevel to modify</param>
		private LodData.LodLevel General_LodFoldout(string name, LodData.LodLevel level) {
			EditorGUILayout.Foldout(true, name);
			EditorGUI.indentLevel++;

			string[] strMeshResOpts = { "32", "64", "128" };
			string[] strMapResOpts = { "32", "64", "128", "256" };

			int[] meshResOpts = { 32, 64, 128 };
			int[] mapResOpts = { 32, 64, 128, 256 };

			level.StartRadius = EditorGUILayout.IntField("Start Radius", level.StartRadius);
			level.MapResolution = EditorGUILayout.IntPopup("Map Res", level.MapResolution, strMapResOpts, mapResOpts);
			level.SplatmapResolution = EditorGUILayout.IntPopup("Splat Res", level.SplatmapResolution, strMapResOpts, mapResOpts);
			level.MeshResolution = EditorGUILayout.IntPopup("Mesh Res", level.MeshResolution, strMeshResOpts, meshResOpts);
			EditorGUI.indentLevel--;

			return level;
		}

		/// <summary>
		/// Displays GUI elements for the "Textures" foldout under 
		/// the detail tab
		/// </summary>
		private void Detail_Texture(BiomeData biome) {
			//Use textures
			if (_settings.Details != null) {
				var detailSubList = AddSubListIfNeeded(biome);
				ReorderableListGUI.ListField(detailSubList.MaterialsList);
			}
		}

		/// <summary>
		/// Displays GUI elements for the "Object Placement" tab
		/// </summary>
		private void Detail_Object(BiomeData biome) {
			//Use objects list
			if (_settings.Details != null) {
				var detailSubList = AddSubListIfNeeded(biome);
				ReorderableListGUI.ListField(detailSubList.ObjectsList);
			}
		}

		/// <summary>
		/// The detail lists from <see cref="_biomeDetails"/> that 
		/// have the passed biome as a key.
		/// </summary>
		/// <returns>Detail sub list if found, default(<see cref="DetailSubList"/>) otherwise</returns>
		private DetailSubList GetDetailListsFor(BiomeData biome) {
			foreach (var kv in _biomeDetails) {
				if (kv.Key == biome)
					return kv.Value;
			}

			return default(DetailSubList);
		}

		/// <summary>
		/// Adds the passed biome to <see cref="_biomeDetails"/> if it is 
		/// not in there already. Returns the associated 
		/// <see cref="DetailSubList" />.
		/// </summary>
		private DetailSubList AddSubListIfNeeded(BiomeData biome) {
			var subList = GetDetailListsFor(biome);
			if (subList.Equals(default(DetailSubList))) {
				subList = new DetailSubList(_settings, biome.Details);
				_biomeDetails.Add(new KeyValuePair<BiomeData, DetailSubList>(biome, subList));
			}

			return subList;
		}

		/// <summary>
		/// Displays a header underneath the toolbar
		/// </summary>
		/// <param name="title">Title of toolbar option</param>
		/// <param name="description">Description under title</param>
		private void Header(string title, string description) {
			EditorGUILayout.Space();
			EditorGUILayout.LabelField(title, EditorGUIExtension.TerraStyle.TextTitle);

			//Description readonly text area
			if (!string.IsNullOrEmpty(description)) {
				EditorStyles.label.wordWrap = true;
				EditorGUILayout.LabelField(description, GUILayout.ExpandWidth(false));
			}

			EditorGUILayout.Space();
		}
	}
}