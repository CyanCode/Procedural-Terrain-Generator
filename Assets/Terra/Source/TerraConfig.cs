using System;
using System.Collections.Generic;
using Terra.Graph;
using Terra.Structures;
using Terra.Terrain;
using Terra.Util;
using UnityEditor;
using UnityEngine;
using Random = System.Random;

namespace Terra.Source {
	[Serializable, ExecuteInEditMode]
	public class TerraConfig: MonoBehaviour {
        public static bool IsInitialized;
        /// <summary>
        /// TerraConfig singleton instance
        /// </summary>
        private static TerraConfig _instance;

        private static bool _singletonSet;
        
		//Topology Generation
		public TerraGraph Graph;
		public GenerationData Generator;
        public ObjectPlacer Placer;
		public int Seed = 1337;
        public BackgroundWorker Worker;

		//Editor state information
		public EditorStateData EditorState;
        
        internal bool IsEditor;

        /// <returns>
        /// TerraConfig singleton or null if TerraConfig does not 
        /// exist in the scene.
        /// </returns>
        public static TerraConfig Instance {
			get {
				if (_singletonSet) {
					return _instance;
				}
				if (!IsInitialized) {
					return null;
				}

				_instance = FindObjectOfType<TerraConfig>();
				_singletonSet = true;
				return _instance;
			}
		}

		public static bool IsInEditMode => !Application.isPlaying && Application.isEditor;

		/// <summary>
        /// Logs the passed message if ShowDebugMessages is enabled
        /// </summary>
        /// <param name="message">message to log</param>
        public static void Log(string message) {
            if (Instance != null && Instance.EditorState.ShowDebugMessages) {
                Debug.Log(message);
            }
        }

		/// <summary>
		/// Initializes fields to default values if they were null 
		/// post serialization.
		/// </summary>
		void OnEnable() {
			_instance = this;
			_singletonSet = true;
			IsInitialized = true;
			 
			Generator ??= new GenerationData();
			EditorState ??= new EditorStateData();
            Placer ??= new ObjectPlacer();
            Worker ??= new BackgroundWorker();

            IsEditor = IsInEditMode;
		}

		void Start() {
            //Register play mode and assembly state handlers once
            EditorApplication.playModeStateChanged -= OnPlayModeStateChange;
            EditorApplication.playModeStateChanged += OnPlayModeStateChange;
            AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;
		    AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;

            IsEditor = IsInEditMode;

            CreateMTD();

			if (Generator.GenerateOnStart) {
				Generate();
			}
		}

		void Update() {
			if (!IsInitialized) return;

			if (Application.isPlaying && Generator.Pool != null && Generator.GenerateOnStart) {
                Generator.Pool.Update();
			}
		}

		void Reset() {
			OnEnable(); //Initialize default values
		}    

        void OnPlayModeStateChange(PlayModeStateChange state) {
            Log("Killing worker threads before exiting play mode");

            // Destroy lingering worker thread
            if (state == PlayModeStateChange.ExitingPlayMode && Worker != null) {
                Worker.ForceStop();
                Worker = null;
            }
        }

        void OnBeforeAssemblyReload() {
            if (!IsEditor && Worker != null) {
                Debug.LogWarning("Assembly reload detected in play mode, stopping worker threads.");
                Worker.ForceStop();
            }
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
				UnityEngine.Random.InitState(Seed);
			else
				Seed = new Random().Next(0, Int32.MaxValue);
			
			//Allows for update to continue
			Generator.GenerateOnStart = true;
		}

		/// <summary>
		/// Starts the generation process tailored specifically 
		/// to the editor.
		/// </summary>
		public void GenerateEditor() {
            CreateMTD();

			//Set default tracked object
			if (Generator.TrackedObject == null) {
				Generator.TrackedObject = Camera.main.gameObject;
			}

			//Set seed for RNG
			if (!Generator.UseRandomSeed)
				UnityEngine.Random.InitState(Seed);
			else
				Seed = new Random().Next(0, Int32.MaxValue);

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
				var pos = Generator.TrackedObject.transform.position;

				Gizmos.color = Color.blue;
				//DrawCylinder(pos);
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
	}

    public static class TerraExtensions {
        public static float NextFloat(this Random random) {
            return (float)random.NextDouble();
        }

        public static float NextFloat(this Random random, float min, float max) {
            return random.NextFloat() * (max - min) + min;
        }
    }
}
