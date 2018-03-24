using Terra.CoherentNoise;
using System.Collections.Generic;
using Terra.GraphEditor;
using UnityEngine;
using UnityEditor;
using Terra.Terrain.Util;

namespace Terra.Terrain {
	[System.Serializable]
	[ExecuteInEditMode]
	public class TerraSettings: MonoBehaviour {
		[System.Serializable]
		public enum ToolbarOptions {
			General = 0,
			Noise = 1,
			Materials = 2,
			ObjectPlacement = 3
		}
		public ToolbarOptions ToolbarSelection = ToolbarOptions.General;
		public GraphLauncher Launcher;

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
		public string SelectedFile = "";
		public Graph LoadedGraph = null;
		public Generator Generator;
		public float Spread = 100f;
		public float Amplitude = 50f;

		//Material Tab
		public List<TerrainPaint.SplatSetting> SplatSettings = null;
		public bool IsMaxHeightSelected = false;
		public bool IsMinHeightSelected = false;
		public bool UseCustomMaterial = false;
		public Material CustomMaterial = null;

		//Object Placement Tab
		public List<ObjectPlacementType> ObjectPlacementSettings = null;
		public bool ShowTranslateFoldout = false;
		public bool ShowRotateFoldout = false;
		public bool ShowScaleFoldout = false;

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
			//Create MT Dispatch if not already there
			if (FindObjectOfType<MTDispatch>() == null) {
				GameObject mtd = new GameObject("Main Thread Dispatch");
				mtd.AddComponent<MTDispatch>();
				mtd.transform.parent = transform;
			}
			
			//Cleanup preview from edit mode
			if (EditorApplication.isPlayingOrWillChangePlaymode) {
				if (GetComponent<MeshRenderer>() != null) Destroy(GetComponent<MeshRenderer>());
				if (GetComponent<MeshFilter>() != null) Destroy(GetComponent<MeshFilter>());
			}

			//Setup object tracking and generator reading
			if (EditorApplication.isPlaying && GenerateOnStart) {
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

				Launcher = new GraphLauncher();
				Launcher.LoadGraph(SelectedFile);
				Launcher.Enable();

				Generator = Launcher.GetGraphGenerator();
			}
			
			//Handle previewing
			if (!EditorApplication.isPlaying && DisplayPreview && Generator != null) {
				Preview.TriggerPreviewUpdate();
			}
		}

		void Update() {
			if (EditorApplication.isPlaying && Pool != null && GenerateOnStart) 
				Pool.Update();
			if (!EditorApplication.isPlaying && Preview == null)
				Preview = new TerrainPreview(this);
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
	}
}
