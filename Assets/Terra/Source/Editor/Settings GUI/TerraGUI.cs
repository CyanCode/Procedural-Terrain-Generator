using System.Collections.Generic;
using Terra.ReorderableList;
using System.Linq;
using Terra;
using Terra.Graph;
using Terra.Structures;
using Terra.Graph.Generators;
using Terra.Terrain;
using UnityEngine;
using XNodeEditor;

namespace UnityEditor.Terra {
	/// <summary>
	/// Handles displaying various GUI elements for the Terra
	/// Settings custom inspector
	/// </summary>
	public class TerraGUI {
	    private TerraConfig _config;
		private Texture[] _toolbarImages;



		public TerraGUI(TerraConfig config) {
			this._config = config;
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

			_config.EditorState.ToolbarSelection = (ToolbarOptions)EditorGUIExtension.EnumToolbar(_config.EditorState.ToolbarSelection, _toolbarImages);
		}

		/// <summary>
		/// Displays GUI elements for the "General" tab
		/// </summary>
		public void General() {
			//Tracked gameobject
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Tracked GameObject", EditorStyles.boldLabel);
			_config.Generator.TrackedObject = (GameObject)EditorGUILayout.ObjectField(_config.Generator.TrackedObject, typeof(GameObject), true);

			//Terrain settings
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Generation Settings", EditorStyles.boldLabel);
			_config.Generator.GenerateOnStart = EditorGUILayout.Toggle("Generate On Start", _config.Generator.GenerateOnStart);
			_config.Generator.GenerationRadius = EditorGUILayout.IntField("Gen Radius", _config.Generator.GenerationRadius);
			_config.Generator.LodChangeRadius = EditorGUILayout.FloatField("LOD Change Radius", _config.Generator.LodChangeRadius);
			if (!_config.Generator.UseRandomSeed)
				TerraConfig.Instance.Seed = EditorGUILayout.IntField("Seed", TerraConfig.Instance.Seed);
			_config.Generator.UseRandomSeed = EditorGUILayout.Toggle("Use Random Seed", _config.Generator.UseRandomSeed);

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Mesh Settings", EditorStyles.boldLabel);

			//Used for LOD display and remap heightmap display
			int[] mapResOpts = { 32, 64, 128, 256, 512 };
			string[] strResOpts = { "32", "64", "128", "256", "512" };
			int[] heightmapResOpts = { 33, 65, 129, 257, 513 };

			//Show LOD info
			_config.EditorState.IsLodFoldout = EditorGUILayout.Foldout(_config.EditorState.IsLodFoldout, "Level of Detail");
			if (_config.EditorState.IsLodFoldout) {
				EditorGUI.indentLevel++;
				
				var lod = _config.Generator.Lod;

				//Enforce >= 1 Lod Count
				EditorGUI.BeginChangeCheck();
				_config.Generator.LodCount = EditorGUILayout.IntField("LOD Count", _config.Generator.LodCount);
				if (_config.Generator.LodCount < 1) {
					_config.Generator.LodCount = 1;
				}
				if (EditorGUI.EndChangeCheck()) {
					lod.AdjustLevelsToCount(_config.Generator.LodCount);
				}

				EditorGUI.BeginChangeCheck(); 
				for (int i = 0; i < lod.LevelsOfDetail.Length; i++) {
					var level = lod.LevelsOfDetail[i]; 
					EditorGUILayout.LabelField("Level " + (i + 1));
				
					EditorGUI.indentLevel++;
					lod.LevelsOfDetail[i].StartRadius = EditorGUILayout.IntField("Start Radius", level.StartRadius);
					lod.LevelsOfDetail[i].Resolution = EditorGUILayout.IntPopup("Resolution", level.Resolution, strResOpts, heightmapResOpts);
					EditorGUI.indentLevel--;
				}
				if (EditorGUI.EndChangeCheck()) {
					lod.SortByStartRadius();
				}

				EditorGUI.indentLevel--;
			}

			_config.Generator.Length = EditorGUILayout.IntField("Length", _config.Generator.Length);
			_config.Generator.Amplitude = EditorGUILayout.FloatField("Amplitude", _config.Generator.Amplitude);
			_config.Generator.Spread = EditorGUILayout.FloatField("Spread", _config.Generator.Spread);

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Advanced", EditorStyles.boldLabel);
			if (!_config.Generator.GenAllColliders)
				_config.Generator.ColliderGenerationExtent = EditorGUILayout.FloatField("Collider Gen Extent", _config.Generator.ColliderGenerationExtent);
			_config.Generator.GenAllColliders = EditorGUILayout.Toggle("Gen All Colliders", _config.Generator.GenAllColliders);
			_config.Generator.UseMultithreading = EditorGUILayout.Toggle("Multithreaded", _config.Generator.UseMultithreading);

			EditorGUILayout.BeginHorizontal();
			_config.Generator.RemapHeightmap = EditorGUILayout.Toggle("Remap Heightmap", _config.Generator.RemapHeightmap);
			if (GUILayout.Button("?", GUILayout.Width(25))) {
				const string msg = "Heightmaps contain values in the range of 0 to 1. Because " +
				                   "noise functions can fall outside of this range, each retrieved value must " +
				                   "be normalized. Enabling this computes the linear transformation to apply " +
				                   "to the heightmap. This should be checked in almost all cases.";
				EditorUtility.DisplayDialog("Help - Calculate Heightmap Transformation", msg, "Close");
			}
			EditorGUILayout.EndHorizontal();
			
			_config.Generator.RemapResolution = EditorGUILayout.IntPopup("Remap Resolution", _config.Generator.RemapResolution, strResOpts, mapResOpts);

			//Preview settings
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Show Previews", EditorStyles.boldLabel);

			_config.EditorState.ShowLodGrid = EditorGUILayout.Toggle("LOD Grid", _config.EditorState.ShowLodGrid);
			_config.EditorState.ShowLodCubes = EditorGUILayout.Toggle("LOD Cubes", _config.EditorState.ShowLodCubes);
			_config.EditorState.ShowLodChangeRadius = EditorGUILayout.Toggle("LOD Change Radius", _config.EditorState.ShowLodChangeRadius);
		}

	    /// <summary>
		/// Display GUI for updating the preview shown in the editor
		/// </summary>
		public void PreviewUpdate() {
			EditorGUILayout.Space();
			EditorGUILayout.Separator();
			EditorGUILayout.Space();

			if (GUILayout.Button("Generate")) {
				if (_config.Generator.GenerationRadius > 4) {
					int amt = TilePool.GetTilePositionsFromRadius(
						_config.Generator.GenerationRadius, new Vector2(0, 0), _config.Generator.Length)
						.Count;
					string msg = "You are about to generate " + amt + " Tiles synchronously which " +
									   "may take a while. Are you sure you want to continue?";
					if (EditorUtility.DisplayDialog("Warning", msg, "Continue")) {
						_config.GenerateEditor();
					}
				} else {
					_config.GenerateEditor();
				}
			}
			if (GUILayout.Button("Clear Tiles")) {
				if (_config.Generator != null && _config.Generator.Pool != null) {
					_config.Generator.Pool.RemoveAll();
				}
			}
		}

		/// <summary>
		/// Displays information useful to debugging Terra
		/// </summary>
		public void Debug() {
			// ReSharper disable once ConditionIsAlwaysTrueOrFalse
#pragma warning disable 162 //Unreachable Code
			if (TerraConfig.TerraDebug.WRITE_SPLAT_TEXTURES) {
				TerraConfig.TerraDebug.MAX_TEXTURE_WRITE_COUNT =
					EditorGUILayout.IntField("Tex Count", TerraConfig.TerraDebug.MAX_TEXTURE_WRITE_COUNT);
			}
#pragma warning restore 162
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