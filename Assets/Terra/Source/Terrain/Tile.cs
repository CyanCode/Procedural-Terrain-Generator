using System;
using System.Collections;
using System.Threading;
using UnityEngine;
using Terra.Structures;
using Terra.Util;

namespace Terra.Terrain {
    /// <summary>
    ///	Tile represents a Terrain gameobject in the scene. 
    ///	This class handles the instantiation of Terrain, noise, 
    ///	position, texture, and detail application.
    /// </summary>
    [ExecuteInEditMode]
    public class Tile : MonoBehaviour, ISerializationCallbackReceiver {
        private TerraConfig Config { get { return TerraConfig.Instance; } }

        [SerializeField]
        private TilePaint _painter;
        [SerializeField]
        private TileMesh _meshManager;
        [SerializeField]
        private TileDetail _detail;

        [HideInInspector]
        public bool IsColliderDirty = false;

        /// <summary>
        /// Position of this Tile in the grid of Tiles
        /// </summary>
        public GridPosition GridPosition { get; private set; }

        /// <summary>
        /// Create and manage mesh(es) attached to this Tile. This 
        /// provides an interface for creating and showing meshes of 
        /// varying resolutions.
        /// </summary>
        public TileMesh MeshManager {
            get {
                return _meshManager ?? (_meshManager = new TileMesh(this, GetLodLevel()));
            }
            set {
                _meshManager = value;
            }
        }

        /// <summary>
        /// Fully constructs this Tile. This includes creating a Mesh, painting 
        /// the terrain, and adding details (grass, objects, etc.)
        /// 
        /// By default, calculating heights is done off of the main thread but 
        /// can be disabled.
        /// </summary>
        /// <param name="onComplete">Called after all calculations have completed. 
        /// <see cref="onComplete"/>Can be null if the result is not needed.</param>
        /// <param name="remapMin">Optionally linear transform the heightmap from [min, max] to [0, 1]</param>
        /// <param name="remapMax">Optionally linear transform the heightmap from [min, max] to [0, 1]</param>
        public void Generate(Action onComplete, float remapMin = 0f, float remapMax = 1f) {
            //Ensure MTD is instantiated
            MTDispatch.Instance();
            GenerateCoroutine(onComplete, remapMin, remapMax);
        }

        /// <summary>
        /// Updates this TerrainTiles position by taking a Vector2 where 
        /// the x and y values are integers on a grid. Internally the x and y values 
        /// are multiplied by the Length of the mesh specified in TerraSettings
        /// </summary> 
        /// <param name="position">Position to set the Tile to (ie [1,0])</param>
        /// <param name="transformInScene">Move this Tile's gameobject to match position change?</param>
        public void UpdatePosition(GridPosition position, bool transformInScene = true) {
            GridPosition = position;

            //Update TileMesh LOD level
            MeshManager.Lod = GetLodLevel();

            if (transformInScene) {
                int len = Config.Generator.Length;
                int halfLen = len / 2;
                transform.position = new Vector3((position.X * len) - halfLen, 0f, (position.Z * len) - halfLen);
            }
        }

        /// <summary>
        /// Sets the heightmap to the current LOD
        /// </summary>
        public void UpdateHeightmap(Action onComplete, float remapMin, float remapMax) {
            MeshManager.Lod = GetLodLevel();
            MeshManager.CalculateHeightmapAsync(() => {
                MeshManager.SetTerrainHeightmap();
                
                if (onComplete != null) {
                    onComplete();
                }
            }, remapMin, remapMax);
        }

        /// <summary>
        /// Checks whether this Tile's heightmap matches its set level of detail.
        /// </summary>
        /// <returns>true if heightmap matches lod, false otherwise</returns>
        public bool IsHeightmapLodValid() {
            return MeshManager.LastGeneratedLodLevel.Resolution >= GetLodLevel().Resolution;
        }

        /// <summary>
        /// Gets the LOD level for this tile based off of its <see cref="GridPosition"/>'s 
        /// distance from the tracked object. If no tracked object is specified, the level 
        /// is determined by the <see cref="GridPosition"/>'s distance from [0, 0].
        /// </summary>
        /// <returns>LOD level</returns>
        public LodData.Lod GetLodLevel() {
            GameObject tracked = Config.Generator.TrackedObject;

            if (tracked == null) {
                return Config.Generator.Lod.GetLevelForPosition(GridPosition, Vector3.zero);
            }

            return Config.Generator.Lod.GetLevelForPosition(GridPosition, tracked.transform.position);
        }

        public override string ToString() {
            return "Tile[" + GridPosition.X + ", " + GridPosition.Z + "]";
        }

        /// <summary>
        /// Creates a gameobject with an attached Tile component and 
        /// places it in the scene. This is a convienence method and is not required 
        /// for correct tile creation.
        /// </summary>
        /// <param name="name">Name of the created gameobject</param>
        /// <returns>The attached Tile component</returns>
        public static Tile CreateTileGameobject(string name) {
            GameObject go = new GameObject(name);
            Tile tt = go.AddComponent<Tile>();

            return tt;
        }

        internal static IEnumerator SkipFrame() {
            yield return null;
        }

        private IEnumerator PostCreateHeightmap(Action onComplete) {
            MeshManager.SetTerrainHeightmap();
            MeshManager.SetVisible();

            //Create TilePaint object
            TilePaint painter = new TilePaint(this);

            yield return null;
            var t = new Util.Timer().Start();
            ThreadPool.QueueUserWorkItem(state => {
                t.StopAndPrintMT("Context switch");
                float[,,] map = painter.GetBiomeMap();

                MTDispatch.Instance().Enqueue(() => {
                    var skipCo = StartCoroutine(SkipFrame());

                    painter.Paint(map, () => {
                        StopCoroutine(skipCo);

                        StartCoroutine(PostSetHeightmap(painter, map, () => {
                            MeshManager.SetVisible(true);
                            onComplete();
                        }));
                    });
                });
            });
        }

        private void GenerateCoroutine(Action onComplete, float remapMin = 0f, float remapMax = 1f) {
            //Cache current LOD
            MeshManager.CalculateHeightmapAsync(() => {
                StartCoroutine(PostCreateHeightmap(onComplete));
            }, remapMin, remapMax);
        }

        private IEnumerator PostSetHeightmap(TilePaint painter, float[,,] biomeMap, Action onComplete) {
            //Create detailer
            TileDetail detailer = new TileDetail(this, painter, biomeMap);

            yield return null;
            detailer.AddTrees();
            
            yield return null;
            detailer.AddDetailLayers();
            onComplete();
        }


		#region Serialization

		[SerializeField]
		private GridPosition _serializedGridPosition;

		public void OnBeforeSerialize() {
			//Grid Position
			_serializedGridPosition = GridPosition;
		}

		public void OnAfterDeserialize() {
			//Grid Position
			GridPosition = _serializedGridPosition;
		}

		#endregion
	}
}