using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Terra.Graph.Biome;
using Terra.Structures;

namespace Terra.Terrain {
    [Serializable]
    public class ObjectPlacer {
        private TerraConfig _config;

        private ObjectPool _pool {
            get { return _poolInternal ?? (_poolInternal = new ObjectPool()); }
        }
        private ObjectPool _poolInternal;

        public List<ObjectDetailNode> ObjectsToPlace {
            get {
                BiomeCombinerNode combiner = TerraConfig.Instance.Graph.GetBiomeCombiner();
                if (combiner == null) {
                    return null;
                }

                return combiner
                    .GetConnectedBiomeNodes()
                    .SelectMany(bn => bn.GetObjectInputs())
                    .ToList();
            }
        }

        /// <summary>
        /// Creates a new ObjectPlacer that uses mesh information provided 
        /// by TerraSettings to calculate where to place objects on meshes. 
        /// Optionally disable observing TerrainTiles if you wish to 
        /// manage the placement of tiles manually rather than displaying 
        /// and hiding when a Tile activates or deactivates.
        /// </summary>
        public ObjectPlacer() {
            RegisterTileEventListeners();
        }

        /// <summary>
        /// Registers listeners that respond to a Tile being activated 
        /// and deactivated. Adds/removes objects accordingly.
        /// </summary>
        public void RegisterTileEventListeners() {
            TerraEvent.OnTileActivated += OnTerrainTileActivate;
            TerraEvent.OnTileDeactivated += OnTerrainTileDeactivate;
        }

        /// <summary>
        /// Called when a Tile has been activated 
        /// </summary>
        /// <param name="tile">Activated tile</param>
        void OnTerrainTileActivate(Tile tile) {
            _pool.ActivateTile(tile);
        }

        /// <summary>
        /// Called when a Tile has been deactivated
        /// </summary>
        /// <param name="tile">Deactivated tile</param>
        void OnTerrainTileDeactivate(Tile tile) {
            _pool.DeactivateTile(tile);
        }
    }

    [Serializable]
    public class ObjectPool {
        private ObjectPlacer _placer {
            get {
                return TerraConfig.Instance.Placer;
            }
        }
        
        [SerializeField] private List<TileContainer> _tiles;

        /// <summary>
        /// Internal representation of <see cref="_containers"/> to 
        /// allow for lazy loading of data
        /// </summary>
        private ObjectContainer[] _containersInternal;
        private ObjectContainer[] _containers {
            get {
                if (_containersInternal == null) {
                    ObjectDetailNode[] objectPlacementData = _placer.ObjectsToPlace.ToArray();
                    _containersInternal = new ObjectContainer[objectPlacementData.Length];

                    for (int i = 0; i < objectPlacementData.Length; i++) {
                        _containersInternal[i] = new ObjectContainer(objectPlacementData[i]);
                    }
                }

                return _containersInternal;
            }
        }

        public ObjectPool() {
            if (_tiles == null) {
                _tiles = new List<TileContainer>();
            }
        }

        /// <summary>
        /// Activates all necessary objects and places them 
        /// on the passed Tile.
        /// </summary>
        /// <param name="tile"></param>
        public void ActivateTile(Tile tile) {
            TileContainer setContainer = null;

            foreach (TileContainer container in _tiles) {
                if (container.Tile == tile) {
                    setContainer = container;
                }
            }

            if (setContainer == null) {
                setContainer = new TileContainer(tile, this);
                _tiles.Add(setContainer);
            }

            //Compute positions if needed
            if (!setContainer.HasComputedPositions()) {
                setContainer.ComputePositions();
            }

            setContainer.PlaceObjects();
        }

        public void DeactivateTile(Tile tile) {
            TileContainer setContainer = null;

            foreach (TileContainer container in _tiles) {
                if (container.Tile == tile) {
                    setContainer = container;
                }
            }

            if (setContainer != null) {
                setContainer.RemoveObjects();
            }
        }

        /// <summary>
        /// Wrapper class for handling the passing of 
        /// gameobjects and their associated placement type
        /// </summary>
        private class ObjectContainer {
            public ObjectDetailNode ObjectPlacementData {
                get; private set;
            }

            private Dictionary<int, GameObject> Active = new Dictionary<int, GameObject>();
            private LinkedList<GameObject> Inactive = new LinkedList<GameObject>();

            public ObjectContainer(ObjectDetailNode objectPlacementData) {
                ObjectPlacementData = objectPlacementData;
            }

            /// <summary>
            /// "Warms up" this object container by instantiating a 
            /// number of Gameobjects.
            /// </summary>
            /// <param name="count">Amount of gameobjects to create</param>
            /// <param name="parent">Where to place objects under</param>
            /// <param name="active">Optionally set gameobjects to active at start</param>
            public void Warmup(int count, Transform parent, bool active = true) {
                for (int i = 0; i < count; i++) {
                    GameObject go = UnityEngine.Object.Instantiate(ObjectPlacementData.Prefab, parent);
                    go.SetActive(active);

                    if (active)
                        Active.Add(go.GetInstanceID(), go);
                    else
                        Inactive.AddLast(go);
                }
            }

            /// <summary>
            /// Finds an inactive object to make active if 
            /// one exists, otherwise a new object is instantiated, 
            /// made active, and returned
            /// </summary>
            /// <param name="parent">Where to insert this gameobject</param>
            /// <returns>GameObject of prefab type specified in ObjectPlacementType</returns>
            public GameObject GetObject(Transform parent) {
                if (Inactive.Count > 0) {
                    GameObject obj = Inactive.Last.Value;
                    Inactive.RemoveLast();

                    obj.SetActive(true);
                    obj.transform.parent = parent;
                    Active.Add(obj.GetInstanceID(), obj);

                    return obj;
                }

                //Inactive list is empty, create new gameobject
                GameObject go = UnityEngine.Object.Instantiate(ObjectPlacementData.Prefab, parent);
                Active.Add(go.GetInstanceID(), go);
                return go;
            }

            /// <summary>
            /// "Removes" an object by making it inactive. This opens 
            /// up the GameObject for reuse if it is needed elsewhere.
            /// </summary>
            /// <param name="go">GameObject to remove</param>
            public void RemoveObject(GameObject go) {
                go.SetActive(false);

                if (Active.ContainsKey(go.GetInstanceID())) {
                    Active.Remove(go.GetInstanceID());
                    Inactive.AddFirst(go);
                } else {
                    Debug.LogWarning("The passed gameobject does not exist in the list of active tiles");
                }
            }

            public override bool Equals(object obj) {
                if (obj is ObjectContainer) {
                    string n1 = this.ObjectPlacementData.Prefab.name;
                    string n2 = ((ObjectContainer)obj).ObjectPlacementData.Prefab.name;

                    return n1 == n2;
                }

                return false;
            }

            public override int GetHashCode() {
                var hashCode = -572560676;
                hashCode = hashCode * -1521134295 + EqualityComparer<ObjectDetailNode>.Default.GetHashCode(ObjectPlacementData);
                hashCode = hashCode * -1521134295 + EqualityComparer<Dictionary<int, GameObject>>.Default.GetHashCode(Active);
                hashCode = hashCode * -1521134295 + EqualityComparer<LinkedList<GameObject>>.Default.GetHashCode(Inactive);
                return hashCode;
            }
        }

        /// <summary>
        /// Wrapper class for managing various TerrainTiles that 
        /// are added and removed
        /// </summary>
        private class TileContainer {
            private class PositionsContainer {
                public Vector3[] Positions;
                public ObjectDetailNode ObjectPlacementData;

                public PositionsContainer(Vector3[] positions, ObjectDetailNode objectPlacementData) {
                    Positions = positions;
                    ObjectPlacementData = objectPlacementData;
                }
            }

            public Tile Tile { get; private set; }

            private readonly ObjectPool Pool;
            private PositionsContainer[] Positions;
            private List<GameObject> PlacedObjects = new List<GameObject>();

            private const string OBJS_CONTAINER_NAME = "OBJECTS";

            /// <summary>
            /// Creates a new TileContainer
            /// </summary>
            /// <param name="tile">Tile to track</param>
            public TileContainer(Tile tile, ObjectPool pool) {
                Tile = tile;
                Pool = pool;
            }

            /// <summary>
            /// Places objects on the Tile if positions have 
            /// already been computed.
            /// </summary>
            public void PlaceObjects() {
                //Get TerraSettings instance
                TerraConfig config = UnityEngine.Object.FindObjectOfType<TerraConfig>();
                int length = config.Generator.Length;

                if (Positions != null && config != null) {
                    foreach (PositionsContainer p in Positions) {
                        ObjectContainer container = GetContainerForType(p.ObjectPlacementData);
                        Transform parent = GetParent();

                        if (container != null) {
                            foreach (Vector3 pos in p.Positions) {
                                GameObject go = container.GetObject(parent);
                                p.ObjectPlacementData.TransformGameObject(go, pos);

                                //Translate object back into place (terrain origin is centered)
                                go.transform.position = 
                                    new Vector3(go.transform.position.x - length / 2f, go.transform.position.y, go.transform.position.z - length / 2f);

                                PlacedObjects.Add(go);
                            }
                        }
                    }
                }
            }

            /// <summary>
            /// Removes any objects that have been placed on this 
            /// Tile.
            /// </summary>
            public void RemoveObjects() {
                foreach (PositionsContainer p in Positions) {
                    ObjectContainer container = GetContainerForType(p.ObjectPlacementData);

                    foreach (GameObject go in PlacedObjects) {
                        container.RemoveObject(go);
                    }

                    //Reset PlacedObjects after removing all
                    PlacedObjects = new List<GameObject>();
                }
            }

            /// <summary>
            /// Computes positions for this Tile and caches 
            /// them.
            /// </summary>
            public void ComputePositions() {
                Positions = new PositionsContainer[Pool._placer.ObjectsToPlace.Count];
                UnityEngine.Terrain t = Tile.MeshManager.ActiveTerrain;
                float amp = TerraConfig.Instance.Generator.Amplitude;

                for (int i = 0; i < Positions.Length; i++) {
                    ObjectDetailNode objectPlacementData = Pool._placer.ObjectsToPlace[i];
                    Vector2[] samples = objectPlacementData.SamplePositions(Tile.Random);
                    List<Vector3> worldPositions = new List<Vector3>((int) (Positions.Length * 0.66f));

                    for (var j = 0; j < samples.Length; j++) {
                        Vector2 pos = samples[j];
                        float height = t.terrainData.GetInterpolatedHeight(pos.x, pos.y);
                        float angle = Vector3.Angle(Vector3.up, t.terrainData.GetInterpolatedNormal(pos.x, pos.y)) / 90;
                        Vector2 world = MathUtil.NormalToWorld(Tile.GridPosition, pos);

                        if (objectPlacementData.ShouldPlaceAt(world.x, world.y, height / amp, angle)) {
                            worldPositions.Add(new Vector3(world.x, height, world.y));
                        }
                    }

                    Positions[i] = new PositionsContainer(worldPositions.ToArray(), objectPlacementData);
                }
            }

            /// <summary>
            /// Checks whether positions for this Tile have been computed 
            /// or not.
            /// </summary>
            /// <returns></returns>
            public bool HasComputedPositions() {
                return Positions != null;
            }

            /// <summary>
            /// Creates a parent GameObject under this Tile that contains 
            /// its objects. If one already exists, it is returned.
            /// </summary>
            Transform GetParent() {
                //Loop through immediate children of tile
                foreach (Transform child in Tile.transform) {
                    if (child.name == OBJS_CONTAINER_NAME) {
                        return child.gameObject.transform;
                    }
                }

                //No child with name found, create
                GameObject go = new GameObject(OBJS_CONTAINER_NAME);
                go.transform.parent = Tile.transform;

                return go.transform;
            }

            /// <summary>
            /// Gets the container that holds the passed 
            /// ObjectPlacementType
            /// </summary>
            /// <param name="objectPlacementData">type to search for</param>
            /// <returns>ObjectContainer, null if no matches were found</returns>
            ObjectContainer GetContainerForType(ObjectDetailNode objectPlacementData) {
                foreach (ObjectContainer c in Pool._containers) {
                    if (c.ObjectPlacementData.Equals(objectPlacementData)) {
                        return c;
                    }
                }

                return null;
            }
        }
    }
}