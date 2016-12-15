using UnityEngine;
using System.Collections;
using Caching;
using System.Collections.Generic;

/// <summary>
/// Contains a pool of Tiles that are can be placed and removed in the world asynchronously 
/// </summary>
public class TilePool {
	private GameObject trackedObject;
	private int generationRadius = 7;

	private const int CACHE_SIZE = 16;

	private List<Tile> activeTiles = new List<Tile>();
	private LRUCache<string, Tile> oldTiles = new LRUCache<string, Tile>(capacity: CACHE_SIZE);

	/// <summary>
	/// Creates a new TilePool instance with a GameObject that is used 
	/// to keep track of where to generate new tiles
	/// </summary>
	/// <param name="trackedObject">GameObject to generate around</param>
	/// <param name="generationRadius">Radius to generate outward</param>
	public TilePool(GameObject trackedObject, int generationRadius) {
		this.trackedObject = trackedObject;
	}

	/// <summary>
	/// Creates a new TilePool instance with a GameObject that is used
	/// to keep track of where to generate new tiles
	/// </summary>
	/// <param name="trackedObject"></param>
	public TilePool(GameObject trackedObject) {
		this.trackedObject = trackedObject;
	}

	/// <summary>
	/// Updates tiles that are surrounding the tracked GameObject
	/// </summary>
	public void UpdateTiles() {

	}

	/// <summary>
	/// Takes the passed chunk position and returns all other chunk positions in <code>generationRadius</code>
	/// </summary>
	/// <returns>Tile x & z positions to add to world</returns>
	private List<Vector2> GetTilePositionsFromRadius() {
		Vector3 trackedPos = trackedObject.transform.position;
		List<Vector2> result = new List<Vector2>();

		for (var zCircle = -generationRadius; zCircle <= generationRadius; zCircle++) {
			for (var xCircle = -generationRadius; xCircle <= generationRadius; xCircle++) {
				if (xCircle * xCircle + zCircle * zCircle < generationRadius * generationRadius)
					result.Add(new Vector2(trackedPos.x + xCircle, trackedPos.z + zCircle));
			}
		}

		return result;
	}
}
