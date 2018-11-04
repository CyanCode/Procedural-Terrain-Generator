using System;
using System.Collections.Generic;
using Terra.Structure;
using UnityEngine;
using Terra.Terrain;
using Terra.Util;
using Random = UnityEngine.Random;

namespace Terra {
	[Serializable, ExecuteInEditMode]
	public class TerraConfig: MonoBehaviour {
		public static bool IsInitialized;

		/// <summary>
		/// Internal TerraSettings instance to avoid finding when its not needed
		/// </summary>
		private static TerraConfig _instance;

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
		/// Finds the active TerraConfig instance in this scene if one exists.
		/// </summary>
		public static TerraConfig Instance {
			get {
				if (_instance != null) {
					return _instance;
				}
				if (!IsInitialized) {
					return null;
				}

				_instance = FindObjectOfType<TerraConfig>();
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
				//Generator.Pool.ResetQueue();
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
		/// <returns>Found <see cref="BiomeData"/> instance, null if nothing was found.</returns> //todo remove
//		public BiomeData GetBiomeAt(float x, float z) { //TODO moev to biomedata?
//			BiomeData chosen = null;
//			var settings = Instance;
//
//			foreach (BiomeData b in BiomesData) {
//				var hm = settings.HeightMapData;
//				var tm = settings.TemperatureMapData;
//				var mm = settings.MoistureMapData;
//
//				if (b.IsHeightConstrained && !hm.HasGenerator()) continue;
//				if (b.IsTemperatureConstrained && !tm.HasGenerator()) continue;
//				if (b.IsMoistureConstrained && !mm.HasGenerator()) continue;
//
//				bool passHeight = b.IsHeightConstrained && b.HeightConstraint.Fits(hm.GetValue(x, z)) || !b.IsHeightConstrained;
//				bool passTemp = b.IsTemperatureConstrained && b.TemperatureConstraint.Fits(tm.GetValue(x, z)) || !b.IsTemperatureConstrained;
//				bool passMoisture = b.IsMoistureConstrained && b.MoistureConstraint.Fits(mm.GetValue(x, z)) || !b.IsMoistureConstrained;
//
//				if (passHeight && passTemp && passMoisture) {
//					chosen = b;
//				}
//			}
//
//			return chosen;
//		}

		void OnDrawGizmosSelected() {
			if (!IsInitialized)
				return;

			//Grid center
			Vector3 worldXYZ = Generator.TrackedObject != null ? Generator.TrackedObject.transform.position : Vector3.zero;
			Vector2 gridCenter = new Vector2(worldXYZ.x, worldXYZ.z);
			

			//On general tab selected: display mesh radius squares and collider radius
			List<GridPosition> positions = TilePool.GetTilePositionsFromRadius(Generator.GenerationRadius, gridCenter, Generator.Length);

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

			bool isCenter00 = !Application.isPlaying && Application.isEditor || Generator.TrackedObject == null;
			Vector3 worldXYZ = isCenter00 ? Vector3.zero : Generator.TrackedObject.transform.position;
			Vector3 worldXZ = new Vector2(worldXYZ.x, worldXYZ.z);

			var lod = Generator.Lod;
			var tileLength = Generator.Length;
			var lvlType = lod.GetLevelTypeForRadius((int)position.Distance(new GridPosition(worldXZ, tileLength)));
			
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
			/// system when <see cref="WRITE_SPLAT_TEXTURES"/> or 
			/// <see cref="WRITE_BIOME_DEBUG_TEXTURE"/> are true?
			/// </summary>
			public static int MAX_TEXTURE_WRITE_COUNT = 5;

			/// <summary>
			/// Writes the weighted biome map textures to the disk
			/// </summary>
			public const bool WRITE_BIOME_DEBUG_TEXTURE = false;

			/// <summary>
			/// Whether to show Debug.Log messages from Terra
			/// </summary>
			public const bool SHOW_DEBUG_MESSAGES = true;
		}
	}
}
