using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Terra.CoherentNoise;
using Terra.CoherentNoise.Generation;
using Terra.CoherentNoise.Generation.Fractal;
using Terra.Graph.Noise;
using Terra.Terrain;
using Random = UnityEngine.Random;

namespace Terra.Data {
	[Serializable, ExecuteInEditMode]
	public class TerraSettings: MonoBehaviour {
		public static bool IsInitialized = false;

		/// <summary>
		/// Internal TerraSettings instance to avoid finding when its not needed
		/// </summary>
		private static TerraSettings _instance;

		public static int GenerationSeed = 1337;

		//Topology Generation
		public GenerationData Generator;
		public TileMapData HeightMapData = new TileMapData { Name = "Height Map" };
		public TileMapData TemperatureMapData = new TileMapData { Name = "Temperature Map", RampColor1 = Color.red, RampColor2 = Color.blue };
		public TileMapData MoistureMapData = new TileMapData { Name = "Moisture Map", RampColor1 = Color.cyan, RampColor2 = Color.white };

		//Detail
		public List<BiomeData> BiomesData; 
		public List<SplatData> Splat; //TODO factor into details
		public List<DetailData> Details;
		public List<ObjectPlacementData> ObjectData;
		public Material CustomMaterial = null;

		public TessellationData Tessellation;
		public GrassData Grass;

		//Editor state information
		public EditorStateData EditorState;
		public TerrainPreview Preview;
		 
		/// <summary>
		/// Finds the active TerraSettings instance in this scene if one exists.
		/// </summary>
		public static TerraSettings Instance {
			get {
				if (_instance != null) {
					return _instance;
				}
				if (!IsInitialized) {
					return null;
				}

				_instance = FindObjectOfType<TerraSettings>();
				return _instance;
			}
		}

		/// <summary>
		/// Initializes fields to default values if they were null 
		/// post serialization.
		/// </summary>
		void OnEnable() {
			IsInitialized = true;
			 
			if (Generator == null) Generator = new GenerationData();
			if (BiomesData == null) BiomesData = new List<BiomeData>();
			if (Splat == null) Splat = new List<SplatData>();
			if (Details == null) Details = new List<DetailData>();
			if (HeightMapData == null) HeightMapData = new TileMapData { Name = "Height Map" };
			if (TemperatureMapData == null) TemperatureMapData = new TileMapData { Name = "Temperature Map", RampColor1 = Color.red, RampColor2 = Color.blue };
			if (MoistureMapData == null) MoistureMapData = new TileMapData { Name = "Moisture Map", RampColor1 = Color.cyan, RampColor2 = Color.white };
			if (Tessellation == null) Tessellation = new TessellationData();
			if (Grass == null) Grass = new GrassData();
			if (EditorState == null) EditorState = new EditorStateData();
			if (Preview == null) Preview = new TerrainPreview();
		}

		void Reset() {
			OnEnable(); //Initialize default values
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
			if (Generator.GenerateOnStart) {
				Generate();
			}
		}

		void Update() {
			if (!IsInitialized) return;

#if UNITY_EDITOR
			if (Application.isEditor && !Application.isPlaying && Preview == null) {
				Preview = new TerrainPreview();
			}
#endif
			if (Application.isPlaying && Generator.Pool != null && Generator.GenerateOnStart) {
				Generator.Pool.Update();
			}
		}

		void OnDrawGizmosSelected() {
			if (!IsInitialized)
				return;

			//On general tab selected: display mesh radius squares and collider radius
			List<Position> positions = TilePool.GetTilePositionsFromRadius(Generator.GenerationRadius, transform.position, Generator.Length);

			//Mesh radius squares
			foreach (Position pos in positions) {
				Vector3 pos3d = new Vector3(pos.X * Generator.Length, 0, pos.Z * Generator.Length);

				Gizmos.color = Color.white;
				Gizmos.DrawWireCube(pos3d, new Vector3(Generator.Length, 0, Generator.Length));
			}

			//Generation radius
			if (Generator.TrackedObject != null) {
				var pos = Generator.TrackedObject.transform.position;
				Vector3 extPos = new Vector3(pos.x, 0, pos.z);

				Gizmos.color = Color.blue;
				Gizmos.DrawWireCube(extPos, new Vector3(Generator.ColliderGenerationExtent, 0, Generator.ColliderGenerationExtent));
			}
		}

		/// <summary>
		/// Gets the biome at the passed x and z world coordinates.
		/// </summary>
		/// <param name="x">world space x coordinate</param>
		/// <param name="z">world space z coordinate</param>
		/// <returns>Found <see cref="BiomeData"/> instance, null if nothing was found.</returns>
		public BiomeData GetBiomeAt(float x, float z) {
			BiomeData chosen = null;
			var settings = Instance;

			foreach (BiomeData b in BiomesData) {
				var hm = settings.HeightMapData;
				var tm = settings.TemperatureMapData;
				var mm = settings.MoistureMapData;

				if (b.IsHeightConstrained && !hm.HasGenerator()) continue;
				if (b.IsTemperatureConstrained && !tm.HasGenerator()) continue;
				if (b.IsMoistureConstrained && !mm.HasGenerator()) continue;

				bool passHeight = b.IsHeightConstrained && b.HeightConstraint.Fits(hm.GetValue(x, z)) || !b.IsHeightConstrained;
				bool passTemp = b.IsTemperatureConstrained && b.TemperatureConstraint.Fits(tm.GetValue(x, z)) || !b.IsTemperatureConstrained;
				bool passMoisture = b.IsMoistureConstrained && b.MoistureConstraint.Fits(mm.GetValue(x, z)) || !b.IsMoistureConstrained;

				if (passHeight && passTemp && passMoisture) {
					chosen = b;
				}
			}

			return chosen;
		}

		private void CreateMTD() {
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
				if (Generator.TrackedObject == null) {
					Generator.TrackedObject = Camera.main.gameObject;
				}

				//Set seed for RNG
				if (!Generator.UseRandomSeed)
					Random.InitState(GenerationSeed);
				else
					GenerationSeed = new System.Random().Next(0, Int32.MaxValue);
			}

			//Allows for update to continue
			Generator.GenerateOnStart = true;
		}



		#region Terra Settings Related Structures

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

		/// <summary>
		/// Container for data relating to the state of the TerraSettingsEditor
		/// </summary>
		[Serializable]
		public class EditorStateData {
			public ToolbarOptions ToolbarSelection = ToolbarOptions.General;

			public bool DisplayPreview = false;
			public bool IsAdvancedFoldout = false;

			//Biomes
			public bool ShowBiomePreview = false;
			public float BiomePreviewZoom = 25f;
			public Texture2D BiomePreview = null;
			public bool ShowWhittakerInfo = false;

			public float InspectorWidth { get { return EditorGUIUtility.currentViewWidth; } }

			private float lastInspectorWidth = 0f;
 
			public bool DidResize(float currentWidth) {
				return Math.Abs(lastInspectorWidth - currentWidth) > 0.1f;
			}
		}

		public enum PlacementType {
			ElevationRange,
			Angle
		}

		[Serializable]
		public enum MapGeneratorType {
			Fractal,
			Perlin,
			Billow,
			Custom
		}

		[Serializable]
		public enum ToolbarOptions {
			General = 0,
			Maps = 1,
			Biomes = 2,
			Details = 3
		}

		#endregion
	}

	#region Data Classes 

	/// <summary>
	/// Represents a constraint between a minimum and 
	/// a maximum.
	/// </summary>
	[Serializable]
	public struct Constraint {
		public float Min;
		public float Max;

		public Constraint(float min, float max) {
			Min = min;
			Max = max;
		}

		/// <summary>
		/// Does the passed value fit within the min and max?
		/// </summary>
		/// <param name="val">Value to check</param>
		public bool Fits(float val) {
			return val > Min && val < Max;
		}
	}

	[Serializable]
	public class TessellationData {
		public float TessellationAmount = 4f;
		public float TessellationMinDistance = 5f;
		public float TessellationMaxDistance = 30f;
		public bool UseTessellation = true;
	}

	[Serializable]
	public class SplatData {
		public Texture2D Diffuse;
		public Texture2D Normal;
		public Vector2 Tiling = new Vector2(1, 1);
		public Vector2 Offset;

		public float Smoothness;
		public float Metallic;
		public float Blend = 30f;

		public TerraSettings.PlacementType PlacementType;

		public float AngleMin = 5f;
		public float AngleMax = 25f;

		public float MinHeight = 0.25f;
		public float MaxHeight = 0.75f;
		public bool IsMaxHeight;
		public bool IsMinHeight;
	}

	[Serializable]
	public class GrassData {
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
	}

	/// <summary>
	/// Represents a Biome and its various settings
	/// </summary>
	[Serializable]
	public class BiomeData {
		/// <summary>
		/// Detail information for this biome
		/// </summary>
		public DetailData Details;

		/// <summary>
		/// Name of this Biome
		/// </summary>
		public string Name = "";

		/// <summary>
		/// Height constraints if enabled
		/// </summary>
		public Constraint HeightConstraint = new Constraint(0, 1);

		/// <summary>
		/// Angle constraints if enabled
		/// </summary>
		public Constraint AngleConstraint = new Constraint(0, 90);

		/// <summary>
		/// Temperature constraint if enabled
		/// </summary>
		public Constraint TemperatureConstraint = new Constraint(0, 1f);

		/// <summary>
		/// Moisture map constraint if enabled
		/// </summary>
		public Constraint MoistureConstraint = new Constraint(0, 1f);

		/// <summary>
		/// Will this biome only appear between constrained 
		/// heights?
		/// </summary>
		public bool IsHeightConstrained = false;

		/// <summary>
		/// Will this biome only appear between constrained 
		/// angles?
		/// </summary>
		public bool IsAngleConstrained = false;

		/// <summary>
		/// Is this biome constrained by the temperature map?
		/// </summary>
		public bool IsTemperatureConstrained = false;

		/// <summary>
		/// Is this biome constrained by the moisture map?
		/// </summary>
		public bool IsMoistureConstrained = false;

		/// <summary>
		/// Display preview texture in editor?
		/// </summary>
		public bool ShowPreviewTexture = false;

		/// <summary>
		/// Preview texture that has possibly been previously calculated
		/// </summary>
		public Texture2D CachedPreviewTexture = null;

		/// <summary>
		/// "Color" assigned to this biome. Used for editor previewing
		/// </summary>
		public Color Color = default(Color);

		/// <summary>
		/// Create a preview texture for the passed list of biomes by 
		/// coloring biomes that pass constraints.
		/// </summary>
		public static Texture2D GetPreviewTexture(int width, int height, float zoom = 1f) {
			Texture2D tex = new Texture2D(width, height);

			for (int i = 0; i < width; i++) {
				for (int j = 0; j < height; j++) {
					BiomeData b = TerraSettings.Instance.GetBiomeAt(i / zoom, j / zoom);
					tex.SetPixel(i, j, b == null ? Color.black : b.Color);
				}
			}

			tex.Apply();
			return tex;
		}
	}

	[Serializable]
	public class DetailData {
		public bool ShowMaterialFoldout = false;
		public bool ShowObjectFoldout = false;
		public bool ShowGrassFoldout = false;

		public bool IsMaxHeightSelected = false;
		public bool IsMinHeightSelected = false;

		public List<SplatData> SplatsData;
		public List<ObjectPlacementData> ObjectData;

		public DetailData() {
			if (SplatsData == null)
				SplatsData = new List<SplatData>();
			if (ObjectData == null)
				ObjectData = new List<ObjectPlacementData>();
		}
	}

	/// <summary>
	/// Holds data relating to various types of maps (ie height, temperature, etc). 
	/// Used by <see cref="TerraSettings"/> for storing information.
	/// </summary>
	[Serializable]
	public class TileMapData {
		/// <summary>
		/// The CoherentNoise Generator attached to this instance. If 
		/// <see cref="MapType"/> is set to <see cref="TerraSettings.MapGeneratorType.Custom"/>, 
		/// <see cref="CustomGenerator"/> is returned.
		/// </summary>
		public Generator Generator {
			get {
				if (_generator == null) {
					UpdateGenerator();
				}

				return _generator;
			}
		}

		/// <summary>
		/// The type of Generator to use when constructing a map.
		/// </summary>
		public TerraSettings.MapGeneratorType MapType {
			get { return _mapType; }
			set {
				if (_mapType != value) {
					//Update generator on MapType change
					_mapType = value;
					UpdateGenerator();
				}
			}
		}

		/// <summary>
		/// If <see cref="MapType"/> is set to Custom, this Generator 
		/// is returned upon accessing <see cref="Generator"/>
		/// </summary>
		public Generator CustomGenerator {
			get { return _customGenerator; }
			set {
				_customGenerator = value;
				if (value != null) {
					MapType = TerraSettings.MapGeneratorType.Custom;
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
		/// Internal <see cref="Generator"/>
		/// </summary>
		private Generator _generator;

		/// <summary>
		/// Internal <see cref="CustomGenerator"/>
		/// </summary>
		private Generator _customGenerator;

		/// <summary>
		/// Internal <see cref="MapType"/>
		/// </summary>
		private TerraSettings.MapGeneratorType _mapType;

		public TileMapData() {
			MapType = TerraSettings.MapGeneratorType.Perlin;
			UpdateGenerator();
		}

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
				
			for (int x = 0; x < width; x++) {
				for (int y = 0; y < height; y++) {
					float v = GetValue(x, y, TextureZoom);
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
		/// Calls GetValue on <see cref="gen"/> at the passed 
		/// x / y coordinates. Normalizes value from [-1, 1] to [0, 1] 
		/// before returning.
		/// </summary>
		/// <param name="x">x coordinate</param>
		/// <param name="z">z coordinate</param>
		/// <param name="zoom">Optionally specify a zoom factor</param>
		/// <returns></returns>
		public float GetValue(float x, float z, float zoom = 1f) {
			return (Generator.GetValue(x / zoom, 0, z / zoom) + 1) / 2;
		}

		/// <summary>
		/// Updates the <see cref="Generator"/> assigned to this instance based on the 
		/// set <see cref="TerraSettings.MapGeneratorType"/> and returns it.
		/// </summary>
		/// <returns>Generator if set, null if <see cref="TerraSettings.MapGeneratorType.Custom"/> 
		/// is set and no <see cref="CustomGenerator"/> is specified</returns>
		public Generator UpdateGenerator() {
			int seed = TerraSettings.GenerationSeed;
			Generator gen;

			switch (MapType) {
				case TerraSettings.MapGeneratorType.Perlin:
					gen = new GradientNoise(seed);
					break;
				case TerraSettings.MapGeneratorType.Fractal:
					gen = new PinkNoise(seed);
					break;
				case TerraSettings.MapGeneratorType.Billow:
					gen = new BillowNoise(seed);
					break;
				case TerraSettings.MapGeneratorType.Custom:
					gen = CustomGenerator;
					break;
				default:
					return null;
			}

			_generator = gen;
			return gen;
		}

		/// <summary>
		/// Does this have a (non-null) generator?
		/// </summary>
		/// <returns>true if there is a generator</returns>
		public bool HasGenerator() {
			return Generator != null;
		}
	}

	[Serializable]
	public class GenerationData {
		public GameObject TrackedObject;
		public int GenerationRadius = 3;

		public bool GenerateOnStart = true;
		public bool UseRandomSeed = false;
		public bool UseMultithreading = true;
		public bool UseCustomMaterial = false;

		public float ColliderGenerationExtent = 50f;
		public bool GenAllColliders = false;

		public int MeshResolution = 128;
		public int Length = 500;
		public float Spread = 100f;
		public float Amplitude = 50f;

		public NoiseGraph Graph;
		public TilePool Pool;

		public GenerationData() { 
			if (Pool == null) Pool = new TilePool();
		}
	}

	#endregion
}
