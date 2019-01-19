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
        private ObjectPool Pool;
        private bool ObserveTiles;

        public List<ObjectDetailNode> ObjectsToPlace {
            get {
                return _objectsToPlace;
            }
            private set {
                _objectsToPlace = value;
            }
        }

        [SerializeField]
        private List<ObjectDetailNode> _objectsToPlace;

        /// <summary>
        /// Creates a new ObjectPlacer that uses mesh information provided 
        /// by TerraSettings to calculate where to place objects on meshes. 
        /// Optionally disable observing TerrainTiles if you wish to 
        /// manage the placement of tiles manually rather than displaying 
        /// and hiding when a Tile activates or deactivates.
        /// </summary>
        /// <param name="observeTiles">Observe Tile events?</param>
        public ObjectPlacer(bool observeTiles = true) {
            _config = TerraConfig.Instance;

            if (_config.Graph.GetBiomeCombiner() == null) {
                return;
            }

            ObserveTiles = observeTiles;
            ObjectsToPlace = _config.Graph.GetBiomeCombiner()
                .GetConnectedBiomeNodes()
                .SelectMany(bn => bn.GetObjectInputs())
                .ToList();
            Pool = new ObjectPool(this);

            if (ObserveTiles) {
                TerraEvent.OnTileActivated += OnTerrainTileActivate;
                TerraEvent.OnTileDeactivated += OnTerrainTileDeactivate;
            }
        }

        /// <summary>
        /// Called when a Tile has been activated 
        /// </summary>
        /// <param name="tile">Activated tile</param>
        void OnTerrainTileActivate(Tile tile) {
            Pool.ActivateTile(tile);
        }

        /// <summary>
        /// Called when a Tile has been deactivated
        /// </summary>
        /// <param name="tile">Deactivated tile</param>
        void OnTerrainTileDeactivate(Tile tile) {
            Pool.DeactivateTile(tile);
        }
    }

    [Serializable]
    public class ObjectPool {
        /// <summary>
        /// Wrapper class for handling the passing of 
        /// gameobjects and their associated placement type
        /// </summary>
        protected class ObjectContainer {
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
        protected class TileContainer {
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

                if (Positions != null && config != null) {
                    foreach (PositionsContainer p in Positions) {
                        ObjectContainer container = GetContainerForType(p.ObjectPlacementData);
                        Transform parent = GetParent();

                        if (container != null) {
                            foreach (Vector3 pos in p.Positions) {
                                GameObject go = container.GetObject(parent);
                                p.ObjectPlacementData.TransformGameObject(go, pos, config.Generator.Length, Tile.transform.position);

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
                Positions = new PositionsContainer[Pool.Placer.ObjectsToPlace.Count];
                UnityEngine.Terrain t = Tile.MeshManager.ActiveTerrain;

                for (int i = 0; i < Positions.Length; i++) {
                    ObjectDetailNode objectPlacementData = Pool.Placer.ObjectsToPlace[i];
                    Vector2[] samples = objectPlacementData.SamplePositions();
                    Vector3[] worldPositions = new Vector3[samples.Length];

                    for (var j = 0; j < samples.Length; j++) {
                        Vector2 pos = samples[j];
                        float height = t.terrainData.GetInterpolatedHeight(pos.x, pos.y);
                        Vector2 world = MathUtil.NormalToWorld(Tile.GridPosition, pos);

                        worldPositions[j] = new Vector3(world.x, height, world.y);
                    }

                    Positions[i] = new PositionsContainer(worldPositions, objectPlacementData);
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
                foreach (ObjectContainer c in Pool.Containers) {
                    if (c.ObjectPlacementData.Equals(objectPlacementData)) {
                        return c;
                    }
                }

                return null;
            }
        }

        [SerializeField]
        private ObjectPlacer Placer;
        [SerializeField]
        private ObjectContainer[] Containers;
        [SerializeField]
        private List<TileContainer> Tiles;

        public ObjectPool(ObjectPlacer placer) {
            Placer = placer;

            ObjectDetailNode[] objectPlacementData = placer.ObjectsToPlace.ToArray();
            Containers = new ObjectContainer[objectPlacementData.Length];
            Tiles = new List<TileContainer>();

            for (int i = 0; i < objectPlacementData.Length; i++) {
                Containers[i] = new ObjectContainer(objectPlacementData[i]);
            }
        }

        /// <summary>
        /// Activates all necessary objects and places them 
        /// on the passed Tile.
        /// </summary>
        /// <param name="tile"></param>
        public void ActivateTile(Tile tile) {
            TileContainer setContainer = null;

            foreach (TileContainer container in Tiles) {
                if (container.Tile == tile) {
                    setContainer = container;
                }
            }

            if (setContainer == null) {
                setContainer = new TileContainer(tile, this);
                Tiles.Add(setContainer);
            }

            //Compute positions if needed
            if (!setContainer.HasComputedPositions()) {
                setContainer.ComputePositions();
            }

            setContainer.PlaceObjects();
        }

        public void DeactivateTile(Tile tile) {
            TileContainer setContainer = null;

            foreach (TileContainer container in Tiles) {
                if (container.Tile == tile) {
                    setContainer = container;
                }
            }

            if (setContainer != null) {
                setContainer.RemoveObjects();
            }
        }
    }
}