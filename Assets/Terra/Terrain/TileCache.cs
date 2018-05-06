using UnityEngine;
using System.Collections.Generic;

namespace Terra.Terrain {
	/// <summary>
	/// A cache that handles individual TerrainTile instances. This class 
	/// handles the activation, deactivation, removal, and caching of 
	/// TerrainTiles.
	/// </summary>
	public class TileCache {
		public List<TerrainTile> ActiveTiles { get; private set; }

		private int CacheCapacity;
		private LinkedList<TerrainTile> CachedTiles = new LinkedList<TerrainTile>();

		public TileCache(int cacheCapacity = 20) {
			CacheCapacity = cacheCapacity;
			ActiveTiles = new List<TerrainTile>();
		}

		/// <summary>
		/// Finds the tile at the passed position in the cache and returns it. 
		/// Once found the tile is removed from the cache as cached tiles should 
		/// not be active in the scene.
		/// </summary>
		/// <param name="position">Position to search for</param>
		/// <returns>
		/// Returns the cached tile if it exists in the cache. If the tile
		/// is not cached, returns null.
		/// </returns>
		public TerrainTile GetCachedTileAtPosition(Vector2 position) {
			LinkedListNode<TerrainTile> node = CachedTiles.First;

			while (node != null) {
				LinkedListNode<TerrainTile> next = node.Next;

				if (node.Value.Position == position) { //Move Tile to front of cache
					CachedTiles.Remove(node);

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
		public bool TileActiveAtPosition(Vector2 position) {
			foreach (TerrainTile t in ActiveTiles) {
				if (t.Position == position)
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
		public List<Vector2> GetNewTilePositions(List<Vector2> positions) {
			List<Vector2> newPositions = new List<Vector2>(ActiveTiles.Count);

			foreach (Vector2 position in positions) {
				bool matched = false;

				foreach (TerrainTile t in ActiveTiles) {
					if (t.Position == position) {
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
		/// Adds the passed TerrainTile to the tile cache. This 
		/// deactivates the passed tile and will no longer be rendered 
		/// until pulled from the cache.
		/// </summary>
		/// <param name="tile">Tile to cache</param>
		public void CacheTile(TerrainTile tile) {
			tile.gameObject.SetActive(false);
			CachedTiles.AddFirst(tile);
			EnforceCacheSize();

			TerraEvent.TriggerOnTileDeactivated(tile);
		}

		/// <summary>
		/// Adds the passed tile to the active tiles list. If 
		/// the passed tile is not active, it is made active.
		/// </summary>
		/// <param name="tile">Tile to activate</param>
		public void AddActiveTile(TerrainTile tile) {
			tile.gameObject.SetActive(true);
			ActiveTiles.Add(tile);

			TerraEvent.TriggerOnTileActivated(tile);
		}

		/// <summary>
		/// Ensures that the cache size is maintained. Removes all extra 
		/// nodes from the back of the linked list.
		/// </summary>
		private void EnforceCacheSize() {
			int removalAmount = CachedTiles.Count - CacheCapacity;

			while (removalAmount > 0) {
				Object.Destroy(CachedTiles.Last.Value);
				CachedTiles.RemoveLast();
				removalAmount--;
			}
		}
	}
}