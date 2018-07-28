using System;
using System.Collections.Generic;
using UnityEngine;
using Terra.CoherentNoise;
using Terra.CoherentNoise.Generation;
using Terra.CoherentNoise.Generation.Fractal;
using Terra.Graph.Noise;
using Terra.Terrain;
using Random = UnityEngine.Random;

namespace Terra.Data {
	[Serializable, ExecuteInEditMode]
	public partial class TerraSettings: MonoBehaviour {
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
		public List<SplatData> Splat; //TODO Remove
		public List<DetailData> Details;
		public List<ObjectPlacementData> ObjectData; //TODO Remove

		public TessellationData Tessellation;
		public GrassData Grass;

		//Editor state information
		public EditorStateData EditorState;
		public Previewer Previewer;
		 
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
			if (BiomesData == null) BiomesData = new List<BiomeData>();
			if (Splat == null) Splat = new List<SplatData>();
			if (Details == null) Details = new List<DetailData>();
			if (HeightMapData == null) HeightMapData = new TileMapData { Name = "Height Map" };
			if (TemperatureMapData == null) TemperatureMapData = new TileMapData { Name = "Temperature Map", RampColor1 = Color.red, RampColor2 = Color.blue };
			if (MoistureMapData == null) MoistureMapData = new TileMapData { Name = "Moisture Map", RampColor1 = Color.cyan, RampColor2 = Color.white };
			if (Tessellation == null) Tessellation = new TessellationData();
			if (Grass == null) Grass = new GrassData();
			if (EditorState == null) EditorState = new EditorStateData();
			if (Previewer == null) Previewer = new Previewer();
		}

		void Reset() {
			OnEnable(); //Initialize default values
		}

		void Start() {
			CreateMTD();

#if UNITY_EDITOR
			//if (!Application.isPlaying && Application.isEditor) {
			//	//Handle Previewing
			//	if (Preview != null && Preview.CanPreview()) {
			//		Preview.TriggerPreviewUpdate();
			//	}
			//}
#endif
			if (Generator.GenerateOnStart) {
				Generate();
			}
		}

		void Update() {
			if (!IsInitialized) return;

#if UNITY_EDITOR
			//if (Application.isEditor && !Application.isPlaying && Preview == null) {
			//	Preview = new TerrainPreview();
			//}
#endif
			if (Application.isPlaying && Generator.Pool != null && Generator.GenerateOnStart) {
				Generator.Pool.Update();
			}
		}

		void OnDrawGizmosSelected() {
			if (!IsInitialized)
				return;

			//On general tab selected: display mesh radius squares and collider radius
			List<GridPosition> positions = TilePool.GetTilePositionsFromRadius(Generator.GenerationRadius, new GridPosition(transform), Generator.Length);

			//Mesh radius squares
			foreach (GridPosition pos in positions) {
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
				//Preview.Cleanup(); TODO Check

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
		
		/// <summary>
		/// Toggles that can aid in debugging Terra.
		/// </summary>
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
	}
}
