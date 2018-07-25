using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Terra.CoherentNoise;
using Terra.Data;
using Object = UnityEngine.Object;

namespace Terra.Terrain {
	/// <summary>
	/// Represents a position in the grid of <see cref="Tile"/>s
	/// </summary>
	public struct Position {
		public int X;
		public int Z;

		public Position(int x, int z) {
			X = x;
			Z = z;
		}

		public static bool operator ==(Position p1, Position p2) {
			return p1.X == p2.X && p1.Z == p2.Z;
		}

		public static bool operator !=(Position p1, Position p2) {
			return !(p1 == p2);
		}
	}

	/// <summary>
	/// Contains a pool of Tiles that are can be placed and removed in the world asynchronously 
	/// using a thread pool.
	/// </summary>
	public class TilePool {
		private const int CACHE_SIZE = 30;
		private TileCache _cache = new TileCache(CACHE_SIZE);
		
		private int _queuedTiles = 0;
		private bool _isGeneratingTile = false;

		private TerraSettings _settings;

		/// <summary>
		/// Returns the amount of tiles that are currently set as 
		/// active.
		/// </summary>
		public int ActiveTileCount {
			get {
				if (_cache == null) {
					return 0;
				}

				return _cache.ActiveTiles.Count;
			}
		}

		/// <summary>
		/// Returns a list of tiles that are currently active in the 
		/// scene
		/// </summary>
		public List<Tile> ActiveTiles {
			get {
				return _cache.ActiveTiles;
			}
		}

		public static List<Position> GetTilePositionsFromRadius(int radius, Vector3 position, int length) {
			int xPos = Mathf.RoundToInt(position.x / length);
			int zPos = Mathf.RoundToInt(position.z / length);
			List<Position> result = new List<Position>(25);

			for (var zCircle = -radius; zCircle <= radius; zCircle++) {
				for (var xCircle = -radius; xCircle <= radius; xCircle++) {
					if (xCircle * xCircle + zCircle * zCircle < radius * radius)
						result.Add(new Position(xPos + xCircle, zPos + zCircle));
				}
			}

			return result;
		}

		public TilePool() {
			_settings = TerraSettings.Instance;
		}

		/// <summary>
		/// Updates tiles to update when the current queue of tiles 
		/// has finished generating.
		/// </summary>
		public void Update() {
			if (_queuedTiles < 1) {
				_settings.StartCoroutine(UpdateTiles());
			}

			_settings.StartCoroutine(UpdateColliders(0.5f));
		}

		/// <summary>
		/// Manually add (and activate) the passed Tile to the TilePool. This does not modify 
		/// <see cref="t"/>. To automatically create a Tile according to 
		/// <see cref="TerraSettings"/> call <see cref="AddTileAt"/> instead.
		/// </summary>
		/// <param name="t">Tile to add</param>
		public void AddTile(Tile t) {
			_cache.AddActiveTile(t);
		}

		/// <summary>
		/// Creates a Tile at the passed grid position, activates it, 
		/// and places it in the scene.
		/// This method calls <see cref="Tile.Generate"/>.
		/// </summary>
		/// <param name="p">position in grid to add tile at.</param>
		/// <param name="onComplete">called when the Tile has finished generating.</param>
		public void AddTileAt(Position p, Action<Tile> onComplete) {
			Tile t = Tile.CreateTileGameobject("Tile [" + p.X + ", " + p.Z + "]");
			t.UpdatePosition(p);

			t.Generate(() => onComplete(t), _settings.Generator.UseMultithreading);
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
			List<Tile> tiles = new List<Tile>(_cache.ActiveTiles.Count);

			foreach (Tile t in _cache.ActiveTiles) {
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
			List<Position> nearbyPositions = GetTilePositionsFromRadius();
			List<Position> newPositions = _cache.GetNewTilePositions(nearbyPositions);

			//Remove old positions
			for (int i = 0; i < _cache.ActiveTiles.Count; i++) {
				bool found = false;

				foreach (Position nearby in nearbyPositions) {
					if (_cache.ActiveTiles[i].Position == nearby) { //Position found, ignore
						found = true;
						break;
					}
				}

				if (!found) {
					_cache.CacheTile(_cache.ActiveTiles[i]);
					_cache.ActiveTiles.RemoveAt(i);
					i--;
				}
			}

			//Add new positions
			foreach (Position pos in newPositions) {
				Tile cached = _cache.GetCachedTileAtPosition(pos);

				//Attempt to pull from cache, generate if not available
				if (cached != null) {
					AddTile(cached);
				} else {
					//Wait for tile to finish generating before starting a new one
					while (_isGeneratingTile)
						yield return null;

					_isGeneratingTile = true;
					AddTileAt(pos, tile => { 
						AddTile(tile);
						_isGeneratingTile = false;
					});
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
			float extent = _settings.Generator.GenAllColliders ? float.MaxValue : _settings.Generator.ColliderGenerationExtent;
			List<Tile> tiles = GetTilesInExtent(_settings.Generator.TrackedObject.transform.position, extent);

			foreach (Tile t in tiles) {
				t.GenerateCollider();
				yield return null;
			}

			yield return new WaitForSeconds(delay);
		}

		/// <summary>
		/// Takes the passed chunk position and returns all other chunk positions in <code>generationRadius</code>
		/// </summary>
		/// <returns>Tile x & z positions to add to world</returns>
		private List<Position> GetTilePositionsFromRadius() {
			return GetTilePositionsFromRadius(_settings.Generator.GenerationRadius, _settings.Generator.TrackedObject.transform.position, _settings.Generator.Length);
		}
	}
}