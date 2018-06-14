using System.Collections.Generic;
using Terra.Graph.Noise;
using Terra.Terrain.Util;
using UnityEngine;


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
}
