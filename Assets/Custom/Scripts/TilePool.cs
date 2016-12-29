using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Contains a pool of Tiles that are can be placed and removed in the world asynchronously 
/// </summary>
public class TilePool {
	private TerrainTiler Tiler;
	private GameObject TrackedObject;
	private int GenerationRadius = 3;
	private TileCache Cache = new TileCache(CACHE_SIZE);

	private const int CACHE_SIZE = 16;

	/// <summary>
	/// Creates a new TilePool instance with a GameObject that is used 
	/// to keep track of where to generate new tiles
	/// </summary>
	/// <param name="trackedObject">GameObject to generate around</param>
	/// <param name="generationRadius">Radius to generate outward</param>
	public TilePool(GameObject trackedObject, TerrainTiler tiler, int generationRadius = 4) {
		this.TrackedObject = trackedObject;
		this.GenerationRadius = generationRadius;
		this.Tiler = tiler;
	}

	/// <summary>
	/// Updates tiles that are surrounding the tracked GameObject
	/// </summary>
	public void UpdateTiles() {
		float meshSize = Tiler.TileSize * Tiler.Resolution;

		List<Vector2> nearbyPositions = GetTilePositionsFromRadius();
		List<Tile> toAdd = new List<Tile>();

		foreach (Vector2 pos in nearbyPositions) {
			Tile cached = Cache.GetCachedTileAtPosition(pos);

			if (cached != null) { //Pull from cache
				toAdd.Add(cached);
			} else { //Generate
				Tile t = new Tile(Tiler.gain, Tiler.Resolution, Tiler.TileSize, Tiler.TileSize);

				t.CreateTerrainTile(meshSize * pos.x, meshSize * pos.y);
				t.ApplyNoise();
				t.ApplyMaterialSettings(Tiler.MaterialSettings);
				t.Render();
				toAdd.Add(t);
			}
		}
		
		Cache.UpdateActiveTiles(toAdd);
	}

	/// <summary>
	/// Takes the passed chunk position and returns all other chunk positions in <code>generationRadius</code>
	/// </summary>
	/// <returns>Tile x & z positions to add to world</returns>
	private List<Vector2> GetTilePositionsFromRadius() {
		Vector3 trackedPos = TrackedObject.transform.position;
		List<Vector2> result = new List<Vector2>();

		for (var zCircle = -GenerationRadius; zCircle <= GenerationRadius; zCircle++) {
			for (var xCircle = -GenerationRadius; xCircle <= GenerationRadius; xCircle++) {
				if (xCircle * xCircle + zCircle * zCircle < GenerationRadius * GenerationRadius)
					result.Add(new Vector2(trackedPos.x + xCircle, trackedPos.z + zCircle));
			}
		}
		
		return result;
	}
}
