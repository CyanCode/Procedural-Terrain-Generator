using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using Terra.Structure;
using Debug = UnityEngine.Debug;

namespace Terra.Terrain {
	/// <summary>
	/// Contains a pool of Tiles that are can be placed and removed in the world asynchronously 
	/// using a thread pool.
	/// </summary>
	[Serializable]
	public class TilePool {
		private const int CACHE_SIZE = 50;
		
		[SerializeField]
		private bool _isGeneratingTile = false;

		//Keeps track of tiles that were queued for generation.
		[SerializeField]
		private int _queuedTiles = 0;
		private Action<GridPosition[]> _queueCompletedAction;
		private Action _queueFirstCompletionAction;

		/// <summary>
		/// Used for computing heightmap transformation.
		/// </summary>
		[SerializeField]
		private bool _isFirstQueue = true;

		[SerializeField]
		private float remapMin = 0;

		[SerializeField]
		private float remapMax = 1;

		private TerraConfig Config {
			get { return TerraConfig.Instance; }
		}

		public TileCache Cache;

		/// <summary>
		/// Returns the amount of tiles that are currently set as 
		/// active.
		/// </summary>
		public int ActiveTileCount {
			get {
				if (Cache == null) {
					return 0;
				}

				return Cache.ActiveTiles.Count;
			}
		}

		/// <summary>
		/// Returns a list of tiles that are currently active in the 
		/// scene
		/// </summary>
		public List<Tile> ActiveTiles {
			get {
				return Cache.ActiveTiles;
			}
		}

		public TilePool() {
			if (Cache == null)
				Cache = new TileCache(CACHE_SIZE);
		}

		/// <summary>
		/// Calculate a circular list of <see cref="GridPosition"/>s around 
		/// the passed <see cref="center"/> with a passed radius. The returned 
		/// list is sorted by distance from <see cref="center"/>
		/// </summary>
		/// <param name="radius">Radius of circle (in grid units)</param>
		/// <param name="center">Center of circle in world space</param>
		/// <param name="length">Length of grid squares</param>
		/// <returns></returns>
		public static List<GridPosition> GetTilePositionsFromRadius(int radius, Vector2 center, int length) {
			int xPos = Mathf.RoundToInt(center.x / length);
			int zPos = Mathf.RoundToInt(center.y / length);
			SortedList<float, GridPosition> result = new SortedList<float, GridPosition>(25);

			for (var zCircle = -radius; zCircle <= radius; zCircle++) {
				for (var xCircle = -radius; xCircle <= radius; xCircle++) {
					if (xCircle * xCircle + zCircle * zCircle < radius * radius) {
						var newPos = new GridPosition(xPos + xCircle, zPos + zCircle);
						var distance = newPos.Distance(new GridPosition(xPos, zPos));

						while (result.ContainsKey(distance))
							distance++;
						result.Add(distance, newPos);
					}
				}
			}

			var normResult = new List<GridPosition>(result.Count);
			normResult.AddRange(result.Values);

			return normResult;
		}

		/// <summary>
		/// Updates tiles to update when the current queue of tiles 
		/// has finished generating.
		/// </summary>
		public void Update() {
			if (_queueCompletedAction == null) {
				_queueCompletedAction = UpdateNeighbors;
			}
			if (_queueFirstCompletionAction == null) {
				_queueFirstCompletionAction = DidCompleteFirstQueue;
			}

			Cache.PurgeDestroyedTiles();

			if (_queuedTiles < 1) {
				Config.StartCoroutine(UpdateTiles());
			}

			//Config.StartCoroutine(UpdateColliders(0.5f)); //todo remove
		}

		/// <summary>
		/// Halts the generation of tiles by resetting the queued tile count.
		/// </summary>
		public void ResetQueue() {
			_queuedTiles = 0;
			_isFirstQueue = true;
		}

		/// <summary>
		/// Manually add (and activate) the passed Tile to the TilePool. This does not modify 
		/// <see cref="t"/>. To automatically create a Tile according to 
		/// <see cref="TerraConfig"/> call <see cref="AddTileAt"/> instead.
		/// </summary>
		/// <param name="t">Tile to add</param>
		public void AddTile(Tile t) {
			if (t != null) {
				Cache.AddActiveTile(t);
			} else {
				Debug.LogWarning("Trying to add Tile that is null.");
			}
		}

		/// <summary>
		/// Creates a Tile at the passed grid position, activates it, //todo update docs
		/// and places it in the scene.
		/// This method calls <see cref="Tile.Generate"/>.
		/// </summary>
		/// <param name="p">position in grid to add tile at.</param>
		/// <param name="onComplete">called when the Tile has finished generating.</param>
		public void AddTileAt(GridPosition p, Action<Tile> onComplete) {
			Tile t = Tile.CreateTileGameobject("Tile [" + p.X + ", " + p.Z + "]");
			t.UpdatePosition(p);

			if (Config.Generator.UseMultithreading) {
				t.MeshManager.CalculateHeightmapAsync(() => {
					PostTileCalculateHeightmap(t);
					onComplete(t);
				});
			} else {
				t.MeshManager.CalculateHeightmap();
				PostTileCalculateHeightmap(t);
				onComplete(t);
			}
		}

		/// <summary>
		/// Remove all active Tiles from the scene. Skips caching.
		/// </summary>
		public void RemoveAll() {
			_queuedTiles = 0;
			_isGeneratingTile = false;

			for (int i = 0; i < ActiveTileCount; i++) {
				Cache.RemoveActiveTile(ActiveTiles[i]);
				i--;
			}
		}

		/// <summary>
		/// Finds all <b>enabled</b> Tile instances that intersect 
		/// the passed square parameters
		/// </summary>
		/// <param name="trackedPos"><code>TrackedObject</code> position</param>
		/// <param name="extent">Extent of collision square, most likely <code>ColliderGenerationExtent</code></param>
		/// <returns>Found, overlapping, Tile instances</returns>
		public List<Tile> GetTilesInExtent(Vector3 trackedPos, float extent) {
			//TODO Remove params
			List<Tile> tiles = new List<Tile>(Cache.ActiveTiles.Count);

			foreach (Tile t in Cache.ActiveTiles) {
				MeshRenderer renderer = t.GetComponent<MeshRenderer>();

				if (renderer != null) {
					Vector3 tilePos = new Vector3(trackedPos.x, renderer.bounds.center.y, trackedPos.z);
					Bounds trackedBounds = new Bounds(tilePos, new Vector3(extent, renderer.bounds.max.y, extent));

					if (renderer.bounds.Intersects(trackedBounds))
						tiles.Add(t);
				}
			}

			return tiles;
		}

		/// <summary>
		/// Updates tiles that are surrounding the tracked GameObject 
		/// asynchronously. When calling this method using 
		/// <see cref="MonoBehaviour.StartCoroutine(System.Collections.IEnumerator)"/>, 
		/// tiles are generated once per frame
		/// </summary>
		public IEnumerator UpdateTiles() {
			List<GridPosition> nearbyPositions = GetTilePositionsFromRadius();
			List<GridPosition> newPositions = Cache.GetNewTilePositions(nearbyPositions);
			List<Tile> needRegenerating = new List<Tile>();

			RemoveOldPositions(ref nearbyPositions, ref needRegenerating);

			//Add new positions
			_queuedTiles = newPositions.Count + needRegenerating.Count;
			foreach (GridPosition pos in newPositions) {
				Tile cached = Cache.GetCachedTileAtPosition(pos);

				//Attempt to pull from cache, generate if not available
				if (cached != null) {
					if (cached.IsHeightmapLodValid()) { //Cached tile is valid, use it
						AddTile(cached);
						_queuedTiles--;

						continue;
					} 
					
					//Cached tile has too low lod, mark for regeneration
					Debug.Log("Cached tile " + cached + " has heightmap res=" + cached.MeshManager.HeightmapResolution + 
						". requested res=" + cached.LodLevel.MapResolution + ". Regenerating.");
					//Cache.RemoveCachedTile(cached);
					needRegenerating.Add(cached);
					Cache.AddActiveTile(cached);
					continue;
				} 

				//Generate one tile per frame
				if (Application.isPlaying)
					yield return null;

				_isGeneratingTile = true;
				AddTileAt(pos, tile => {
					_queuedTiles--;
					_isGeneratingTile = false;

					if (_queuedTiles == 0)
						_queueCompletedAction(newPositions.ToArray());
				});
			}

			//Regenerate tiles with outdated positions
			for (int i = 0; i < needRegenerating.Count; i++) {
				Tile t = needRegenerating[i];
				Debug.Log("Active tile " + t + " has heightmap res=" + t.MeshManager.HeightmapResolution +
				          ". requested res=" + t.LodLevel.MapResolution + ". Regenerating.");

				//Generate one tile per frame
				if (Application.isPlaying)
					yield return null;

				_isGeneratingTile = true;
				AddTileAt(t.GridPosition, tile => {
					_queuedTiles--;
					_isGeneratingTile = false;

					//Remove low res tile
					Cache.RemoveActiveTile(t);
					Cache.RemoveCachedTile(t);

					if (_queuedTiles == 0)
						_queueCompletedAction(newPositions.ToArray());
				});
			}

			//If tiles were updated synchronously, notify queue completion
			if (newPositions.Count > 0 && _queuedTiles == 0) {
				_queueCompletedAction(newPositions.ToArray());

				//If first completion of queue, notify
				if (_isFirstQueue) {
					_isFirstQueue = false;
					_queueFirstCompletionAction();
				}
			}
		}

		/// <summary>
		/// Updates the neighboring Tiles of the passed list of 
		/// Tiles.
		/// </summary>
		public void UpdateNeighbors(GridPosition[] positions) {
			if (positions == null)
				return;

			List<Tile> tiles = new List<Tile>(positions.Length);
			foreach (var pos in positions)
				foreach (var tile in Cache.ActiveTiles)
					if (tile.GridPosition == pos)
						tiles.Add(tile);

			//Get neighboring tiles
			foreach (Tile tile in tiles) {
				GridPosition[] neighborPos = tile.GridPosition.Neighbors;
				Tile[] neighborTiles = new Tile[4];

				for (int i = 0; i < 4; i++) {
					Tile found = Cache.ActiveTiles.Find(t => t.GridPosition == neighborPos[i]);
					neighborTiles[i] = found;
				}

				tile.MeshManager.SetNeighboringTiles(new Neighborhood(neighborTiles));
			}
		}

		/// <summary>
		/// Continuation of <see cref="AddTileAt"/>'s async and synchronous logic
		/// </summary>
		private void PostTileCalculateHeightmap(Tile t) {
			if (!_isFirstQueue && remapMin != 0f && remapMax != 1f) {
				float offset = Config.Generator.LinearTransformOffset;
				t.MeshManager.RemapHeightmap(remapMin, remapMax, offset, 1 - offset);
			}
			if (!_isFirstQueue) {
				t.MeshManager.SetTerrainHeightmap();
			}

			Cache.AddActiveTile(t);
		}

		/// <summary>
		/// Removes old unneeded positions from the passed nearbyPositions list.
		/// </summary>
		/// <param name="nearbyPositions"></param>
		/// <param name="needRegenerating"></param>
		private void RemoveOldPositions(ref List<GridPosition> nearbyPositions, ref List<Tile> needRegenerating) {
			for (int i = 0; i < Cache.ActiveTiles.Count; i++) {
				bool found = false;

				foreach (GridPosition nearby in nearbyPositions) {
					Tile t = Cache.ActiveTiles[i];

					if (t.GridPosition == nearby) { //Position found in ActiveTiles
						if (t.IsHeightmapLodValid()) { //Correct heightmap, ignore
							found = true;
							break;
						}

						//Invalid heightmap, mark for regeneration
						found = true;
						needRegenerating.Add(t);
					}
				}

				if (!found) {
					Cache.CacheTile(Cache.ActiveTiles[i]);
					Cache.ActiveTiles.RemoveAt(i);
					i--;
				}
			}
		}

		private void DidCompleteFirstQueue() {
			//Remap all tiles to loweset min and biggest max from tiles
			float min = float.PositiveInfinity;
			float max = float.NegativeInfinity;

			foreach (Tile t in ActiveTiles) {
				float tMin = t.MeshManager.HeightmapMinHeight;
				float tMax = t.MeshManager.HeightmapMaxHeight;

				if (tMin < min) {
					min = tMin;
				}
				if (tMax > max) {
					max = tMax;
				}
			}

			//Set remap values for instance
			remapMin = min;
			remapMax = max;

			//Remap and set heightmap for terrain
			List<GridPosition> gps = new List<GridPosition>(ActiveTileCount);
			float offset = Config.Generator.LinearTransformOffset;
			foreach (Tile t in ActiveTiles) {
				t.MeshManager.RemapHeightmap(min, max, offset, 1 - offset);
				t.MeshManager.SetTerrainHeightmap(); //todo make coroutine?
			}

			UpdateNeighbors(gps.ToArray());
		}

		///// <summary>
		///// Checks each heightmap in <see cref="ActiveTiles"/> and finds the maximum height 
		///// out of all. 
		///// <remarks>If <see cref="GenerationData.PrecalculateMaxHeight"/> is false or the first 
		///// queue has completed, this method does nothing.</remarks>
		///// </summary>
		//private void CalculateHeightmapTransform(GridPosition[] positions) {
		//	if (!Config.Generator.PrecalculateMaxHeight || !_isFirstQueue) {
		//		return;
		//	}

		//	_isFirstQueue = false;
		//	Stopwatch sw = new Stopwatch();
		//	sw.Start();
		//	//Determine max
		//	float max = float.NegativeInfinity;
		//	foreach (Tile t in ActiveTiles) {
		//		if (t.MeshManager == null || t.MeshManager.Heightmap == null) {
		//			continue;
		//		}

		//		float[,] hm = t.MeshManager.Heightmap;
		//		int res = t.MeshManager.HeightmapResolution;

		//		for (int x = 0; x < res; x++) {
		//			for (int z = 0; z < res; z++) {
		//				float val = hm[x, z];

		//				if (val > max) {
		//					max = val;
		//				}
		//			}
		//		}
		//	}

		//	//Calculate linear transformation t value
		//	//transf = v * t + k
		//	float halfOffset = Config.Generator.LinearTransformOffset / 2;
		//	Config.Generator.LinearTransformTValue = 1 / (max + halfOffset);

		//	//Apply transformation to ActiveTiles
		//	foreach (Tile t in ActiveTiles) {
		//		if (t.MeshManager == null || t.MeshManager.Heightmap == null) {
		//			continue;
		//		}

		//		float[,] hm = t.MeshManager.Heightmap;
		//		int res = t.MeshManager.HeightmapResolution;

		//		for (int x = 0; x < res; x++) {
		//			for (int z = 0; z < res; z++) {
		//				float val = hm[x, z];
		//				hm[x, z] = val * Config.Generator.LinearTransformTValue;
		//			}
		//		}
		//	}

		//	sw.Stop();
		//	Debug.Log("Took " + sw.Elapsed.Seconds + " seconds");
		//}

		/// <summary>
		/// Updates colliders that match the Settings specified collider 
		/// generation extent.
		/// </summary>
		/// <param name="delay">Delay in seconds before checking colliders again</param>
		/// <returns>IEnumerator for use in a Coroutine</returns>
		//private IEnumerator UpdateColliders(float delay) { //todo remove
		//	//If we're generating all colliders the extent of collision generation is 
		//	//technically infinity (max value works just as well though)
		//	float extent = Config.Generator.GenAllColliders ? float.MaxValue : Config.Generator.ColliderGenerationExtent;
		//	List<Tile> tiles = GetTilesInExtent(Config.Generator.TrackedObject.transform.position, extent);

		//	foreach (Tile t in tiles) {
		//		t.MeshManager.CalculateCollider();
		//		yield return null;
		//	}

		//	yield return new WaitForSeconds(delay);
		//}

		/// <summary>
		/// Takes the passed chunk position and returns all other chunk positions in <see cref="TerraConfig.Generator.GenerationRadius"/>
		/// </summary>
		/// <returns>Tile x & z positions to add to world</returns>
		private List<GridPosition> GetTilePositionsFromRadius() {
			Vector3 world3D = Config.Generator.TrackedObject.transform.position;
			Vector2 world = new Vector2(world3D.x, world3D.z);
			return GetTilePositionsFromRadius(Config.Generator.GenerationRadius, world, Config.Generator.Length);
		}
	}
}