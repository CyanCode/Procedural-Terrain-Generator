using System;
using System.Collections.Generic;
using Terra.Structures;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Terra.Terrain {
	/// <summary>
	/// A cache that handles individual Tile instances. This class 
	/// handles the activation, deactivation, removal, and caching of 
	/// TerrainTiles.
	/// </summary>
	[Serializable]
	public class TileCache: ISerializationCallbackReceiver {
		public List<Tile> ActiveTiles { get; private set; }

		[SerializeField]
		private int _cacheCapacity;
		[SerializeField]
		private LinkedList<Tile> _cachedTiles = new LinkedList<Tile>();

		public TileCache(int cacheCapacity = 20) {
			_cacheCapacity = cacheCapacity;

			if (ActiveTiles == null)
				ActiveTiles = new List<Tile>();
		}

		/// <summary>
		/// Finds the tile at the passed position in the cache and returns it. 
		/// Once found, the tile is removed from the cache as cached tiles should 
		/// not be active in the scene.
		/// </summary>
		/// <param name="position">Position to search for</param>
		/// <returns>
		/// Returns the cached tile if it exists in the cache. If the tile
		/// is not cached, returns null.
		/// </returns>
		public Tile GetCachedTileAtPosition(GridPosition position) {
			LinkedListNode<Tile> node = _cachedTiles.First;

			while (node != null) {
				if (node.Value.GridPosition == position) { //Move Tile to front of cache
					_cachedTiles.Remove(node);

					return node.Value;
				}

				node = node.Next;
			}

			return null;
		}

		/// <summary>
		/// Checks if there is a tile active at the passed <see cref="GridPosition"/> position
		/// </summary>
		/// <param name="position">Position to look for</param>
		/// <returns>True if tile at position was found, false otherwise</returns>
		public bool IsTileActiveAtPosition(GridPosition position) {
			foreach (Tile t in ActiveTiles) {
				if (t.GridPosition == position)
					return true;
			}

			return false;
		}

		/// <summary>
		/// Finds all of the tile positions that aren't currently active in the scene 
		/// out of the passed positions collection.
		/// </summary>
		/// <param name="positions">Positions to compare</param>
		/// <returns>New positions to add</returns>
		public List<GridPosition> GetNewTilePositions(List<GridPosition> positions) {
			List<GridPosition> newPositions = new List<GridPosition>(ActiveTiles.Count);

			foreach (GridPosition position in positions) {
				bool matched = false;

				foreach (Tile t in ActiveTiles) {
					if (t != null && t.GridPosition == position) {
						matched = true;
						break;
					}
				}

				if (!matched)
					newPositions.Add(position);
			}

			return newPositions;
		}

		/// <summary>
		/// Adds the passed Tile to the tile cache. This 
		/// deactivates the passed tile and will no longer be rendered 
		/// until pulled from the cache.
		/// </summary>
		/// <param name="tile">Tile to cache</param>
		public void CacheTile(Tile tile) {
			tile.gameObject.SetActive(false);
			_cachedTiles.AddFirst(tile);
			EnforceCacheSize();

			TerraEvent.TriggerOnTileDeactivated(tile);
		}

		/// <summary>
		/// Removes the passed Tile from <see cref="ActiveTiles"/> 
		/// (if it exists), and then calls <see cref="UnityEngine.Object.Destroy(UnityEngine.Object,float)"/>
		/// </summary>
		/// <param name="tile">Tile to remove and destroy</param>
		public void RemoveActiveTile(Tile tile) {
			if (ActiveTiles.Contains(tile)) {
				Destroy(tile.gameObject);
				ActiveTiles.Remove(tile);
			}

			PurgeDestroyedTiles();
		}

		/// <summary>
		/// Removes the passed Tile from the cache if it exists and calls 
		/// <see cref="Object.Destroy(UnityEngine.Object,float)"/> on its gameobject.
		/// </summary>
		/// <param name="tile">Tile to remove and destroy</param>
		public void RemoveCachedTile(Tile tile) {
			if (_cachedTiles.Contains(tile)) {
				_cachedTiles.Remove(tile);
				Destroy(tile.gameObject);
			}
		}

		/// <summary>
		/// Adds the passed tile to the active tiles list. If 
		/// the passed tile is not active, it is made active.
		/// </summary>
		/// <param name="tile">Tile to activate</param>
		public void AddActiveTile(Tile tile) {
			tile.gameObject.SetActive(true);
			ActiveTiles.Add(tile);

			TerraEvent.TriggerOnTileActivated(tile);
		}

		/// <summary>
		/// Removes previously destroyed tiles from the cache.
		/// </summary>
		public void PurgeDestroyedTiles() {
			for (var i = 0; i < ActiveTiles.Count; i++) {
				if (ActiveTiles[i] == null) {
					ActiveTiles.RemoveAt(i);
					i--;
				}
			}
		}

		/// <summary>
		/// Ensures that the cache size is maintained. Removes all extra 
		/// nodes from the back of the linked list.
		/// </summary>
		private void EnforceCacheSize() {
			int removalAmount = _cachedTiles.Count - _cacheCapacity;

			while (removalAmount > 0) {
				Destroy(_cachedTiles.Last.Value.gameObject);
				_cachedTiles.RemoveLast();
				removalAmount--;
			}
		}

		/// <summary>
		/// Destroys the passed gameobject in the world. Will destroy differently 
		/// based on whether play mode is active.
		/// </summary>
		private void Destroy(GameObject go) {
			if (Application.isPlaying) { //Play mode
				Object.Destroy(go);
			} else if (Application.isEditor) { //Edit mode
				Object.DestroyImmediate(go);
			}
		}

		#region Serialization

		[SerializeField]
		private List<Tile> _serializedCachedTiles;
		[SerializeField]
		private List<Tile> _serializedActiveTiles;

		public void OnBeforeSerialize() {
			//Cached tiles
			if (_cachedTiles != null) {
				_serializedCachedTiles = new List<Tile>(_cachedTiles.Count);

				LinkedList<Tile>.Enumerator enumerator = _cachedTiles.GetEnumerator();
				while (enumerator.MoveNext()) {
					_serializedCachedTiles.Add(enumerator.Current);
				}
				enumerator.Dispose();
			}

			//Active tiles
			if (ActiveTiles != null) {
				_serializedActiveTiles = new List<Tile>(ActiveTiles.Count);
				_serializedActiveTiles.AddRange(ActiveTiles);
			}
		}

		public void OnAfterDeserialize() {
			//Cached tiles
			if (_serializedCachedTiles != null) {
				_cachedTiles = new LinkedList<Tile>();

				foreach (Tile t in _cachedTiles) {
					_cachedTiles.AddLast(t);
				}
			}

			//Active tiles
			if (_serializedActiveTiles != null) {
				ActiveTiles = new List<Tile>(_serializedActiveTiles.Count);
				ActiveTiles.AddRange(_serializedActiveTiles);
			}
		}

		#endregion
	}
}