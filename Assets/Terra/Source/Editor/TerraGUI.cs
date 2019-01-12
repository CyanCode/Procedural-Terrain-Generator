using Terra;
using Terra.Graph;
using Terra.Structures;
using Terra.Terrain;
using UnityEngine;

namespace UnityEditor.Terra {
    /// <summary>
    /// Handles displaying various GUI elements for the Terra
    /// Settings custom inspector
    /// </summary>
    public class TerraGUI {
	    private TerraConfig _config;
		private Texture[] _toolbarImages;

	    private string[] strResOpts = { "32", "64", "128", "256", "512", "1024" };
	    private int[] heightmapResOpts = { 33, 65, 129, 257, 513, 1025 };
        private int[] mapResOpts = { 32, 64, 128, 256, 512, 1024 };

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

            //Node Graph
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Node Graph", EditorStyles.boldLabel);
            _config.Graph = (TerraGraph)EditorGUILayout.ObjectField(_config.Graph, typeof(TerraGraph), false);

			//Terrain settings
			ShowTerrain();

            //Detail
            ShowTreeDetails();

			//Advanced
            ShowAdvanced();

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

        private void ShowTerrain() {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Generation", EditorStyles.boldLabel);
            _config.Generator.GenerateOnStart = EditorGUILayout.Toggle("Generate On Start", _config.Generator.GenerateOnStart);
            _config.Generator.GenerationRadius = EditorGUILayout.IntField("Gen Radius", _config.Generator.GenerationRadius);
            _config.Generator.LodChangeRadius = EditorGUILayout.FloatField("LOD Change Radius", _config.Generator.LodChangeRadius);
            if (!_config.Generator.UseRandomSeed)
                TerraConfig.Instance.Seed = EditorGUILayout.IntField("Seed", TerraConfig.Instance.Seed);
            _config.Generator.UseRandomSeed = EditorGUILayout.Toggle("Random Seed", _config.Generator.UseRandomSeed);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Terrain", EditorStyles.boldLabel);

            ShowLod();

            _config.Generator.Length = EditorGUILayout.IntField("Length", _config.Generator.Length);
            _config.Generator.Amplitude = EditorGUILayout.FloatField("Amplitude", _config.Generator.Amplitude);
            _config.Generator.Spread = EditorGUILayout.FloatField("Spread", _config.Generator.Spread);
            _config.Generator.SplatmapResolution = EditorGUILayout.IntPopup("Splatmap Resolution",
                _config.Generator.SplatmapResolution, strResOpts, mapResOpts);
            _config.Generator.DetailmapResolution = EditorGUILayout.IntPopup("Detailmap Resolution",
                _config.Generator.DetailmapResolution, strResOpts, mapResOpts);
            _config.Generator.DetailResolutionPerPatch = EditorGUILayout.IntField("Res Per Patch", _config.Generator.DetailResolutionPerPatch);

        }

        private void ShowAdvanced() {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Advanced", EditorStyles.boldLabel);

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
        }

        private void ShowTreeDetails() {
            EditorGUILayout.Space(); 
            EditorGUILayout.LabelField("Trees & Detail Objects", EditorGUIExtension.TerraStyle.TextBold);
            
            _config.Generator.DetailDistance = 
                EditorGUIExtension.MinMaxIntField("Detail Distance", _config.Generator.DetailDistance, 0, 250);
            _config.Generator.DetailDensity = 
                EditorGUIExtension.MinMaxFloatField("Detail Density", _config.Generator.DetailDensity, 0, 1);
            _config.Generator.TreeDistance = 
                EditorGUIExtension.MinMaxIntField("Tree Distance", _config.Generator.TreeDistance, 0, 2000);
            _config.Generator.BillboardStart =
                EditorGUIExtension.MinMaxIntField("Billboard Start", _config.Generator.BillboardStart, 5, 2000);
            _config.Generator.FadeLength = 
                EditorGUIExtension.MinMaxIntField("Fade Length", _config.Generator.FadeLength, 0, 200);
            _config.Generator.MaxMeshTrees = 
                EditorGUIExtension.MinMaxIntField("Max Mesh Trees", _config.Generator.MaxMeshTrees, 0, 10000);
        }

        private void ShowLod() {
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
        }
	}
}