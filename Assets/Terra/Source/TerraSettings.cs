using System;
using System.Collections.Generic;
using UnityEngine;
using Terra.Terrain;
using Random = UnityEngine.Random;

namespace Terra.Data {
	[Serializable, ExecuteInEditMode]
	public class TerraSettings: MonoBehaviour { //TODO rename to TerraConfig
		public static bool IsInitialized;

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
		public ShaderData ShaderData;
		public List<BiomeData> BiomesData;
		public List<DetailData> Details;
		public List<ObjectPlacementData> ObjectData; //TODO Remove

		public TessellationData Tessellation;
		public GrassData Grass;

		//Editor state information
		public EditorStateData EditorState;
		 
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
			_instance = this;
			IsInitialized = true;
			 
			if (Generator == null) Generator = new GenerationData();
			if (ShaderData == null) ShaderData = new ShaderData(); 
			if (BiomesData == null) BiomesData = new List<BiomeData>();
			if (Details == null) Details = new List<DetailData>();
			if (HeightMapData == null) HeightMapData = new TileMapData { Name = "Height Map" };
			if (TemperatureMapData == null) TemperatureMapData = new TileMapData { Name = "Temperature Map", RampColor1 = Color.red, RampColor2 = Color.blue };
			if (MoistureMapData == null) MoistureMapData = new TileMapData { Name = "Moisture Map", RampColor1 = Color.cyan, RampColor2 = Color.white };
			if (Tessellation == null) Tessellation = new TessellationData();
			if (Grass == null) Grass = new GrassData();
			if (EditorState == null) EditorState = new EditorStateData();
		}

		void Start() {
			CreateMTD();

			if (Generator.GenerateOnStart) {
				Generate();
			}
		}

		void Update() {
			if (!IsInitialized) return;

			if (Application.isPlaying && Generator.Pool != null && Generator.GenerateOnStart) {
				Generator.Pool.ResetQueue();
				Generator.Pool.Update();
			}
		}

		void Reset() {
			OnEnable(); //Initialize default values
		}

		/// <summary>
		/// Starts the generation process (for use in play mode)
		/// </summary>
		public void Generate() {
			CreateMTD();

			//Set default tracked object
			if (Generator.TrackedObject == null) {
				Generator.TrackedObject = Camera.main.gameObject;
			}

			//Set seed for RNG
			if (!Generator.UseRandomSeed)
				Random.InitState(GenerationSeed);
			else
				GenerationSeed = new System.Random().Next(0, Int32.MaxValue);
			
			//Allows for update to continue
			Generator.GenerateOnStart = true;
		}

		/// <summary>
		/// Starts the generation process tailored specifically 
		/// to the editor.
		/// </summary>
		public void GenerateEditor() {
			//Set default tracked object
			if (Generator.TrackedObject == null) {
				Generator.TrackedObject = Camera.main.gameObject;
			}

			//Set seed for RNG
			if (!Generator.UseRandomSeed)
				Random.InitState(GenerationSeed);
			else
				GenerationSeed = new System.Random().Next(0, Int32.MaxValue);

			Generator.Pool.ResetQueue(); 
			Generator.Pool.Update();
		}

		/// <summary>
		/// Gets the biome at the passed x and z world coordinates.
		/// </summary>
		/// <param name="x">world space x coordinate</param>
		/// <param name="z">world space z coordinate</param>
		/// <returns>Found <see cref="BiomeData"/> instance, null if nothing was found.</returns>
		public BiomeData GetBiomeAt(float x, float z) { //TODO moev to biomedata?
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

		void OnDrawGizmosSelected() {
			if (!IsInitialized)
				return;

			//On general tab selected: display mesh radius squares and collider radius
			List<GridPosition> positions = TilePool.GetTilePositionsFromRadius(Generator.GenerationRadius, new GridPosition(transform), Generator.Length);

			//Mesh radius squares
			foreach (GridPosition pos in positions) {
				Vector3 pos3D = new Vector3(pos.X * Generator.Length, 0, pos.Z * Generator.Length);
 
				//Draw LOD squares
				Gizmos.color = GetLodPreviewColor(pos);
				//bool isPreviewTile = Previewer.GetPreviewingPositions().Contains(pos);
				if (Gizmos.color != Color.white)
					Gizmos.DrawCube(pos3D, new Vector3(Generator.Length, 0, Generator.Length));

				//Draw overlayed grid
				Gizmos.color = Color.white;
				pos3D.y += 0.1f;
				Gizmos.DrawWireCube(pos3D, new Vector3(Generator.Length, 0, Generator.Length));
			}

			//Generation radius
			if (Generator.TrackedObject != null) {
				var pos = Generator.TrackedObject.transform.position;
				Vector3 extPos = new Vector3(pos.x, 0, pos.z);

				Gizmos.color = Color.blue;
				Gizmos.DrawWireCube(extPos, new Vector3(Generator.ColliderGenerationExtent, 0, Generator.ColliderGenerationExtent));
			}
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
		/// Gets the grid square preview color based on which LOD 
		/// level it falls within.
		/// </summary>
		/// <returns></returns>
		private Color GetLodPreviewColor(GridPosition position) {
			if (Generator == null || Generator.Lod == null)
				return Color.white;

			var lod = Generator.Lod;
			var lvlType = lod.GetLevelTypeForRadius((int)position.Distance(new GridPosition(0, 0)));
			
			switch (lvlType) {
				case LodData.LodLevelType.High:
					return Color.green;
				case LodData.LodLevelType.Medium:
					return Color.yellow;
				case LodData.LodLevelType.Low:
					return Color.red;
			}

			return Color.white;
		}
		
		/// <summary>
		/// Toggles that can aid in debugging Terra.
		/// </summary>
		public static class TerraDebug {
			/// <summary>
			/// Sets components to display/hide in TerraSettings 
			/// gameobject
			/// </summary>
			public const bool HIDE_IN_INSPECTOR = true;

			/// <summary>
			/// Writes splat control textures to the file system 
			/// for debug purposes
			/// </summary>
			public const bool WRITE_SPLAT_TEXTURES = false;

			/// <summary>
			/// How many textures should be written to the file 
			/// system when <see cref="WRITE_SPLAT_TEXTURES"/> is 
			/// enabled?
			/// </summary>
			public static int MAX_TEXTURE_WRITE_COUNT = 10;

			/// <summary>
			/// Displays the weighted biome map on created terrain
			/// </summary>
			public const bool SHOW_BIOME_DEBUG_TEXTURE = false;
		}
	}

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

		/// <summary>
		/// Checks whether:
		/// val >= max 
		/// OR 
		/// val <= min
		/// </summary>
		/// <param name="val">Value to check</param>
		public bool FitsMinMax(float val) {
			return val >= Max || val <= Min;
		}

		/// <summary>
		/// Calculates the "weight" of the passed value by finding
		/// the passed value's smaller distance between the min & max 
		/// and dividing the value by <see cref="blend"/>. The result is 
		/// then raised to the power of <see cref="falloff"/>.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="blend"></param>
		/// <param name="falloff"></param>
		/// <returns>A weight in the range of 0 and 1</returns>
		public float Weight(float value, float blend) {
			float range = Max - Min;
			float weight = (range - Mathf.Abs(value - Max)) * blend;

			return Mathf.Clamp01(weight);
		}
	}
}
