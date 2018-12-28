using System;
using System.Collections.Generic;
using Terra.Graph;
using Terra.Structures;
using UnityEngine;
using Terra.Terrain;
using Terra.Util;
using UnityEditor;
using XNode;
using Random = UnityEngine.Random;

namespace Terra {
	[Serializable, ExecuteInEditMode]
	public class TerraConfig: MonoBehaviour {
		public static bool IsInitialized;
		public static int GenerationSeed = 1337;

		/// <summary>
		/// Internal TerraSettings instance to avoid finding when its not needed
		/// </summary>
		private static TerraConfig _instance;


		public TerraGraph Graph;

		//Topology Generation
		public GenerationData Generator;

		//Detail
		public ShaderData ShaderData;
		public List<BiomeData> BiomesData;
		public List<DetailData> Details;

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

		public static bool IsInEditMode {
			get {
				return !Application.isPlaying && Application.isEditor;
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
				if (!EditorState.ShowLodGrid && !EditorState.ShowLodCubes) {
					break;
				}

				Vector3 pos3D = new Vector3(pos.X * Generator.Length, 0, pos.Z * Generator.Length);
				Color prevColor = GetLodPreviewColor(pos);

				//Draw LOD squares and cubes
				if (EditorState.ShowLodGrid) {	
					Gizmos.color = prevColor;
					if (Gizmos.color != Color.white)
						Gizmos.DrawCube(pos3D, new Vector3(Generator.Length, 0, Generator.Length));

					//Draw overlayed grid
					Gizmos.color = Color.white;
					pos3D.y += 0.1f;
					Gizmos.DrawWireCube(pos3D, new Vector3(Generator.Length, 0, Generator.Length));
				}

				//Draw cube wireframes
				if (EditorState.ShowLodCubes) {
					float height = Generator.Amplitude;
					Gizmos.color = prevColor;
					pos3D.y += height / 2;
					Gizmos.DrawWireCube(pos3D, new Vector3(Generator.Length - 1f, height, Generator.Length - 1f));
				}
			}

			//LOD change radius
			if (Generator.TrackedObject != null && EditorState.ShowLodChangeRadius) {
				Gizmos.color = Color.blue;
				Handles.color = Gizmos.color;

				Vector3 pos = Generator.TrackedObject.transform.position;
				DrawCylinder(pos, Generator.LodChangeRadius);
			}

			//Generation radius
			if (Generator.TrackedObject != null) {
				Gizmos.color = Color.blue;
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

			Vector3 worldXYZ = Generator.TrackedObject == null ? Vector3.zero : Generator.TrackedObject.transform.position;
			LodData lod = Generator.Lod;
			LodData.Lod lvlType = lod.GetLevelForPosition(position, worldXYZ);

			return lvlType.PreviewColor;
		}

		private void DrawCylinder(Vector3 position, float radius) {
			Vector3 startCenter = position;
			Vector3 endCenter = position;

			startCenter.y = 0;
			endCenter.y = Generator.Amplitude;

			//Draw both circles
			Handles.DrawWireArc(startCenter, Vector3.up, Vector3.forward, 360f, radius);
			Handles.DrawWireArc(endCenter, Vector3.up, Vector3.forward, 360f, radius);

			//Draw squares in center
			Vector3 sqrCenter = position;
			sqrCenter.y = Generator.Amplitude / 2f;
			float vertLen = Generator.Amplitude;

			Gizmos.DrawWireCube(sqrCenter, new Vector3(radius * 2, vertLen, 0f));
			Gizmos.DrawWireCube(sqrCenter, new Vector3(0f, vertLen, radius * 2));
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
