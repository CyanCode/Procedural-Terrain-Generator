using UnityEngine;
using System.Collections.Generic;

public class TileCache {
	private int CacheCapacity;
	private List<Tile> ActiveTiles = new List<Tile>();
	private LinkedList<Tile> CachedTiles = new LinkedList<Tile>();

	public TileCache(int cacheCapacity = 20) {
		CacheCapacity = cacheCapacity;
	}

	/// <summary>
	/// Finds the tile at the passed position in the cache and returns it. 
	/// Once found the tile is moved to the front of the LRU Cache
	/// </summary>
	/// <param name="position">Position to search for</param>
	/// <returns>
	/// Returns the cached tile if it exists in the cache. If the tile
	/// is not cached, returns null.
	/// </returns>
	public Tile GetCachedTileAtPosition(Vector2 position) {
		LinkedListNode<Tile> node = CachedTiles.First;

		while (node != null) {
			LinkedListNode<Tile> next = node.Next;

			if (node.Value.tilePosition == position) { //Move Tile to front of cache
				CachedTiles.Remove(next);
				CachedTiles.AddFirst(next);

				return node.Value;
			}

			node = next;
		}

		return null;
	}
	
	/// <summary>
	/// Updates the active tiles internally. Old tiles which were not added 
	/// to the update are sent to the cache and the cache size is enforced.
	/// </summary>
	/// <param name="tiles">Tiles to make active</param>
	public void UpdateActiveTiles(IEnumerable<Tile> tiles) {
		foreach (Tile t in tiles) { //Cache old tiles
			if (!ActiveTiles.Contains(t)) {
				t.isActive = false;
				CachedTiles.AddFirst(t);
			} else {
				t.isActive = true;
			}
		}

		EnforceCacheSize();
		ActiveTiles = new List<Tile>(tiles);
	}

	/// <summary>
	/// Ensures that the cache size is maintained. Removes all extra 
	/// nodes from the back of the linked list.
	/// </summary>
	private void EnforceCacheSize() {
		int removalAmount = CachedTiles.Count - CacheCapacity;

		while (removalAmount > 0) {
			CachedTiles.RemoveLast();
			removalAmount--;
		}
	}
}
