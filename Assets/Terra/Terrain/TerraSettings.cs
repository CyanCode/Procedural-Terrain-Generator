using System.Collections.Generic;
using Terra.Graph.Noise;
using Terra.Terrain.Util;
using UnityEngine;


namespace Terra.Terrain {
	[System.Serializable, ExecuteInEditMode]
	public class TerraSettings: MonoBehaviour {
		[System.Serializable]
		public enum ToolbarOptions {
			General = 0,
			Noise = 1,
			Materials = 2,
			ObjectPlacement = 3
		}
		public ToolbarOptions ToolbarSelection = ToolbarOptions.General;

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
		public bool IsMaxHeightSelected = false;
		public bool IsMinHeightSelected = false;
		public bool UseCustomMaterial = false;
		public Material CustomMaterial = null;

		//Object Placement Tab
		public List<ObjectPlacementType> ObjectPlacementSettings = new List<ObjectPlacementType>();

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
		/// ObjectPlacer instance attached to this TerraSettings instance. This is instantiated
		/// in <code>Awake</code>.
		/// </summary>
		public ObjectPlacer Placer;

		void Awake() {
			Pool = new TilePool(this);
			Preview = new TerrainPreview(this);
			Placer = new ObjectPlacer(this);
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
				Preview = new TerrainPreview(this);
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

				//Setup object tracking and generator reading
				//Set default tracked object
				if (TrackedObject == null) {
					TrackedObject = new GameObject("Default Tracked Position");
					TrackedObject.transform.position = Vector3.zero;
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
	}
}
