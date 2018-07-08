using System;
using System.Collections.Generic;
using Terra.CoherentNoise;
using Terra.CoherentNoise.Generation;
using Terra.CoherentNoise.Generation.Fractal;
using Terra.Graph.Noise;
using Terra.Terrain.Util;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;


namespace Terra.Terrain {
	[System.Serializable, ExecuteInEditMode]
	public class TerraSettings: MonoBehaviour {
		/// <summary>
		/// Internal TerraSettings instance to avoid finding when its not needed
		/// </summary>
		private static TerraSettings _instance;

		[System.Serializable]
		public enum ToolbarOptions {
			General = 0,
			Maps = 1,
			Materials = 2,
			ObjectPlacement = 3
		}
		public ToolbarOptions ToolbarSelection = ToolbarOptions.General;

		#region Tab Settings

		//General Tab
		public GameObject TrackedObject;
		public bool GenerateOnStart = true;
		public int GenerationRadius = 3;
		public float ColliderGenerationExtent = 50f;
		public static int GenerationSeed = 1337;
		public int MeshResolution = 128;
		public int Length = 500;
		public bool UseRandomSeed = false;
		public bool GenAllColliders = false;
		public bool DisplayPreview = false;
		public bool UseMultithreading = true;

		//Noise Tab
		public NoiseGraph Graph;
		public float Spread = 100f;
		public float Amplitude = 50f;

		//Material Tab
		public List<TerrainPaint.SplatSetting> SplatSettings = new List<TerrainPaint.SplatSetting>();
		public float TessellationAmount = 4f;
		public float TessellationMinDistance = 5f;
		public float TessellationMaxDistance = 30f;
		public bool UseTessellation = true;
		public bool IsAdvancedFoldout = false;
		public bool IsMaxHeightSelected = false;
		public bool IsMinHeightSelected = false;
		public bool UseCustomMaterial = false;
		public Material CustomMaterial = null;
		public bool PlaceGrass = false;
		public float GrassStepLength = 1.5f;
		public float GrassVariation = 0.8f;
		public float GrassHeight = 1.5f;
		public float BillboardDistance = 75f;
		public float ClipCutoff = 0.25f;
		public bool GrassConstrainHeight = false;
		public float GrassMinHeight = 0f;
		public float GrassMaxHeight = 200f;
		public bool GrassConstrainAngle = false;
		public float GrassAngleMin = 0f;
		public float GrassAngleMax = 25f;
		public Texture2D GrassTexture = null;

		//Object Placement Tab
		public List<ObjectPlacementType> ObjectPlacementSettings = new List<ObjectPlacementType>();

		public TileMapData HeightMapData = new TileMapData { Name = "Height Map" };
		public TileMapData TemperatureMapData = new TileMapData { Name = "Temperature Map", RampColor1 = Color.red, RampColor2 = Color.blue };
		public TileMapData MoistureMapData = new TileMapData { Name = "Moisture Map", RampColor1 = Color.cyan, RampColor2 = Color.white };

		//Editor state information
		public EditorStateData EditorState = new EditorStateData();

		#endregion

		/// <summary>
		/// TilePool instance attached to this TerraSettings instance. This is instantiated
		/// in <code>Awake</code>.
		/// </summary> 
		public TilePool Pool;

		/// <summary>
		/// TerrainPreview instance attached to this TerraSettings instance. Instantiated in 
		/// <code>Awake</code> and handles previewing of the generated terrain.
		/// </summary>
		public TerrainPreview Preview;

		/// <summary>
		/// Finds the active TerraSettings instance in this scene if one exists.
		/// </summary>
		public static TerraSettings Instance {
			get {
				if (_instance != null) {
					return _instance;
				}

				_instance = FindObjectOfType<TerraSettings>();
				return _instance;
			}
		}

		void Awake() {
			Pool = new TilePool();
			Preview = new TerrainPreview();
		}

		void Start() {
			CreateMTD();

#if UNITY_EDITOR
			if (!Application.isPlaying && Application.isEditor) {
				//Handle Previewing
				if (Preview != null && Preview.CanPreview()) {
					Preview.TriggerPreviewUpdate();
				}
			}
#endif
			if (GenerateOnStart) {
				Generate();
			}
		}

		void Update() {
#if UNITY_EDITOR
			if (Application.isEditor && !Application.isPlaying && Preview == null) {
				Preview = new TerrainPreview();
			}
#endif
			if (Application.isPlaying && Pool != null && GenerateOnStart) {
				Pool.Update();
			}
		}

		void OnDrawGizmosSelected() {
			//On general tab selected: display mesh radius squares and collider radius
			List<Vector2> positions = TilePool.GetTilePositionsFromRadius(GenerationRadius, transform.position, Length);

			//Mesh radius squares
			foreach (Vector2 pos in positions) {
				Vector3 pos3d = new Vector3(pos.x * Length, 0, pos.y * Length);

				Gizmos.color = Color.white;
				Gizmos.DrawWireCube(pos3d, new Vector3(Length, 0, Length));
			}

			//Generation radius
			if (TrackedObject != null) {
				var pos = TrackedObject.transform.position;
				Vector3 extPos = new Vector3(pos.x, 0, pos.z);

				Gizmos.color = Color.blue;
				Gizmos.DrawWireCube(extPos, new Vector3(ColliderGenerationExtent, 0, ColliderGenerationExtent));
			}
		}

		void CreateMTD() {
			//Create MT Dispatch if not already there
			if (FindObjectOfType<MTDispatch>() == null) {
				GameObject mtd = new GameObject("Main Thread Dispatch");
				mtd.AddComponent<MTDispatch>();
				mtd.transform.parent = transform;
			}
		}

		/// <summary>
		/// Starts the generation process 
		/// </summary>
		public void Generate() {
			CreateMTD();

			if (Application.isPlaying) {
				//Cleanup preview from edit mode
				Preview.Cleanup();

				//Set default tracked object
				if (TrackedObject == null) {
					TrackedObject = Camera.main.gameObject;
				}

				//Set seed for RNG
				if (!UseRandomSeed)
					Random.InitState(GenerationSeed);
				else
					GenerationSeed = new System.Random().Next(0, System.Int32.MaxValue);
			}

			//Allows for update to continue
			GenerateOnStart = true;
		}

		/// <summary>
		/// Container for data relating to the state of the TerraSettingsEditor
		/// </summary>
		public class EditorStateData {
			public float lastInspectorWidth = 0f;
			public float inspectorWidth { get { return EditorGUIUtility.currentViewWidth; } }

			public bool DidResize(float currentWidth) {
				return Math.Abs(lastInspectorWidth - currentWidth) > 0.1f;
			}
		}
	}

	internal static class TerraDebug {
		/// <summary>
		/// Sets components to display/hide in TerraSettings 
		/// gameobject
		/// </summary>
		internal const bool HIDE_IN_INSPECTOR = true;

		/// <summary>
		/// Writes splat control textures to the file system 
		/// for debug purposes
		/// </summary>
		internal const bool WRITE_SPLAT_TEXTURES = false;
	}

	[Serializable]
	public enum MapGeneratorType {
		Fractal,
		Perlin,
		Billow,
		Custom
	}

	/// <summary>
	/// Holds data relating to various types of maps (ie height, temperature, etc). 
	/// Used by <see cref="TerraSettings"/> for storing information.
	/// </summary>
	[Serializable]
	public class TileMapData {
		/// <summary>
		/// The CoherentNoise Generator attached to this instance. If 
		/// <see cref="MapType"/> is set to <see cref="MapGeneratorType.Custom"/>, 
		/// <see cref="CustomGenerator"/> is returned.
		/// </summary>
		public Generator Generator {
			get {
				return GetGenerator();
			}
		}

		/// <summary>
		/// The type of Generator to use when constructing a map.
		/// </summary>
		public MapGeneratorType MapType = MapGeneratorType.Perlin;

		/// <summary>
		/// If <see cref="MapType"/> is set to Custom, this Generator 
		/// is returned upon accessing <see cref="Generator"/>
		/// </summary>
		public Generator CustomGenerator {
			get { return _customGenerator; }
			set {
				_customGenerator = value;
				if (value != null) {
					MapType = MapGeneratorType.Custom;
				}
			}
		}

		/// <summary>
		/// "Zoom" level applied to the preview texture
		/// </summary>
		public float TextureZoom = 25f;

		/// <summary>
		/// Lower color in the preview texture gradient
		/// </summary>
		public Color RampColor1 = Color.black;

		/// <summary>
		/// Higher color in the preview texture gradient
		/// </summary>
		public Color RampColor2 = Color.white;

		/// <summary>
		/// Last generated preview texture. Assuming <see cref="UpdatePreviewTexture(int,int,UnityEngine.Color,UnityEngine.Color)"/> 
		/// has already been called.
		/// </summary>
		public Texture2D PreviewTexture;

		/// <summary>
		/// Name of this map
		/// </summary>
		public string Name = "";

		/// <summary>
		/// Internal <see cref="CustomGenerator"/>
		/// </summary>
		private Generator _customGenerator;

		/// <summary>
		/// Updates the preview texture using the two passed colors 
		/// to form a gradient where -1 is color 1 and 1 is color 2. 
		/// Data is taken from <see cref="Generator"/>.
		/// </summary>
		/// <param name="width">Width of texture in pixels</param>
		/// <param name="height">Height of texture in pixels</param>
		/// <param name="c1">Color 1 in gradient</param>
		/// <param name="c2">Color 2 in gradient</param>
		public void UpdatePreviewTexture(int width, int height, Color c1, Color c2) {
			Texture2D tex = new Texture2D(width, height);
			Generator gen = Generator;

			for (int x = 0; x < width; x++) {
				for (int y = 0; y < height; y++) {
					//normalize [-1, 1] -> [0, 1]
					float v = (gen.GetValue(x / TextureZoom, 0, y / TextureZoom) + 1) / 2;
					Color c = Color.Lerp(c1, c2, v);

					tex.SetPixel(x, y, c);
				}
			}

			tex.Apply();
			PreviewTexture = tex;
		}

		/// <summary>
		/// Updates the preview texture with data from <see cref="Generator"/>. 
		/// The texture is colored as a gradient between the two colors 
		/// from <see cref="RampColor1"/> and <see cref="RampColor2"/>
		/// </summary>
		/// <param name="width">Width of texture in pixels</param>
		/// <param name="height">Height of texture in pixels</param>
		public void UpdatePreviewTexture(int width, int height) {
			UpdatePreviewTexture(width, height, RampColor1, RampColor2);
		}

		/// <summary>
		/// Gets the Generator assigned to this instance based on the 
		/// set <see cref="MapGeneratorType"/>.
		/// </summary>
		/// <returns>Generator if set, null if <see cref="MapGeneratorType.Custom"/> 
		/// is set and no <see cref="CustomGeneratorGraph"/> is specified</returns>
		private Generator GetGenerator() {
			int seed = TerraSettings.GenerationSeed;

			switch (MapType) {
				case MapGeneratorType.Perlin:
					return new GradientNoise(seed);
				case MapGeneratorType.Fractal:
					return new PinkNoise(seed);
				case MapGeneratorType.Billow:
					return new BillowNoise(seed);
				case MapGeneratorType.Custom:
					return CustomGenerator;
				default:
					return null;
			}
		}
	}
}
