using System;
using System.Collections.Generic;
using Terra.Structure;
using Terra.Util;
using UnityEngine;

namespace Terra.Terrain.Detail {
	[Serializable]
	public class ObjectRenderer {
		private TerraConfig _config;
		private ObjectPool Pool;
		private bool ObserveTiles;

		public List<ObjectPlacementData> ObjectsToPlace {
			get; private set;
		}

		private const int GRID_SIZE = 20;

		/// <summary>
		/// Creates a new ObjectPlacer that uses mesh information provided 
		/// by TerraSettings to calculate where to place objects on meshes. 
		/// Optionally disable observing TerrainTiles if you wish to 
		/// manage the placement of tiles manually rather than displaying 
		/// and hiding when a Tile activates or deactivates.
		/// </summary>
		/// <param name="observeTiles">Observe Tile events?</param>
		public ObjectRenderer(bool observeTiles = true) {
			_config = TerraConfig.Instance;
			ObserveTiles = observeTiles;
			//ObjectsToPlace = _config.ObjectData;
			Pool = new ObjectPool(this);

			if (ObserveTiles) {
				TerraEvent.OnTileActivated += OnTerrainTileActivate;
				TerraEvent.OnTileDeactivated += OnTerrainTileDeactivate;
			}
		}

		/// <summary>
		/// Calculates a grid using the poisson disc sampling method. 
		/// The 2D grid positions fall within the range of [0, 1].
		/// 
		/// Can be called off of Unity's main thread.
		/// </summary>
		/// <param name="opt">How dense should the samples be</param>
		/// <returns>List of vectors within the grid</returns>
		public List<Vector2> GetPoissonGrid(float density) {
			PoissonDiscSampler pds = new PoissonDiscSampler(GRID_SIZE, GRID_SIZE, density);
			List<Vector2> total = new List<Vector2>();

			foreach (Vector2 sample in pds.Samples()) {
				//Normalize in range of [0, 1] before adding
				total.Add(sample / GRID_SIZE);
			}

			return total;
		}

		/// <summary>
		/// First creates a poisson grid based on the passed density. 
		/// Positions are then filtered based on the passed object placement 
		/// type taking into account height and angle constraints.
		/// 
		/// Unlike the <c>GetFilteredGrid(ObjectPlacementType, float)</c> method 
		/// this method samples from the passed Mesh rather than pulling 
		/// mesh information from TerraSettings.
		/// </summary>
		/// <param name="m">Mesh to sample height and angle values from</param>
		/// <param name="objectPlacementData">object placement type to sample</param>
		/// <returns>List of vectors within the grid and sample constraints</returns>
		public List<Vector3> GetFilteredGrid(Mesh m, ObjectPlacementData objectPlacementData) {
			MeshSampler sampler = /**new MeshSampler(m, Settings.Generator.MeshResolution);*/ new MeshSampler(m, 128); //TODO Resolution from TerraSettings
			List<Vector2> grid = GetPoissonGrid(objectPlacementData.Spread / 10);
			List<Vector3> toAdd = new List<Vector3>();

			foreach (Vector2 pos in grid) {
				MeshSampler.MeshSample sample = sampler.SampleAt(pos.x, pos.y);

				if (objectPlacementData.ShouldPlaceAt(sample.Height, sample.Angle)) {
					Vector3 newPos = new Vector3(pos.x, sample.Height, pos.y);
					toAdd.Add(newPos);
				}
			}

			return toAdd;
		}

		/// <summary>
		/// First creates a poisson grid based on the passed density. 
		/// Positions are then filtered based on the passed object placement 
		/// type taking into account height and angle constraints.
		/// </summary>
		/// <param name="m">Mesh to sample height and angle values from</param>
		/// <param name="objectPlacementData">object placement type to sample</param>
		/// <param name="density">How dense should the samples be</param>
		/// <returns>List of vectors within the grid and sample constraints</returns>
		public List<Vector3> GetFilteredGrid(Tile tile, ObjectPlacementData objectPlacementData, float density) {
			MeshFilter mf = tile.GetComponent<MeshFilter>();
			if (mf == null) {
				throw new ArgumentException("The passed Tile does not have an attached MeshFilter. Has a mesh been created?");
			}

			return GetFilteredGrid(mf.sharedMesh, objectPlacementData);
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

	public class ObjectPool {
		/// <summary>
		/// Wrapper class for handling the passing of 
		/// gameobjects and their associated placement type
		/// </summary>
		protected class ObjectContainer {
			public ObjectPlacementData ObjectPlacementData {
				get; private set;
			}

			private Dictionary<int, GameObject> Active = new Dictionary<int, GameObject>();
			private LinkedList<GameObject> Inactive = new LinkedList<GameObject>();

			public ObjectContainer(ObjectPlacementData objectPlacementData) {
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
				hashCode = hashCode * -1521134295 + EqualityComparer<ObjectPlacementData>.Default.GetHashCode(ObjectPlacementData);
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
				public ObjectPlacementData ObjectPlacementData;

				public PositionsContainer(Vector3[] positions, ObjectPlacementData objectPlacementData) {
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

				for (int i = 0; i < Positions.Length; i++) {
					ObjectPlacementData objectPlacementData = Pool.Placer.ObjectsToPlace[i];
					Vector3[] locations = Pool.Placer.GetFilteredGrid(Tile, objectPlacementData, 1).ToArray();
					Positions[i] = new PositionsContainer(locations, objectPlacementData);
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
			ObjectContainer GetContainerForType(ObjectPlacementData objectPlacementData) {
				foreach (ObjectContainer c in Pool.Containers) {
					if (c.ObjectPlacementData.Equals(objectPlacementData)) {
						return c;
					}
				}

				return null;
			}
		}

		private ObjectRenderer Placer;

		private ObjectContainer[] Containers;
		private List<TileContainer> Tiles;

		public ObjectPool(ObjectRenderer placer) {
			Placer = placer;

			ObjectPlacementData[] objectPlacementData = placer.ObjectsToPlace.ToArray();
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
