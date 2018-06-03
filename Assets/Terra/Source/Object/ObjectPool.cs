using System.Collections.Generic;
using Terra.Terrain;
using UnityEngine;

public class ObjectPool {
	/// <summary>
	/// Wrapper class for handling the passing of 
	/// gameobjects and their associated placement type
	/// </summary>
	protected class ObjectContainer {
		public ObjectPlacementType Type {
			get; private set;
		}
		
		private Dictionary<int, GameObject> Active = new Dictionary<int, GameObject>();
		private LinkedList<GameObject> Inactive = new LinkedList<GameObject>();

		public ObjectContainer(ObjectPlacementType type) {
			Type = type;
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
				GameObject go = Object.Instantiate(Type.Prefab, parent);
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
			GameObject go = Object.Instantiate(Type.Prefab, parent);
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
				string n1 = this.Type.Prefab.name;
				string n2 = ((ObjectContainer)obj).Type.Prefab.name;

				return n1 == n2;
			}

			return false;
		}
	}

	/// <summary>
	/// Wrapper class for managing various TerrainTiles that 
	/// are added and removed
	/// </summary>
	protected class TileContainer {
		private class PositionsContainer {
			public Vector3[] Positions;
			public ObjectPlacementType Type;

			public PositionsContainer(Vector3[] positions, ObjectPlacementType type) {
				Positions = positions;
				Type = type;
			}
		}

		public TerrainTile Tile { get; private set; }

		private readonly ObjectPool Pool;
		private PositionsContainer[] Positions;
		private List<GameObject> PlacedObjects = new List<GameObject>();

		private const string OBJS_CONTAINER_NAME = "OBJECTS";

		/// <summary>
		/// Creates a new TileContainer
		/// </summary>
		/// <param name="tile">Tile to track</param>
		public TileContainer(TerrainTile tile, ObjectPool pool) {
			Tile = tile;
			Pool = pool;
		}

		/// <summary>
		/// Places objects on the TerrainTile if positions have 
		/// already been computed.
		/// </summary>
		public void PlaceObjects() {
			//Get TerraSettings instance
			TerraSettings settings = Object.FindObjectOfType<TerraSettings>();

			if (Positions != null && settings != null) {
				foreach (PositionsContainer p in Positions) {
					ObjectContainer container = GetContainerForType(p.Type);
					Transform parent = GetParent();

					if (container != null) {
						foreach (Vector3 pos in p.Positions) {
							GameObject go = container.GetObject(parent);
							p.Type.TransformGameObject(go, pos, settings.Length, Tile.transform.position);

							PlacedObjects.Add(go);
						}
					}
				}
			}
		}

		/// <summary>
		/// Removes any objects that have been placed on this 
		/// TerrainTile.
		/// </summary>
		public void RemoveObjects() {
			foreach (PositionsContainer p in Positions) {
				ObjectContainer container = GetContainerForType(p.Type);

				foreach (GameObject go in PlacedObjects) {
					container.RemoveObject(go);
				}

				//Reset PlacedObjects after removing all
				PlacedObjects = new List<GameObject>();
			}
		}

		/// <summary>
		/// Computes positions for this TerrainTile and caches 
		/// them.
		/// </summary>
		public void ComputePositions() {
			Positions = new PositionsContainer[Pool.Placer.ObjectsToPlace.Count];

			for (int i = 0; i < Positions.Length; i++) {
				ObjectPlacementType type = Pool.Placer.ObjectsToPlace[i];
				Vector3[] locations = Pool.Placer.GetFilteredGrid(Tile, type, 1).ToArray();
				Positions[i] = new PositionsContainer(locations, type);
			}
		}

		/// <summary>
		/// Checks whether positions for this TerrainTile have been computed 
		/// or not.
		/// </summary>
		/// <returns></returns>
		public bool HasComputedPositions() {
			return Positions != null;
		}

		/// <summary>
		/// Creates a parent GameObject under this TerrainTile that contains 
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
		/// <param name="type">type to search for</param>
		/// <returns>ObjectContainer, null if no matches were found</returns>
		ObjectContainer GetContainerForType(ObjectPlacementType type) {
			foreach (ObjectContainer c in Pool.Containers) {
				if (c.Type.Equals(type)) {
					return c;
				}
			}

			return null;
		}
	}

	private ObjectPlacer Placer;

	private ObjectContainer[] Containers;
	private List<TileContainer> Tiles;

	public ObjectPool(ObjectPlacer placer) {
		Placer = placer;

		ObjectPlacementType[] types = placer.ObjectsToPlace.ToArray();
		Containers = new ObjectContainer[types.Length];
		Tiles = new List<TileContainer>();

		for (int i = 0; i < types.Length; i++) {
			Containers[i] = new ObjectContainer(types[i]);
		}
	}

	/// <summary>
	/// Activates all necessary objects and places them 
	/// on the passed TerrainTile.
	/// </summary>
	/// <param name="tile"></param>
	public void ActivateTile(TerrainTile tile) {
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

	public void DeactivateTile(TerrainTile tile) {
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