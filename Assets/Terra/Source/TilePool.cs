using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Terra.CoherentNoise;
using System.Threading;

namespace Terra.Terrain {
	/// <summary>
	/// Contains a pool of Tiles that are can be placed and removed in the world asynchronously 
	/// using a thread pool.
	/// </summary>
	public class TilePool {
		public TerraSettings Settings;

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
		public List<TerrainTile> ActiveTiles {
			get {
				return Cache.ActiveTiles;
			}
		}

		private TileCache Cache = new TileCache(CACHE_SIZE);
		private int queuedTiles = 0;
		private Object pollLock = new Object();
		private const int CACHE_SIZE = 30;
		private const float ADD_TILE_DELAY = 0.5f;
		
		private struct ThreadData {
			public TerrainTile tile;
			public Generator gen;
		}

		public TilePool() {
			Settings = TerraSettings.Instance;

		}

		/// <summary>
		/// Updates tiles to update when the current queue of tiles 
		/// has finished generating.
		/// </summary>
		public void Update() {
			if (queuedTiles < 1) {
				Settings.StartCoroutine(UpdateTiles());
			}

			Settings.StartCoroutine(UpdateColliders(0.5f));
		}

		public static List<Vector2> GetTilePositionsFromRadius(int radius, Vector3 position, int length) {
			int xPos = Mathf.RoundToInt(position.x / length);
			int zPos = Mathf.RoundToInt(position.z / length);
			List<Vector2> result = new List<Vector2>(25);

			for (var zCircle = -radius; zCircle <= radius; zCircle++) {
				for (var xCircle = -radius; xCircle <= radius; xCircle++) {
					if (xCircle * xCircle + zCircle * zCircle < radius * radius)
						result.Add(new Vector2(xPos + xCircle, zPos + zCircle));
				}
			}

			return result;
		}

		/// <summary>
		/// Finds all <b>enabled</b> TerrainTile instances that intersect 
		/// the passed square parameters
		/// </summary>
		/// <param name="trackedPos"><code>TrackedObject</code> position</param>
		/// <param name="extent">Extent of collision square, most likely <code>ColliderGenerationExtent</code></param>
		/// <returns>Found, overlapping, TerrainTile instances</returns>
		public List<TerrainTile> GetTilesInExtent(Vector3 trackedPos, float extent) {
			//TODO Remove params
			List<TerrainTile> tiles = new List<TerrainTile>(Cache.ActiveTiles.Count);

			foreach (TerrainTile t in Cache.ActiveTiles) {
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
		/// asynchronously
		/// </summary>
		public IEnumerator UpdateTiles() {
			List<Vector2> nearbyPositions = GetTilePositionsFromRadius();
			List<Vector2> newPositions = Cache.GetNewTilePositions(nearbyPositions);

			//Remove old positions
			for (int i = 0; i < Cache.ActiveTiles.Count; i++) {
				bool found = false;

				foreach (Vector2 nearby in nearbyPositions) {
					if (Cache.ActiveTiles[i].Position == nearby) { //Position found, ignore
						found = true;
						break;
					}
				}

				if (!found) {
					Cache.CacheTile(Cache.ActiveTiles[i]);
					Cache.ActiveTiles.RemoveAt(i);
					i--;
				}
			}

			//Add new positions
			foreach (Vector2 pos in newPositions) {
				TerrainTile cached = Cache.GetCachedTileAtPosition(pos);

				//Attempt to pull from cache, generate if not available
				if (cached != null) {
					Cache.AddActiveTile(cached);
				} else {
					yield return AddTileAsync(pos);
				}
			}
		}

		/// <summary>
		/// Updates colliders that match the Settings specified collider 
		/// generation extent.
		/// </summary>
		/// <param name="delay">Delay in seconds before checking colliders again</param>
		/// <returns>IEnumerator for use in a Coroutine</returns>
		private IEnumerator UpdateColliders(float delay) {
			//If we're generating all colliders the extent of collision generation is 
			//technically infinity (max value works just as well though)
			float extent = Settings.GenAllColliders ? float.MaxValue : Settings.ColliderGenerationExtent;
			List<TerrainTile> tiles = GetTilesInExtent(Settings.TrackedObject.transform.position, extent);

			foreach (TerrainTile t in tiles) {
				t.GenerateCollider();
				yield return null;
			}

			yield return new WaitForSeconds(delay);
		}

		/// <summary>
		/// Takes the passed chunk position and returns all other chunk positions in <code>generationRadius</code>
		/// </summary>
		/// <returns>Tile x & z positions to add to world</returns>
		private List<Vector2> GetTilePositionsFromRadius() {
			return GetTilePositionsFromRadius(Settings.GenerationRadius, Settings.TrackedObject.transform.position, Settings.Length);
		}

		/// <summary>
		/// Adds a tile at the passed position asynchronously
		/// </summary>
		/// <param name="pos">Position to add tile at</param>
		public IEnumerator AddTileAsync(Vector2 pos) {
			TerrainTile tile = new GameObject("Tile: " + pos).AddComponent<TerrainTile>();
			queuedTiles++;

			if (Settings.UseMultithreading) {
				ThreadData data = new ThreadData();
				data.tile = tile;
				data.gen = Settings.Graph.GetEndGenerator();

				ThreadPool.QueueUserWorkItem(new WaitCallback((d) => {
					//GetValue is not thread safe and must be locked
					lock (pollLock) {
						if (d is ThreadData) {
							ThreadData tData = (ThreadData)d;
							TerrainTile.MeshData md = tData.tile.CreateRawMesh(pos, tData.gen);

							MTDispatch.Instance().Enqueue(() => { //Main Thread
								tData.tile.RenderRawMeshData(md);

								if (Settings.UseCustomMaterial)
									tile.ApplyCustomMaterial();
								else
									tile.Details.ApplySplatmap();

								tile.UpdatePosition(pos);
								Cache.AddActiveTile(tile);
								queuedTiles--;
							});
						}
					}
				}), data);
			} else {
				yield return new WaitForSecondsRealtime(ADD_TILE_DELAY);

				tile.CreateMesh(pos, false);
				yield return null;

				if (Settings.UseCustomMaterial)
					tile.ApplyCustomMaterial();
				else
					tile.Details.ApplySplatmap();
				tile.gameObject.GetComponent<MeshRenderer>().enabled = true;

				Cache.AddActiveTile(tile);

				queuedTiles--;
			}
		}
	}
}