using System;
using System.Collections;
using System.Diagnostics;
using UnityEngine;
using Terra.Structures;
using Terra.Util;
using Debug = UnityEngine.Debug;

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
            
            if (TerraConfig.IsInEditMode) {
                GenerateEditor(remapMin, remapMax);
                onComplete();
            } else {
                StartCoroutine(GenerateCoroutine(onComplete, remapMin, remapMax));
            }
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
        /// Sets the heightmap to the current LOD asynchronously
        /// </summary>
        public IEnumerator UpdateHeightmapAsync(Action onComplete, float remapMin, float remapMax) {
            MeshManager.Lod = GetLodLevel();

            bool updatedHm = false;
            Debug.Log("Updating heightmap start");
            MeshManager.CalculateHeightmapAsync(remapMin, remapMax, () => {
                MeshManager.SetTerrainHeightmap();
                updatedHm = true;
            });

            while (!updatedHm)
                yield return null;

            if (onComplete != null)
                onComplete();
        }

        /// <summary>
        /// Sets the heightmap to the current LOD synchronously
        /// </summary>
        public void UpdateHeightmap(float remapMin, float remapMax) {
            MeshManager.Lod = GetLodLevel();
            MeshManager.CalculateHeightmap(null, remapMin, remapMax);
            MeshManager.SetTerrainHeightmap();   
        }

        /// <summary>
        /// Checks whether this Tile's heightmap matches its set level of detail.
        /// </summary>
        /// <returns>true if heightmap matches lod, false otherwise</returns>
        public bool IsHeightmapLodValid() {
            if (MeshManager.LastGeneratedLodLevel == null) {
                return false;
            }

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

        private IEnumerator ApplyDetails(TilePaint painter, float[,,] biomeMap, Action onComplete = null) {
            //Create detailer
            TileDetail detailer = new TileDetail(this, painter, biomeMap);

            yield return null;
            detailer.AddTrees();
            
            yield return null;
            detailer.AddDetailLayers();

            if (onComplete != null) {
                onComplete();
            }
        }

        private IEnumerator GenerateCoroutine(Action onComplete, float remapMin = 0f, float remapMax = 1f) {
            TerraConfig conf = TerraConfig.Instance;
#if TERRA_DEBUG
            Debug.Log("Started tile " + GridPosition);
#endif

            //Make & set heightmap
            bool madeHm = false;
            MeshManager.CalculateHeightmapAsync(remapMin, remapMax, () => madeHm = true);
            while (!madeHm)
                yield return null;

            MeshManager.SetTerrainHeightmap();
            MeshManager.SetVisible();

            //Create TilePaint object
            TilePaint painter = new TilePaint(this);

            //Make biomemap
            bool madeBm = false;
            float[,,] map = null;
            conf.Worker.Enqueue(() => map = painter.GetBiomeMap(), () => madeBm = true);
            int x = 0;
            while (!madeBm) {
                yield return null; //Skip frame until biomemap made
                x++;
            }
               
            //Paint terrain
            bool madePaint = false;
            yield return StartCoroutine(painter.PaintAsync(map, () => madePaint = true));
            while (!madePaint)
                yield return null;

            //Apply details to terrain
            bool madeDetails = false;
            yield return StartCoroutine(ApplyDetails(painter, map, () => madeDetails = true));
            while (!madeDetails)
                yield return null;

            MeshManager.SetVisible(true);

#if TERRA_DEBUG
            Debug.Log("Completed tile " + GridPosition);
#endif
            onComplete();
        }

        /// <summary>
        /// Works the same as <see cref="GenerateCoroutine"/> without the 
        /// yield instructions. Runs synchronously.
        /// </summary>
        /// <param name="remapMin"></param>
        /// <param name="remapMax"></param>
        private void GenerateEditor(float remapMin = 0f, float remapMax = 1f) {
#if TERRA_DEBUG
            Debug.Log("Started tile " + GridPosition);
#endif

            //Make & set heightmap
            MeshManager.CalculateHeightmap(GridPosition, remapMin, remapMax);

            MeshManager.SetTerrainHeightmap();
            MeshManager.SetVisible();

            //Create TilePaint object
            TilePaint painter = new TilePaint(this);

            //Make biomemap
            float[,,] map = painter.GetBiomeMap();

            //Paint terrain
            painter.Paint(map);

            //Apply details to terrain
            // ReSharper disable once IteratorMethodResultIsIgnored
            ApplyDetails(painter, map);
            MeshManager.SetVisible(true);

#if TERRA_DEBUG
            Debug.Log("Completed tile " + GridPosition);
#endif
        }

        public override string ToString() {
            return "Tile[" + GridPosition.X + ", " + GridPosition.Z + "]";
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