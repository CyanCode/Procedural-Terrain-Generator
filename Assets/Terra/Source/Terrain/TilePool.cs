using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using Terra.Structures;
using Debug = UnityEngine.Debug;

namespace Terra.Terrain {
	/// <summary>
	/// Contains a pool of Tiles that are can be placed and removed in the world asynchronously 
	/// using a thread pool.
	/// </summary>
	[Serializable]
	public class TilePool {
		private const int CACHE_SIZE = 50;

		//Keeps track of tiles that were queued for generation.
		[SerializeField]
		private int _queuedTiles = 0;

		private bool _isFirstUpdate = true;

		[SerializeField]
		private float _remapMin = 0;

		[SerializeField]
		private float _remapMax = 1;

		private TerraConfig Config {
			get { return TerraConfig.Instance; }
		}

		public TileCache Cache;

		/// <summary>
		/// Min value after calling <see cref="CalculateHeightmapRemap"/>. Used 
		/// for remapping the heightmap upon calling <see cref="Tile.Generate"/>
		/// </summary>
		public float RemapMin {
			get { return _remapMin;  }
		}

		/// <summary>
		/// Max value after calling <see cref="CalculateHeightmapRemap"/>. Used 
		/// for remapping the heightmap upon calling <see cref="Tile.Generate"/>
		/// </summary>
		public float RemapMax {
			get { return _remapMax; }
		}

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

			t.Generate(() => {
				AddTile(t);
				onComplete(t);
			}, RemapMin, RemapMax);
		}

		/// <summary>
		/// Creates a Tile at the passed grid position, activates it, and 
		/// swaps it with the Tile that currently exists at the passed grid 
		/// position. If a tile doesn't exist at that location, this functions 
		/// the same as <see cref="AddTileAt"/>.
		/// </summary>
		/// <param name="p">position to create/swap tiles with</param>
		/// <param name="onComplete">Called when the generated Tile has been swapped</param>
		public void SwapTileAt(GridPosition p, Action<Tile> onComplete) {

		}

		/// <summary>
		/// Calculates the min and maximum values to use when applying a heightmap 
		/// remap. This sets <see cref="RemapMax"/> and <see cref="RemapMin"/>.
		/// </summary>
		public void CalculateHeightmapRemap() {
			Stopwatch sw = new Stopwatch();
			sw.Start();

			float min = float.PositiveInfinity;
			float max = float.NegativeInfinity;
			int res = Config.Generator.RemapResolution;

			for (int x = 0; x < res; x++) {
				for (int z = 0; z < res; z++) {
					float value = Config.Graph.GetEndGenerator().GetValue(x, z, 0f);

					if (value > max) {
						max = value;
					}
					if (value < min) {
						min = value;
					}
				}
			}

			//Set remap values for instance
			_remapMin = min;
			_remapMax = max;

			sw.Stop();
		    // ReSharper disable once ConditionIsAlwaysTrueOrFalse
#pragma warning disable 162
			if (TerraConfig.TerraDebug.SHOW_DEBUG_MESSAGES) {
				Debug.Log("CalculateHeightmapRemap took " + sw.ElapsedMilliseconds + "ms to complete. " +
				          "New min=" + min + " New max=" + max);
			}
#pragma warning restore 162
		}

		/// <summary>
		/// Remove all active Tiles from the scene. Skips caching.
		/// </summary>
		public void RemoveAll() {
			_queuedTiles = 0;

			for (int i = 0; i < ActiveTileCount; i++) {
				Cache.RemoveActiveTile(ActiveTiles[i]);
				i--;
			}
		}

		/// <summary>
		/// Halts the generation of tiles by resetting the queued tile count.
		/// </summary>
		public void ResetQueue() {
			_queuedTiles = 0;
			_isFirstUpdate = true;
		}

		/// <summary>
		/// Updates tiles to generate when the current queue of tiles 
		/// has finished generating.
		/// </summary>
		public void Update() {
			//Calculate remap
			if (Config.Generator.RemapHeightmap && _isFirstUpdate) {
				_isFirstUpdate = false;
				CalculateHeightmapRemap();
			}

			Cache.PurgeDestroyedTiles();

			if (_queuedTiles < 1) {
				Config.StartCoroutine(UpdateTiles());
			}
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
			List<Tile> toRegenerate = new List<Tile>();

			RemoveOldPositions(ref nearbyPositions, ref toRegenerate);

			//Add new positions
			_queuedTiles = newPositions.Count + toRegenerate.Count;
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
					if (TerraConfig.TerraDebug.SHOW_DEBUG_MESSAGES) {
						Debug.Log("Cached tile " + cached + " has heightmap res=" + cached.MeshManager.HeightmapResolution +
						". requested res=" + cached.GetLodLevel().Resolution + ". Regenerating.");
					}
					
					toRegenerate.Add(cached);
					Cache.AddActiveTile(cached);
					continue;
				} 

				//Generate one tile per frame
				if (Application.isPlaying)
					yield return null;

				AddTileAt(pos, tile => {
					_queuedTiles--;

					if (_queuedTiles == 0)
						UpdateNeighbors(newPositions.ToArray(), false);
				});
			}

			//Regenerate tiles with outdated positions
			for (int i = 0; i < toRegenerate.Count; i++) {
				Tile t = toRegenerate[i];

				// ReSharper disable once ConditionIsAlwaysTrueOrFalse
				if (TerraConfig.TerraDebug.SHOW_DEBUG_MESSAGES) {
					Debug.Log("Active tile " + t + " has heightmap res=" + t.MeshManager.HeightmapResolution +
							  ". requested res=" + t.GetLodLevel().Resolution + ". Regenerating.");
				}

				//Generate one tile per frame
				if (Application.isPlaying)
					yield return null;

				AddTileAt(t.GridPosition, tile => {
					tile.enabled = false;
					_queuedTiles--;

					UpdateNeighbors(new[] { tile.GridPosition }, true, tile1 => {
						//Remove low res tile
						Cache.RemoveActiveTile(t);
						Cache.RemoveCachedTile(t);
						tile.enabled = true;
					});

					if (_queuedTiles == 0)
						UpdateNeighbors(newPositions.ToArray(), false);
				});
			}

			//If tiles were updated synchronously, notify queue completion
			if (newPositions.Count > 0 && _queuedTiles == 0) {
				UpdateNeighbors(newPositions.ToArray(), false);
			}
		}

		/// <summary>
		/// Updates the neighboring Tiles of the passed list of 
		/// Tiles.
		/// </summary>
		/// <param name="positions">A list of tile positions to updatee</param>
		/// <param name="hideTerrain">Optionally hide the terrain when updating neighbors</param>
		/// <param name="onComplete">Called when a Tile's neighbors have been updated since 
		/// <see cref="TileMesh.SetTerrainHeightmap"/> can use a coroutine</param>
		public void UpdateNeighbors(GridPosition[] positions, bool hideTerrain, Action<Tile> onComplete = null) {
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

				//Copy reference to avoid loss of reference in closure
				var tile1 = tile;

				tile.MeshManager.SetNeighboringTiles(new Neighborhood(neighborTiles), true, hideTerrain, () => {
					if (onComplete != null) {
						onComplete(tile1);
					}
				});
			}
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