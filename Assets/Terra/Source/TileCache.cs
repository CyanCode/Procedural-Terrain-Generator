using System;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace Terra.Terrain {
	/// <summary>
	/// A cache that handles individual Tile instances. This class 
	/// handles the activation, deactivation, removal, and caching of 
	/// TerrainTiles.
	/// </summary>
	[Serializable]
	public class TileCache {
		public List<Tile> ActiveTiles { get; private set; }

		private int _cacheCapacity;
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
		/// Checks if there is a tile active at the passed Vector2 position
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
					if (t.GridPosition == position) {
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
		public void RemoveTile(Tile tile) {
			if (ActiveTiles.Contains(tile)) {
				ActiveTiles.Remove(tile);

#if UNITY_EDITOR
				Object.DestroyImmediate(tile.gameObject);
#else
				Object.Destroy(tile.gameObject);
#endif
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
		/// Ensures that the cache size is maintained. Removes all extra 
		/// nodes from the back of the linked list.
		/// </summary>
		private void EnforceCacheSize() {
			int removalAmount = _cachedTiles.Count - _cacheCapacity;

			while (removalAmount > 0) {
#if UNITY_EDITOR
				Object.DestroyImmediate(_cachedTiles.Last.Value);
#else
				Object.Destroy(_cachedTiles.Last.Value);
#endif
				_cachedTiles.RemoveLast();
				removalAmount--;
			}
		}
	}
}